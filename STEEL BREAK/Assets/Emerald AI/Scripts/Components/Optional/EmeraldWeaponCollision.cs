using UnityEngine;                           // Unity �̊�{API
using System;                                // ��{�V�X�e���@�\�i���g�p����������ێ��j
using System.Collections.Generic;            // List<T> ���g�p���邽��

namespace EmeraldAI                           // EmeraldAI �̖��O���
{
    [RequireComponent(typeof(BoxCollider))]   // BoxCollider �̕t�^��K�{��
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/weapon-collisions-component")] // ����Wiki�ւ̃����N

    // �y�N���X�T�v�zEmeraldWeaponCollision�F
    //  �ߐڕ���̓����蔻��iTrigger�j���Ǘ����A�U���A�j���[�V�����̃^�C�~���O��
    //  �R���C�_�[��L��/�������A�Փˑ���Ƀ_���[�W�������s���R���|�[�l���g�B
    public class EmeraldWeaponCollision : MonoBehaviour
    {
        [Header("�yEditor�\���z�ݒ�Q���B���i�܂肽���ݐ���j")]
        public bool HideSettingsFoldout;                  // �C���X�y�N�^�Őݒ�Z�N�V�������B����

        [Header("�yEditor�\���zWeapon Collision �Z�N�V�����̐܂肽����")]
        public bool WeaponCollisionFoldout;               // �Z�N�V�����̊J�t���O

        [Header("����̓����蔻��Ɏg�p���� BoxCollider�i�����擾�ETrigger�^�p�j")]
        public BoxCollider WeaponCollider;                // ����̓����蔻��p�R���W����

        [Header("Gizmos�i�I�����j�ŕ\������R���W�����̐F�i�������j")]
        public Color CollisionBoxColor = new Color(1, 0.85f, 0, 0.25f); // �R���W�����̉����F

        [Header("���̍U�����ɂ��łɃq�b�g�������^�[�Q�b�g�i�d���q�b�g�h�~�p�j")]
        public List<Transform> HitTargets = new List<Transform>();      // ����^�[�Q�b�g�ւ̑��d�_���[�W�}��

        [Header("�Փ˒��t���O�i�g��/�f�o�b�O�p�j")]
        public bool OnCollision;                         // �C�ӂ̏Փˏ�ԁi�������ł͖��g�p�j

        [Header("�e�� EmeraldSystem �Q�ƁiAI�{�̂ւ̃n���h�I�t�Ɏg�p�j")]
        EmeraldSystem EmeraldComponent;                  // �e�K�w����擾

        [Header("Kinematic �� Rigidbody�iTrigger�^�p�̈��艻�̂��ߕt�^�j")]
        Rigidbody m_Rigidbody;                           // �K�v�ɉ����ē��I�ɒǉ�


        private void Start()                             // ������
        {
            EmeraldComponent = GetComponentInParent<EmeraldSystem>();   // �e���� EmeraldSystem ���擾
            EmeraldComponent.CombatComponent.WeaponColliders.Add(this);  // ���̕���R���W������o�^
            EmeraldComponent.AnimationComponent.OnGetHit += DisableWeaponCollider; // ��e/�̂����蒆�͕��픻��𖳌���
            EmeraldComponent.AnimationComponent.OnRecoil += DisableWeaponCollider; // ���R�C������������
            WeaponCollider = GetComponent<BoxCollider>();                // ���g�� BoxCollider ���擾
            WeaponCollider.enabled = false;                              // �����͖���
            WeaponCollider.isTrigger = true;                             // Trigger �Ƃ��Ĉ���
            if (m_Rigidbody == null) m_Rigidbody = gameObject.AddComponent<Rigidbody>(); // ������� Rigidbody ��t�^
            m_Rigidbody.isKinematic = true;                              // �����V�~�����s��Ȃ��iTrigger�p�j
        }

        public void EnableWeaponCollider(string Name)    // �w�薼�̃I�u�W�F�N�g�Ɉ�v����ꍇ�̂ݗL�����i�A�j���C�x���g�z��j
        {
            if (gameObject.name == Name)                 // ���O��v�̃`�F�b�N
            {
                if (gameObject.GetComponent<Collider>() == null)
                    return;                              // Collider ��������Ή������Ȃ�

                WeaponCollider.enabled = true;           // �R���W������L����
                EmeraldComponent.CombatComponent.CurrentWeaponCollision = this; // ���݂̕���R���W�����Ƃ��ēo�^
            }
        }

