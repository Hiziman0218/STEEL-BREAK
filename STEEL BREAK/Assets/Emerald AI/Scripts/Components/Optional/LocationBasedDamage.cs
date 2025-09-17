using System.Collections;                       // コルーチン等の基本コレクションAPI
using System.Collections.Generic;               // List<T> などの汎用コレクション
using UnityEngine;                              // Unity の基本API

namespace EmeraldAI                               // EmeraldAI 名前空間
{
    // 【クラス概要】LocationBasedDamage：
    // 部位ごとの当たり判定（コライダー）にダメージ倍率を持たせ、被弾部位に応じたダメージ計算を可能にするコンポーネント。
    // LBD（Location Based Damage）各コライダーのレイヤー/タグ設定や、死亡時・リセット時のレイヤー変更、ラグドール安定化のための再初期化を行う。
    public class LocationBasedDamage : MonoBehaviour
    {
        #region Variables                        // —— LBD の設定・参照・部位リストなど ——

        [Header("視線判定から除外するコライダー（Line of Sight を無視）")]
        public List<Collider> IgnoreLineOfSight = new List<Collider>(); // 視線可視化/検出時に無視するコライダー

        [Header("LBD用コライダーが属するレイヤー（生存時）")]
        public int LBDComponentsLayer;           // 生存時に設定する LBD 部位の Layer

        [Header("LBD用コライダーが属するレイヤー（死亡時）")]
        public int DeadLBDComponentsLayer;       // 死亡後に設定する LBD 部位の Layer

        [Header("LBDコライダーへ Layer/Tag を自動設定する（Yes=自動設定）")]
        public bool SetCollidersLayerAndTag = true; // 自動で Layer/Tag を適用するか

        [Header("LBDコライダーに付与する Tag（自動設定時）")]
        public string LBDComponentsTag = "Untagged"; // 自動設定で付与する Tag

        [Header("EmeraldSystem 参照（AI本体・各コンポーネントアクセス用）")]
        EmeraldSystem EmeraldComponent;          // 実行時に取得される AI 本体参照

        [Header("（Editor表示）LBD設定セクションの折りたたみ（初期表示）")]
        public bool LBDSettingsFoldout = true;   // インスペクタ折りたたみ

        [Header("（Editor表示）設定群を隠す（折りたたみ制御）")]
        public bool HideSettingsFoldout;         // インスペクタで非表示にするか

        [Header("部位コライダーの定義リスト（倍率/元姿勢などを保持）")]
        [SerializeField]                         // 元ソース通り保持（public だが SerializeField も付与されている）
        public List<LocationBasedDamageClass> ColliderList = new List<LocationBasedDamageClass>(); // LBD 部位一覧

        [System.Serializable]                    // インスペクタ編集可能なサブクラス
        public class LocationBasedDamageClass    // 部位コライダー1件分の情報
        {
            [Header("対象部位のコライダー（Rigidbody 必須）")]
            public Collider ColliderObject;      // 対象となる部位のコライダー

            [Header("この部位のダメージ倍率（1=等倍）")]
            public float DamageMultiplier = 1;   // 部位ごとのダメージ補正

            [Header("部位の基準ボーン位置（リセット時の復元に使用）")]
            public Vector3 BonePosition;         // 初期位置キャッシュ

            [Header("部位の基準ボーン回転（リセット時の復元に使用）")]
            public Quaternion BoneRotation;      // 初期回転キャッシュ

            public LocationBasedDamageClass(Collider m_ColliderObject, int m_DamageMultiplier) // コンストラクタ
            {
                ColliderObject = m_ColliderObject;  // コライダー参照を設定
                DamageMultiplier = m_DamageMultiplier; // 倍率を設定
            }

            public static bool Contains(List<LocationBasedDamageClass> m_LocationBasedDamageList, LocationBasedDamageClass m_LocationBasedDamageClass) // リストに含まれるか
            {
                foreach (LocationBasedDamageClass lbdc in m_LocationBasedDamageList) // 走査
                {
                    return (lbdc.ColliderObject == m_LocationBasedDamageClass.ColliderObject); // ※最初の要素のみ比較（元実装のまま）
                }

                return false;                    // 空、または先頭が一致しない場合
            }
        }
        #endregion

        private void Start()                     // Unity ライフサイクル：開始時
        {
            InitializeLocationBasedDamage();     // LBD 初期化
        }

