using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 飛行ユニット制御ナビシステム
/// </summary>
public class FlyNaviSystem : MonoBehaviour
{
    public NavMeshAgent m_Agent;
    [Header("飛行するユニット")]
    public GameObject m_Unit;

    [Header("高度")]
    public Transform m_UpPoint;

    [Header("飛行ユニットセット時のユニットの位置")]
    public Vector3 m_RoundPoint;

    [Header("移動目標")]
    public Vector3 m_Target;

    [Header("戦闘モード")]
    public bool m_BattleMode;

    [Header("高度到着カウンター")]
    public float m_LerpMoveCounter = 0.0f;
    [Header("高度到着スピード/秒")]
    public float m_LaepMoveSpeed = 1.0f;
    [Header("旋回カウンター")]
    public float m_LerpRotCounter = 0.0f;
    [Header("旋回到着スピード/秒")]
    public float m_LaepRotSpeed = 1.0f;


    [Header("ナビゲーション時間")]
    public float m_NaviTime = 0.0f;
    [Header("ナビゲーション最大時間")]
    public float m_NaviMaxTime = 3.0f;

    public Vector2 m_UpDwonSetPoint = new Vector2(5.0f,2.0f);
    public float m_UpDwonPoint = 0.0f;
    void Start()
    {
        m_NaviTime = 0.0f;
        m_LerpMoveCounter = 0.0f;
        m_LerpRotCounter = 0.0f;
        m_UpDwonPoint = Random.Range(m_UpDwonSetPoint.x, m_UpDwonSetPoint.y);
        m_UpPoint.transform.position = new Vector3(m_UpPoint.position.x, m_UpDwonPoint, m_UpPoint.position.z);
        m_RoundPoint = transform.position;
    }

    void LateUpdate()
    {
        FlyMove();
    }
    public void FlyMove()
    {
        //ユニットが存在し、移動先も存在する
        if (m_Unit)
        {
            if (m_LerpMoveCounter == 1.0f)
            {
                m_Unit.transform.position = m_UpPoint.position;
            }
            else
            {
                m_LerpMoveCounter += m_LaepMoveSpeed * Time.deltaTime;
                if (m_LerpMoveCounter > 1.0f) m_LerpMoveCounter = 1.0f;
                m_Unit.transform.position = Vector3.Lerp(m_RoundPoint, m_UpPoint.position,  m_LerpMoveCounter);
            }

            if (!m_BattleMode)
            {
                NomalMoveMode();
            }
            NaviTimes();
        }
    }
    public void SetUp(GameObject Unit)
    {
        m_Unit = Unit;
        m_NaviTime = 0.0f;
        m_LerpMoveCounter = 0.0f;
        m_LerpRotCounter = 0.0f;
        m_RoundPoint = transform.position;
    }
    public void Reset()
    {
        m_Unit.GetComponent<Rigidbody>().useGravity = true;
        Destroy(gameObject);
    }
    public void NomalMoveMode()
    {
        m_LerpRotCounter += m_LaepRotSpeed * Time.deltaTime;
        if (m_LerpRotCounter > 1.0f) m_LerpRotCounter = 1.0f;
        m_Unit.transform.rotation = Quaternion.Lerp(m_Unit.transform.rotation,m_UpPoint.rotation,m_LerpRotCounter);
    }
    public void NaviTimes()
    {
        if (m_NaviTime <= 0.0f)
        {
            m_NaviTime = Random.Range(m_NaviMaxTime, m_NaviMaxTime * 1.5f);
            m_Target = new Vector3(Random.Range(10.0f,-10.0f),0, Random.Range(10.0f, -10.0f));
            m_Agent.SetDestination(m_Target);
            m_UpDwonPoint = Random.Range(m_UpDwonSetPoint.x, m_UpDwonSetPoint.y);
            m_UpPoint.transform.position = new Vector3(m_UpPoint.position.x, m_UpDwonPoint, m_UpPoint.position.z);
            m_LerpMoveCounter = 0.0f;
            m_LerpRotCounter = 0.0f;
            m_RoundPoint = m_Unit.transform.position;

        }
        else
        {
            m_NaviTime -= Time.deltaTime;
        }
    }
}
