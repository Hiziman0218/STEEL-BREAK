using Ilumisoft.RadarSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPlayer : PlayerBase
{
    //�f�o�b�O�p �������C���X�y�N�^�Őݒ�
    [SerializeField] private MonoBehaviour m_righthandWeaponMono; //�E�蕐��(�f�o�b�O)
    [SerializeField] private MonoBehaviour m_lefthandWeaponMono;  //���蕐��(�f�o�b�O)

    protected override void Initialize()
    {
        //���N���X�̏������Ăяo��
        base.Initialize();
    }
}