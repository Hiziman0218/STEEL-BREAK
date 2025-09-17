using UnityEngine;                             // Unity の基本APIを利用するための名前空間
using System.Collections.Generic;              // List<T> などコレクションを使用するため
using UnityEngine.Events;                      // UnityEvent を使用するため

namespace EmeraldAI                              // EmeraldAI 関連のクラスをまとめる名前空間
{
    /// <summary>                                         // 既存の英語ドキュメント（変更なし）
    /// A component for spawning and positioning decals.
    /// </summary>
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/decal-component")] // Wiki へのヘルプURL
    [RequireComponent(typeof(EmeraldEvents))]  // 同一オブジェクトに EmeraldEvents を必須とする
    // 【クラス概要】EmeraldDecals：被ダメージイベントを契機に、血痕などの「デカール」エフェクトをスポーン・配置する補助コンポーネント
    public class EmeraldDecals : MonoBehaviour // MonoBehaviour を継承したデカール生成クラス
    {
        #region Decals Variables                 // —— デカール関連の公開/内部変数 ——

        [Header("血液エフェクトのプレハブ一覧（ランダム選択してスポーン）")] // インスペクタ見出し：血液エフェクト群
        public List<GameObject> BloodEffects = new List<GameObject>(); // スポーン候補となるデカール（血痕等）のプレハブ一覧

        [Header("血痕の生成高さ（yオフセットの目安）※このクラス内では直接未使用")] // 高さパラメータ（現実装では未参照）
        [Range(0, 3f)]                         // スライダー制限：0〜3
        public float BloodSpawnHeight = 0.33f;  // 生成時の高さ（将来拡張用）

        [Header("血痕生成の遅延秒数（Invoke による呼び出し遅延）")] // 生成ディレイの説明
        [Range(0, 3f)]                         // スライダー制限：0〜3
        public float BloodSpawnDelay = 0;       // エフェクト生成の遅延（秒）

        [Header("血痕生成時の半径（XZ 平面でのランダム半径）")] // 生成位置のばらつき半径
        [Range(0f, 3f)]                        // スライダー制限：0〜3
        public float BloodSpawnRadius = 0.6f;   // 生成半径（transform 周囲にランダム配置）

        [Header("血痕の自動消滅時間（秒）※オブジェクトプールのデスポーン時間")] // デスポーン時間の説明
        [Range(3f, 60f)]                       // スライダー制限：3〜60
        public int BloodDespawnTime = 16;       // 生存時間を過ぎると自動的にプールへ返却

        [Header("血痕を生成する確率（%）0=出ない / 100=毎回出る")] // 生成確率の説明
        [Range(1, 100)]                        // スライダー制限：1〜100
        public int OddsForBlood = 100;          // 乱数に対する当選閾値（%）

        [Header("EmeraldEvents 参照（被ダメージの UnityEvent を購読）")] // 必須コンポーネントの参照
        EmeraldEvents EmeraldEventsComponent;    // OnTakeDamageEvent へリスナー登録するための参照

        [Header("EmeraldSystem 参照（AI本体。状態/アニメを参照）")] // AI 本体への参照
        EmeraldSystem EmeraldComponent;          // アニメーション状態（ブロック中か等）の確認に使用
        #endregion

        #region Editor Variables                 // —— エディタ（インスペクタ）表示制御用の変数 ——

        [Header("設定セクションを隠す（折りたたみの表示制御）")] // インスペクタの見た目を調整
        public bool HideSettingsFoldout;         // 設定の折りたたみを隠すかどうか

        [Header("デカール設定の折りたたみ（開閉トグル）")] // セクションの開閉状態
        public bool DecalsFoldout;               // デカール関連設定の折りたたみ状態

        [Header("説明/注意メッセージを閉じたかどうかのフラグ")] // 一度閉じたかの記録
        public bool MessageDismissed;            // メッセージ非表示フラグ
        #endregion

        void Start()                              // Unity ライフサイクル：開始時に呼ばれる
        {
            Initialize();                         // 初期化処理（参照取得とイベント購読）
        }

        /// <summary>
        /// Initialize the Events Component.       // 既存コメント（英語）：イベント初期化
        /// </summary>
        void Initialize()                         // 初期化：必要なコンポーネント参照を取得し、イベントを購読
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();   // 同一 GameObject から EmeraldSystem を取得
            EmeraldEventsComponent = GetComponent<EmeraldEvents>(); // 同一 GameObject から EmeraldEvents を取得

            // 被ダメージ時イベントに、血痕生成処理（CreateBloodSplatter）を遅延実行するリスナーを登録
            EmeraldEventsComponent.OnTakeDamageEvent.AddListener(() => { CreateBloodSplatter(); });
        }

        public void CreateBloodSplatter()          // 血痕生成のエントリーポイント（外部/イベントから呼ばれる）
        {
            // Invoke により遅延して実体生成を行う（BloodSpawnDelay 秒）
            Invoke("DelayCreateBloodSplatter", BloodSpawnDelay);
        }

        void DelayCreateBloodSplatter()            // 実際のスポーン処理（Invoke で遅延呼び出し）
        {
            var Odds = Random.Range(0, 101);       // 0〜100 の整数乱数（上限101は排他的なので0..100）を取得

            // 確率判定に通り、AI 参照が有効、かつブロック中でなければ生成を実施
            if (Odds <= OddsForBlood && EmeraldComponent != null && !EmeraldComponent.AnimationComponent.IsBlocking)
            {
                // プレハブをランダム選択し、XZ ランダム半径位置にスポーン（オブジェクトプール利用）
                GameObject BloodEffect = EmeraldAI.Utility.EmeraldObjectPool.SpawnEffect(
                    BloodEffects[Random.Range(0, BloodEffects.Count)],                   // ランダムな血痕プレハブ
                    transform.position + Random.insideUnitSphere * BloodSpawnRadius,     // 中心から半径内のランダム位置
                    Quaternion.identity,                                                 // 初期回転は無回転
                    BloodDespawnTime);                                                   // デスポーン（自動消滅）秒数

                // 生成位置の高さをこの AI の高さに合わせる（y のみ補正）
                BloodEffect.transform.position = new Vector3(
                    BloodEffect.transform.position.x,                                    // X はそのまま
                    transform.position.y,                                                // Y を AI の高さへ
                    BloodEffect.transform.position.z);                                   // Z はそのまま

                // ランダムな傾き（X軸 55〜125度）と回転（Z軸 10〜350度）を与えて自然な散り方に
                BloodEffect.transform.rotation =
                    Quaternion.AngleAxis(Random.Range(55, 125), Vector3.right) *
                    Quaternion.AngleAxis(Random.Range(10, 350), Vector3.forward);

                // スケールを 0.8〜1.0 の範囲でランダム化（サイズのバリエーション）
                BloodEffect.transform.localScale = Vector3.one * Random.Range(0.8f, 1f);
            }
        }
    }
}
