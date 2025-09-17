using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelPlay.Utils
{
    public static class ScreenUtility
    {
        /// <summary>
        /// ワールド座標をスクリーン座標に変換
        /// </summary>
        /// <param name="cam">カメラ</param>
        /// <param name="worldPos">対象のワールド座標</param>
        /// <returns>スクリーン座標に変換後の値</returns>
        public static Vector3 WorldToScreen(Camera cam, Vector3 worldPos)
        {
            return cam.WorldToScreenPoint(worldPos);
        }

        /// <summary>
        /// 対象が画面内にいるか取得
        /// </summary>
        /// <param name="screenPos">対象の座標</param>
        /// <returns>画面内にいるか</returns>
        public static bool IsInScreen(Vector3 screenPos)
        {
            return screenPos.z > 0
                && screenPos.x > 0 && screenPos.x < Screen.width
                && screenPos.y > 0 && screenPos.y < Screen.height;
        }
    }
}
