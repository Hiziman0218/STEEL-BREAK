using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// �O���e�L�X�g�t�@�C������V�i���I��ǂݎ��A�R�}���h�����s����V�X�e��
/// </summary>
public class TextAdventureSystem : MonoBehaviour
{
    [Header("�e�L�X�g��\������UI")]
    public Text m_DisplayText;
    [Header("�摜�\���p�~3")]
    public List<Image> m_DisplayImage;
    [Header("���\�[�X�y�摜�z")]
    public string m_ImageResourcePath = "Images/";
    [Header("BGM�p")]
    public AudioSource m_BgmSource;
    [Header("��荞�񂾃V�i���I")]
    public Queue<string> m_Commands = new Queue<string>();
    [Header("���N���b�N�҂��t���O")]
    public bool m_WaitingForClick = false;

    public GameObject m_TEXTWindow;

    void Start()
    {
        // �L�����N�^�[�摜�����������ɂ���
        foreach(Image Dummy in m_DisplayImage)
           Dummy.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        // StreamingAssets���̃V�i���I�t�@�C�������[�h
        LoadScenario(Application.streamingAssetsPath + "/scenario.txt");//��StreamingAssets�t�@�C��

        // �ŏ��̃R�}���h�����s
        NextCommand();
    }

    void Update()
    {
        // m_WaitingForClick��true�̎��A���N���b�N�����o
        if (m_WaitingForClick && Input.GetMouseButtonDown(0))
        {
            // ���̃R�}���h�����s
            NextCommand();
        }
    }

