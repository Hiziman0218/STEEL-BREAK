using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Attack_Shots : MonoBehaviour
{
    ///使い方
    /// StartCoroutine(Attack_Shots.ShotR(owner.m_Enemy, owner.m_CoolDown, owner.m_CoolTime));
    /// StartCoroutine付けるのを忘れないように

    /// <summary>
    /// 共通射撃処理
    /// 武器選択ロジックを外部から渡すことで「右のみ」「左のみ」「両方」「ランダム」など柔軟に対応可能
    /// shots と interval を指定することで単発／連射を統一的に扱える
    /// </summary>
    /// <param name="enemy">エネミーのスクリプト</param>
    /// <param name="cd">クールダウン管理スクリプト</param>
    /// <param name="coolTime">攻撃後に付与するクールタイム秒数</param>
    /// <param name="weaponSelector">武器選択ロジック（右・左・両方・ランダムなど</param>
    /// <param name="shots">射撃する回数（1なら単発、複数なら連射</param>
    /// <param name="interval">射撃間隔（0なら即時連射、>0なら一定間隔</param>
    public static IEnumerator ExecuteAttack(
        Enemy enemy,
        CoolDown cd,
        float coolTime,
        Func<List<Action>, IEnumerable<Action>> weaponSelector,
        int shots = 1,
        float interval = 0f)
    {
        if (enemy == null) yield break;

        // 利用可能な武器をリスト化（右武器・左武器が存在すれば追加）
        var weapons = new List<Action>();
        if (enemy.weaponR != null) weapons.Add(enemy.UseR);
        if (enemy.weaponL != null) weapons.Add(enemy.UseL);

        if (weapons.Count == 0) yield break; // 武器がない場合は処理終了

        // 指定回数分射撃
        for (int i = 0; i < shots; i++)
        {
            // weaponSelector によって選ばれた武器を発射
            foreach (var fire in weaponSelector(weapons))
            {
                fire?.Invoke();
            }

            // 射撃間隔が指定されていれば待機
            if (interval > 0f)
                yield return new WaitForSeconds(interval);
        }

        // 最後にクールダウンを開始
        cd.StartCoolDown("Attack", coolTime);
    }

    /// <summary>
    /// 右武器のみ単発射撃
    /// </summary>
    public static IEnumerator ShotR(Enemy e, CoolDown cd, float ct)
        => ExecuteAttack(e, cd, ct, weapons => new[] { weapons[0] });

    /// <summary>
    /// 左武器のみ単発射撃
    /// </summary>
    public static IEnumerator ShotL(Enemy e, CoolDown cd, float ct)
        => ExecuteAttack(e, cd, ct, weapons => new[] { weapons[weapons.Count - 1] });

    /// <summary>
    /// 両武器同時射撃（単発）
    /// </summary>
    public static IEnumerator ShotBoth(Enemy e, CoolDown cd, float ct)
        => ExecuteAttack(e, cd, ct, weapons => weapons);

    /// <summary>
    /// ランダム武器射撃（連射対応）
    /// - shots と interval を指定することで「ランダム連射」が可能
    /// </summary>
    public static IEnumerator ShotRandom(Enemy e, CoolDown cd, float ct, int shots, float interval)
        => ExecuteAttack(e, cd, ct, weapons =>
        {
            int choice = UnityEngine.Random.Range(0, weapons.Count);
            return new[] { weapons[choice] };
        }, shots, interval);

    /// <summary>
    /// 両武器バースト射撃
    /// - 1〜maxShots の範囲でランダム回数連射
    /// - 両武器を同時に撃つ
    /// </summary>
    public static IEnumerator ShotBurst(Enemy e, CoolDown cd, float ct, int maxShots, float interval)
    {
        int shots = UnityEngine.Random.Range(1, maxShots);
        return ExecuteAttack(e, cd, ct, weapons => weapons, shots, interval);
    }
}
