using StateMachineAI;
using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

public class Attack_Side : MonoBehaviour
{
    //������̍U���̏���
    public static void Execute(Transform m_My, Transform m_EnemyModel, Transform m_Player, CoolDown m_CoolDown)
    {
        Debug.Log("���ʂ���U���I");
        //���f���̌������v���C���[�����֌�������
        m_My.LookAt(m_Player.position);
        m_EnemyModel.transform.LookAt(m_Player.position);

        //�U��
        m_My.Translate(new Vector3(0, 0, 1));

        //�N�[���_�E���ݒ�
        m_CoolDown.StartCoolDown("Attack", 2);
    }



}
