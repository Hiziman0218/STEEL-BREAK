using System;
using UnityEngine;
using TMPro;
using System.Collections;

public class TypeWriterEffect : MonoBehaviour
{
    // --- イベント ---
    public event Action OnTypingFinished;     // 全てのメッセージを表示し終わったときに通知
    public event Action<int> OnMessageChanged; // 新しいメッセージを表示するタイミングで通知（インデックス付き）

    [SerializeField] private TextMeshProUGUI textComponent; // メッセージ表示用のTextMeshProUGUI
    [SerializeField] private float typingSpeed = 0.05f;     // 1文字ずつ表示する間隔（秒）

    private string[] messages;           // 表示するメッセージの配列
    private int currentMessageIndex = 0; // 現在表示しているメッセージのインデックス

    private Coroutine typingCoroutine;   // 実行中のコルーチンを保持するための変数
    private bool isTyping = false;       // 現在タイピング中かどうか
    private string currentMessage;       // 現在表示中のメッセージ（スキップ用）

    /// <summary>
    /// メッセージ配列を受け取り、最初のメッセージ表示を開始する
    /// </summary>
    public void StartTyping(string[] messages)
    {
        this.messages = messages;
        currentMessageIndex = 0;

        // メッセージが無い場合は即終了
        if (messages == null || messages.Length == 0)
        {
            OnTypingFinished?.Invoke();  // イベントを発火
            return;
        }

        ShowNextMessage();
    }

    /// <summary>
    /// 次のメッセージを表示する
    /// </summary>
    private void ShowNextMessage()
    {
        // 全メッセージを表示し終わったら終了イベント
        if (currentMessageIndex >= messages.Length)
        {
            OnTypingFinished?.Invoke();
            return;
        }

        currentMessage = messages[currentMessageIndex];

        // メッセージが切り替わったことを通知（ボイス再生などに利用）
        OnMessageChanged?.Invoke(currentMessageIndex);

        currentMessageIndex++;

        // 既にコルーチンが動いていたら止める（次のメッセージに切り替え）
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // 1文字ずつ表示するコルーチンを開始
        typingCoroutine = StartCoroutine(TypeMessageCoroutine(currentMessage));
    }

    /// <summary>
    /// メッセージを1文字ずつ表示するコルーチン
    /// </summary>
    private IEnumerator TypeMessageCoroutine(string message)
    {
        isTyping = true;
        textComponent.text = "";

        foreach (char c in message)
        {
            textComponent.text += c;                // 1文字追加
            yield return new WaitForSeconds(typingSpeed); // 指定時間待機
        }

        isTyping = false; // 打ち終わったら解除
    }

    /// <summary>
    /// プレイヤーがクリック/キー入力したときの挙動
    /// </summary>
    public void OnUserClicked()
    {
        if (isTyping)
        {
            // タイピング中なら即表示（スキップ）
            StopCoroutine(typingCoroutine);
            textComponent.text = currentMessage;
            isTyping = false;
        }
        else
        {
            // タイピングが終わっていたら次のメッセージへ
            ShowNextMessage();
        }
    }

    private void Update()
    {
        // スペースキーが押されたらクリック扱い
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnUserClicked();
        }
    }
}
