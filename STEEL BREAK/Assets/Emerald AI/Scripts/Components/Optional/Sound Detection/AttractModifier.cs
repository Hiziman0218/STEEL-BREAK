using System.Collections.Generic;                   // （保持）汎用コレクション
using System.Linq;                                  // （保持）LINQ
using UnityEngine;                                   // Unity ランタイムAPI

namespace EmeraldAI.SoundDetection                   // サウンド検知システム用の名前空間
{
    [RequireComponent(typeof(AudioSource))]          // このコンポーネントの利用には AudioSource が必須
    // 【クラス概要】AttractModifier：
    //  一定条件（開始時/トリガ/衝突/カスタム呼び出し）で「音」や「反応（Reaction）」を発生させ、
    //  指定半径内の Emerald AI に対して「誘引（Attract）」リアクションを実行させる補助コンポーネントです。
    //  主にサウンドデコイ・投擲物の着弾音・環境音トラップなどに利用できます。
    public class AttractModifier : MonoBehaviour
    {
        #region Variables
        [Header("プレイヤー（発生源）の派閥データ。敵対関係の判定に使用")]
        public FactionClass PlayerFaction;

        [Header("反応を検知する半径（ワールド空間, メートル換算相当）")]
        public int Radius = 10;

        [Header("衝突トリガ時に必要な最小相対速度（これ以上で反応）")]
        public float MinVelocity = 3.5f;

        [Header("トリガサウンドのクールダウン（秒）。連続再生を抑制")]
        public float SoundCooldownSeconds = 1f;

        [Header("リアクションのクールダウン（秒）。連続反応を抑制")]
        public float ReactionCooldownSeconds = 1f;

        [Header("この AttractModifier を起動できるレイヤー（トリガ条件）")]
        public LayerMask TriggerLayers = ~0; // 既定は全レイヤーを許可

        [Header("検知対象となる Emerald AI のレイヤー")]
        public LayerMask EmeraldAILayer;

        [Header("発火条件の種類（開始時/トリガ/衝突/カスタム呼び出し）")]
        public TriggerTypes TriggerType = TriggerTypes.OnCollision;

        [Header("検知AIへ実行させるリアクション（Reaction Object）")]
        public ReactionObject AttractReaction;

        [Header("敵対関係（Enemy）に限定して誘引を許可するか")]
        public bool EnemyRelationsOnly = true;

        [Header("トリガ時に再生する効果音クリップ（ランダム再生）")]
        public List<AudioClip> TriggerSounds = new List<AudioClip>();

        AudioSource m_AudioSource;                    // 内部用：サウンド再生に使用
        bool ReactionTriggered;                       // 内部用：リアクションクールダウン中か
        bool SoundTriggered;                          // 内部用：サウンドクールダウン中か
        #endregion

        #region Editor Variables
        [Header("インスペクタ表示：設定セクションを非表示にするか")]
        public bool HideSettingsFoldout;

        [Header("インスペクタ表示：AttractModifier 設定の折りたたみ")]
        public bool AttractModifierFoldout;
        #endregion

        void Start()
        {
            m_AudioSource = GetComponent<AudioSource>();   // 必須の AudioSource を取得

            // トリガタイプが「開始時」の場合、起動直後に検知処理を実行
            if (TriggerType == TriggerTypes.OnStart)
            {
                GetTargets();
            }
        }

        /// <summary>
        /// （日本語）トリガーコライダー侵入時に、設定に応じて AttractReaction を呼び出します。
        /// </summary>
        private void OnTriggerEnter(Collider collision)
        {
            if (TriggerType == TriggerTypes.OnTrigger)
            {
                // 衝突相手のレイヤーが TriggerLayers に含まれているか判定してから実行
                GetTargets(((1 << collision.gameObject.layer) & TriggerLayers) != 0);
            }
        }

