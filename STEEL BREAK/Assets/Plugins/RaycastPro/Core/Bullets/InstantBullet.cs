using RaycastPro.RaySensors;

namespace RaycastPro.Bullets
{
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Bullets/" + nameof(InstantBullet))]
    public sealed class InstantBullet : Bullet
    {
        [Tooltip("弾丸の衝突時に発生するオフセットは、ヒット方向の[反転ベクトル]として算出されます。")]
        public float hitOffset = .1f;

        [Tooltip("弾が外れた際に終了処理を実行し、レイの先端に弾丸が残らないようにします。")]
        public bool endOnMiss = true;
        
        [Tooltip("対象オブジェクトが動く場合にこのオプションを有効にすると、弾丸がペアレント化され、オブジェクトと一緒に移動します。")]
        public bool forceToParentHit;

        [Tooltip("Planar Sensitive と同様の挙動をし、最後に生成されたクローンに配置されます。")]
        public bool throughClones = true;

        internal override void RuntimeUpdate() => UpdateLifeProcess(GetDelta(timeMode));

        private RaySensor lastRay;
        private Vector3 hitDirection;
        
        protected override void OnCast()
        {
            lastRay = throughClones && raySource.planarSensitive ? raySource.LastClone : raySource;
            
            if (lastRay.hit.transform)
            {
                hitDirection = lastRay.HitDirection.normalized;
                transform.position = lastRay.TipTarget - hitDirection * hitOffset;
                transform.forward = hitDirection;
                
                if (forceToParentHit) transform.SetParent(lastRay.hit.transform, true);
                    InvokeDamageEvent(raySource.hit.transform);
            }
            else if (endOnMiss)
            {
                 OnEndCast(caster);
            }
        }
        protected override void CollisionBehaviour() { }
#if UNITY_EDITOR
        internal override string Info => "センサー・レイの先端で検出されたターゲットに向けて即時射撃を行います。" + HDependent;
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(hitOffset)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(endOnMiss)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(throughClones)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(forceToParentHit)));
            }
            if (hasGeneral) GeneralField(_so);

            if (hasEvents) EventField(_so);

            if (hasInfo) InformationField();
        }
#endif

    }
}