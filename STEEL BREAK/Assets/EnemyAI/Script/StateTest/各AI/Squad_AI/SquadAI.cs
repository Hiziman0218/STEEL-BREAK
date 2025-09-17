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
    public enum AIState_SquadAI
    {
        Attack,
        SetPosition,
        Escape,
    }


    public class SquadAI
        : StatefulObjectBase<SquadAI, AIState_SquadAI>
    {
        //ナビメッシュ
        public NavMeshAgent agent;
        //クールダウン管理用
        public CoolDown m_CoolDown;
        //キャラクター
        public Transform m_Player;
        void Start()
        {

            //存在していないクラスが指定されたら本体消滅
            if (!AddStateByName("Attack"))
                Destroy(gameObject);
            if (!AddStateByName("SetPosition"))
                Destroy(gameObject);
            if (!AddStateByName("Escape"))
                Destroy(gameObject);

            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<SquadAI>();

            //初期起動時は、別ポイントへ逃げる状態に移行させる
            ChangeState(AIState_SquadAI.Escape);
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

                // 型が State<SquadAI> かどうかをチェック
                if (!typeof(State<SquadAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<EnemyAI> 型ではありません。\nだからよ…止まるんじゃ…ねぇぞ…。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(SquadAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<SquadAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<SquadAI>;

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
