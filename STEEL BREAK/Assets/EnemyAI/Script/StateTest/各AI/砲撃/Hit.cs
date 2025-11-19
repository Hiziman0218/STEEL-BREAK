namespace StateMachineAI
{
    public class Hit_GunBatteryAI : State<GunBatteryAI>
    {
        //コンストラクタ
        public Hit_GunBatteryAI(GunBatteryAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //HitStunがクールダウン中ならreturn
            if (owner.m_CoolDown.IsCoolDown("HitStun")) return;

            // 0.5秒間は再度Hitに入らない
            owner.m_CoolDown.StartCoolDown("HitStun", 0.5f);

        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            // ヒット硬直
            if (owner.m_CoolDown.IsCoolDown("HitStun"))
                return;

            //ステート移行
            owner.ChangeState(AIState_GunBatteryAI.Caution);

        }
        public override void Exit()
        {
        }
    }
}