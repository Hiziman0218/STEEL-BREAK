using UnityEngine;


namespace StateMachineAI
{
    public class Caution : State<GunBatteryAI>
    {
        //コンストラクタ
        public Caution(GunBatteryAI owner) : base(owner) { }
        private float nextChangeTime = 0f;
        private Quaternion randomTarget;
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("警戒中");
        }

        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //一定時間ごとに向く方向をランダムで決める処理
            if (Time.time > nextChangeTime)
            {
                // 新しいランダム方向を決める
                randomTarget = Quaternion.Euler(
                    Random.Range(owner.minPitchAngle, owner.maxPitchAngle),
                    Random.Range(0f, 360f),
                    0f
                );
                nextChangeTime = Time.time + 2f; // 2秒ごとに方向を変える
            }

            //砲身を動かす
            foreach (Transform muzzle in owner.m_Muzzles)
            {
                muzzle.rotation = Quaternion.Slerp(muzzle.rotation, randomTarget, Time.deltaTime * owner.m_rotationSpeedV);
            }

            //攻撃可能範囲に入った
            if (Vector3.Distance(owner.m_Player.position, owner.transform.position) < owner.m_AttackDistance)
            {
                owner.ChangeState(AIState_GunBatteryAI.Attack);
            }
        }
        public override void Exit()
        {

        }
    }
}