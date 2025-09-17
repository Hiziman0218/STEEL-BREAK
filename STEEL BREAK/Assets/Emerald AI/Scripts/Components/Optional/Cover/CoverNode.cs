using UnityEngine;                                // Unity の基本API
#if UNITY_EDITOR
using UnityEditor;                                // エディタ拡張（Handles, Gizmos 用）
#endif

namespace EmeraldAI                                // EmeraldAI 名前空間
{
#if UNITY_EDITOR
    [CanEditMultipleObjects]                      // 複数選択時の編集を許可（エディタ用）
#endif
    [RequireComponent(typeof(SphereCollider))]    // 必ず SphereCollider を付与する
    // 【クラス概要】CoverNode：
    //  カバー（遮蔽）行動に使用される「カバーノード」を表すクラス。
    //  ノードの種類（しゃがみ/ピーク/立ち）や視界角、占有状態を管理し、
    //  エディタ上で可視化（フィールドオブビュー、矢印、記号）を行う。
    public class CoverNode : MonoBehaviour
    {
        [Header("カバー動作の種類（しゃがみ＆ピーク / 一度しゃがむ / 立ち）")]
        public CoverTypes CoverType = CoverTypes.CrouchAndPeak;   // カバータイプ

        [Header("視線上の位置取得を行うか（Yes=行う）")]
        public YesOrNo GetLineOfSightPosition = YesOrNo.No;       // 視線位置取得の有無

        [Header("カバー可能な視界角（度）※60～180の範囲で制限")]
        [Range(60, 180)]
        public int CoverAngleLimit = 180;                         // 視界角リミット

        [Header("矢印（ガイド）描画の色")]
        public Color ArrowColor = Color.red;                      // ガイド矢印の色

        [Header("ノード（球）描画の色（半透明）")]
        public Color NodeColor = new Color32(0, 224, 9, 154);     // ノードの色

        [Header("このノードが現在AIに占有されているか")]
        public bool IsOccupied;                                   // 占有フラグ

        [Header("このノードを現在使用中のAI（占有者）")]
        public Transform Occupant;                                // 占有者参照

        [Header("【Editor表示】設定セクションの折りたたみ")]
        public bool SettingsFoldout;                              // セクション開閉

        [Header("【Editor表示】設定群を隠す（折りたたみ制御）")]
        public bool HideSettingsFoldout;                          // 非表示切り替え

        [Header("内部参照：このノードの SphereCollider（自動設定）")]
        SphereCollider NodeCollider;                               // ノードの球コライダー

