using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : CharaBase
{
    public bool IsAlive { get; private set; } = true; //��������
    public event Action<Enemy> OnDeath;  //���S�C�x���g
    public GameObject DestructionEffect; //�j��G�t�F�N�g
    public EnemyGun weaponR; //�E����(�^�ϊ��O)
    public EnemyGun weaponL; //������(�^�ϊ��O)

    protected override void Initialize()
    {
        //���N���X�̏����������Ăяo��
        base.Initialize();

        weaponR.SetTeam(m_parameter.GetTeam());
        weaponL.SetTeam(m_parameter.GetTeam());
    }

    private void Update()
    {
        //UseR();
        //UseL();

        //HP��0�ȉ��Ȃ�A���S
        if (m_status.GetHP() <= 0)
        {
            Die();
        }
    }

    public void UseR()
    {
        weaponR ?.Fire();
    }

    public void UseL()
    {
        weaponL ?.Fire();
    }

    /// <summary>
    /// ���S����
    /// </summary>
    private void Die()
    {
        //���S�C�x���g��ʒm
        OnDeath?.Invoke(this);

        //�t���O��false�ɂ��A�G�t�F�N�g���Đ�������폜
        IsAlive = false;
        Instantiate(DestructionEffect, transform.position, transform.rotation);
        Destroy(gameObject);
        StageCount();
    }

    private void StageCount()
    {
        GameObject stageObj = GameObject.FindGameObjectWithTag("Stage");
        if (stageObj != null)
        {
            Stage stage = stageObj.GetComponent<Stage>();
            if (stage != null)
            {
                stage.OnEnemyDestroyed();
            }
        }
    }
}