namespace EmeraldAI.SoundDetection
{
    // �y�񋓑̂̊T�v�zAttractModifierReactionTypes�F
    //  AttractModifier�i�U���g���K�j�ɑ΂��āAAI ���ǂ̎�ނ̔��������邩���`����񋓑́B
    //  �l�i0, 25, 50�j�̓G�f�B�^��̕��сE�d�ݕt�������Ȃǂɗp������z��ŁA�Q�[�������̋�ʎq�ł��B
    public enum AttractModifierReactionTypes
    {
        LookAtAttractSource = 0,        // �U�����i�����Ȃǁj�̕������u����v
        MoveAroundAttractSource = 25,   // �U�����́u���͂�����v����i���a�E�|�C���g���̓��A�N�V�������̐ݒ�Ɉˑ��j
        MoveToAttractSource = 50,       // �U�����ցu�ړ��v����i���B�ҋ@�Ȃǂ̋����̓��A�N�V�������̐ݒ�Ɉˑ��j
    }
}
