using UnityEngine;


namespace StateMachineAI
{
    public class Caution : State<GunBatteryAI>
    {
        //コンストラクタ
        public Caution(GunBatteryAI owner) : base(owner) { }
        private float nextChangeTime = 0f;
        private Quaternion randomTargetYaw;
        private Quaternion randomTargetPitch;

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
                float pitch = Random.Range(owner.minPitchAngle, owner.maxPitchAngle); // 縦方向ランダム
                float yaw = Random.Range(0f, 360f);

                // 横回転用（台座）
                randomTargetYaw = Quaternion.Euler(0f, yaw, 0f);
                // 縦回転用（砲身）※-pitchにすることでminとmaxの値が間隔的にずれることを防止（元のままだと上下反転してしまう）
                randomTargetPitch = Quaternion.Euler(-pitch, 0f, 0f);


                //次の指定ポイントを取るまでのカウントダウン
                nextChangeTime = Time.time + Random.Range(2f, 6f);
            }

            // 横方向は RotPoint を動かす
            if (owner.m_RotPoint != null)
            {
                owner.m_RotPoint.localRotation = Quaternion.RotateTowards(
                    owner.m_RotPoint.localRotation,
                    randomTargetYaw,
                    owner.m_rotationSpeedH * 30f * Time.deltaTime
                );
            }

            // 縦方向は砲身を動かす
            foreach (Transform muzzle in owner.m_Muzzles)
            {
                muzzle.localRotation = Quaternion.RotateTowards(
                    muzzle.localRotation,
                    randomTargetPitch,
                    owner.m_rotationSpeedV * 30f * Time.deltaTime
                );
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