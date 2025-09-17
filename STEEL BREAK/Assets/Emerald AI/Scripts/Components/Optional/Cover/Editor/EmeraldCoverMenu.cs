using UnityEditor;                         // Unity �G�f�B�^�g��API�i���j���[�E�V�[���r���[�EUndo ���j
using UnityEngine;                         // Unity �̊�{API�iGameObject, Transform, Camera, Vector3 �Ȃǁj

namespace EmeraldAI.Utility                // EmeraldAI �̃��[�e�B���e�B���O���
{
    /// <summary>
    /// �i���{��j�J�o�[�m�[�h�iCover Node�j���A���j���[����̍쐬����уq�G�����L�[��ł̉E�N���b�N�쐬�ɂ���Đ����ł���悤�ɂ���G�f�B�^�g���ł��B
    /// </summary>
    // �y�N���X�T�v�zEmeraldCoverMenu�F
    //  �G�f�B�^�́uGameObject > Emerald AI > Create Cover Node�v���j���[��ǉ����A
    //  ���s���ɁuEmerald Cover Node�v�Ƃ������O�� GameObject �𐶐����� CoverNode �R���|�[�l���g��t�^���܂��B
    //  ���j���[���s���̃R���e�L�X�g�iHierarchy �őI�𒆂̃I�u�W�F�N�g�j������΁A�����e�ɂ��Č��_�֔z�u���܂��B
    //  �R���e�L�X�g���Ȃ��ꍇ�́A�V�[���r���[�̃J�����O���i5m�j�ɐ����E�z�u���܂��B
    public static class EmeraldCoverMenu      // static�F�C���X�^���X�s�v�̃��[�e�B���e�B�^�i���j���[�R�}���h�̂ݒ񋟁j
    {
        [MenuItem("GameObject/Emerald AI/Create Cover Node", false, 1)]  // �G�f�B�^���j���[�ɍ��ڂ�ǉ��i�p�X / ���؂͂��Ȃ� / ���я��j
        private static void CreateCustomObject(MenuCommand menuCommand)   // ���j���[����Ă΂�鐶�������iEditorOnly�j
        {
            GameObject coverNode = new GameObject("Emerald Cover Node");  // �V�K GameObject �𐶐��i���O�͉p���̂܂܁F�����d�l�ɏ����j
            coverNode.AddComponent<CoverNode>();                          // CoverNode �R���|�[�l���g��t�^

            GameObject context = menuCommand.context as GameObject;       // ���j���[���s���̃R���e�L�X�g�iHierarchy �I���I�u�W�F�N�g�j���擾
            if (context != null)                                          // �e�ɂł���Ώۂ�����ꍇ
            {
                coverNode.transform.SetParent(context.transform);          // �e�q�t���i�K�w�ɒǉ��j
                coverNode.transform.localPosition = Vector3.zero;         // �e�̌��_�ɔz�u�i���[�J�����W 0,0,0�j
            }
            else                                                          // �e�ɂł���Ώۂ��Ȃ��ꍇ
            {
                Camera sceneCamera = SceneView.lastActiveSceneView.camera; // �V�[���r���[�̍Ō�ɃA�N�e�B�u�������J�������擾
                Vector3 spawnPosition = sceneCamera.transform.position +   // �J�����̈ʒu����O���� 5m �̈ʒu���Z�o
                                         sceneCamera.transform.forward * 5f;
                coverNode.transform.position = spawnPosition;              // ���[���h���W�Ŕz�u
            }

            Undo.RegisterCreatedObjectUndo(coverNode, "Create Cover Node"); // Undo �ɑΉ��i���ɖ߂��ō쐬����������j
            Selection.activeObject = coverNode;                             // ���������m�[�h��I����Ԃɂ���i�����ɕҏW�ł���j
        }
    }
}
