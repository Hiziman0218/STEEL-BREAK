using UnityEngine;

[CreateAssetMenu(fileName = "NewStatusData", menuName = "Game/StatusData")]
public class StatusData : ScriptableObject
{
    [Tooltip("‘Ï‹v")]
    public float HP;      //‘Ï‹v
    [Tooltip("UŒ‚—Í")]
    public float Power;   //UŒ‚—Í
    [Tooltip("–hŒä—Í")]
    public float Defence; //–hŒä—Í
    [Tooltip("ˆÚ“®‘¬“x")]
    public float Speed;   //ˆÚ“®‘¬“x
    [Tooltip("Š‘®ƒ`[ƒ€")]
    public string Team;   //Š‘®ƒ`[ƒ€
}
