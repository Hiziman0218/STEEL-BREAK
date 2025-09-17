using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("マウス横回転感度")]
    [Tooltip("大きいほどゆっくり、小さいほど速く回転します")]
    public float mouseSensitivity = 100f;

    void Update()
    {
        transform.Rotate(0f, Input.GetAxis("Mouse X"), 0f);
    }
}
