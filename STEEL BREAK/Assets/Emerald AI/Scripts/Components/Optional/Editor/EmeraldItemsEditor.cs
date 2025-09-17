using System.Collections;                         // （保持）コルーチン関連
using System.Collections.Generic;                 // （保持）汎用コレクション
using UnityEngine;                                // Unity ランタイムAPI
using UnityEditor;                                // エディタ拡張API
using UnityEditorInternal;                        // ReorderableList 等

namespace EmeraldAI.Utility                       // EmeraldAI のユーティリティ名前空間
{
    [CustomEditor(typeof(EmeraldItems))]          // このカスタムインスペクタは EmeraldItems 用
    [CanEditMultipleObjects]                      // 複数選択編集を許可

    // 【クラス概要】EmeraldItemsEditor：
    //  EmeraldItems コンポーネントのインスペクタを拡張し、
    //  「武器の装備/収納/ドロップ」設定と「任意アイテムの有効/無効切替（ID指定）」を
    //  ReorderableList を用いて編集しやすくするエディタクラス。
    public class EmeraldItemsEditor : Editor
    {
        [Header("折りたたみ見出しのスタイル（EditorGUI 用）")]
        GUIStyle FoldoutStyle;                     // フォールドアウトの見た目

        [Header("EmeraldCombat 参照（武器タイプ数の判定に使用）")]
        EmeraldCombat EmeraldCombat;               // 対象AIの EmeraldCombat コンポーネント

        [Header("ヘッダーに表示するアイコン（Resources から読込）")]
        Texture ItemsEditorIcon;                   // インスペクタ上部のアイコン

        #region SerializedProperties
        //Bools
        [Header("エディタ表示制御フラグ（SerializedProperty）")]
        SerializedProperty HideSettingsFoldout,    // 設定全体の非表示トグル
                          WeaponsFoldout,          // 「武器設定」セクションの折りたたみ
                          ItemsFoldout;            // 「アイテム設定」セクションの折りたたみ

        [Header("ReorderableList 参照（タイプ別装備武器 / アイテム一覧）")]
        ReorderableList ItemList,                  // アイテム一覧（ID+Object）
                       Type1EquippableWeaponsList, // 武器タイプ1の装備設定リスト
                       Type2EquippableWeaponsList; // 武器タイプ2の装備設定リスト
        #endregion

        void OnEnable()                            // エディタ有効化時
        {
            EmeraldItems self = (EmeraldItems)target;                                      // 対象コンポーネント
            EmeraldCombat = self.GetComponent<EmeraldCombat>();                            // 参照を取得
            if (ItemsEditorIcon == null) ItemsEditorIcon = Resources.Load("Editor Icons/EmeraldItems") as Texture; // ヘッダー用アイコン

            InitializeProperties();                                                       // SerializedProperty を紐付け
            InitializeLists();                                                            // ReorderableList を初期化
        }

        void InitializeProperties()                  // 対象フィールドの SerializedProperty を取得
        {
            //Bools
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");   // 非表示
            WeaponsFoldout = serializedObject.FindProperty("WeaponsFoldout");        // 武器設定の折りたたみ
            ItemsFoldout = serializedObject.FindProperty("ItemsFoldout");          // アイテム設定の折りたたみ
        }

        void InitializeLists()                      // ReorderableList を作成・設定
        {
            // Type 1（表示名：日本語へ置換）
            Type1EquippableWeaponsList = DrawEquippableWeaponsList(
                Type1EquippableWeaponsList,
                "Type1EquippableWeapons",
                "タイプ1 装備可能武器"
            );

            // Type 2（表示名：日本語へ置換）
            Type2EquippableWeaponsList = DrawEquippableWeaponsList(
                Type2EquippableWeaponsList,
                "Type2EquippableWeapons",
                "タイプ2 装備可能武器"
            );

            // Item Objects（ヘッダー表示を日本語に）
            ItemList = new ReorderableList(serializedObject, serializedObject.FindProperty("ItemList"), true, true, true, true);
            ItemList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    EditorGUI.BeginChangeCheck();
                    var element = ItemList.serializedProperty.GetArrayElementAtIndex(index);
                    // 右側に ItemObject、左側に ID を配置（レイアウトは元のまま）
                    EditorGUI.PropertyField(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("ItemObject"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("ItemID"), GUIContent.none);
                };

            ItemList.drawHeaderCallback = rect =>
            {
                // "   ID          Item Object" → 日本語へ
                EditorGUI.LabelField(rect, "   ID  " + "         アイテムオブジェクト", EditorStyles.boldLabel);
            };
        }

