using System.Collections;                         // （保持）コルーチン関連
using System.Collections.Generic;                 // （保持）汎用コレクション
using UnityEngine;                                // Unity ランタイムAPI
using UnityEditor;                                // エディタ拡張API（Editor など）
using UnityEditorInternal;                        // ReorderableList 等（本ファイルでは未使用だが原文保持）

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldWeaponCollision))] // このカスタムインスペクタは EmeraldWeaponCollision 用
    [CanEditMultipleObjects]                       // 複数オブジェクト同時編集を許可

    // 【クラス概要】EmeraldWeaponCollisionEditor：
    //  EmeraldWeaponCollision コンポーネント（近接武器用の当たり判定ボックス）を
    //  インスペクタ上で編集しやすくするためのエディタ拡張クラス。
    //  ボックスコライダーの可視色、説明テキスト、折りたたみUIなどを提供する。
    public class EmeraldWeaponCollisionEditor : Editor
    {
        [Header("フォールドアウト見出しのスタイル（EditorGUI 用）")]
        GUIStyle FoldoutStyle;                     // 見出しの描画スタイル

        [Header("ヘッダーに表示するアイコン（Resources から読み込み）")]
        Texture WeaponCollisionEditorIcon;         // インスペクタ上部のアイコン

        [Header("SerializedProperty 参照（色/折りたたみ/非表示トグル）")]
        SerializedProperty CollisionBoxColor,      // コリジョンボックスの色
                          HideSettingsFoldout,     // 設定全体の非表示トグル
                          WeaponCollisionFoldout;  // 「武器コリジョン設定」セクションの開閉

        /// <summary>
        /// （日本語）エディタ有効化時に呼ばれる。アイコン読み込みと対象プロパティのバインド、武器の BoxCollider の自動取得を行う。
        /// </summary>
        void OnEnable()
        {
            // ヘッダー用アイコンを読み込み（パスは既存仕様のまま）
            if (WeaponCollisionEditorIcon == null) WeaponCollisionEditorIcon = Resources.Load("Editor Icons/EmeraldWeaponCollision") as Texture;

            // 対象コンポーネントへの参照を取得
            EmeraldWeaponCollision self = (EmeraldWeaponCollision)target;

            // 武器の BoxCollider を自動で取得して保持（元実装の挙動のまま）
            self.WeaponCollider = self.GetComponent<BoxCollider>();

            // シリアライズ済みフィールドをプロパティへバインド
            CollisionBoxColor = serializedObject.FindProperty("CollisionBoxColor");   // コリジョンボックスの色
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout"); // 非表示トグル
            WeaponCollisionFoldout = serializedObject.FindProperty("WeaponCollisionFoldout"); // セクション折りたたみ
        }

        /// <summary>
        /// （日本語）インスペクタのメイン描画。ヘッダーと「武器コリジョン設定」セクションを表示する。
        /// </summary>
        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles(); // カスタムスタイル更新
            serializedObject.Update();                                  // 直列化オブジェクトを最新化

            // ヘッダー（英語 "Weapon Collision" → 日本語「武器コリジョン」へ差し替え）
            CustomEditorProperties.BeginScriptHeaderNew("武器コリジョン", WeaponCollisionEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue) // 非表示でなければ内容を描画
            {
                EditorGUILayout.Space();
                WeaponCollisionSettings();      // 設定セクションの描画
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();   // ヘッダー終了

            serializedObject.ApplyModifiedProperties(); // 変更適用
        }

        /// <summary>
        /// （日本語）武器コリジョンの設定UIを描画する。説明文・色設定・ヘルプを日本語で表示。
        /// </summary>
        void WeaponCollisionSettings()
        {
            // セクション見出し（英語 "Weapon Collision Settings" → 日本語「武器コリジョン設定」）
            WeaponCollisionFoldout.boolValue = EditorGUILayout.Foldout(WeaponCollisionFoldout.boolValue, "武器コリジョン設定", true, FoldoutStyle);

            if (WeaponCollisionFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // タイトル＋説明（英語→日本語）
                CustomEditorProperties.TextTitleWithDescription(
                    "武器コリジョン設定",
                    "Box Collider（BoxCollider コンポーネント）を用いて、AI の武器に合わせてコライダーのサイズと位置を調整してください。"
                    + "武器コリジョンは近接（メレー）攻撃用を想定しています。"
                    + "この Weapon Collision コンポーネントを機能させるには、アニメーションイベントを通して有効化する必要があります。",
                    true
                );

                // 色プロパティ（英語ラベル → 日本語ラベルへ置換）
                EditorGUILayout.PropertyField(CollisionBoxColor, new GUIContent("コリジョンボックスの色"));
                CustomEditorProperties.CustomHelpLabelField("コリジョンボックスの可視化に使用する色を制御します。", true);

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}
