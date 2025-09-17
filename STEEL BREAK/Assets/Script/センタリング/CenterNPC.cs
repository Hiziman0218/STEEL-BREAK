using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class CenterNPC : MonoBehaviour
{
    [Header("プレイヤー")]
    public Transform m_Player;
    [Header("ナビターゲット")]
    public Vector3 m_Target;
    [Header("ナビ")]
    public NavMeshAgent m_NavMeshAgent;
    [Header("エネミーモデル")]
    public Transform m_EnemyModel;

    [Header("向き補正")]
    public float m_Moku = 1;
    [Header("攻撃可能角度[-1 = 完全に背後, 0 = 真横, 1 = 正面]")]
    public float m_BackstabDotThreshold = -0.7f;
    [Header("攻撃可能距離")]
    public float m_AttackDistance = 2f;
    void Start()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {

        if (m_NavMeshAgent)
            m_NavMeshAgent.destination = m_Target;
        CenterPoint();
        if (Input.GetMouseButtonDown(0))
            m_Moku *= - 1;
    }
    /// <summary>
    /// 旋回警戒用AI行動
    /// </summary>
    public void CenterPoint()
    {
        ///センターポイントを取得(一回のみ格納の方が効率的)
        GameObject CenterMarker = GameObject.Find("センターポインター");
        ///センタポイントが無い場合はこのルーチンは使用禁止
        if (!CenterMarker)
            return;

        ///まずは、プレイヤー(ターゲット)の位置を取得
        Vector3 TargetPosition =  m_Player.position;
        ///ターゲットのY軸を揃える
        TargetPosition.y = transform.position.y;
        ///センターポイントの座標を(Y補正付き)ターゲットに合わせる
        CenterMarker.transform.position = TargetPosition;
        ///センターポイントの向きをNPCへ向けさせる
        CenterMarker.transform.LookAt(this.transform.position);
        ///１回分の旋回角度分回転
        CenterMarker.transform.Rotate(new Vector3(0, 10f * m_Moku, 0));
        ///センターポイントをターゲットから指定分遠ざける(相対距離位置指定)
        CenterMarker.transform.Translate(new Vector3(0, 0, 10));
        ///その地点をNPCの目標地点とする
        m_Target = CenterMarker.transform.position;
        ///エネミーのモデル向きを変更
        EnemyModelLook();
        AttackChance();
    }
    /// <summary>
    /// 強制NPCモデル向き補正
    /// </summary>
    public void EnemyModelLook()
    {
        ///ターゲットのプレイヤー座標を取得
        Vector3 Pos = m_Player.position;
        ///Y軸を揃える
        Pos.y = m_EnemyModel.position.y;
        ///Y軸補正付きで、モデルデータをプレイヤーに向けさせる
        m_EnemyModel.LookAt(Pos);
    }

    public void AttackChance()
    {
        if (m_Player == null) return;

        ///プレイヤーの正面ベクトルを取得
        Vector3 playerForward = m_Player.forward;

        ///プレイヤーから見た、NPCの方向ベクトル
        Vector3 directionToSelf = (transform.position - m_Player.position).normalized;

        ///内積を使って角度を計算
        float dot = Vector3.Dot(playerForward, directionToSelf);

        ///プレイヤーとの相対距離チェック
        float distance = Vector3.Distance(transform.position, m_Player.position);

        // 背後にいて、且つ距離が近ければ攻撃
        if (dot < m_BackstabDotThreshold && distance <= m_AttackDistance)
        {
            if(Random.Range(0,100) > 95)
                AttackPlayer();
        }
    }
    /// <summary>
    /// 仮組のプレイヤーへの攻撃
    /// </summary>
    public void AttackPlayer()
    {
        Debug.Log("背後から攻撃！");
        this.transform.LookAt(m_Player.position);
        m_EnemyModel.transform.LookAt(m_Player.position);
        this.transform.Translate(new Vector3(0, 0, 1));
    }
}
