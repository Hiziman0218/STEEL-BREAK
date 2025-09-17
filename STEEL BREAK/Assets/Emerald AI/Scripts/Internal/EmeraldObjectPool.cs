using UnityEngine;                         // Unity のコアAPIを使用
using System.Collections.Generic;          // Dictionary, Stack などのコレクションを使用

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【EmeraldObjectPool】
    /// オブジェクトプールを提供する静的ユーティリティクラス。
    /// ・プレハブごとにプール（Pool）を保持し、高頻度生成/破棄コストを削減
    /// ・Spawn/Despawn により再利用を行う
    /// ・効果（エフェクト）には自動デスポーン秒数を割り当て可能
    /// </summary>
    public static class EmeraldObjectPool
    {
        // Stack の内部配列リサイズを避けるため、想定プールサイズ以上の値に設定するとよい。
        // なお、Preload() を使えばプールの初期サイズを明示的に確保できる（弾丸など例外的に大きいプールで有効）。
        [Header("デフォルトのプール初期サイズ（Stack確保数の初期値）")]
        const int DEFAULT_POOL_SIZE = 3;

        /// <summary>
        /// 【Pool】特定プレハブ用のオブジェクトプールを表す内部クラス。
        /// </summary>
        class Pool
        {
            [Header("このプールの親オブジェクト（階層整理用）。必要時に '<Prefab名> (Pool)' を生成")]
            private GameObject PoolParent;

            // 生成したオブジェクト名に付与する連番ID（見た目上の識別用途）
            [Header("生成インスタンス名に付与する連番ID（外観上の識別。機能には影響しない）")]
            int nextId = 1;

            // 非アクティブのオブジェクトを保持する構造。
            // List ではなく Stack を使うのは、配列の先頭/中間から取り出す必要がなく、
            // 末尾（トップ）から取得するだけで良いから（メモリの並べ替え不要）。
            [Header("非アクティブ状態のオブジェクトを保持するスタック（再利用待機列）")]
            Stack<GameObject> inactive;

            [Header("このプールで扱うプレハブ参照")]
            GameObject prefab;

            /// <summary>
            /// コンストラクタ：対象プレハブと初期確保数を受け取り、内部スタックを準備する。
            /// </summary>
            public Pool(GameObject prefab, int initialQty)
            {
                this.prefab = prefab;
                //PoolParent = new GameObject(prefab.name + " (Pool)");

                // Stack が内部的に連結リストなら初期サイズは効果が薄いが、害はないため残す。
                inactive = new Stack<GameObject>(initialQty);
            }

            /// <summary>
            /// プールからオブジェクトを取り出して生成（在庫がなければ新規 Instantiate）。
            /// </summary>
            public GameObject Spawn(Vector3 pos, Quaternion rot)
            {
                GameObject obj;
                if (inactive.Count == 0)
                {
                    // プールに在庫がないため、新規にインスタンス化する
                    obj = (GameObject)GameObject.Instantiate(prefab, pos, rot);
                    obj.name = prefab.name + " (" + (nextId++) + ")";

                    // どのプールに属するか識別するため、PoolMember を付与
                    obj.AddComponent<PoolMember>().myPool = this;
                }
                else
                {
                    // 非アクティブ配列の末尾（トップ）から取得
                    obj = inactive.Pop();

                    if (obj == null)
                    {
                        // 期待した非アクティブオブジェクトが存在しない場合の主な原因：
                        //  - 誰かが Destroy() を呼んだ
                        //  - シーン遷移によりオブジェクトが破棄された
                        //    ※どうしても維持したい場合は DontDestroyOnLoad で回避可能
                        // 次の候補を再帰的に試す
                        return Spawn(pos, rot);
                    }
                }

                obj.transform.position = pos;
                obj.transform.rotation = rot;

                if (PoolParent == null) PoolParent = new GameObject(prefab.name + " (Pool)");
                PoolParent.transform.parent = EmeraldSystem.ObjectPool.transform;
                obj.transform.SetParent(PoolParent.transform);

                obj.SetActive(true);
                return obj;

            }

            /// <summary>
            /// エフェクト等をスポーンし、指定秒数後に自動 Despawn する設定を行うスポーン。
            /// </summary>
            public GameObject SpawnEffect(Vector3 pos, Quaternion rot, float SecondsToDespawn)
            {
                GameObject obj;

                if (inactive.Count == 0)
                {
                    // プールに在庫がないため、新規にインスタンス化する
                    obj = (GameObject)GameObject.Instantiate(prefab, pos, rot);
                    obj.name = prefab.name + " (" + (nextId++) + ")";

                    // どのプールに属するか識別するため、PoolMember を付与
                    obj.AddComponent<PoolMember>().myPool = this;
                }
                else
                {
                    // 非アクティブ配列の末尾（トップ）から取得
                    obj = inactive.Pop();

                    if (obj == null)
                    {
                        // 期待した非アクティブオブジェクトが存在しない場合の主な原因：
                        //  - 誰かが Destroy() を呼んだ
                        //  - シーン遷移によりオブジェクトが破棄された
                        //    ※どうしても維持したい場合は DontDestroyOnLoad で回避可能
                        // 次の候補を再帰的に試す
                        return SpawnEffect(pos, rot, SecondsToDespawn);
                    }
                }

                AssignTimedDespawn(obj, SecondsToDespawn);

                obj.transform.position = pos;
                obj.transform.rotation = rot;

                if (PoolParent == null) PoolParent = new GameObject(prefab.name + " (Pool)");
                PoolParent.transform.parent = EmeraldSystem.ObjectPool.transform;
                obj.transform.SetParent(PoolParent.transform);

                obj.SetActive(true);
                return obj;

            }

            /// <summary>
            /// 指定秒数経過後に自動 Despawn するための設定を付与する。
            /// </summary>
            void AssignTimedDespawn(GameObject obj, float SecondsToDespawn)
            {
                EmeraldTimedDespawn TimedDespawn = obj.GetComponent<EmeraldTimedDespawn>();
                if (TimedDespawn == null) TimedDespawn = obj.AddComponent<EmeraldTimedDespawn>();
                TimedDespawn.SecondsToDespawn = SecondsToDespawn;
            }

            /// <summary>
            /// オブジェクトを非アクティブ化してプールへ返却する。
            /// </summary>
            public void Despawn(GameObject obj)
            {
                obj.SetActive(false);

                // Stack は Capacity を持たないため、内部配列の拡張係数を制御できない。
                // とはいえ、内部が連結リストであってもコンストラクタでサイズ指定があるなど挙動は実装依存。
                inactive.Push(obj);
            }

        }


        /// <summary>
        /// 【PoolMember】生成直後のオブジェクトへ付与し、Despawn 時に正しいプールへ返却するためのリンクを保持する。
        /// </summary>
        class PoolMember : MonoBehaviour
        {
            [Header("このオブジェクトが属するプール（返却先）")]
            public Pool myPool;
        }

        [Header("プレハブごとのプールを保持する辞書（キー: プレハブ, 値: Pool）")]
        // すべてのプール
        static Dictionary<GameObject, Pool> pools;

        /// <summary>
        /// 辞書の初期化。必要に応じて対象プレハブのプールを生成する。
        /// </summary>
        static void Init(GameObject prefab = null, int qty = DEFAULT_POOL_SIZE)
        {
            if (pools == null)
            {
                pools = new Dictionary<GameObject, Pool>();
            }
            if (prefab != null && pools.ContainsKey(prefab) == false)
            {
                pools[prefab] = new Pool(prefab, qty);
            }
        }

        /// <summary>
        /// 事前にプレハブのインスタンスを確保（プリロード）する。
        /// 0→100個など短時間で大量生成する見込みがある場合に有効。
        /// 実装は簡潔さを優先。Spawn/Despawn の流れを使いコード重複を避ける。
        /// </summary>
        static public void Preload(GameObject prefab, int qty = 1)
        {
            Init(prefab, qty);

            // これから事前生成するオブジェクトを受け取る配列
            GameObject[] obs = new GameObject[qty];
            for (int i = 0; i < qty; i++)
            {
                obs[i] = Spawn(prefab, Vector3.zero, Quaternion.identity);
            }

            // 生成した分をすべていったん戻す
            for (int i = 0; i < qty; i++)
            {
                Despawn(obs[i]);
            }
        }

        /// <summary>
        /// プールのクリア。初期化時に一度だけ、プールがすでに埋まっている場合に呼び出される想定。
        /// （典型例：シーン変更後）
        /// </summary>
        static public void Clear()
        {
            if (pools != null && pools.Count != 0) pools.Clear();
        }

        /// <summary>
        /// 指定プレハブのインスタンスをスポーン（必要なら Instantiate）。
        /// 注意：Awake()/Start() は**最初の生成時のみ**実行される。
        /// メンバー変数は自動で初期化されない点に注意。OnEnable は生成後にも呼ばれる
        /// （IsActive の切替でも呼ばれる）ことを覚えておく。
        /// </summary>
        static public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            Init(prefab);

            return pools[prefab].Spawn(pos, rot);
        }

        /// <summary>
        /// 指定プレハブのインスタンスをスポーン（必要なら Instantiate）。
        /// こちらは指定秒数後に自動 Despawn する効果用のスポーン。
        /// 注意事項は Spawn と同様（Awake/Start/OnEnable の呼び出しタイミングなど）。
        /// </summary>
        static public GameObject SpawnEffect(GameObject prefab, Vector3 pos, Quaternion rot, float SecondsToDespawn)
        {
            Init(prefab);

            return pools[prefab].SpawnEffect(pos, rot, SecondsToDespawn);
        }

        /// <summary>
        /// 指定のゲームオブジェクトを該当プールへ返却（Despawn）する。
        /// </summary>
        static public void Despawn(GameObject obj)
        {
            PoolMember pm = obj.GetComponent<PoolMember>();
            if (pm == null)
            {
                // プールから生成されていないオブジェクトだった場合は Destroy で破棄し、注意ログを出す
                Debug.Log("オブジェクト '" + obj.name + "' はプールから生成されていません。代わりに Destroy します。");
                GameObject.Destroy(obj);
            }
            else
            {
                pm.myPool.Despawn(obj);
            }
        }
    }
}
