using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 【FootstepSurfaceObject】
/// 足音の検知方法（タグ／地形テクスチャ）と、再生する効果音・エフェクト・足跡などをまとめて管理する ScriptableObject。
/// インスペクターでの可読性向上のため、各メンバーに日本語の [Header] を付与しています。
/// </summary>
[CreateAssetMenu(fileName = "フットステップ サーフェス オブジェクト", menuName = "Emerald AI/フットステップ/サーフェスオブジェクト")]
public class FootstepSurfaceObject : ScriptableObject
{
    [Header("インスペクター: 設定を隠す（エディタ用フラグ）")]
    public bool HideSettingsFoldout;

    [Header("インスペクター: サーフェス設定の折りたたみ（エディタ用フラグ）")]
    public bool SurfaceSettingsFoldout;

    public enum SurfaceTypes { Tag = 1, Texture = 2 };

    [Header("サーフェスタイプ（タグ または テクスチャ）")]
    [Tooltip("フットステップ（足音）情報をどの方法で取得するかを制御します。")]
    public SurfaceTypes SurfaceType = SurfaceTypes.Tag;

    [Space(5)]
    [Header("検知対象の Terrain テクスチャ一覧")]
    [Tooltip("このフットステップサーフェスで検知すべき地形テクスチャ。\n\n注意：Unity の Terrain からのみ取得されます。")]
    public List<Texture> SurfaceTextures = new List<Texture>();

    [Space(5)]
    [Header("検知対象のタグ（Unity Terrain を除く任意の GameObject）")]
    [Tooltip("このフットステップサーフェスで検知すべきタグ。\n\n注意：Unity Terrain 以外の任意の GameObject に適用できます。")]
    [Tag][SerializeField] public string SurfaceTag = "Untagged";

    [Space(10)]
    [Header("足音の音量（0〜1）")]
    [Tooltip("このフットステップサーフェスで再生される足音の音量。")]
    [Range(0, 1)] public float StepVolume = 1;

    [Space(10)]
    [Header("足音のオーディオクリップ一覧（ランダム再生）")]
    [Tooltip("このフットステップサーフェスで使用される足音効果音のリスト。ランダムに再生されます。")]
    public List<AudioClip> StepSounds = new List<AudioClip>();

    [Space(10)]
    [Header("ステップエフェクトの消滅までの時間（秒）")]
    [Tooltip("ステップエフェクトが消滅するまでの時間（秒）を制御します。")]
    [Range(0.5f, 6)] public float StepEffectTimeout = 2;

    [Header("ステップエフェクトのプレハブ一覧（ランダム選択・任意）")]
    [Tooltip("このフットステップサーフェスで使用されるステップエフェクトのリスト。ランダムに選ばれます。\n\n注意：ステップエフェクトを使用しない場合は空のままで構いません。")]
    public List<GameObject> StepEffects = new List<GameObject>();

    [Space(10)]
    [Header("足跡の消滅までの時間（秒）")]
    [Tooltip("フットプリント（足跡）が消滅するまでの時間（秒）を制御します。")]
    [Range(1f, 30)] public float FootprintTimeout = 10;

    [Header("足跡プレハブ一覧（ランダム選択・任意）")]
    [Tooltip("このフットステップサーフェスで使用されるフットプリント（足跡）プレハブのリスト。ランダムに選ばれ、検知した地表へ位置合わせされます。\n\n注意：足跡を使用しない場合は空のままで構いません。")]
    public List<GameObject> Footprints = new List<GameObject>();
}
