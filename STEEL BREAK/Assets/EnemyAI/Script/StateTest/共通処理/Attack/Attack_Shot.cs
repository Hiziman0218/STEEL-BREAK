using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

//�ˌ�
public class Attack_Shot : MonoBehaviour
{
    public static void Execute(Enemy m_Enemy, CoolDown m_CoolDown)
    {
        Debug.Log("�ˌ�");

        //�Y������R���|�[�l���g�������
        if (m_Enemy != null)
        {
            //�����_���񐔎ˌ�������
            for(int i = 0; i<Random.Range(1, 7); i++)
            {
                m_Enemy.UseR();
            }
        }

        //�N�[���_�E���ݒ�
        m_CoolDown.StartCoolDown("Attack", 4);

    }
}
