using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine.AI;
//using Unity.VisualScripting;

namespace StateMachineAI
{
    /// <summary>
    /// 敵のステートリスト
    /// ここでステートを登録していない場合、
    /// 該当する行動が全くでなきい。
    /// </summary>
    /// 
    public enum AIState_ABType
    {
        Idle,
        Wandering,
        Chase,
        Attack,
        AttackMode,
        RotMove,
    }


    public class AITester 
        : StatefulObjectBase<AITester, AIState_ABType>
    {
        //ナビメッシュ
        public NavMeshAgent agent;
        //クールダウン管理用
        public CoolDown m_CoolDown;
        public RandomMove m_RondomMove;
        public Lookat m_RotMove;
        //キャラクター
        public Transform m_Player;
        //パトロール範囲
        public float m_PatrolRadius;
        //チェイスモードになる範囲
        public float m_ChaseDistanse;
        //攻撃モードになる範囲
        public float m_AttackDistanse;
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            m_CoolDown = GetComponent<CoolDown>();
            m_RondomMove = GetComponent<RandomMove>();
            m_RotMove = GetComponent<Lookat>();

            //存在していないクラスが指定されたら本体消滅
            if (!AddStateByName("Idle"))
                Destroy(gameObject);
            if (!AddStateByName("Wandering"))
                Destroy(gameObject);
            if (!AddStateByName("Chase"))
                Destroy(gameObject);
            if (!AddStateByName("Attack"))
                Destroy(gameObject);
            if (!AddStateByName("AttackMode"))
                Destroy(gameObject);

            //前回の奴
            //S_TypeA ステートを登録する(ステートリスト0番目)
            //stateList.Add(new S_TypeA(this));
            //S_TypeB ステートを登録する(ステートリスト1番目)
            //stateList.Add(new S_TypeB(this));

            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<AITester>();

            //初期起動時は、待機状態に移行させる
            ChangeState(AIState_ABType.Idle);
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

                // 型が State<AITester> かどうかをチェック
                if (!typeof(State<AITester>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<EnemyAI> 型ではありません。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(AITester) });
                

                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<AITester> StateInstance = 
                    Constructor.Invoke(new object[] { this }) as State<AITester>;

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
