using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Vector3 m_MovePoint;   //�ړ��p
    public Animator m_animator;   //�A�j���[�^�[
    public Rigidbody m_rigidbody; //���W�b�h�{�f�B

    public bool IsFireinRightHand { get; private set; } //�E�蕐���̍U���̍U���̓��͎󂯎��
    public bool IsFireinLeftHand { get; private set; }  //���蕐���̍U���̓��͎󂯎��
    public bool IsBoost { get; private set; }           //�u�[�X�g�̓��͎󂯎��
    public bool IsBoostDash { get; private set; }       //�u�[�X�g�_�b�V���̓��͎󂯎��
    public bool IsJump { get; private set; }            //�㏸�̓��͎󂯎��
    public bool IsJumpDown { get; private set; }        //�㏸�̓��͎󂯎��J�n
    public bool IsJumpUp { get; private set; }          //�㏸�̓��͎󂯎��I��
    public bool IsFall { get; private set; }            //���R�����̓��͎󂯎��
    public bool IsLockOnCancel { get; private set; }    //���b�N�I����ԉ����̓��͎󂯎��
    public bool IsTargetChange {  get; private set; }   //�^�[�Q�b�g�؂�ւ��̓��͎󂯎��
    public bool IsReload { get; private set; }          //�����[�h���邩(�蓮�����[�h)


    private void Start()
    {
        //�A�j���[�^�[���擾
        m_animator = GetComponent<Animator>();
        //���W�b�h�{�f�B���擾
        m_rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //���W����
        m_MovePoint = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

        //�A�j���[�V��������
        m_animator.SetFloat("X", m_MovePoint.x);
        m_animator.SetFloat("Y", m_MovePoint.z);

        IsFireinRightHand = Input.GetKey(KeyCode.E);    //E�L�[�������Ă���Ԃ͉E�蕐���̎g�p
        IsFireinLeftHand = Input.GetKey(KeyCode.Q);     //Q�L�[�������Ă���Ԃ͉E�蕐���̎g�p
        IsBoost = Input.GetMouseButton(1);              //�E�N���b�N�������Ă���Ԃ̓u�[�X�g(����)
        IsBoostDash = Input.GetMouseButtonDown(1);      //�E�N���b�N���������u�Ԃ̓u�[�X�g(��������)
        IsJump = Input.GetKey(KeyCode.Space);           //Space�L�[�������Ă���Ԃ͏㏸
        IsJumpDown = Input.GetKeyDown(KeyCode.Space);   //Space�L�[���������u�ԃW�����v���͂̌v���J�n
        IsJumpUp = Input.GetKeyUp(KeyCode.Space);       //Space�L�[�𗣂����u�ԃW�����v���͂̌v���I��
        IsFall = Input.GetKeyDown(KeyCode.C);           //C�L�[�������Ǝ��R����
        IsLockOnCancel = Input.GetKeyDown(KeyCode.Tab); //Tab�L�[�������ƃ��b�N�I���@�\���g��Ȃ�
        IsTargetChange = Input.GetKeyDown(KeyCode.V);   //V�L�[�������ƃ^�[�Q�b�g�؂�ւ�
        IsReload = Input.GetKeyDown(KeyCode.R);         //R�L�[�������Ǝ蓮�����[�h
    }
}
