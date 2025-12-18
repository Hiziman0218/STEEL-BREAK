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
    //プレイヤーの装備する武器の残弾数
    public AmmoDisplay m_ammoDisplay;

    //ボスに設定するHPバー
    public ProgressBar m_bossHPBar;

    //プレイヤーが死亡したか
    private bool m_playerDied;

    void Awake()
    {
        Instance = this;

        //マウスカーソルを非表示にする
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (m_playerDied)
        {
            GameData.ShowGameOver();
        }
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
        m_ammoDisplay.SetPlayer(player);
    }

    /// <summary>
    /// ボス用のHPバーを取得
    /// </summary>
    /// <returns></returns>
    public ProgressBar GetBossHPBar()
    {
        return m_bossHPBar;
    }

    /// <summary>
    /// プレイヤーの死亡を通知
    /// </summary>
    public void PlayerDie()
    {
        m_playerDied = true;
    }

    private void OnDestroy()
    {
        //マウスカーソルを表示する
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
