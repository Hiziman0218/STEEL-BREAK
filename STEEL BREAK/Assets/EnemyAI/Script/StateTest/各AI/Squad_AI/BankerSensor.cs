using UnityEngine;

/// <summary>
/// �o���J�[�Z���T�[
#region �o���J�[�Z���T�[�Ƃ�?
/// �o���J�[�Z���T�[�Ƃ́A����Ӗ��y�q�z�̎��ŁA�v���C���[�����̃Z���T�[�G���A�ɐN�������ۂ�
/// �o���J�[�Z�b�g�֐N���̎|��`����
/// �A���A����̕����́y�Z���T�[�}�[�J�[���z�ŁA�����܂ŃZ���T�[�Ƃ��Ă̋@�\�����Ȃ��A
/// �N���������瑦���ɓP�ގw�����o���J�[�Z�b�g�ɏo���̂ł͂Ȃ��B(�o���J�[�Z�b�g���u�C���t���v��)
/// �o���J�[�Z���T�[���N���x���炷���ŁA�ǂ̕�������N�������̂����킩��Ղ��A
/// ���ʓ����o�H���m�ۂ�����ł̓P�ނ��\�ƂȂ�B
/// �A�������l���ɂ��N�U�ɂ͌����Ă��Ȃ����A����ăZ���T�[���������ɂ���������
#endregion
/// </summary>
public class BankerSensor : MonoBehaviour
{
    public bool m_PlayerHit = false;

    /// <summary>
    /// ��ԓ��Ƀv���C���[���ڐG�������Ă�����
    /// </summary>
    /// <param name="other">�ڐG��</param>
    private void OnTriggerStay(Collider other)
    {
        ///���肪�v���C���[�Ȃ�A���[��ON����ȊO�̓A���[��OFF
        if (other.tag != "Player")
            m_PlayerHit = false;
        else
            m_PlayerHit = true;
    }
    /// <summary>
    /// ��ԓ�����v���C���[���o��ꍇ
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        ///�A���[���I��
        m_PlayerHit = false;
    }
}
