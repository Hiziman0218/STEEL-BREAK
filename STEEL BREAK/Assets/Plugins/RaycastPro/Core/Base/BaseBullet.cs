namespace RaycastPro
{
    using Bullets;
    using Bullets2D;
    using UnityEngine;
    using System;
    using UnityEngine.Events;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    public enum MoveType
    {
        Speed,
        Duration,
        Curve,
    }
    
    [Serializable]
    public class CasterEvent : UnityEvent<BaseCaster> { }

    public abstract class BaseBullet : RaycastCore
    {
        #region Public Methods

        public void UnParent(Transform child) => child.parent = null;
        public void DetachTrail()
        {
            var _trail = GetComponentInChildren<TrailRenderer>();
            if (_trail)
            {
                _trail.transform.parent = null;
            }
        }
        public void DestroyTrail(float delay)
        {
            var _trail = GetComponentInChildren<TrailRenderer>();
            if (_trail) Destroy(_trail.gameObject, delay);
        }
        public void DestroyAllTrails(float delay)
        {
            foreach (var _tR in GetComponentsInChildren<TrailRenderer>())
            {
                Destroy(_tR.gameObject, delay);
            }
        }
        public void ClearTrail()
        {
            var _trail = GetComponentInChildren<TrailRenderer>();
            _trail.Clear();
        }
        #endregion
        
        public override bool Performed { get => false; protected set {} }
        
        public TimeMode timeMode = TimeMode.DeltaTime;

        [Header("CollisionRay を使用する際は、ヒット判定処理が過剰に繰り返されないように、検出ごとに一定の間隔を設けるのが望ましいです。"),Tooltip("CollisionRay を使用する際は、ヒット判定処理が過剰に繰り返されないように、検出ごとに一定の間隔を設けるのが望ましいです。")]
        [SerializeField] protected float baseIgnoreTime = .1f;
        
        [Header("弾を個別に扱い、必要に応じて再生成できるようになります。（ArrayCasting用）"), Tooltip("弾を個別に扱い、必要に応じて再生成できるようになります。（ArrayCasting用）")]
        public string bulletID;

        /// <summary>
        /// Invoke on Cast
        /// </summary>
        public CasterEvent onCast;
        /// <summary>
        /// Ending Before Delay
        /// </summary>
        public CasterEvent onEndCast;
        /// <summary>
        /// Ending On Delay
        /// </summary>
        public CasterEvent onEnd;

        public UnityEvent onPlanar;

        //protected Coroutine RunCoroutine;
        public MoveType moveType = MoveType.Speed;

        internal float position;

        /// <summary>
        /// Set Bullet Position as "Clamp01" on the path.
        /// </summary>
        public float Position
        {
            get => position;
            set => position = Mathf.Clamp01(position);
        }
        [SerializeField] private string callMethod = "OnBullet";
        [SerializeField] private bool messageUpward;

        /// <summary>
        /// Use this variable to carry owner object reference
        /// </summary>
        public GameObject ownerReference;
        
        [SerializeField] public float damage = 10;
        public float speed = 6;

        [Header("この弾の寿命を秒数で指定します。寿命が尽きると、その弾の処理は停止されます。\r\n値が -1 の場合は無限に動作し続けます。"),Tooltip("この弾の寿命を秒数で指定します。寿命が尽きると、その弾の処理は停止されます。\r\n値が -1 の場合は無限に動作し続けます。")]
        public float lifeTime = 10;
        internal float life;
        public float Life
        {
            get => life;
            internal set => life = value;
        }

        [Header("弾が完全に終了するまでのダウンタイムです。パーティクルを保持するために、弾を短時間だけ“死んだ状態”で残しておくことができます。"), Tooltip("弾が完全に終了するまでのダウンタイムです。パーティクルを保持するために、弾を短時間だけ“死んだ状態”で残しておくことができます。")]
        public float endDelay;
        public enum EndType { Disable, Destroy }

        [SerializeField]
        public EndType endFunction = EndType.Destroy;

        [Header("Planar 以外のオブジェクトと衝突した後にこのオプションが有効になっている場合、弾の寿命は終了します。"), Tooltip("Planar 以外のオブジェクトと衝突した後にこのオプションが有効になっている場合、弾の寿命は終了します。")]
        public bool endOnCollide = true;
        public abstract void SetCollision(bool turn);

        protected abstract void CollisionRun(float delta);

        #region Updates

        internal abstract void RuntimeUpdate();
        protected void Update()
        {
            if (autoUpdate != UpdateMode.Normal) return;
            RuntimeUpdate();
        }
        protected void LateUpdate()
        {
            if (autoUpdate != UpdateMode.Late) return;
            RuntimeUpdate();
        }
        protected void FixedUpdate()
        {
            if (autoUpdate != UpdateMode.Fixed) return;
            RuntimeUpdate();
        }
        #endregion
        internal abstract void Cast<R>(BaseCaster _caster, R raySensor);
        
        internal bool ended;
        protected void OnEndCast<B>(B _caster) where B : BaseCaster// Review
        {
            if (ended) return;
            ended = true;
            
            position = 1;
            onEndCast?.Invoke(_caster);
            
            StartCoroutine(DelayRun(endDelay, () =>
            {
                if (endFunction == EndType.Destroy) // In Normal Casting
                {
                    Destroy(gameObject);
                }
                else // In Array Casting
                {
                    gameObject.SetActive(false);
                }
                onEnd?.Invoke(_caster);
            }));
        }
        
        /// <summary>
        /// This Script need to be divided for more optimizing.
        /// </summary>
        /// <param name="target"></param>
        protected void InvokeDamageEvent(Transform target)
        { if (callMethod == "") return;

            if (messageUpward)
            {
                if (this is Bullet _blt)    
                {
                    target.SendMessageUpwards(callMethod, _blt, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    target.SendMessageUpwards(callMethod, this as Bullet2D, SendMessageOptions.DontRequireReceiver);
                }
            }
            
            else
            {
                if (this is Bullet _blt)
                {
                    target.SendMessage(callMethod, _blt, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    target.SendMessage(callMethod, this as Bullet2D, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        
        /// <summary>
        /// Optimized if delta is calculated before run this method
        /// </summary>
        /// <param name="delta"></param>
        protected void UpdateLifeProcess(float delta)
        {
            if (life >= lifeTime && lifeTime >= 0)
            {
                // On End Run to End Delay
                if (this is Bullet bullet)
                {
                    OnEndCast(bullet.caster);
                }
                else if (this is Bullet2D bullet2D)
                {
                    OnEndCast(bullet2D.caster);
                }
            }
            else life += delta;
        }

#if UNITY_EDITOR
        protected const string CPositionLerp = "位置の線形補間(Position Lerp)";
        protected const string CRotationLerp = "向きの線形補間(Rotation Lerp)";
        protected const string TPositionLerp = "このアルゴリズムは、一定時間後に必ず目的地へ到達する位置計算に基づいています。例えば、Dota2 や他のストラテジーゲームに登場する遠距離キャラの投射型スキルなどに使用できます。";
        protected const string TRotationLerp = "このアルゴリズムは回転を基礎としており、弾道ミサイルのように目標へ向かって方向を変えようとしますが、必ず命中する保証はありません。";
        protected void CastTypeField(SerializedProperty _moveType, SerializedProperty _speed, SerializedProperty _duration,
            SerializedProperty _curve)
        {
            BeginVerticalBox();
            
            PropertyEnumField(_moveType, 3, CMoveType.ToContent(TMoveType), new GUIContent[]
            {
                CSpeed.ToContent("By Speed"),
                CDuration.ToContent("By Duration"),
                CCurve.ToContent("By Animation Curve"),
            });

            switch (moveType)
            {
                case MoveType.Speed:
                    EditorGUILayout.PropertyField(_speed);
                    break;
                case MoveType.Duration:
                    EditorGUILayout.PropertyField(_duration);
                    break;
                case MoveType.Curve:
                    BeginHorizontal();
                    

                    InLabelWidth(() =>
                    {
                        EditorGUILayout.CurveField(_curve, RCProEditor.Aqua, new Rect(0, 0, 1, 1), CCurve.ToContent(CCurve));

                    }, 80);
                    
                    EditorGUILayout.PropertyField(_duration, GUIContent.none, GUILayout.Width(30));

                    EndHorizontal();
                    break;
            }

            EndVertical();
        }
        protected void EventField(SerializedObject _so)
        {
            EventFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(EventFoldout, CEvents.ToContent(TEvents),
                RCProEditor.HeaderFoldout);
            if (EventFoldout) RCProEditor.EventField(_so, events); EditorGUILayout.EndFoldoutHeaderGroup();
        }

        protected static readonly string[] events = new[] {nameof(onCast), nameof(onEndCast), nameof(onEnd)};

        protected abstract void CollisionRayField(SerializedObject _so);

        protected void BulletInfoField()
        {
            InformationField(() =>
            {
                PercentProgressField(life/lifeTime, "Life2");
            });
        }
        protected void GeneralField(SerializedObject _so)
        {
            BeginVerticalBox();
            PropertyMaxField(_so.FindProperty(nameof(lifeTime)), -1f);
            PropertyMaxField(_so.FindProperty(nameof(endDelay)));
            PropertyTimeModeField(_so.FindProperty(nameof(timeMode)));
            CollisionRayField(_so);
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(endOnCollide)));
            EndVertical();
            BeginVerticalBox();
            BeginHorizontal();
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(callMethod)), "Call Method".ToContent("Calls this method on every MonoBehaviour in bullet's hit target." +
                "\nex: ----------------------------------"+
                "\npublic void OnBullet(Bullet _bullet)" +
                "\n{" +
                "\n    Add_Character_Hp(-_bullet.damage);"+
                "\n}" +
                "\n -------------------------------------"+
                "\n(Just leave it empty to cancel messaging.)"));

            MiniField(_so.FindProperty(nameof(messageUpward)), "U".ToContent("Calls the method on every ancestor of the behaviour in addition to every MonoBehaviour."));
            EndHorizontal();
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(damage)));
            EndVertical();
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(bulletID)));
            BaseField(_so);
        }
#endif
    }
}