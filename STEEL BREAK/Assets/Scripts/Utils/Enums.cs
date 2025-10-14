namespace Game.Enum
{
    //装備できる箇所
    public enum WeaponSlot
    {
        RightHand, //右手武装
        LeftHand,  //左手武装
        RightBack, //右背面武装
        LeftBack,  //左背面武装
    }

    //どっちの手/背面で装備した時に反転するか
    public enum AttachSide
    {
        None,  //反転しない
        Right, //右で装備した時
        Left,  //左で装備した時
    }

    //プレイヤーの移動状態
    public enum BoostState
    {
        None,   //移動していない
        Normal, //通常移動
        Dash,   //ブースト移動
    } 
}