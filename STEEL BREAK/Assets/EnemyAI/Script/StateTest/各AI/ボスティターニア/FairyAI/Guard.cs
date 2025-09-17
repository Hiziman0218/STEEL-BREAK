using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    //ティターニアを守るフェアリー
    public class Guard : State<FairyAI>
    {
        //コンストラクタ
        public Guard(FairyAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //追従するターゲットの変更
            ChangeTarget.Change(owner.m_GuardPointer.transform, owner.myAgent);

            Debug.Log("ガード");
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //攻撃可能かのチェック
            (float distance, _, _) = Distance_Check.Check(owner.transform, owner.m_Player);
            //守護位置と自分の距離
            (float guarddistance, _, _) = Distance_Check.Check(owner.transform, owner.m_CenterMarker.transform);

            //Flyにy軸回転動を同期
            Quaternion yOnlyRotation = Quaternion.Euler(0, owner.myAgent.transform.rotation.eulerAngles.y, 0);
            owner.transform.rotation = yOnlyRotation;

            //攻撃可能範囲より守護位置の距離が遠ければ
            if (guarddistance > owner.m_AttackDistance)
            {
                //守護位置へ近づく
                Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);
            }
            else
            {
                /*
                //ねじれを含んだ回転処理
                Vector3 newTarget = Centering.RotAroundGuardPoint3DFixed(
                    owner.m_GuardPointer.transform.position,  // 守護ポイント
                    Time.time,                                // 時間（回転の角度に使う）
                    owner.m_Radius,                           // 半径（遠さ）
                    owner.m_RotSpeed,                         // 回転速度
                    owner.m_Vertical,                         // 上下揺れの幅
                    owner.m_Twist_x                           // X軸のねじれの幅
                );
                owner.m_CenterMarker.transform.position = newTarget;
                */
                //ガードポイントに追従
                Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);
            }

            // 攻撃範囲内に入れば
            if (distance <= owner.m_AttackDistance)
            {
                //owner.ChangeState(AIState_Fairy.RushShot);
            }

        }

        public override void Exit()
        {

        }
    }
}