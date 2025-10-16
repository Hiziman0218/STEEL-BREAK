using UnityEngine;

namespace StateMachineAI
{
    //子機を出す
    public class Spawn_T : State<Titania_T>
    {
        //コンストラクタ
        public Spawn_T(Titania_T owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("雑魚召喚開始");

            // Null掃除
            owner.m_spawnedAttackEnemies.RemoveAll(item => item == null);
            owner.m_spawnedDefensEnemies.RemoveAll(item => item == null);

            // コルーチン起動
            owner.StartCoroutine(
                SpawnFairys_T.SpawnWithInterval(
                    owner.m_Fairys,
                    owner.m_SpawnPoints,
                    owner.m_spawnedEnemies,
                    owner.m_spawnedAttackEnemies,
                    owner.m_spawnedDefensEnemies,
                    owner.m_SpawnPer,
                    owner.m_waitSeconds,
                    owner.m_MaxFairys,
                    () =>
                    {
                        // 召喚完了後に Idle に戻す
                        owner.ChangeState(AIState_Titania_T.Idle_T);
                    }
                )
            );
        }
    }
}