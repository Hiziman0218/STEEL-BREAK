using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine.AI;
//using Unity.VisualScripting;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEditorInternal;
using Plugins.RaycastPro.Demo.Scripts;

namespace StateMachineAI
{
    /// <summary>
    /// �G�̃X�e�[�g���X�g
    /// �����ŃX�e�[�g��o�^���Ă��Ȃ��ꍇ�A
    /// �Y������s�����S���ł��Ȃ��B
    /// </summary>
    /// 
    public enum AIState_BombAI
    {
        Chase,
        Ramming,
        Explosion,
    }

    public class BombAI
        : StatefulObjectBase<BombAI, AIState_BombAI>
    {
        [Header("�v���C���[")]
        public Transform m_Player;
        [Header("�����J�n����")]
        [Range(15f, 50f)]
        public float m_AttackDistance = 30;
        [Header("�����O�̍ő�ːi�X�s�[�h")]
        [Range(10f, 200f)]
        public float m_maxspeed = 100f;
        [Header("�����܂ł̗P�\")]
        [Range(1f, 10f)]
        public float m_explosion_count = 3f;
        [Header("�����x")]
        [Range(10f, 50f)]
        public float m_acceleration = 20f;
        [Header("�Ǐ]�␳�i�l���������قǊɂ��Ǐ]����j")]
        [Range(0.001f, 0.005f)]
        public float m_turnsmooth = 0.001f;


        [HideInInspector]
        public Rigidbody m_Rigidbody;
        [HideInInspector]
        public BoxCollider m_BoxCollider;
        [HideInInspector]
        // ������p���j�b�g
        public GameObject myAgent;
        [HideInInspector]
        // ���ݑ��x��ێ�
        public float m_currentspeed = 0f;


        void OnCollisionEnter(Collision collision)
        {
            // �v���C���[���ǂȂǁA�����ɓ��������玩���X�e�[�g�ɑJ��
            ChangeState(AIState_BombAI.Explosion);
        }

        void Start()
        {
            //�v���C���[���^�O�Ō������Ď擾
            m_Player = GameObject.FindWithTag("Player")?.transform;

            //�A�^�b�`���Ă���X�v���N�g�̎����擾
            AutoComponentInitializer.InitializeComponents(this);

            m_Rigidbody = GetComponent<Rigidbody>();
            m_BoxCollider = GetComponent<BoxCollider>();

            //���݂��Ă��Ȃ��N���X���w�肳�ꂽ��{�̏���
            if (!AddStateByName("Chase_BombAI"))
                Destroy(gameObject);
            if (!AddStateByName("Ramming_BombAI"))
                Destroy(gameObject);
            if (!AddStateByName("Explosion"))
                Destroy(gameObject);

            //�X�e�[�g�}�V�[�������g�Ƃ��Đݒ�
            stateMachine = new StateMachine<BombAI>();

            //�����N�����́A�v���C���[��ǂ��������ԂɈڍs������
            ChangeState(AIState_BombAI.Chase);
        }

        /// <summary>
        /// �N���X�������ɃX�e�[�g�𐶐����Ēǉ�����
        /// </summary>
        /// <param name="ClassName">��������N���X�̖��O</param>
        public bool AddStateByName(string ClassName)
        {
            try
            {
                // ���݂̃A�Z���u������N���X���擾
                Type StateType = Assembly.GetExecutingAssembly().GetType($"StateMachineAI.{ClassName}");

                // �N���X��������Ȃ������ꍇ�̑Ώ�
                if (StateType == null)
                {
                    Debug.LogError($"{ClassName} �N���X��������܂���ł����B");
                    return true;
                }

                // �^�� State<GunBattery_AI> ���ǂ������`�F�b�N
                if (!typeof(State<BombAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} �� State<EnemyAI> �^�ł͂���܂���B");
                    return true;
                }

                // �C���X�^���X�𐶐�
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(BombAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} �̃R���X�g���N�^��������܂���ł����B");
                    return true;
                }

                State<BombAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<BombAI>;

                if (StateInstance != null)
                {
                    // �X�e�[�g���X�g�ɒǉ�
                    stateList.Add(StateInstance);
                    Debug.Log($"{ClassName} ���X�e�[�g���X�g�ɒǉ����܂����B");
                    return true;
                }
                else
                {
                    Debug.LogError($"{ClassName} �̃C���X�^���X�����Ɏ��s���܂����B");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"�G���[���������܂����B: {ex.Message}");
                return false;
            }
        }

    }
}