    /// <summary>
    /// �O���e�L�X�g�t�@�C���i�V�i���I�j��ǂݍ��ޏ���
    /// </summary>
    /// <param name="filePath">�t�@�C���p�X</param>
    void LoadScenario(string filePath)
    {
        if (!File.Exists(filePath))
        {
            // �t�@�C�������݂��Ȃ��ꍇ�G���[��\��
            Debug.LogError("�V�i���I�t�@�C����������˂���!: " + filePath);
            return;
        }

        // �t�@�C����1�s���ǂݍ���
        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            // ��s�������ăL���[�ɒǉ�
            if (!string.IsNullOrWhiteSpace(line))
                m_Commands.Enqueue(line.Trim());
        }
    }

    /// <summary>
    /// ���̃R�}���h���L���[������o���A���s
    /// </summary>
    void NextCommand()
    {
        if (m_Commands.Count == 0)
        {
            // �V�i���I���I�������ꍇ
            m_DisplayText.text = "�V�i���I�I��";
            m_WaitingForClick = false; // �ҋ@����
            return;
        }

        // �L���[���玟�̃R�}���h�����o��
        string command = m_Commands.Dequeue();

        // �R�}���h�����s
        ExecuteCommand(command);
    }

    /// <summary>
    /// �R�}���h�̓��e�ɉ��������������s
    /// </summary>
    /// <param name="command">���s����R�}���h������</param>
    void ExecuteCommand(string command)
    {
        #region "TEXT:" �Ŏn�܂�ꍇ�F�e�L�X�g�\��
        // "TEXT:" �Ŏn�܂�ꍇ�F�e�L�X�g�\��
        if (command.StartsWith("TEXT:"))
        {
            //5�����폜��AMessage��\������
            TextRender(DataPatch(command, 5));
            // ���N���b�N�҂��ɂ���
            m_WaitingForClick = true;
        }
        #endregion

        else if (command.StartsWith("TEXTWindowCLS:"))
        {
            m_TEXTWindow.SetActive(false);
            m_WaitingForClick = true;

        }

        #region "TEXTCLS:" �Ŏn�܂�ꍇ�F���͏���
        else if (command.StartsWith("TEXTCLS:"))
        {
            //5�����폜��AMessage��\������
            TextClear();
            // ���N���b�N�҂��ɂ���
            m_WaitingForClick = true;
        }
        #endregion
        #region "SHOW_IMAGE_S:" �Ŏn�܂�ꍇ�F�摜�\��
        // "SHOW_IMAGE_S:" �Ŏn�܂�ꍇ�F�摜�\��
        else if (command.StartsWith("SHOW_IMAGE_S:"))
        {
            //13�����폜��A�L�����C���[�W�y�����z��\������
            ImageCharacterRender(DataPatch(command, 13), 0);
            // �摜�\����͎����i�s
            NextCommand();
        }
        #endregion
        #region "SHOW_IMAGE_R:" �Ŏn�܂�ꍇ�F�摜�\��
        // "SHOW_IMAGE_R:" �Ŏn�܂�ꍇ�F�摜�\��
        else if (command.StartsWith("SHOW_IMAGE_R:"))
        {

            //13�����폜��A�L�����C���[�W�y�E�z��\������
            ImageCharacterRender(DataPatch(command, 13), 1);
            // �摜�\����͎����i�s
            NextCommand();
        }
        #endregion
        #region "SHOW_IMAGE_L:" �Ŏn�܂�ꍇ�F�摜�\��
        // "SHOW_IMAGE_L:" �Ŏn�܂�ꍇ�F�摜�\��
        else if (command.StartsWith("SHOW_IMAGE_L:"))
        {
            //13�����폜��A�L�����C���[�W�y���z��\������
            ImageCharacterRender(DataPatch(command, 13), 2);
            // �摜�\����͎����i�s
            NextCommand();
        }
        #endregion
        #region "PLAY_BGM:" �Ŏn�܂�ꍇ�FBGM�Đ�
        // "PLAY_BGM:" �Ŏn�܂�ꍇ�FBGM�Đ�
        else if (command.StartsWith("PLAY_BGM:"))
        {
            // "PLAY_BGM:"�ȍ~�̕�����
            string bgmName = command.Substring(9);
            //9�����폜��A�w�肵���o�b�N�~���[�W�b�N�𗬂�
            BackGroundMusic(DataPatch(command, 9));
            // BGM�Đ���͎����i�s
            NextCommand();
        }
        #endregion
        #region "SHOW_IMAGE_S_CLS:" �Ŏn�܂�ꍇ�F�摜�\��
        // "SHOW_IMAGE_S_CLS:" �Ŏn�܂�ꍇ�F�摜�\��
        else if (command.StartsWith("SHOW_IMAGE_S_CLS:"))
        {
            // �����C���[�W�L�����N�^�[�p�l��������
            ImageCharacterCLS(0);
            // �摜�\����͎����i�s
            NextCommand();
        }
        #endregion
        #region "SHOW_IMAGE_R_CLS:" �Ŏn�܂�ꍇ�F�摜�\��
        // "SHOW_IMAGE_R_CLS:" �Ŏn�܂�ꍇ�F�摜�\��
        else if (command.StartsWith("SHOW_IMAGE_R_CLS:"))
        {
            // �����C���[�W�L�����N�^�[�p�l��������
            ImageCharacterCLS(1);
            // �摜�\����͎����i�s
            NextCommand();
        }
        #endregion
        #region "SHOW_IMAGE_L_CLS:" �Ŏn�܂�ꍇ�F�摜�\��
        // "SHOW_IMAGE_L_CLS:" �Ŏn�܂�ꍇ�F�摜�\��
        else if (command.StartsWith("SHOW_IMAGE_L_CLS:"))
        {
            // �����C���[�W�L�����N�^�[�p�l��������
            ImageCharacterCLS(2);
            // �摜�\����͎����i�s
            NextCommand();
        }
        #endregion
        #region ���̑�
        else
        {
            // ���m�̃R�}���h
            Debug.LogWarning("���m�̃R�}���h�ɂ�?: " + command);
            // �s���ȃR�}���h�̓X�L�b�v
            NextCommand();
        }
        #endregion
    }
    /// <summary>
    /// �폜���������폜������̕�����Ԃ�
    /// </summary>
    /// <param name="Message">����ꂽ�����f�[�^</param>
    /// <param name="Count">�폜���镶����</param>
    /// <returns></returns>
    public string DataPatch(string Message, int Count)
    {
        //�w�肵�������񐔂��폜���ĕԂ�
        return Message.Substring(Count);
    }
    /// <summary>
    /// Message�����b�Z�[�W�E�B���h�D�ɕ\������
    /// </summary>
    /// <param name="Message"></param>
    public void TextRender(string Message)
    {
        // �e�L�X�g��\��
        m_DisplayText.text = Message;
        // ���N���b�N�҂��ɂ���
        m_WaitingForClick = true;

    }

    public void TextClear()
    {
        // �e�L�X�g��\��
        m_DisplayText.text = " ";
        // ���N���b�N�҂��ɂ���
        m_WaitingForClick = true;
    }
    /// <summary>
    /// �L�����N�^�[�C���[�W��resource������o���Ďw�肵���ꏊ�ɕ\������
    /// </summary>
    /// <param name="Message">resource�t�H���_������o�������摜��</param>
    /// <param name="No">�\���������p�l���ԍ�[0.�����A1.�E�A2.��]</param>
    public void ImageCharacterRender(string Message,int No)
    {
        // Resources����摜�ǂݍ���
        Sprite sprite = Resources.Load<Sprite>(m_ImageResourcePath + Message);
        // �\���悪���邩�ǂ���
        if (sprite != null)
        {
            //�摜(Sprite)����
            m_DisplayImage[No].sprite = sprite;
            //�摜�̃��l��Max�ɂ���
            m_DisplayImage[No].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        else
            Debug.LogWarning("�摜���������!: " + Message);
    }
    public void BackGroundMusic(string Message)
    {
        // Resources����BGM�ǂݍ���
        AudioClip clip = Resources.Load<AudioClip>("Audio/" + Message);
        if (clip != null)
        {
            //�ǂݍ���BGM����
            m_BgmSource.clip = clip;
            // BGM�Đ�
            m_BgmSource.Play();
        }
        else
        {
            Debug.LogWarning("BGM���������!: " + Message);
        }
    }

    /// <summary>
    /// �C���[�W�L�����N�^�[�p�l���̏�����
    /// </summary>
    /// <param name="No">���������C���[�W�L�����N�^�[�p�l���ԍ�</param>
    public void ImageCharacterCLS(int No)
    {
        //�Y���摜���폜
        m_DisplayImage[No].sprite = null;
        //���l��0�ɂ���(������)
        m_DisplayImage[No].color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }
}
