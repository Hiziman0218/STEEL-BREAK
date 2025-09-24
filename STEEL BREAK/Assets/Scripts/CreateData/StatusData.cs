using UnityEngine;

[CreateAssetMenu(fileName = "NewStatusData", menuName = "Game/StatusData")]
public class StatusData : ScriptableObject
{
    public float HP;      //�ϋv
    public float Power;   //�U����
    public float Defence; //�h���
    public float Speed;   //�ړ����x
    public string Team;   //��������`�[��
}
