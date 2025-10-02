using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI;

/// <summary>
/// 【TestPlayerBridge】
/// EmeraldPlayerBridge を継承したテスト用プレイヤーブリッジ。
/// 各メソッド内のコメントに沿って、実際のキャラクターコントローラ連携コードを実装してください。
/// </summary>
public class TestPlayerBridge : EmeraldPlayerBridge
{
    public override void Start()
    {
        // ここで、プレイヤーのキャラクターコントローラの体力値と同じになるように
        // StartHealth と Health を設定してください。
    }

    public override void DamageCharacterController(int DamageAmount, Transform Target)
    {
        // プレイヤーのキャラクターコントローラへダメージを与える処理を
        // ここに実装してください。
    }

    public override bool IsAttacking()
    {
        // このターゲットが攻撃中かどうかを検出するために使用します。
        return false;
    }

    public override bool IsBlocking()
    {
        // このターゲットが防御中かどうかを検出するために使用します。
        return false;
    }

    public override bool IsDodging()
    {
        // このターゲットが回避中かどうかを検出するために使用します。
        return false;
    }

    public override void TriggerStun(float StunLength)
    {
        // カスタムのスタン付与処理をここに実装できます（必須ではありません）。
    }
}
