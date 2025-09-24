using UnityEngine;

public interface IWeapon
{
    /// <summary>
    /// ��Ɏ����A����������
    /// </summary>
    /// <param name="hand">��̃g�����X�t�H�[��</param>
    /// <param name="left">���肩(�E�肩����̓���Ȃ̂Ńt���O�Ǘ�)</param>
    public void AttachToHand(Transform hand, bool left);

    ///<summary>
    ///�����g�p
    ///</summary>
    void Use();

    /// <summary>
    /// ���탊���[�h
    /// </summary>
    void Reload();

    /// <summary>
    /// IK�̊���/��������ݒ�
    /// </summary>
    /// <param name="IKFinished">IK������������</param>
    void SetIKFinished(bool IKFinished);

    ///<summary>
    ///���O���擾
    ///</summary>
    ///<returns>�����̖��O</returns>
    string GetName();

    /// <summary>
    /// �`�[����ݒ�
    /// </summary>
    /// <param name="team">���݂̃`�[��</param>
    void SetTeam(string team);
}
