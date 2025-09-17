using System.Collections;                         // コルーチンに使用
using System.Collections.Generic;                 // List 等のコレクション
using UnityEngine;                                // Unity の基本 API

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/sounds-component")]
    // 【クラス概要】EmeraldSounds：
    //  Emerald AI のサウンド周り（待機・攻撃・負傷・死亡・装備/収納・足音・汎用SE）を一元管理するコンポーネント。
    //  ・サウンドプロファイルの設定と AudioSource 構成（メイン/セカンダリ/イベント）
    //  ・EmeraldHealth/EmeraldItems のイベントに購読し、状況に応じてSEを再生
    //  ・足音は Footsteps コンポーネント未使用時のみこちらで再生
    public class EmeraldSounds : MonoBehaviour
    {
        #region Variables
        [Header("このAIのサウンド設定（各種クリップと音量を保持）")]
        public Utility.EmeraldSoundProfile SoundProfile;

        [Header("インスペクタ：Sound Profile セクションの折りたたみ")]
        public bool SoundProfileFoldout;

        [Header("インスペクタ：設定全体を隠す")]
        public bool HideSettingsFoldout;

        [Header("待機音（Idle）を鳴らすまでの秒数（ランダムに再設定）")]
        public int IdleSoundsSeconds;

        [Header("待機音（Idle）の経過タイマー（秒）")]
        public float IdleSoundsTimer;

        [Header("メインの AudioSource（優先的に使用）")]
        public AudioSource m_AudioSource;

        [Header("セカンダリ AudioSource（メイン再生中のバックアップ）")]
        public AudioSource m_SecondaryAudioSource;

        [Header("イベント用 AudioSource（上記2つが埋まっている時に使用）")]
        public AudioSource m_EventAudioSource;

        [Header("主要コンポーネント EmeraldSystem 参照（内部で取得）")]
        EmeraldSystem EmeraldComponent;

        [Header("体力・被弾イベントの参照（内部で取得）")]
        EmeraldHealth EmeraldHealth;

        [Header("装備/収納イベントの参照（内部で取得・任意）")]
        EmeraldItems EmeraldItems;
        #endregion

        void Awake()
        {
            InitializeSounds(); // EmeraldSounds の初期化
        }

        /// <summary>
        /// （日本語）サウンド設定を初期化します。
        /// ・主要コンポーネントの取得
        /// ・SoundProfile が有効な場合のみ、各イベントへ購読
        /// ・AudioSource を（メイン/セカンダリ/イベント）で構成・同期
        /// ・Idle の初期待機秒をランダム決定
        /// </summary>
        public void InitializeSounds()
        {
            EmeraldHealth = GetComponent<EmeraldHealth>();      // 被弾・死亡イベント元
            EmeraldItems = GetComponent<EmeraldItems>();        // 装備/収納イベント元（無い場合あり）
            EmeraldComponent = GetComponent<EmeraldSystem>();   // 中心コンポーネント

            // SoundProfile が未設定なら、イベント購読は行わず終了
            if (SoundProfile == null)
                return;

            // Health 由来のイベントに購読
            EmeraldHealth.OnTakeDamage += PlayInjuredSound;     // 負傷 SE
            EmeraldHealth.OnTakeCritDamage += PlayInjuredSound; // クリティカル負傷 SE
            EmeraldHealth.OnBlock += PlayBlockSound;            // ブロック SE
            EmeraldHealth.OnDeath += PlayDeathSound;            // 死亡 SE

            // Items 由来のイベントに購読（存在する時のみ）
            if (EmeraldItems != null)
            {
                EmeraldItems.OnEquipWeapon += PlayEquipSound;   // 装備 SE
                EmeraldItems.OnUnequipWeapon += PlayUnequipSound; // 収納 SE
            }

            // Idle 関連の初期化
            IdleSoundsSeconds = Random.Range(SoundProfile.IdleSoundsSecondsMin, SoundProfile.IdleSoundsSecondsMax + 1);

            // AudioSource 構成：メイン取得＋セカンダリ/イベントを追加し、主要プロパティをメインに合わせる
            m_AudioSource = GetComponent<AudioSource>();
            m_SecondaryAudioSource = gameObject.AddComponent<AudioSource>();
            m_SecondaryAudioSource.priority = m_AudioSource.priority;
            m_SecondaryAudioSource.spatialBlend = m_AudioSource.spatialBlend;
            m_SecondaryAudioSource.minDistance = m_AudioSource.minDistance;
            m_SecondaryAudioSource.maxDistance = m_AudioSource.maxDistance;
            m_SecondaryAudioSource.rolloffMode = m_AudioSource.rolloffMode;

            m_EventAudioSource = gameObject.AddComponent<AudioSource>();
            m_EventAudioSource.priority = m_AudioSource.priority;
            m_EventAudioSource.spatialBlend = m_AudioSource.spatialBlend;
            m_EventAudioSource.minDistance = m_AudioSource.minDistance;
            m_EventAudioSource.maxDistance = m_AudioSource.maxDistance;
            m_EventAudioSource.rolloffMode = m_AudioSource.rolloffMode;
        }

        /// <summary>
        /// （日本語）Idle の経過時間を更新し、設定秒を超えたらランダムな待機音を再生します。
        /// </summary>
        public void IdleSoundsUpdate()
        {
            IdleSoundsTimer += Time.deltaTime;                  // 経過加算
            if (IdleSoundsTimer >= IdleSoundsSeconds)           // しきい値到達
            {
                PlayIdleSound();                                // Idle SE 再生
                IdleSoundsTimer = 0;                            // タイマーリセット
            }
        }

        /// <summary>
        /// （日本語）任意の AudioClip を即時再生します（音量=1、3系統の AudioSource を順に使用）。
        /// </summary>
        public void PlaySoundClip(AudioClip Clip)
        {
            if (!m_AudioSource.isPlaying)                       // メインが空いていれば使用
            {
                m_AudioSource.volume = 1;
                m_AudioSource.PlayOneShot(Clip);
            }
            else if (!m_SecondaryAudioSource.isPlaying)         // セカンダリが空いていれば使用
            {
                m_SecondaryAudioSource.volume = 1;
                m_SecondaryAudioSource.PlayOneShot(Clip);
            }
            else                                                // どちらも再生中ならイベント用を使用
            {
                m_EventAudioSource.volume = 1;
                m_EventAudioSource.PlayOneShot(Clip);
            }
        }

        /// <summary>
        /// （日本語）音量を指定して AudioClip を再生します（3系統を順に使用）。
        /// </summary>
        public void PlayAudioClip(AudioClip Clip, float Volume = 1)
        {
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.volume = Volume;
                m_AudioSource.PlayOneShot(Clip);
            }
            else if (!m_SecondaryAudioSource.isPlaying)
            {
                m_SecondaryAudioSource.volume = Volume;
                m_SecondaryAudioSource.PlayOneShot(Clip);
            }
            else
            {
                m_EventAudioSource.volume = Volume;
                m_EventAudioSource.PlayOneShot(Clip);
            }
        }

        /// <summary>
        /// （日本語）カスタム音量で AudioClip を再生します（PlayAudioClip と同等）。
        /// </summary>
        public void PlaySoundClipWithVolume(AudioClip Clip, float Volume)
        {
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.volume = Volume;
                m_AudioSource.PlayOneShot(Clip);
            }
            else if (!m_SecondaryAudioSource.isPlaying)
            {
                m_SecondaryAudioSource.volume = Volume;
                m_SecondaryAudioSource.PlayOneShot(Clip);
            }
            else
            {
                m_EventAudioSource.volume = Volume;
                m_EventAudioSource.PlayOneShot(Clip);
            }
        }

        /// <summary>
        /// （日本語）ランダムな待機（Idle）サウンドを再生します。再生後、次回までの待機秒をランダム＋クリップ長で再設定します。
        /// </summary>
        public void PlayIdleSound()
        {
            if (SoundProfile && SoundProfile.IdleSounds.Count > 0)
            {
                if (!m_AudioSource.isPlaying)
                {
                    AudioClip m_RandomIdleSoundClip = SoundProfile.IdleSounds[Random.Range(0, SoundProfile.IdleSounds.Count)];
                    if (m_RandomIdleSoundClip != null)
                    {
                        m_AudioSource.volume = SoundProfile.IdleVolume;
                        m_AudioSource.PlayOneShot(m_RandomIdleSoundClip);
                        IdleSoundsSeconds = Random.Range(SoundProfile.IdleSoundsSecondsMin, SoundProfile.IdleSoundsSecondsMax);
                        IdleSoundsSeconds = (int)m_RandomIdleSoundClip.length + IdleSoundsSeconds;
                    }
                }
                else
                {
                    AudioClip m_RandomIdleSoundClip = SoundProfile.IdleSounds[Random.Range(0, SoundProfile.IdleSounds.Count)];
                    if (m_RandomIdleSoundClip != null)
                    {
                        m_SecondaryAudioSource.volume = SoundProfile.IdleVolume;
                        m_SecondaryAudioSource.PlayOneShot(m_RandomIdleSoundClip);
                        IdleSoundsSeconds = Random.Range(SoundProfile.IdleSoundsSecondsMin, SoundProfile.IdleSoundsSecondsMax);
                        IdleSoundsSeconds = (int)m_RandomIdleSoundClip.length + IdleSoundsSeconds;
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）攻撃サウンドをランダム再生します（アニメーションイベントからも呼出可能）。
        /// </summary>
        public void PlayAttackSound()
        {
            if (SoundProfile.AttackSounds.Count > 0)
            {
                if (!m_AudioSource.isPlaying)
                {
                    m_AudioSource.volume = SoundProfile.AttackVolume;
                    m_AudioSource.pitch = Mathf.Round(Random.Range(0.9f, 1.1f) * 10) / 10;
                    m_AudioSource.PlayOneShot(SoundProfile.AttackSounds[Random.Range(0, SoundProfile.AttackSounds.Count)]);
                }
                else if (!m_SecondaryAudioSource.isPlaying)
                {
                    m_SecondaryAudioSource.volume = SoundProfile.AttackVolume;
                    m_SecondaryAudioSource.PlayOneShot(SoundProfile.AttackSounds[Random.Range(0, SoundProfile.AttackSounds.Count)]);
                }
                else
                {
                    m_EventAudioSource.volume = SoundProfile.AttackVolume;
                    m_EventAudioSource.PlayOneShot(SoundProfile.AttackSounds[Random.Range(0, SoundProfile.AttackSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// （日本語）装備サウンドを再生します（EquipWeapon アニメーションイベントから自動的に呼ばれます）。
        /// WeaponType は "Weapon Type 1" または "Weapon Type 2" を想定。
        /// </summary>
        public void PlayEquipSound(string WeaponType)
        {
            if (WeaponType == "Weapon Type 1")
            {
                if (SoundProfile.UnsheatheWeapon != null)
                {
                    m_AudioSource.volume = SoundProfile.EquipVolume;
                    m_SecondaryAudioSource.volume = SoundProfile.EquipVolume;

                    if (!m_AudioSource.isPlaying)
                    {
                        m_AudioSource.PlayOneShot(SoundProfile.UnsheatheWeapon);
                    }
                    else if (!m_SecondaryAudioSource.isPlaying)
                    {
                        m_SecondaryAudioSource.PlayOneShot(SoundProfile.UnsheatheWeapon);
                    }
                    else
                    {
                        m_EventAudioSource.PlayOneShot(SoundProfile.UnsheatheWeapon);
                    }
                }
            }
            else if (WeaponType == "Weapon Type 2")
            {
                if (SoundProfile.RangedUnsheatheWeapon != null)
                {
                    m_AudioSource.volume = SoundProfile.RangedEquipVolume;
                    m_SecondaryAudioSource.volume = SoundProfile.RangedEquipVolume;

                    if (!m_AudioSource.isPlaying)
                    {
                        m_AudioSource.PlayOneShot(SoundProfile.RangedUnsheatheWeapon);
                    }
                    else if (!m_SecondaryAudioSource.isPlaying)
                    {
                        m_SecondaryAudioSource.PlayOneShot(SoundProfile.RangedUnsheatheWeapon);
                    }
                    else
                    {
                        m_EventAudioSource.PlayOneShot(SoundProfile.RangedUnsheatheWeapon);
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）収納サウンドを再生します（UnequipWeapon アニメーションイベントから自動的に呼ばれます）。
        /// WeaponType は "Weapon Type 1" または "Weapon Type 2" を想定。
        /// </summary>
        public void PlayUnequipSound(string WeaponType)
        {
            if (WeaponType == "Weapon Type 1")
            {
                if (SoundProfile.SheatheWeapon != null)
                {
                    m_AudioSource.volume = SoundProfile.UnequipVolume;
                    m_SecondaryAudioSource.volume = SoundProfile.UnequipVolume;

                    if (!m_AudioSource.isPlaying)
                    {
                        m_AudioSource.PlayOneShot(SoundProfile.SheatheWeapon);
                    }
                    else if (!m_SecondaryAudioSource.isPlaying)
                    {
                        m_SecondaryAudioSource.PlayOneShot(SoundProfile.SheatheWeapon);
                    }
                    else
                    {
                        m_EventAudioSource.PlayOneShot(SoundProfile.SheatheWeapon);
                    }
                }
            }
            else if (WeaponType == "Weapon Type 2")
            {
                if (SoundProfile.RangedSheatheWeapon != null)
                {
                    m_AudioSource.volume = SoundProfile.RangedUnequipVolume;
                    m_SecondaryAudioSource.volume = SoundProfile.RangedUnequipVolume;

                    if (!m_AudioSource.isPlaying)
                    {
                        m_AudioSource.PlayOneShot(SoundProfile.RangedSheatheWeapon);
                    }
                    else if (!m_SecondaryAudioSource.isPlaying)
                    {
                        m_SecondaryAudioSource.PlayOneShot(SoundProfile.RangedSheatheWeapon);
                    }
                    else
                    {
                        m_EventAudioSource.PlayOneShot(SoundProfile.RangedSheatheWeapon);
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）警告（Warning）サウンドをランダム再生します。
        /// </summary>
        public void PlayWarningSound()
        {
            if (SoundProfile.WarningSounds.Count > 0)
            {
                if (!m_AudioSource.isPlaying)
                {
                    m_AudioSource.volume = SoundProfile.WarningVolume;
                    m_AudioSource.PlayOneShot(SoundProfile.WarningSounds[Random.Range(0, SoundProfile.WarningSounds.Count)]);
                }
                else if (!m_SecondaryAudioSource.isPlaying)
                {
                    m_SecondaryAudioSource.volume = SoundProfile.WarningVolume;
                    m_SecondaryAudioSource.PlayOneShot(SoundProfile.WarningSounds[Random.Range(0, SoundProfile.WarningSounds.Count)]);
                }
                else
                {
                    m_EventAudioSource.volume = SoundProfile.WarningVolume;
                    m_EventAudioSource.PlayOneShot(SoundProfile.WarningSounds[Random.Range(0, SoundProfile.WarningSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// （日本語）ブロック時のサウンドをランダム再生します。ピッチに少し揺らぎを与えます。
        /// </summary>
        public void PlayBlockSound()
        {
            if (SoundProfile.BlockingSounds.Count > 0)
            {
                if (!m_AudioSource.isPlaying)
                {
                    m_AudioSource.volume = SoundProfile.BlockVolume;
                    m_AudioSource.pitch = Mathf.Round(Random.Range(0.7f, 1.1f) * 10) / 10;
                    m_AudioSource.PlayOneShot(SoundProfile.BlockingSounds[Random.Range(0, SoundProfile.BlockingSounds.Count)]);
                }
                else if (!m_SecondaryAudioSource.isPlaying)
                {
                    m_SecondaryAudioSource.volume = SoundProfile.BlockVolume;
                    m_SecondaryAudioSource.pitch = Mathf.Round(Random.Range(0.7f, 1.1f) * 10) / 10;
                    m_SecondaryAudioSource.PlayOneShot(SoundProfile.BlockingSounds[Random.Range(0, SoundProfile.BlockingSounds.Count)]);
                }
                else
                {
                    m_EventAudioSource.volume = SoundProfile.BlockVolume;
                    m_EventAudioSource.pitch = Mathf.Round(Random.Range(0.7f, 1.1f) * 10) / 10;
                    m_EventAudioSource.PlayOneShot(SoundProfile.BlockingSounds[Random.Range(0, SoundProfile.BlockingSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// （日本語）負傷サウンドをランダム再生します。確率（InjuredSoundOdds）とブロック状態を考慮します。
        /// </summary>
        public void PlayInjuredSound()
        {
            int Odds = Random.Range(1, 101);                     // 1〜100
            if (Odds > SoundProfile.InjuredSoundOdds) return;    // 確率判定

            if (SoundProfile.InjuredSounds.Count > 0 && !EmeraldComponent.AnimationComponent.IsBlocking)
            {
                if (!m_AudioSource.isPlaying)
                {
                    m_AudioSource.volume = SoundProfile.InjuredVolume;
                    m_AudioSource.pitch = Mathf.Round(Random.Range(0.8f, 1.1f) * 10) / 10;
                    m_AudioSource.PlayOneShot(SoundProfile.InjuredSounds[Random.Range(0, SoundProfile.InjuredSounds.Count)]);
                }
                else if (!m_SecondaryAudioSource.isPlaying)
                {
                    m_SecondaryAudioSource.volume = SoundProfile.InjuredVolume;
                    m_SecondaryAudioSource.PlayOneShot(SoundProfile.InjuredSounds[Random.Range(0, SoundProfile.InjuredSounds.Count)]);
                }
                else
                {
                    m_EventAudioSource.volume = SoundProfile.InjuredVolume;
                    m_EventAudioSource.PlayOneShot(SoundProfile.InjuredSounds[Random.Range(0, SoundProfile.InjuredSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// （日本語）死亡サウンドをランダム再生します（アニメーションイベントからも呼出可能）。
        /// </summary>
        public void PlayDeathSound()
        {
            if (SoundProfile.DeathSounds.Count > 0)
            {
                if (!m_AudioSource.isPlaying)
                {
                    m_AudioSource.volume = SoundProfile.DeathVolume;
                    m_AudioSource.PlayOneShot(SoundProfile.DeathSounds[Random.Range(0, SoundProfile.DeathSounds.Count)]);
                }
                else if (!m_SecondaryAudioSource.isPlaying)
                {
                    m_SecondaryAudioSource.volume = SoundProfile.DeathVolume;
                    m_SecondaryAudioSource.PlayOneShot(SoundProfile.DeathSounds[Random.Range(0, SoundProfile.DeathSounds.Count)]);
                }
                else
                {
                    m_EventAudioSource.volume = SoundProfile.DeathVolume;
                    m_EventAudioSource.PlayOneShot(SoundProfile.DeathSounds[Random.Range(0, SoundProfile.DeathSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// （日本語）Footsteps コンポーネント未使用時に、足音を再生します。
        /// ・歩き/走りの状態から音量を切り替え
        /// ・空いている AudioSource を選択して再生
        /// </summary>
        public void Footstep()
        {
            if (GetComponent<EmeraldFootsteps>() != null) return; // Footsteps コンポーネントがある場合はそちらで処理

            if (EmeraldComponent.MovementComponent.CanPlayWalkFootstepSound() || EmeraldComponent.MovementComponent.CanPlayRunFootstepSound())
            {
                float StepVolume = EmeraldComponent.MovementComponent.CanPlayWalkFootstepSound() ? SoundProfile.WalkFootstepVolume : SoundProfile.RunFootstepVolume;

                if (SoundProfile.FootStepSounds.Count > 0)
                {
                    if (!m_AudioSource.isPlaying)
                    {
                        m_AudioSource.volume = StepVolume;
                        m_AudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                    else if (!m_SecondaryAudioSource.isPlaying)
                    {
                        m_SecondaryAudioSource.volume = StepVolume;
                        m_SecondaryAudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                    else
                    {
                        m_EventAudioSource.volume = StepVolume;
                        m_EventAudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）歩行用の足音を再生します（アニメーションイベント経由で呼び出す想定）。
        /// Footsteps コンポーネントが存在する場合は、こちらでは処理を行いません。
        /// </summary>
        public void WalkFootstepSound()
        {
            if (GetComponent<EmeraldFootsteps>() != null) return;

            if (EmeraldComponent.MovementComponent.CanPlayWalkFootstepSound())
            {
                if (SoundProfile.FootStepSounds.Count > 0)
                {
                    if (!m_AudioSource.isPlaying)
                    {
                        m_AudioSource.volume = SoundProfile.WalkFootstepVolume;
                        m_AudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                    else if (!m_SecondaryAudioSource.isPlaying)
                    {
                        m_SecondaryAudioSource.volume = SoundProfile.WalkFootstepVolume;
                        m_SecondaryAudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                    else
                    {
                        m_EventAudioSource.volume = SoundProfile.WalkFootstepVolume;
                        m_EventAudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）走行用の足音を再生します（アニメーションイベント経由で呼び出す想定）。
        /// Footsteps コンポーネントが存在する場合は、こちらでは処理を行いません。
        /// </summary>
        public void RunFootstepSound()
        {
            if (GetComponent<EmeraldFootsteps>() != null) return;

            if (EmeraldComponent.MovementComponent.CanPlayRunFootstepSound())
            {
                if (SoundProfile.FootStepSounds.Count > 0)
                {
                    if (!m_AudioSource.isPlaying)
                    {
                        m_AudioSource.volume = SoundProfile.RunFootstepVolume;
                        m_AudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                    else if (!m_SecondaryAudioSource.isPlaying)
                    {
                        m_SecondaryAudioSource.volume = SoundProfile.RunFootstepVolume;
                        m_SecondaryAudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                    else
                    {
                        m_EventAudioSource.volume = SoundProfile.RunFootstepVolume;
                        m_EventAudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）汎用 SE リスト（InteractSounds）からランダムに 1 つ再生します。
        /// </summary>
        public void PlayRandomSoundEffect()
        {
            if (SoundProfile.InteractSounds.Count > 0)
            {
                if (!m_AudioSource.isPlaying)
                {
                    m_AudioSource.volume = 1;
                    m_AudioSource.PlayOneShot(SoundProfile.InteractSounds[Random.Range(0, SoundProfile.InteractSounds.Count)].SoundEffectClip);
                }
                else if (!m_SecondaryAudioSource.isPlaying)
                {
                    m_SecondaryAudioSource.volume = 1;
                    m_SecondaryAudioSource.PlayOneShot(SoundProfile.InteractSounds[Random.Range(0, SoundProfile.InteractSounds.Count)].SoundEffectClip);
                }
                else
                {
                    m_EventAudioSource.volume = 1;
                    m_EventAudioSource.PlayOneShot(SoundProfile.InteractSounds[Random.Range(0, SoundProfile.InteractSounds.Count)].SoundEffectClip);
                }
            }
        }

        /// <summary>
        /// （日本語）汎用 SE リスト（InteractSounds）から ID 指定で再生します。
        /// </summary>
        public void PlaySoundEffect(int SoundEffectID)
        {
            if (SoundProfile.InteractSounds.Count > 0)
            {
                for (int i = 0; i < SoundProfile.InteractSounds.Count; i++)
                {
                    if (SoundProfile.InteractSounds[i].SoundEffectID == SoundEffectID)
                    {
                        if (!m_AudioSource.isPlaying)
                        {
                            m_AudioSource.volume = 1;
                            m_AudioSource.PlayOneShot(SoundProfile.InteractSounds[i].SoundEffectClip);
                        }
                        else if (!m_SecondaryAudioSource.isPlaying)
                        {
                            m_SecondaryAudioSource.volume = 1;
                            m_SecondaryAudioSource.PlayOneShot(SoundProfile.InteractSounds[i].SoundEffectClip);
                        }
                        else
                        {
                            m_EventAudioSource.volume = 1;
                            m_EventAudioSource.PlayOneShot(SoundProfile.InteractSounds[i].SoundEffectClip);
                        }
                    }
                }
            }
        }
    }
}
