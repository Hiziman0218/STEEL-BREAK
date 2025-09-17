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
        //�R���X�g���N�^
        public Explosion(BombAI owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            //�I�u�W�F�N�g�폜
            UnityEngine.Object.Destroy(owner.gameObject);

        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
        }
        public override void Exit()
        {

        }
    }
}