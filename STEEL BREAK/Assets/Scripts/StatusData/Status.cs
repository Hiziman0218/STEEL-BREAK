using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Status
{
    private float m_HP;      //Œ»İ‚Ì‘Ï‹v
    private float m_maxHP;   //Å‘å‘Ï‹v
    private float m_power;   //UŒ‚—Í
    private float m_defence; //–hŒä—Í
    private float m_speed;   //ˆÚ“®‘¬“x

    public Status(StatusData data)
    {
        m_HP = data.HP;
        m_maxHP = data.HP;
        m_power = data.Power;
        m_defence = data.Defence;
        m_speed = data.Speed;
    }

    public float GetHP()
    {
        return m_HP;
    }

    public void SetHP(float HP)
    {
        m_HP = HP;
    }

    public float GetMaxHP()
    {
        return m_maxHP;
    }

    public void SetMaxHP(float MaxHP)
    {
        m_maxHP = MaxHP;
    }

    public float GetPower()
    {
        return m_power;
    }

    public void SetPower(float Power)
    {
        m_power = Power;
    }

    public float GetDefence()
    {
        return m_defence;
    }

    public void SetDefence(float Defence)
    {
        m_defence = Defence;
    }

    public float GetSpeed()
    {
        return m_speed;
    }

    public void SetSpeed(float speed)
    {
        m_speed = speed;
    }
}
