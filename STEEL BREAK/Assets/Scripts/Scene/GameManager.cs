using Ilumisoft.RadarSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //���g�̃C���X�^���X
    public static GameManager Instance { get; private set; }

    //�v���C���[�ɐݒ肷��HP�o�[
    public ProgressBar m_playerHPBar;
    //�v���C���[�ɐݒ肷��u�[�X�g�Q�[�W
    public ProgressBar m_playerBoostGauge;
    //�v���C���[�������[�_�[
    public Radar m_radar;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// �v���C���[��UI��ݒ�
    /// </summary>
    /// <param name="playerObj"></param>
    public void OnPlayerSpawned(GameObject playerObj)
    {
        Player player = playerObj.GetComponent<Player>();
        //�v���C���[��HP�o�[�A�u�[�X�g�Q�[�W��ݒ�
        player.SetHPBar(m_playerHPBar);
        player.SetBoostGauge(m_playerBoostGauge);
        player.SetRadar(m_radar);
    }
}