        public void DisableWeaponCollider(string Name)   // �w�薼��v���ɖ�����
        {
            if (gameObject.name == Name)                 // ���O��v�̃`�F�b�N
            {
                if (gameObject.GetComponent<Collider>() == null)
                    return;                              // Collider ��������Ή������Ȃ�

                WeaponCollider.enabled = false;          // �R���W�����𖳌���
                EmeraldComponent.CombatComponent.CurrentWeaponCollision = null; // ���݂̕���Q�Ƃ��N���A
                HitTargets.Clear();                      // �q�b�g�ς݃��X�g�����Z�b�g
            }
        }

        void DisableWeaponCollider()                    // �ėp�̖������i��e/���R�C�����̃C�x���g�p�j
        {
            if (WeaponCollider.enabled)                  // �L����������
            {
                WeaponCollider.enabled = false;          // ������
                HitTargets.Clear();                      // �q�b�g�ς݂��N���A�i�V�����U���ɔ�����j
            }
        }

        private void OnTriggerEnter(Collider collision)  // Trigger �ɓ���������Ƃ̐ڐG����
        {
            if (collision.gameObject != EmeraldComponent.gameObject) // �������g�͖���
            {
                // ���肪 LBD �̕��� or IDamageable �������Ă��邩�m�F
                if (collision.gameObject.GetComponent<LocationBasedDamageArea>() != null || collision.gameObject.GetComponent<IDamageable>() != null)
                {
                    // LBDComponent �g�p���́ALBD �̃R���C�_�[���X�g�ɖ��o�^�̑���̂݃_���[�W
                    if (EmeraldComponent.LBDComponent != null && !EmeraldComponent.LBDComponent.ColliderList.Exists(x => x.ColliderObject == collision))
                    {
                        DamageTarget(collision.gameObject); // �_���[�W����
                    }
                    // LBD ���g���Ă��Ȃ��ꍇ�͂��̂܂܃_���[�W
                    else if (EmeraldComponent.LBDComponent == null)
                    {
                        DamageTarget(collision.gameObject); // �_���[�W����
                    }
                }
            }
        }

        /// <summary>
        /// �i���{��j����ƏՓ˂����^�[�Q�b�g�Ƀ_���[�W��^����i�Ώۂ� IDamageable �������Ƃ��O��j�B
        /// </summary>
        void DamageTarget(GameObject Target)             // �_���[�W�K�p����
        {
            MeleeAbility m_MeleeAbility = EmeraldComponent.CombatComponent.CurrentEmeraldAIAbility as MeleeAbility; // ���݂̋ߐڃA�r���e�B���擾

            if (m_MeleeAbility != null)                  // �ߐڃA�r���e�B���L���Ȃ�
            {
                Transform TargetRoot = m_MeleeAbility.GetTargetRoot(Target); // �_���[�W�����Ώۂ̃��[�gTransform���擾

                if (TargetRoot != null && !HitTargets.Contains(TargetRoot))  // �܂��q�b�g���Ă��Ȃ�����Ȃ�
                {
                    m_MeleeAbility.MeleeDamage(EmeraldComponent.gameObject, Target, TargetRoot); // �ߐڃ_���[�W��K�p
                    HitTargets.Add(TargetRoot);          // ���d�q�b�g�h�~�̂��ߋL�^
                }
            }
        }

        private void OnDrawGizmosSelected()              // �G�f�B�^�őI�𒆂̂݃R���W����������
        {
            if (WeaponCollider == null)
                return;                                  // �Q�Ƃ�������Ή������Ȃ�

            if (WeaponCollider.enabled)                  // �R���W�������L���ȂƂ��̂ݕ`��
            {
                Gizmos.color = CollisionBoxColor;        // �w��F��ݒ�
                Gizmos.matrix = Matrix4x4.TRS(
                    transform.TransformPoint(WeaponCollider.center), // ���S�ʒu�i���[�J�������[���h�j
                    transform.rotation,                               // ��]��K�p
                    transform.lossyScale);                            // �X�P�[����K�p
                Gizmos.DrawCube(Vector3.zero, WeaponCollider.size);   // �T�C�Y�Ɋ�Â��L���[�u��`��
            }
        }
    }
}
