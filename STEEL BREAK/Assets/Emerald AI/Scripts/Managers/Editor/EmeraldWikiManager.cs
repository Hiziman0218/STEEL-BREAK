using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【EmeraldWikiManager】
    /// メニューから「公式 Emerald AI Wiki」の各ページをブラウザで開くユーティリティ。
    /// ※メンバー変数は存在しないため [Header] の適用対象はありません。
    /// </summary>
    public class EmeraldWikiManager : EditorWindow
    {
        // === 公式 Wiki（ホーム/導入/レンダーパイプライン） ===

        /// <summary>公式 Wiki ホームを開く</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/ホーム", false, 250)]
        public static void Home()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/");
        }

        /// <summary>はじめに（Getting Started）を開く</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/はじめに", false, 250)]
        public static void GettingStarted()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/getting-started/getting-started");
        }

        /// <summary>URP/HDRP へのアップグレード手順を開く</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/URP/HDRPへのアップグレード", false, 251)]
        public static void UpgradingToURPAndHDRP()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/getting-started/upgrading-to-urp-and-hdrp");
        }

        // === 必須コンポーネント ===

        /// <summary>Animation コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/必須/Animation コンポーネント", false, 300)]
        public static void AnimationComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/animation-component");
        }

        /// <summary>Behaviors コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/必須/Behaviors コンポーネント", false, 300)]
        public static void BehaviorsComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/behaviors-component");
        }

        /// <summary>Combat コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/必須/Combat コンポーネント", false, 300)]
        public static void CombatComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/combat-component");
        }

        /// <summary>Detection コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/必須/Detection コンポーネント", false, 300)]
        public static void DetectionComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/detection-component");
        }

        /// <summary>Movement コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/必須/Movement コンポーネント", false, 300)]
        public static void MovementComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/movement-component");
        }

        /// <summary>Health コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/必須/Health コンポーネント", false, 300)]
        public static void HealthComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/health-component");
        }

        /// <summary>Sounds コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/必須/Sounds コンポーネント", false, 300)]
        public static void SoundsComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/sounds-component");
        }

        // === 任意コンポーネント ===

        /// <summary>Cover コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/任意/Cover コンポーネント", false, 400)]
        public static void CoverComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/cover-component");
        }

        /// <summary>Debugger コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/任意/Debugger コンポーネント", false, 400)]
        public static void DebuggerComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/debugger-component");
        }

        /// <summary>Decal コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/任意/Decal コンポーネント", false, 400)]
        public static void DecalComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/decal-component");
        }

        /// <summary>Events コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/任意/Events コンポーネント", false, 400)]
        public static void EventsComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/events-component");
        }

        /// <summary>Footsteps コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/任意/Footsteps コンポーネント", false, 400)]
        public static void FootstepsComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/footsteps-component");
        }

        /// <summary>Inverse Kinematics コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/任意/Inverse Kinematics コンポーネント", false, 400)]
        public static void IKComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/inverse-kinematics-component");
        }

        /// <summary>Items コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/任意/Items コンポーネント", false, 400)]
        public static void ItemsComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/items-component");
        }

        /// <summary>Location Based Damage コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/任意/Location Based Damage コンポーネント", false, 400)]
        public static void LBDComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/location-based-damage-component");
        }

        /// <summary>Optimization コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/任意/Optimization コンポーネント", false, 400)]
        public static void OptimizationComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/optimization-component");
        }

        /// <summary>Sound Detector コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/任意/Sound Detector コンポーネント", false, 400)]
        public static void SoundDetectorComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/sound-detector-component");
        }

        /// <summary>Target Position Modifier コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/任意/Target Position Modifier コンポーネント", false, 400)]
        public static void TPMComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/target-position-modifier-component");
        }

        /// <summary>UI コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/任意/UI コンポーネント", false, 400)]
        public static void UIComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/ui-component");
        }

        /// <summary>Weapon Collisions コンポーネント解説</summary>
        [MenuItem("Window/Emerald AI/公式 Emerald AI Wiki/任意/Weapon Collisions コンポーネント", false, 400)]
        public static void WeaponCollisionsComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/weapon-collisions-component");
        }

        // === 連携（Integrations） ===

        /// <summary>FPS Engine 連携</summary>
        [MenuItem("Window/Emerald AI/連携/FPS Engine", false, 251)]
        public static void FPSEngineIntegration()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/integrations/integrations/fps-engine");
        }

        /// <summary>Invector 連携</summary>
        [MenuItem("Window/Emerald AI/連携/Invector", false, 252)]
        public static void InvectorIntegration()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/integrations/invector");
        }

        /// <summary>Final IK 連携</summary>
        [MenuItem("Window/Emerald AI/連携/Final IK", false, 253)]
        public static void FinalIKIntegration()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/integrations/final-ik");
        }

        /// <summary>Dialogue System 連携</summary>
        [MenuItem("Window/Emerald AI/連携/Dialogue System", false, 253)]
        public static void DialogueSystemIntegration()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/integrations/integrations/dialogue-system");
        }

        /// <summary>Quest Machine 連携</summary>
        [MenuItem("Window/Emerald AI/連携/Quest Machine", false, 254)]
        public static void QuestMachineIntegration()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/integrations/integrations/quest-machine");
        }

        /// <summary>Love Hate 連携</summary>
        [MenuItem("Window/Emerald AI/連携/Love Hate", false, 255)]
        public static void LoveHateIntegration()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/integrations/integrations/love-hate");
        }

        // === サポート ===

        /// <summary>AI 生成ソリューション（Wiki 検索ツールの使い方）</summary>
        [MenuItem("Window/Emerald AI/サポート/AI 生成ソリューション", false, 253)]
        public static void AIGeneratedSolutions()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/help/using-the-wiki-search-tool");
        }

        /// <summary>起こりうる問題の解決策</summary>
        [MenuItem("Window/Emerald AI/サポート/起こりうる問題の解決策", false, 253)]
        public static void SolutionsToPossibleIssues()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/help/solutions-to-possible-issues");
        }

        /// <summary>お問い合わせ（サポート窓口）</summary>
        [MenuItem("Window/Emerald AI/サポート/お問い合わせ", false, 253)]
        public static void ContactSupport()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/help/support");
        }

        /// <summary>バグの報告（GitHub Issues）</summary>
        [MenuItem("Window/Emerald AI/バグの報告", false, 300)]
        public static void ReportBug()
        {
            Application.OpenURL("https://github.com/Black-Horizon-Studios/Emerald-AI-2024/issues");
        }
    }
}
