using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// �yIAvoidable�z
    /// AI ���u������ׂ��Ώہi�I�u�W�F�N�g�^�A�r���e�B�j�v�����o�ł���悤�ɂ��邽�߂̃C���^�[�t�F�C�X�B
    /// AbilityTarget �́A���̃A�r���e�B���u�N�i�ǂ� Transform�j��_���Ă��邩�v��\���܂��B
    /// ���C���^�[�t�F�C�X�̂��߁A�C���X�y�N�^�ɕ\�������t�B�[���h�͎������A[Header] �����̕t�^�Ώۂ�����܂���B
    /// </summary>
    public interface IAvoidable
    {
        /// <summary>
        /// ���̃A�r���e�B���Ӑ}����^�[�Q�b�g�iTransform�j�B
        /// AI �͂��̒l���Q�Ƃ��āA���Ώۂ��ǂ����𔻒f���܂��B
        /// </summary>
        Transform AbilityTarget { get; set; }
    }
}
