using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Vector3 m_MovePoint;   //移動用
    public Animator m_animator;   //アニメーター
    public Rigidbody m_rigidbody; //リジッドボディ

    public bool IsFireinRightHand { get; private set; } //右手武装の攻撃の攻撃の入力受け取り
    public bool IsFireinLeftHand { get; private set; }  //左手武装の攻撃の入力受け取り
    public bool IsBoost { get; private set; }           //ブーストの入力受け取り
    public bool IsBoostDash { get; private set; }       //ブーストダッシュの入力受け取り
    public bool IsJump { get; private set; }            //上昇の入力受け取り
    public bool IsJumpDown { get; private set; }        //上昇の入力受け取り開始
    public bool IsJumpUp { get; private set; }          //上昇の入力受け取り終了
    public bool IsFall { get; private set; }            //自由落下の入力受け取り
    public bool IsLockOnCancel { get; private set; }    //ロックオン状態解除の入力受け取り
    public bool IsTargetChange {  get; private set; }   //ターゲット切り替えの入力受け取り
    public bool IsReload { get; private set; }          //リロードするか(手動リロード)


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
        m_MovePoint = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

        //アニメーション制御
        m_animator.SetFloat("X", m_MovePoint.x);
        m_animator.SetFloat("Y", m_MovePoint.z);

        IsFireinRightHand = Input.GetKey(KeyCode.E);    //Eキーを押している間は右手武装の使用
        IsFireinLeftHand = Input.GetKey(KeyCode.Q);     //Qキーを押している間は右手武装の使用
        IsBoost = Input.GetMouseButton(1);              //右クリックを押している間はブースト(加速)
        IsBoostDash = Input.GetMouseButtonDown(1);      //右クリックを押した瞬間はブースト(初期加速)
        IsJump = Input.GetKey(KeyCode.Space);           //Spaceキーを押している間は上昇
        IsJumpDown = Input.GetKeyDown(KeyCode.Space);   //Spaceキーを押した瞬間ジャンプ入力の計測開始
        IsJumpUp = Input.GetKeyUp(KeyCode.Space);       //Spaceキーを離した瞬間ジャンプ入力の計測終了
        IsFall = Input.GetKeyDown(KeyCode.C);           //Cキーを押すと自由落下
        IsLockOnCancel = Input.GetKeyDown(KeyCode.Tab); //Tabキーを押すとロックオン機能を使わない
        IsTargetChange = Input.GetKeyDown(KeyCode.V);   //Vキーを押すとターゲット切り替え
        IsReload = Input.GetKeyDown(KeyCode.R);         //Rキーを押すと手動リロード
    }
}
