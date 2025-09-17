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
    /// 敵のステートリスト
    /// ここでステートを登録していない場合、
    /// 該当する行動が全くできない。
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
        [Header("プレイヤー")]
        public Transform m_Player;
        [Header("自爆開始距離")]
        [Range(15f, 50f)]
        public float m_AttackDistance = 30;
        [Header("自爆前の最大突進スピード")]
        [Range(10f, 200f)]
        public float m_maxspeed = 100f;
        [Header("自爆までの猶予")]
        [Range(1f, 10f)]
        public float m_explosion_count = 3f;
        [Header("加速度")]
        [Range(10f, 50f)]
        public float m_acceleration = 20f;
        [Header("追従補正（値が小さいほど緩く追従する）")]
        [Range(0.001f, 0.005f)]
        public float m_turnsmooth = 0.001f;


        [HideInInspector]
        public Rigidbody m_Rigidbody;
        [HideInInspector]
        public BoxCollider m_BoxCollider;
        [HideInInspector]
        // 自分専用ユニット
        public GameObject myAgent;
        [HideInInspector]
        // 現在速度を保持
        public float m_currentspeed = 0f;


        void OnCollisionEnter(Collision collision)
        {
            // プレイヤーか壁など、何かに当たったら自爆ステートに遷移
            ChangeState(AIState_BombAI.Explosion);
        }

        void Start()
        {
            //プレイヤーをタグで検索して取得
            m_Player = GameObject.FindWithTag("Player")?.transform;

            //アタッチしているスプリクトの自動取得
            AutoComponentInitializer.InitializeComponents(this);

            m_Rigidbody = GetComponent<Rigidbody>();
            m_BoxCollider = GetComponent<BoxCollider>();

            //存在していないクラスが指定されたら本体消滅
            if (!AddStateByName("Chase_BombAI"))
                Destroy(gameObject);
            if (!AddStateByName("Ramming_BombAI"))
                Destroy(gameObject);
            if (!AddStateByName("Explosion"))
                Destroy(gameObject);

            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<BombAI>();

            //初期起動時は、プレイヤーを追いかける状態に移行させる
            ChangeState(AIState_BombAI.Chase);
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
                if (!typeof(State<BombAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<EnemyAI> 型ではありません。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(BombAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<BombAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<BombAI>;

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
