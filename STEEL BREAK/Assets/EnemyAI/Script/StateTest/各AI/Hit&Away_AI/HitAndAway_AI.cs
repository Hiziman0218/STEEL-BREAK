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
using RaycastPro.Detectors;
using Plugins.RaycastPro.Demo.Scripts;
using static UnityEngine.GraphicsBuffer;

namespace StateMachineAI
{
    /// <summary>
    /// 敵のステートリスト
    /// ここでステートを登録していない場合、
    /// 該当する行動が全くできない。
    /// </summary>
    /// 
    public enum AIState_HitAndAwayAI
    {
        Chase,
        Attack,
        Away,
        Return,
    }

    public class HitAndAwayAI
        : StatefulObjectBase<HitAndAwayAI, AIState_HitAndAwayAI>
    {
        [Header("プレイヤー")]
        public Transform m_Player;
        [Header("エネミーモデル")]
        public Transform m_EnemyModel;
        [Header("センターポイントの取得")]
        public GameObject m_CenterMarker;
        [Header("攻撃可能距離")]
        public float m_AttackDistance = 10;
        [Header("正面の攻撃可能角度[-1 = 完全に背後, 0 = 真横, 1 = 正面]")]
        public float m_forwardDotThreshold = 0.8f;

        [Header("突撃時の最大突進スピード")]
        [Range(10f, 40f)]
        public float m_maxspeed = 20f;
        [Header("加速度")]
        [Range(10f, 100f)]
        public float m_acceleration = 40f;
        [Header("追従補正（値が小さいほど緩く追従する）")]
        [Range(0.001f, 0.1f)]
        public float m_turnsmooth = 0.005f;

        [HideInInspector]
        public CoolDown m_CoolDown;
        [HideInInspector]
        public Rigidbody m_Rigidbody;
        [HideInInspector]
        // 自分専用ユニット
        public GameObject myAgent;
        [HideInInspector]
        //現在スピード
        public float m_currentspeed = 0;
        void Start()
        {
            //プレイヤーをタグで検索して取得
            m_Player = GameObject.FindWithTag("Player")?.transform;

            //センターポインターを個別に取得する
            m_CenterMarker = PoolManager.Instance.Get("CenterPoint", transform.position + transform.forward, m_Player);

            //agent生成
            myAgent = PoolManager.Instance.Get("FlyingFollowing", transform.position + transform.forward, m_Player);

            //アタッチしているスプリクトの自動取得
            AutoComponentInitializer.InitializeComponents(this);
            m_Rigidbody = GetComponent<Rigidbody>();

            //存在していないクラスが指定されたら本体消滅
            if (!AddStateByName("Chase"))
                Destroy(gameObject);
            if (!AddStateByName("Attack"))
                Destroy(gameObject);
            if (!AddStateByName("Away"))
                Destroy(gameObject);
            if (!AddStateByName("Return"))
                Destroy(gameObject);

            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<HitAndAwayAI>();

            //初期起動時は、プレイヤーを追いかける状態に移行させる
            ChangeState(AIState_HitAndAwayAI.Chase);
        }

        /// <summary>
        /// クラス名を元にステートを生成して追加する
        /// </summary>
        /// <param name="ClassName">生成するクラスの名前</param>
        public bool AddStateByName(string ClassName)
        {
            try
            {
                // 現在のアセンブリからクラスを取得
                Type StateType = Assembly.GetExecutingAssembly().GetType($"StateMachineAI.{ClassName}");

                // クラスが見つからなかった場合の対処
                if (StateType == null)
                {
                    Debug.LogError($"{ClassName} クラスが見つかりませんでした。");
                    return true;
                }

                // 型が State<GunBattery_AI> かどうかをチェック
                if (!typeof(State<HitAndAwayAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<EnemyAI> 型ではありません。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(HitAndAwayAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<HitAndAwayAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<HitAndAwayAI>;

                if (StateInstance != null)
                {
                    // ステートリストに追加
                    stateList.Add(StateInstance);
                    Debug.Log($"{ClassName} をステートリストに追加しました。");
                    return true;
                }
                else
                {
                    Debug.LogError($"{ClassName} のインスタンス生成に失敗しました。");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"エラーが発生しました。: {ex.Message}");
                return false;
            }
        }

    }
}