        /// <summary>
        /// （日本語）衝突時に、相対速度が MinVelocity 以上であれば AttractReaction を呼び出します。
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            if (TriggerType == TriggerTypes.OnCollision && collision.relativeVelocity.magnitude >= MinVelocity)
            {
                // 衝突相手のレイヤーが TriggerLayers に含まれているか判定してから実行
                GetTargets(((1 << collision.gameObject.layer) & TriggerLayers) != 0);
            }
        }

        /// <summary>
        /// （日本語）外部スクリプトやアニメーションイベント等から手動起動するためのメソッド
        /// （TriggerType が OnCustomCall のときのみ有効）。
        /// </summary>
        public void ActivateAttraction()
        {
            if (TriggerType == TriggerTypes.OnCustomCall)
            {
                GetTargets();
            }
        }

        /// <summary>
        /// （日本語）指定半径内にいる Emerald AI を全て探索し、条件を満たすものへ AttractReaction を実行します。
        /// </summary>
        void GetTargets(bool HasTriggerLayer = true)
        {
            PlayTriggerSound(); // まず効果音の再生（クールダウン付き）

            // 起動直後の暴発防止（0.5秒未満）や、クールダウン/レイヤー不一致なら終了
            if (ReactionTriggered || Time.time < 0.5f || !HasTriggerLayer)
                return;

            // 指定レイヤー（Emerald AI）に属する対象を OverlapSphere で収集
            Collider[] m_DetectedTargets = Physics.OverlapSphere(transform.position, Radius, EmeraldAILayer);

            if (m_DetectedTargets.Length == 0)
                return;

            for (int i = 0; i < m_DetectedTargets.Length; i++)
            {
                if (m_DetectedTargets[i].GetComponent<EmeraldSoundDetector>() != null)
                {
                    EmeraldSystem EmeraldComponent = m_DetectedTargets[i].GetComponent<EmeraldSystem>(); // EmeraldSystem をキャッシュ

                    // フォロワー追従中の AI は Attract を無効（プレイヤー追従等の動作を尊重）
                    if (EmeraldComponent.TargetToFollow != null) continue;

                    // 敵対関係のみ許可が有効な場合、プレイヤー派閥との関係が「敵（RelationType == 0）」以外は除外
                    if (EnemyRelationsOnly && EmeraldComponent.DetectionComponent.FactionRelationsList.Exists(x => x.FactionIndex == PlayerFaction.FactionIndex && x.RelationType != 0)) continue;

                    if (AttractReaction != null)
                    {
                        // 検知元（このオブジェクト）を AI の SoundDetector に知らせる
                        EmeraldComponent.SoundDetectorComponent.DetectedAttractModifier = gameObject;

                        // リアクションを実行（第二引数 true は「Attract（誘引）」としての呼び出しを示す）
                        EmeraldComponent.SoundDetectorComponent.InvokeReactionList(AttractReaction, true);
                    }
                    else
                    {
                        // セットアップ不備の通知（挙動変更を避けるためメッセージは原文のまま）
                        Debug.Log("There's no Reaction Object on the " + gameObject.name + "'s AttractReaction slot. Please add one in order for Attract Modifier to work correctly.");
                    }
                }
            }

            // クールダウン開始
            ReactionTriggered = true;
            Invoke("ReactionCooldown", ReactionCooldownSeconds);
        }

        // トリガサウンドの再生（クールダウン管理）
        void PlayTriggerSound()
        {
            if (SoundTriggered || Time.time < 0.5f)
                return;

            if (TriggerSounds.Count > 0)
                m_AudioSource.PlayOneShot(TriggerSounds[Random.Range(0, TriggerSounds.Count)]);

            SoundTriggered = true;
            Invoke("SoundCooldown", SoundCooldownSeconds); // 一定時間後に解除
        }

        // サウンドのクールダウン解除
        void SoundCooldown()
        {
            SoundTriggered = false;
        }

        // リアクションのクールダウン解除
        void ReactionCooldown()
        {
            ReactionTriggered = false;
        }
    }
}
