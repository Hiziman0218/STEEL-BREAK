using System.Collections;                             // （保持）コルーチン関連
using System.Collections.Generic;                     // （保持）汎用コレクション
using UnityEngine;                                    // Unity ランタイムAPI
using UnityEditor;                                    // エディタ拡張API
using UnityEditorInternal;                            // ReorderableList 等
using System.Linq;                                    // （保持）LINQ
using UnityEngine.Animations.Rigging;                 // RigBuilder / Rig / RigLayer

namespace EmeraldAI.Utility                           // EmeraldAI のユーティリティ名前空間
{
    [CustomEditor(typeof(EmeraldInverseKinematics))]  // このカスタムインスペクタは EmeraldInverseKinematics 用
    [CanEditMultipleObjects]                          // 複数オブジェクト同時編集を許可

    // 【クラス概要】EmeraldInverseKinematicsEditor：
    //  EmeraldInverseKinematics コンポーネントのインスペクタを拡張し、
    //  リグ設定（上半身リグの一覧や新規リグ作成）と、徘徊/戦闘時の注視IKパラメータ（角度/速度/距離/高さオフセット）
    //  を日本語UIで編集できるようにするエディタクラス。
    public class EmeraldInverseKinematicsEditor : Editor
    {
        [Header("折りたたみ見出しのスタイル（EditorGUI 用）")]
        GUIStyle FoldoutStyle;                         // 見出しの描画スタイル

        [Header("ヘッダーに表示するアイコン（Resources から読込）")]
        Texture InverseKinematicsEditorIcon;           // インスペクタ上部のアイコン

        [Header("徘徊/戦闘時の注視IKパラメータ（角度/速度/距離/高さオフセット）")]
        SerializedProperty WanderingLookAtLimit,       // 徘徊時：注視角度の上限
                         WanderingLookSpeed,           // 徘徊時：注視の回頭速度
                         WanderingLookDistance,        // 徘徊時：注視対象までの距離
                         WanderingLookHeightOffset,    // 徘徊時：注視の高さオフセット
                         CombatLookAtLimit,            // 戦闘時：注視角度の上限
                         CombatLookSpeed,              // 戦闘時：注視の回頭速度
                         CombatLookDistance,           // 戦闘時：注視対象までの距離
                         CombatLookHeightOffset;       // 戦闘時：注視の高さオフセット

        [Header("エディタ表示制御フラグ（折りたたみ/非表示）")]
        SerializedProperty HideSettingsFoldout,        // 全体の非表示トグル
                         GeneralIKSettingsFoldout,     // 「一般IK設定」セクションの開閉
                         RigSettingsFoldout;           // 「リグ設定」セクションの開閉

        [Header("上半身リグのリスト（ReorderableList で編集）")]
        ReorderableList UpperBodyRigsList;             // 上半身用の Rig を並べ替え・追加・削除

        private void OnEnable()                        // エディタ有効化時（アイコン読込・プロパティ初期化・リスト初期化）
        {
            if (InverseKinematicsEditorIcon == null)
                InverseKinematicsEditorIcon = Resources.Load("Editor Icons/EmeraldInverseKinematics") as Texture; // ヘッダー用アイコンをロード
            InitializeProperties();                    // 対象プロパティへバインド
            InitializeLists();                         // ReorderableList の初期化
        }

        void InitializeProperties()                  // シリアライズ済みフィールドを探してプロパティに紐付け
        {
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");          // 非表示
            GeneralIKSettingsFoldout = serializedObject.FindProperty("GeneralIKSettingsFoldout"); // 一般IK
            RigSettingsFoldout = serializedObject.FindProperty("RigSettingsFoldout");             // リグ設定

            WanderingLookAtLimit = serializedObject.FindProperty("WanderingLookAtLimit");         // 徘徊：角度
            WanderingLookSpeed = serializedObject.FindProperty("WanderingLookSpeed");             // 徘徊：速度
            WanderingLookDistance = serializedObject.FindProperty("WanderingLookDistance");       // 徘徊：距離
            WanderingLookHeightOffset = serializedObject.FindProperty("WanderingLookHeightOffset");// 徘徊：高さ
            CombatLookAtLimit = serializedObject.FindProperty("CombatLookAtLimit");               // 戦闘：角度
            CombatLookSpeed = serializedObject.FindProperty("CombatLookSpeed");                   // 戦闘：速度
            CombatLookDistance = serializedObject.FindProperty("CombatLookDistance");             // 戦闘：距離
            CombatLookHeightOffset = serializedObject.FindProperty("CombatLookHeightOffset");     // 戦闘：高さ
        }

