using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "GameData/UnitData")]
public class UnitData : ScriptableObject
{
    [Header("�L�����N�^�[��")]
    public string m_CharacterName;

    [Header("�̗�")]
    public int m_Hp;

    [Header("Character����")]
    [TextArea]
    public string m_Description;
}
