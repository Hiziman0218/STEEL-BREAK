using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackShot : MonoBehaviour
{
    public static void Attack_Shot(Transform m_My,Transform m_Player, CoolDown m_CoolDown)
    {
        Debug.Log("�U���I");

        //�N�[���_�E���ݒ�
        m_CoolDown.StartCoolDown("Attack", 2);
    }
}
