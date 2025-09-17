using System; // .NETの基本機能（配列や数値型など）を使用するための名前空間
using System.Collections.Generic; // ListやDictionaryなどのコレクションを使用可能にする

namespace RaycastPro // 名前空間RaycastProの定義。Raycast関連のコードをグループ化
{
    using System.Linq; // LINQ（クエリ構文）を使用するための名前空間
    using UnityEngine; // Unityの基本機能（MonoBehaviour等）を使うための名前空間
#if UNITY_EDITOR
    using Editor; // カスタムエディタ関連の名前空間（ユーザー独自）
    using UnityEditor; // UnityEditor API使用のための名前空間（エディタ限定）
#endif

    [ExecuteInEditMode] // エディタ実行中でも処理を実行できる属性
    [AddComponentMenu("RaycastPro/Utility/" + nameof(RayManager))] // コンポーネント追加メニューに表示される名前
    public sealed class RayManager : RaycastCore // RaycastCore を継承した RayManager クラス
    {
        [SerializeField] private RaycastCore[] cores; // 子オブジェクトのRaycastCoreを格納する配列（インスペクター表示用）

        [SerializeField] private bool[] Foldouts = Array.Empty<bool>(); // 各コアの折りたたみ状態を保持する配列（エディタ用）

        public override bool Performed // Performedプロパティのオーバーライド
        {
            get => cores.All(r => r.Performed); // すべてのRaycastCoreがPerformedならtrueを返す
            protected set { } // 設定は不可（空定義）
        }

        [ExecuteAlways] // エディタ上で変更時にも呼び出される属性
        protected void OnTransformChildrenChanged() // 子Transformの変更時に呼び出されるUnityイベント関数
        {
            Refresh(); // コア情報を再取得
        }

        protected void Refresh() // コア配列の再構成を行う関数
        {
            cores = GetComponentsInChildren<RaycastCore>(true).Where(c => !(c is RayManager)).ToArray(); // 自身以外のRaycastCoreを取得
            Array.Resize(ref Foldouts, cores.Length); // フォルドアウト配列をコア数に合わせてリサイズ
        }

        protected void Reset() // 初期化用関数。エディタでAddされた時などに呼ばれる
        {
            Refresh(); // コア配列を更新

            styleH = new GUIStyle // ヘッダー用GUIスタイルの設定
            {
                margin = new RectOffset(0, 0, 4, 4), // マージン設定
                padding = new RectOffset(0, 0, 2, 4), // パディング設定
                stretchWidth = false, // 幅を自動伸縮させない
                wordWrap = true, // テキストを自動折り返し
            };

            styleM = new GUIStyle // 中央用スタイル設定
            {
                margin = new RectOffset(1, 1, 4, 4), // マージン設定
                padding = new RectOffset(5, 5, 4, 4), // パディング設定
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true // テキスト中央揃え＋折り返し
            };
        }

        private GUIStyle styleH, styleM; // GUI描画用のスタイル変数
        protected override void OnCast() // Raycastを実行するためのオーバーライド関数（未実装）
        {
            // 処理なし（将来的に拡張可能）
        }

#if UNITY_EDITOR // 以下はUnityエディタ実行時のみ有効なコードブロック

        internal override string Info => "このレイ制御および管理ツールは、子オブジェクトのレイを自動的に検出します。" + HUtility + HDependent; // エディタで表示される情報文字列
        internal override void OnGizmos() // ギズモ描画処理（未使用）
        { }

        [SerializeField]
        private bool showMain = true; // メイン設定表示フラグ
        [SerializeField]
        private bool showGeneral = false; // 一般設定表示フラグ

