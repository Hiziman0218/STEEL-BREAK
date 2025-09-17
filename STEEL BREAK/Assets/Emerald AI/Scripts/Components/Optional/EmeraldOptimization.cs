using System.Collections;                       // コルーチンを扱うためのコレクションAPI
using System.Collections.Generic;               // 汎用コレクション（未使用だが原文通り保持）
using UnityEngine;                              // Unity の基本API
using EmeraldAI.Utility;                        // Emerald のユーティリティ（VisibilityCheck 等）
using UnityEditor;                              // UnityEditor（実行時は参照されないが原文通り保持）

namespace EmeraldAI                               // EmeraldAI 名前空間
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/optimization-component")] // 公式WikiのヘルプURL

    // 【クラス概要】EmeraldOptimization：
    // AI の描画可視性や LOD を監視し、最適化（非表示時の処理抑制など）を行うための補助コンポーネント。
    // LODGroup または単一メッシュに対応し、可視性チェック用のコンポーネントを自動付与して制御します。
    public class EmeraldOptimization : MonoBehaviour
    {
        [Header("【Editor表示】設定群を隠す（折りたたみ制御）")]
        public bool HideSettingsFoldout;                 // インスペクタの表示制御用

        [Header("【Editor表示】Optimization セクションの折りたたみ状態")]
        public bool OptimizationFoldout;                 // 最適化設定の折りたたみ

        // 最適化状態（有効/無効）
        public enum OptimizedStates { Active = 0, Inactive = 1 };

        [Header("現在の最適化状態（Active=最適化中 / Inactive=非最適化）")]
        public OptimizedStates OptimizedState = OptimizedStates.Inactive; // 実行時に状態を保持

        // 利用する LOD 総数（1〜4）
        public enum TotalLODsEnum { One = 1, Two = 2, Three = 3, Four = 4 };

        [Header("LOD の総数（LODGroup のレベル数に一致させる）")]
        public TotalLODsEnum TotalLODsRef = TotalLODsEnum.Three; // 既定は3段

        [Header("AI の最適化を有効にするか（Yes=有効）")]
        public YesOrNo OptimizeAI = YesOrNo.Yes;         // 機能全体のON/OFF

        [Header("非表示直後に即時無効化せず、遅延を用いるか")]
        public YesOrNo UseDeactivateDelay = YesOrNo.No;  // 遅延無効化の使用有無

        // メッシュのタイプ（単一メッシュ or LODGroup）
        public enum MeshTypes { SingleMesh, LODGroup };

        [Header("メッシュ種別（SingleMesh or LODGroup）")]
        public MeshTypes MeshType = MeshTypes.SingleMesh; // 既定は単一メッシュ

        [Header("単一メッシュ時に対象とする Renderer（AI 本体の描画）")]
        public Renderer AIRenderer;                       // 単一メッシュ用のレンダラー

        [Header("LOD1 の Renderer（LODGroup 使用時）")]
        public Renderer Renderer1;                        // LODレベル1のレンダラー

        [Header("LOD2 の Renderer（LODGroup 使用時）")]
        public Renderer Renderer2;                        // LODレベル2のレンダラー

        [Header("LOD3 の Renderer（LODGroup 使用時）")]
        public Renderer Renderer3;                        // LODレベル3のレンダラー

        [Header("LOD4 の Renderer（LODGroup 使用時）")]
        public Renderer Renderer4;                        // LODレベル4のレンダラー

        [Header("可視性チェック用コンポーネント参照（実行時に自動追加）")]
        public VisibilityCheck m_VisibilityCheck;         // 可視/不可視の監視

        [Header("非アクティブ化までの遅延秒数（UseDeactivateDelay=Yes のとき使用）")]
        public int DeactivateDelay = 5;                   // 無効化までの遅延

        [Header("初期化完了フラグ（内部状態）")]
        public bool Initialized;                          // 初期化完了かどうか

        [Header("EmeraldSystem 参照（AI 本体コンポーネント）")]
        EmeraldSystem EmeraldComponent;                   // 実行時に取得

        void Start()                                      // Unity ライフサイクル：開始時
        {
            InitializeOptimizationSettings();             // 最適化設定の初期化
            StartCoroutine(Initialize());                 // 遅延初期化（0.5秒経過を待つ）
        }

        IEnumerator Initialize()                         // 遅延初期化用コルーチン
        {
            while (Time.time < 0.5f)                     // 起動直後は少し待機
            {
                yield return null;                       // 次フレームまで待つ
            }

            Initialized = true;                          // 初期化完了をマーク
        }

        /// <summary>
        /// （日本語）最適化設定を初期化する。
        /// 可視性チェックの付与、LODGroup の読込、各種参照のセットアップ等を行う。
        /// </summary>
        public void InitializeOptimizationSettings()
        {
            if (OptimizeAI == YesOrNo.Yes)               // 最適化が有効な場合のみ処理
            {
                EmeraldComponent = GetComponent<EmeraldSystem>(); // AI 本体参照を取得

                if (OptimizeAI == YesOrNo.Yes && MeshType == MeshTypes.SingleMesh) // 単一メッシュ運用
                {
                    if (AIRenderer != null && UseDeactivateDelay == YesOrNo.No)    // 遅延を使わない場合
                    {
                        DeactivateDelay = 0;                                       // 遅延を0に
                        m_VisibilityCheck = AIRenderer.gameObject.AddComponent<VisibilityCheck>(); // 可視性チェックを付与
                        m_VisibilityCheck.EmeraldComponent = EmeraldComponent;     // 参照をセット
                        m_VisibilityCheck.EmeraldOptimization = this;
                    }
                    else if (AIRenderer != null && UseDeactivateDelay == YesOrNo.Yes) // 遅延あり
                    {
                        m_VisibilityCheck = AIRenderer.gameObject.AddComponent<VisibilityCheck>();
                        m_VisibilityCheck.EmeraldComponent = EmeraldComponent;
                        m_VisibilityCheck.EmeraldOptimization = this;
                    }
                    else if (MeshType == MeshTypes.SingleMesh && AIRenderer == null) // 単一メッシュなのに Renderer 未設定
                    {
                        OptimizeAI = YesOrNo.No;                                    // 最適化を無効化
                    }
                }

                if (MeshType == MeshTypes.LODGroup)       // LODGroup 運用
                {
                    GetLODs();                             // LODGroup から各 LOD の Renderer を取得

                    if (TotalLODsRef == TotalLODsEnum.One) // LOD が1段のとき
                    {
                        if (Renderer1 == null)
                        {
                            OptimizeAI = YesOrNo.No;       // 不備があれば無効化して単一メッシュ扱いへ
                            MeshType = MeshTypes.SingleMesh;
                        }
                        else
                        {
                            m_VisibilityCheck = Renderer1.gameObject.AddComponent<VisibilityCheck>();
                            m_VisibilityCheck.EmeraldComponent = EmeraldComponent;
                            m_VisibilityCheck.EmeraldOptimization = this;
                        }
                    }
                    else if (TotalLODsRef == TotalLODsEnum.Two) // LOD が2段
                    {
                        if (Renderer1 == null || Renderer2 == null)
                        {
                            OptimizeAI = YesOrNo.No;
                            MeshType = MeshTypes.SingleMesh;
                        }
                        else
                        {
                            m_VisibilityCheck = Renderer2.gameObject.AddComponent<VisibilityCheck>();
                            m_VisibilityCheck.EmeraldComponent = EmeraldComponent;
                            m_VisibilityCheck.EmeraldOptimization = this;
                        }
                    }
                    else if (TotalLODsRef == TotalLODsEnum.Three) // LOD が3段
                    {
                        if (Renderer1 == null || Renderer2 == null || Renderer3 == null)
                        {
                            OptimizeAI = YesOrNo.No;
                            MeshType = MeshTypes.SingleMesh;
                        }
                        else
                        {
                            m_VisibilityCheck = Renderer3.gameObject.AddComponent<VisibilityCheck>();
                            m_VisibilityCheck.EmeraldComponent = EmeraldComponent;
                            m_VisibilityCheck.EmeraldOptimization = this;
                        }
                    }
                    else if (TotalLODsRef == TotalLODsEnum.Four) // LOD が4段
                    {
                        if (Renderer1 == null || Renderer2 == null ||
                            Renderer3 == null || Renderer4 == null)
                        {
                            OptimizeAI = YesOrNo.No;
                            MeshType = MeshTypes.SingleMesh;
                        }
                        else
                        {
                            m_VisibilityCheck = Renderer4.gameObject.AddComponent<VisibilityCheck>();
                            m_VisibilityCheck.EmeraldComponent = EmeraldComponent;
                            m_VisibilityCheck.EmeraldOptimization = this;
                        }
                    }
                }
            }
            else if (OptimizeAI == YesOrNo.No)           // 最適化が無効の場合
            {
                OptimizedState = OptimizedStates.Inactive; // 状態を非最適化に
            }
        }

        /// <summary>
        /// （日本語）AI の LODGroup コンポーネントから、各 LOD レベルの Renderer を取得する。
        /// </summary>
        void GetLODs()
        {
            LODGroup _LODGroup = GetComponentInChildren<LODGroup>(); // 子階層から LODGroup を取得

            if (_LODGroup == null)                   // LODGroup が存在しない場合
            {
                Debug.LogError("No LOD Group could be found. Please ensure that your AI has an LOD group that has at least 1 levels. The LODGroup Feature has been disabled."); // 英文の原文ログ
                MeshType = MeshTypes.SingleMesh;     // 単一メッシュ扱いにフォールバック
            }
            else if (_LODGroup != null)              // 見つかった場合
            {
                LOD[] AllLODs = _LODGroup.GetLODs(); // すべての LOD 情報を取得

                if (_LODGroup.lodCount <= 4)         // 最大4段までを想定
                {
                    TotalLODsRef = (TotalLODsEnum)(_LODGroup.lodCount); // 実際の段数に合わせて更新
                }

                if (_LODGroup.lodCount >= 1)         // 1段以上ある場合
                {
                    for (int i = 0; i < _LODGroup.lodCount; i++) // 各 LOD レベルを走査
                    {
                        if (i == 0)
                        {
                            Renderer1 = AllLODs[i].renderers[0]; // LOD1 の先頭 Renderer
                        }
                        if (i == 1)
                        {
                            Renderer2 = AllLODs[i].renderers[0]; // LOD2 の先頭 Renderer
                        }
                        if (i == 2)
                        {
                            Renderer3 = AllLODs[i].renderers[0]; // LOD3 の先頭 Renderer
                        }
                        if (i == 3)
                        {
                            Renderer4 = AllLODs[i].renderers[0]; // LOD4 の先頭 Renderer
                        }
                    }
                }
            }
        }

        void Update()                                // 毎フレーム更新
        {
            // 最適化機能が有効で、LODGroup を使用し、初期化完了後であれば
            // LOD の各 Renderer 可視性をチェック（VisibilityCheck 経由）
            if (OptimizeAI == YesOrNo.Yes && MeshType == MeshTypes.LODGroup && Initialized)
            {
                m_VisibilityCheck.CheckAIRenderers(); // 可視性チェックを実行
            }
        }
    }
}
