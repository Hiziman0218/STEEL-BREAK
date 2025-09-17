using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Lookat : MonoBehaviour
{
    //回転の軸にするオブジェクト
    public Transform pivotObject;

    public void LookTaget()
    {
        transform.LookAt(pivotObject);
    }

}
