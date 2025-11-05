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
            owner.currentEnemy.m_spawnedEnemies.RemoveAll(item => item == null);
            owner.currentEnemy.m_spawnedAttackEnemies.RemoveAll(item => item == null);
            owner.currentEnemy.m_spawnedDefensEnemies.RemoveAll(item => item == null);


            //現在場にいる雑魚が上限の半数未満なら再生成
            if (owner.currentEnemy.m_spawnedEnemies.Count < owner.m_MaxFairys / 2)
            {
                owner.StartCoroutine(
                    SpawnFairys_T.SpawnWithInterval(
                        owner.m_Fairys,
                        owner.m_SpawnPoints,
                        owner.currentEnemy.m_spawnedEnemies,
                        owner.currentEnemy.m_spawnedAttackEnemies,
                        owner.currentEnemy.m_spawnedDefensEnemies,
                        owner.m_SpawnPer,
                        owner.m_waitSeconds,
                        owner.m_MaxFairys,
                        owner.m_MaxDefensFairys,
                        () => owner.ChangeState(AIState_Titania_T.Idle_T)
                    )
                );
            }
            else
            {
                //生成できないならランダム移動に遷移
                owner.ChangeState(AIState_Titania_T.RandomMove_T);
            }
        }
    }
}