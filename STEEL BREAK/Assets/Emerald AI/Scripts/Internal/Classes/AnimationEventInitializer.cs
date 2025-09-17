using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// �yAnimationEventInitializer�z
    /// �A�j���[�V�����v���r���[�G�f�B�^�ɁA���炩���ߗp�ӂ��ꂽ�A�j���[�V�����C�x���g�̃v���Z�b�g��o�^���܂��B
    /// </summary>
    public static class AnimationEventInitializer
    {
        /// <summary>
        /// Emerald AI �p�̃A�j���[�V�����C�x���g�ꗗ�i�v���Z�b�g�j�𐶐����ĕԂ��܂��B
        /// </summary>
        public static List<EmeraldAnimationEventsClass> GetEmeraldAnimationEvents()
        {
            List<EmeraldAnimationEventsClass> EmeraldAnimationEvents = new List<EmeraldAnimationEventsClass>();

            // Custom�i�J�X�^���j
            AnimationEvent Custom = new AnimationEvent();
            Custom.functionName = "---YOUR FUNCTION NAME HERE---"; // �Ăяo�������֐������L���i��FOnMyCustomEvent�j
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "�J�X�^��",
                    Custom,
                    "�ǉ��p�����[�^�̂Ȃ��J�X�^���^�f�t�H���g�̃C�x���g�B�C�ӂ̊֐������w�肵�Ďg�p���܂��B"
                )
            );

            // Emerald Attack Event�i�A�r���e�B�����j
            AnimationEvent EmeraldAttack = new AnimationEvent();
            EmeraldAttack.functionName = "CreateAbility"; // �֐����͎d�l�ǂ���ێ�
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "�A�r���e�B����",
                    EmeraldAttack,
                    "AI �́w���݂̃A�r���e�B�x�𐶐�����C�x���g�i����: EmeraldAttackEvent�j�B�A�r���e�B�I�u�W�F�N�g���N�����邽�߂ɕK�v�ŁA���ׂĂ̍U���A�j���[�V�����ɐݒ肵�Ă��������B\n\n" +
                    "���ӁFAI �� Attack Transform �𗘗p���Ă���ꍇ�A���̃C�x���g�� String �p�����[�^�� Attack Transform �����L�����Ă��������B����ɂ��A�r���e�B�͂��� Transform �̈ʒu���琶������܂��B"
                )
            );

            // Charge Ability�i�`���[�W�E�G�t�F�N�g�j
            AnimationEvent ChargeEffect = new AnimationEvent();
            ChargeEffect.functionName = "ChargeEffect"; // �֐����͎d�l�ǂ���ێ�
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "�`���[�W�E�G�t�F�N�g",
                    ChargeEffect,
                    "AI �̌��݃A�r���e�B�ɂ�����w�`���[�W�p�G�t�F�N�g�x���N�����܂��B�ǂ� Attack Transform �Ő������邩�� String �p�����[�^�Ŏw�肵�܂��iCombat �R���|�[�l���g�� Attack Transform ���X�g�Ɋ�Â��j�B" +
                    "�A�r���e�B�I�u�W�F�N�g���Ƀ`���[�W���W���[�������݂��A�L��������Ă���K�v������܂��B�����ȏꍇ�A���̃C�x���g�̓X�L�b�v����܂��B\n\n" +
                    "���ӁF���̃C�x���g�̓A�r���e�B���̂͐������܂���B�w�A�r���e�B�����iCreateAbility�j�x�C�x���g���A�ʏ킱�̌�Ɋ��蓖�ĂĂ��������B���̃A�j���[�V�����C�x���g�͔C�ӂł��B"
                )
            );

            // Fade Out IK�iIK ���t�F�[�h�A�E�g�j
            AnimationEvent FadeOutIK = new AnimationEvent();
            FadeOutIK.functionName = "FadeOutIK"; // �֐����͎d�l�ǂ���ێ�
            FadeOutIK.floatParameter = 5f;
            FadeOutIK.stringParameter = "---�t�F�[�h�����������O����������---";
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "IK ���t�F�[�h�A�E�g",
                    FadeOutIK,
                    "AI �� IK �����Ԍo�߂ŏ��X�ɖ��������܂��B��e�A�����A����̍U���A���S�A�j���[�V�����Ȃǂ� IK ��������ꍇ�ɗL�p�ł��B\n\n" +
                    "FloatParameter = �t�F�[�h�A�E�g���ԁi�b�j\n\n" +
                    "StringParameter = �t�F�[�h�Ώۂ� Rig ��"
                )
            );

            // Fade In IK�iIK ���t�F�[�h�C���j
            AnimationEvent FadeInIK = new AnimationEvent();
            FadeInIK.functionName = "FadeInIK"; // �֐����͎d�l�ǂ���ێ�
            FadeInIK.floatParameter = 5f;
            FadeInIK.stringParameter = "---�t�F�[�h�����������O����������---";
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "IK ���t�F�[�h�C��",
                    FadeInIK,
                    "AI �� IK �����Ԍo�߂ŏ��X�ɗL�������܂��B�wIK ���t�F�[�h�A�E�g�x���g�p������Ɏg�p���Ă��������B\n\n" +
                    "FloatParameter = �t�F�[�h�C�����ԁi�b�j\n\n" +
                    "StringParameter = �t�F�[�h�Ώۂ� Rig ��"
                )
            );

            // Enable Weapon Collider�i����R���C�_�[��L�����j
            AnimationEvent EnableWeaponCollider = new AnimationEvent();
            EnableWeaponCollider.functionName = "EnableWeaponCollider"; // �֐����͎d�l�ǂ���ێ�
            EnableWeaponCollider.stringParameter = "---AI �̕��햼�������ɓ���---";
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "����R���C�_�[��L����",
                    EnableWeaponCollider,
                    "AI �̕���I�u�W�F�N�g�ɕt�^���ꂽ�R���C�_�[��L�������܂��i����ɂ� WeaponCollider �R���|�[�l���g���K�v�ŁAEmeraldItems �R���|�[�l���g�Őݒ�ς݂ł��邱�Ɓj�B\n\n" +
                    "���ӁF���̃A�j���[�V�����C�x���g�� String �p�����[�^�ɁAAI �̕���I�u�W�F�N�g����ݒ肵�Ă��������BItems �R���|�[�l���g������A���̖��̂ŊY��������������܂��B�ڂ����� Emerald AI Wiki ���Q�Ƃ��Ă��������B"
                )
            );

            // Disable Weapon Collider�i����R���C�_�[�𖳌����j
            AnimationEvent DisableWeaponCollider = new AnimationEvent();
            DisableWeaponCollider.functionName = "DisableWeaponCollider"; // �֐����͎d�l�ǂ���ێ�
            DisableWeaponCollider.stringParameter = "---AI �̕��햼�������ɓ���---";
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "����R���C�_�[�𖳌���",
                    DisableWeaponCollider,
                    "AI �̕���I�u�W�F�N�g�ɕt�^���ꂽ�R���C�_�[�𖳌������܂��i����ɂ� WeaponCollider �R���|�[�l���g���K�v�ŁAEmeraldItems �R���|�[�l���g�Őݒ�ς݂ł��邱�Ɓj�B\n\n" +
                    "���ӁF���̃A�j���[�V�����C�x���g�� String �p�����[�^�ɁAAI �̕���I�u�W�F�N�g����ݒ肵�Ă��������BItems �R���|�[�l���g������A���̖��̂ŊY��������������܂��B�ڂ����� Emerald AI Wiki ���Q�Ƃ��Ă��������B"
                )
            );

            // Equip Weapon 1�i����^�C�v1�𑕔��j
            AnimationEvent EquipWeapon1 = new AnimationEvent();
            EquipWeapon1.functionName = "EquipWeapon"; // �֐����͎d�l�ǂ���ێ�
            EquipWeapon1.stringParameter = "Weapon Type 1"; // �d�l��̎��ʎq�̉\�������邽�ߌ������ێ�
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "����^�C�v1�𑕔�",
                    EquipWeapon1,
                    "AI �́wWeapon Type 1�x�𑕔����܂��i����� Emerald AI ��ŃZ�b�g�A�b�v����Ă���K�v������܂��j�B"
                )
            );

            // Equip Weapon 2�i����^�C�v2�𑕔��j
            AnimationEvent EquipWeapon2 = new AnimationEvent();
            EquipWeapon2.functionName = "EquipWeapon"; // �֐����͎d�l�ǂ���ێ�
            EquipWeapon2.stringParameter = "Weapon Type 2"; // �d�l��̎��ʎq�̉\�������邽�ߌ������ێ�
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "����^�C�v2�𑕔�",
                    EquipWeapon2,
                    "AI �́wWeapon Type 2�x�𑕔����܂��i����� Emerald AI ��ŃZ�b�g�A�b�v����Ă���K�v������܂��j�B"
                )
            );

            // Unequip Weapon 1�i����^�C�v1���O���j
            AnimationEvent UnequipWeapon1 = new AnimationEvent();
            UnequipWeapon1.functionName = "UnequipWeapon"; // �֐����͎d�l�ǂ���ێ�
            UnequipWeapon1.stringParameter = "Weapon Type 1"; // �d�l��̎��ʎq�̉\�������邽�ߌ������ێ�
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "����^�C�v1���O��",
                    UnequipWeapon1,
                    "AI �́wWeapon Type 1�x���O���܂��i����� Emerald AI ��ŃZ�b�g�A�b�v����Ă���K�v������܂��j�B"
                )
            );

            // Unequip Weapon 2�i����^�C�v2���O���j
            AnimationEvent UnequipWeapon2 = new AnimationEvent();
            UnequipWeapon2.functionName = "UnequipWeapon"; // �֐����͎d�l�ǂ���ێ�
            UnequipWeapon2.stringParameter = "Weapon Type 2"; // �d�l��̎��ʎq�̉\�������邽�ߌ������ێ�
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "����^�C�v2���O��",
                    UnequipWeapon2,
                    "AI �́wWeapon Type 2�x���O���܂��i����� Emerald AI ��ŃZ�b�g�A�b�v����Ă���K�v������܂��j�B"
                )
            );

            // Enable Item�i�A�C�e����L�����j
            AnimationEvent EnableItem = new AnimationEvent();
            EnableItem.functionName = "EnableItem"; // �֐����͎d�l�ǂ���ێ�
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "�A�C�e����L����",
                    EnableItem,
                    "ItemID ���w�肵�ăA�C�e����L�������܂��BAI �� Item List �Ɋ�Â��AAI �ɂ� EmeraldAIItem �R���|�[�l���g���K�v�ł��B\n\nIntParameter = ItemID"
                )
            );

            // Disable Item�i�A�C�e���𖳌����j
            AnimationEvent DisableItem = new AnimationEvent();
            DisableItem.functionName = "DisableItem"; // �֐����͎d�l�ǂ���ێ�
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "�A�C�e���𖳌���",
                    DisableItem,
                    "ItemID ���w�肵�ăA�C�e���𖳌������܂��BAI �� Item List �Ɋ�Â��AAI �ɂ� EmeraldAIItem �R���|�[�l���g���K�v�ł��B\n\nIntParameter = ItemID"
                )
            );

            // Footstep Sound�i�t�b�g�X�e�b�v�j
            AnimationEvent Footstep = new AnimationEvent();
            Footstep.functionName = "Footstep"; // �֐����͎d�l�ǂ���ێ�
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "�t�b�g�X�e�b�v",
                    Footstep,
                    "Footstep �R���|�[�l���g���g�p���Ă���ꍇ�F\n���o���ꂽ�ڒn�ʂɉ����āA�����G�t�F�N�g�ƃT�E���h�𐶐����܂��i���O�� Footstep �R���|�[�l���g�̃Z�b�g�A�b�v���K�v�j�B\n\n" +
                    "Footstep �R���|�[�l���g���g�p���Ă��Ȃ��ꍇ�F\nAI �� Walk Sound List �Ɋ�Â��A�����_���ȑ������Đ����܂��B"
                )
            );

            // Play Attack Sound�i�U�������Đ��j
            AnimationEvent PlayAttackSound = new AnimationEvent();
            PlayAttackSound.functionName = "PlayAttackSound"; // �֐����͎d�l�ǂ���ێ�
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "�U�������Đ�",
                    PlayAttackSound,
                    "AI �� Attack Sound List �Ɋ�Â��A�����_���ȍU���T�E���h���Đ����܂��B"
                )
            );

            // Play Sound Effect�i���ʉ����Đ��j
            AnimationEvent PlaySoundEffect = new AnimationEvent();
            PlaySoundEffect.functionName = "PlaySoundEffect"; // �֐����͎d�l�ǂ���ێ�
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "���ʉ����Đ�",
                    PlaySoundEffect,
                    "AI �� Sounds List ����A�w�肵�� SoundEffectID �̃T�E���h���Đ����܂��B\n\nIntParameter = SoundEffectID"
                )
            );

            // Play Warning Sound�i�x�������Đ��j
            AnimationEvent PlayWarningSound = new AnimationEvent();
            PlayWarningSound.functionName = "PlayWarningSound"; // �֐����͎d�l�ǂ���ێ�
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "�x�������Đ�",
                    PlayWarningSound,
                    "AI �� Warning Sound List �Ɋ�Â��A�����_���Ȍx���T�E���h���Đ����܂��B"
                )
            );

            return EmeraldAnimationEvents;
        }
    }
}
