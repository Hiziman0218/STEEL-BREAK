using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Vector3 m_MovePoint;   //移動用
    public Animator m_animator;   //アニメーター
    public Rigidbody m_rigidbody; //リジッドボディ

    public bool IsFireRightHand { get; private set; }  //右手武装の攻撃の攻撃の入力受け取り
    public bool IsFireLeftHand { get; private set; }   //左手武装の攻撃の入力受け取り
    public bool IsFireRightBack {  get; private set; } //右背面武装の攻撃の入力受け取り
    public bool IsFireLeftBack {  get; private set; }  //左背面武装の攻撃の入力受け取り
    public bool IsReloadRightHand {  get; private set; } //右手武装のリロード入力受け取り
    public bool IsReloadLeftHand {  get; private set; }  //左手武装のリロード入力受け取り
    public bool IsReloadRightBack { get; private set; }  //右背面武装のリロード入力受け取り
    public bool IsReloadLeftBack { get; private set; }   //左背面武装のリロード入力受け取り
    public bool IsBoost { get; private set; }            //ブーストの入力受け取り
    public bool IsBoostDash { get; private set; }        //ブーストダッシュの入力受け取り
    public bool IsJump { get; private set; }             //上昇の入力受け取り
    public bool IsJumpDown { get; private set; }         //上昇の入力受け取り開始
    public bool IsJumpUp { get; private set; }           //上昇の入力受け取り終了
    public bool IsFall { get; private set; }             //落下の入力受け取り
    public bool IsFallDown {  get; private set; }        //落下の入力受け取り開始
    public bool IsFallUp {  get; private set; }          //落下の入力受け取り終了
    public bool IsLockOnCancel { get; private set; }     //ロックオン状態解除の入力受け取り
    public bool IsTargetChange {  get; private set; }    //ターゲット切り替えの入力受け取り
    public bool IsReload { get; private set; }           //リロードするか(手動リロード)


    private void Start()
    {
        //アニメーターを取得
        m_animator = GetComponent<Animator>();
        //リジッドボディを取得
        m_rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //座標制御
        m_MovePoint = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));

        //アニメーション制御
        m_animator.SetFloat("X", m_MovePoint.x);
        m_animator.SetFloat("Y", m_MovePoint.z);

        IsFireRightHand = Input.GetMouseButton(1) && !IsReload;  //右クリックを押していてリロード入力をしていない間は右手武装の使用
        IsFireLeftHand = Input.GetMouseButton(0) && !IsReload;   //左クリックを押していてリロード入力をしていない間は左手武装の使用
        IsFireRightBack = Input.GetKey(KeyCode.E) && !IsReload;  //Eキーを押していてリロード入力をしていない間は右背面武装の使用
        IsFireLeftBack = Input.GetKey(KeyCode.Q) && !IsReload;   //Qキーを押していてリロード入力をしていない間は左背面武装の使用
        IsReloadRightHand = Input.GetMouseButton(1) && IsReload; //右クリックを押していてリロード入力をしている間は右手武装のリロード
        IsReloadLeftHand = Input.GetMouseButton(0) && IsReload;  //左クリックを押していてリロード入力をしている間は左手武装のリロード
        IsReloadRightBack = Input.GetKey(KeyCode.E) && IsReload; //Eキーを押していてリロード入力をしている間は右背面武装のリロード
        IsReloadLeftBack = Input.GetKey(KeyCode.Q) && IsReload;  //Qキーを押していてリロード入力をしている間は左背面武装のリロード
        IsBoost = Input.GetKey(KeyCode.LeftShift);          //左shiftを押している間はブースト(加速)
        IsBoostDash = Input.GetKeyDown(KeyCode.LeftShift);  //左shiftを押した瞬間はブースト(初期加速)
        IsJump = Input.GetKey(KeyCode.Space);               //Spaceキーを押している間は上昇
        IsJumpDown = Input.GetKeyDown(KeyCode.Space);       //Spaceキーを押した瞬間ジャンプ入力の計測開始
        IsJumpUp = Input.GetKeyUp(KeyCode.Space);           //Spaceキーを離した瞬間ジャンプ入力の計測終了
        IsFall = Input.GetKey(KeyCode.LeftControl);         //左ctrlキーを押すと落下
        IsFallDown = Input.GetKeyDown(KeyCode.LeftControl); //左ctrlキーを押した瞬間落下入力の計測開始
        IsFallUp = Input.GetKeyUp(KeyCode.LeftControl);     //左ctrlキーを離した瞬間落下入力の計測終了
        IsLockOnCancel = Input.GetKeyDown(KeyCode.Tab);     //Tabキーを押すとロックオン機能使用/不使用を切り替え
        IsTargetChange = Input.GetKeyDown(KeyCode.F);       //Fキーを押すとターゲット切り替え
        IsReload = Input.GetKey(KeyCode.R);                 //Rキーを押している間手動リロード待機
    }

    /// <summary>
    /// 移動しているか
    /// </summary>
    /// <returns></returns>
    public bool IsMoving()
    {
        // 入力ベクトルの長さがある程度以上なら「移動中」
        return m_MovePoint.sqrMagnitude > 0.01f;
    }
}