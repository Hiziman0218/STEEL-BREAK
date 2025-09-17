using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// �yICombat�z
    /// �^�[�Q�b�g�̐퓬�s���i�U���E�K�[�h�E����Ȃǁj��u�_���[�W�ʒu�v���Ď��E�ǐՂ��邽�߂̃C���^�[�t�F�C�X�B
    /// ����ɂ��A����AI�͊֐���ʂ��ĔC�Ӄ^�[�Q�b�g�̐퓬���փA�N�Z�X�ł��܂��B
    /// ���ӁF�v���C���[���^�[�Q�b�g�ɂ����ۂ� Emerald AI �́u�K�[�h�i�u���b�N�j�v�u����v�@�\�𗘗p����ꍇ�A
    /// 3rd�p�[�e�B�^�J�X�^���̃L�����N�^�[�R���g���[�����ɖ{�C���^�[�t�F�C�X�̎������K�{�ł��B
    /// </summary>
    public interface ICombat
    {
        /// <summary>
        /// �^�[�Q�b�g�� Transform ���擾���܂��B
        /// </summary>
        Transform TargetTransform();

        /// <summary>
        /// �^�[�Q�b�g�́u�_���[�W�ʒu�v���擾���܂��i�q�b�g�G�t�F�N�g��_���[�W�\���̊�_�j�B
        /// </summary>
        Vector3 DamagePosition();

        /// <summary>
        /// �^�[�Q�b�g���U�����ł��邩�����o���܂��B
        /// </summary>
        bool IsAttacking();

        /// <summary>
        /// �^�[�Q�b�g���K�[�h�i�u���b�N�j���ł��邩�����o���܂��B
        /// </summary>
        bool IsBlocking();

        /// <summary>
        /// �^�[�Q�b�g������i�h�b�W�j���ł��邩�����o���܂��B
        /// </summary>
        bool IsDodging();

        /// <summary>
        /// �X�^���i�C��j��Ԃ𔭐������܂��B�����̓X�^���̌p�����ԁi�b�j�B
        /// Emerald AI ����W���I�ɌĂяo����܂����A�J�X�^���R���g���[�����X�^���@�\�����ꍇ��
        /// �������g���K�[����p�r�ɂ��g���\�ł��B
        /// </summary>
        void TriggerStun(float StunLength);
    }
}
