using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SwordEfecctSystem : MonoBehaviour
{
	public MeshCollider m_MeshCollider;

	//　剣元
	[SerializeField]
	private Transform startPosition;
	//　剣先
	[SerializeField]
	private Transform endPosition;
	//　メッシュ
	private Mesh mesh;
	//　軌跡用の四角形の表示個数
	[SerializeField]
	private int saveMeshNum = 10;
	//　頂点リスト
	[SerializeField]
	private List<Vector3> verticesLists = new List<Vector3>();
	//　UVリスト
	[SerializeField]
	private List<Vector2> uvsLists = new List<Vector2>();
	//　剣元の位置リスト
	[SerializeField]
	private List<Vector3> startPoints = new List<Vector3>();
	//　剣先の位置リスト
	[SerializeField]
	private List<Vector3> endPoints = new List<Vector3>();
	//　三角形のリスト
	[SerializeField]
	private List<int> tempTriangles = new List<int>();

	// Use this for initialization
	void Start()
	{
		mesh = GetComponent<MeshFilter>().mesh;
	}
	void LateUpdate()
	{
		//　必要頂点数を超えたら削除
		if (startPoints.Count >= saveMeshNum + 1)
		{
			startPoints.RemoveAt(0);
			endPoints.RemoveAt(0);
		}
		//　現在の剣元、剣先を登録
		startPoints.Add(startPosition.position);
		endPoints.Add(endPosition.position);

		//　頂点がメッシュ数＋１以上になったら剣の軌跡メッシュを作成
		if (startPoints.Count >= saveMeshNum + 1)
		{
			CreateMesh();
			m_MeshCollider.sharedMesh = GetComponent<MeshFilter>().mesh;
		}
	}
	//　剣の軌跡作成メソッド
	void CreateMesh()
	{
		//　メッシュのクリア
		mesh.Clear();

		//　リストのクリア
		verticesLists.Clear();
		uvsLists.Clear();
		tempTriangles.Clear();

		for (int i = 0; i < saveMeshNum; i++)
		{
			verticesLists.AddRange(new Vector3[] {
				startPoints[i], endPoints[i], startPoints[i + 1],
				startPoints[i + 1], endPoints[i], endPoints[i + 1]
			});
			//Debug.DrawLine(startPoints[i], endPoints[i], Color.red);
			//Debug.DrawLine(startPoints[i + 1], endPoints[i + 1], Color.yellow);
		}

		// UVMapのパラメータ設定
		float addParam = 0f;
		for (int i = 0; i < saveMeshNum; i++)
		{
			// 四角形のテクスチャの割り当てを設定
			uvsLists.AddRange(new Vector2[]{
				new Vector2(addParam, 0f), new Vector2(addParam, 1f), new Vector2(addParam + 1f / saveMeshNum, 0f),
				new Vector2(addParam + 1f / saveMeshNum, 0f), new Vector2(addParam, 1f), new Vector2(addParam + 1f / saveMeshNum, 1f)
			});
			//　表示する四角形数で1を割って割合を計算
			addParam += 1f / saveMeshNum;

			//Debug.Log(addParam);
		}

		//　メッシュ用の三角形を登録した頂点で設定
		for (int i = 0; i < verticesLists.Count; i++)
		{
			tempTriangles.Add(i);
		}

		mesh.vertices = verticesLists.ToArray();
		mesh.uv = uvsLists.ToArray();
		mesh.triangles = tempTriangles.ToArray();

		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
	}

}
