using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Lookat : MonoBehaviour
{
    //��]�̎��ɂ���I�u�W�F�N�g
    public Transform pivotObject;

    public void LookTaget()
    {
        transform.LookAt(pivotObject);
    }

}
