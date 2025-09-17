using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWeapon
{
    /// <summary>
    /// ��Ɏ���
    /// </summary>
    /// <param name="hand"></param>
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
    /// IK�̊���/��������ʒB
    /// </summary>
    void SetIKFinished(bool IKFinished);

    ///<summary>
    ///���O���擾
    ///</summary>
    ///<returns>�����̖��O</returns>
    string GetName();

    void SetTeam(string team);
}
