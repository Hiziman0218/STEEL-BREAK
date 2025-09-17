namespace EmeraldAI
{
    /// <summary>
    /// �yAnimationStateTypes�z
    /// �A�j���[�V������Ԃ�\���񋓑́B<see cref="System.FlagsAttribute"/> �ɂ��r�b�g�t���O�Ƃ��Ĉ����A
    /// �����̏�Ԃ𓯎��ɕێ��ł��܂��i��FMoving | TurningLeft�j�B
    /// �e�l�� 1 �����V�t�g���Ē�`����A���݂ɏd�����Ȃ��r�b�g�������܂��B
    /// </summary>
    [System.Flags]
    public enum AnimationStateTypes
    {
        /// <summary>
        /// ������̏�Ԃɂ��Y�����Ȃ��i�t���O�Ȃ��j
        /// </summary>
        None = 0,

        /// <summary>
        /// �A�C�h���i�ҋ@�j���
        /// </summary>
        Idling = 1 << 1,

        /// <summary>
        /// �ړ���
        /// </summary>
        Moving = 1 << 2,

        /// <summary>
        /// ��ޒ�
        /// </summary>
        BackingUp = 1 << 3,

        /// <summary>
        /// ������
        /// </summary>
        TurningLeft = 1 << 4,

        /// <summary>
        /// �E����
        /// </summary>
        TurningRight = 1 << 5,

        /// <summary>
        /// �U����
        /// </summary>
        Attacking = 1 << 6,

        /// <summary>
        /// �X�g���C�t�i���ړ��j��
        /// </summary>
        Strafing = 1 << 7,

        /// <summary>
        /// �K�[�h�i�u���b�N�j��
        /// </summary>
        Blocking = 1 << 8,

        /// <summary>
        /// ����i�h�b�W�j��
        /// </summary>
        Dodging = 1 << 9,

        /// <summary>
        /// �����i���R�C���j��
        /// </summary>
        Recoiling = 1 << 10,

        /// <summary>
        /// �X�^���i�C��j��
        /// </summary>
        Stunned = 1 << 11,

        /// <summary>
        /// ��e���i�q�b�g�����j
        /// </summary>
        GettingHit = 1 << 12,

        /// <summary>
        /// ����𑕔����i���o�����Ȃǁj
        /// </summary>
        Equipping = 1 << 13,

        /// <summary>
        /// ����̐؂�ւ���
        /// </summary>
        SwitchingWeapons = 1 << 14,

        /// <summary>
        /// �퓬�s�\�i���S�j���
        /// </summary>
        Dead = 1 << 15,

        /// <summary>
        /// �G���[�g�i����\���j��
        /// </summary>
        Emoting = 1 << 16,

        /// <summary>
        /// ���ׂẴt���O���܂ށi�r�b�g���]�ɂ��S�r�b�gON�j
        /// </summary>
        Everything = ~0,
    }
}
