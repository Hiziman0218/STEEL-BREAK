using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "GameData/UnitData")]
public class UnitData : ScriptableObject
{
    [Header("キャラクター名")]
    public string m_CharacterName;

    [Header("体力")]
    public int m_Hp;

    [Header("Character説明")]
    [TextArea]
    public string m_Description;
}
