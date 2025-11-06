using Plugins.RaycastPro.Demo.Scripts;
using RaycastPro.Detectors;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using Unity.VisualScripting;
using UnityEngine;

namespace StateMachineAI
{
    public class Explosion : State<BombAI>
    {
        //コンストラクタ
        public Explosion(BombAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            // 爆風を生成
            if (owner.blastPrefab != null)
            {
                GameObject blastObj = Object.Instantiate(
                    owner.blastPrefab,
                    owner.transform.position,
                    Quaternion.identity
                );
            }

            //オブジェクト削除
            Object.Destroy(owner.gameObject);

        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
        }
        public override void Exit()
        {

        }
    }
}