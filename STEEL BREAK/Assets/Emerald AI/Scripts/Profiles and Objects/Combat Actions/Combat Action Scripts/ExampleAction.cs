using UnityEngine;
using System.Collections;

namespace EmeraldAI
{
    /// <summary>
    /// 【ExampleAction】
    /// Unity コンソールへランダムな数値を Debug.Log するサンプルのコンバットアクション。
    /// ※挙動は元ソースのまま。日本語コメントとメニュー名のみ変更しています。
    /// </summary>
    [CreateAssetMenu(fileName = "サンプルアクション", menuName = "Emerald AI/コンバットアクション/サンプルアクション")]
    public class ExampleAction : EmeraldAction
    {
        /// <summary>
        /// EmeraldAction を継続的に更新します。
        /// これは Update 相当の処理で、渡された EmeraldComponent と ActionClass の情報を用いて
        /// このアクション内で実行されます。
        /// </summary>
        public override void UpdateAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            if (!CanExecute(EmeraldComponent, ActionClass))
                return;

            Execute(EmeraldComponent, ActionClass);
        }

        /// <summary>
        /// この EmeraldAction を実行するために必要な条件。
        /// </summary>
        bool CanExecute(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            var Conditions = (((int)EnterConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
            return ActionClass.CooldownLengthTimer >= CooldownLength && Conditions && !ActionClass.IsActive;
        }

        /// <summary>
        /// CanExecute の条件を満たしたため、アクションを実行します。
        /// </summary>
        void Execute(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            // EmeraldComponent の MonoBehaviour を使って個別のコルーチンを開始します。
            // これにより、必要であればローカル変数を保持できます。
            EmeraldComponent.GetComponent<MonoBehaviour>().StartCoroutine(cAction(EmeraldComponent, ActionClass));
        }

        IEnumerator cAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            // アクションをアクティブに設定
            ActionClass.IsActive = true;

            // 渡された EmeraldComponent の MonoBehaviour からローカル変数を用いて、
            // コンソールにランダムな数を出力します。
            int LocalVariableExample = Random.Range(0, 256);
            Debug.Log(EmeraldComponent.gameObject.name + "  " + LocalVariableExample);

            // 1秒待機
            yield return new WaitForSeconds(1);

            // 1秒後に、同じ処理を再度実行
            LocalVariableExample = Random.Range(0, 256);
            Debug.Log(EmeraldComponent.gameObject.name + "  " + LocalVariableExample);

            // クールダウンをリセットし、アクションを非アクティブ化
            ActionClass.CooldownLengthTimer = 0;
            ActionClass.IsActive = false;
        }
    }
}
