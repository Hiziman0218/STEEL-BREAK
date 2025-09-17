using UnityEditor;                         // Unity エディタ拡張API（メニュー・シーンビュー・Undo 等）
using UnityEngine;                         // Unity の基本API（GameObject, Transform, Camera, Vector3 など）

namespace EmeraldAI.Utility                // EmeraldAI のユーティリティ名前空間
{
    /// <summary>
    /// （日本語）カバーノード（Cover Node）を、メニューからの作成およびヒエラルキー上での右クリック作成によって生成できるようにするエディタ拡張です。
    /// </summary>
    // 【クラス概要】EmeraldCoverMenu：
    //  エディタの「GameObject > Emerald AI > Create Cover Node」メニューを追加し、
    //  実行時に「Emerald Cover Node」という名前の GameObject を生成して CoverNode コンポーネントを付与します。
    //  メニュー実行時のコンテキスト（Hierarchy で選択中のオブジェクト）があれば、それを親にして原点へ配置します。
    //  コンテキストがない場合は、シーンビューのカメラ前方（5m）に生成・配置します。
    public static class EmeraldCoverMenu      // static：インスタンス不要のユーティリティ型（メニューコマンドのみ提供）
    {
        [MenuItem("GameObject/Emerald AI/Create Cover Node", false, 1)]  // エディタメニューに項目を追加（パス / 検証はしない / 並び順）
        private static void CreateCustomObject(MenuCommand menuCommand)   // メニューから呼ばれる生成処理（EditorOnly）
        {
            GameObject coverNode = new GameObject("Emerald Cover Node");  // 新規 GameObject を生成（名前は英名のまま：既存仕様に準拠）
            coverNode.AddComponent<CoverNode>();                          // CoverNode コンポーネントを付与

            GameObject context = menuCommand.context as GameObject;       // メニュー実行時のコンテキスト（Hierarchy 選択オブジェクト）を取得
            if (context != null)                                          // 親にできる対象がある場合
            {
                coverNode.transform.SetParent(context.transform);          // 親子付け（階層に追加）
                coverNode.transform.localPosition = Vector3.zero;         // 親の原点に配置（ローカル座標 0,0,0）
            }
            else                                                          // 親にできる対象がない場合
            {
                Camera sceneCamera = SceneView.lastActiveSceneView.camera; // シーンビューの最後にアクティブだったカメラを取得
                Vector3 spawnPosition = sceneCamera.transform.position +   // カメラの位置から前方へ 5m の位置を算出
                                         sceneCamera.transform.forward * 5f;
                coverNode.transform.position = spawnPosition;              // ワールド座標で配置
            }

            Undo.RegisterCreatedObjectUndo(coverNode, "Create Cover Node"); // Undo に対応（元に戻すで作成を取り消せる）
            Selection.activeObject = coverNode;                             // 生成したノードを選択状態にする（すぐに編集できる）
        }
    }
}
