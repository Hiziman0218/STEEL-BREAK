using UnityEngine;                     // Unity ランタイム API
using UnityEditor;                     // エディタ拡張用 API（Editor など）
using UnityEditorInternal;             // InternalEditorUtility.layers を使用するため

namespace EmeraldAI.Utility
{
    // 【クラス概要】LayerMaskDrawer：
    //  Unity の LayerMask と、エディタ UI（レイヤー配列に基づくビットフラグ）間の
    //  相互変換ユーティリティ（Editor 専用）。レイヤーの並び順に応じてビットを割り当て直します。
    public class LayerMaskDrawer : Editor
    {
        /// <summary>
        /// （日本語）LayerMask を「エディタのレイヤー選択フィールド値」に変換します。
        /// InternalEditorUtility.layers の順序に合わせてビットを並び替えます。
        /// </summary>
        public static int LayerMaskToField(LayerMask mask)
        {
            int field = 0;                                                // 返却用のフィールド値（ビットフラグ）
            var layers = InternalEditorUtility.layers;                    // プロジェクト内の有効レイヤー名一覧（並び順つき）
            for (int c = 0; c < layers.Length; c++)                       // 一覧の順序（0..n）で走査
            {
                // mask に、実レイヤー番号（NameToLayer）に対応するビットが立っているか？
                if ((mask & (1 << LayerMask.NameToLayer(layers[c]))) != 0)
                {
                    field |= 1 << c;                                      // 立っていれば「配列順 c 番目」に対応するビットを立てる
                }
            }
            return field;                                                 // フィールド値を返す（エディタ UI 用）
        }

        /// <summary>
        /// （日本語）エディタのレイヤー選択フィールド値を LayerMask に変換します。
        /// InternalEditorUtility.layers の順序から実レイヤー番号へマッピングします。
        /// </summary>
        public static LayerMask FieldToLayerMask(int field)
        {
            LayerMask mask = 0;                                           // 返却用の LayerMask
            var layers = InternalEditorUtility.layers;                    // プロジェクト内の有効レイヤー名一覧
            for (int c = 0; c < layers.Length; c++)                       // 一覧の順序（0..n）で走査
            {
                if ((field & (1 << c)) != 0)                              // 「配列順 c 番目」のビットが立っているなら
                {
                    mask |= 1 << LayerMask.NameToLayer(layers[c]);        // 実レイヤー番号（NameToLayer）へ変換してビットを立てる
                }
            }
            return mask;                                                  // LayerMask を返す（実行側で使用）
        }
    }
}
