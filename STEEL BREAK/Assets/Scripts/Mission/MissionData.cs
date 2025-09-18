using UnityEngine;

[CreateAssetMenu(fileName = "MissionData", menuName = "Mission/MissionData")]
public class MissionData : ScriptableObject
{
    public string missionName;     // ミッション名
    public string client;          // 発注企業名
    public string reward;          // 報酬（文字）

    [TextArea(3, 5)]
    public string description;     // 説明文

    public string sceneName;       // ステージ遷移用シーン名
    public Sprite missionImage;    // ステージ画像

    //追加項目
    public Sprite companyImage;    // 企業画像
    public string stageName;       // ステージ名
    public string[] objectives = new string[3];      // ミッション目標（3つ）
    public int[] objectiveAmounts = new int[3];      // ミッション目標金額（3つ）
    public int rewardAmount;                         // 報酬金額（数値）

    [TextArea(3, 5)]
    public string[] messages;      // 💬 複数メッセージ ←★変更

    public string[] voices;        // 🔊 各メッセージに対応するボイス

    public string battlesceneName;       // ステージ遷移用シーン名

    // 内部識別用
    public string missionID;
}
