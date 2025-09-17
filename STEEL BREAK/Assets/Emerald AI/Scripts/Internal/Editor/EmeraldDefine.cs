using UnityEditor;  // Unity エディタ拡張用のAPI

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【EmeraldDefine】
    /// エディタ起動時（[InitializeOnLoad]）に、現在のビルドターゲットグループへ
    /// スクリプト定義シンボル「EMERALD_AI_2024_PRESENT」を自動的に付与します。
    /// すでに定義されている場合は何もしません。
    /// </summary>
    [InitializeOnLoad]
    public class EmeraldDefine
    {
        [UnityEngine.Header("自動で付与するスクリプト定義シンボル（Scripting Define Symbols）")]
        const string EmeraldAIDefinesString = "EMERALD_AI_2024_PRESENT";

        /// <summary>
        /// 【静的コンストラクタ】
        /// クラスが初期化されたタイミングで1度だけ呼び出され、
        /// 定義シンボルの初期化処理 <see cref="InitializeEmeraldAIDefines"/> を実行します。
        /// </summary>
        static EmeraldDefine()
        {
            InitializeEmeraldAIDefines();
        }

        /// <summary>
        /// 【定義シンボルの初期化処理】
        /// 現在選択中の BuildTargetGroup を取得し、そのグループに設定されている
        /// Scripting Define Symbols を走査。対象シンボルが未含有であれば追記します。
        /// </summary>
        static void InitializeEmeraldAIDefines()
        {
            // 現在のビルドターゲットグループ（例：Standalone, Android, iOS など）を取得
            var BTG = EditorUserBuildSettings.selectedBuildTargetGroup;

            // 当該グループの Scripting Define Symbols を取得（セミコロン区切りの文字列）
            string EmeraldAIDef = PlayerSettings.GetScriptingDefineSymbolsForGroup(BTG);

            // まだ対象シンボルが含まれていない場合のみ処理
            if (!EmeraldAIDef.Contains(EmeraldAIDefinesString))
            {
                if (string.IsNullOrEmpty(EmeraldAIDef))
                {
                    // 何も定義がなければ、そのまま1件だけ設定
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BTG, EmeraldAIDefinesString);
                }
                else
                {
                    // 末尾が ';' で終わっていなければセパレータを追加
                    if (EmeraldAIDef[EmeraldAIDef.Length - 1] != ';')
                    {
                        EmeraldAIDef += ';';
                    }

                    // 対象シンボルを追記して反映
                    EmeraldAIDef += EmeraldAIDefinesString;
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BTG, EmeraldAIDef);
                }
            }
        }
    }
}
