using UnityEngine;
using System;

/// <summary>
/// �yDrawIfRangeAttribute�z
/// �w�肵���u��r�Ώۃv���p�e�B�v�̒l�� <see cref="comparedValue"/> ��
/// <see cref="comparisonType"/> �̏����Ŕ�r���A**�������������ꂽ�ꍇ�ɂ̂�**
/// �Ώۂ̃t�B�[���h/�v���p�e�B���C���X�y�N�^�ɕ`�悷�鑮���ł��B
/// ����ɁA�`�悳���t�B�[���h�ɑ΂��āA<see cref="styleType"/> �ɉ�����
/// **�X���C�_�[�i�͈́j** ��K�p���܂��B
///
/// �Q�l: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawIfRangeAttribute : PropertyAttribute
{
    #region Fields

    /// <summary>
    /// ��r�ΏۂƂȂ�v���p�e�B���i�啶���������͋�ʂ���܂��j�B
    /// ��: "AttackType" �Ȃ�
    /// </summary>
    public string comparedPropertyName { get; private set; }

    /// <summary>
    /// ��r�ɗp����l�B��r�Ώۃv���p�e�B�̒l�Ƃ��̒l���ƍ����܂��B
    /// </summary>
    public object comparedValue { get; private set; }

    /// <summary>
    /// ��r�̕��@���w�肵�܂��i�������^���傫���^�ȉ� �Ȃǁj�B
    /// </summary>
    public ComparisonType comparisonType { get; private set; }

    /// <summary>
    /// �X���C�_�[�̎�ށi����/�����X���C�_�[/�����X���C�_�[�j���w�肵�܂��B
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
        /// ����i�X���C�_�[�Ȃ��A�ʏ�`��j
        /// </summary>
        Default = 1,

        /// <summary>
        /// �����ifloat�j�X���C�_�[
        /// </summary>
        FloatSlider = 2,

        /// <summary>
        /// �����iint�j�X���C�_�[
        /// </summary>
        IntSlider = 3
    }

    /// <summary>
    /// ��r�̎�ށB
    /// </summary>
    public enum ComparisonType
    {
        /// <summary>
        /// ������
        /// </summary>
        Equals = 1,

        /// <summary>
        /// �������Ȃ�
        /// </summary>
        NotEqual = 2,

        /// <summary>
        /// ���傫��
        /// </summary>
        GreaterThan = 3,

        /// <summary>
        /// ��菬����
        /// </summary>
        SmallerThan = 4,

        /// <summary>
        /// �ȉ��i�������܂��͓������j
        /// </summary>
        SmallerOrEqual = 5,

        /// <summary>
        /// �ȏ�i�傫���܂��͓������j
        /// </summary>
        GreaterOrEqual = 6
    }

    #endregion

    /// <summary>
    /// �y�R���X�g���N�^�z
    /// �������������ꂽ�ꍇ�ɂ̂ݑΏۃt�B�[���h��`�悵�A�����ɃX���C�_�[�͈͂�K�p���܂��B
    /// �񋓑́Ebool ���̔�r�ɑΉ����܂��B
    /// </summary>
    /// <param name="comparedPropertyName">��r�Ώۃv���p�e�B���i������v�E�啶����������ʁj�B</param>
    /// <param name="comparedValue">��r�l�i���̒l�Ɣ�r���ď�������j�B</param>
    /// <param name="comparisonType">��r���@�i�������E���傫�� ���j�B</param>
    /// <param name="min">�X���C�_�[�ŏ��l�B</param>
    /// <param name="max">�X���C�_�[�ő�l�B</param>
    /// <param name="styleType">�X���C�_�[��ށi����/����/�����j�B</param>
    public DrawIfRangeAttribute(string comparedPropertyName, object comparedValue, ComparisonType comparisonType, float min, float max, StyleType styleType)
    {
        this.comparedPropertyName = comparedPropertyName;
        this.comparedValue = comparedValue;
        this.comparisonType = comparisonType;
        this.min = min;
        this.max = max;
        this.styleType = styleType;
    }
}
