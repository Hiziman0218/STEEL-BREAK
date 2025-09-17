using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace StateMachineAI
{
    ///★Unity干渉　基底クラスの多重継承が出来ない為、ネームスペースによる
    ///             外部干渉を利用し、無記入のUnity使用可能クラスを作成し、
    ///             それを緩衝材として噛ませる事でUnityライブラリを使用可能にする。
    ///             必要なのは、MonoBehaviourのみ
    public class Unity干渉 : MonoBehaviour
    {
    }
}
