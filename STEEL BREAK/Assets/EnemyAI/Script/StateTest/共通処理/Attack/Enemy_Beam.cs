using UnityEngine;

public class Enemy_Beam : MonoBehaviour
{
    [SerializeField] private Transform muzzle;
    [SerializeField] private float beamLength = 20f;
    private LineRenderer line;

    private void Awake()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.startWidth = 0.2f;
        line.endWidth = 0.2f;
        line.material = new Material(Shader.Find("Unlit/Color"));
        line.material.color = Color.red;
    }

    private void Update()
    {
        line.SetPosition(0, muzzle.position);
        line.SetPosition(1, muzzle.position + transform.forward * beamLength);
    }
}
