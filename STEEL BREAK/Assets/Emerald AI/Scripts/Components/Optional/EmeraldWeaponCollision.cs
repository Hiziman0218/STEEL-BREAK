using UnityEngine;                           // Unity の基本API
using System;                                // 基本システム機能（未使用だが原文を保持）
using System.Collections.Generic;            // List<T> を使用するため

namespace EmeraldAI                           // EmeraldAI の名前空間
{
    [RequireComponent(typeof(BoxCollider))]   // BoxCollider の付与を必須化
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/weapon-collisions-component")] // 公式Wikiへのリンク

    // 【クラス概要】EmeraldWeaponCollision：
    //  近接武器の当たり判定（Trigger）を管理し、攻撃アニメーションのタイミングで
    //  コライダーを有効/無効化、衝突相手にダメージ処理を行うコンポーネント。
    public class EmeraldWeaponCollision : MonoBehaviour
    {
        [Header("【Editor表示】設定群を隠す（折りたたみ制御）")]
        public bool HideSettingsFoldout;                  // インスペクタで設定セクションを隠すか

        [Header("【Editor表示】Weapon Collision セクションの折りたたみ")]
        public bool WeaponCollisionFoldout;               // セクションの開閉フラグ

        [Header("武器の当たり判定に使用する BoxCollider（自動取得・Trigger運用）")]
        public BoxCollider WeaponCollider;                // 武器の当たり判定用コリジョン

        [Header("Gizmos（選択時）で表示するコリジョンの色（半透明）")]
        public Color CollisionBoxColor = new Color(1, 0.85f, 0, 0.25f); // コリジョンの可視化色

        [Header("この攻撃中にすでにヒットさせたターゲット（重複ヒット防止用）")]
        public List<Transform> HitTargets = new List<Transform>();      // 同一ターゲットへの多重ダメージ抑制

        [Header("衝突中フラグ（拡張/デバッグ用）")]
        public bool OnCollision;                         // 任意の衝突状態（現実装では未使用）

        [Header("親の EmeraldSystem 参照（AI本体へのハンドオフに使用）")]
        EmeraldSystem EmeraldComponent;                  // 親階層から取得

        [Header("Kinematic な Rigidbody（Trigger運用の安定化のため付与）")]
        Rigidbody m_Rigidbody;                           // 必要に応じて動的に追加


        private void Start()                             // 初期化
        {
            EmeraldComponent = GetComponentInParent<EmeraldSystem>();   // 親から EmeraldSystem を取得
            EmeraldComponent.CombatComponent.WeaponColliders.Add(this);  // この武器コリジョンを登録
            EmeraldComponent.AnimationComponent.OnGetHit += DisableWeaponCollider; // 被弾/のけぞり中は武器判定を無効化
            EmeraldComponent.AnimationComponent.OnRecoil += DisableWeaponCollider; // リコイル中も無効化
            WeaponCollider = GetComponent<BoxCollider>();                // 自身の BoxCollider を取得
            WeaponCollider.enabled = false;                              // 初期は無効
            WeaponCollider.isTrigger = true;                             // Trigger として扱う
            if (m_Rigidbody == null) m_Rigidbody = gameObject.AddComponent<Rigidbody>(); // 無ければ Rigidbody を付与
            m_Rigidbody.isKinematic = true;                              // 物理シミュを行わない（Trigger用）
        }

        public void EnableWeaponCollider(string Name)    // 指定名のオブジェクトに一致する場合のみ有効化（アニメイベント想定）
        {
            if (gameObject.name == Name)                 // 名前一致のチェック
            {
                if (gameObject.GetComponent<Collider>() == null)
                    return;                              // Collider が無ければ何もしない

                WeaponCollider.enabled = true;           // コリジョンを有効化
                EmeraldComponent.CombatComponent.CurrentWeaponCollision = this; // 現在の武器コリジョンとして登録
            }
        }

        public void DisableWeaponCollider(string Name)   // 指定名一致時に無効化
        {
            if (gameObject.name == Name)                 // 名前一致のチェック
            {
                if (gameObject.GetComponent<Collider>() == null)
                    return;                              // Collider が無ければ何もしない

                WeaponCollider.enabled = false;          // コリジョンを無効化
                EmeraldComponent.CombatComponent.CurrentWeaponCollision = null; // 現在の武器参照をクリア
                HitTargets.Clear();                      // ヒット済みリストをリセット
            }
        }

        void DisableWeaponCollider()                    // 汎用の無効化（被弾/リコイル時のイベント用）
        {
            if (WeaponCollider.enabled)                  // 有効だったら
            {
                WeaponCollider.enabled = false;          // 無効化
                HitTargets.Clear();                      // ヒット済みをクリア（新しい攻撃に備える）
            }
        }

        private void OnTriggerEnter(Collider collision)  // Trigger に入った相手との接触判定
        {
            if (collision.gameObject != EmeraldComponent.gameObject) // 自分自身は無視
            {
                // 相手が LBD の部位 or IDamageable を持っているか確認
                if (collision.gameObject.GetComponent<LocationBasedDamageArea>() != null || collision.gameObject.GetComponent<IDamageable>() != null)
                {
                    // LBDComponent 使用時は、LBD のコライダーリストに未登録の相手のみダメージ
                    if (EmeraldComponent.LBDComponent != null && !EmeraldComponent.LBDComponent.ColliderList.Exists(x => x.ColliderObject == collision))
                    {
                        DamageTarget(collision.gameObject); // ダメージ処理
                    }
                    // LBD を使っていない場合はそのままダメージ
                    else if (EmeraldComponent.LBDComponent == null)
                    {
                        DamageTarget(collision.gameObject); // ダメージ処理
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）武器と衝突したターゲットにダメージを与える（対象が IDamageable を持つことが前提）。
        /// </summary>
        void DamageTarget(GameObject Target)             // ダメージ適用処理
        {
            MeleeAbility m_MeleeAbility = EmeraldComponent.CombatComponent.CurrentEmeraldAIAbility as MeleeAbility; // 現在の近接アビリティを取得

            if (m_MeleeAbility != null)                  // 近接アビリティが有効なら
            {
                Transform TargetRoot = m_MeleeAbility.GetTargetRoot(Target); // ダメージ処理対象のルートTransformを取得

                if (TargetRoot != null && !HitTargets.Contains(TargetRoot))  // まだヒットしていない相手なら
                {
                    m_MeleeAbility.MeleeDamage(EmeraldComponent.gameObject, Target, TargetRoot); // 近接ダメージを適用
                    HitTargets.Add(TargetRoot);          // 多重ヒット防止のため記録
                }
            }
        }

        private void OnDrawGizmosSelected()              // エディタで選択中のみコリジョンを可視化
        {
            if (WeaponCollider == null)
                return;                                  // 参照が無ければ何もしない

            if (WeaponCollider.enabled)                  // コリジョンが有効なときのみ描画
            {
                Gizmos.color = CollisionBoxColor;        // 指定色を設定
                Gizmos.matrix = Matrix4x4.TRS(
                    transform.TransformPoint(WeaponCollider.center), // 中心位置（ローカル→ワールド）
                    transform.rotation,                               // 回転を適用
                    transform.lossyScale);                            // スケールを適用
                Gizmos.DrawCube(Vector3.zero, WeaponCollider.size);   // サイズに基づくキューブを描画
            }
        }
    }
}
