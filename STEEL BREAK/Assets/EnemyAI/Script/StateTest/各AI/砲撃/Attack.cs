using UnityEngine;

namespace StateMachineAI
{
    public class Attack_GunBatteryAI : State<GunBatteryAI>
    {
        //コンストラクタ
        public Attack_GunBatteryAI(GunBatteryAI owner) : base(owner) { }
        // 角度閾値を共通化
        float angleThreshold = 15f;

        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            
        }

        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //プレイヤーがなければリターン
            if (owner.m_Player == null) return;

            //砲身の上下移動
            foreach (Transform muzzle in owner.m_Muzzles)
            {
                LookVertical.Look_Vertical(muzzle, owner.m_Player, owner.minPitchAngle, owner.maxPitchAngle, owner.m_rotationSpeedV);
            }
            //砲台の横移動
            Lookhorizontal.Look_horizontal(owner.m_RotPoint, owner.m_Player, owner.m_rotationSpeedH);

            //攻撃可能かのチェック
            (float distance, _, _) = Distance_Check.Check(owner.transform, owner.m_Player);

            //クールダウン中でなければ
            if (owner.m_CoolDown != null && !owner.m_CoolDown.IsCoolDown("Attack"))
            {
                foreach (Transform muzzle in owner.m_Muzzles)
                {
                    // プレイヤー方向ベクトル
                    Vector3 toPlayer = (owner.m_Player.position - muzzle.position).normalized;

                    // 砲身 forward とプレイヤー方向の角度差を計算
                    float angle = Vector3.Angle(muzzle.forward, toPlayer);

                    // angleThresholdの角度内なら「狙えている」と判定＆攻撃可能距離なら
                    if (angle < angleThreshold && distance <= owner.m_AttackDistance)
                    {
                        owner.StartCoroutine(Attack_Shots.ShotBoth(owner.m_Enemy, owner.m_CoolDown, 4f));
                    }
                }
            }

            //攻撃可能範囲から出た
            if (Vector3.Distance(owner.m_Player.position, owner.transform.position) > owner.m_AttackDistance)
            {
                owner.ChangeState(AIState_GunBatteryAI.Caution);
            }
        }
        public override void Exit()
        {

        }
    }
}