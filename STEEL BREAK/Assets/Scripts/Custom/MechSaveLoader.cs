using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 機体の構成を保存・読み込みするクラス
/// </summary>
public class MechSaveLoader : MonoBehaviour
{
    [SerializeField] private MechAssemblyManager assemblyManager;  // パーツ装着を管理するクラス
}
