// ===============================================================
// ファイル名 : AbilityData.cs
// 目的     : Emerald AI 2025 のアビリティ関連パラメータをまとめたデータコンテナ
// 注意     : ご主人様の指示により実行ロジックは一切変更せず、コメントとEditor用属性のみ追加
// ポリシー : ・各メンバー変数に [Header("…")] を追加（HideInInspector でも付与）
//            ・すべての Tooltip を日本語化
//            ・クラス宣言の直前に日本語の用途注釈
//            ・可能な範囲で各行に日本語コメント（過度な冗長化は避けつつ可読性重視）
//            ・Debug.Log/Log 系の行には理由と影響を注釈
// ===============================================================

using UnityEngine;                           // Unityエンジンの基本API
using System.Collections;                    // コルーチン等
using System.Collections.Generic;            // Listなどのコレクション
using EmeraldAI.Utility;                     // Emerald AI ユーティリティ（オブジェクトプール等）

namespace EmeraldAI                           // EmeraldAI 名前空間
{
    /// <summary>
    /// 【AbilityData】アビリティ設定の親データクラス（モジュール別の入れ子クラスを保持）
    /// </summary>
    [System.Serializable]
    public class AbilityData
    {
        /// <summary>
        /// 【CreateSettingsData】アビリティ生成（作成）時の演出設定（エフェクト/サウンド/寿命）
        /// </summary>
        [System.Serializable]
        public class CreateSettingsData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [Tooltip("このモジュールを有効にするかどうかを制御します。")]
            [HideInInspector] public bool Enabled;                             // 有効フラグ（非表示）

            [Header("生成時エフェクト（作成時に再生）")]
            [Tooltip("アビリティが生成された際に再生するエフェクトを指定します。")]
            public GameObject CreateEffect;                                     // 生成時のエフェクト

            [Header("生成エフェクトの寿命（秒）")]
            [Range(0.5f, 8f)]
            [Tooltip("生成エフェクトが消えるまでの時間（秒）を制御します。")]
            public float CreateEffectTimeout = 2;                               // エフェクト寿命

            [Header("生成時サウンド候補リスト")]
            [Tooltip("生成エフェクト再生時に鳴らすサウンドの候補リストです。")]
            public List<AudioClip> CreateSoundsList = new List<AudioClip>();    // サウンド一覧

