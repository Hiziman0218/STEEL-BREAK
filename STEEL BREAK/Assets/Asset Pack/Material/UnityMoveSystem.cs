using UnityEngine;

public class UnityMoveSystem : MonoBehaviour
{
    public UnityMoveData m_Master;
    public UnityMoveData m_UnityMoveData;
    public GameObject m_Panel;
    void Start()
    {
        m_UnityMoveData = new UnityMoveData();
        m_UnityMoveData.SetUp(
            m_Master,
            m_Panel,
            m_Panel.GetComponent<Renderer>().material);
    }

    void Update()
    {
        m_UnityMoveData.MoveAnimetion(
            new Vector2(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical")));
    }
}
