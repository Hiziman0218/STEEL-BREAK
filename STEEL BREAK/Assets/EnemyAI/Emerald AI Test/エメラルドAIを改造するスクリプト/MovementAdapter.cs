using Plugins.RaycastPro.Demo.Scripts;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 移動系の共通インターフェース
/// NavMeshAgent と RayCastPro の両方を同じように扱うための窓口
/// </summary>
public interface IMovementAgent
{
    // 目的地
    void SetDestination(Vector3 targetPos);
    // 現在の目的地までの距離
    float RemainingDistance { get; }
    // NavMesh 上にいるかどうか
    // RayCastPro の場合は常に true 扱い
    bool IsOnNavMesh { get; }
}

/// <summary>
/// NavMeshAgent を IMovementAgent として扱うためのアダプター
/// </summary>
public class NavMeshAgentAdapter : IMovementAgent
{
    private NavMeshAgent agent;

    public NavMeshAgentAdapter(NavMeshAgent agent)
    {
        this.agent = agent;
    }

    // NavMeshAgent に目的地を設定
    public void SetDestination(Vector3 targetPos)
    {
        if (agent != null && agent.isOnNavMesh)
            agent.SetDestination(targetPos);
    }

    // NavMeshAgent の remainingDistance を返す NavMesh 上にいない場合は Infinity
    public float RemainingDistance => (agent != null && agent.isOnNavMesh) ? agent.remainingDistance : Mathf.Infinity;

    // NavMesh 上にいるかどうか
    public bool IsOnNavMesh => agent != null && agent.isOnNavMesh;
}

/// <summary>
/// RayCastPro を IMovementAgent として扱うためのアダプター
/// </summary>
public class RayCastProAdapter : IMovementAgent
{
    private SteeringController m_rcController;
    private Transform owner;

    public RayCastProAdapter(SteeringController m_rcController, Transform owner)
    {
        this.m_rcController = m_rcController;
        this.owner = owner;
    }

    // RayCastPro の detector.destination の位置を直接書き換えて目的地を設定
    public void SetDestination(Vector3 targetPos)
    {
        if (m_rcController.detector.destination != null)
        {
            m_rcController.detector.destination.position = targetPos;
        }
    }

    // 現在位置と目的地の Transform の距離を計算して返す
    public float RemainingDistance
    {
        get
        {
            if (m_rcController != null && m_rcController.detector.destination != null)
                return Vector3.Distance(owner.position, m_rcController.detector.destination.position);
            return Mathf.Infinity;
        }
    }

    // RayCastPro では常に true 扱い
    public bool IsOnNavMesh => true;
}

