using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// �yEmeraldAnimationEventsClass�z
    /// �A�j���[�V�����ݒ�UI�ŕ\������C�x���g���E�������EAnimationEvent�{�̂�1�Z�b�g�Ƃ��ĕێ�����f�[�^�N���X�B
    /// </summary>
    public class EmeraldAnimationEventsClass
    {
        [Header("UI�⃊�X�g��ŕ\������C�x���g���i���[�U�[�����̌��o���j")]
        public string eventDisplayName;         // �C�x���g�̕\����

        [Header("�C�x���g�̐������i�g�����E���ӓ_�Ȃǁj")]
        public string eventDescription;         // �C�x���g�̉���e�L�X�g

        [Header("Unity��AnimationEvent���́ifunctionName ��e��p�����[�^��ێ��j")]
        public AnimationEvent animationEvent;   // ���ۂɌĂяo����� AnimationEvent

        /// <summary>
        /// �R���X�g���N�^�F�\�����AAnimationEvent�A���������󂯎�菉�������܂��B
        /// </summary>
        public EmeraldAnimationEventsClass(string m_eventDisplayName, AnimationEvent m_animationEvent, string m_eventDescription)
        {
            eventDisplayName = m_eventDisplayName;   // �\������ݒ�
            animationEvent = m_animationEvent;       // AnimationEvent ��ݒ�
            eventDescription = m_eventDescription;   // ��������ݒ�
        }
    }
}