        [Header("矢印や曲線描画で使用するカーブ（0→1の補間）")]
        AnimationCurve curvature = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)); // 補間カーブ

        public void SetOccupant(Transform occupant)               // このノードを占有者に割当
        {
            Occupant = occupant;                                  // 占有者設定
            IsOccupied = true;                                    // 占有中にする
        }

        public void ClearOccupant()                                // 占有を解除
        {
            Occupant = null;                                      // 参照クリア
            IsOccupied = false;                                   // フラグ解除
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()                                // 選択中のみのGizmo描画
        {
            DrawFieldOfView();                                     // 視界角の扇形を描く
        }

        void OnDrawGizmos()                                        // 常時のGizmo描画
        {
            DrawNode();                                            // ノード本体（球）を描く

            if (NodeCollider == null) NodeCollider = GetComponent<SphereCollider>(); // コライダー取得

            NodeCollider.radius = 0.05f;                           // 半径を小さく
            NodeCollider.isTrigger = true;                         // トリガー扱い

            DrawArrow();                                           // 進行方向の矢印を描画

            // カバータイプに応じたガイドGizmoを描画
            if (CoverType == CoverTypes.CrouchAndPeak) DrawCrouchAndPeakGizmo();
            else if (CoverType == CoverTypes.CrouchOnce) DrawCrouchOnceGizmo();
            else if (CoverType == CoverTypes.Stand) DrawStandGizmo();

            Gizmos.color = NodeColor;                              // 色設定
            Gizmos.matrix = Matrix4x4.TRS(                         // 変換行列を設定
                transform.TransformPoint(NodeCollider.center),     // 中心位置（ローカル→ワールド）
                transform.rotation,                                // 回転
                transform.lossyScale);                             // スケール
        }

        void DrawFieldOfView()                                     // 視界角（カバー可能範囲）の可視化
        {
            float CircleSize = 1;                                  // 円の半径

            // 緑＝カバー可能、赤＝カバー外（視線外）
            Handles.color = new Color(0, 0.75f, 0, 1f);
            Handles.DrawWireArc(transform.position, transform.up, transform.forward, (float)CoverAngleLimit / 2f, CircleSize, 2f);
            Handles.DrawWireArc(transform.position, transform.up, transform.forward, -(float)CoverAngleLimit / 2f, CircleSize, 2f);

            Handles.color = Color.red;
            Handles.DrawWireArc(transform.position, transform.up, -transform.forward, (360 - CoverAngleLimit) / 2f, CircleSize, 2f);
            Handles.DrawWireArc(transform.position, transform.up, -transform.forward, -(360 - CoverAngleLimit) / 2f, CircleSize, 2f);

            Vector3 viewAngleA = DirFromAngle(transform, -CoverAngleLimit / 2f, false); // 左境界
            Vector3 viewAngleB = DirFromAngle(transform, CoverAngleLimit / 2f, false);  // 右境界

            Handles.color = new Color(1, 0, 0, 1f);
            if (CoverAngleLimit < 360)
            {
                Handles.DrawLine(transform.position, transform.position + viewAngleA * CircleSize, 2f); // 左境界線
                Handles.DrawLine(transform.position, transform.position + viewAngleB * CircleSize, 2f); // 右境界線
            }
            Handles.color = Color.white;                            // 色を戻す
        }

        Vector3 DirFromAngle(Transform transform, float angleInDegrees, bool angleIsGlobal) // 角度からベクトルを計算
        {
            if (!angleIsGlobal)                                     // ローカル角度なら
                angleInDegrees += transform.eulerAngles.y;          // Y回りの回転を加算
            return transform.rotation                                // 回転を反映
                 * Quaternion.Euler(new Vector3(0, -transform.eulerAngles.y, 0))
                 * new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }

        void DrawArrow()                                            // 進行方向の矢印を描画
        {
            Color arrowColor = Color.red;                           // 色
            float arrowLength = 0.55f;                              // 長さ
            float arrowHeadLength = 0.15f;                          // 矢尻の長さ
            float arrowHeadAngle = 20.0f;                           // 矢尻角

            Gizmos.color = arrowColor;

            Vector3 NodeHeight = transform.position + transform.forward * 0.2f; // 少し前方の高さ

            // 矢印の軸線
            Vector3 endPosition = NodeHeight + transform.forward.normalized * arrowLength;
            Handles.DrawLine(NodeHeight, endPosition, 4f);

            // 矢尻の左右方向
            Vector3 right = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;

            // 矢尻の描画
            Handles.DrawLine(endPosition, endPosition + right * arrowHeadLength, 4f);
            Handles.DrawLine(endPosition, endPosition + left * arrowHeadLength, 4f);
        }

        void DrawCrouchOnceGizmo()                                  // 「一度しゃがむ」タイプのガイド描画
        {
            float arrowLength = 0.55f;
            float arrowHeadLength = 0.2f;
            float arrowHeadAngle = 20.0f;
            int curveResolution = 20;
            Gizmos.color = Color.yellow;

            Vector3 startPosition = transform.position + transform.forward * 0.55f + transform.up * 0.3f;
            Vector3 forward = -transform.forward;                   // 後方へ
            Vector3 curveDir = transform.up;                        // 上方向へ
            float radius = arrowLength;

            Vector3[] points = new Vector3[curveResolution + 1];

            for (int i = 0; i <= curveResolution; i++)
            {
                float t = (float)i / curveResolution;
                float angle = (Mathf.PI / 2) * curvature.Evaluate(t); // 0→90度のカーブ
                Vector3 point = startPosition + forward * radius * Mathf.Cos(angle) + curveDir * radius * Mathf.Sin(angle);
                points[i] = point;
            }

            // 曲線本体
            Handles.color = ArrowColor;
            Handles.DrawAAPolyLine(5f, points);

            // 終端の矢尻
            Vector3 endPosition = points[curveResolution];
            Vector3 endDirection = (points[curveResolution] - points[curveResolution - 1]).normalized;
            Vector3 normal = Vector3.Cross(endDirection, transform.right).normalized;
            Vector3 right = Quaternion.AngleAxis(arrowHeadAngle, normal) * -endDirection;
            Vector3 left = Quaternion.AngleAxis(-arrowHeadAngle, normal) * -endDirection;

            Handles.DrawLine(endPosition, endPosition + right * arrowHeadLength, 4f);
            Handles.DrawLine(endPosition, endPosition + left * arrowHeadLength, 4f);

            // 「X」マーク（位置目印）
            Vector3 rightDir = transform.right;
            Vector3 forwardDir = transform.forward;
            float xSize = 0.1f;

            Vector3 topLeft = transform.position + transform.up * 0.3f + (rightDir * -xSize) + (forwardDir * xSize);
            Vector3 bottomRight = transform.position + transform.up * 0.3f + (rightDir * xSize) + (forwardDir * -xSize);

            Vector3 topRight = transform.position + transform.up * 0.3f + (rightDir * xSize) + (forwardDir * xSize);
            Vector3 bottomLeft = transform.position + transform.up * 0.3f + (rightDir * -xSize) + (forwardDir * -xSize);

            Handles.DrawLine(topLeft, bottomRight, 4f);
            Handles.DrawLine(topRight, bottomLeft, 4f);
            //X
        }

        void DrawCrouchAndPeakGizmo()                               // 「しゃがみ＆ピーク」タイプのガイド描画
        {
            float arrowLength = 0.55f;
            float arrowHeadLength = 0.2f;
            float arrowHeadAngle = 20.0f;
            int curveResolution = 20;
            Gizmos.color = Color.yellow;

            Vector3 startPosition = transform.position + transform.forward * 0.55f + transform.up * 0.3f;
            Vector3 forward = -transform.forward;                   // 後方へ
            Vector3 curveDir = transform.up;                        // 上方向へ
            float radius = arrowLength;

            Vector3[] points = new Vector3[curveResolution + 1];

            for (int i = 0; i <= curveResolution; i++)
            {
                float t = (float)i / curveResolution;
                float angle = (Mathf.PI / 2) * curvature.Evaluate(t);
                Vector3 point = startPosition + forward * radius * Mathf.Cos(angle)
                                              + curveDir * radius * Mathf.Sin(angle);
                points[i] = point;
            }

            // 曲線本体
            Handles.color = ArrowColor;
            Handles.DrawAAPolyLine(5f, points);

            // 終端の矢尻
            Vector3 endPosition = points[curveResolution];
            Vector3 endDirection = (points[curveResolution] - points[curveResolution - 1]).normalized;
            Vector3 endNormal = Vector3.Cross(endDirection, transform.right).normalized;
            Vector3 endRight = Quaternion.AngleAxis(arrowHeadAngle, endNormal) * -endDirection;
            Vector3 endLeft = Quaternion.AngleAxis(-arrowHeadAngle, endNormal) * -endDirection;

            Handles.DrawLine(endPosition, endPosition + endRight * arrowHeadLength, 4f);
            Handles.DrawLine(endPosition, endPosition + endLeft * arrowHeadLength, 4f);

            // 曲線始点側の矢尻（往復を示す）
            Vector3 startCurvePos = points[0];
            Vector3 startDirection = (points[1] - points[0]).normalized;
            Vector3 startNormal = Vector3.Cross(startDirection, transform.right).normalized;

            Vector3 startRight = Quaternion.AngleAxis(arrowHeadAngle, startNormal) * startDirection;
            Vector3 startLeft = Quaternion.AngleAxis(-arrowHeadAngle, startNormal) * startDirection;

            Handles.DrawLine(startCurvePos, startCurvePos + startRight * arrowHeadLength, 4f);
            Handles.DrawLine(startCurvePos, startCurvePos + startLeft * arrowHeadLength, 4f);
        }

        void DrawStandGizmo()                                      // 「立ち」タイプのガイド描画
        {
            float lineLength = 0.75f;                              // 線の長さ
            float xSize = 0.1f;                                    // X印のサイズ

            Gizmos.color = ArrowColor;
            Handles.color = ArrowColor;

            Vector3 startPosition = transform.position + transform.up * 0.3f; // 始点
            Vector3 endPosition = startPosition + transform.up * lineLength; // 終点

            Handles.DrawAAPolyLine(5f, new Vector3[] { startPosition, endPosition }); // 垂直線

            Vector3 rightDir = transform.right;                   // 右方向
            Vector3 forwardDir = transform.forward;                 // 前方向

            // X印
            Vector3 topLeft = startPosition + (rightDir * -xSize) + (forwardDir * xSize);
            Vector3 bottomRight = startPosition + (rightDir * xSize) + (forwardDir * -xSize);

            Vector3 topRight = startPosition + (rightDir * xSize) + (forwardDir * xSize);
            Vector3 bottomLeft = startPosition + (rightDir * -xSize) + (forwardDir * -xSize);

            Handles.DrawLine(topLeft, bottomRight, 4f);
            Handles.DrawLine(topRight, bottomLeft, 4f);

            float arrowHeadLength = 0.2f;                           // 矢尻長
            float arrowHeadAngle = 25f;                             // 矢尻角

            Vector3 arrowDir = transform.up;                         // 矢印方向：上

            Vector3 arrowNormal = Vector3.Cross(arrowDir, transform.right).normalized; // 法線

            Vector3 arrowRight = Quaternion.AngleAxis(+arrowHeadAngle, arrowNormal) * -arrowDir; // 右矢尻
            Vector3 arrowLeft = Quaternion.AngleAxis(-arrowHeadAngle, arrowNormal) * -arrowDir; // 左矢尻

            Handles.DrawLine(endPosition, endPosition + arrowRight * arrowHeadLength, 4f);
            Handles.DrawLine(endPosition, endPosition + arrowLeft * arrowHeadLength, 4f);
        }

        void DrawNode()                                           // ノード球の描画
        {
            Gizmos.color = NodeColor;                              // 色
            Gizmos.DrawSphere(transform.position, 0.2f);           // 球を描く
        }
#endif
    }
}
