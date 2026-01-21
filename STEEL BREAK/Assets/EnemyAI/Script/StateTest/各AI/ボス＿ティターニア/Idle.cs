using UnityEngine;

namespace StateMachineAI
{
    public class Idle_T : State<Titania_T>
    {
        //コンストラクタ
        public Idle_T(Titania_T owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //Debug.Log("行動決め待機時間");
            //別ステートでエージェントを解除して取得していなければ
            if (owner.myAgent == null || !owner.myAgent.activeInHierarchy)
            {
                //エージェントがnullなら再取得する
                owner.myAgent = PoolManager.Instance.Get("Titania", owner.transform.position, owner.m_CenterMarker.transform);
            }

            //idle状態になった時の攻撃隙
            owner.m_CoolDown.StartCoolDown("Idle", 2f);

            //リジットボディがおかしくならないようにリセット
            owner.m_Rigidbody.linearVelocity = Vector3.zero;
            owner.m_Rigidbody.angularVelocity = Vector3.zero;

            //ステート遷移可能条件を追加
            foreach (var entry in owner.actionEntries)
            {
                switch (entry.state)
                {
                    //突進行動はプレイヤーが一定の高度である時
                    case AIState_Titania_T.Rush_T:
                        entry.condition = (owner) =>
                        {
                            float playerHeight = owner.m_Player.position.y;
                            return playerHeight >= owner.m_ground; // 高度m_ground以上なら突進可能
                        };
                        break;

                    //他のステートは今のところ特に条件なし
                    case AIState_Titania_T.TurnBeam_T:
                    case AIState_Titania_T.Spawn_T:
                    case AIState_Titania_T.RandomMove_T:
                        entry.condition = null; // 条件なし
                        break;
                }
            }

        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            // クールタイムが終わっていたら行動を開始
            if (!owner.m_CoolDown.IsCoolDown("Idle"))
            {
                // まず条件を満たす候補だけを抽出
                var validEntries = owner.actionEntries.FindAll(entry =>
                {
                    // クールダウンOKか？
                    bool cooldownOK = string.IsNullOrEmpty(entry.cooldownKey) || !owner.m_CoolDown.IsCoolDown(entry.cooldownKey);
                    // 条件OKか？
                    bool conditionOK = (entry.condition == null || entry.condition(owner));
                    return cooldownOK && conditionOK && entry.weight > 0f;
                });

                ///重みとか累積についてのメモ
                ///重みはｎ本の中からランダムに1本引く、くじ引きで当たる確率みたいなやつ
                ///重みが高いほど当たりやすくなり、0なら当たることはない
                ///累積は０～５、６～１０みたいに各区間を作って生成された乱数がどの区間に入っているかで決めるもの
                ///流れとしては各重みを調べて合計の中で乱数を決める
                ///例）〇が５本、△が３本、□が２本の合計１０本、累積にすると〇は０～５の区間、△は５～８、□は８～１０の区間に分けられる
                ///　　合計値１０の中からランダムな値を生成してその値が区間内のモノが選ばれる
                ///例）乱数：3.2　　選ばれたもの：〇
                //残った候補の重みの計算（攻撃確率をごちゃごちゃと決めていく）
                if (validEntries.Count > 0)
                {
                    // 合計重みを計算
                    float total = 0f;
                    foreach (var entry in validEntries)
                        total += entry.weight;

                    // 0〜total の範囲で乱数を生成
                    float rand = Random.value * total;

                    //累積の初期化
                    float cumulative = 0f;
                    // 累積で判定
                    foreach (var entry in validEntries)
                    {
                        // cumulativeに重みを足していく
                        cumulative += entry.weight;

                        // cumulativeがrandより大きければ当たり
                        if (rand < cumulative)
                        {
                            //当たったステートに遷移
                            owner.ChangeState(entry.state);
                            break;
                        }
                    }
                }
                else
                {
                    // 候補がなければフォールバック(とりあえずランダム移動するようにする)
                    owner.ChangeState(AIState_Titania_T.RandomMove_T);
                }
            }
        }

        public override void Exit()
        {
        }
    }
}