using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【EmeraldSoundProfile】
    /// AI の待機・足音・攻撃・被弾・死亡・警告など、各種サウンドと音量をまとめて管理する ScriptableObject。
    /// インスペクターの可読性向上のため、全メンバーに [Header] を付与しています。
    /// </summary>
    [CreateAssetMenu(fileName = "サウンドプロファイル", menuName = "Emerald AI/サウンド/サウンドプロファイル")]
    public class EmeraldSoundProfile : ScriptableObject
    {
        [Header("待機サウンドの折りたたみ表示フラグ（エディタ用）")]
        public bool IdleSoundsFoldout;

        [Header("足音サウンドの折りたたみ表示フラグ（エディタ用）")]
        public bool FootstepSoundsFoldout;

        [Header("インタラクトサウンドの折りたたみ表示フラグ（エディタ用）")]
        public bool InteractSoundsFoldout;

        [Header("装備/納刀サウンドの折りたたみ表示フラグ（エディタ用）")]
        public bool EquipAndUnequipSoundsFoldout;

        [Header("攻撃サウンドの折りたたみ表示フラグ（エディタ用）")]
        public bool AttackSoundsFoldout;

        [Header("被弾サウンドの折りたたみ表示フラグ（エディタ用）")]
        public bool InjuredSoundsFoldout;

        [Header("ブロックサウンドの折りたたみ表示フラグ（エディタ用）")]
        public bool BlockSoundsFoldout;

        [Header("死亡サウンドの折りたたみ表示フラグ（エディタ用）")]
        public bool DeathSoundsFoldout;

        [Header("警告サウンドの折りたたみ表示フラグ（エディタ用）")]
        public bool WarningSoundsFoldout;

        [Header("次の待機サウンド再生までの残り秒数（内部タイマー秒）")]
        public int IdleSoundsSeconds;

        [Header("待機サウンドの再生間隔：最小秒数")]
        public int IdleSoundsSecondsMin = 5;

        [Header("待機サウンドの再生間隔：最大秒数")]
        public int IdleSoundsSecondsMax = 10;

        [Header("待機サウンド用の経過タイマー（秒）")]
        public float IdleSoundsTimer;

        [Header("近接武器を納刀する時のサウンドクリップ")]
        public AudioClip SheatheWeapon;

        [Header("近接武器を抜刀する時のサウンドクリップ")]
        public AudioClip UnsheatheWeapon;

        [Header("遠隔武器を納刀する時のサウンドクリップ")]
        public AudioClip RangedSheatheWeapon;

        [Header("遠隔武器を装備（抜刀）する時のサウンドクリップ")]
        public AudioClip RangedUnsheatheWeapon;

        [Header("待機（Idle）時にランダム再生するサウンド一覧")]
        public List<AudioClip> IdleSounds = new List<AudioClip>();

        [Header("攻撃時にランダム再生するサウンド一覧")]
        public List<AudioClip> AttackSounds = new List<AudioClip>();

        [Header("被弾時にランダム再生するサウンド一覧")]
        public List<AudioClip> InjuredSounds = new List<AudioClip>();

        [Header("警告（Warning）時にランダム再生するサウンド一覧")]
        public List<AudioClip> WarningSounds = new List<AudioClip>();

        [Header("死亡時にランダム再生するサウンド一覧")]
        public List<AudioClip> DeathSounds = new List<AudioClip>();

        [Header("足音に使用するサウンド一覧（歩行/走行で音量調整）")]
        public List<AudioClip> FootStepSounds = new List<AudioClip>();

        [Header("ブロック成功時に再生するサウンド一覧")]
        public List<AudioClip> BlockingSounds = new List<AudioClip>();

        [Header("待機サウンドの音量（0〜1）")]
        public float IdleVolume = 1;

        [Header("歩行時の足音ボリューム（0〜1）")]
        public float WalkFootstepVolume = 0.1f;

        [Header("走行時の足音ボリューム（0〜1）")]
        public float RunFootstepVolume = 0.1f;

        [Header("ブロックサウンドの音量（0〜1）")]
        public float BlockVolume = 0.65f;

        [Header("被弾サウンドが再生される確率（%）")]
        public int InjuredSoundOdds = 100;

        [Header("被弾サウンドの音量（0〜1）")]
        public float InjuredVolume = 1;

        [Header("攻撃サウンドの音量（0〜1）")]
        public float AttackVolume = 1;

        [Header("警告サウンドの音量（0〜1）")]
        public float WarningVolume = 1;

        [Header("死亡サウンドの音量（0〜1）")]
        public float DeathVolume = 0.7f;

        [Header("（近接）装備サウンドの音量（0〜1）")]
        public float EquipVolume = 1;

        [Header("（近接）納刀サウンドの音量（0〜1）")]
        public float UnequipVolume = 1;

        [Header("（遠隔）装備サウンドの音量（0〜1）")]
        public float RangedEquipVolume = 1;

        [Header("（遠隔）納刀サウンドの音量（0〜1）")]
        public float RangedUnequipVolume = 1;

        [Header("インタラクト時に再生するサウンドの一覧（ID で識別）")]
        [SerializeField]
        public List<InteractSoundClass> InteractSounds = new List<InteractSoundClass>();

        [System.Serializable]
        public class InteractSoundClass
        {
            [Header("サウンドエフェクトのID（任意の識別子）")]
            public int SoundEffectID = 1;

            [Header("サウンドエフェクトのオーディオクリップ")]
            public AudioClip SoundEffectClip;
        }
    }
}
