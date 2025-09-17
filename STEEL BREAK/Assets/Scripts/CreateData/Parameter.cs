using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parameter
{
    private float m_Height;  //‚‚³
    private string m_myTeam; //Š‘®‚·‚éƒ`[ƒ€

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
