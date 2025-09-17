using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "�L�����N�^�[�ړ��f�[�^", menuName = "GameData/�L�����N�^�[�ړ��f�[�^")]
public class UnityMoveData : ScriptableObject
{
    [Header("AnimationPanel�����N")]
    public GameObject m_AnimationPanel;

    [Header("AnimationPanel��Material�����N")]
    public Material m_Material;

    [Header("AnimationPanel�̐e�̕��������N")]
    public Rigidbody m_Rigidbody;

    [Header("���E�A�j���[�V����")]
    public List<Sprite> m_LR_Animetion;
    [Header("��A�j���[�V����")]
    public List<Sprite> m_Up_Animetion;
    [Header("���A�j���[�V����")]
    public List<Sprite> m_Down_Animetion;
    [Header("���s����Animation�ԍ�")]
    public int m_AnimationNo = 0;

    [Header("�p�^�[���A�j���[�V�����؂�ւ�����")]
    public float m_AnimationTime = 0;
    [Header("�p�^�[���A�j���[�V�����؂�ւ��ő厞��")]
    public float m_AnimationMaxTime = 0.2f;
    [Header("�L�����N�^�[�̈ړ����x")]
    public float m_Speed = 1.2f;

    //�C�[�i���ɂ��������
    public enum Muki
    {
        IsUp = 0,
        IsDown = 1,
        IsLeft = 2,
        IsRight = 3,
        IsNo = -1,
    }
    [Header("�������")]
    public Muki m_MukiData = Muki.IsRight;


    public void SetUp(UnityMoveData UMD, GameObject AnimationPanel,Material material)
    {

        m_LR_Animetion = UMD.m_LR_Animetion;
        m_Up_Animetion = UMD.m_Up_Animetion;
        m_Down_Animetion = UMD.m_Down_Animetion;
        m_Speed = UMD.m_Speed;
        m_AnimationTime = m_AnimationMaxTime = UMD.m_AnimationMaxTime;

        //AnimationPanel���擾
        m_AnimationPanel = AnimationPanel;
        //AnimationPanel��Material�擾
        m_Material = material;


    }

    /// <summary>
    /// �\���L�[����
    /// </summary>
    /// <param name="InputPoint">�L�[����</param>
    public void MoveAnimetion(Vector2 InputPoint)
    {
        //�A�N�V����Panel�������ꍇ�͏��������G���[��Ԃ�
        if (!m_AnimationPanel)
        {
            Debug.LogError("AnimationPanel�����݂��܂���!");
            return;
        }
        //���͂��Ă��Ȃ�
        if (InputPoint.x == 0 && InputPoint.y == 0)
        {
            Move(InputPoint);
            //�����I��
            return;
        }
        else
        {
            //�A�j���[�V����No�̉��Z����
            if (m_AnimationTime <= 0)
            {
                //�A�j���[�V��������i�߂�
                m_AnimationNo++;
                //�A�j���[�V�������Ԃ�������
                m_AnimationTime = m_AnimationMaxTime;
            }
            else
            {
                //�A�j���[�V�������Ԃ�����������
                m_AnimationTime -= 1.0f * Time.deltaTime;
            }

            //�㉺���͂�0�ł͂Ȃ�(�D��)
            if (InputPoint.y != 0)
            {
                //�㉺�ɃA�j���[�V�����ړ��p�^�[���Z�b�g
                //��Ȃ��A�j���A���Ȃ牺�A�j�����X�V
                if (InputPoint.y > 0)
                    AnimationSet(m_Up_Animetion);
                else
                    AnimationSet(m_Down_Animetion);
            }
            else if (InputPoint.x != 0)
            {
                //���E���ʂɃA�j���[�V�����ړ��p�^�[���Z�b�g
                //�E�p�^�[���̂ݍX�V
                AnimationSet(m_LR_Animetion);
            }
            //AnimationPanel���]����
            MukiSetUp(InputPoint);
            //�ړ�����
            Move(InputPoint);
        }
    }
    /// <summary>
    /// �A�j���[�V�������Z�b�g����
    /// </summary>
    /// <param name="Data">Animation�f�[�^</param>
    public void AnimationSet(List<Sprite> Data)
    {
        //�A�j���[�V�������f�[�^���X�g�𒴂��Ă���ꍇ��0�ɂ���B
        if (m_AnimationNo >= Data.Count)
            m_AnimationNo = 0;

        //�A�j���[�V�����X�V
        m_Material.mainTexture = 
            Data[m_AnimationNo].texture;

    }

    public void MukiSetUp(Vector2 InputPoint)
    {
        //���݂̌�������
        Muki muki = m_MukiData;
        //�Ȍ�͓��͕����ɏ]���ăC�[�i�����C��(���͊Y���������ꍇ�́A���̂܂�)
        if (InputPoint.y > 0)
            muki = Muki.IsUp;
        else if (InputPoint.y < 0)
            muki = Muki.IsDown;
        else if (InputPoint.x > 0)
            muki = Muki.IsRight;
        else if (InputPoint.x < 0)
            muki = Muki.IsLeft;

        //�����I��(���͐悪�O�Ɠ���)
        if (m_MukiData == muki)
            return;

        //�����X�V
        m_MukiData = muki;
        //�����𐳋K��
        m_AnimationPanel.transform.rotation = m_AnimationPanel.transform.parent.rotation;

        //�����͂̏ꍇ�A���]����
        if (m_MukiData == Muki.IsLeft)
            m_AnimationPanel.transform.Rotate(new Vector3(0, 180, 0));
    }
    /// <summary>
    /// �ړ�����
    /// </summary>
    /// <param name="InputPoint"></param>
    public void Move(Vector2 InputPoint)
    {
        //AnimationPanel�̏�̃I�u�W�F�N�g���{�̂Ȃ̂ŁA�{�̂��擾
        Transform Dummy = m_AnimationPanel.transform.parent;
        //�ړ������ɑ΂��āA�X�s�[�h�l���|���Ĉړ��͂𓾂�
        Vector2 Speed = InputPoint * m_Speed;
        //�{�̂Ƀ��[�J���ړ��ňړ�������
        //Dummy.GetComponent<Rigidbody>().linearVelocity =
        //    Dummy.right * Speed.x +
        //    Dummy.up * Speed.y;
    }
}
