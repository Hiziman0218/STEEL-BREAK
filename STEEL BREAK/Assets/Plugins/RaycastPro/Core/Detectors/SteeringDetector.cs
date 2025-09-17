using RaycastPro.RaySensors;
using UnityEngine.Serialization;

namespace RaycastPro.Detectors
{
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Detectors/" + nameof(SteeringDetector))]
    public sealed class SteeringDetector : Detector, IRadius, IPulse
    {
        [Tooltip("目的地の位置。Solver（解決システム）は可能な限り障害物を検出し、それを避けながら目的地に向かいます。")]
        public Transform destination;
        
        [Tooltip("同期用の平面方向を取得するための地面レイ（Ground Ray）")]
        public RaySensor groundRay;
        
        [Tooltip("空洞（キャビティ）を検出するために使用される補助レイの寸法です。キャラクター自身のサイズに応じて調整するのが望ましいです。」")]
        public float colliderSize = .1f;
        
        public float radius = 20f;
        
        public float stoppingDistance = 2f;
        
        [Tooltip("制限付きレイは、このオブジェクトのローカル方向に発射されます。")]
        public bool local;
        
        public float angleX = 120f;
        public float angleY = 90f;

        [Tooltip("1秒間に生成されるランダムレイの数を指定します。")]
        public int iteration = 8;
        
        
        [FormerlySerializedAs("sharpness")] [Tooltip("旋回方向の変化速度です。数値が高いほど素早く旋回し、低いほど重くゆっくりと動きます。")]
        public float moveSharpness = 6;
        
        [Tooltip("ステアリングが過去の位置に戻らないよう、通過ポイントを記録して制御する補助メモリです。数値が低いと動きはダイナミックになりますが、以前の場所を再び選んでしまう可能性が高くなります。")]
        public int markSolverCount = 6;
        
        [Tooltip("Mark Solverは、AIの進行方向を決定するための記録機能です。メモリが空になると、過去の情報が失われ、進行方向が不安定になります。")]
        public float markSolverInfluence = 1;
        
        [Tooltip("Mark Solver の記憶データを一定時間ごとに更新するための間隔（秒）です。")]
        public float markSolverRefreshTime = 1;

        
        public float obstacleNormalInfluence = 1f;
        public float obstacleDistanceInfluence = 1f;
        
        [Tooltip("このソルバーは、各イテレーションで線の終点と目的地との視線（LOS：Line of Sight）を確認することで、障害物検出の精度を向上させることができます。ただし、その分パフォーマンスコストが高くなります。")]
        public bool spiderSolver = true;
        
        public TimeMode timeMode = TimeMode.DeltaTime;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }

        public override bool Performed
        {
            get => hitCounts > 0;
            protected set { }
        }
        
        #region cached;
        private int i;
        private float delta, _dis, _cRadius;
        private float F;
        private Vector3 _pos, _randomVector, _dir, _rRadiusVector, _qVec;
        private RaycastHit _raycastHit;
        #endregion

        private Transform currentDestination;
        private int hitCounts;
        private float distValue;
        private float zeroHitOverTime;
        private float weightLocateTimer;
        /// <summary>
        /// Average Point of all detected hits.
        /// </summary>
        private Vector3 averageWeight;
        /// <summary>
        /// Average Normal of all detected hits.
        /// </summary>
        private Vector3 averageNormal;

        public Vector3 Weight => averageWeight;

        /// <summary>
        /// Non-Normalized Steering Direction
        /// </summary>
        public Vector3 RawSteeringDirection => averageNormal + (_pos - averageWeight).normalized;

        [Tooltip("ステアリング（進行）方向の正規化ベクトルです。方向のみを保持し、長さは常に1になります。")]
        public Vector3 calculatedDirection;
        
        /// <summary>
        /// Normalized Steering Direction
        /// </summary>
        public Vector3 SteeringDirection
        {
            get
            {
                if (groundRay)
                {
                    return Vector3.ProjectOnPlane(averageNormal + (_pos - averageWeight).normalized, groundRay.Performed ? groundRay.Normal: groundRay.transform.up).normalized;
                }
                
                return (averageNormal + (_pos - averageWeight).normalized).normalized;
            }
        }

        /// <summary>
        /// Steering Direction as Quaternion
        /// </summary>
        public Quaternion SteeringRotation => Quaternion.LookRotation(SteeringDirection,  groundRay ? groundRay.Normal : Vector3.up);
        public float Distance => Vector3.Distance(transform.position, destination.position);

