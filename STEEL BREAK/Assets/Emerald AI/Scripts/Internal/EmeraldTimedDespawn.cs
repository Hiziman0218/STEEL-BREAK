using UnityEngine;                          // Unity の基礎APIを使用

namespace EmeraldAI.Utility                  // EmeraldAI のユーティリティ用名前空間
{
    /// <summary>
    /// 【EmeraldTimedDespawn】
    /// ・一定時間が経過したら、自動的にオブジェクトプールへ返却（Despawn）するコンポーネント
    /// ・エフェクト等の一時的なオブジェクトに付与して、寿命管理を簡素化する
    /// </summary>
    public class EmeraldTimedDespawn : MonoBehaviour
    {
        [Header("自動デスポーンまでの待機秒数（この秒数を超えるとプールへ返却される）")]
        public float SecondsToDespawn = 3;  // 既定は3秒

        [Header("経過時間を累積する内部タイマー（秒）")]
        float Timer;                        // フレームごとに Time.deltaTime を加算して監視

        /// <summary>
        /// 毎フレーム呼び出し：経過時間を加算し、閾値に達したら Despawn する
        /// </summary>
        void Update()
        {
            Timer += Time.deltaTime;        // 前フレームからの経過時間を加算
            if (Timer >= SecondsToDespawn)  // 規定秒数を超えたか判定
            {
                EmeraldObjectPool.Despawn(gameObject); // オブジェクトプールへ返却（非アクティブ化・再利用待ちへ）
            }
        }

        /// <summary>
        /// 無効化時：次回再利用に備えてタイマーを0へリセット
        /// </summary>
        void OnDisable()
        {
            Timer = 0;                      // 再有効化（再スポーン）時に正しく測り直すため初期化
        }
    }
}
