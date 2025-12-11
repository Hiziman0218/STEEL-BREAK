using UnityEngine;
using System.Collections.Generic;
using Game.Enum;
using System.Collections;

public class Backpack : MonoBehaviour
{
    [Header("ブースト性能設定")]
    [Tooltip("移動力にかかる倍率(1 = 等倍)")]
    [SerializeField] private float m_moveMultiplier = 1f;

    [Header("アタッチポイント設定")]
    [Tooltip("背面武装が装備される右側ポイント")]
    [SerializeField] private Transform m_rightAttachPoint;
    [Tooltip("背面武装が装備される左側ポイント")]
    [SerializeField] private Transform m_leftAttachPoint;

    [Header("ブーストエフェクト設定")]
    [Tooltip("ブーストエフェクトが出る位置(数は任意)")]
    [SerializeField] private List<Transform> m_boostPoints = new List<Transform>();
    [Header("エフェクト設定")]
    [Tooltip("通常ブーストエフェクトプレハブ")]
    [SerializeField] private GameObject m_normalBoostEffectPrefab;
    [Tooltip("ブーストダッシュエフェクトプレハブ")]
    [SerializeField] private GameObject m_dashBoostEffectPrefab;

    [Header("移動SE設定")]
    [Tooltip("音声管理クラス")]
    [SerializeField] private AudioSource m_audioSource;
    [Tooltip("通常移動のループSE")]
    [SerializeField] private AudioClip m_normalMoveSE;
    [Tooltip("ブースト移動のループSE")]
    [SerializeField] private AudioClip m_dashMoveSE;
    [Tooltip("SEが切り替わる際のフェード時間(sec)")]
    [SerializeField] private float m_fadeDuration = 0.25f;

    private List<GameObject> m_activeEffects = new List<GameObject>(); //生成しているエフェクトのリスト

    private AudioSource m_audioSourceA;
    private AudioSource m_audioSourceB;
    private AudioSource m_currentSource;   //現在再生されているるSE
    private AudioSource m_nextSource;      //次に再生するSE
    private Coroutine m_fadeCoroutine = null;

    private InputManager m_inputManager; //入力管理クラス
    private Movement m_movement; //移動管理クラス
    private PlayerBase m_player; //プレイヤー

    private BoostState m_currentBoostState = BoostState.None; //現在の移動状態
    private BoostState m_prevBoostState = BoostState.None;    //前フレームの移動状態

    private void Awake()
    {
        SettingAttachPoints();

        // AudioSource A
        m_audioSourceA = gameObject.AddComponent<AudioSource>();
        m_audioSourceA.loop = true;
        m_audioSourceA.playOnAwake = false;
        m_audioSourceA.volume = 0f;

        // AudioSource B
        m_audioSourceB = gameObject.AddComponent<AudioSource>();
        m_audioSourceB.loop = true;
        m_audioSourceB.playOnAwake = false;
        m_audioSourceB.volume = 0f;

        m_currentSource = m_audioSourceA;
        m_nextSource = m_audioSourceB;
    }

    private void Start()
    {
        //プレイヤーのInputManagerを取得
        m_inputManager = transform.root.GetComponent<InputManager>();
        m_movement = transform.root.GetComponent<Movement>();
        m_audioSource = GetComponent<AudioSource>();
        if (m_audioSource == null) m_audioSource = gameObject.AddComponent<AudioSource>();
        m_audioSource.loop = true; //ループ素材前提

        //移動倍率を適用
        ApplyMovementMultiplier();
    }

    private void Update()
    {
        //ブーストの状態とそれに応じたエフェクトの更新
        UpdateBoostState();

        //状態が変化したときのみエフェクト/SEを更新
        if (m_currentBoostState != m_prevBoostState)
        {
            StartCoroutine(UpdateBoostEffect());
            CrossfadeMoveSE();
            m_prevBoostState = m_currentBoostState;
        }
    }

    /// <summary>
    /// 親が変わったときに呼ばれる処理
    /// </summary>
    private void OnTransformParentChanged()
    {
        SettingAttachPoints();
    }

    /// <summary>
    /// プレイヤーに自身のアタッチポイントを設定
    /// </summary>
    public void SettingAttachPoints()
    {
        //親にPlayerがいるか調べる
        var player = transform.GetComponentInParent<PlayerBase>();

        //見つかった場合 → プレイヤーに自分の装備ポイントを登録
        if (player != null)
        {
            m_player = player;
            m_player.SetBackAttachPoints(m_rightAttachPoint, m_leftAttachPoint);
        }
    }

