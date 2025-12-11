using UnityEngine;

public class InvisibleWall : MonoBehaviour
{
    [SerializeField] private GameObject continuousEffectPrefab; //表示するエフェクト

    private GameObject currentEffect; //エフェクト保存用

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //1つだけエフェクト生成
            if (currentEffect == null)
            {
                ContactPoint contact = collision.contacts[0];
                Vector3 hitPoint = contact.point;
                Vector3 normal = contact.normal;

                //normal を forward にして回転を作る
                Quaternion rot = Quaternion.LookRotation(normal);

                currentEffect = Instantiate(continuousEffectPrefab, hitPoint, rot);
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && currentEffect != null)
        {
            //追突中は常に位置を更新
            ContactPoint contact = collision.contacts[0];
            Vector3 hitPoint = contact.point;
            Vector3 normal = contact.normal;

            currentEffect.transform.position = hitPoint;
            currentEffect.transform.rotation = Quaternion.LookRotation(normal); // 向き更新
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //離れたらエフェクト破棄
            if (currentEffect != null)
            {
                Destroy(currentEffect);
                currentEffect = null;
            }
        }
    }
}