            /// <summary>
            /// 指定位置に生成エフェクトをスポーンします。
            /// </summary>
            /// <param name="Owner">このアビリティの所有者。</param>
            /// <param name="SpawnPosition">生成エフェクトのスポーン位置。</param>
            public void SpawnCreateEffect(GameObject Owner, Transform SpawnPosition)
            {
                if (Enabled)                                                    // モジュールが有効な場合のみ
                {
                    if (CreateEffect != null)                                   // エフェクトが設定されている
                    {
                        // オブジェクトプールから生成し、寿命付きで再生
                        GameObject SpawnedCreateEffect = EmeraldObjectPool.SpawnEffect(CreateEffect, SpawnPosition.position, Owner.transform.rotation, CreateEffectTimeout);
                        SpawnedCreateEffect.name = CreateEffect.name;           // 名前同期（管理しやすくするため）
                        SpawnedCreateEffect.transform.localScale = CreateEffect.transform.localScale; // スケール同期
                    }

                    if (CreateSoundsList.Count > 0)                             // サウンド候補がある場合
                    {
                        AudioClip Clip = CreateSoundsList[Random.Range(0, CreateSoundsList.Count)];
                        if (Clip)                                               // クリップが有効
                        {
                            // サウンド専用プールオブジェクトによりワンショット再生
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition.position, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);         // 音量に軽い揺らぎ
                            TempSound.PlayOneShot(Clip);                        // 再生
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 【ChargeSettingsData】詠唱/チャージ中の演出設定（エフェクト/サウンド/長さ）
        /// </summary>
        [System.Serializable]
        public class ChargeSettingsData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [Tooltip("このモジュールを有効にするかどうかを制御します。")]
            [HideInInspector] public bool Enabled;                               // 有効フラグ（非表示）

            [Header("チャージ（詠唱）エフェクト（粒子推奨・単一オブジェクト）")]
            [Tooltip("アビリティのチャージ/詠唱中に再生するエフェクトを制御します。\n\n注意: パーティクルシステムを持つ単一オブジェクトである必要があります。")]
            public GameObject ChargeEffect;                                      // チャージ用エフェクト

            [Header("チャージ継続時間（秒）")]
            [Range(0.5f, 8f)]
            [Tooltip("チャージエフェクトの持続時間です。ChargeLength をパーティクルの duration に合わせてください。エフェクトの Loop は false を推奨。")]
            public float ChargeLength = 2;                                       // チャージ時間

            [Header("チャージ時サウンド候補リスト")]
            [Tooltip("チャージエフェクト生成時に再生されるサウンドを制御します。")]
            public List<AudioClip> ChargeSoundsList = new List<AudioClip>();     // サウンド候補

            /// <summary>
            /// 指定位置にチャージエフェクトをスポーンします。
            /// </summary>
            /// <param name="Owner">アビリティの所有者。</param>
            /// <param name="SpawnPosition">チャージエフェクトのスポーン位置。</param>
            public void SpawnChargeEffect(GameObject Owner, Transform SpawnPosition)
            {
                if (Enabled)                                                     // 有効時のみ
                {
                    if (ChargeEffect != null)                                    // エフェクト設定あり
                    {
                        // 寿命に+1fの余裕（フェード等の猶予）
                        GameObject SpawnedChargeEffect = EmeraldObjectPool.SpawnEffect(ChargeEffect, SpawnPosition.position, Owner.transform.rotation, ChargeLength + 1f);
                        SpawnedChargeEffect.name = ChargeEffect.name;            // 名称同期
                        SpawnedChargeEffect.transform.localScale = ChargeEffect.transform.localScale; // スケール同期
                        SpawnedChargeEffect.transform.SetParent(SpawnPosition);  // 詠唱点に追従

                        ParticleSystem ParticleSystemRef = SpawnedChargeEffect.GetComponent<ParticleSystem>(); // 粒子取得
                        if (ParticleSystemRef)
                        {
                            ParticleSystemRef.Stop();                            // いったん停止してから
                            var main = ParticleSystemRef.main;                   // メインモジュール取得
                            main.duration = ChargeLength;                        // チャージ時間に同期
                            ParticleSystemRef.Play();                            // 再生
                        }
                        else
                        {
                            // ▼ログ注釈：指定エフェクトに ParticleSystem が無い警告。
                            //   実行続行は可能だが、チャージ演出が機能せず視覚フィードバックが欠落する。
                            Debug.LogError("The " + ChargeEffect.name + " does not have a Particle System on it so it will not be used. A Particle System is required to be used as an ability's Charge Effect.");
                        }
                    }

                    if (ChargeSoundsList.Count > 0)                              // サウンド候補あり
                    {
                        AudioClip Clip = ChargeSoundsList[Random.Range(0, ChargeSoundsList.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition.position, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);         // 音量揺らぎ
                            TempSound.PlayOneShot(Clip);                        // 再生
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 【MeleeData】近接攻撃アビリティの演出・当たり範囲設定
        /// </summary>
        [System.Serializable]
        public class MeleeData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled;                               // 有効フラグ（非表示）

            [Header("命中時エフェクト（近接が当たった瞬間）")]
            [Tooltip("アビリティが対象に命中した際のエフェクトを制御します。")]
            public GameObject ImpactEffect;                                      // 命中エフェクト

            [Header("命中エフェクトの寿命（秒）")]
            [Range(0.5f, 10)]
            [Tooltip("命中エフェクトがスポーン後に無効化されるまでの時間（秒）を制御します。")]
            public float ImpactEffectTimeoutSeconds = 2;                         // 寿命

            [Header("命中時サウンド候補リスト")]
            [Tooltip("対象に命中した時に再生されるサウンドの候補一覧です。")]
            public List<AudioClip> ImpactSoundsList = new List<AudioClip>();     // サウンド候補

            [Space(15)] // 視覚的区切り

            [Header("ダメージ有効距離（最大）")]
            [Range(1, 30)]
            [Tooltip("このアビリティでダメージを与えられる最大距離を制御します。\n\n注意: 近接攻撃アニメが武器衝突イベントを使用している場合、この設定は無視され、武器のコリジョン成否に依存します。")]
            public float MaxDamageDistance = 4;                                   // 有効距離

            [Header("攻撃可能高さ（最大）")]
            [Range(1, 20)]
            [Tooltip("AIが攻撃可能な最大高さを制御します。\n\n注意: この高さを超えると、対象が範囲内に入るまで攻撃を待機します。")]
            public float MaxAttackHeight = 5;                                     // 高さ

            [Header("ダメージ有効角度（最大）")]
            [Range(5, 360)]
            [Tooltip("このアビリティでダメージが入る最大角度を制御します。\n\n注意: 武器衝突イベントを使用する場合、この設定は無視されます。")]
            public float MaxDamageAngle = 90;                                     // 角度
        }

        /// <summary>
        /// 【ProjectileData】一般的な投射体の演出（生成/発射/着弾エフェクト・サウンド）
        /// </summary>
        [System.Serializable]
        public class ProjectileData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = true;                        // 有効（非表示）

            [Header("投射生成時エフェクト（スポーン時）")]
            public GameObject SpawnProjectileEffect;                              // 生成エフェクト

            [Header("投射生成エフェクトの寿命（秒）")]
            [Range(1, 15)]
            public float SpawnProjectileTimeoutSeconds = 2;                       // 生成寿命

            [Header("投射生成サウンド候補リスト")]
            public List<AudioClip> SpawnProjectileSoundsList = new List<AudioClip>(); // 生成時サウンド

            [Space(15)]

            [Header("【必須】投射体本体のエフェクト")]
            [Tooltip("投射体オブジェクトとして使用されるエフェクト（必須）を指定します。")]
            public GameObject ProjectileEffect;                                   // 本体

            [Header("投射体の寿命（秒）")]
            [Range(1, 15)]
            [Tooltip("投射体エフェクトのタイムアウト（秒）。タイムアウト時、ImpactEffect が設定されていれば着弾エフェクトを再生します。")]
            public float ProjectileTimeoutSeconds = 6;                            // 本体寿命

            [Header("移動中ループサウンド（任意）")]
            [Tooltip("投射体移動中にループ再生されるサウンド。衝突または終了時に停止します。")]
            public AudioClip TravelSound;                                         // 走行音

            [Header("衝突時に無効化する子エフェクト名リスト")]
            [Tooltip("着弾後（Effects Disable Time 経過後）に無効化するパーティクル名のリスト。トレイル等の終了時間を確保しつつ、本体の停止を可能にします。\n\n注意: ProjectileEffect 側の子階層に対する名前参照です。")]
            public List<string> EffectsToDisable = new List<string>();            // 無効化対象

            [Space(15)]

            [Header("発射直前（Launch）エフェクト")]
            [Tooltip("投射体が目標へ向けて動き始めるときのエフェクト。\n\n補足: Launch Projectile Delay を使う際に特に有効です。")]
            public GameObject LaunchProjectileEffect;                             // ランチ演出

            [Header("ランチエフェクトの寿命（秒）")]
            [Range(1, 15)]
            [Tooltip("ランチエフェクトがスポーン後に無効化されるまでの時間（秒）。")]
            public float LaunchProjectileTimeoutSeconds = 2;                      // ランチ寿命

            [Header("発射の遅延（秒）")]
            [Range(0, 4)]
            [Tooltip("ProjectileEffect のスポーン後、実際に発射を開始するまでの遅延時間（秒）。")]
            public float LaunchProjectileDelay = 0f;                              // 遅延

            [Header("ランチ時サウンド候補リスト")]
            public List<AudioClip> LaunchProjectileSoundsList = new List<AudioClip>(); // 発射音

            [Space(15)]

            [Header("着弾（Impact）エフェクト")]
            [Tooltip("投射体が対象またはサーフェスに衝突した後のエフェクト。")]
            public GameObject ImpactEffect;                                       // 着弾エフェクト

            [Header("着弾エフェクトの寿命（秒）")]
            [Range(1, 15)]
            [Tooltip("着弾エフェクトがスポーン後に無効化されるまでの時間（秒）。")]
            public float ImpactTimeoutSeconds = 6;                                // 寿命

            [Header("着弾サウンド候補リスト")]
            public List<AudioClip> ImpactSoundsList = new List<AudioClip>();      // 着弾音

            /// <summary>
            /// 指定位置にランチ（発射開始）エフェクトをスポーンします。
            /// </summary>
            /// <param name="Owner">アビリティの所有者。</param>
            /// <param name="SpawnPosition">ランチエフェクトのスポーン位置。</param>
            public void SpawnLaunchProjectileEffect(GameObject Owner, Vector3 SpawnPosition)
            {
                if (Enabled)
                {
                    if (LaunchProjectileEffect != null)
                    {
                        GameObject SpawnedLaunchEffect = EmeraldObjectPool.SpawnEffect(LaunchProjectileEffect, SpawnPosition, LaunchProjectileEffect.transform.rotation, LaunchProjectileTimeoutSeconds);
                        SpawnedLaunchEffect.name = LaunchProjectileEffect.name;
                        SpawnedLaunchEffect.transform.localScale = LaunchProjectileEffect.transform.localScale;
                    }

                    if (LaunchProjectileSoundsList.Count > 0)
                    {
                        AudioClip Clip = LaunchProjectileSoundsList[Random.Range(0, LaunchProjectileSoundsList.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }
            }

            /// <summary>
            /// 投射体の生成時（作成時）にスポーンするエフェクト。
            /// </summary>
            /// <param name="Owner">アビリティの所有者。</param>
            /// <param name="SpawnPosition">スポーン位置。</param>
            public void SpawnEffect(GameObject Owner, Vector3 SpawnPosition)
            {
                if (Enabled)
                {
                    if (SpawnProjectileEffect != null)
                    {
                        GameObject SpawnedEffect = EmeraldObjectPool.SpawnEffect(SpawnProjectileEffect, SpawnPosition, SpawnProjectileEffect.transform.rotation, SpawnProjectileTimeoutSeconds);
                        SpawnedEffect.name = SpawnProjectileEffect.name;
                        SpawnedEffect.transform.localScale = SpawnProjectileEffect.transform.localScale;
                    }

                    if (SpawnProjectileSoundsList.Count > 0)
                    {
                        AudioClip Clip = SpawnProjectileSoundsList[Random.Range(0, SpawnProjectileSoundsList.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }
            }

            /// <summary>
            /// 着弾時にスポーンするエフェクト。
            /// </summary>
            /// <param name="Owner">アビリティの所有者。</param>
            /// <param name="SpawnPosition">スポーン位置。</param>
            public void SpawnImpactEffect(GameObject Owner, Vector3 SpawnPosition)
            {
                if (Enabled)
                {
                    if (ImpactEffect != null)
                    {
                        GameObject SpawnedEffect = EmeraldObjectPool.SpawnEffect(ImpactEffect, SpawnPosition, ImpactEffect.transform.rotation, ImpactTimeoutSeconds);
                        SpawnedEffect.name = ImpactEffect.name;
                        SpawnedEffect.transform.localScale = ImpactEffect.transform.localScale;
                    }

                    if (ImpactSoundsList.Count > 0)
                    {
                        AudioClip Clip = ImpactSoundsList[Random.Range(0, ImpactSoundsList.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 【GeneralProjectileData】投射一般性能（速度/角度/弾数/連射間隔/刺さり挙動）
        /// </summary>
        [System.Serializable]
        public class GeneralProjectileData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = true;

            [Header("投射速度")]
            [Range(1, 100)]
            [Tooltip("投射体の速度を制御します。")]
            public int ProjectileSpeed = 30;

            [Header("発射角度上限（度）")]
            [Range(10, 360)]
            [Tooltip("投射体の打ち上げ上限角度を制御します。")]
            public int ProjectileMaxLaunchAngle = 140;

            [Header("同時生成する投射数")]
            [Range(1, 40)]
            [Tooltip("アビリティ生成時に作成する投射体の総数を制御します。")]
            public int TotalProjectiles = 2;

            [Header("投射生成間隔（秒）")]
            [Range(0, 1)]
            [Tooltip("複数生成する場合の、各投射間の時間（秒）を制御します。")]
            public float TimeBetweenProjectiles = 0.2f;

            [Header("命中時に対象へ刺さる（Stick）挙動")]
            [Tooltip("この投射体が命中時に対象へ刺さるかどうかを制御します。\n\n注意: 刺さりが持続する時間は Collider モジュールの Collision Timeout に基づきます。AIの Location Based Damage と組み合わせると効果的です。")]
            public bool AttachToTarget = false;
        }

        /// <summary>
        /// 【GrenadeData】手榴弾系（投擲物）設定
        /// </summary>
        [System.Serializable]
        public class GrenadeData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = true;

            [Header("使用する手榴弾オブジェクト")]
            [Tooltip("手榴弾として使用するオブジェクトを指定します。")]
            public GameObject GrenadeObject;

            [Header("手榴弾に割り当てるレイヤー")]
            [Tooltip("手榴弾に割り当てるレイヤーを制御します。\n\n注意: AIが手榴弾を回避できるようにするには、AIの Projectile Layers にこのレイヤーが含まれている必要があります。デフォルトは Ignore Layermask。")]
            [Layer] public int GrenadeLayer = 2;

            [Header("投擲高さ")]
            [Range(1, 8)]
            [Tooltip("投擲時の高さ（アーチ量）を制御します。")]
            public float ThrowHeight = 5f;

            [Header("投擲サウンド")]
            [Tooltip("手榴弾を投げる際に再生するサウンドを指定します。")]
            public AudioClip ThrowSound;

            [Header("空中回転（投擲後の見栄え）")]
            [Tooltip("投擲後、着弾まで動的に回転させるかどうかを制御します。衝突時に停止します。")]
            public bool RotateOnThrow;

            [Header("衝突時サウンド候補（地面/壁など）")]
            [Tooltip("手榴弾が表面に衝突した際に再生するサウンド候補です。")]
            public List<AudioClip> ImpactSoundsList = new List<AudioClip>();

            [Space(15)]

            [Header("爆発の影響対象レイヤー（Rigidbodyがあれば力を加える）")]
            [Tooltip("爆発で影響を与えるレイヤー。Rigidbody が検出された場合、爆発力を付与します。\n\n注意: ダメージは投擲者から見た敵対関係に自動適用（味方にも与える場合は Can Damage Friendlies を有効化）。対象レイヤーは投擲者の Detection Component の Detection Layers に依存します。")]
            public LayerMask RigidbodyLayers = 0;

            [Header("爆発半径")]
            [Range(1, 15)]
            [Tooltip("爆発の半径。半径内の対象へ影響を与えます。")]
            public int ExplosionRadius = 4;

            [Header("爆発までの遅延（秒）")]
            [Range(0, 10)]
            [Tooltip("手榴弾が爆発するまでの時間（秒）。")]
            public float ExplosionTime = 2.5f;

            [Header("爆発エフェクト")]
            public GameObject ExplosionEffect;

            [Header("爆発エフェクトの寿命（秒）")]
            [Range(1, 15)]
            [Tooltip("爆発エフェクトがタイムアウトするまでの時間（秒）。")]
            public float ExplosionTimeoutSeconds = 3;

            [Header("味方や投擲者もダメージ対象に含める")]
            [Tooltip("爆発で味方および投擲者自身にダメージを与えるかどうか。")]
            public bool CanDamageFriendlies = false;

            [Header("爆発サウンド候補リスト")]
            public List<AudioClip> ExplosionSoundsList = new List<AudioClip>();

            /// <summary>
            /// 指定位置で爆発エフェクトとサウンドを再生します。
            /// </summary>
            /// <param name="Owner">アビリティの所有者。</param>
            /// <param name="SpawnPosition">爆発の発生位置。</param>
            public void ExplodeGrenade(GameObject Owner, Vector3 SpawnPosition)
            {
                if (Enabled)
                {
                    if (ExplosionEffect != null)
                    {
                        GameObject SpawnedEffect = EmeraldObjectPool.SpawnEffect(ExplosionEffect, SpawnPosition, ExplosionEffect.transform.rotation, ExplosionTimeoutSeconds);
                        SpawnedEffect.name = ExplosionEffect.name;
                        SpawnedEffect.transform.localScale = ExplosionEffect.transform.localScale;
                    }

                    if (ExplosionSoundsList.Count > 0)
                    {
                        AudioClip Clip = ExplosionSoundsList[Random.Range(0, ExplosionSoundsList.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 【BulletProjectileData】銃弾系の設定（命中効果/弾道/マズルフラッシュ/デフォルト着弾 等）
        /// </summary>
        [System.Serializable]
        public class BulletProjectileData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = true;

            [Header("無視するレイヤー（弾の衝突検出を回避）")]
            [Tooltip("弾が無視するレイヤー。特定のコライダー等との不要な衝突を回避します。")]
            public LayerMask IgnoreLayers;

            [Header("弾として使用するオブジェクト（コライダー不要）")]
            [Tooltip("弾として用いるGameObject。コライダーは不要（内部で衝突検出）。Trail Renderer は弾の軌跡として使用可。")]
            public GameObject BulletObject;

            [Header("弾オブジェクトの寿命（秒、未衝突時）")]
            [Range(1, 10)]
            [Tooltip("何にも衝突しなかった場合に弾オブジェクトが消えるまでの時間（秒）。")]
            public int BulletObjectTimeout = 4;

            [Header("衝突後の表示猶予（秒）")]
            [Range(0.05f, 0.75f)]
            [Tooltip("衝突後に弾オブジェクトを消すまでの時間（秒）。トレイル再生完了の猶予に。")]
            public float BulletCollisionTimeout = 0.15f;

            [Header("弾速")]
            [Range(20, 75)]
            [Tooltip("弾の速度を制御します。")]
            public int BulletSpeed = 50;

            [Header("衝突チェック周期（秒・小さいほど高精度）")]
            [Range(0, 0.006f)]
            [Tooltip("衝突判定を行う周期（秒）。小さいほど見逃しにくい。\n\n注意: 弾速が高いほど見逃し防止のためチェックを増やす必要があります。すり抜ける場合は値を少しずつ下げて調整してください。")]
            public float CollisionCheckSpeed = 0.003f;

            [Header("弾の拡散（X軸）")]
            [Range(0, 100)]
            [Tooltip("X軸方向の拡散量。値が高いほど命中が不正確になります。")]
            public int BulletSpreadX = 5;

            [Header("弾の拡散（Y軸）")]
            [Range(0, 100)]
            [Tooltip("Y軸方向の拡散量。値が高いほど命中が不正確になります。")]
            public int BulletSpreadY = 5;

            [Header("連射時の総弾数")]
            [Range(1, 40)]
            [Tooltip("アビリティ生成時に発射される弾丸の総数（オート/セミオート表現に使用）。")]
            public int TotalBullets = 1;

            [Header("弾生成間隔（秒）")]
            [Range(0, 1)]
            [Tooltip("複数弾を発射する際の各弾間の時間（秒）。")]
            public float TimeBetweenBullets = 0.2f;

            [Space(15)]

            [Header("マズルフラッシュ（発砲時エフェクト）")]
            [Tooltip("発砲時に再生するエフェクト。\n\n注意: マズル位置はAIの現在の Attack Transform を使用します。")]
            public GameObject MuzzleFlashEffect;

            [Header("マズルフラッシュ寿命（秒）")]
            [Range(1, 10)]
            [Tooltip("マズルフラッシュが無効化されるまでの時間（秒）。")]
            public float MuzzleFlashEffectTimeoutSeconds = 2;

            [Header("発砲音量（0〜1）")]
            [Range(0, 1)]
            [Tooltip("発砲サウンドの音量を制御します。")]
            public float FireSoundsVolume = 1;

            [Header("発砲サウンド候補リスト")]
            [Tooltip("発射ごとに再生されるサウンド候補。")]
            public List<AudioClip> FireSoundsList = new List<AudioClip>();

            [Space(15)]

            [Header("デフォルト着弾エフェクト（タグ個別設定が無い場合）")]
            [Tooltip("個別の着弾設定が無い・見つからない場合に使用されるデフォルトの着弾エフェクト。")]
            public GameObject DefaultImpactEffect;

            [Header("デフォルト着弾エフェクト寿命（秒）")]
            [Range(1, 20)]
            [Tooltip("デフォルト着弾エフェクトが無効化されるまでの時間（秒）。")]
            public float DefaultImpactEffectTimeoutSeconds = 8;

            [Header("デフォルト着弾サウンド音量（0〜1）")]
            [Range(0, 1)]
            [Tooltip("デフォルト着弾サウンドの音量。")]
            public float DefaultImpactSoundsVolume = 1;

            [Header("デフォルト着弾サウンド候補")]
            [Tooltip("個別設定がない場合に使用されるデフォルトの着弾サウンド。")]
            public List<AudioClip> DefaultImpactSounds = new List<AudioClip>();

            /// <summary>
            /// 【BulletImpactClass】タグ別の着弾効果設定
            /// </summary>
            [System.Serializable]
            public class BulletImpactClass
            {
                [Header("表面識別用タグ")]
                [Tooltip("この着弾設定を適用するためのタグ。")]
                [Tag] public string SurfaceTag = "Untagged";

                [Header("該当タグへの着弾エフェクト")]
                [Tooltip("上記タグに着弾した際に再生されるエフェクト。")]
                public GameObject ImpactEffect;

                [Header("着弾エフェクト寿命（秒）")]
                [Range(1, 20)]
                [Tooltip("着弾エフェクトが無効化されるまでの時間（秒）。")]
                public float ImpactEffectTimeoutSeconds = 8;

                [Header("着弾サウンド音量（0〜1）")]
                [Range(0, 1)]
                [Tooltip("着弾サウンドの音量。")]
                public float ImpactSoundsVolume = 1;

                [Header("着弾サウンド候補")]
                [Tooltip("上記タグに着弾した場合のサウンド候補。")]
                public List<AudioClip> ImpactSounds = new List<AudioClip>();

                // public List<GameObject> ImpactDecals = new List<GameObject>(); // ※将来的なデカール対応のためのコメントアウト
            }

            [Space(15)]
            [SerializeField]
            [Header("タグ別 着弾データリスト")]
            [Tooltip("弾が衝突したタグに応じて、異なる着弾エフェクト/サウンドを再生できます。\n\n注意: マッチするデータがない場合はデフォルトの着弾エフェクト/サウンドが使用されます。")]
            public List<BulletImpactClass> BulletImpactData = new List<BulletImpactClass>();

            /// <summary>
            /// マズルフラッシュと発砲音を再生します。
            /// </summary>
            /// <param name="Owner">所有者。</param>
            /// <param name="SpawnPosition">発砲音の座標。</param>
            /// <param name="AttackTransform">マズル位置/回転。</param>
            public void SpawnBulletEffect(GameObject Owner, Vector3 SpawnPosition, Transform AttackTransform)
            {
                if (Enabled)
                {
                    if (MuzzleFlashEffect != null)
                    {
                        GameObject SpawnedLaunchEffect = EmeraldObjectPool.SpawnEffect(MuzzleFlashEffect, AttackTransform.position, AttackTransform.rotation, MuzzleFlashEffectTimeoutSeconds);
                        SpawnedLaunchEffect.name = MuzzleFlashEffect.name;
                        SpawnedLaunchEffect.transform.localScale = MuzzleFlashEffect.transform.localScale;
                    }

                    if (FireSoundsList.Count > 0)
                    {
                        AudioClip Clip = FireSoundsList[Random.Range(0, FireSoundsList.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f) * FireSoundsVolume;
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }
            }

            /// <summary>
            /// タグ別設定に基づく着弾エフェクト/サウンドの再生。
            /// </summary>
            /// <param name="Target">ヒット対象。</param>
            /// <param name="ImpactData">タグ別の着弾データ。</param>
            /// <param name="SpawnPosition">発生位置。</param>
            /// <param name="HitNormal">命中面法線（向き合わせに使用）。</param>
            public void SpawnBulletImpact(GameObject Target, BulletImpactClass ImpactData, Vector3 SpawnPosition, Vector3 HitNormal)
            {
                if (Enabled)
                {
                    if (ImpactData.ImpactEffect != null)
                    {
                        GameObject SpawnedEffect = EmeraldObjectPool.SpawnEffect(ImpactData.ImpactEffect, SpawnPosition, ImpactData.ImpactEffect.transform.rotation, ImpactData.ImpactEffectTimeoutSeconds);
                        SpawnedEffect.name = ImpactData.ImpactEffect.name;
                        SpawnedEffect.transform.localScale = ImpactData.ImpactEffect.transform.localScale;
                        // 衝突面に向けてエフェクトのForwardを合わせる
                        SpawnedEffect.transform.rotation = Quaternion.FromToRotation(SpawnedEffect.transform.forward, HitNormal) * SpawnedEffect.transform.rotation;
                        SpawnedEffect.transform.SetParent(Target.transform); // 対象に追従
                    }

                    if (ImpactData.ImpactSounds.Count > 0)
                    {
                        AudioClip Clip = ImpactData.ImpactSounds[Random.Range(0, ImpactData.ImpactSounds.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f) * ImpactData.ImpactSoundsVolume;
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }

                    /*
                    // ★デカールの将来対応（現在はコメントアウトのまま）
                    if (ImpactData.ImpactDecals.Count > 0)
                    {
                        GameObject RandomDecal = ImpactData.ImpactDecals[Random.Range(0, ImpactData.ImpactDecals.Count)];
                        if (RandomDecal)
                        {
                            GameObject SpawnedDecal = EmeraldObjectPool.SpawnEffect(RandomDecal, SpawnPosition, Quaternion.identity, 20);
                            SpawnedDecal.transform.position = SpawnPosition;
                            SpawnedDecal.transform.rotation = Quaternion.FromToRotation(SpawnedDecal.transform.up, HitNormal) * SpawnedDecal.transform.rotation;
                        }
                    }
                    */
                }
            }

            /// <summary>
            /// デフォルトの着弾エフェクト/サウンドの再生。
            /// </summary>
            /// <param name="Target">ヒット対象。</param>
            /// <param name="SpawnPosition">発生位置。</param>
            /// <param name="HitNormal">命中面法線。</param>
            public void SpawnDefaultBulletImpact(GameObject Target, Vector3 SpawnPosition, Vector3 HitNormal)
            {
                if (Enabled)
                {
                    if (DefaultImpactEffect != null)
                    {
                        GameObject SpawnedEffect = EmeraldObjectPool.SpawnEffect(DefaultImpactEffect, SpawnPosition, DefaultImpactEffect.transform.rotation, DefaultImpactEffectTimeoutSeconds);
                        SpawnedEffect.name = DefaultImpactEffect.name;
                        SpawnedEffect.transform.localScale = DefaultImpactEffect.transform.localScale;
                        SpawnedEffect.transform.rotation = Quaternion.FromToRotation(SpawnedEffect.transform.forward, HitNormal) * SpawnedEffect.transform.rotation;
                        SpawnedEffect.transform.SetParent(Target.transform);
                    }

                    if (DefaultImpactSounds.Count > 0)
                    {
                        AudioClip Clip = DefaultImpactSounds[Random.Range(0, DefaultImpactSounds.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f) * DefaultImpactSoundsVolume;
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 【ArrowProjectileData】矢などの刺突系投射（速度/刺さり）
        /// </summary>
        [System.Serializable]
        public class ArrowProjectileData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = true;

            [Header("投射速度")]
            [Range(1, 100)]
            [Tooltip("投射体の速度を制御します。")]
            public int ProjectileSpeed = 30;

            [Header("命中時の刺さり挙動")]
            [Tooltip("投射体が命中対象に刺さるかどうか。\n\n注意: 刺さり時間は Collider モジュールの Collision Timeout に依存。Location Based Damage 併用を推奨。")]
            public bool AttachToTarget = true;
        }

        /// <summary>
        /// 【AerialProjectileData】上空から落下/降下するタイプ（上方発生/半径/追尾等）
        /// </summary>
        [System.Serializable]
        public class AerialProjectileData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = true;

            [Header("発生源位置エフェクト（上空）")]
            [Tooltip("Spawn Source 位置で再生されるエフェクト。")]
            public GameObject AerialEffect;

            [Header("発生源エフェクトの寿命（秒）")]
            [Range(0.5f, 15)]
            public float AerialEffectTimeoutSeconds = 2;

            public enum SpawnSources { AboveSelf, AboveTarget }

            [Header("発生源（自分の上/対象の上）")]
            [Tooltip("上空系アビリティのスポーン起点を制御します。")]
            public SpawnSources SpawnSource = SpawnSources.AboveSelf;

            public enum AimSources { TargetPosition, GroundPosition }

            [Header("狙いの基準（対象位置/地面位置）")]
            [Tooltip("投射体の照準基準。\n\n注意: Homing Seconds が 0 より大きく有効な場合、GroundPosition は無視されます。")]
            public AimSources AimSource = AimSources.TargetPosition;

            [Space(15)]

            [Header("発生位置の高さオフセット")]
            [Range(0f, 30f)]
            [Tooltip("Spawn Source からさらに加算する高さ。")]
            public float HeightOffset = 12;

            [Header("初期のばらつき角（度）")]
            [Range(0f, 45f)]
            [Tooltip("投射体が照準に回頭する際の最大ランダム角。")]
            public float LaunchAngle = 0;

            [Header("スポーン半径（円形散布）")]
            [Range(0f, 30f)]
            [Tooltip("Spawn Source からの最大半径（円内ランダム）。")]
            public float Radius = 4;

            [Space(15)]

            [Header("投射速度")]
            [Range(1, 100)]
            [Tooltip("投射体の速度。")]
            public int ProjectileSpeed = 30;

            [Header("同時生成数")]
            [Range(1, 40)]
            [Tooltip("アビリティ生成時に作成する投射体の数。")]
            public int TotalProjectiles = 2;

            [Header("生成間隔（秒）")]
            [Range(0, 1)]
            [Tooltip("複数生成時の投射間隔（秒）。")]
            public float TimeBetweenProjectiles = 0.2f;

            [Header("命中時に刺さる（Stick）")]
            public bool AttachToTarget = false;

            /// <summary>
            /// 上空発生エフェクトを生成します（自分の上/対象の上）。
            /// </summary>
            public void SpawnAerialEffect(GameObject Owner, Transform Target)
            {
                if (Enabled && AerialEffect != null)
                {
                    if (SpawnSource == SpawnSources.AboveTarget)
                    {
                        EmeraldObjectPool.SpawnEffect(AerialEffect, new Vector3(Target.position.x, Target.position.y + HeightOffset, Target.position.z), AerialEffect.transform.rotation, AerialEffectTimeoutSeconds);
                    }
                    else if (SpawnSource == SpawnSources.AboveSelf)
                    {
                        EmeraldObjectPool.SpawnEffect(AerialEffect, new Vector3(Owner.transform.position.x, Owner.transform.position.y + HeightOffset, Owner.transform.position.z), AerialEffect.transform.rotation, AerialEffectTimeoutSeconds);
                    }
                }
            }
        }

        /// <summary>
        /// 【BarrageProjectileData】連続射出・弾幕系（速度/総数/間隔）
        /// </summary>
        [System.Serializable]
        public class BarrageProjectileData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = true;

            [Header("投射速度")]
            [Range(1, 100)]
            [Tooltip("投射体の速度。")]
            public int ProjectileSpeed = 30;

            [Header("総投射数")]
            [Range(1, 40)]
            [Tooltip("アビリティ生成時に作成する投射体の数。")]
            public int TotalProjectiles = 2;

            [Header("生成間隔（秒）")]
            [Range(0, 1)]
            [Tooltip("複数生成時の投射間隔（秒）。")]
            public float TimeBetweenProjectiles = 0.2f;
        }

        /// <summary>
        /// 【GroundProjectileData】地表追従型（地面アライン/距離/角度/速度/散布）
        /// </summary>
        [System.Serializable]
        public class GroundProjectileData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = true;

            [Header("アライン対象レイヤー（地面/環境のみ推奨）")]
            [Tooltip("移動中に地形へ追従させる際の対象レイヤー。\n\n注意: プレイヤーやAI等のターゲットレイヤーは除外してください。")]
            public LayerMask AllignmentLayers;

            [Header("アライン速度")]
            [Range(1, 50)]
            [Tooltip("移動中の地形追従速度。")]
            public float AlignmentSpeed = 10;

            [Header("追従する最大傾斜角")]
            [Range(1, 90)]
            [Tooltip("移動中に追従可能な最大角度。")]
            public float MaxAlignmentAngle = 45;

            [Header("移動停止しうる傾斜角（Kill角）")]
            [Range(5, 90)]
            [Tooltip("移動中にこの角度に達した場合、アビリティを停止させる角度。")]
            public float KillAngle = 90;

            [Header("高さオフセット")]
            [Range(-5, 5)]
            [Tooltip("移動中の高さオフセット。高/低すぎる場合の微調整に使用します。")]
            public float HeightOffset = 0f;

            [Space(15)]

            [Header("最大移動距離")]
            [Range(1, 50)]
            [Tooltip("地表エフェクトが停止するまでの最大移動距離。")]
            public float MaxTravelDistance = 10;

            [Header("散布角（複数発射時の扇形角度）")]
            [Range(0, 360)]
            [Tooltip("Total Projectiles が複数の場合に均等に散らす角度。")]
            public float AngleSpread = 45;

            [Space(15)]

            [Header("投射速度")]
            [Range(1, 100)]
            [Tooltip("投射体の速度。")]
            public int ProjectileSpeed = 30;

            [Header("同時生成数")]
            [Range(1, 40)]
            [Tooltip("アビリティ生成時に作成する投射体の数。")]
            public int TotalProjectiles = 2;

            [Header("生成間隔（秒）")]
            [Range(0, 1)]
            [Tooltip("複数生成時の投射間隔（秒）。")]
            public float TimeBetweenProjectiles = 0.2f;
        }

        /// <summary>
        /// 【HomingData】追尾設定（追尾時間/最小追尾距離/回頭速度）
        /// </summary>
        [System.Serializable]
        public class HomingData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled;

            [Header("追尾時間（秒）")]
            [Range(0, 15)]
            [Tooltip("投射体が対象を追尾する時間（秒）。")]
            public float HomingSeconds = 0;

            [Header("最小追尾距離")]
            [Range(0, 10)]
            [Tooltip("投射体が対象を追尾する最小距離。\n\n注意: 0 を指定すると無効化できます。")]
            public float MinimumHomingDistance = 0;

            [Header("回頭速度")]
            [Range(1, 10)]
            [Tooltip("投射体が対象へ回頭する速度。")]
            public float HomingSpeed = 4;
        }

        /// <summary>
        /// 【AreaOfEffectData】範囲効果（半径/遅延/ヒット時演出/効果音）
        /// </summary>
        [System.Serializable]
        public class AreaOfEffectData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = true;

            // Coming with update
            // public enum LocationTypes { Self, RandomWithinRadiusOfSelf }
            // public LocationTypes LocationType = LocationTypes.Self;

            [Header("AOEの視覚エフェクト")]
            [Tooltip("範囲効果で使用される視覚エフェクト。")]
            public GameObject VisualEffect;

            [Header("視覚エフェクト寿命（秒）")]
            [Range(0.5f, 15)]
            [Tooltip("視覚エフェクトがスポーン後に無効化されるまでの時間（秒）。")]
            public float VisualEffectTimeoutSeconds = 6;

            [Header("高さオフセット")]
            [Range(-5, 5)]
            [Tooltip("視覚エフェクトのスポーン時の高さオフセット。")]
            public float HeightOffset = 0;

            [Header("ダメージ半径")]
            [Range(1, 20)]
            [Tooltip("範囲効果の半径。")]
            public float Radius = 3;

            [Header("判定までの遅延（秒）")]
            [Range(0f, 5f)]
            [Tooltip("AOEのターゲット検出が発動するまでの遅延時間（秒）。")]
            public float Delay = 0;

            [Header("ヒット時の視覚エフェクト")]
            [Tooltip("有効ターゲットにヒットした際に再生されるエフェクト。")]
            public GameObject HitTargetEffect;

            [Header("ヒット時エフェクト寿命（秒）")]
            [Range(0.5f, 15)]
            public float HitTargetEffectTimeoutSeconds = 2;

            [Header("AOE再生時サウンド候補リスト")]
            [Tooltip("視覚エフェクト再生時に鳴るサウンドの候補。")]
            public List<AudioClip> AOESoundsList = new List<AudioClip>();

            /// <summary>
            /// 指定位置にAOEエフェクトをスポーンします。
            /// </summary>
            /// <param name="Owner">所有者。</param>
            /// <param name="SpawnPosition">スポーン位置。</param>
            public GameObject SpawnAOEEffect(GameObject Owner, Vector3 SpawnPosition)
            {
                GameObject SpawnedVisualEffect = null;

                if (Enabled)
                {
                    if (VisualEffect != null)
                    {
                        SpawnedVisualEffect = EmeraldObjectPool.SpawnEffect(VisualEffect, SpawnPosition, VisualEffect.transform.rotation, VisualEffectTimeoutSeconds);
                        SpawnedVisualEffect.name = VisualEffect.name;
                        SpawnedVisualEffect.transform.localScale = VisualEffect.transform.localScale;
                    }

                    if (AOESoundsList.Count > 0)
                    {
                        AudioClip Clip = AOESoundsList[Random.Range(0, AOESoundsList.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }

                return SpawnedVisualEffect;
            }
        }

        /// <summary>
        /// 【SummonData】召喚系（キャスト/召喚演出・音/数・半径・寿命・デスポーン）
        /// </summary>
        [System.Serializable]
        public class SummonData
        {
            [Header("Editor用の折り畳み状態（非表示）")]
            [HideInInspector] public bool Foldout = true;

            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = true;

            [Header("キャスト時エフェクト")]
            [Tooltip("アビリティ詠唱（キャスト）時に使用する視覚エフェクト。")]
            public GameObject CastEffect;

            [Header("キャストエフェクト寿命（秒）")]
            [Range(0.5f, 10)]
            [Tooltip("召喚時エフェクトが無効化されるまでの時間（秒）。")]
            public float CastEffectTimeoutSeconds = 4;

            [Header("キャストサウンド候補")]
            [Tooltip("アビリティがキャストされたときに再生されるサウンド。")]
            public List<AudioClip> CastSounds = new List<AudioClip>();

            [Header("召喚開始時エフェクト")]
            [Tooltip("召喚アビリティが生成されたときに使用する視覚エフェクト。")]
            public GameObject SummonEffect;

            [Header("召喚エフェクト寿命（秒）")]
            [Range(0.5f, 10)]
            [Tooltip("召喚エフェクトが無効化されるまでの時間（秒）。")]
            public float SummonEffectTimeoutSeconds = 4;

            [Header("召喚エフェクト高さオフセット")]
            [Range(-3f, 3f)]
            [Tooltip("召喚エフェクトのY座標オフセット。")]
            public float SummonEffectHeightOffset = 0f;

            [Header("召喚時サウンド候補")]
            [Tooltip("AIが召喚された際に再生されるサウンド。")]
            public List<AudioClip> SummonSounds = new List<AudioClip>();

            [Header("召喚数")]
            [Tooltip("このアビリティで召喚されるAIの数。")]
            [Range(1, 6)]
            public int SummonAmount = 1;

            public enum SummonPositions { Self, Target };

            [Header("召喚位置（自分/ターゲット周辺）")]
            [Tooltip("AIを召喚する基準位置。\n\nSelf - 召喚者の周囲\nTarget - 現在のターゲットの周囲")]
            public SummonPositions SummonPosition = SummonPositions.Self;

            [Header("召喚半径（スポーン円半径）")]
            [Tooltip("召喚位置を中心としたスポーン半径。")]
            [Range(2f, 12f)]
            public float SummonRadius = 4f;

            [Header("召喚までの遅延（秒）")]
            [Range(0f, 1f)]
            [Tooltip("アビリティキャスト後にAIが召喚されるまでの遅延（秒）。")]
            public float SummonDelay = 0;

            [Header("召喚AIプレハブ候補リスト")]
            [Tooltip("召喚に使用されるAIプレハブの候補。Summon Amount の数だけスポーンします。")]
            public List<GameObject> AIPrefabs = new List<GameObject>();

            [Header("時間制限付き召喚（寿命で自動Kill）")]
            [Tooltip("一定時間後に召喚AIを自動的にKillするか。")]
            public bool IsTimedSummon = false;

            [Header("召喚寿命（秒）")]
            [Range(10, 180)]
            [Tooltip("AIがKillされるまでの時間（秒）。")]
            public int SummonLength = 20;

            [Header("死亡後にオブジェクトプールへ戻す（Despawn）")]
            [Tooltip("召喚AIの死体をデスポーン（オブジェクトプールへ返却）するかどうか。返却時に初期状態へリセットされます。")]
            public bool DespawnAfterKilled = false;

            [Header("死亡後デスポーンまでの時間（秒）")]
            [Range(5, 120)]
            [Tooltip("Kill後にオブジェクトプールへ返却するまでの時間（秒）。")]
            public int DespawnLength = 20;

            /// <summary>
            /// 汎用のサウンド/エフェクト生成関数（召喚・キャスト等で共用）
            /// </summary>
            public GameObject SpawnEffect(GameObject Owner, Vector3 SpawnPosition, GameObject VisualEffect, float TimeoutSeconds, List<AudioClip> EffectSounds, bool SkipSound)
            {
                GameObject SpawnedVisualEffect = null;

                if (Enabled)
                {
                    if (VisualEffect != null)
                    {
                        SpawnedVisualEffect = EmeraldObjectPool.SpawnEffect(VisualEffect, SpawnPosition, VisualEffect.transform.rotation, TimeoutSeconds);
                        SpawnedVisualEffect.name = VisualEffect.name;
                        SpawnedVisualEffect.transform.localScale = VisualEffect.transform.localScale;
                    }

                    if (EffectSounds.Count > 0 && !SkipSound)
                    {
                        AudioClip Clip = EffectSounds[Random.Range(0, EffectSounds.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }

                return SpawnedVisualEffect;
            }
        }

        /// <summary>
        /// 【HealingData】回復系（対象/方式/HoT/半径/遅延/演出）
        /// </summary>
        [System.Serializable]
        public class HealingData
        {
            [Header("Editor用の折り畳み状態（非表示）")]
            [HideInInspector] public bool Foldout = true;

            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = true;

            [Header("回復エフェクト（生成時）")]
            [Tooltip("回復アビリティ生成時に使用される視覚エフェクト。")]
            public GameObject HealingEffect;

            [Header("回復エフェクト寿命（秒）")]
            [Range(0.5f, 15)]
            [Tooltip("回復エフェクトが無効化されるまでの時間（秒）。")]
            public float HealingEffectTimeoutSeconds = 6;

            public enum TargetTypes { Self, Target, Area };

            [Header("回復対象タイプ（自分/単体/範囲）")]
            [Tooltip("このアビリティが影響する対象の種類を制御します。\n\nSelf - 施術者自身のみ\nTarget - 半径内の味方のうち最もHPが低い1体\nArea - 半径内の味方全員（Detection Layers に依存）")]
            public TargetTypes TargetType = TargetTypes.Self;

            public enum HealingTypes { Instant, OverTime };

            [Header("回復方式（即時/継続）")]
            [Tooltip("回復方式を制御します。\n\nInstant - Base Heal Amount に従って即時回復\nOver Time - Base Heal Amount に加え、時間経過でも回復（初回回復を不要にする場合は Base Heal Amount=0も可）")]
            public HealingTypes HealingType = HealingTypes.Instant;

            // Base Heals
            [Header("基本回復量（初回の即時回復）")]
            [Tooltip("このアビリティの基本回復量、または初回即時回復量です。")]
            public int BaseHealAmount = 20;
            // Base Heals

            // Heals Over Time
            [Header("HoTのティック間隔（秒）")]
            [Tooltip("継続回復（HoT）を適用する間隔（秒）。")]
            [Range(0.1f, 10f)]
            public float TickRate = 1;

            [Header("ティックごとの回復量")]
            [Tooltip("ティック間隔ごとに回復する量。")]
            public int HealsPerTick = 5;

            [Header("HoTの継続時間（秒）")]
            [Range(0f, 10f)]
            [Tooltip("継続回復の合計継続時間（秒）。")]
            public float HealOverTimeLength = 3;

            [Header("ティック時サウンド候補リスト")]
            [Tooltip("各ティック時に再生するサウンド候補。")]
            public List<AudioClip> HealTickSounds = new List<AudioClip>();
            // Heals Over Time

            [Header("エフェクトの高さオフセット")]
            [Range(-5, 5)]
            [Tooltip("回復エフェクトの高さオフセット。")]
            public float EffectHeightOffset = 0;

            [Header("回復半径")]
            [Range(1, 20)]
            [Tooltip("回復アビリティの半径。")]
            public float Radius = 3;

            [Header("検出遅延（秒）")]
            [Range(0f, 5f)]
            [Tooltip("範囲回復のターゲット検出までの遅延。")]
            public float Delay = 0;

            [Header("対象回復時エフェクト")]
            [Tooltip("有効ターゲットが回復した際の視覚エフェクト。")]
            public GameObject HealTargetEffect;

            [Header("対象回復時エフェクト寿命（秒）")]
            [Range(0.5f, 15)]
            public float HealTargetEffectTimeoutSeconds = 2;

            [Header("回復再生時サウンド候補リスト")]
            [Tooltip("回復エフェクト再生時のサウンド候補。")]
            public List<AudioClip> HealingSoundsList = new List<AudioClip>();

            /// <summary>
            /// 回復系の汎用エフェクト/サウンド生成関数。
            /// </summary>
            public GameObject SpawnHealingEffect(GameObject Owner, Vector3 SpawnPosition, GameObject VisualEffect, float TimeoutSeconds, List<AudioClip> EffectSounds)
            {
                GameObject SpawnedVisualEffect = null;

                if (Enabled)
                {
                    if (VisualEffect != null)
                    {
                        SpawnedVisualEffect = EmeraldObjectPool.SpawnEffect(VisualEffect, SpawnPosition, VisualEffect.transform.rotation, TimeoutSeconds);
                        SpawnedVisualEffect.name = VisualEffect.name;
                        SpawnedVisualEffect.transform.localScale = VisualEffect.transform.localScale;
                    }

                    if (EffectSounds.Count > 0)
                    {
                        AudioClip Clip = EffectSounds[Random.Range(0, EffectSounds.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }

                return SpawnedVisualEffect;
            }
        }

        /// <summary>
        /// 【ConditionData】発動条件（自己/味方の低HP、対象距離、召喚数など）
        /// </summary>
        [System.Serializable]
        public class ConditionData
        {
            [Header("Editor用の折り畳み状態（非表示）")]
            [HideInInspector] public bool Foldout = true;

            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled;

            [Header("条件種別（自己低HP/味方低HP/対象距離/召喚数ゼロ）")]
            [Tooltip("アビリティ発動に必要な条件を制御します。High Priority を有効にすると他アビリティより優先して選択されます。\n\nSelf Low Health - 自身のHPがしきい値以下\nTarget Low Health - 近くの味方のHPがしきい値以下\nNo Current Summons - 現在の召喚数が0")]
            public ConditionTypes ConditionType = ConditionTypes.SelfLowHealth;

            public enum ValueCompareTypes { GreaterThan, LessThan };

            [Header("比較方法（以上/以下）")]
            [Tooltip("値比較の方法を制御します。")]
            public ValueCompareTypes ValueCompareType = ValueCompareTypes.LessThan;

            [Header("高優先度（Pick Type を無視して優先発動）")]
            [Tooltip("この条件が満たされたとき、他のアビリティより優先して実行します（AIのPick Typeを無視）。特に支援系（回復等）で有効。\n\n注意: 無効の場合は通常のPick Typeに従いますが、条件未達ならスキップされます。")]
            public bool HighPriority = false;

            [Header("対象との距離しきい値（Distance From Target）")]
            [Range(1, 40)]
            [Tooltip("Distance from Target 条件のための距離しきい値。")]
            public float DistanceFromTarget = 3f;

            [Header("低HPしきい値（％）")]
            [Range(1, 99)]
            [Tooltip("低HPと見なす割合（%）。")]
            public float LowHealthPercentage = 60f;
        }

        /// <summary>
        /// 【ColliderData】投射の衝突判定（コリジョン対象/レイヤー/タイムアウト/半径/前方オフセット）
        /// </summary>
        [System.Serializable]
        public class ColliderData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = true;

            [Header("衝突可能レイヤー")]
            [Tooltip("この投射体が衝突可能なレイヤーを制御します。\n\n注意: ターゲットレイヤー（および Location Based Damage のレイヤー）を含めないと無視されます。")]
            public LayerMask CollidableLayers = ~0;

            [Header("投射体が属するレイヤー")]
            [Tooltip("投射体に割り当てるレイヤー。\n\n注意: 投射体同士の意図しない衝突を避けるため、互いに無視する設定を推奨。")]
            [Layer] public int ProjectileLayer = 2;

            [Header("衝突後にオブジェクトを無効化するまでの時間（秒）")]
            [Range(0f, 30f)]
            [Tooltip("ターゲット/オブジェクトに衝突後、投射本体を無効化するまでの時間（秒）。")]
            public float CollisionTimeout = 3;

            [Header("自動生成SphereColliderの半径")]
            [Range(0, 1)]
            [Tooltip("投射体の Sphere Collider 半径。")]
            [DrawIf("AutoCreateSphereCollider", true)]
            public float ColliderRadius = 0.05f;

            [Header("自動生成SphereColliderの前方オフセット（Z）")]
            [Range(-2, 2)]
            [Tooltip("投射体の Sphere Collider の前方位置オフセット。")]
            public float ZOffet = 0;
        }

        /// <summary>
        /// 【SpreadData】散布パターン（ランダム/扇形）と各パラメータ
        /// </summary>
        [System.Serializable]
        public class SpreadData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled;

            [Header("散布方式（ランダム/水平半径）")]
            [Tooltip("投射体の散布方式を制御します。")]
            public SpreadTypes SpreadType = SpreadTypes.Random;

            [Header("ランダム散布：X最小角")]
            [CompareEnumWithRange("SpreadType", 0f, 180f, CompareEnumWithRangeAttribute.StyleType.FloatSlider, SpreadTypes.Random)]
            public float MinSpreadX = 0;

            [Header("ランダム散布：X最大角")]
            [CompareEnumWithRange("SpreadType", -180f, 0, CompareEnumWithRangeAttribute.StyleType.FloatSlider, SpreadTypes.Random)]
            public float MaxSpreadX = 0;

            [Header("ランダム散布：Y最小角")]
            [CompareEnumWithRange("SpreadType", 0f, 180f, CompareEnumWithRangeAttribute.StyleType.FloatSlider, SpreadTypes.Random)]
            public float MinSpreadY = 0;

            [Header("ランダム散布：Y最大角")]
            [CompareEnumWithRange("SpreadType", -180f, 0, CompareEnumWithRangeAttribute.StyleType.FloatSlider, SpreadTypes.Random)]
            public float MaxSpreadY = 0;

            [Header("水平半径散布：水平角（X）")]
            [Tooltip("投射体を水平に均等配分する角度。総投射数に応じて均等分割されます。")]
            [CompareEnumWithRange("SpreadType", 0f, 360f, CompareEnumWithRangeAttribute.StyleType.FloatSlider, SpreadTypes.HorizontalRadius)]
            public float SpreadAngleX = 180;

            [Header("水平半径散布：傾き角（Y）")]
            [Tooltip("上下方向の傾き角（均等配分）。")]
            [CompareEnumWithRange("SpreadType", 0f, 90f, CompareEnumWithRangeAttribute.StyleType.FloatSlider, SpreadTypes.HorizontalRadius)]
            public float TiltAngleY = 0;

            [Header("スポーン距離（所有者からの距離）")]
            [Tooltip("投射体を所有者からどれだけ離してスポーンさせるか。")]
            [CompareEnumWithRange("SpreadType", 0f, 6f, CompareEnumWithRangeAttribute.StyleType.FloatSlider, SpreadTypes.HorizontalRadius)]
            public float SpawnDistance = 0.5f;
        }

        /// <summary>
        /// 【TargetTypeData】ターゲット選択（現在/複数 等）
        /// </summary>
        [System.Serializable]
        public class TargetTypeData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = true;

            [Header("ターゲット選択方式")]
            [Tooltip("アビリティのターゲット選択方法を制御します。\n\n注意: Multiple を選ぶと複数の対象に影響/ターゲット可能になります。不明な場合は Current Target を推奨。")]
            public TargetTypes TargetType = TargetTypes.CurrentTarget;
        }

        /// <summary>
        /// 【CooldownData】クールダウン設定
        /// </summary>
        [System.Serializable]
        public class CooldownData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled;

            [Header("クールダウン長（秒）")]
            [Range(0, 60)]
            [Tooltip("このアビリティを再使用可能になるまでの時間（秒）。連続使用を抑制するのに有効。\n\n注意: アニメーションイベント経由の Create Ability により上書きされる場合、この設定は無視されます。")]
            public float CooldownLength = 1;
        }

        // TODO: 将来のアップデートで分岐（Branch）機能を追加予定
        /*
        [System.Serializable]
        public class BranchData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [Tooltip("このモジュールを有効にするかどうかを制御します。")]
            [HideInInspector] public bool Enabled;

            [Header("ターゲットの種類")]
            [Tooltip("このアビリティが影響するターゲットの種類を制御します。")]
            public TargetTypes TargetType = TargetTypes.CurrentTarget;

            [Header("分岐の検出半径")]
            [Range(0.25f, 10)]
            [Tooltip("分岐に使用するターゲット検出半径を制御します。")]
            public float BranchRadius = 1f;

            [Header("分岐成功の確率（%）")]
            [Range(1, 100)]
            [Tooltip("分岐が成功する確率を制御します。")]
            public int BranchOdds = 20;

            [Header("1つの投射につき最大分岐数")]
            [Range(1, 25)]
            [Tooltip("1つの投射が分岐できる上限回数。")]
            public int BranchCap = 5;
        }
        */

        /// <summary>
        /// 【TeleportData】テレポート演出（消失/再出現/遅延/半径/回避可能通知）
        /// </summary>
        [System.Serializable]
        public class TeleportData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = true;

            [Header("消失エフェクト")]
            [Tooltip("テレポート時にAIが消える際のエフェクト。")]
            public GameObject DisappearEffect;

            [Header("消失エフェクト寿命（秒）")]
            [Range(0.5f, 15)]
            public float DisappearEffectTimeoutSeconds = 2;

            [Header("消失時サウンド候補")]
            public List<AudioClip> DisappearSoundsList = new List<AudioClip>();

            [Header("再出現前の回避可能通知（Avoidable Call）")]
            [Tooltip("再出現時に回避可能通知を送るかどうか。\n\n注意: 近くのターゲットに回避/ブロックの機会を与えます（対応アクションが必要）。")]
            public bool ReappearTriggersAvoidable;

            [Header("再出現インジケータ（事前表示）")]
            [Tooltip("AIが再出現する直前に表示する目印用エフェクト。現れる位置の予告に使用。")]
            public GameObject ReappearIndicatorEffect;

            [Header("再出現インジケータ寿命（秒）")]
            [Range(0.5f, 15)]
            public float ReappearIndicatorEffectTimeoutSeconds = 2;

            [Header("再出現までの遅延（秒）")]
            [Range(0f, 2.5f)]
            [Tooltip("再出現処理が遅延する時間（秒）。")]
            public float ReappearDelay = 0.15f;

            [Header("再出現インジケータ サウンド候補")]
            public List<AudioClip> ReappearIndicatorSoundsList = new List<AudioClip>();

            [Space(10)]

            [Header("再出現エフェクト")]
            [Tooltip("テレポート後にAIが再出現する際のエフェクト。")]
            public GameObject ReappearEffect;

            [Header("再出現エフェクト寿命（秒）")]
            [Range(0.5f, 15)]
            public float ReappearEffectTimeoutSeconds = 2;

            [Header("再出現サウンド候補")]
            public List<AudioClip> ReappearSoundsList = new List<AudioClip>();

            [Space(10)]

            [Header("テレポートに要する時間（秒）")]
            [Range(0f, 10)]
            [Tooltip("AIが消えてから再出現するまでの時間。")]
            public float TeleportTime = 1;

            [Header("再出現位置の生成半径")]
            [Range(0f, 10)]
            [Tooltip("テレポート先の生成半径。先の基準は Target Type に依存します。")]
            public float TeleportRadius = 3;
        }

        /// <summary>
        /// 【StunnedData】スタン付与設定（確率/時間）
        /// </summary>
        [System.Serializable]
        public class StunnedData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = false;

            [Header("スタン発生確率（%）")]
            [Range(0, 100)]
            [Tooltip("敵対象にヒットした際、スタンが発生する確率（%）。")]
            public float OddsToStun = 50;

            [Header("スタン継続時間（秒）")]
            [Range(1, 15)]
            [Tooltip("対象がスタンする時間（秒）。")]
            public float StunLength = 3;

            /// <summary>
            /// スタンの成否を確率で判定して返します。
            /// </summary>
            public bool RollForStun()
            {
                int Roll = Random.Range(1, 101);
                return (Roll <= OddsToStun);
            }
        }

        /// <summary>
        /// 【KnockbackData】ノックバック設定（確率/距離/時間/プレイヤー影響可否）
        /// </summary>
        [System.Serializable]
        public class KnockbackData
        {
            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = false;

            [Header("ノックバック発生確率（%）")]
            [Range(0, 100)]
            [Tooltip("敵対象にヒットした際、ノックバックが発生する確率（%）。")]
            public float OddsToKnockback = 100;

            [Header("ノックバック距離")]
            [Range(1f, 8f)]
            [Tooltip("ターゲットが押し戻される距離。攻撃者のForward方向を基準にします。")]
            public float KnockbackDistance = 2.5f;

            [Header("ノックバック所要時間（秒）")]
            [Range(0.1f, 1f)]
            [Tooltip("ノックバック移動にかかる時間（秒）。")]
            public float KnockbackDuration = 0.25f;

            [Header("移動再開までの遅延（秒、AI対象）")]
            [Range(0.0f, 1f)]
            [Tooltip("ノックバック後、Emerald AI ターゲットが移動を再開するまでの遅延（秒）。")]
            public float MovementDelay = 0.25f;

            [Header("プレイヤー対象にも適用する")]
            [Tooltip("ノックバックをプレイヤーにも適用するか。プレイヤーがノックバック後に地面高さを自動更新できる環境が必要。\n\n注意: プレイヤーが地面貫通/空中停止となる場合は使用不可。NavMesh使用のプレイヤーなら無視可能（NavMeshで高さ調整）。")]
            public bool AffectsPlayerTargets = false;

            public IEnumerator KnockbackSequence(Vector3 Direction, Transform Target, ICombat TargetICombat)
            {
                // ブロック/回避中ならノックバックを無効化
                if (TargetICombat.IsBlocking() || TargetICombat.IsDodging())
                {
                    yield break;
                }

                UnityEngine.AI.NavMeshAgent navMeshAgent = Target.GetComponent<UnityEngine.AI.NavMeshAgent>();
                EmeraldSystem TargetEmeraldComponent = Target.GetComponent<EmeraldSystem>();

                Vector3 destination = Target.position + Direction * KnockbackDistance;
                destination.y = Target.position.y;

                // 死亡状態なら終了
                if (TargetEmeraldComponent && TargetEmeraldComponent.AnimationComponent.IsDead)
                {
                    yield break;
                }

                // 進行方向が障害物で塞がれている場合はノックバックを中止
                Ray ray = new Ray(Target.position + Vector3.up * 1f, Direction * KnockbackDistance);
                // Debug.DrawRay(Target.position + Vector3.up * 1f, Direction * KnockbackDistance, Color.red, 10f);
                if (Physics.Raycast(ray, out RaycastHit hit, KnockbackDistance, TargetEmeraldComponent.MovementComponent.AlignmentLayerMask))
                {
                    yield break;
                }

                Vector3 start = Target.position;
                float elapsed = 0f;
                float t = 0f;

                while (t < 1f)
                {
                    t = elapsed / KnockbackDuration;
                    Vector3 flatPos = Vector3.Lerp(start, destination, t);
                    Vector3 groundPos = GetGroundedPosition(flatPos);

                    if (navMeshAgent) navMeshAgent.Warp(groundPos);
                    else if (AffectsPlayerTargets) Target.position = flatPos;

                    elapsed += Time.deltaTime;

                    // ノックバック中に死亡したら中断
                    if (TargetEmeraldComponent && TargetEmeraldComponent.AnimationComponent.IsDead)
                    {
                        yield break;
                    }

                    yield return null;
                }

                if (navMeshAgent) navMeshAgent.isStopped = true;
                yield return new WaitForSeconds(MovementDelay);
                if (navMeshAgent) navMeshAgent.isStopped = false;

                /// <summary>
                /// レイキャストで地面に合わせた位置を返す補助関数
                /// </summary>
                Vector3 GetGroundedPosition(Vector3 position)
                {
                    if (TargetEmeraldComponent)
                    {
                        Ray ray = new Ray(position + Vector3.up * 0.2f, Vector3.down);
                        if (Physics.Raycast(ray, out RaycastHit hit, 2f, TargetEmeraldComponent.MovementComponent.AlignmentLayerMask))
                        {
                            position.y = hit.point.y;
                        }
                        return position;
                    }
                    else
                    {
                        return position;
                    }
                }
            }

            /// <summary>
            /// ノックバックの成否を確率で判定して返します。
            /// </summary>
            public bool RollForKnockback()
            {
                int Roll = Random.Range(1, 101);
                return (Roll <= OddsToKnockback);
            }
        }

        /// <summary>
        /// 汎用の「エフェクト+サウンド」生成ユーティリティ（任意の場面で使用可）
        /// </summary>
        public static void SpawnEffectAndSound(GameObject Owner, Vector3 SpawnPosition, GameObject Effect, float TimeoutSeconds, List<AudioClip> SoundsList)
        {
            if (Effect != null)
            {
                GameObject SpawnedEffect = EmeraldObjectPool.SpawnEffect(Effect, SpawnPosition, Effect.transform.rotation, TimeoutSeconds);
                SpawnedEffect.name = Effect.name;
                SpawnedEffect.transform.localScale = Effect.transform.localScale;
            }

            if (SoundsList.Count > 0)
            {
                AudioClip Clip = SoundsList[Random.Range(0, SoundsList.Count)];
                if (Clip)
                {
                    AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                    TempSound.volume = Random.Range(0.75f, 1f);
                    TempSound.pitch = Random.Range(0.9f, 1.1f);
                    TempSound.PlayOneShot(Clip);
                }
            }
        }

        /// <summary>
        /// 【DamageData】ダメージ設定（基本/クリティカル/DoT）
        /// </summary>
        [System.Serializable]
        public class DamageData
        {
            [Header("Editor用の折り畳み状態（非表示）")]
            [HideInInspector] public bool Foldout;

            [Header("モジュールの有効/無効（インスペクタ非表示）")]
            [HideInInspector] public bool Enabled = true;

            [Space(10)]

            [Header("クリティカルヒットを使用")]
            [Tooltip("このアビリティでクリティカルヒットを使用するかどうか。")]
            public bool UseCriticalHits;

            [Space(10)]

            [Header("ダメージ継続（DoT）を使用")]
            [Tooltip("このアビリティで継続ダメージ（Damage Over Time）を使用するか。")]
            public bool UseDamageOverTime;

            [Header("基本ダメージ設定（固定/ランダム/ラグドール押し出し）")]
            public BaseDamageClass BaseDamageSettings;

            /// <summary>
            /// 【BaseDamageClass】基本ダメージ（固定orランダム）とラグドール力
            /// </summary>
            [System.Serializable]
            public class BaseDamageClass
            {
                [Header("ダメージ量をランダム化")]
                [Tooltip("このアビリティでダメージ量のランダム化を使用するかどうか。")]
                public bool UseRandomAmounts;

                [Header("固定ダメージ量（ランダム無効時）")]
                [DrawIf("UseRandomAmounts", false)]
                [Tooltip("ランダムを使用しない場合の基本ダメージ量。")]
                public int BaseAmount = 5;

                [Header("最小ダメージ量（ランダム有効時）")]
                [DrawIf("UseRandomAmounts", true)]
                [Tooltip("ランダム生成される最小ダメージ量。")]
                public int MinAmount = 5;

                [Header("最大ダメージ量（ランダム有効時）")]
                [DrawIf("UseRandomAmounts", true)]
                [Tooltip("ランダム生成される最大ダメージ量。")]
                public int MaxAmount = 10;

                [Header("ラグドールへの押し出し力")]
                [Tooltip("死亡時にラグドールへ加える力。")]
                public int RagdollForce = 50;
            }

            [Header("クリティカル設定（確率/倍率/サウンド）")]
            public CriticalHitClass CriticalHitSettings;

            /// <summary>
            /// 【CriticalHitClass】クリティカル確率・倍率・サウンド
            /// </summary>
            [System.Serializable]
            public class CriticalHitClass
            {
                [Header("クリティカル発生確率（%）")]
                [Tooltip("クリティカルヒットの発生確率。")]
                [Range(0f, 100f)]
                public float CriticalHitOdds = 6.25f;

                [Header("クリティカル倍率（%加算）")]
                [Tooltip("クリティカル時の倍率（GeneratedDamage + (GeneratedDamage * (%)CriticalHitMultiplier) = 最終ダメージ）。")]
                public float CriticalHitMultiplier = 1.1f;

                [Header("クリティカル時サウンド候補")]
                [Tooltip("クリティカルヒット成功時に再生されるサウンド。")]
                public List<AudioClip> CriticalHitSounds = new List<AudioClip>();
            }

            [Header("DoT設定（エフェクト/ティック/合計時間/サウンド）")]
            public DamageOverTimeClass DamageOverTimeSettings;

            /// <summary>
            /// 【DamageOverTimeClass】継続ダメージ（DOT）の詳細
            /// </summary>
            [System.Serializable]
            public class DamageOverTimeClass
            {
                [Header("ティック毎にスポーンするエフェクト")]
                [Tooltip("各ティックでスポーンするエフェクト。")]
                public GameObject DamageOverTimeEffect;

                [Header("ティックエフェクト寿命（秒）")]
                [Range(0.5f, 5)]
                public float OverTimeEffectTimeOutSeconds = 1.5f;

                [Header("ティック間隔（秒）")]
                [Tooltip("継続ダメージの適用間隔（秒）。")]
                [Range(0.1f, 10f)]
                public float TickRate = 1;

                [Header("ティック毎のダメージ量")]
                [Tooltip("ティックごとに与えるダメージ量。")]
                public int DamagePerTick = 1;

                [Header("継続時間（秒）")]
                [Range(0f, 10f)]
                [Tooltip("継続ダメージの合計時間（秒）。")]
                public float DamageOverTimeLength = 3;

                [Header("ティック時サウンド候補")]
                [Tooltip("各ティックで再生されるサウンドの候補。")]
                public List<AudioClip> OverTimeSounds = new List<AudioClip>();
            }

            /// <summary>
            /// 初期ダメージを生成します（基本/ランダム/クリティカルを考慮。DoTは除外）。
            /// </summary>
            public int GenerateDamage(bool IsCritHit)
            {
                int DamageAmount = 0;
                if (!BaseDamageSettings.UseRandomAmounts) DamageAmount = BaseDamageSettings.BaseAmount;
                else if (BaseDamageSettings.UseRandomAmounts) DamageAmount = Random.Range(BaseDamageSettings.MinAmount, BaseDamageSettings.MaxAmount + 1);
                if (UseCriticalHits && IsCritHit) DamageAmount = DamageAmount + Mathf.FloorToInt(DamageAmount * (CriticalHitSettings.CriticalHitMultiplier * 0.01f));

                return DamageAmount;
            }

            /// <summary>
            /// DoTを初期化し、対象へ継続ダメージを与えます（アビリティがDoTを使用する場合）。
            /// </summary>
            public void DamageTargetOverTime(EmeraldAbilityObject AbilityObject, DamageData DamageDataInfo, GameObject Owner, GameObject Target)
            {
                if (UseDamageOverTime)
                {
                    if (IDamageableHelper.CheckAbilityActiveEffects(Target, AbilityObject))
                    {
                        // 既存ActiveEffectに無い場合は追加
                        IDamageableHelper.AddAbilityActiveEffect(Target, AbilityObject);
                        GameObject SpawnedDamageOverTimeComponent = EmeraldObjectPool.Spawn(Resources.Load("Damage Over Time Component") as GameObject, Target.transform.position, Quaternion.identity);
                        SpawnedDamageOverTimeComponent.GetComponent<EmeraldDamageOverTime>().Initialize(AbilityObject, DamageDataInfo, Target.transform, Owner.transform);
                    }
                }
            }

            /// <summary>
            /// クリティカルヒット判定を行います（CriticalHitOdds に基づく）。
            /// </summary>
            public bool GenerateCritHit()
            {
                bool CriticalHit = false;
                float m_GeneratedOdds = Random.Range(0.0f, 1.0f);
                m_GeneratedOdds = Mathf.RoundToInt(m_GeneratedOdds * 100);

                if (m_GeneratedOdds <= CriticalHitSettings.CriticalHitOdds)
                {
                    CriticalHit = true;
                }

                return CriticalHit;
            }
        }

        // ===== 列挙型（ターゲット元/ターゲット種別/散布方式） =====

        //[Header("ターゲットの敵味方種別（列挙型）")]
        public enum TargetSources { Enemy, Ally }

        //[Header("ターゲット選択方式（列挙型）")]
        public enum TargetTypes
        {
            CurrentTarget,              // 現在のターゲット
            ClosestEnemy,               // 最も近い敵
            SingleRandomEnemy,          // ランダムな敵（単体）
            MultipleRandomEnemies,      // ランダムな敵（複数）
            // TODO: 将来追加
            // ClosestAlly,
            // SingleRandomAlly,
            // MultipleRandomAllies,
            // RandomGroundPosition,
        }

        //[Header("散布方式（列挙型）")]
        public enum SpreadTypes
        {
            Random,                     // ランダム散布
            HorizontalRadius,           // 水平半径での均等散布
        }
    }
}
