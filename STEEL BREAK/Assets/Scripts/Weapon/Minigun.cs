using UnityEngine;

public class Minigun : MonoBehaviour
{
    [Header("バレル設定")]
    [Tooltip("回転基部")]
    [SerializeField] private Transform m_gunBarrel;
    [Tooltip("砲身の数")]
    [SerializeField] private int m_gunBurrelNumber;
    [Tooltip("回転速度(秒速)")]
    [SerializeField] private float m_rotateSpeed;

    [Header("回転速度制御")]
    [SerializeField] private float m_initialRotateSpeed = 60f;   //初速
    [SerializeField] private float m_maxRotateSpeed = 720f;      //最大速度
    [SerializeField] private float m_rotateAcceleration = 360f;  //加速度(度/秒^2)

    private float m_currentRotateSpeed; // 現在の回転速度

    private bool m_isUsing = false;        //現在使用されているか

    private float m_fireIntervalAngle;     //弾を発射する回転数
    private float m_accumulatedAngle = 0f; //前回の発射から何度回転したか

    private Weapon_Shooting m_shooting;    //銃クラス

    private void Start()
    {
        m_shooting = GetComponent<Weapon_Shooting>(); //銃クラス

        if(m_shooting != null)
        {
            //外部管理をするように設定
            m_shooting.ExternalControl();
        }

        //弾を発射する回転数を計算
        m_fireIntervalAngle = 360f / m_gunBurrelNumber;

        //初速を設定
        m_currentRotateSpeed = m_initialRotateSpeed;
    }

    private void Update()
    {
        if(m_shooting != null)
        {
            //外部管理用フラグをfalseに設定し、使用しているかのフラグを取得
            m_shooting.SetIsFire(false);
            m_isUsing = m_shooting.GetIsUsing();
        }

        /*
        //使用中は常に回転
        if (m_isUsing)
        {
            //バレルを回転
            m_gunBarrel.Rotate(0f, m_rotateSpeed * Time.deltaTime, 0f);
            //回転量を加算
            m_accumulatedAngle += m_rotateSpeed * Time.deltaTime;

            //回転した量が弾を発射する回転数に到達したら、弾を発射し現在の回転量をリセット
            if(m_accumulatedAngle >= m_fireIntervalAngle)
            {
                if(m_shooting != null)
                {
                    m_shooting.SetIsFire(true);
                }
                m_accumulatedAngle -= m_fireIntervalAngle;
            }
        }*/

        if (m_isUsing)
        {
            //徐々に加速
            m_currentRotateSpeed += m_rotateAcceleration * Time.deltaTime;
            m_currentRotateSpeed = Mathf.Min(m_currentRotateSpeed, m_maxRotateSpeed);

            //回転
            float rotateAmount = m_currentRotateSpeed * Time.deltaTime;
            m_gunBarrel.Rotate(0f, rotateAmount, 0f);
            m_accumulatedAngle += rotateAmount;

            //発射判定
            if (m_accumulatedAngle >= m_fireIntervalAngle)
            {
                if (m_shooting != null)
                {
                    m_shooting.SetIsFire(true);
                }
                m_accumulatedAngle -= m_fireIntervalAngle;
            }
        }
        else
        {
            //使用終了時は速度リセット
            m_currentRotateSpeed = m_initialRotateSpeed;
            m_accumulatedAngle = 0f;
        }
    }
}