        void InitializeLists()                        // ReorderableList の構築（上半身リグ）
        {
            UpperBodyRigsList = new ReorderableList(serializedObject, serializedObject.FindProperty("UpperBodyRigsList"), true, true, true, true);
            UpperBodyRigsList.drawHeaderCallback = rect =>
            {
                // 英語 "Upper Body Rigs List" → 日本語「上半身リグのリスト」
                EditorGUI.LabelField(rect, "上半身リグのリスト", EditorStyles.boldLabel);
            };
            UpperBodyRigsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = UpperBodyRigsList.serializedProperty.GetArrayElementAtIndex(index);
                    // ObjectField として Rig 参照を表示
                    EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };
        }

        public override void OnInspectorGUI()         // インスペクタのメイン描画
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles(); // カスタムスタイル更新
            EmeraldInverseKinematics self = (EmeraldInverseKinematics)target; // 対象コンポーネント
            serializedObject.Update();                                   // 直列化オブジェクト同期

            // ヘッダー：英語 "Inverse Kinematics" → 日本語「インバースキネマティクス」
            CustomEditorProperties.BeginScriptHeaderNew("インバースキネマティクス", InverseKinematicsEditorIcon, new GUIContent(), HideSettingsFoldout);

            MissingRigMessage(self);                                     // リグ未設定の警告表示

            if (!HideSettingsFoldout.boolValue)                          // 非表示でなければ内容を描画
            {
                EditorGUILayout.Space();
                RigSettings(self);                                       // リグ設定
                EditorGUILayout.Space();
                GeneralIKSettings(self);                                 // 一般IK設定
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();                    // ヘッダー終了

            serializedObject.ApplyModifiedProperties();                  // 変更を適用
        }

        /// <summary>
        /// （日本語）EmeraldInverseKinematicsEditor 内で、リグ未設定の警告メッセージを表示します。
        /// </summary>
        void MissingRigMessage(EmeraldInverseKinematics self)
        {
            if (self.UpperBodyRigsList.Count == 0)
            {
                // 英文→日本語
                CustomEditorProperties.DisplaySetupWarning(
                    "この AI には適用済みの Rig がありません。IK コンポーネントで制御されないため、" +
                    "『リグ設定』セクション内の「上半身リグのリスト」に制御したい Rig を追加してください。"
                );
            }
        }

