using RaycastPro.Detectors;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace StateMachineAI
{
    //�q�@���o��
    public class Spawn : State<Titania>
    {
        public float m_PopCoolTime;
        public float m_PopMaxCoolTime;
        //�R���X�g���N�^
        public Spawn(Titania owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            Debug.Log("�G������");
            m_PopCoolTime = 0.0f;
            owner.m_spawnedAttackEnemies.RemoveAll(item => item == null);
            owner.m_spawnedDefensEnemies.RemoveAll(item => item == null);
        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {

            if (m_PopCoolTime <= 0.0f)
            {
                m_PopCoolTime = m_PopMaxCoolTime;
                if (owner.m_spawnedAttackEnemies.Count < owner.m_MaxAttackFairys)
                    EnemySpwon(true);
                if (owner.m_spawnedDefensEnemies.Count < owner.m_MaxDefensFairys)
                    EnemySpwon(false);
                if (owner.m_spawnedAttackEnemies.Count >= owner.m_MaxAttackFairys &&
                    owner.m_spawnedDefensEnemies.Count >= owner.m_MaxDefensFairys)
                {
                    owner.ChangeState(AIState_Titania.Idle);
                }
            }
            else
            {
                m_PopCoolTime-= Time.deltaTime;
            }

        }
        public override void Exit()
        {
        }
        public void EnemySpwon(bool Flag)
        {
            GameObject Dummy = GameObject.Instantiate(
             owner.m_Fairys,
             owner.m_SpawnPoints[Random.Range(0, owner.m_SpawnPoints.Count)].transform.position,
             owner.transform.rotation);
            if (Flag)
            {
                owner.m_spawnedAttackEnemies.Add(Dummy);
            }
            else
            {
                if (Dummy.GetComponent<StateMachineAI.FairyAI>())
                    GameObject.Destroy(Dummy.GetComponent<StateMachineAI.FairyAI>());
                GameObject Dummy2 = GameObject.Instantiate(
                    owner.m_DefensEnemySenterObject,
                    owner.transform.position,
                    owner.transform.rotation);
                Dummy2.transform.parent = owner.transform;
                if (!Dummy.GetComponent<RootMove>())
                {
                    Dummy.AddComponent<RootMove>();
                }
                Dummy.GetComponent<RootMove>().m_RootTaerget = Dummy2.transform.GetChild(0);
                owner.m_spawnedDefensEnemies.Add(Dummy);
            }
        }
    }
}