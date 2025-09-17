


#if UNITY_EDITOR
namespace RaycastPro
{
    using UnityEngine;
    using UnityEditor;
    using Editor;
    using System.Reflection;
    
    [InitializeOnLoad]
    public class Autorun
    {
        internal const string FIRST_TIME = "RKPRO_FirstTime";

        internal static bool DarkMode;

        static Autorun()
        {
            EditorApplication.update += RunOnce;
            EditorApplication.quitting += Quit;
        }
        private static void Quit() => EditorPrefs.DeleteKey(FIRST_TIME);
        private static void RunOnce()
        {
            // need scriptable object System
            RCProPanel.LoadPreferences(false);
            
            DarkMode = EditorGUIUtility.isProSkin;
            
            // Obsolete
            RCProPanel.RefreshIcons();
            var firstTime = EditorPrefs.GetBool(FIRST_TIME, true);
            if (firstTime)
            {
                RCProEditor.Log("<color=#38FFD3>プロジェクトへようこそ。準備はできていますか？</color>");
                EditorPrefs.SetBool(FIRST_TIME, false);
                if (EditorPrefs.GetBool(RCProPanel.KEY + RCProPanel.CShowOnStart, true))
                {
                    RCProPanel.LoadWhenOpen = true;
                    RCProPanel.Init();
                    RCProPanel.LoadWhenOpen = false;
                }
                else
                {
                    RCProPanel.LoadPreferences();
                }
                //
                RCProPanel.RefreshIcons();
            }
            EditorApplication.update -= RunOnce;
        }
    }
}
#endif