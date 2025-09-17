using System;
using System.Collections.Generic;

namespace RaycastPro.Sensor
{
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    
    public sealed class FOVMesh : BaseUtility
    {
        public override bool Performed { get; protected set; }

        [Tooltip("The length of the field of view cone in world units.")]
        public float Length = 5f;

        [Tooltip("The size of the field of view cones base in world units.")]
        public float BaseSize = 0.5f;

        [Range(1f, 180f), Tooltip("The arc angle of the fov cone.")]
        public float FOVAngle = 90f;

        [Range(1f, 180f), Tooltip("The elevation angle of the cone.")]
        public float ElevationAngle = 90f;

        [Range(0, 8), Tooltip("The number of vertices used to approximate the arc of the fov cone. Ideally this should be as low as possible.")]
        public int Resolution = 0;

        // Returns the generated collider mesh so that it can be rendered.
        //public Mesh FOVMesh { get { return mesh; } }

        [SerializeField] private Mesh mesh;
        private Vector3[] pts = Array.Empty<Vector3>();
        private int[] triangles = Array.Empty<int>();

        protected override void OnCast()
        {
            
        }
        


#if UNITY_EDITOR
        internal override string Info => "Auxiliary tool for creating dynamic view FOV Mesh. Best suitable for Mesh Detector" + HUtility + HDependent;
        internal override void OnGizmos()
        {
            if (IsSceneView && !IsPlaying)
            {
                OnCast();
            }
        }
        
    private void DrawCleanQuadEdges(Mesh mesh)
    {
        // Gizmos.color = Color.green; // Set the color for the Gizmos
        //
        // Vector3[] vertices = mesh.vertices;
        // int[] triangles = mesh.triangles;
        //
        // HashSet<Edge> edges = new HashSet<Edge>();
        //
        // // Loop through each quad (4 vertices) and collect the outer edges
        // for (int i = 0; i < triangles.Length; i += 4)
        // {
        //     // Ensure there are enough vertices for a quad
        //     if (i + 3 < triangles.Length)
        //     {
        //         Vector3 v1 = transform.TransformPoint(vertices[triangles[i]]);
        //         Vector3 v2 = transform.TransformPoint(vertices[triangles[i + 1]]);
        //         Vector3 v3 = transform.TransformPoint(vertices[triangles[i + 2]]);
        //         Vector3 v4 = transform.TransformPoint(vertices[triangles[i + 3]]);
        //
        //         // Add only the perimeter edges of the quad (ignore diagonals)
        //         AddEdge(edges, v1, v2);
        //         AddEdge(edges, v2, v3);
        //         AddEdge(edges, v3, v4);
        //         AddEdge(edges, v4, v1);
        //     }
        // }
        //
        // // Draw the unique edges
        // foreach (Edge edge in edges)
        // {
        //     DrawLine(edge.start, edge.end);
        // }
    }

    private void AddEdge(HashSet<Edge> edges, Vector3 v1, Vector3 v2)
    {
        // Create an edge with sorted vertices to avoid duplicates
        edges.Add(new Edge(v1, v2));
    }

    private void DrawLine(Vector3 start, Vector3 end)
    {
        Gizmos.DrawLine(start, end);
    }

    private struct Edge
    {
        public Vector3 start;
        public Vector3 end;

        public Edge(Vector3 v1, Vector3 v2)
        {
            // Always store the edge in a consistent order
            start = v1;
            end = v2;

            if (Vector3.Distance(v1, v2) < 0.001f)
            {
                // If the two points are nearly the same, we should not consider this edge
                start = end = Vector3.zero;
            }
            else if (v1.GetHashCode() > v2.GetHashCode())
            {
                (start, end) = (end, start);
            }
        }

        public override int GetHashCode()
        {
            return start.GetHashCode() ^ end.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Edge)) return false;
            Edge edge = (Edge)obj;
            return (start.Equals(edge.start) && end.Equals(edge.end)) || (start.Equals(edge.end) && end.Equals(edge.start));
        }
    }
        
        
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(Length)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(BaseSize)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(FOVAngle)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(ElevationAngle)));
            }
            if (hasGeneral)
            {
                BaseField(_so);
            }
        }
#endif
    }
}