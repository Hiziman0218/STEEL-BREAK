using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [Header("ロゴ関連")]
    public Image logoImage;                  //最初のロゴ表示用 Image
    public Sprite logoBefore;                //撃たれる前のロゴスプライト
    public Sprite logoAfter;                 //撃たれた後のロゴスプライト
    public float logoDisplayDelay = 1.0f;    //ロゴが完全表示されてから次の処理までの待ち時間

    [Header("フェード用 CanvasGroup")]
    public CanvasGroup fadePanel_black;      //全画面フェード用の黒い Panel に付ける CanvasGroup
    public CanvasGroup fadePanel_white;      //全画面発砲演出用の白い Panel に付ける CanvasGroup
    public float fadeDuration_Start = 1.0f;  //フェードイン・フェードアウトの時間
    public float fadeDuration_Button = 1.0f; //フェードイン・フェードアウトの時間

    [Header("ボタン群")]
    public CanvasGroup buttonGroup;          //ボタン群一式をまとめた CanvasGroup（最初は alpha=0）

    [Header("SE 用 AudioSource")]
    public AudioSource seSource;             //発砲音を再生する AudioSource

    private Coroutine playSequence;          //再生中のコルーチン参照
    private bool skipRequested = false;      //スキップキーが押されたかどうか

    void Start()
    {
        // 初期状態をセット
        logoImage.sprite = logoBefore;
        fadePanel_black.alpha = 1f; // 最初は真っ黒
        fadePanel_white.alpha = 0f; // 最初は透明
        buttonGroup.alpha = 0f;     // ボタン非表示

        // コルーチンで順次演出を実行
        playSequence = StartCoroutine(PlayTitleSequence());
    }

    void Update()
    {
        //任意キーでスキップフラグを立てる
        if (!skipRequested && /*Input.GetKeyDown(KeyCode.Space)*/ Input.anyKeyDown)
        {
            skipRequested = true;
        }
    }

    private IEnumerator PlayTitleSequence()
    {
        // 1) 黒→透明フェードイン：2 秒かけて黒から消す
        yield return StartCoroutine(FadeCanvasGroup(fadePanel_black, 1f, 0f, fadeDuration_Start));

        // 2) ロゴ（logoBefore）を完全表示 → 少し待機
        yield return WaitOrSkip(logoDisplayDelay);

        // 3) 発砲 SE ＋ 白フラッシュ
        seSource.Play(); // SE 再生
        // フラッシュ用に一瞬 alpha を 1 にして、すぐ戻す
        fadePanel_white.alpha = 1f;
        yield return WaitOrSkip(0.1f); // 0.1 秒ほど見せる
        fadePanel_white.alpha = 0f;

        // 4) ロゴスプライト差し替え（撃たれた後のロゴ）
        logoImage.sprite = logoAfter;

        // 5) ロゴを画面中央→上部へ移動（RectTransform の Y 座標を補間）
        RectTransform rt = logoImage.rectTransform;
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, startPos.y + 60f); // 好きな移動量
        float moveTime = 0.8f;
        float t = 0f;
        while (t < moveTime)
        {
            if (skipRequested) break; // スキップされたら即時終了
            t += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t / moveTime);
            yield return null;
        }
        rt.anchoredPosition = endPos; // 最終位置を担保

        // 6) ボタン群をフェードイン（0→1）させる
        yield return StartCoroutine(FadeCanvasGroup(buttonGroup, 0f, 1f, fadeDuration_Button));

        // ここまで来たら演出完了
    }

    /// <summary>
    /// CanvasGroup の α を from→to に fadeTime 秒かけて補間するコルーチン
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float fadeTime)
    {
        float t = 0f;
        cg.alpha = from;
        while (t < fadeTime)
        {
            if (skipRequested) { cg.alpha = to; yield break; } // スキップされたら即時最終値に
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        cg.alpha = to;
    }

    /// <summary>
    /// 秒数待つか、スキップキー押下されたら即座に戻るヘルパー
    /// </summary>
    private IEnumerator WaitOrSkip(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            if (skipRequested) yield break;
            t += Time.deltaTime;
            yield return null;
        }
    }

    // ゲームスタートボタンから呼び出す関数
    public void OnClickGameStart()
    {
        //ゲームシーンを生成
        SceneHistoryManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
