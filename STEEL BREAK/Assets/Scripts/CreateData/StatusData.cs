using UnityEngine;

[CreateAssetMenu(fileName = "NewStatusData", menuName = "Game/StatusData")]
public class StatusData : ScriptableObject
{
    [Tooltip("耐久")]
    public float HP;      //耐久
    [Tooltip("攻撃力")]
    public float Power;   //攻撃力
    [Tooltip("防御力")]
    public float Defence; //防御力
    [Tooltip("移動速度")]
    public float Speed;   //移動速度
    [Tooltip("所属チーム")]
    public string Team;   //所属チーム
    [Tooltip("死亡エフェクト")]
    public DestructionEffect DestructionEffect; //死亡エフェクト
}
