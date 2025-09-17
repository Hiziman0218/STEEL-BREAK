using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelPlay.Utils
{
    public static class ScreenUtility
    {
        /// <summary>
        /// ���[���h���W���X�N���[�����W�ɕϊ�
        /// </summary>
        /// <param name="cam">�J����</param>
        /// <param name="worldPos">�Ώۂ̃��[���h���W</param>
        /// <returns>�X�N���[�����W�ɕϊ���̒l</returns>
        public static Vector3 WorldToScreen(Camera cam, Vector3 worldPos)
        {
            return cam.WorldToScreenPoint(worldPos);
        }

        /// <summary>
        /// �Ώۂ���ʓ��ɂ��邩�擾
        /// </summary>
        /// <param name="screenPos">�Ώۂ̍��W</param>
        /// <returns>��ʓ��ɂ��邩</returns>
        public static bool IsInScreen(Vector3 screenPos)
        {
            return screenPos.z > 0
                && screenPos.x > 0 && screenPos.x < Screen.width
                && screenPos.y > 0 && screenPos.y < Screen.height;
        }
    }
}
