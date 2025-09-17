using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 【ICombat】
    /// ターゲットの戦闘行動（攻撃・ガード・回避など）や「ダメージ位置」を監視・追跡するためのインターフェイス。
    /// これにより、他のAIは関数を通じて任意ターゲットの戦闘情報へアクセスできます。
    /// 注意：プレイヤーをターゲットにした際に Emerald AI の「ガード（ブロック）」「回避」機能を利用する場合、
    /// 3rdパーティ／カスタムのキャラクターコントローラ側に本インターフェイスの実装が必須です。
    /// </summary>
    public interface ICombat
    {
        /// <summary>
        /// ターゲットの Transform を取得します。
        /// </summary>
        Transform TargetTransform();

        /// <summary>
        /// ターゲットの「ダメージ位置」を取得します（ヒットエフェクトやダメージ表示の基準点）。
        /// </summary>
        Vector3 DamagePosition();

        /// <summary>
        /// ターゲットが攻撃中であるかを検出します。
        /// </summary>
        bool IsAttacking();

        /// <summary>
        /// ターゲットがガード（ブロック）中であるかを検出します。
        /// </summary>
        bool IsBlocking();

        /// <summary>
        /// ターゲットが回避（ドッジ）中であるかを検出します。
        /// </summary>
        bool IsDodging();

        /// <summary>
        /// スタン（気絶）状態を発生させます。引数はスタンの継続時間（秒）。
        /// Emerald AI から標準的に呼び出されますが、カスタムコントローラがスタン機構を持つ場合は
        /// それらをトリガーする用途にも拡張可能です。
        /// </summary>
        void TriggerStun(float StunLength);
    }
}
