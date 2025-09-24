using System.Collections.Generic;
using UnityEngine;

public class BoneSet : MonoBehaviour
{
    /* 各部位と必要なパーツ
    //頭部

    //胴体

    //腕部(左)
      　>肩
        >上腕
        >前腕
        >手
        
    //腕部(右)
      　>肩
        >上腕
        >下腕
        >拳

    //腰部

    //脚部(左右)
        >大腿
        >下腿
        >足

    //バックパック
    */

    [System.Serializable]
    //パーツの構造体
    public struct BoneParts
    {
        public string m_Name;         //パーツの名前
        public Transform m_Parts;     //パーツのトランスフォーム
        public Vector3 m_Weight;      //ボーンのウェイト
        public GameObject m_newParts; //ボーンに対応するパーツ
    }

    [System.Serializable]
    //メカの構造体
    public struct Mecha
    {
        public string m_Name;           //部位の名前
        public List<BoneParts> m_Parts; //部位を構成するパーツのリスト
    }
    public List<Mecha> mecha;

    private void LateUpdate()
    {
        //リスト内のメカの全てのボーンを設定
        foreach(Mecha dummy in mecha)
        {
            //SetBone(dummy.m_Parts);
        } 
    }

    public void SetBone(List<BoneParts> Parts)
    {
        //全てのパーツにウェイトを反映
        //Zの要素のみウェイトを参照
        foreach (BoneParts dummy in Parts)
        {
            float X = dummy.m_Weight.x;
            if (X <= 1.0f) X = 1.0f;
            float Y = dummy.m_Weight.y;
            if (Y <= 1.0f) Y = 1.0f;
            float Z = dummy.m_Weight.z;
            if (Z <= 1.0f) Z = 1.0f;
            dummy.m_Parts.localScale = new Vector3(
                X,
                Y,
                Z
                );
            dummy.m_newParts.transform.position = dummy.m_Parts.transform.position;
            dummy.m_newParts.transform.rotation= dummy.m_Parts.transform.rotation;
        }
    }
}
