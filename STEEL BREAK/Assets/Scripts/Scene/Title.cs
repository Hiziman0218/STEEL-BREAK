using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [Header("���S�֘A")]
    public Image logoImage;                  //�ŏ��̃��S�\���p Image
    public Sprite logoBefore;                //�������O�̃��S�X�v���C�g
    public Sprite logoAfter;                 //�����ꂽ��̃��S�X�v���C�g
    public float logoDisplayDelay = 1.0f;    //���S�����S�\������Ă��玟�̏����܂ł̑҂�����

    [Header("�t�F�[�h�p CanvasGroup")]
    public CanvasGroup fadePanel_black;      //�S��ʃt�F�[�h�p�̍��� Panel �ɕt���� CanvasGroup
    public CanvasGroup fadePanel_white;      //�S��ʔ��C���o�p�̔��� Panel �ɕt���� CanvasGroup
    public float fadeDuration_Start = 1.0f;  //�t�F�[�h�C���E�t�F�[�h�A�E�g�̎���
    public float fadeDuration_Button = 1.0f; //�t�F�[�h�C���E�t�F�[�h�A�E�g�̎���

    [Header("�{�^���Q")]
    public CanvasGroup buttonGroup;          //�{�^���Q�ꎮ���܂Ƃ߂� CanvasGroup�i�ŏ��� alpha=0�j

    [Header("SE �p AudioSource")]
    public AudioSource seSource;             //���C�����Đ����� AudioSource

    private Coroutine playSequence;          //�Đ����̃R���[�`���Q��
    private bool skipRequested = false;      //�X�L�b�v�L�[�������ꂽ���ǂ���

    void Start()
    {
        // ������Ԃ��Z�b�g
        logoImage.sprite = logoBefore;
        fadePanel_black.alpha = 1f; // �ŏ��͐^����
        fadePanel_white.alpha = 0f; // �ŏ��͓���
        buttonGroup.alpha = 0f;     // �{�^����\��

        // �R���[�`���ŏ������o�����s
        playSequence = StartCoroutine(PlayTitleSequence());
    }

    void Update()
    {
        //�C�ӃL�[�ŃX�L�b�v�t���O�𗧂Ă�
        if (!skipRequested && /*Input.GetKeyDown(KeyCode.Space)*/ Input.anyKeyDown)
        {
            skipRequested = true;
        }
    }

    private IEnumerator PlayTitleSequence()
    {
        // 1) ���������t�F�[�h�C���F2 �b�����č��������
        yield return StartCoroutine(FadeCanvasGroup(fadePanel_black, 1f, 0f, fadeDuration_Start));

        // 2) ���S�ilogoBefore�j�����S�\�� �� �����ҋ@
        yield return WaitOrSkip(logoDisplayDelay);

        // 3) ���C SE �{ ���t���b�V��
        seSource.Play(); // SE �Đ�
        // �t���b�V���p�Ɉ�u alpha �� 1 �ɂ��āA�����߂�
        fadePanel_white.alpha = 1f;
        yield return WaitOrSkip(0.1f); // 0.1 �b�قǌ�����
        fadePanel_white.alpha = 0f;

        // 4) ���S�X�v���C�g�����ւ��i�����ꂽ��̃��S�j
        logoImage.sprite = logoAfter;

        // 5) ���S����ʒ������㕔�ֈړ��iRectTransform �� Y ���W���ԁj
        RectTransform rt = logoImage.rectTransform;
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, startPos.y + 60f); // �D���Ȉړ���
        float moveTime = 0.8f;
        float t = 0f;
        while (t < moveTime)
        {
            if (skipRequested) break; // �X�L�b�v���ꂽ�瑦���I��
            t += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t / moveTime);
            yield return null;
        }
        rt.anchoredPosition = endPos; // �ŏI�ʒu��S��

        // 6) �{�^���Q���t�F�[�h�C���i0��1�j������
        yield return StartCoroutine(FadeCanvasGroup(buttonGroup, 0f, 1f, fadeDuration_Button));

        // �����܂ŗ����牉�o����
    }

    /// <summary>
    /// CanvasGroup �� �� �� from��to �� fadeTime �b�����ĕ�Ԃ���R���[�`��
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float fadeTime)
    {
        float t = 0f;
        cg.alpha = from;
        while (t < fadeTime)
        {
            if (skipRequested) { cg.alpha = to; yield break; } // �X�L�b�v���ꂽ�瑦���ŏI�l��
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        cg.alpha = to;
    }

    /// <summary>
    /// �b���҂��A�X�L�b�v�L�[�������ꂽ�瑦���ɖ߂�w���p�[
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

    // �Q�[���X�^�[�g�{�^������Ăяo���֐�
    public void OnClickGameStart()
    {
        //�Q�[���V�[���𐶐�
        SceneHistoryManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
