using System.Collections;                         // コルーチン等の基本コレクションAPI
using System.Collections.Generic;                 // List<T> などの汎用コレクション
using UnityEngine;                                // Unity の基本API
using UnityEngine.UI;                             // UI（Text, Image, Canvas 等）
using EmeraldAI.Utility;                          // Emerald ユーティリティ

namespace EmeraldAI                                // EmeraldAI 名前空間
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/ui-component")] // ヘルプURL（インスペクタから参照）

    // 【クラス概要】EmeraldUI：
    //  AI の頭上に表示するヘルスバーや名前/レベルなどの UI を生成・表示/非表示制御するコンポーネント。
    //  検出状態に応じて UI を自動で切り替え、見た目（色/フォント/アウトライン等）を設定できる。
    public class EmeraldUI : MonoBehaviour
    {
        #region Variables // —— UI 設定・参照などのメンバ変数 ——

        [Header("AIの表示名（頭上に表示する名前）")]
        public string AIName = "AI Name";                // AI の名前テキスト

        [Header("AIの肩書き/サブタイトル（表示名の2行目として追加可能）")]
        public string AITitle = "AI Title";              // AI の肩書き

        [Header("AIのレベル（数値表示）")]
        public int AILevel = 1;                          // レベル数値

        [Header("【Editor表示】設定セクションを隠す（折りたたみ制御）")]
        public bool HideSettingsFoldout;                 // インスペクタの表示切替

        [Header("【Editor表示】UI設定セクションの折りたたみ")]
        public bool UISettingsFoldout;                   // UI 設定の開閉

        [Header("【Editor表示】ヘルスバー設定の折りたたみ")]
        public bool HealthBarsFoldout;                   // ヘルスバー関連の開閉

        [Header("【Editor表示】コンバットテキスト設定の折りたたみ")]
        public bool CombatTextFoldout;                   // コンバットテキスト関連の開閉

        [Header("【Editor表示】名前テキスト設定の折りたたみ")]
        public bool NameTextFoldout;                     // 名前テキストの開閉

        [Header("【Editor表示】レベルテキスト設定の折りたたみ")]
        public bool LevelTextFoldout;                    // レベルテキストの開閉

        [Header("カスタムヘルスバー画像を使用する（Yes=使用）")]
        public YesOrNo UseCustomHealthBar = YesOrNo.No;  // 独自スプライトの使用可否

        [Header("AIの名前テキストを表示する（Yes=表示）")]
        public YesOrNo DisplayAIName = YesOrNo.No;       // 名前の表示可否

        [Header("AIの肩書きを名前の2行目として表示する（Yes=表示）")]
        public YesOrNo DisplayAITitle = YesOrNo.No;      // 肩書きの表示可否

        [Header("AIのレベルを表示する（Yes=表示）")]
        public YesOrNo DisplayAILevel = YesOrNo.No;      // レベルの表示可否

        [Header("AI名テキストにアウトライン効果を付与（Yes=付与）")]
        public YesOrNo UseAINameUIOutlineEffect = YesOrNo.Yes; // 名前アウトライン

        [Header("AIレベルテキストにアウトライン効果を付与（Yes=付与）")]
        public YesOrNo UseAILevelUIOutlineEffect = YesOrNo.Yes; // レベルアウトライン

        [Header("AI名テキストにカスタムフォントを使用（Yes=使用）")]
        public YesOrNo UseCustomFontAIName = YesOrNo.No; // 名前フォント差し替え

        [Header("AIレベルテキストにカスタムフォントを使用（Yes=使用）")]
        public YesOrNo UseCustomFontAILevel = YesOrNo.No; // レベルフォント差し替え

        [Header("ヘルスバーを自動作成する（Resources から自動ロード/生成）")]
        public YesOrNo AutoCreateHealthBars = YesOrNo.No; // 自動作成の可否

        [Header("生成されたヘルスバーの Canvas 参照（自動作成時に取得）")]
        public Canvas HealthBarCanvasRef;                 // Canvas 参照

        [Header("生成されたヘルスバーのルート GameObject（親）")]
        public GameObject HealthBar;                      // ヘルスバーの親

        [Header("ヘルスバーのプレハブ（未割当かつ自動作成時は Resources からロード）")]
        public GameObject HealthBarCanvas;                // ヘルスバーのプレハブ

        [Header("参照するカメラのタグ（看板表示の向き調整等で使用想定）")]
        public string CameraTag = "MainCamera";           // カメラタグ

        [Header("EmeraldHealthBar コンポーネント参照（生成時に付与/取得）")]
        public EmeraldHealthBar m_HealthBarComponent;     // スクリプト参照

        [Header("UI 表示の対象となるプレイヤーのタグ")]
        public string UITag = "Player";                   // プレイヤータグ

        [Header("UI 表示検出用のレイヤーマスク（OverlapSphereに使用）")]
        public LayerMask UILayerMask = 0;                 // UI 検出レイヤー

        [Header("UI の最大スケール（距離によるスケーリングの上限）")]
        public int MaxUIScaleSize = 16;                   // 最大スケール値

        [Header("AI名テキストの Text コンポーネント参照（生成時に取得）")]
        public Text AINameUI;                             // 名前 Text

        [Header("AI名テキストに使用するフォント（UseCustomFontAIName=Yes時）")]
        public Font AINameFont;                           // 名前フォント

        [Header("AI名テキストの行間（肩書き併記時の間隔調整）")]
        public float AINameLineSpacing = 0.75f;           // 行間

        [Header("AI名テキストのアウトライン距離（X,Y）")]
        public Vector2 AINameUIOutlineSize = new Vector2(0.35f, -0.35f); // アウトライン距離

        [Header("AI名テキストのアウトライン色")]
        public Color AINameUIOutlineColor = Color.black;  // アウトライン色

        [Header("AIレベルテキストの Text コンポーネント参照（生成時に取得）")]
        public Text AILevelUI;                            // レベル Text

        [Header("AIレベルテキストに使用するフォント（UseCustomFontAILevel=Yes時）")]
        public Font AILevelFont;                          // レベルフォント

        [Header("AIレベルテキストのアウトライン距離（X,Y）")]
        public Vector2 AILevelUIOutlineSize = new Vector2(0.35f, -0.35f); // アウトライン距離

        [Header("AIレベルテキストのアウトライン色")]
        public Color AILevelUIOutlineColor = Color.black; // アウトライン色

        [Header("ヘルスバーの前景スプライト（UseCustomHealthBar=Yes時）")]
        public Sprite HealthBarImage;                     // 前景スプライト

        [Header("ヘルスバーの背景スプライト（UseCustomHealthBar=Yes時）")]
        public Sprite HealthBarBackgroundImage;           // 背景スプライト

        [Header("ヘルスバーのローカル座標（AI基準のオフセット）")]
        public Vector3 HealthBarPos = new Vector3(0, 1.75f, 0); // 位置オフセット

        [Header("ヘルスバー前景の色（現在HP）")]
        public Color HealthBarColor = new Color32(197, 41, 41, 255); // 前景色

        [Header("ヘルスバーの被ダメージ色（遅延ダメージバー等）")]
        public Color HealthBarDamageColor = new Color32(248, 217, 4, 255); // ダメージ色

        [Header("ヘルスバー背景の色")]
        public Color HealthBarBackgroundColor = new Color32(36, 36, 36, 255); // 背景色

        [Header("AI名テキストの色")]
        public Color NameTextColor = new Color32(255, 255, 255, 255); // 名前色

        [Header("AIレベルテキストの色")]
        public Color LevelTextColor = new Color32(255, 255, 255, 255); // レベル色

        [Header("ヘルスバーのローカルスケール")]
        public Vector3 HealthBarScale = new Vector3(0.75f, 1, 1); // スケール

        [Header("AI名テキストのフォントサイズ")]
        public int NameTextFontSize = 7;                   // 名前フォントサイズ

        [Header("（未使用/拡張用）ウェイポイント親参照")]
        public GameObject WaypointParent;                  // 予備/拡張

        [Header("（未使用/拡張用）ウェイポイントの起点名")]
        public string WaypointOrigin;                      // 予備/拡張

        [Header("AI名テキストのローカル位置（ヘルスバー基準）")]
        public Vector3 AINamePos = new Vector3(0, 3, 0);  // 名前位置

        [Header("AIレベルテキストのローカル位置（ヘルスバー基準）")]
        public Vector3 AILevelPos = new Vector3(1.5f, 0, 0); // レベル位置

        [Header("EmeraldSystem 参照（AI本体）")]
        EmeraldSystem EmeraldComponent;                    // 実行時に取得
        #endregion

        void Start()                                       // Unity ライフサイクル：開始時
        {
            InitializeUI();                                // EmeraldUI の初期化を実行
        }

        /// <summary>
        /// （日本語）UI の初期化を行う。
        /// 参照取得、イベント購読、必要に応じたヘルスバーの自動生成と各種外観設定を行う。
        /// </summary>
        void InitializeUI()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>(); // AI 本体参照を取得
            EmeraldComponent.DetectionComponent.OnDetectionUpdate += UpdateAIUI; // 検出状態更新時に UI 更新を行う

            // 自動作成 or 名前表示が必要だがプレハブ未割当の場合、Resources からロード
            if (AutoCreateHealthBars == YesOrNo.Yes && HealthBarCanvas == null || DisplayAIName == YesOrNo.Yes && HealthBarCanvas == null)
            {
                HealthBarCanvas = Resources.Load("AI Health Bar Canvas") as GameObject;
            }

            // 自動作成 or 名前表示が必要で、プレハブが用意できた場合は生成して初期設定
            if (AutoCreateHealthBars == YesOrNo.Yes && HealthBarCanvas != null || DisplayAIName == YesOrNo.Yes && HealthBarCanvas != null)
            {
                HealthBar = Instantiate(HealthBarCanvas, Vector3.zero, Quaternion.identity) as GameObject; // 生成
                GameObject HealthBarParent = new GameObject();         // 親ゲームオブジェクトを作成
                HealthBarParent.name = "HealthBarParent";              // 名前設定
                HealthBarParent.transform.SetParent(this.transform);   // AI を親にする
                HealthBarParent.transform.localPosition = new Vector3(0, 0, 0); // 原点に配置

                HealthBar.transform.SetParent(HealthBarParent.transform);       // 親子付け
                HealthBar.transform.localPosition = HealthBarPos;               // 指定位置に配置
                HealthBar.AddComponent<EmeraldHealthBar>();                     // コントローラを付与
                EmeraldHealthBar HealthBarScript = HealthBar.GetComponent<EmeraldHealthBar>(); // 参照取得
                m_HealthBarComponent = HealthBarScript;                         // 保持
                HealthBar.name = "AI Health Bar Canvas";                        // 生成物の名前

                GameObject HealthBarChild = HealthBar.transform.Find("AI Health Bar Background").gameObject; // 背景オブジェクト参照
                HealthBarChild.transform.localScale = HealthBarScale;            // スケール設定

                Image HealthBarRef = HealthBarChild.transform.Find("AI Health Bar").GetComponent<Image>(); // 前景バー
                HealthBarRef.color = HealthBarColor;                             // 色設定

                Image HealthBarDamageRef = HealthBarChild.transform.Find("AI Health Bar (Damage)").GetComponent<Image>(); // ダメージバー
                HealthBarDamageRef.color = HealthBarDamageColor;                 // 色設定

                Image HealthBarBackgroundImageRef = HealthBarChild.GetComponent<Image>(); // 背景イメージ
                HealthBarBackgroundImageRef.color = HealthBarBackgroundColor;    // 背景色設定

                HealthBarCanvasRef = HealthBar.GetComponent<Canvas>();           // Canvas 参照

                // 自動作成しない設定なら、ヘルスバー本体の表示を抑制
                if (AutoCreateHealthBars == YesOrNo.No)
                {
                    HealthBarChild.GetComponent<Image>().enabled = false;        // 背景Imageの可視をOFF
                    HealthBarRef.gameObject.SetActive(false);                    // 前景バーを非表示
                }

                // カスタムヘルスバー使用時はスプライトを差し替え
                if (UseCustomHealthBar == YesOrNo.Yes && HealthBarBackgroundImage != null && HealthBarImage != null)
                {
                    HealthBarBackgroundImageRef.sprite = HealthBarBackgroundImage;
                    HealthBarRef.sprite = HealthBarImage;
                }

                // —— AI 名の表示設定（有効時）——
                if (DisplayAIName == YesOrNo.Yes)
                {
                    AINameUI = HealthBar.transform.Find("AI Name Text").gameObject.GetComponent<Text>(); // 名前 Text を取得

                    if (UseAINameUIOutlineEffect == YesOrNo.Yes)                 // アウトライン有効なら設定
                    {
                        Outline AINameOutline = AINameUI.GetComponent<Outline>();
                        AINameOutline.effectDistance = AINameUIOutlineSize;
                        AINameOutline.effectColor = AINameUIOutlineColor;
                    }
                    else
                    {
                        AINameUI.GetComponent<Outline>().enabled = false;        // 無効化
                    }

                    if (DisplayAITitle == YesOrNo.Yes)                            // 肩書きを2行目として追加
                    {
                        AIName = AIName + "\\n" + AITitle;                        // 改行コードを挿入（エスケープ表記）
                        AIName = AIName.Replace("\\n", "\n");                     // 実際の改行へ置換
                        AINamePos.y += 0.25f;                                     // 少し上げて被りを回避

                        if (UseAINameUIOutlineEffect == YesOrNo.Yes)
                            AINameUI.lineSpacing = AINameLineSpacing;             // 行間調整
                    }

                    AINameUI.transform.localPosition = new Vector3(AINamePos.x, AINamePos.y - HealthBarPos.y, AINamePos.z); // ローカル位置
                    AINameUI.text = AIName;                                       // テキスト設定
                    AINameUI.fontSize = NameTextFontSize;                         // フォントサイズ
                    AINameUI.color = NameTextColor;                               // 色

                    if (UseCustomFontAIName == YesOrNo.Yes)                       // カスタムフォント使用
                        AINameUI.font = AINameFont;
                }

                // —— レベル表示設定（有効時）——
                if (DisplayAILevel == YesOrNo.Yes)
                {
                    AILevelUI = HealthBar.transform.Find("AI Level Text").gameObject.GetComponent<Text>(); // レベル Text を取得
                    AILevelUI.text = "   " + AILevel.ToString();                 // 任意スペース＋数値
                    AILevelUI.color = LevelTextColor;                            // 色
                    AILevelUI.transform.localPosition = new Vector3(AILevelPos.x, AILevelPos.y, AILevelPos.z); // 位置

                    if (UseCustomFontAILevel == YesOrNo.Yes)                     // カスタムフォント
                        AILevelUI.font = AILevelFont;

                    // ※原典仕様：ここでは UseAINameUIOutlineEffect を参照し、AINameUI のアウトラインを再設定しています（レベル用の参照ではありません）。
                    if (UseAINameUIOutlineEffect == YesOrNo.Yes)
                    {
                        Outline AINameOutline = AINameUI.GetComponent<Outline>();
                        AINameOutline.effectDistance = AINameUIOutlineSize;
                        AINameOutline.effectColor = AINameUIOutlineColor;
                    }
                    else
                    {
                        AILevelUI.GetComponent<Outline>().enabled = false;       // レベル側のアウトラインを無効化
                    }
                }

                // 初期状態は非表示にしておき、検出更新で制御する
                HealthBarCanvasRef.enabled = false;
                if (AutoCreateHealthBars == YesOrNo.No)
                {
                    HealthBarBackgroundImageRef.gameObject.SetActive(false);     // 自動作成しない場合は背景を非表示
                }
                if (AINameUI != null && DisplayAIName == YesOrNo.Yes)
                {
                    AINameUI.gameObject.SetActive(false);                        // 名前UIを非表示
                }
                if (AILevelUI != null && DisplayAILevel == YesOrNo.Yes)
                {
                    AILevelUI.gameObject.SetActive(false);                       // レベルUIを非表示
                }
            }
        }

        /// <summary>
        /// （日本語）UI が有効な場合に、その表示状態を更新する。
        /// プレイヤーが検出範囲内にいるかを判定し、UI の表示/非表示を切り替える。
        /// </summary>
        void UpdateAIUI()
        {
            if (AutoCreateHealthBars == YesOrNo.Yes || DisplayAIName == YesOrNo.Yes) // いずれかのUI機能が有効なら
            {
                // 検出半径内で UILayerMask に該当するコライダーを収集
                Collider[] CurrentlyDetectedTargets = Physics.OverlapSphere(transform.position, EmeraldComponent.DetectionComponent.DetectionRadius, UILayerMask);
                if (CurrentlyDetectedTargets.Length > 0)
                {
                    List<Collider> TargetList = new List<Collider>();            // プレイヤーのみ抽出
                    for (int i = 0; i < CurrentlyDetectedTargets.Length; i++)
                    {
                        if (CurrentlyDetectedTargets[i].CompareTag(EmeraldComponent.DetectionComponent.PlayerTag))
                        {
                            TargetList.Add(CurrentlyDetectedTargets[i]);
                        }
                    }

                    if (TargetList.Count > 0) SetUI(true);                       // プレイヤーがいれば表示
                    else SetUI(false);                                           // いなければ非表示
                }
                else
                {
                    SetUI(false);                                                // 検出ゼロなら非表示
                }
            }
        }

        public void SetUI(bool Enabled)                 // UI の表示/非表示をまとめて切り替える
        {
            if (EmeraldComponent.AnimationComponent.IsDead || HealthBarCanvas == null) return; // 死亡中/未生成なら無処理

            m_HealthBarComponent.CalculateUI();          // スケール/カメラ向き等の再計算
            HealthBarCanvasRef.enabled = Enabled;        // Canvas の有効/無効

            if (AutoCreateHealthBars == YesOrNo.Yes)     // 自動作成した場合のみ本体のアクティブ切替
            {
                HealthBar.SetActive(Enabled);

                if (DisplayAILevel == YesOrNo.Yes)       // レベルUIの有効/無効
                {
                    AILevelUI.gameObject.SetActive(Enabled);
                }
            }

            if (DisplayAIName == YesOrNo.Yes)            // 名前UIの有効/無効
            {
                AINameUI.gameObject.SetActive(Enabled);
            }
        }
    }
}