        public void InitializeLocationBasedDamage() // LBD セットアップ処理（レイヤー/タグ適用、部位の初期登録等）
        {
            EmeraldComponent = GetComponent<EmeraldSystem>(); // AI 本体参照を取得
            EmeraldComponent.LBDComponent = this;             // AI へ自身を登録（他コンポーネントから参照される）

            // AI の基礎 BoxCollider を LBD 運用に合わせて調整（トリガー化・中心/サイズ）
            EmeraldComponent.AIBoxCollider.size = new Vector3(0.015f, EmeraldComponent.AIBoxCollider.size.y, 0.015f); // X/Z を極小に
            EmeraldComponent.AIBoxCollider.center = Vector3.zero;              // いったん原点へ
            EmeraldComponent.AIBoxCollider.center = Vector3.up * transform.localScale.y; // その後、身長ぶん上へ
            EmeraldComponent.AIBoxCollider.isTrigger = true;                   // トリガーとして運用
            if (SetCollidersLayerAndTag) EmeraldDetection.LBDLayers |= (1 << LBDComponentsLayer); // 自動設定が有効なら検出用マスクに LBD レイヤーを加算
            EmeraldComponent.HealthComponent.OnDeath += InitializeDeathLayer;  // 死亡時に部位コライダーのレイヤーを切り替える

            // — 各部位コライダーの初期設定 —
            for (int i = 0; i < ColliderList.Count; i++)
            {
                if (ColliderList[i].ColliderObject.GetComponent<Rigidbody>() != null) // Rigidbody がある部位のみ処理
                {
                    Rigidbody ColliderRigidbody = ColliderList[i].ColliderObject.GetComponent<Rigidbody>(); // 参照
                    ColliderRigidbody.useGravity = true;                 // 重力ON
                    ColliderRigidbody.isKinematic = true;                // キネマティック（通常時はアニメ駆動）

                    // 各コライダーの初期姿勢をキャッシュ（AI 再利用時の安定化に使用）
                    ColliderList[i].BonePosition = ColliderRigidbody.position;
                    ColliderList[i].BoneRotation = ColliderRigidbody.rotation;

                    // 当たり部位用のスクリプトを動的に付与し、倍率を設定
                    LocationBasedDamageArea DamageComponent = ColliderList[i].ColliderObject.gameObject.AddComponent<LocationBasedDamageArea>();
                    DamageComponent.EmeraldComponent = EmeraldComponent;              // AI 本体参照
                    DamageComponent.DamageMultiplier = ColliderList[i].DamageMultiplier; // ダメージ倍率

                    // Invector（Melee/Shooter）統合サポート（コンパイル定義がある場合のみ）
#if INVECTOR_MELEE || INVECTOR_SHOOTER
                    ColliderList[i].ColliderObject.gameObject.AddComponent<Invector.vCharacterController.vDamageReceiver>();
#endif

                    // Emerald の検出コンポーネントへ、無視すべきコライダーとして登録（自キャラの一部なので）
                    EmeraldComponent.DetectionComponent.IgnoredColliders.Add(ColliderList[i].ColliderObject);
                }

                // 自動設定が有効ならレイヤー/タグを適用
                if (SetCollidersLayerAndTag)
                {
                    ColliderList[i].ColliderObject.gameObject.layer = LBDComponentsLayer; // 生存時Layer
                    ColliderList[i].ColliderObject.gameObject.tag = LBDComponentsTag;     // 指定Tag
                }
            }

            // 視線判定から除外するコライダーは Ignore Raycast レイヤーへ
            for (int i = 0; i < IgnoreLineOfSight.Count; i++)
            {
                IgnoreLineOfSight[i].gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }

        void InitializeDeathLayer()             // 死亡時に部位コライダーの Layer を切り替える
        {
            if (!SetCollidersLayerAndTag) return; // 自動設定を行わない場合は何もしない

            for (int i = 0; i < ColliderList.Count; i++)
            {
                if (ColliderList[i].ColliderObject.GetComponent<Rigidbody>() != null)
                {
                    ColliderList[i].ColliderObject.gameObject.layer = DeadLBDComponentsLayer; // 死亡用Layerへ
                }
            }
        }

        /// <summary>
        /// LDB コンポーネントをリセットします（AI がリセットされたときに呼ばれます）。
        /// </summary>
        public void ResetLBDComponents()        // LBD各部位のリセットエントリ
        {
            for (int i = 0; i < ColliderList.Count; i++)
            {
                if (ColliderList[i].ColliderObject.GetComponent<Rigidbody>() != null)
                {
                    StartCoroutine(Reset(ColliderList[i])); // コルーチンで順次リセット
                }
            }
        }

        /// <summary>
        /// Rigidbody と Joint コンポーネントをリセットします。異なる場所で再利用された際にラグドールが不安定になるのを防ぎます。
        /// </summary>
        IEnumerator Reset(LocationBasedDamageClass LBDC) // 個別部位のリセット手順
        {
            Rigidbody ColliderRigidbody = LBDC.ColliderObject.GetComponent<Rigidbody>(); // 参照取得
            ColliderRigidbody.useGravity = true;                // 重力ON
            ColliderRigidbody.isKinematic = true;               // 通常時はキネマティック
            ColliderRigidbody.linearVelocity = Vector3.zero;    // 平行速度をゼロに（原実装のまま）
            ColliderRigidbody.angularVelocity = Vector3.zero;   // 角速度をゼロに
            if (SetCollidersLayerAndTag) LBDC.ColliderObject.gameObject.layer = LBDComponentsLayer; // 生存時Layerへ戻す

            yield return new WaitForSeconds(0.05f);             // 少し待ってから
            ColliderRigidbody.position = LBDC.BonePosition;     // 位置を初期値に復元
            yield return new WaitForSeconds(0.05f);             // 少し待ってから
            ColliderRigidbody.rotation = LBDC.BoneRotation;     // 回転を初期値に復元

            yield return new WaitForSeconds(0.05f);             // 少し待ってから
            Joint ColliderJoint = LBDC.ColliderObject.GetComponent<Joint>(); // 関節を取得
            if (ColliderJoint)
            {
                ColliderJoint.autoConfigureConnectedAnchor = false; // 一度オフ
                yield return new WaitForSeconds(0.05f);             // 待機
                ColliderJoint.autoConfigureConnectedAnchor = true;  // 再度オン（安定化を狙う）
            }

            yield return new WaitForSeconds(0.05f);             // 少し待ってから
            LBDC.ColliderObject.gameObject.SetActive(false);    // 一旦無効化
            yield return new WaitForSeconds(0.05f);             // 少し待ってから
            LBDC.ColliderObject.gameObject.SetActive(true);     // 再有効化（状態リフレッシュ）
        }
    }
}
