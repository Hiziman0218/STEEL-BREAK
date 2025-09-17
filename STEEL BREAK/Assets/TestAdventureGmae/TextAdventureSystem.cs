using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 外部テキストファイルからシナリオを読み取り、コマンドを実行するシステム
/// </summary>
public class TextAdventureSystem : MonoBehaviour
{
    [Header("テキストを表示するUI")]
    public Text m_DisplayText;
    [Header("画像表示用×3")]
    public List<Image> m_DisplayImage;
    [Header("リソース【画像】")]
    public string m_ImageResourcePath = "Images/";
    [Header("BGM用")]
    public AudioSource m_BgmSource;
    [Header("取り込んだシナリオ")]
    public Queue<string> m_Commands = new Queue<string>();
    [Header("左クリック待ちフラグ")]
    public bool m_WaitingForClick = false;

    public GameObject m_TEXTWindow;

    void Start()
    {
        // キャラクター画像を初期透明にする
        foreach(Image Dummy in m_DisplayImage)
           Dummy.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        // StreamingAssets内のシナリオファイルをロード
        LoadScenario(Application.streamingAssetsPath + "/scenario.txt");//はStreamingAssetsファイル

        // 最初のコマンドを実行
        NextCommand();
    }

    void Update()
    {
        // m_WaitingForClickがtrueの時、左クリックを検出
        if (m_WaitingForClick && Input.GetMouseButtonDown(0))
        {
            // 次のコマンドを実行
            NextCommand();
        }
    }

