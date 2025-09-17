using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// バンカーセットシステム
#region バンカーとは
/// バンカーは、NPC(AI)がポイントディフェンス(地点防衛)の場所で、タクティカルエリアとも云われます。
/// 司令官(スカッドAI)によって、キャラAIに防衛地点を指示され、その地点からプレイヤーを向かいうちます。
/// バンカーが機能しない、もしくは、バンカーが襲撃された場合、バンカーに留まるNPCは、退避経路上のバンカー
/// へ移動し、継続戦闘を行います。
/// このバンカーセットの利点は、スナイプポイント(芋る地点)の指定をプレイヤーの地点から割り出して決める事が
/// 出来たり、防衛地点を定めてプレイヤーを挟撃する等が出来る為、難易度調整に向きます。
/// 複数人数によるAIを含めた対戦にも、このシステムは使いやすく、進撃、退避のタイミング等をスカッドAIからの
/// 指示を組み込めれば、タクティカルバトル(戦術戦闘)が可能になります。
#endregion
/// </summary>
public class BankerSet : MonoBehaviour
{
    /// <summary>
    /// バンカーのNPC逃走用センターと、退避する為のバンカー位置's
    /// </summary>
    [System.Serializable]
    public struct EscapeSensor
    {
        [Header("バンカーsensor")]
        public BankerSensor m_BankerSensor;
        [Header("退避用の移動先バンカー位置")]
        public List<Transform> m_EscapePoint;
    }
    [Header("Sensorと逃げ道")]
    public List<EscapeSensor> m_EscapeSensors;

    [Header("現在バンカーにいるユニットリスト")]
    public List<Unit> m_Units;

    private void Update()
    {
        ///侵入者チェック
        InvaderHit();
    }
    /// <summary>
    /// 侵入者確認
    /// </summary>
    public void InvaderHit()
    {
        if (m_Units.Count == 0)
            return;

        ///全てのセンサーをチェック
        for(int i=0;i< m_EscapeSensors.Count;i++)
        {
            ///センサーにプレイヤーがヒットしている場合
            if (m_EscapeSensors[i].m_BankerSensor.m_PlayerHit)
            {
                ///そのセンサー側からプレイヤーが侵入したので、退避行動を取らせる
                EscapeRoutine(i);
            }
        }
    }
    /// <summary>
    /// 退避行動実行
    /// </summary>
    /// <param name="No">センサー番号</param>
    public void EscapeRoutine(int No)
    {
        if (m_Units.Count == 0)
            return;
        ///現在バンカーに所属しているNPCを走査
        for(int i=0;i< m_Units.Count;i++)
        {
            if (m_Units[i])
            {
                ///NPCに次の移動先として、センサーに引っかかった方向を加味した退避バンカーを
                ///割り出してターゲットに渡す
                m_Units[i].m_Target = m_EscapeSensors[No].m_EscapePoint[
                    Random.Range(
                        0,
                        m_EscapeSensors[No].m_EscapePoint.Count)];
                ///現バンカー所属を破棄する
                m_Units[i].m_BankerSet = null;
                m_Units[i] = null;
            }
        }
        // null要素を削除
        m_Units.RemoveAll(item => item == null);
    }
    /// <summary>
    /// NPCがバンカーに到着
    /// </summary>
    /// <param name="other">NPC</param>
    public void OnTriggerStay(Collider other)
    {
        ///相手はNPCである
        if (other.GetComponent<Unit>())
        {
            ///NPCの行先が本バンカーであり、また、NPC側の所属バンカーはnullである
            if (other.GetComponent<Unit>().m_Target == this.transform && 
                other.GetComponent<Unit>().m_BankerSet == null)
            {
                ///NPC側のバンカー所属は自身とする
                other.GetComponent<Unit>().m_BankerSet = this;
                ///バンカー所属リストにNPCを追加する
                m_Units.Add(other.GetComponent<Unit>());
            }
        }
    }
    private void OnDrawGizmos()
    {
        foreach (EscapeSensor ES in m_EscapeSensors)
        {
            Handles.color = Color.red;
            // 現在のオブジェクト位置に半径1の球体を描画
            Gizmos.color = Color.white;
            Gizmos.DrawCube(ES.m_BankerSensor.transform.position, new Vector3(1,2,1));
            for (int i = 0; i < ES.m_EscapePoint.Count; i++)
            {
                Handles.DrawAAPolyLine(
                    10.0f,
                    ES.m_BankerSensor.transform.position,
                    ES.m_EscapePoint[i].position);
                // 現在のオブジェクト位置に半径1の球体を描画
                Gizmos.DrawSphere(ES.m_EscapePoint[i].position, 1.0f);
            }
        }
    }
}
