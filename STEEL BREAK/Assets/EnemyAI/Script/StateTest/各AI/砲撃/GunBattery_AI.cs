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

namespace StateMachineAI
{
    /// <summary>
    /// 敵のステートリスト
    /// ここでステートを登録していない場合、
    /// 該当する行動が全くできない。
    /// </summary>
    /// 
    public enum AIState_GunBatteryAI
    {
        Caution,
        Attack,
    }

    public class GunBatteryAI
        : StatefulObjectBase<GunBatteryAI, AIState_GunBatteryAI>
    {
        [Header("プレイヤー")]
        public Transform m_Player;
        [Header("砲身モデル")]
        public Transform[] m_Muzzles;

        [Header("砲身の仰角制限")]
        [Range(-10f, 0f)]
        public float minPitchAngle = -5f;
        [Range(0f, 80f)]
        public float maxPitchAngle = 60f;

        [Header("砲台の横回転のラグタイム")]
        [Range(1f, 10f)]
        public float m_rotationSpeedH;
        [Header("砲身の縦回転のラグタイム")]
        [Range(1f, 10f)]
        public float m_rotationSpeedV;

        [Header("攻撃可能距離")]
        public float m_AttackDistance = 10f;

        [Header("アタッチするもの（設定する必要なし)")]
        public CoolDown m_CoolDown;

        void Start()
        {
            //プレイヤーをタグで検索して取得
            m_Player = GameObject.FindWithTag("Player")?.transform;

            //アタッチしているスプリクトの自動取得
            AutoComponentInitializer.InitializeComponents(this);

            //存在していないクラスが指定されたら本体消滅
            if (!AddStateByName("Caution"))
                Destroy(gameObject);
            if (!AddStateByName("Attack_GunBatteryAI"))
                Destroy(gameObject);

            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<GunBatteryAI>();

            //初期起動時は、プレイヤーを追いかける状態に移行させる
            ChangeState(AIState_GunBatteryAI.Caution);
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
                if (!typeof(State<GunBatteryAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<EnemyAI> 型ではありません。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(GunBatteryAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<GunBatteryAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<GunBatteryAI>;

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
