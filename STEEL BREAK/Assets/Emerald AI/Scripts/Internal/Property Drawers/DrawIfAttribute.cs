using UnityEngine;
using System;

/// <summary>
/// �yDrawIfAttribute�z
/// �������������ꂽ�ꍇ�ɂ̂݁A�Ώۂ̃t�B�[���h/�v���p�e�B���C���X�y�N�^��ɕ`�悷�鑮���B
/// �u����v���p�e�B�̒l�v�Ɓu�w��l�icomparedValue�j�v���r���A���v�����ꍇ�����`�悵�܂��B
/// </summary>
// �Q�l: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawIfAttribute : PropertyAttribute
{
    #region Fields

    /// <summary>
    /// ��r�ΏۂƂȂ�v���p�e�B���i�啶���������͋�ʂ���܂��j�B
    /// ��: "AttackType" �Ȃ�
    /// </summary>
    public string comparedPropertyName { get; private set; }

    /// <summary>
    /// ��r�ɗp����l�B��r�Ώۃv���p�e�B�̒l������ƈ�v�����ꍇ�ɕ`�悵�܂��B
    /// bool / enum ���ɑΉ��B
    /// </summary>
    public object comparedValue { get; private set; }

    /// <summary>
    /// �����𖞂����Ȃ��ꍇ�̖��������@�B
    /// </summary>
    public DisablingType disablingType { get; private set; }

    /// <summary>
    /// ���������@�̎�ށB
    /// </summary>
    public enum DisablingType
    {
        /// <summary>
        /// �ǂݎ���p�Ƃ��ĕ\���i�ҏW�s�j
        /// </summary>
        ReadOnly = 2,

        /// <summary>
        /// ���������`�悵�Ȃ��i��\���j
        /// </summary>
        DontDraw = 3
    }

    #endregion

    /// <summary>
    /// �������������ꂽ�ꍇ�ɂ̂݁A�Ώۂ̃t�B�[���h��`�悵�܂��ibool/enum�ɑΉ��j�B
    /// </summary>
    /// <param name="comparedPropertyName">��r�Ώۃv���p�e�B���i������v�E�啶����������ʁj�B</param>
    /// <param name="comparedValue">��r�l�i���̒l�ƈ�v�����ꍇ�ɕ`��j�B</param>
    /// <param name="disablingType">�����s��v���̖��������@�i����� <see cref="DisablingType.DontDraw"/>�j�B</param>
    public DrawIfAttribute(string comparedPropertyName, object comparedValue, DisablingType disablingType = DisablingType.DontDraw)
    {
        this.comparedPropertyName = comparedPropertyName;
        this.comparedValue = comparedValue;
        this.disablingType = disablingType;
    }
}
