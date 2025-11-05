using UnityEngine;

public class BulletBase : MonoBehaviour
{
    protected Weapon_Shooting m_shooting; //自身を生成した銃
    protected string m_myTeam;    //所属チーム
    protected float m_damage;     //与えられるダメージ
    protected float m_speed;      //弾速
    protected Transform m_target; //自身が狙っているターゲット

    /// <summary>
    /// 自身を生成した銃を取得
    /// </summary>
    /// <returns></returns>
    public Weapon_Shooting GetShooting()
    {
        return m_shooting;
    }

    /// <summary>
    /// 自身を生成した銃を設定
    /// </summary>
    /// <param name="shooting"></param>
    public void SetShooting(Weapon_Shooting shooting)
    {
        m_shooting = shooting;
    }

    /// <summary>
    /// 弾丸の所属チームを取得
    /// </summary>
    /// <returns></returns>
    public string GetTeam()
    {
        return m_myTeam;
    }

    /// <summary>
    /// 自身の所属するチームを設定
    /// </summary>
    /// <param name="team"></param>
    public void SetTeam(string team)
    {
        m_myTeam = team;
    }

    /// <summary>
    /// 与えるダメージ量を取得
    /// </summary>
    /// <returns></returns>
    public float GetDamage()
    {
        return m_damage;
    }

    /// <summary>
    /// 与えるダメージ量を設定
    /// </summary>
    /// <param name="damage"></param>
    public void SetDamage(float damage)
    {
        m_damage = damage;
    }

    /// <summary>
    /// 弾速を取得
    /// </summary>
    /// <returns></returns>
    public float GetSpeed()
    {
        return m_speed;
    }

    /// <summary>
    /// 弾速を設定
    /// </summary>
    /// <param name="speed"></param>
    public void SetSpeed(float speed)
    {
        m_speed = speed;
    }

    /// <summary>
    /// ターゲットを取得
    /// </summary>
    /// <returns></returns>
    public Transform GetTarget()
    {
        return m_target;
    }

    /// <summary>
    /// ターゲットを設定
    /// </summary>
    /// <param name="target"></param>
    public void SetTarget(Transform target)
    {
        m_target = target;
    }
}
