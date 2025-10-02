using UnityEngine;

namespace EmeraldAI
{
    [System.Serializable]
    /// <summary>
    /// EmeraldAction の状態とランタイム情報を保持するデータクラス。
    /// ScriptableObject 側（EmeraldAction）には保持できない実行時データをここで管理します。
    /// インスペクターでの視認性向上のため、各メンバーに [Header] を付与しています。
    /// </summary>
    public class ActionsClass
    {
        //[Header("実行するアクション（ScriptableObject 参照）")]
        public EmeraldAction emeraldAction;

        //[Header("このアクションを有効化するか（ScriptableObject 内に保持できないためここで管理）")]
        public bool Enabled = true; // このアクションが有効かどうか（EmeraldAction ScriptableObject 内では保持できないため、ここで管理）

        [Header("現在このアクションがアクティブか（実行中フラグ。ScriptableObject 内に保持できないためここで管理）")]
        public bool IsActive; // このアクションの現在のアクティブ状態（EmeraldAction ScriptableObject 内では保持できないため、ここで管理）

        [Header("クールダウンの現在値（残り時間の計測）。ScriptableObject 内に保持できないためここで管理")]
        public float CooldownLengthTimer; // 現在のクールダウン時間を計測（EmeraldAction ScriptableObject 内では保持できないため、ここで管理）

        [Header("アクション継続時間（使用する場合はここに生成された長さを保持）")]
        public float ActionLength; // 生成されたアクションの継続時間（使用時）。ScriptableObject 内では保持できないため、ここで管理

        [Header("アクション継続時間の経過タイマー（ActionLength 用）")]
        public float ActionLengthTimer; // アクション継続時間の経過を計測（EmeraldAction ScriptableObject 内では保持できないため、ここで管理）

        [Header("汎用タイマー（カスタムアクション内で任意用途に使用）")]
        public float Timer; // カスタムアクション内での時間計測用（EmeraldAction ScriptableObject 内では保持できないため、ここで管理）

        [Header("このアクションの使用回数（統計・条件用）")]
        public int TimesUsed; // このアクションが使用された回数（EmeraldAction ScriptableObject 内では保持できないため、ここで管理）

        [Header("このアクションのコルーチン参照（停止や状態管理に使用）")]
        public Coroutine ActionCoroutine; // このアクションのコルーチン参照（EmeraldAction ScriptableObject 内では保持できないため、ここで管理）
    }
}