    /// <summary>
    /// プレイヤーの入力から現在のBoostStateを更新
    /// </summary>
    private void UpdateBoostState()
    {
        //入力管理クラスが無ければ、以降の処理を行わない
        if (m_inputManager == null)
            return;

        //現在の移動状態を確認し設定
        bool isMoving = m_inputManager.IsMoving();
        bool isBoosting = m_movement.IsBoosting;

        if (isBoosting)
            m_currentBoostState = BoostState.Dash;
        else if (isMoving)
            m_currentBoostState = BoostState.Normal;
        else
            m_currentBoostState = BoostState.None;
    }

    /// <summary>
    /// 現在の状態に応じてエフェクトを切り替え
    /// </summary>
    private System.Collections.IEnumerator UpdateBoostEffect()
    {
        //既存のエフェクトを「即消し」ではなく「停止して自然消滅」
        foreach (var effect in m_activeEffects)
        {
            if (effect == null) continue;

            var particle = effect.GetComponent<ParticleSystem>();
            if (particle != null)
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(effect, particle.main.startLifetime.constantMax + 0.5f);
            }
            else
            {
                Destroy(effect);
            }
        }
        m_activeEffects.Clear();

        yield return null; //1フレーム待機

        //状態に応じて新しいエフェクトを生成
        GameObject prefabToSpawn = null;
        switch (m_currentBoostState)
        {
            case BoostState.Normal:
                prefabToSpawn = m_normalBoostEffectPrefab;
                break;
            case BoostState.Dash:
                prefabToSpawn = m_dashBoostEffectPrefab;
                break;
        }

        if (prefabToSpawn == null)
            yield break;

        foreach (var point in m_boostPoints)
        {
            if (point == null) continue;
            var effect = Instantiate(prefabToSpawn, point.position, point.rotation, point);
            m_activeEffects.Add(effect);
        }
    }

    /// <summary>
    /// 現在の状態に応じて SE をクロスフェードで切り替え
    /// </summary>
    private void CrossfadeMoveSE()
    {
        AudioClip targetClip = null;
        switch (m_currentBoostState)
        {
            case BoostState.Normal:
                targetClip = m_normalMoveSE;
                break;
            case BoostState.Dash:
                targetClip = m_dashMoveSE;
                break;
            case BoostState.None:
                targetClip = null;
                break;
        }

        //フェード処理を強制停止してから新しい処理へ
        if (m_fadeCoroutine != null)
        {
            StopCoroutine(m_fadeCoroutine);
            m_fadeCoroutine = null;
        }

        //None の場合 → すべて強制フェードアウトして終了
        if (targetClip == null)
        {
            m_fadeCoroutine = StartCoroutine(FadeOutAll());
            return;
        }

        //同じクリップでも、volume=0ならフェードインが必要なので常に処理
        m_fadeCoroutine = StartCoroutine(FadeToClip(targetClip));
    }

    private IEnumerator FadeOutAll()
    {
        float time = 0f;
        float startA = m_audioSourceA.volume;
        float startB = m_audioSourceB.volume;

        while (time < m_fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / m_fadeDuration;

            m_audioSourceA.volume = Mathf.Lerp(startA, 0f, t);
            m_audioSourceB.volume = Mathf.Lerp(startB, 0f, t);

            yield return null;
        }

        m_audioSourceA.Stop();
        m_audioSourceB.Stop();
        m_audioSourceA.volume = 0f;
        m_audioSourceB.volume = 0f;

        m_fadeCoroutine = null;
    }

    private IEnumerator FadeToClip(AudioClip newClip)
    {
        // currentSource と nextSource を入れ替える
        AudioSource from = m_currentSource;
        AudioSource to = m_nextSource;

        // 新しい Clip をセットし、音量 0 で再生スタート
        to.clip = newClip;
        to.volume = 0f;
        to.Play();

        float time = 0f;
        float fromStart = from.volume;

        while (time < m_fadeDuration)
        {
            // None が来たら即終了 → FadeOutAll にバトンタッチ
            if (m_currentBoostState == BoostState.None)
            {
                yield return FadeOutAll();
                yield break;
            }

            time += Time.deltaTime;
            float t = time / m_fadeDuration;

            to.volume = t;
            from.volume = Mathf.Lerp(fromStart, 0f, t);

            yield return null;
        }

        from.Stop();
        to.volume = 1f;

        // ソースの役割を入れ替え
        m_currentSource = to;
        m_nextSource = from;

        m_fadeCoroutine = null;
    }

    /// <summary>
    /// Movement に倍率を設定する
    /// </summary>
    public void ApplyMovementMultiplier()
    {
        if (m_movement != null)
            m_movement.SetMoveMultiplier(m_moveMultiplier);
    }
}
