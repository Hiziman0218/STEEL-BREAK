using UnityEngine;                     // Unity �����^�C�� API
using UnityEditor;                     // �G�f�B�^�g���p API�iEditor �Ȃǁj
using UnityEditorInternal;             // InternalEditorUtility.layers ���g�p���邽��

namespace EmeraldAI.Utility
{
    // �y�N���X�T�v�zLayerMaskDrawer�F
    //  Unity �� LayerMask �ƁA�G�f�B�^ UI�i���C���[�z��Ɋ�Â��r�b�g�t���O�j�Ԃ�
    //  ���ݕϊ����[�e�B���e�B�iEditor ��p�j�B���C���[�̕��я��ɉ����ăr�b�g�����蓖�Ē����܂��B
    public class LayerMaskDrawer : Editor
    {
        /// <summary>
        /// �i���{��jLayerMask ���u�G�f�B�^�̃��C���[�I���t�B�[���h�l�v�ɕϊ����܂��B
        /// InternalEditorUtility.layers �̏����ɍ��킹�ăr�b�g����ёւ��܂��B
        /// </summary>
        public static int LayerMaskToField(LayerMask mask)
        {
            int field = 0;                                                // �ԋp�p�̃t�B�[���h�l�i�r�b�g�t���O�j
            var layers = InternalEditorUtility.layers;                    // �v���W�F�N�g���̗L�����C���[���ꗗ�i���я����j
            for (int c = 0; c < layers.Length; c++)                       // �ꗗ�̏����i0..n�j�ő���
            {
                // mask �ɁA�����C���[�ԍ��iNameToLayer�j�ɑΉ�����r�b�g�������Ă��邩�H
                if ((mask & (1 << LayerMask.NameToLayer(layers[c]))) != 0)
                {
                    field |= 1 << c;                                      // �����Ă���΁u�z�� c �Ԗځv�ɑΉ�����r�b�g�𗧂Ă�
                }
            }
            return field;                                                 // �t�B�[���h�l��Ԃ��i�G�f�B�^ UI �p�j
        }

        /// <summary>
        /// �i���{��j�G�f�B�^�̃��C���[�I���t�B�[���h�l�� LayerMask �ɕϊ����܂��B
        /// InternalEditorUtility.layers �̏�����������C���[�ԍ��փ}�b�s���O���܂��B
        /// </summary>
        public static LayerMask FieldToLayerMask(int field)
        {
            LayerMask mask = 0;                                           // �ԋp�p�� LayerMask
            var layers = InternalEditorUtility.layers;                    // �v���W�F�N�g���̗L�����C���[���ꗗ
            for (int c = 0; c < layers.Length; c++)                       // �ꗗ�̏����i0..n�j�ő���
            {
                if ((field & (1 << c)) != 0)                              // �u�z�� c �Ԗځv�̃r�b�g�������Ă���Ȃ�
                {
                    mask |= 1 << LayerMask.NameToLayer(layers[c]);        // �����C���[�ԍ��iNameToLayer�j�֕ϊ����ăr�b�g�𗧂Ă�
                }
            }
            return mask;                                                  // LayerMask ��Ԃ��i���s���Ŏg�p�j
        }
    }
}
