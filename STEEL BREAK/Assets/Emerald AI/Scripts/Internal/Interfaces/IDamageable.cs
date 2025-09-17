using UnityEngine;
using System.Collections.Generic;

namespace EmeraldAI
{
    /// <summary>
    /// 【IDamageable】
    /// 任意のターゲットの「被ダメージ情報」を監視・参照するためのインターフェイス。
    /// これにより、他のAIはカスタマイズ可能な関数を通じて任意ターゲットの情報へアクセスできます。
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// このインターフェイスを実装したスクリプトへダメージ値を渡すために使用します。
        /// </summary>
        /// <param name="DamageAmount">与えるダメージ量</param>
        /// <param name="AttackerTransform">攻撃者の Transform（任意）</param>
        /// <param name="RagdollForce">ラグドールへ与える力（既定: 100）</param>
        /// <param name="CriticalHit">クリティカルヒットかどうか</param>
        void Damage(int DamageAmount, Transform AttackerTransform = null, int RagdollForce = 100, bool CriticalHit = false);

        /// <summary>
        /// 現在の体力値（HP）
        /// </summary>
        int Health { get; set; }

        /// <summary>
        /// 開始時の体力値（最大HPなどの基準）
        /// </summary>
        int StartHealth { get; set; }

        /// <summary>
        /// 対象に付与されている「継続ダメージ（DoT）」などのアクティブ効果名を追跡するためのリスト。
        /// </summary>
        List<string> ActiveEffects { get; set; }
    }

    /// <summary>
    /// 【IDamageableHelper】
    /// IDamageable を持つ GameObject に対する拡張メソッド群。
    /// </summary>
    public static class IDamageableHelper
    {
        /// <summary>
        /// 対象が死亡状態（Health <= 0）かを判定します。
        /// </summary>
        public static bool IsDead(this GameObject receiver)
        {
            var m_IDamageable = receiver.GetComponent<IDamageable>();
            if (m_IDamageable != null) return m_IDamageable.Health <= 0;
            else return false;
        }

        /// <summary>
        /// 指定のアビリティ名が、対象のアクティブ効果に「未登録」であり、
        /// かつアビリティ名が空文字ではないことを確認します。
        /// </summary>
        public static bool CheckAbilityActiveEffects(this GameObject receiver, EmeraldAbilityObject AbilityData)
        {
            var m_IDamageable = receiver.GetComponent<IDamageable>();
            if (m_IDamageable != null)
            {
                return !m_IDamageable.ActiveEffects.Contains(AbilityData.AbilityName) && AbilityData.AbilityName != string.Empty;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 指定のアビリティ名を、対象のアクティブ効果リストへ追加します
        /// （未登録かつアビリティ名が空でない場合）。
        /// </summary>
        public static void AddAbilityActiveEffect(this GameObject receiver, EmeraldAbilityObject AbilityData)
        {
            var m_IDamageable = receiver.GetComponent<IDamageable>();
            if (m_IDamageable != null)
            {
                if (!m_IDamageable.ActiveEffects.Contains(AbilityData.AbilityName) && AbilityData.AbilityName != string.Empty)
                {
                    m_IDamageable.ActiveEffects.Add(AbilityData.AbilityName);
                }
            }
        }

        /// <summary>
        /// 指定のアビリティ名を、対象のアクティブ効果リストから削除します
        /// （登録済みかつアビリティ名が空でない場合）。
        /// </summary>
        public static void RemoveAbilityActiveEffect(this GameObject receiver, EmeraldAbilityObject AbilityData)
        {
            var m_IDamageable = receiver.GetComponent<IDamageable>();
            if (m_IDamageable != null)
            {
                if (m_IDamageable.ActiveEffects.Contains(AbilityData.AbilityName) && AbilityData.AbilityName != string.Empty)
                {
                    m_IDamageable.ActiveEffects.Remove(AbilityData.AbilityName);
                }
            }
        }
    }
}