        private readonly Queue<Vector3> weightLocate = new Queue<Vector3>();

        private float _F;
        private Vector3 _DirN;

        private bool IsDirect
        {
            get
            {
                Physics.Linecast(_pos,  destination.position, out _raycastHit, detectLayer.value, triggerInteraction);
                return !_raycastHit.transform || _raycastHit.transform == destination;
            }
        }

        public bool InDistance => Vector3.Distance(transform.position, destination.position) < stoppingDistance;

        protected override void OnCast()
        {
#if UNITY_EDITOR
            GizmoGate = null;
#endif


            if (!destination) return;
            if (Distance <= stoppingDistance)
            {
                calculatedDirection =
                    Vector3.Lerp(calculatedDirection, Vector3.zero, 1 - Mathf.Exp(-moveSharpness * delta));
                return;
            }

            _pos = transform.position;
            
            hitCounts = 0;
            delta = GetDelta(timeMode);
            
            _dir = (destination.position - _pos);
            _DirN = _dir.normalized;
            _dis = Vector3.Distance(destination.position, _pos);
            _cRadius = Mathf.Min(_dis,radius)*Random.value;
            
            if (IsDirect) // Direct Destination
            {
                Physics.SphereCast(_pos-_DirN*colliderSize, colliderSize , _dir, out _raycastHit, _dir.magnitude, detectLayer.value, triggerInteraction);
                if (!_raycastHit.transform || _raycastHit.transform == destination) // When No Obstacle
                {
                    _F = 1 - Mathf.Exp(-moveSharpness * delta);
                    averageWeight = Vector3.Lerp(averageWeight, _pos-_DirN, _F);
                    averageNormal = Vector3.Lerp(averageNormal, _DirN, _F);
                    
                    calculatedDirection = Vector3.Lerp(calculatedDirection, SteeringDirection, _F);
#if UNITY_EDITOR
                    GizmoGate += () =>
                    {
                        Handles.color = HelperColor;
                        DrawCapsuleLine(_pos, destination.position, colliderSize);
                    };
#endif
                    return;
                }
            }
            else // Mark Solver Activating
            {
                if (weightLocateTimer >= markSolverRefreshTime)
                {
                    weightLocateTimer = 0f;
                    if (weightLocate.Count >= markSolverCount) weightLocate.Dequeue();
                    weightLocate.Enqueue(_pos);
                }
                else
                {
                    weightLocateTimer += delta;
                }

                
                if (markSolverInfluence > 0)
                {
                    var _allW = Vector3.zero;
#if UNITY_EDITOR
                    _qVec = Vector3.up * (DotSize * 4f);
#endif
                    foreach (var _tVec in weightLocate)
                    {
                        _allW += _tVec;
#if UNITY_EDITOR
                        GizmoGate += () =>
                        {
                            Handles.color = HelperColor;
                            DrawLineZTest(_tVec, _tVec + _qVec);
                        };
#endif
                    }

                    if (weightLocate.Count > 0) // Mark Solver
                    {
                        _F = 1 - Mathf.Exp(-delta * markSolverInfluence);
                        averageWeight = Vector3.Lerp(averageWeight, (_allW / weightLocate.Count), _F);
                        averageNormal = Vector3.Lerp(averageNormal, (_pos - _allW / weightLocate.Count).normalized, _F);
                        
                        calculatedDirection = Vector3.Lerp(calculatedDirection, SteeringDirection, _F);
                    }
                }
            }

            // On Obstacle Solver
            for (i = 0; i < iteration; i++)
            {
                _randomVector = Quaternion.Euler(Random.Range(-angleY, angleY)/2, Random.Range(-angleX, angleX)/2, 0) * (local ? transform.forward : Vector3.forward);
                _rRadiusVector = _randomVector * _cRadius;
                if (Physics.Raycast(_pos, _randomVector, out _raycastHit, _cRadius, detectLayer.value, triggerInteraction))
                {
                    hitCounts++;
                    distValue = Mathf.Pow(_raycastHit.distance / _cRadius , 2);
                    _F = 1 - Mathf.Exp(F);
                    F = -delta * hitCounts/iteration * (1 - distValue) * moveSharpness;
                    averageWeight = Vector3.Lerp(averageWeight, _raycastHit.point, _F * obstacleDistanceInfluence);
                    averageNormal = Vector3.Lerp(averageNormal,
                        Vector3.Lerp(_raycastHit.normal*(radius-_raycastHit.distance), (destination.position-_raycastHit.point).normalized, distValue), _F * obstacleNormalInfluence);
                    
                    calculatedDirection = Vector3.Lerp(calculatedDirection, SteeringDirection, _F);
                    
#if UNITY_EDITOR
                    var _p = _raycastHit.point;
                    var _rP = _pos + _rRadiusVector;
                    GizmoGate += () =>
                    {
                        Handles.color = DetectColor;
                        DrawLineZTest(_pos, _p);
                        
                        Handles.color = BlockColor;
                        DrawLineZTest(_p, _rP, true);
                    };
#endif
                }
                else if (spiderSolver) // Spider Solver
                {
                    var _avPoint = Vector3.zero;
                    var _avDir = Vector3.zero;
                    var _spCount = 0;
                         
                    if (Vector3.Distance(_pos+_rRadiusVector, destination.position) <= _dis) 
                    {
                        Physics.Linecast(_pos + _rRadiusVector, destination.position, out _raycastHit,
                            detectLayer.value, triggerInteraction);
                        if (!_raycastHit.transform || _raycastHit.transform == destination.transform)
                        {
                            _avPoint += _pos - _randomVector;
                            _avDir += _randomVector.normalized;
                            _spCount++;
                        }
                    }

                    if (_spCount > 0)
                    {
                        _F = 1 - Mathf.Exp(-moveSharpness * delta);
                        averageWeight = Vector3.Lerp(averageWeight, _avPoint/_spCount , _F);
                        averageNormal = Vector3.Lerp(averageNormal, _avDir/_spCount, _F);
                        
                        calculatedDirection = Vector3.Lerp(calculatedDirection, SteeringDirection, _F);
                    }
                }
            }
         
            
            if  (hitCounts == 0) // On Free Move
            {
                zeroHitOverTime = Mathf.Min(zeroHitOverTime + delta, .2f);
                if (zeroHitOverTime >= .2f)
                {
                    _F = 1 - Mathf.Exp(-moveSharpness * delta);
                    averageWeight = Vector3.Lerp(averageWeight, _pos, _F);
                    averageNormal = Vector3.Lerp(averageNormal, (destination.position-transform.position).normalized, _F);
                    
                    calculatedDirection = Vector3.Lerp(calculatedDirection, SteeringDirection, _F);
                }
            }
            else
            {
                zeroHitOverTime = Mathf.Max(zeroHitOverTime - delta, 0f);
            }
        }
#if UNITY_EDITOR
        internal override string Info => "障害物を検出しながら目的地までの経路を探索します（探索が失敗することもあります）\n" + HDependent + HRDetector + HIRadius;
        internal override void OnGizmos()
        {
            EditorUpdate();
            
            if (IsGuide && IsPlaying)
            {
                DrawNormal(transform.position, calculatedDirection, "Steering Direction", DiscSize);
            }
        }
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(destination)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(stoppingDistance)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(groundRay)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(moveSharpness)));

                DetectLayerField(_so);
                BeginHorizontal();
                RadiusField(_so);
                LocalField(_so.FindProperty(nameof(local)));
                EndHorizontal();

                EditorGUILayout.PropertyField(_so.FindProperty(nameof(iteration)));
                
                PropertySliderField(_so.FindProperty(nameof(angleX)), 0f, 360f, "Arc X".ToContent("Throw rays into range of Horizontal."));
                PropertySliderField(_so.FindProperty(nameof(angleY)), 0f, 360f, "Arc Y".ToContent("Throw rays into range of Vertical."));
                
                PropertySliderField(_so.FindProperty(nameof(obstacleNormalInfluence)), 0f, 1, "Obstacle Normal Influence".ToContent());
                PropertySliderField(_so.FindProperty(nameof(obstacleDistanceInfluence)), 0f, 1, "Obstacle Distance Influence".ToContent());
                
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(colliderSize)));
                
                
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(markSolverRefreshTime)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(markSolverCount)));
                PropertySliderField(_so.FindProperty(nameof(markSolverInfluence)), 0f, 10f, "Mark Solver Influence".ToContent());
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(spiderSolver)));
                PropertyTimeModeField(_so.FindProperty(nameof(timeMode)));
            }

            if (hasGeneral)
            {
                GeneralField(_so, layerField: false);
                BaseField(_so);
            }
            
            if (hasEvents) EventField(_so);

            if (hasInfo) InformationField(PanelGate);
        }
        protected override void DrawDetectorGuide(Vector3 point) { }
#endif
    }
}