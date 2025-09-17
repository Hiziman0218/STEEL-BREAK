using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    public class CeackGuard : State<FairyAI>
    {
        //コンストラクタ
        public CeackGuard(FairyAI owner) : base(owner) { }
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
                foreach (var fairy in UnityEngine.Object.FindObjectsOfType<FairyAI>())
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
                    owner.ChangeState(AIState_Fairy.Guard);
                    return;
                }
            }

            //空きがなければ攻撃しに行く
            //owner.ChangeState(AIState_Fairy.RushShot);
        }

        public override void Exit()
        {

        }
    }
}