using UnityEngine;  // Unity �̊�{API

namespace EmeraldAI  // EmeraldAI �p�̖��O���
{
    /// <summary>
    /// �yProjectileEffectsClass�z
    /// �e�iProjectile�j���痘�p�����G�t�F�N�g�Q�Ƃ��L���b�V�����A
    /// �K�v�ɉ����ėL�����^�������ł���悤�ɕێ����邽�߂̃f�[�^�N���X�B
    /// </summary>
    [System.Serializable]  // �V���A���C�Y���ăC���X�y�N�^�ɕ\���E�ۑ��ł���悤�ɂ��鑮��
    public class ProjectileEffectsClass
    {
        [Header("�e���g�p����p�[�e�B�N���̃����_���[�Q�Ɓi�L��/�����֑̐ؑΏہj")]
        public ParticleSystemRenderer EffectParticle;  // �G�t�F�N�g�\���ɗp���� ParticleSystemRenderer

        [Header("�G�t�F�N�g���܂Ƃ߂����[�g GameObject�i����/�񊈐���؂�ւ���Ώہj")]
        public GameObject EffectObject;                // ���̂ƂȂ�G�t�F�N�g�� GameObject

        /// <summary>
        /// �R���X�g���N�^�F�p�[�e�B�N�������_���[�ƃG�t�F�N�g�I�u�W�F�N�g���󂯎��A�e�t�B�[���h�֐ݒ肵�܂��B
        /// </summary>
        public ProjectileEffectsClass(ParticleSystemRenderer m_EffectParticle, GameObject m_EffectObject)
        {
            EffectParticle = m_EffectParticle;  // �󂯎�����p�[�e�B�N�������_���[��ێ�
            EffectObject = m_EffectObject;    // �󂯎�����G�t�F�N�g�̃��[�g�I�u�W�F�N�g��ێ�
        }
    }
}
