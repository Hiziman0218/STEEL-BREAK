namespace EmeraldAI
{
    /// <summary>
    /// �yPickTargetTypes�z
    /// �^�[�Q�b�g�̑I�������\���񋓑̂ł��B
    /// AI�����m�������̒�����A�ǂ̃^�[�Q�b�g���̗p���邩�̊���w�肵�܂��B
    /// </summary>
    public enum PickTargetTypes
    {
        /// <summary>
        /// �ł��߂��^�[�Q�b�g��I�����܂��B
        /// </summary>
        Closest = 0,

        /// <summary>
        /// �ŏ��Ɍ��m�����^�[�Q�b�g��I�����܂��B
        /// </summary>
        FirstDetected = 1,

        /// <summary>
        /// ���̒����烉���_���ɑI�����܂��B
        /// </summary>
        Random = 2
    }
}
