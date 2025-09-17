using System.Collections;                       // コルーチン等の基本コレクションAPI
using System.Collections.Generic;               // List<T> などの汎用コレクション
using UnityEngine;                              // UnityEngine の基礎API
using System.Linq;                              // Linq（OrderBy などの拡張メソッド）
using EmeraldAI;                                // EmeraldAI 名前空間の型を直接参照

namespace EmeraldAI                               // EmeraldAI 関連コンポーネント群の名前空間
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/footsteps-component")] // 公式ドキュメントへのリンク

    // 【クラス概要】EmeraldFootsteps：
    // アニメーションイベント（足が地面に接地したタイミング）で呼ばれ、
    // もっとも低い足の位置を基準にレイキャストを行い、地面の種類（タグ or Terrain の支配テクスチャ）に応じて
    // 「足音（Audio）」「足跡（Decal/Effect）」を再生・生成する補助コンポーネント。
    public class EmeraldFootsteps : MonoBehaviour // MonoBehaviour を継承した足音制御クラス
    {
        #region Variables                          // —— ランタイム/設定用の変数群 ——

        [Header("足元の検出対象レイヤー（Raycastのヒット先として許可するレイヤー）")]
        public LayerMask DetectableLayers;        // 検出対象とするレイヤーの集合

        [Header("Raycastで無視するレイヤー（自分自身・LBD等を自動追加）")]
        public LayerMask IgnoreLayers;            // 無視対象とするレイヤーの集合

        [Header("足（ボーン）Transformの一覧（左右など複数を登録。最下点を自動選択）")]
        public List<Transform> FeetTransforms = new List<Transform>(); // 足候補Transformのリスト

        [Header("サーフェス設定（タグ/テクスチャごとの足音・足跡・エフェクト定義）")]
        public List<FootstepSurfaceObject> FootstepSurfaces = new List<FootstepSurfaceObject>(); // 地面ごとの反応定義

        [Header("【Editor表示】設定セクション全体の折りたたみを隠すか")]
        public bool HideSettingsFoldout;          // インスペクタでのセクション表示管理

        [Header("【Editor表示】Footstepsセクションの折りたたみ状態")]
        public bool FootstepsFoldout;             // 足音設定セクションの開閉

        [Header("【Editor表示】Surfaceセクションの折りたたみ状態")]
        public bool SurfaceFoldout;               // サーフェス設定セクションの開閉

        [Header("EmeraldSystem参照（AI本体：アニメ/検出/サウンド等へアクセス）")]
        EmeraldSystem EmeraldComponent;           // 実行時に GetComponent で取得

        [Header("内部拡張されたIgnoreLayers（AI自身/LBDレイヤーを自動包含）")]
        LayerMask InternalIgnoreLayers;           // 自動補完後の無視レイヤー

        [Header("現在フレームで最も低い足（接地判定に用いる）")]
        Transform CurrentFoot;                    // 計算で選ばれた足

        [Header("直前フレームの最下足（同一連打の抑制や参照用）")]
        Transform LastFoot;                       // 直前の足

        [Header("さらに前のフレームの最下足（拡張用の保持フィールド）")]
        Transform LastFoot2;                      // その前の足（現状未使用）

        [Header("直近の足音処理時刻（多重発火を0.25秒抑制するためのタイムスタンプ）")]
        float TimeStamp;                          // デバウンス用の時刻
        #endregion

        void Start()                              // Unity ライフサイクル：開始時
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();        // 同一GameObject上の AI 本体を取得
            Invoke(nameof(SetupIgnoreLayers), 0.1f);                 // 0.1秒遅延して無視レイヤーを自動設定
            if (FeetTransforms.Count == 0)                           // 足Transform未登録のチェック
                Debug.LogError("The '" + gameObject.name + "' does not have any Feet Transforms. Please assign some in order to use the Footsteps Component."); // エラー表示（英語のまま）
        }

        /// <summary>
        /// （日本語）AI自身のレイヤーと、LBDコンポーネントのコライダーレイヤーを
        /// IgnoreLayers（無視レイヤー）に自動追加する初期化処理。
        /// </summary>
        void SetupIgnoreLayers()                 // 無視レイヤーの自動構成
        {
            InternalIgnoreLayers = IgnoreLayers;  // 現在の設定を内部コピー

            LayerMask LBDLayers = 0;              // LBD用の一時レイヤーマスク

            // AIがLBDComponentを使用している場合、そのコライダーレイヤーを追加
            if (EmeraldComponent.LBDComponent != null)
            {
                LBDLayers |= (1 << EmeraldComponent.LBDComponent.LBDComponentsLayer); // 対応ビットをOR
            }

            // AI自身のレイヤーも無視対象へ
            InternalIgnoreLayers |= (1 << EmeraldComponent.gameObject.layer); // 自身のlayerをOR

            // LBDLayersに立っている全ビットをInternalIgnoreLayersへ取り込み
            for (int i = 0; i < 32; i++)          // Unityのレイヤーは0〜31
            {
                if (LBDLayers == (LBDLayers | (1 << i))) // i番目ビットが含まれるなら
                {
                    InternalIgnoreLayers |= (1 << i);    // 取り込む
                }
            }

            // 自動追加を反映してIgnoreLayersを更新
            IgnoreLayers = InternalIgnoreLayers;  // 公開フィールド側へ反映
        }

        /// <summary>
        /// （日本語）足音を生成する汎用エントリ関数（歩き・走りなど共通で使用）。
        /// </summary>
        public void Footstep()                    // 共通トリガ
        {
            CreateFootstep();                     // 実処理へ委譲
        }

        /// <summary>
        /// （日本語）旧来の「歩き用」足音関数（後方互換のため残置）。
        /// 将来的に非推奨になる予定。Footstep() の使用を推奨。
        /// </summary>
        public void WalkFootstepSound()           // 互換：歩行足音
        {
            CreateFootstep();                     // 実処理共通
        }

        /// <summary>
        /// （日本語）旧来の「走り用」足音関数（後方互換のため残置）。
        /// 将来的に非推奨になる予定。Footstep() の使用を推奨。
        /// </summary>
        public void RunFootstepSound()            // 互換：走行足音
        {
            CreateFootstep();                     // 実処理共通
        }

        /// <summary>
        /// （日本語）アニメーションイベント時点で最も低い足（地面に近い足）を選び、
        /// その足元へレイキャストしてサーフェスに応じた効果音・エフェクトを生成する。
        /// </summary>
        void CreateFootstep()                     // 足音・足跡の生成本体
        {
            // 以下の条件のいずれかなら早期リターン：
            // ・待機中（IsIdling） ・直近処理から0.25秒未満 ・ステート遷移間の忙しい状態
            // ・足Transform未登録
            if (EmeraldComponent.AnimationComponent.IsIdling || Time.time < (TimeStamp + 0.25f) || EmeraldComponent.AnimationComponent.BusyBetweenStates || FeetTransforms.Count == 0) return;

            TimeStamp = Time.time;                // 最終実行時刻を記録（多重発火抑制）

            // 最も低い足を算出
            CalculateLowestFoot();                // CurrentFoot を更新

            if (CurrentFoot != null)              // 有効な足が取得できた場合のみ続行
            {
                // CurrentFoot 位置から下方向に短距離レイキャスト（IgnoreLayers を除外）
                RaycastHit hit;                   // レイキャストのヒット情報
                if (Physics.Raycast(CurrentFoot.position, Vector3.up * -0.25f, out hit, 1f, ~IgnoreLayers)) // 下向き（-Y）へ1m
                {
                    if (hit.collider != null)     // 何かに当たった場合
                    {
                        // まずはコライダーのタグ一致でサーフェス設定を検索
                        var StepData = FootstepSurfaces.Find(step => step.SurfaceType == FootstepSurfaceObject.SurfaceTypes.Tag && hit.collider.CompareTag(step.SurfaceTag));

                        // 見つからない場合はTerrainを確認し、該当地点の最優勢テクスチャからサーフェス設定を検索
                        if (StepData == null)
                        {
                            Terrain CurrentTerrain = hit.collider.GetComponent<Terrain>(); // 当たったのがTerrainか判定
                            if (CurrentTerrain != null)
                            {
                                Texture CurrentTexture = GetTerrainTexture(transform.position, CurrentTerrain); // 該当地点の支配的テクスチャを取得
                                StepData = FootstepSurfaces.Find(step => step.SurfaceType == FootstepSurfaceObject.SurfaceTypes.Texture && step.SurfaceTextures.Contains(CurrentTexture)); // テクスチャ一致で検索
                            }
                        }

                        // —— デバッグ：足接地点の可視化（ライン＋円）——
                        if (EmeraldComponent.DebuggerComponent != null && EmeraldComponent.DebuggerComponent.DrawFootstepPositions == YesOrNo.Yes)
                        {
                            Debug.DrawLine(CurrentFoot.position, hit.point, Color.yellow, 6);                 // 足→接地点のライン
                            DrawCircle(hit.point + Vector3.up * 0.05f, 0.25f, Color.yellow);                 // 接地点まわりの円
                        }

                        // サーフェス設定が見つかっていれば効果を再生
                        if (StepData != null)
                        {
                            // —— デバッグログ：どのサーフェスが使われたか —— 
                            if (EmeraldComponent.DebuggerComponent != null && EmeraldComponent.DebuggerComponent.DebugLogFootsteps == YesOrNo.Yes)
                            {
                                Debug.Log("The <b><color=green>" + gameObject.name + "</color></b> footstep collided with <b><color=green>" + hit.collider.name + "</color></b> and used the <b><color=green>" + StepData.name + "</color></b> Footstep Surface Object."); // 英文ログ（原文維持）
                            }

                            // —— 足元エフェクト（砂埃・水しぶき等）——
                            if (StepData.StepEffects.Count > 0)
                            {
                                GameObject StepEffect = StepData.StepEffects[Random.Range(0, StepData.StepEffects.Count)]; // ランダム選択
                                if (StepEffect != null)
                                    EmeraldAI.Utility.EmeraldObjectPool.SpawnEffect(                       // オブジェクトプールで生成
                                        StepEffect,
                                        new Vector3(CurrentFoot.position.x, hit.point.y + 0.01f, CurrentFoot.position.z), // 地面高に合わせて少し浮かせる
                                        Quaternion.FromToRotation(Vector3.up, hit.normal),                // 法線に合わせる
                                        StepData.StepEffectTimeout);                                      // 生存時間
                            }

                            // —— 足跡（デカール）——
                            if (StepData.Footprints.Count > 0)
                            {
                                GameObject Footprint = StepData.Footprints[Random.Range(0, StepData.Footprints.Count)]; // ランダム選択
                                if (Footprint != null)
                                    EmeraldAI.Utility.EmeraldObjectPool.SpawnEffect(
                                        Footprint,
                                        new Vector3(CurrentFoot.position.x, hit.point.y + 0.01f, CurrentFoot.position.z), // 地面高に合わせる
                                        Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation,         // 体の向き＋法線補正
                                        StepData.FootprintTimeout);                                                        // 生存時間
                            }

                            // —— 足音（AudioClip）——
                            if (StepData.StepSounds.Count > 0)
                            {
                                AudioClip StepSound = StepData.StepSounds[Random.Range(0, StepData.StepSounds.Count)]; // ランダム選択
                                if (StepSound != null)
                                    EmeraldComponent.SoundComponent.PlayAudioClip(StepSound, StepData.StepVolume);     // 音量指定で再生
                            }
                        }
                    }
                }
            }
        }

        Texture GetTerrainTexture(Vector3 Position, Terrain terrain) // Terrain上の支配的テクスチャを取得
        {
            int surfaceIndex = 0;                                    // インデックス初期化
            surfaceIndex = GetDominateTexture(Position, terrain, terrain.terrainData); // 支配的テクスチャのインデックスを取得
            return terrain.terrainData.terrainLayers[surfaceIndex].diffuseTexture;     // 対応するディフューズテクスチャを返す
        }

        float[] GetTextureBlend(Vector3 Pos, Terrain terrain, TerrainData terrainData) // 指定地点のテクスチャブレンド配列を取得
        {
            int posX = (int)(((Pos.x - terrain.transform.position.x) / terrainData.size.x) * terrainData.alphamapWidth);  // アルファマップX座標
            int posZ = (int)(((Pos.z - terrain.transform.position.z) / terrainData.size.z) * terrainData.alphamapHeight); // アルファマップZ座標
            float[,,] SplatmapData = terrainData.GetAlphamaps(posX, posZ, 1, 1);                                           // 1x1 のスプラットマップ
            float[] blend = new float[SplatmapData.GetUpperBound(2) + 1];                                                   // レイヤー数に応じた配列

            for (int i = 0; i < blend.Length; i++)                 // 各レイヤーの寄与度を取り出す
            {
                blend[i] = SplatmapData[0, 0, i];                  // [x=0, y=0, layer=i] の値を格納
            }

            return blend;                                          // ブレンド結果を返す
        }

        int GetDominateTexture(Vector3 Pos, Terrain terrain, TerrainData terrainData) // 寄与度最大のテクスチャのインデックス
        {
            float[] textureMix = GetTextureBlend(Pos, terrain, terrainData); // ブレンド配列を取得
            int greatestIndex = 0;                          // 最大インデックス初期化
            float maxTextureMix = 0;                        // 最大寄与度初期化

            for (int i = 0; i < textureMix.Length; i++)     // 全レイヤーを走査
            {
                if (textureMix[i] > maxTextureMix)          // より大きい寄与度なら更新
                {
                    greatestIndex = i;                      // インデックス更新
                    maxTextureMix = textureMix[i];          // 最大値更新
                }
            }

            return greatestIndex;                           // 最も支配的なテクスチャのインデックス
        }

        /// <summary>
        /// （日本語）FeetTransformsをY座標の低い順に並べ、最下点の足をCurrentFootに設定。
        /// 直前と同じ足だった場合は、リスト末尾の足を代替採用して連続発火を回避。
        /// </summary>
        void CalculateLowestFoot()                          // 最下点の足を決定
        {
            LastFoot = CurrentFoot;                         // 直前の足を保持
            CurrentFoot = FeetTransforms.OrderBy(p => p.position.y).First(); // Y最小の足を選択
            if (LastFoot == CurrentFoot) CurrentFoot = FeetTransforms[FeetTransforms.Count - 1]; // 同一なら末尾を採用
        }

        /// <summary>
        /// （日本語）各足音の衝突位置に円を描くデバッグ描画。
        /// </summary>
        void DrawCircle(Vector3 center, float radius, Color color) // Debug.DrawLine を用いた円近似
        {
            Vector3 prevPos = center + new Vector3(radius, 0, 0);  // 初期点：右方向に半径ぶん
            for (int i = 0; i < 30; i++)                           // 30分割で円を近似
            {
                float angle = (float)(i + 1) / 30.0f * Mathf.PI * 2.0f;  // 次角度（ラジアン）
                Vector3 newPos = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius); // 新点
                Debug.DrawLine(prevPos, newPos, color, 6);          // 線で結ぶ（6秒表示）
                prevPos = newPos;                                   // 前回点更新
            }
        }
    }
}