        void RigSettings(EmeraldInverseKinematics self) // 「リグ設定」セクション
        {
            // 英語 "Rig Settings" → 日本語「リグ設定」
            RigSettingsFoldout.boolValue = CustomEditorProperties.Foldout(RigSettingsFoldout.boolValue, "リグ設定", true, FoldoutStyle);

            if (RigSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // 英語説明 → 日本語
                CustomEditorProperties.TextTitleWithDescription(
                    "リグ設定",
                    "この AI が IK に使用する Rig コンポーネントを割り当てます。上半身リグは『見る/狙う』挙動、" +
                    "下半身リグは脚や足の接地などに使用します。詳細は下の設定で制御できます。",
                    true
                );

                // ボタンとツールチップを日本語に（挙動は同じ）
                if (GUILayout.Button(
                    new GUIContent(
                        "新規リグを作成",
                        "すでに作成済みのリグがあっても、新しい Rig を追加作成し、この AI に自動でアサイン＆ペアレントします。\n\n" +
                        "注意：Rig 配下の子オブジェクトに、目的のコンストレイント（Constraint）コンポーネントを別途追加する必要があります。"
                    )))
                {
                    CustomRigSetup(self); // 新規 Rig の自動セットアップ
                }

                EditorGUILayout.Space();
                // 英語 → 日本語
                CustomEditorProperties.CustomHelpLabelField(
                    "この AI が『注視/エイム』IK に使用する上半身の Rig コンポーネント（Head, Spine, Chest, Arms など）です。",
                    false
                );
                UpperBodyRigsList.DoLayoutList(); // 上半身リグの ReorderableList
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void CustomRigSetup(EmeraldInverseKinematics self) // 新規 Rig の自動セットアップ
        {
            var rigBuilder = self.transform.GetComponent<RigBuilder>();

            if (rigBuilder == null)
                rigBuilder = Undo.AddComponent<RigBuilder>(self.transform.gameObject);
            else
                Undo.RecordObject(rigBuilder, "Rig Builder コンポーネントを追加"); // Undo 履歴名を日本語化

            var name = "Rig"; // 生成する子 Rig のベース名（既存数に応じて "Rig n"）
            var cnt = 1;
            while (rigBuilder.transform.Find(string.Format("{0} {1}", name, cnt)) != null)
            {
                cnt++;
            }
            name = string.Format("{0} {1}", name, cnt);
            var rigGameObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(rigGameObject, name);
            rigGameObject.transform.SetParent(rigBuilder.transform);
            rigGameObject.transform.localPosition = Vector3.zero;
            rigGameObject.transform.localScale = Vector3.one;

            var rig = Undo.AddComponent<Rig>(rigGameObject);
            rigBuilder.layers.Add(new RigLayer(rig));

            if (PrefabUtility.IsPartOfPrefabInstance(rigBuilder))
                EditorUtility.SetDirty(rigBuilder);

            self.UpperBodyRigsList.Add(rig); // 上半身リグ一覧に追加
        }

        void GeneralIKSettings(EmeraldInverseKinematics self) // 「一般IK設定」セクション
        {
            // 英語 "General IK Settings" → 日本語「一般IK設定」
            GeneralIKSettingsFoldout.boolValue = CustomEditorProperties.Foldout(GeneralIKSettingsFoldout.boolValue, "一般IK設定", true, FoldoutStyle);

            if (GeneralIKSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // 英語説明 → 日本語
                CustomEditorProperties.TextTitleWithDescription(
                    "一般IK設定",
                    "この AI の IK に関する速度や角度などの共通パラメータを制御します。",
                    true
                );

                // —— 徘徊：角度上限 ——
                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), WanderingLookAtLimit, "徘徊時の角度上限", 1, 90);
                CustomEditorProperties.CustomHelpLabelField("ターゲットを注視する際の角度上限を制御します。", true);

                // —— 戦闘：角度上限 ——
                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), CombatLookAtLimit, "戦闘時の角度上限", 1, 90);
                CustomEditorProperties.CustomHelpLabelField("ターゲットを注視する際の角度上限を制御します。", true);

                // —— 徘徊：注視速度 ——
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), WanderingLookSpeed, "徘徊時の注視速度", 1f, 15f);
                CustomEditorProperties.CustomHelpLabelField("AI がターゲット方向へ視線を向ける速さを制御します。", true);

                // —— 戦闘：注視速度 ——
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), CombatLookSpeed, "戦闘時の注視速度", 1f, 15f);
                CustomEditorProperties.CustomHelpLabelField("AI がターゲット方向へ視線を向ける速さを制御します。", true);

                // —— 徘徊：注視距離 ——
                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), WanderingLookDistance, "徘徊時の注視距離", 5, 40);
                CustomEditorProperties.CustomHelpLabelField("注視対象までの距離を制御します。", true);

                // —— 戦闘：注視距離 ——
                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), CombatLookDistance, "戦闘時の注視距離", 5, 40);
                CustomEditorProperties.CustomHelpLabelField("注視対象までの距離を制御します。", true);

                // —— 徘徊：高さオフセット ——
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), WanderingLookHeightOffset, "徘徊時の高さオフセット", -3f, 3f);
                CustomEditorProperties.CustomHelpLabelField("ターゲットを注視するときの基準位置に対する高さの補正量を制御します。", true);

                // —— 戦闘：高さオフセット ——
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), CombatLookHeightOffset, "戦闘時の高さオフセット", -3f, 3f);
                CustomEditorProperties.CustomHelpLabelField("ターゲットを注視するときの基準位置に対する高さの補正量を制御します。", true);

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}