        private int index; // コア配列用のインデックス変数
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true) // エディタ用パネル描画関数のオーバーライド
        {
            BeginVerticalBox(); // 縦レイアウト開始
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(showMain))); // showMainの表示設定
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(showGeneral))); // showGeneralの表示設定
            EndVertical(); // 縦レイアウト終了

            for (index = 0; index < cores.Length; index++) // 各コアに対してUI描画を行うループ
            {
                var core = cores[index]; // 現在のコアを取得

                BeginVerticalBox(); // 各コア用レイアウト開始
                EditorGUILayout.BeginHorizontal(); // 横並びレイアウト開始
                var guiStyle = new GUIStyle(EditorStyles.foldout) // フォルドアウトスタイルの定義
                {
                    margin = new RectOffset(10, 10, 0, 5) // 左右10pxのマージン
                };

                Foldouts[index] = EditorGUILayout.Foldout(Foldouts[index], core.name.ToContent(RCProEditor.GetInfo(core)), guiStyle); // コア名の折りたたみ表示

                var _t = EditorGUIUtility.labelWidth; // ラベル幅を退避
                InLabelWidth(() => // 一時的にラベル幅を変更してボタン表示
                {
                    cores[index].gameObject.SetActive(EditorGUILayout.ToggleLeft("A".ToContent(), cores[index].gameObject.activeInHierarchy, GUILayout.Width(30))); // Activeトグル
                    cores[index].enabled = EditorGUILayout.ToggleLeft("E".ToContent(), cores[index].enabled, GUILayout.Width(30)); // Enableトグル
                }, 15);
                if (cores[index].gameObject.activeInHierarchy) // アクティブ状態のとき
                {
                    var _cSO = new SerializedObject(cores[index]); // シリアライズ化
                    _cSO.Update(); // 更新
                    var prop = _cSO.FindProperty("gizmosUpdate"); // ギズモ設定取得

                    if (GUILayout.Button(cores[index].gizmosUpdate.ToString(), GUILayout.Width(60f))) // ボタンを押すとギズモモードを変更
                    {
                        switch (cores[index].gizmosUpdate) // ギズモモードを切り替える処理
                        {
                            case GizmosMode.Select:
                                prop.enumValueIndex = 0;
                                break;
                            case GizmosMode.Auto:
                                prop.enumValueIndex = 2;
                                break;
                            case GizmosMode.Fix:
                                prop.enumValueIndex = 3;
                                break;
                            case GizmosMode.Off:
                                prop.enumValueIndex = 1;
                                break;
                        }
                        _cSO.ApplyModifiedProperties(); // 変更を適用
                    }
                }
                else // 非アクティブの場合はラベル表示
                {
                    GUILayout.Box("Off", RCProEditor.BoxStyle, GUILayout.Width(60), GUILayout.Height(20)); // オフ表示ボックス
                }

                GUI.backgroundColor = (core.Performed ? DetectColor : BlockColor).Alpha(.4f); // 実行済みかどうかで背景色変更
                if (GUILayout.Button("Cast", GUILayout.Width(60f))) // Castボタン
                {
                    core.TestCast(); // コアのテストキャスト実行
                }

                // 実行結果マーク用の旧コード（コメントアウト）：
                //GUILayout.Box(raySensor.Performed ? "<color=#61FF38>✓</color>" : "<color=#FF3822>x</color>", RCProEditor.BoxStyle, GUILayout.Width(40), GUILayout.Height(20));
                EndHorizontal(); // 横並び終了
                GUI.backgroundColor = RCProEditor.Violet; // 背景色をリセット

                if (Foldouts[index]) // フォルドアウト展開中なら詳細描画
                {
                    var _cSO = new SerializedObject(core); // シリアライズ化
                    _cSO.Update(); // 更新
                    EditorGUI.BeginChangeCheck(); // 変更チェック開始
                    core.EditorPanel(_cSO, showMain, showGeneral, false, false); // 子パネル描画
                    if (EditorGUI.EndChangeCheck()) _cSO.ApplyModifiedProperties(); // 変更があれば適用
                }

                EndVertical(); // コア用レイアウト終了
            }
        }
#endif
    }
}