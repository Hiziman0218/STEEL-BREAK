using Ilumisoft.RadarSystem;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //自身のインスタンス
    public static GameManager Instance { get; private set; }

    //プレイヤーに設定するHPバー
    public ProgressBar m_playerHPBar;
    //プレイヤーに設定するブーストゲージ
    public ProgressBar m_playerBoostGauge;
    //プレイヤーが持つレーダー
    public Radar m_radar;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// プレイヤーにUIを設定
    /// </summary>
    /// <param name="playerObj">プレイヤー</param>
    public void OnPlayerSpawned(GameObject playerObj)
    {
        Player player = playerObj.GetComponent<Player>();
        //プレイヤーの各種UIを設定
        player.SetHPBar(m_playerHPBar);
        player.SetBoostGauge(m_playerBoostGauge);
        player.SetRadar(m_radar);
    }
}
