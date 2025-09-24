using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Collider m_attackCollider; //�����蔻��̃R���C�_�[
    [SerializeField] private float m_speed; //�e��
    [SerializeField] private float m_destroyTime; //���˂��Ă���폜�����܂ł̎���
    [SerializeField] private bool m_disappearOnHit; //�q�b�g���ɏ����邩
    private List<CharaBase> m_hitList = new List<CharaBase>(); //��x�̍U�����œ��������G�̃��X�g(���i�q�b�g�΍�)�������e�ۂ��G�ɓ������Ă���������Ȃ炢��Ȃ�
    [SerializeField] private float m_elapsedTime = 0f; //�o�ߎ��Ԍv���p�ϐ�
    private ObjectPool<Bullet> pool; //�I�u�W�F�N�g�v�[��
    private Rigidbody rb;

    ///<summary>
    ///���ˑO�̏������������܂Ƃ߂����\�b�h
    ///</summary>
    ///<param name="shooter">���ˌ�(Player��Enemy)</param>
    ///<param name="pool">���� Bullet �̏����v�[��</param>
    public void Initialize(GameObject shooter, ObjectPool<Bullet> pool)
    {
        //�e���������I�u�W�F�N�g�ɉ����ă��C���[��ݒ�
        if (shooter.CompareTag("Player"))
            gameObject.layer = LayerMask.NameToLayer("Player_Bullet");
        else if (shooter.CompareTag("Enemy"))
            gameObject.layer = LayerMask.NameToLayer("Enemy_Bullet");

        //�v�[���o�^
        this.pool = pool;

        //���W�b�h�{�f�B�̐ݒ�ύX�A�ǂȂǂ����蔲���Ȃ��悤�ɐݒ�
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        //���W�b�h�{�f�B�̗͂̏�����
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        //�q�b�g���X�g��o�ߎ��Ԃ����Z�b�g
        m_hitList.Clear();
        m_elapsedTime = 0f;

        //�R���C�_�[�͍ŏ��͖����ɂ��Ă����AAttackStart()�ŗL����
        m_attackCollider.enabled = false;

        //�A�N�e�B�u��(ObjectPool����擾���������inactive�Ȃ̂�)
        gameObject.SetActive(true);
    }

    ///<summary>
    ///�X�V����
    ///</summary>
    private void Update()
    {
        //�f���^�^�C�������Z
        m_elapsedTime += Time.deltaTime;
        //�����o�ߎ��Ԃ��폜���Ԃ��z���Ă�����A�v�[���ɕԋp
        if (m_elapsedTime > m_destroyTime)
        {
            pool.ReturnToPool(this);
        }
    }

    private void FixedUpdate()
    {
        //�ړ�����
        rb.isKinematic = false;
        rb.AddForce(transform.forward * m_speed);
    }

    /// <summary>
    /// �����蔻��
    /// </summary>
    /// <param name="other">���������I�u�W�F�N�g</param>
    private void OnTriggerEnter(Collider other)
    {
        var chara = other.GetComponentInParent<CharaBase>();
        //���������I�u�W�F�N�g���L�����N�^�[���A�q�b�g���X�g�ɖ�����΁A
        //�L�����N�^�[�Ƀ_���[�W��^���A�q�b�g���X�g�ɒǉ�
        if (chara != null && !m_hitList.Contains(chara))
        {
            chara.GetDamage(1.0f);
            m_hitList.Add(chara);
            //�q�b�g���ɏ�����e�Ȃ�A�v�[���ɕԋp
            if (m_disappearOnHit)
            {
                pool.ReturnToPool(this);
            }
        }
    }

    ///<summary>
    ///�U���J�n�̏���
    ///</summary>
    public void AttackStart()
    {
        //���X�g��������
        m_hitList.Clear();
        //�R���C�_�[��L����
        m_attackCollider.enabled = true;
    }

    ///<summary>
    ///�U���I���̏���
    ///</summary>
    public void AttackEnd()
    {
        //�R���C�_�[�𖳌���
        m_attackCollider.enabled = false;
    }
}
