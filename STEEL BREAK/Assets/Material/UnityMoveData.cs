using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "キャラクター移動データ", menuName = "GameData/キャラクター移動データ")]
public class UnityMoveData : ScriptableObject
{
    [Header("AnimationPanelリンク")]
    public GameObject m_AnimationPanel;

    [Header("AnimationPanelのMaterialリンク")]
    public Material m_Material;

    [Header("AnimationPanelの親の物理リンク")]
    public Rigidbody m_Rigidbody;

    [Header("左右アニメーション")]
    public List<Sprite> m_LR_Animetion;
    [Header("上アニメーション")]
    public List<Sprite> m_Up_Animetion;
    [Header("下アニメーション")]
    public List<Sprite> m_Down_Animetion;
    [Header("実行中のAnimation番号")]
    public int m_AnimationNo = 0;

    [Header("パターンアニメーション切り替え時間")]
    public float m_AnimationTime = 0;
    [Header("パターンアニメーション切り替え最大時間")]
    public float m_AnimationMaxTime = 0.2f;
    [Header("キャラクターの移動速度")]
    public float m_Speed = 1.2f;

    //イーナムによる向き情報
    public enum Muki
    {
        IsUp = 0,
        IsDown = 1,
        IsLeft = 2,
        IsRight = 3,
        IsNo = -1,
    }
    [Header("向き情報")]
    public Muki m_MukiData = Muki.IsRight;


    public void SetUp(UnityMoveData UMD, GameObject AnimationPanel,Material material)
    {

        m_LR_Animetion = UMD.m_LR_Animetion;
        m_Up_Animetion = UMD.m_Up_Animetion;
        m_Down_Animetion = UMD.m_Down_Animetion;
        m_Speed = UMD.m_Speed;
        m_AnimationTime = m_AnimationMaxTime = UMD.m_AnimationMaxTime;

        //AnimationPanelを取得
        m_AnimationPanel = AnimationPanel;
        //AnimationPanelのMaterial取得
        m_Material = material;


    }

    /// <summary>
    /// 十字キー入力
    /// </summary>
    /// <param name="InputPoint">キー入力</param>
    public void MoveAnimetion(Vector2 InputPoint)
    {
        //アクションPanelが無い場合は処理せずエラーを返す
        if (!m_AnimationPanel)
        {
            Debug.LogError("AnimationPanelが存在しません!");
            return;
        }
        //入力していない
        if (InputPoint.x == 0 && InputPoint.y == 0)
        {
            Move(InputPoint);
            //処理終了
            return;
        }
        else
        {
            //アニメーションNoの加算処理
            if (m_AnimationTime <= 0)
            {
                //アニメーションを一つ進める
                m_AnimationNo++;
                //アニメーション時間を初期化
                m_AnimationTime = m_AnimationMaxTime;
            }
            else
            {
                //アニメーション時間を減少させる
                m_AnimationTime -= 1.0f * Time.deltaTime;
            }

            //上下入力が0ではない(優先)
            if (InputPoint.y != 0)
            {
                //上下にアニメーション移動パターンセット
                //上なら上アニメ、下なら下アニメを更新
                if (InputPoint.y > 0)
                    AnimationSet(m_Up_Animetion);
                else
                    AnimationSet(m_Down_Animetion);
            }
            else if (InputPoint.x != 0)
            {
                //左右共通にアニメーション移動パターンセット
                //右パターンのみ更新
                AnimationSet(m_LR_Animetion);
            }
            //AnimationPanel反転処理
            MukiSetUp(InputPoint);
            //移動処理
            Move(InputPoint);
        }
    }
    /// <summary>
    /// アニメーションをセットする
    /// </summary>
    /// <param name="Data">Animationデータ</param>
    public void AnimationSet(List<Sprite> Data)
    {
        //アニメーションがデータリストを超えている場合は0にする。
        if (m_AnimationNo >= Data.Count)
            m_AnimationNo = 0;

        //アニメーション更新
        m_Material.mainTexture = 
            Data[m_AnimationNo].texture;

    }

    public void MukiSetUp(Vector2 InputPoint)
    {
        //現在の向きを代入
        Muki muki = m_MukiData;
        //以後は入力方向に従ってイーナムを修正(入力該当が無い場合は、そのまま)
        if (InputPoint.y > 0)
            muki = Muki.IsUp;
        else if (InputPoint.y < 0)
            muki = Muki.IsDown;
        else if (InputPoint.x > 0)
            muki = Muki.IsRight;
        else if (InputPoint.x < 0)
            muki = Muki.IsLeft;

        //処理終了(入力先が前と同じ)
        if (m_MukiData == muki)
            return;

        //向き更新
        m_MukiData = muki;
        //向きを正規化
        m_AnimationPanel.transform.rotation = m_AnimationPanel.transform.parent.rotation;

        //左入力の場合、反転処理
        if (m_MukiData == Muki.IsLeft)
            m_AnimationPanel.transform.Rotate(new Vector3(0, 180, 0));
    }
    /// <summary>
    /// 移動処理
    /// </summary>
    /// <param name="InputPoint"></param>
    public void Move(Vector2 InputPoint)
    {
        //AnimationPanelの上のオブジェクトが本体なので、本体を取得
        Transform Dummy = m_AnimationPanel.transform.parent;
        //移動方向に対して、スピード値を掛けて移動力を得る
        Vector2 Speed = InputPoint * m_Speed;
        //本体にローカル移動で移動させる
        //Dummy.GetComponent<Rigidbody>().linearVelocity =
        //    Dummy.right * Speed.x +
        //    Dummy.up * Speed.y;
    }
}
