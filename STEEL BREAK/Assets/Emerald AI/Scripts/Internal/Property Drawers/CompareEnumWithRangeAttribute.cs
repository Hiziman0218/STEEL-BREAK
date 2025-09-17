using UnityEngine;
using System;

/// <summary>
/// �yCompareEnumWithRangeAttribute�z
/// ��r�p�̕ʃv���p�e�B�̒l�i�񋓒l�Ȃǁj�ɉ����āA�Ώۂ̃t�B�[���h/�v���p�e�B��
/// �u�C���X�y�N�^��ŕ\�����邩�ǂ����v��u�X���C�_�[�͈́v�𐧌䂷�邽�߂̑����ł��B
/// �Q�l: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class CompareEnumWithRangeAttribute : PropertyAttribute
{
    #region Fields
    /// <summary>
    /// ��r�ΏۂƂȂ�v���p�e�B���i�啶��/�������͋�ʂ���܂��j�B
    /// ��: "AttackType" �Ȃ�
    /// </summary>
    public string comparedPropertyName { get; private set; }

    /// <summary>
    /// ��r�Ɏg�p����񋓒l/�l�i1�ځj�B��v�����ꍇ�ɖ{�����̏������������܂��B
    /// </summary>
    public object comparedValue1 { get; private set; }

    /// <summary>
    /// ��r�Ɏg�p����񋓒l/�l�i2�ځE�C�Ӂj�B
    /// </summary>
    public object comparedValue2 { get; private set; }

    /// <summary>
    /// ��r�Ɏg�p����񋓒l/�l�i3�ځE�C�Ӂj�B
    /// </summary>
    public object comparedValue3 { get; private set; }

    /// <summary>
    /// �X���C�_�[�̎�ށi��������/�����j���w�肵�܂��B
    /// </summary>
    public StyleType styleType { get; private set; }

    [Header("�X���C�_�[�̍ŏ��l�iMin�j�BStyleType �ɉ����� float/int �Ƃ��ĉ��߂���܂�")]
    /// <summary>
    /// �X���C�_�[�̍ŏ��l�B
    /// </summary>
    public float min;

    [Header("�X���C�_�[�̍ő�l�iMax�j�BStyleType �ɉ����� float/int �Ƃ��ĉ��߂���܂�")]
    /// <summary>
    /// �X���C�_�[�̍ő�l�B
    /// </summary>
    public float max;

    /// <summary>
    /// �ϐ��ɓK�p�ł���X���C�_�[�̎�ށB
    /// </summary>
    public enum StyleType
    {
        /// <summary>
        /// �����ifloat�j�X���C�_�[
        /// </summary>
        FloatSlider = 1,

        /// <summary>
        /// �����iint�j�X���C�_�[
        /// </summary>
        IntSlider = 2
    }
    #endregion

    /// <summary>
    /// �y�R���X�g���N�^�z
    /// �������������ꂽ�ꍇ�ɂ̂݁A�Ώۂ̃t�B�[���h/�v���p�e�B��`�悵�܂��B
    /// </summary>
    /// <param name="comparedPropertyName">��r�Ώۂ̃v���p�e�B���i������v�E�啶����������ʁj�B</param>
    /// <param name="min">�X���C�_�[�̍ŏ��l�B</param>
    /// <param name="max">�X���C�_�[�̍ő�l�B</param>
    /// <param name="styleType">�X���C�_�[�̎�ށiFloat/Int�j�B</param>
    /// <param name="comparedValue1">��r�l1�i���̒l�ƈ�v������\���j�B</param>
    /// <param name="comparedValue2">��r�l2�i�C�Ӂj�B</param>
    /// <param name="comparedValue3">��r�l3�i�C�Ӂj�B</param>
    public CompareEnumWithRangeAttribute(string comparedPropertyName, float min, float max, StyleType styleType, object comparedValue1, object comparedValue2 = null, object comparedValue3 = null)
    {
        this.comparedPropertyName = comparedPropertyName;
        this.min = min;
        this.max = max;
        this.styleType = styleType;
        this.comparedValue1 = comparedValue1;
        this.comparedValue2 = comparedValue2;
        this.comparedValue3 = comparedValue3;
    }
}
