using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class CenterNPC : MonoBehaviour
{
    [Header("�v���C���[")]
    public Transform m_Player;
    [Header("�i�r�^�[�Q�b�g")]
    public Vector3 m_Target;
    [Header("�i�r")]
    public NavMeshAgent m_NavMeshAgent;
    [Header("�G�l�~�[���f��")]
    public Transform m_EnemyModel;

    [Header("�����␳")]
    public float m_Moku = 1;
    [Header("�U���\�p�x[-1 = ���S�ɔw��, 0 = �^��, 1 = ����]")]
    public float m_BackstabDotThreshold = -0.7f;
    [Header("�U���\����")]
    public float m_AttackDistance = 2f;
    void Start()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {

        if (m_NavMeshAgent)
            m_NavMeshAgent.destination = m_Target;
        CenterPoint();
        if (Input.GetMouseButtonDown(0))
            m_Moku *= - 1;
    }
    /// <summary>
    /// ����x���pAI�s��
    /// </summary>
    public void CenterPoint()
    {
        ///�Z���^�[�|�C���g���擾(���̂݊i�[�̕��������I)
        GameObject CenterMarker = GameObject.Find("�Z���^�[�|�C���^�[");
        ///�Z���^�|�C���g�������ꍇ�͂��̃��[�`���͎g�p�֎~
        if (!CenterMarker)
            return;

        ///�܂��́A�v���C���[(�^�[�Q�b�g)�̈ʒu���擾
        Vector3 TargetPosition =  m_Player.position;
        ///�^�[�Q�b�g��Y���𑵂���
        TargetPosition.y = transform.position.y;
        ///�Z���^�[�|�C���g�̍��W��(Y�␳�t��)�^�[�Q�b�g�ɍ��킹��
        CenterMarker.transform.position = TargetPosition;
        ///�Z���^�[�|�C���g�̌�����NPC�֌���������
        CenterMarker.transform.LookAt(this.transform.position);
        ///�P�񕪂̐���p�x����]
        CenterMarker.transform.Rotate(new Vector3(0, 10f * m_Moku, 0));
        ///�Z���^�[�|�C���g���^�[�Q�b�g����w�蕪��������(���΋����ʒu�w��)
        CenterMarker.transform.Translate(new Vector3(0, 0, 10));
        ///���̒n�_��NPC�̖ڕW�n�_�Ƃ���
        m_Target = CenterMarker.transform.position;
        ///�G�l�~�[�̃��f��������ύX
        EnemyModelLook();
        AttackChance();
    }
    /// <summary>
    /// ����NPC���f�������␳
    /// </summary>
    public void EnemyModelLook()
    {
        ///�^�[�Q�b�g�̃v���C���[���W���擾
        Vector3 Pos = m_Player.position;
        ///Y���𑵂���
        Pos.y = m_EnemyModel.position.y;
        ///Y���␳�t���ŁA���f���f�[�^���v���C���[�Ɍ���������
        m_EnemyModel.LookAt(Pos);
    }

    public void AttackChance()
    {
        if (m_Player == null) return;

        ///�v���C���[�̐��ʃx�N�g�����擾
        Vector3 playerForward = m_Player.forward;

        ///�v���C���[���猩���ANPC�̕����x�N�g��
        Vector3 directionToSelf = (transform.position - m_Player.position).normalized;

        ///���ς��g���Ċp�x���v�Z
        float dot = Vector3.Dot(playerForward, directionToSelf);

        ///�v���C���[�Ƃ̑��΋����`�F�b�N
        float distance = Vector3.Distance(transform.position, m_Player.position);

        // �w��ɂ��āA���������߂���΍U��
        if (dot < m_BackstabDotThreshold && distance <= m_AttackDistance)
        {
            if(Random.Range(0,100) > 95)
                AttackPlayer();
        }
    }
    /// <summary>
    /// ���g�̃v���C���[�ւ̍U��
    /// </summary>
    public void AttackPlayer()
    {
        Debug.Log("�w�ォ��U���I");
        this.transform.LookAt(m_Player.position);
        m_EnemyModel.transform.LookAt(m_Player.position);
        this.transform.Translate(new Vector3(0, 0, 1));
    }
}
