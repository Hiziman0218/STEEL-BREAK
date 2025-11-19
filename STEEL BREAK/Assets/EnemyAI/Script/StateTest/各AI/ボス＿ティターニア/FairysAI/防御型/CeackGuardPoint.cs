using UnityEngine;

namespace StateMachineAI
{
    public class CeackGuard_Gyardian : State<GyardianFairysAI>
    {
        //コンストラクタ
        public CeackGuard_Gyardian(GyardianFairysAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("ガード地点決め");
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //防衛ポイントリストの中身をすべて走査
            foreach (Transform guard in owner.m_GuardPoint)
            {
                Debug.Log("ガード地点検索中");
                bool isOccupied = false;
                
                //全てのフェアリーAIを走査して防衛ポイントに被りがないかチェック
                foreach (var fairy in Object.FindObjectsByType<GyardianFairysAI>(FindObjectsSortMode.None))
                {
                    //もし自分ではないフェアリーと被っていればtureを返して終わる
                    if (fairy != owner && fairy.m_GuardPointer == guard.gameObject)
                    {
                        Debug.Log("ガード地点被り攻撃に移行");
                        isOccupied = true;
                        break;
                    }
                }

                //誰も防衛ポイントを使ってなければ
                if (!isOccupied)
                {
                    //ガードポインタ―を対象のガードポイントに紐づけ
                    owner.m_GuardPointer = guard.gameObject;
                    owner.ChangeState(AIState_Gyardian.Guard);
                    return;
                }
            }

            //空きがなければ攻撃しに行く
            owner.ChangeState(AIState_Gyardian.Shot);
        }

        public override void Exit()
        {

        }
    }
}