using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

/// <summary>
///�N�[���_�E���̎g����
///��j m_CoolDown.StartCoolDown("Attack",10);
///�̂悤�ɃN�[���_�E���̖��O�ƕb�������߂�
///��j!IsCoolDown("Attack")
///�Ƃ��邱�Ƃ�Attack�̃N�[���_�E�����I��������ǂ������m�F���邱�Ƃ��ł���
/// </summary>

public class CoolDown : MonoBehaviour
{
    //�e�s���̃N�[���_�E�����Ԃ��擾
    private Dictionary<string, float> CoolDowns = new Dictionary<string, float>();
    [Header("�N�[���_�E������")]
    private Dictionary<string, float> CoolTimes = new Dictionary<string, float>();
    //�N�[���_�E���J�n
    public void StartCoolDown(string actionName, float duration)
    {
        //Debug.Log("�N�[���_�E���J�n");
        CoolDowns[actionName] = duration;
        CoolTimes[actionName] = Time.time;
    }

    //�N�[���_�E�������ǂ���
    public bool IsCoolDown(string actionName)
    {
        //�J�n�L�^���Ȃ��Ȃ�
        if (!CoolDowns.ContainsKey(actionName) || !CoolTimes.ContainsKey(actionName))
        {
            //�N�[���_�E���f�[�^�Ȃ����N�[���_�E�����ł͂Ȃ�
            return false;
        }

        return Time.time - CoolTimes[actionName] < CoolDowns[actionName];
    }

    // �N�[���_�E���̏�Ԃ����O�ɏo��
    //�g�����@��jowner.m_CoolDown.DebugCoolDownProgress("Attack");
    public void DebugCoolDownProgress(string actionName)
    {
        if (!CoolDowns.ContainsKey(actionName) || !CoolTimes.ContainsKey(actionName))
        {
            Debug.Log($"[{actionName}] �N�[���_�E���Ȃ�");
            return;
        }

        float elapsedTime = Time.time - CoolTimes[actionName];
        float remainingTime = CoolDowns[actionName] - elapsedTime;

        Debug.Log($"[{actionName}] �o�ߎ���: {elapsedTime:F2} �b / �c�莞��: {remainingTime:F2} �b");
    }

}