        ReorderableList DrawEquippableWeaponsList(ReorderableList WeaponsList, string WeaponsPropertyName, string WeaponsDisplayName)
        {
            WeaponsList = new ReorderableList(serializedObject, serializedObject.FindProperty(WeaponsPropertyName), true, true, true, true);
            WeaponsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    EditorGUI.BeginChangeCheck();
                    var element = WeaponsList.serializedProperty.GetArrayElementAtIndex(index);

                    // "Item n" ラベル → 日本語「アイテム n」
                    EditorGUI.LabelField(new Rect(rect.x + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "アイテム " + (index + 1).ToString());

                    float Height = (EditorGUIUtility.singleLineHeight * 1.25f);

                    // "Held" とその説明 → 日本語
                    EditorGUI.LabelField(
                        new Rect(rect.x + 10, rect.y + Height, rect.width, EditorGUIUtility.singleLineHeight),
                        new GUIContent(
                            "手持ち（Held）",
                            "手持ちオブジェクト。ドロップ用武器のスポーン位置・回転の参照、および装備時の見た目位置として使用します。"
                        )
                    ); // タイトル
                    EditorGUI.PropertyField(
                        new Rect(rect.x + 100, rect.y + Height, rect.width - 100, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("HeldObject"),
                        GUIContent.none
                    ); // オブジェクト

                    // "Holstered" チェック & 説明 → 日本語
                    EditorGUI.PropertyField(
                        new Rect(rect.x - 12.5f, rect.y + Height * 2, rect.width, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("HolsteredToggle"),
                        GUIContent.none
                    ); // トグル
                    EditorGUI.LabelField(
                        new Rect(rect.x + 10, rect.y + Height * 2, rect.width, EditorGUIUtility.singleLineHeight),
                        new GUIContent(
                            "ホルスター（収納）",
                            "AI の腰や背中などに収納されている武器オブジェクト。装備アニメーションを使用している場合に利用されます。\n\n注意：アニメーションプロファイルに装備/収納アニメがない場合、この設定は無視されます。"
                        )
                    ); // タイトル
                    EditorGUI.BeginDisabledGroup(!element.FindPropertyRelative("HolsteredToggle").boolValue);
                    EditorGUI.PropertyField(
                        new Rect(rect.x + 100, rect.y + Height * 2, rect.width - 100, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("HolsteredObject"),
                        GUIContent.none
                    ); // オブジェクト
                    EditorGUI.EndDisabledGroup();

                    // "Droppable" チェック & 説明 → 日本語
                    EditorGUI.PropertyField(
                        new Rect(rect.x - 12.5f, rect.y + Height * 3, rect.width, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("DroppableToggle"),
                        GUIContent.none
                    ); // トグル
                    EditorGUI.LabelField(
                        new Rect(rect.x + 10, rect.y + Height * 3, rect.width, EditorGUIUtility.singleLineHeight),
                        new GUIContent(
                            "ドロップ可能（Droppable）",
                            "有効にすると、このアイテムの『手持ち（Held）』の位置・回転に合わせて、アタッチされたオブジェクトのコピーをスポーンします。"
                        )
                    ); // タイトル
                    EditorGUI.BeginDisabledGroup(!element.FindPropertyRelative("DroppableToggle").boolValue);
                    EditorGUI.PropertyField(
                        new Rect(rect.x + 100, rect.y + Height * 3, rect.width - 100, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("DroppableObject"),
                        GUIContent.none
                    ); // オブジェクト
                    EditorGUI.EndDisabledGroup();
                };

            WeaponsList.elementHeightCallback = (int index) =>
            {
                SerializedProperty element = WeaponsList.serializedProperty.GetArrayElementAtIndex(index);
                return EditorGUIUtility.singleLineHeight * 5.0f; // 行高は元のまま
            };

            WeaponsList.drawHeaderCallback = rect =>
            {
                // ヘッダー表示名を日本語へ
                EditorGUI.LabelField(rect, WeaponsDisplayName, EditorStyles.boldLabel);
            };

            return WeaponsList;
        }


        public override void OnInspectorGUI()        // インスペクタのメイン描画
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();                     // カスタムスタイル更新
            EmeraldItems self = (EmeraldItems)target;                                       // 対象
            serializedObject.Update();                                                      // 直列化同期

            // ヘッダー "Items" → 日本語「アイテム」
            CustomEditorProperties.BeginScriptHeaderNew("アイテム", ItemsEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)                                            // 非表示でなければ描画
            {
                EditorGUILayout.Space();
                WeaponSettings(self);                                                      // 武器設定
                EditorGUILayout.Space();
                ItemSettings(self);                                                        // アイテム設定
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();                                     // 変更適用
            CustomEditorProperties.EndScriptHeader();                                       // ヘッダー終了
        }

        void WeaponSettings(EmeraldItems self)         // 「武器設定」セクション
        {
            // "Weapon Settings" → 日本語「武器設定」
            WeaponsFoldout.boolValue = EditorGUILayout.Foldout(WeaponsFoldout.boolValue, "武器設定", true, FoldoutStyle);

            if (WeaponsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // タイトル＆説明（英語→日本語）
                CustomEditorProperties.TextTitleWithDescription(
                    "武器設定",
                    "装備/収納アニメーションを使用して、アニメーションイベント経由で武器オブジェクトの表示/非表示を切り替えられるようにします（AI に装備/収納アニメが必要）。",
                    true
                );

                // タイプ1の武器リスト
                Type1EquippableWeaponsList.DoLayoutList();

                // 武器タイプが「Two」のときのみタイプ2リストを表示（元挙動のまま）
                if (EmeraldCombat.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two)
                {
                    Type2EquippableWeaponsList.DoLayoutList();
                }

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void ItemSettings(EmeraldItems self)           // 「アイテム設定」セクション
        {
            // "Item Settings" → 日本語「アイテム設定」
            ItemsFoldout.boolValue = EditorGUILayout.Foldout(ItemsFoldout.boolValue, "アイテム設定", true, FoldoutStyle);

            if (ItemsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // タイトル＆説明（英語→日本語）
                CustomEditorProperties.TextTitleWithDescription(
                    "アイテム設定",
                    "AI に紐づくオブジェクトを、アニメーションイベントまたはスクリプト（アイテムID指定）で有効/無効にできます。クエスト用アイテム、演出用オブジェクト、" +
                    "特定アニメーション専用のオブジェクトなどに便利です。詳細はドキュメントを参照してください。",
                    true
                );

                // ヘルプ（英語→日本語）
                CustomEditorProperties.CustomHelpLabelField(
                    "下の各アイテムには固有の ID があります。この ID を用いて、Emerald AI の API から該当アイテムを検索し、有効化または無効化できます。",
                    true
                );

                // アイテム一覧（ID + オブジェクト）
                ItemList.DoLayoutList();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}
