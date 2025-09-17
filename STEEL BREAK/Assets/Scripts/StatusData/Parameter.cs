using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parameter
{
    private float m_Height;  //����
    private string m_myTeam; //��������`�[��

    public Parameter(CharaData data)
    {
        m_Height = data.Height;
        m_myTeam = data.Team;
    }

    public float GetHeight()
    {
        return m_Height;
    }

    public string GetTeam()
    {
        return m_myTeam;
    }
}
