using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 【EmeraldWaypointObject】
    /// AI の巡回・移動などに使用するウェイポイント群を保持する ScriptableObject。
    /// </summary>
    public class EmeraldWaypointObject : ScriptableObject
    {
        [Header("ウェイポイント座標のリスト（AI の巡回点）")]
        public List<Vector3> Waypoints = new List<Vector3>();
    }
}
