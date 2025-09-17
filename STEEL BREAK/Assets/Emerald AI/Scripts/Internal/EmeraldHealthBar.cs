using System.Collections;                         // コルーチン（IEnumerator）を利用するため
using UnityEngine;                                // Unity の基本API
using UnityEngine.UI;                             // UI（Image, Text, Canvas 等）を利用するため

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【EmeraldHealthBar】
    /// AIの体力バー（UI）を制御するクラスです。
    /// ・カメラ方向へビルボード表示
    /// ・距離に応じたスケール調整
    /// ・ダメージ/回復時のアニメーション（遅れて減少する赤ゲージ等）
    /// ・死亡時のフェードアウト
    /// これらは EmeraldHealth/EmeraldUI/EmeraldSystem からのイベント購読と参照で動作します。
    /// </summary>
    public class EmeraldHealthBar : MonoBehaviour
    {
        #region Health Bar Variables                      // ヘルスバー関連の変数群

        [Header("カメラ参照。UIをカメラに正対させ、距離スケールを計算するために使用")]
        Camera m_Camera;                                  // メインカメラ（EmeraldUI.CameraTagで検索して取得）

        [Header("このオブジェクトに付与された Canvas。スケール/アルファ制御で使用")]
        Canvas canvas;                                    // 体力バー用の Canvas

        [Header("現在HPを示す前景ゲージ（緑など）。fillAmount を直接更新")]
        Image HealthBar;                                  // 実HP表示用ゲージ

        [Header("ダメージ遅延表示用の後退ゲージ（赤）。時間差で HealthBar に追随")]
        Image HealthBarDamage;                            // 遅延ダメージ表示用ゲージ

        [Header("CanvasGroup。UIのフェード（アルファ）制御に使用")]
        CanvasGroup CG;                                   // アルファ制御

        [Header("AIの名前を表示する Text UI")]
        Text AINameUI;                                    // 名前表示

        [Header("AIのレベルを表示する Text UI")]
        Text AILevelUI;                                   // レベル表示

        [Header("フェードアウト等の処理に使用するコルーチン参照")]
        Coroutine C;                                      // フェード等の進行管理

        [Header("AIの戦闘状態等を参照するための EmeraldSystem コンポーネント")]
        EmeraldSystem EmeraldComponent;                   // 戦闘状態の参照に使用

        [Header("UI設定（スケール最大値、カメラタグ 等）を保持する EmeraldUI コンポーネント")]
        EmeraldUI EmeraldUI;                              // UI設定の参照に使用

        [Header("HP値やイベント（OnDeath/OnTakeDamage 等）を提供する EmeraldHealth コンポーネント")]
        EmeraldHealth EmeraldHealth;                      // 体力・イベント元

        [Header("ダメージ遷移（後退ゲージの追従）用のコルーチン参照")]
        Coroutine CoroutineTransitionDamage;              // ダメージ遷移用コルーチン

        #endregion

        void Start()
        {
            InitializeHealthBar();                        // ヘルスバー初期化を実行
        }

        /// <summary>
        /// ヘルスバーの初期化を行います。
        /// ・主要コンポーネントの参照取得
        /// ・イベント購読設定（死亡/被ダメージ/回復/HP変化）
        /// ・カメラ参照の確立
        /// </summary>
        void InitializeHealthBar()
        {
            // ヘルスバーのTransform階層は固定前提のため、親の親から Emerald 系コンポーネントを取得
            canvas = GetComponent<Canvas>();                                                      // この UI の Canvas
            EmeraldUI = transform.parent.parent.GetComponent<EmeraldUI>();                       // UI設定
            EmeraldHealth = transform.parent.parent.GetComponent<EmeraldHealth>();               // 体力とイベント
            EmeraldComponent = transform.parent.parent.GetComponent<EmeraldSystem>();            // 戦闘状態など

            if (m_Camera == null)                                                                // カメラ参照未設定なら
                m_Camera = GameObject.FindGameObjectWithTag(EmeraldUI.CameraTag).GetComponent<Camera>(); // EmeraldUI.CameraTag を用いてカメラを検索・取得

            CG = GetComponent<CanvasGroup>();                                                    // フェード用 CanvasGroup
            HealthBar = transform.Find("AI Health Bar Background/AI Health Bar").GetComponent<Image>();                 // 前景ゲージ
            HealthBarDamage = transform.Find("AI Health Bar Background/AI Health Bar (Damage)").GetComponent<Image>();  // 後退ゲージ
            AINameUI = transform.Find("AI Name Text").GetComponent<Text>();                      // 名前表示
            AILevelUI = transform.Find("AI Level Text").GetComponent<Text>();                    // レベル表示

            EmeraldHealth.OnDeath += FadeOutUI;                  // 死亡時：UIをフェードアウト（OnDeath デリゲートへ購読）
            EmeraldHealth.OnTakeDamage += TransitionDamage;      // 被ダメージ時：ダメージ遷移を開始
            EmeraldHealth.OnTakeCritDamage += TransitionDamage;  // クリティカル被ダメージ時：同上
            EmeraldHealth.OnHealRateTick += TransitionHealing;   // 回復Tick時：回復遷移を開始
            EmeraldHealth.OnHealthChange += UpdateHealthUI;      // HP変化時：即時UI更新
        }

        void Update()
        {
            CalculateUI();                                   // 毎フレーム、ビルボード化と距離スケールを計算
        }

        /// <summary>
        /// UIの向き（カメラへ正対）と距離スケールを計算・適用します。
        /// </summary>
        public void CalculateUI()
        {
            if (m_Camera != null)                            // カメラが有効な場合のみ
            {
                if (HealthBar != null)                       // ゲージ参照が有効な場合のみ
                {
                    // 親（Canvasの親）をカメラ方向へ向ける（ビルボード）
                    canvas.transform.parent.LookAt(
                        canvas.transform.parent.position + m_Camera.transform.rotation * Vector3.forward,
                        m_Camera.transform.rotation * Vector3.up
                    );

                    // カメラ距離からスケールを算出し、上限値（MaxUIScaleSize）を超えないように調整
                    float dist = Vector3.Distance(m_Camera.transform.position, transform.position);
                    if (dist < EmeraldUI.MaxUIScaleSize)
                    {
                        canvas.transform.localScale = new Vector3(dist * 0.085f, dist * 0.085f, dist * 0.085f);
                    }
                    else
                    {
                        canvas.transform.localScale = new Vector3(EmeraldUI.MaxUIScaleSize * 0.085f, EmeraldUI.MaxUIScaleSize * 0.085f, EmeraldUI.MaxUIScaleSize * 0.085f);
                    }
                }
            }
        }

        /// <summary>
        /// UIをフェードアウトします（EmeraldHealth.OnDeath により自動で呼び出されます）。
        /// </summary>
        void FadeOutUI()
        {
            if (gameObject.activeSelf)                       // オブジェクトが有効なときのみ
            {
                if (C != null) { StopCoroutine(C); }         // 進行中のフェードを停止
                C = StartCoroutine(FadeOutUIInternal(0.0f, 1.5f)); // 目標アルファ0, 1.5秒でフェード
            }
        }

        void OnDisable()
        {
            // 戦闘中ではない場合に UI 値を初期状態へ戻す
            if (EmeraldComponent != null && !EmeraldComponent.CombatComponent.CombatState) ResetValues(); // 既定値へリセット
        }

        /// <summary>
        /// UIフェードアウトの内部処理（コルーチン）。
        /// </summary>
        IEnumerator FadeOutUIInternal(float DesiredValue, float TransitionTime)
        {
            // 現在HPを反映（死亡直前などで乖離しないよう更新）
            HealthBar.fillAmount = ((float)EmeraldHealth.Health / (float)EmeraldHealth.StartHealth);

            float alpha = CG.alpha;                           // 現在のアルファ
            float t = 0;                                      // 経過時間

            while ((t / TransitionTime) < 1)                  // 規定時間まで補間
            {
                t += Time.deltaTime;
                Color newColor1 = new Color(1, 1, 1, Mathf.Lerp(alpha, DesiredValue, t)); // アルファを補間
                CG.alpha = newColor1.a;                       // CanvasGroup のアルファ反映
                AINameUI.color = new Color(AINameUI.color.r, AINameUI.color.g, AINameUI.color.b, newColor1.a);   // 名前表示のアルファ
                AILevelUI.color = new Color(AILevelUI.color.r, AILevelUI.color.g, AILevelUI.color.b, newColor1.a); // レベル表示のアルファ
                yield return null;
            }

            gameObject.SetActive(false);                      // 完了後、非表示（無効化）
        }

        /// <summary>
        /// 被ダメージ時の処理：後退ゲージを時間差で追随させる演出を開始します。
        /// </summary>
        void TransitionDamage()
        {
            if (gameObject.activeSelf)                        // UIが表示状態ならアニメーション
            {
                if (CoroutineTransitionDamage != null) StopCoroutine(CoroutineTransitionDamage); // 多重開始を防止
                CoroutineTransitionDamage = StartCoroutine(TransitionDamageInternal());           // 遅延追随を開始
            }
            else                                               // 非表示状態なら数値だけ同期
            {
                HealthBar.fillAmount = ((float)EmeraldHealth.Health / (float)EmeraldHealth.StartHealth);
                HealthBarDamage.fillAmount = HealthBar.fillAmount;
            }
        }

        /// <summary>
        /// 被ダメージ時の内部処理（コルーチン）。0.75秒待機後、後退ゲージが現在HPへ滑らかに追随。
        /// </summary>
        IEnumerator TransitionDamageInternal()
        {
            HealthBar.fillAmount = ((float)EmeraldHealth.Health / (float)EmeraldHealth.StartHealth); // 先に実HPゲージを反映
            float Start = HealthBarDamage.fillAmount;        // 後退ゲージの開始値
            float t = 0;                                     // 経過時間
            yield return new WaitForSeconds(0.75f);          // 少し待ってから減少演出

            while ((t / 1f) < 1)                             // 1秒で補間
            {
                t += Time.deltaTime;
                HealthBarDamage.fillAmount = Mathf.Lerp(Start, HealthBar.fillAmount, t); // 後退ゲージを追随
                yield return null;
            }
        }

        /// <summary>
        /// 回復時の処理：ヘルスバーを滑らかに増加させます。
        /// </summary>
        void TransitionHealing()
        {
            if (gameObject.activeSelf)                        // 表示中のみアニメーション
            {
                StartCoroutine(TransitionHealingInternal());
            }
            else                                               // 非表示状態なら数値だけ同期
            {
                HealthBar.fillAmount = ((float)EmeraldHealth.Health / (float)EmeraldHealth.StartHealth);
                HealthBarDamage.fillAmount = HealthBar.fillAmount;
            }
        }

        /// <summary>
        /// AIの現在HPに基づき、体力バー（前景/後退）を即時更新します。
        /// </summary>
        void UpdateHealthUI()
        {
            HealthBar.fillAmount = ((float)EmeraldHealth.Health / (float)EmeraldHealth.StartHealth); // 前景ゲージ
            HealthBarDamage.fillAmount = HealthBar.fillAmount;                                       // 後退ゲージも同期
        }

        /// <summary>
        /// 回復時アニメーションの内部処理（コルーチン）。1秒間で現在値→回復後の値へ補間。
        /// </summary>
        IEnumerator TransitionHealingInternal()
        {
            float HealAmount = ((float)EmeraldHealth.Health / (float)EmeraldHealth.StartHealth); // 目標値（回復後）
            float Start = HealthBar.fillAmount;                                                  // 現在値
            float t = 0;                                                                         // 経過時間

            while ((t / 1f) < 1)                                                                 // 1秒で補間
            {
                t += Time.deltaTime;
                HealthBar.fillAmount = Mathf.Lerp(Start, HealAmount, t);                         // 前景ゲージ
                HealthBarDamage.fillAmount = Mathf.Lerp(Start, HealAmount, t);                   // 後退ゲージも同時に追随（回復時は同調）
                yield return null;
            }
        }

        /// <summary>
        /// UIの表示値を既定値（フルHP・完全不透明）へ戻します。
        /// </summary>
        void ResetValues()
        {
            if (CG != null)
            {
                Color newColor1 = new Color(1, 1, 1, 1);                   // 完全不透明
                CG.alpha = newColor1.a;                                    // CanvasGroup のアルファを1へ
                AINameUI.color = new Color(AINameUI.color.r, AINameUI.color.g, AINameUI.color.b, newColor1.a);   // 名前表示のアルファ復帰
                AILevelUI.color = new Color(AILevelUI.color.r, AILevelUI.color.g, AILevelUI.color.b, newColor1.a); // レベル表示のアルファ復帰
                HealthBar.fillAmount = 1;                                  // 前景ゲージを満タンへ
                HealthBarDamage.fillAmount = 1;                            // 後退ゲージも満タンへ
            }
        }
    }
}