    /// <summary>
    /// 外部テキストファイル（シナリオ）を読み込む処理
    /// </summary>
    /// <param name="filePath">ファイルパス</param>
    void LoadScenario(string filePath)
    {
        if (!File.Exists(filePath))
        {
            // ファイルが存在しない場合エラーを表示
            Debug.LogError("シナリオファイルが見つからねぇよ!: " + filePath);
            return;
        }

        // ファイルを1行ずつ読み込む
        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            // 空行を除いてキューに追加
            if (!string.IsNullOrWhiteSpace(line))
                m_Commands.Enqueue(line.Trim());
        }
    }

    /// <summary>
    /// 次のコマンドをキューから取り出し、実行
    /// </summary>
    void NextCommand()
    {
        if (m_Commands.Count == 0)
        {
            // シナリオが終了した場合
            m_DisplayText.text = "シナリオ終了";
            m_WaitingForClick = false; // 待機解除
            return;
        }

        // キューから次のコマンドを取り出す
        string command = m_Commands.Dequeue();

        // コマンドを実行
        ExecuteCommand(command);
    }

    /// <summary>
    /// コマンドの内容に応じた処理を実行
    /// </summary>
    /// <param name="command">実行するコマンド文字列</param>
    void ExecuteCommand(string command)
    {
        #region "TEXT:" で始まる場合：テキスト表示
        // "TEXT:" で始まる場合：テキスト表示
        if (command.StartsWith("TEXT:"))
        {
            //5文字削除後、Messageを表示する
            TextRender(DataPatch(command, 5));
            // 左クリック待ちにする
            m_WaitingForClick = true;
        }
        #endregion

        else if (command.StartsWith("TEXTWindowCLS:"))
        {
            m_TEXTWindow.SetActive(false);
            m_WaitingForClick = true;

        }

        #region "TEXTCLS:" で始まる場合：文章消去
        else if (command.StartsWith("TEXTCLS:"))
        {
            //5文字削除後、Messageを表示する
            TextClear();
            // 左クリック待ちにする
            m_WaitingForClick = true;
        }
        #endregion
        #region "SHOW_IMAGE_S:" で始まる場合：画像表示
        // "SHOW_IMAGE_S:" で始まる場合：画像表示
        else if (command.StartsWith("SHOW_IMAGE_S:"))
        {
            //13文字削除後、キャライメージ【中央】を表示する
            ImageCharacterRender(DataPatch(command, 13), 0);
            // 画像表示後は自動進行
            NextCommand();
        }
        #endregion
        #region "SHOW_IMAGE_R:" で始まる場合：画像表示
        // "SHOW_IMAGE_R:" で始まる場合：画像表示
        else if (command.StartsWith("SHOW_IMAGE_R:"))
        {

            //13文字削除後、キャライメージ【右】を表示する
            ImageCharacterRender(DataPatch(command, 13), 1);
            // 画像表示後は自動進行
            NextCommand();
        }
        #endregion
        #region "SHOW_IMAGE_L:" で始まる場合：画像表示
        // "SHOW_IMAGE_L:" で始まる場合：画像表示
        else if (command.StartsWith("SHOW_IMAGE_L:"))
        {
            //13文字削除後、キャライメージ【左】を表示する
            ImageCharacterRender(DataPatch(command, 13), 2);
            // 画像表示後は自動進行
            NextCommand();
        }
        #endregion
        #region "PLAY_BGM:" で始まる場合：BGM再生
        // "PLAY_BGM:" で始まる場合：BGM再生
        else if (command.StartsWith("PLAY_BGM:"))
        {
            // "PLAY_BGM:"以降の文字列
            string bgmName = command.Substring(9);
            //9文字削除後、指定したバックミュージックを流す
            BackGroundMusic(DataPatch(command, 9));
            // BGM再生後は自動進行
            NextCommand();
        }
        #endregion
        #region "SHOW_IMAGE_S_CLS:" で始まる場合：画像表示
        // "SHOW_IMAGE_S_CLS:" で始まる場合：画像表示
        else if (command.StartsWith("SHOW_IMAGE_S_CLS:"))
        {
            // 中央イメージキャラクターパネルを消す
            ImageCharacterCLS(0);
            // 画像表示後は自動進行
            NextCommand();
        }
        #endregion
        #region "SHOW_IMAGE_R_CLS:" で始まる場合：画像表示
        // "SHOW_IMAGE_R_CLS:" で始まる場合：画像表示
        else if (command.StartsWith("SHOW_IMAGE_R_CLS:"))
        {
            // 中央イメージキャラクターパネルを消す
            ImageCharacterCLS(1);
            // 画像表示後は自動進行
            NextCommand();
        }
        #endregion
        #region "SHOW_IMAGE_L_CLS:" で始まる場合：画像表示
        // "SHOW_IMAGE_L_CLS:" で始まる場合：画像表示
        else if (command.StartsWith("SHOW_IMAGE_L_CLS:"))
        {
            // 中央イメージキャラクターパネルを消す
            ImageCharacterCLS(2);
            // 画像表示後は自動進行
            NextCommand();
        }
        #endregion
        #region その他
        else
        {
            // 未知のコマンド
            Debug.LogWarning("未知のコマンドにゃ?: " + command);
            // 不明なコマンドはスキップ
            NextCommand();
        }
        #endregion
    }
    /// <summary>
    /// 削除文字数文削除した後の文字を返す
    /// </summary>
    /// <param name="Message">送られた文字データ</param>
    /// <param name="Count">削除する文字数</param>
    /// <returns></returns>
    public string DataPatch(string Message, int Count)
    {
        //指定した文字列数を削除して返す
        return Message.Substring(Count);
    }
    /// <summary>
    /// Messageをメッセージウィンドゥに表示する
    /// </summary>
    /// <param name="Message"></param>
    public void TextRender(string Message)
    {
        // テキストを表示
        m_DisplayText.text = Message;
        // 左クリック待ちにする
        m_WaitingForClick = true;

    }

    public void TextClear()
    {
        // テキストを表示
        m_DisplayText.text = " ";
        // 左クリック待ちにする
        m_WaitingForClick = true;
    }
    /// <summary>
    /// キャラクターイメージをresourceから取り出して指定した場所に表示する
    /// </summary>
    /// <param name="Message">resourceフォルダから取り出したい画像名</param>
    /// <param name="No">表示したいパネル番号[0.中央、1.右、2.左]</param>
    public void ImageCharacterRender(string Message,int No)
    {
        // Resourcesから画像読み込み
        Sprite sprite = Resources.Load<Sprite>(m_ImageResourcePath + Message);
        // 表示先があるかどうか
        if (sprite != null)
        {
            //画像(Sprite)を代入
            m_DisplayImage[No].sprite = sprite;
            //画像のα値をMaxにする
            m_DisplayImage[No].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        else
            Debug.LogWarning("画像が見つからん!: " + Message);
    }
    public void BackGroundMusic(string Message)
    {
        // ResourcesからBGM読み込み
        AudioClip clip = Resources.Load<AudioClip>("Audio/" + Message);
        if (clip != null)
        {
            //読み込んだBGMを代入
            m_BgmSource.clip = clip;
            // BGM再生
            m_BgmSource.Play();
        }
        else
        {
            Debug.LogWarning("BGMが見つからん!: " + Message);
        }
    }

    /// <summary>
    /// イメージキャラクターパネルの初期化
    /// </summary>
    /// <param name="No">消したいイメージキャラクターパネル番号</param>
    public void ImageCharacterCLS(int No)
    {
        //該当画像を削除
        m_DisplayImage[No].sprite = null;
        //α値を0にする(透明化)
        m_DisplayImage[No].color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }
}
