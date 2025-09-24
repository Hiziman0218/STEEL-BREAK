using System.Collections.Generic;
using UnityEngine;

public class BoneSet : MonoBehaviour
{
    /* �e���ʂƕK�v�ȃp�[�c
    //����

    //����

    //�r��(��)
      �@>��
        >��r
        >�O�r
        >��
        
    //�r��(�E)
      �@>��
        >��r
        >���r
        >��

    //����

    //�r��(���E)
        >���
        >����
        >��

    //�o�b�N�p�b�N
    */

    [System.Serializable]
    //�p�[�c�̍\����
    public struct BoneParts
    {
        public string m_Name;         //�p�[�c�̖��O
        public Transform m_Parts;     //�p�[�c�̃g�����X�t�H�[��
        public Vector3 m_Weight;      //�{�[���̃E�F�C�g
        public GameObject m_newParts; //�{�[���ɑΉ�����p�[�c
    }

    [System.Serializable]
    //���J�̍\����
    public struct Mecha
    {
        public string m_Name;           //���ʂ̖��O
        public List<BoneParts> m_Parts; //���ʂ��\������p�[�c�̃��X�g
    }
    public List<Mecha> mecha;

    private void LateUpdate()
    {
        //���X�g���̃��J�̑S�Ẵ{�[����ݒ�
        foreach(Mecha dummy in mecha)
        {
            //SetBone(dummy.m_Parts);
        } 
    }

    public void SetBone(List<BoneParts> Parts)
    {
        //�S�Ẵp�[�c�ɃE�F�C�g�𔽉f
        //Z�̗v�f�̂݃E�F�C�g���Q��
        foreach (BoneParts dummy in Parts)
        {
            float X = dummy.m_Weight.x;
            if (X <= 1.0f) X = 1.0f;
            float Y = dummy.m_Weight.y;
            if (Y <= 1.0f) Y = 1.0f;
            float Z = dummy.m_Weight.z;
            if (Z <= 1.0f) Z = 1.0f;
            dummy.m_Parts.localScale = new Vector3(
                X,
                Y,
                Z
                );
            dummy.m_newParts.transform.position = dummy.m_Parts.transform.position;
            dummy.m_newParts.transform.rotation= dummy.m_Parts.transform.rotation;
        }
    }
}
