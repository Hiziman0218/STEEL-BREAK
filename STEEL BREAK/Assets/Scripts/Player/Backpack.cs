using UnityEngine;
using System.Collections.Generic;
using Game.Enum;

public class Backpack : MonoBehaviour
{
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

    private List<GameObject> m_activeEffects = new List<GameObject>(); //生成しているエフェクトのリスト

    private InputManager m_inputManager; //入力管理クラス
    private Player m_player; //プレイヤー

    private BoostState m_currentBoostState = BoostState.None; //現在の移動状態
    private BoostState m_prevBoostState = BoostState.None;    //前フレームの移動状態

    private void Start()
    {
        // プレイヤーのInputManagerを取得
        m_inputManager = transform.root.GetComponent<InputManager>();

        if (m_inputManager == null)
        {
            Debug.LogError("InputManagerがルートオブジェクトに存在しません。");
        }
    }

    private void Update()
    {
        //ブーストの状態とそれに応じたエフェクトの更新
        UpdateBoostState();

        // 状態が変化したときのみエフェクトを更新
        if (m_currentBoostState != m_prevBoostState)
        {
            StartCoroutine(UpdateBoostEffect());
            //UpdateBoostEffect();
            m_prevBoostState = m_currentBoostState;
        }
    }

    /// <summary>
    /// 親が変わったときに呼ばれる処理
    /// プレイヤーに装備ポイントを設定
    /// </summary>
    private void OnTransformParentChanged()
    {
        //親にPlayerがいるか調べる
        var player = transform.root.GetComponent<Player>();

        //見つかった場合 → プレイヤーに自分の装備ポイントを登録
        if (player != null)
        {
            m_player = player;
            player.SetBackAttachPoints(m_rightAttachPoint, m_leftAttachPoint);
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
        bool isBoosting = m_inputManager.IsBoost;

        if (isBoosting)
            m_currentBoostState = BoostState.Dash;
        else if (isMoving)
            m_currentBoostState = BoostState.Normal;
        else
            m_currentBoostState = BoostState.None;
    }

    /// <summary>
    /// 現在の状態に応じてエフェクトを切り替え（フェード対応）
    /// </summary>
    private System.Collections.IEnumerator UpdateBoostEffect()
    {
        // 既存のエフェクトを「即消し」ではなく「停止して自然消滅」
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

        yield return null; // 1フレーム待機（保険）

        // 状態に応じて新しいエフェクトを生成
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

    /*
    /// <summary>
    /// 現在の移動状態に応じてエフェクトを切り替え
    /// </summary>
    private void UpdateBoostEffect()
    {
        // 既存のエフェクトを削除
        foreach (var effect in m_activeEffects)
        {
            if (effect != null)
                Destroy(effect);
        }
        m_activeEffects.Clear();

        // 状態に応じて新しいエフェクトを生成
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
            return;

        foreach (var point in m_boostPoints)
        {
            if (point == null) continue;
            var effect = Instantiate(prefabToSpawn, point.position, point.rotation, point);
            m_activeEffects.Add(effect);
        }
    }

    /// <summary>
    /// 指定エフェクトを全ブーストポイントに生成
    /// </summary>
    private void SpawnEffect(GameObject effectPrefab)
    {
        if (effectPrefab == null)
            return;

        foreach (var point in m_boostPoints)
        {
            if (point == null) continue;

            var effect = Instantiate(effectPrefab, point.position, point.rotation, point);
            m_activeEffects.Add(effect);
        }
    }*/
}
