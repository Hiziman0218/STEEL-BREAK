using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// �o���J�[�Z�b�g�V�X�e��
#region �o���J�[�Ƃ�
/// �o���J�[�́ANPC(AI)���|�C���g�f�B�t�F���X(�n�_�h�q)�̏ꏊ�ŁA�^�N�e�B�J���G���A�Ƃ��]���܂��B
/// �i�ߊ�(�X�J�b�hAI)�ɂ���āA�L����AI�ɖh�q�n�_���w������A���̒n�_����v���C���[�������������܂��B
/// �o���J�[���@�\���Ȃ��A�������́A�o���J�[���P�����ꂽ�ꍇ�A�o���J�[�ɗ��܂�NPC�́A�ޔ��o�H��̃o���J�[
/// �ֈړ����A�p���퓬���s���܂��B
/// ���̃o���J�[�Z�b�g�̗��_�́A�X�i�C�v�|�C���g(����n�_)�̎w����v���C���[�̒n�_���犄��o���Č��߂鎖��
/// �o������A�h�q�n�_���߂ăv���C���[���������铙���o����ׁA��Փx�����Ɍ����܂��B
/// �����l���ɂ��AI���܂߂��ΐ�ɂ��A���̃V�X�e���͎g���₷���A�i���A�ޔ��̃^�C�~���O�����X�J�b�hAI�����
/// �w����g�ݍ��߂�΁A�^�N�e�B�J���o�g��(��p�퓬)���\�ɂȂ�܂��B
#endregion
/// </summary>
public class BankerSet : MonoBehaviour
{
    /// <summary>
    /// �o���J�[��NPC�����p�Z���^�[�ƁA�ޔ�����ׂ̃o���J�[�ʒu's
    /// </summary>
    [System.Serializable]
    public struct EscapeSensor
    {
        [Header("�o���J�[sensor")]
        public BankerSensor m_BankerSensor;
        [Header("�ޔ�p�̈ړ���o���J�[�ʒu")]
        public List<Transform> m_EscapePoint;
    }
    [Header("Sensor�Ɠ�����")]
    public List<EscapeSensor> m_EscapeSensors;

    [Header("���݃o���J�[�ɂ��郆�j�b�g���X�g")]
    public List<Unit> m_Units;

    private void Update()
    {
        ///�N���҃`�F�b�N
        InvaderHit();
    }
    /// <summary>
    /// �N���Ҋm�F
    /// </summary>
    public void InvaderHit()
    {
        if (m_Units.Count == 0)
            return;

        ///�S�ẴZ���T�[���`�F�b�N
        for(int i=0;i< m_EscapeSensors.Count;i++)
        {
            ///�Z���T�[�Ƀv���C���[���q�b�g���Ă���ꍇ
            if (m_EscapeSensors[i].m_BankerSensor.m_PlayerHit)
            {
                ///���̃Z���T�[������v���C���[���N�������̂ŁA�ޔ��s������点��
                EscapeRoutine(i);
            }
        }
    }
    /// <summary>
    /// �ޔ��s�����s
    /// </summary>
    /// <param name="No">�Z���T�[�ԍ�</param>
    public void EscapeRoutine(int No)
    {
        if (m_Units.Count == 0)
            return;
        ///���݃o���J�[�ɏ������Ă���NPC�𑖍�
        for(int i=0;i< m_Units.Count;i++)
        {
            if (m_Units[i])
            {
                ///NPC�Ɏ��̈ړ���Ƃ��āA�Z���T�[�Ɉ��������������������������ޔ��o���J�[��
                ///����o���ă^�[�Q�b�g�ɓn��
                m_Units[i].m_Target = m_EscapeSensors[No].m_EscapePoint[
                    Random.Range(
                        0,
                        m_EscapeSensors[No].m_EscapePoint.Count)];
                ///���o���J�[������j������
                m_Units[i].m_BankerSet = null;
                m_Units[i] = null;
            }
        }
        // null�v�f���폜
        m_Units.RemoveAll(item => item == null);
    }
    /// <summary>
    /// NPC���o���J�[�ɓ���
    /// </summary>
    /// <param name="other">NPC</param>
    public void OnTriggerStay(Collider other)
    {
        ///�����NPC�ł���
        if (other.GetComponent<Unit>())
        {
            ///NPC�̍s�悪�{�o���J�[�ł���A�܂��ANPC���̏����o���J�[��null�ł���
            if (other.GetComponent<Unit>().m_Target == this.transform && 
                other.GetComponent<Unit>().m_BankerSet == null)
            {
                ///NPC���̃o���J�[�����͎��g�Ƃ���
                other.GetComponent<Unit>().m_BankerSet = this;
                ///�o���J�[�������X�g��NPC��ǉ�����
                m_Units.Add(other.GetComponent<Unit>());
            }
        }
    }
    private void OnDrawGizmos()
    {
        foreach (EscapeSensor ES in m_EscapeSensors)
        {
            Handles.color = Color.red;
            // ���݂̃I�u�W�F�N�g�ʒu�ɔ��a1�̋��̂�`��
            Gizmos.color = Color.white;
            Gizmos.DrawCube(ES.m_BankerSensor.transform.position, new Vector3(1,2,1));
            for (int i = 0; i < ES.m_EscapePoint.Count; i++)
            {
                Handles.DrawAAPolyLine(
                    10.0f,
                    ES.m_BankerSensor.transform.position,
                    ES.m_EscapePoint[i].position);
                // ���݂̃I�u�W�F�N�g�ʒu�ɔ��a1�̋��̂�`��
                Gizmos.DrawSphere(ES.m_EscapePoint[i].position, 1.0f);
            }
        }
    }
}
