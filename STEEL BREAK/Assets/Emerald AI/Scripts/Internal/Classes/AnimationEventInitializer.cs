using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【AnimationEventInitializer】
    /// アニメーションプレビューエディタに、あらかじめ用意されたアニメーションイベントのプリセットを登録します。
    /// </summary>
    public static class AnimationEventInitializer
    {
        /// <summary>
        /// Emerald AI 用のアニメーションイベント一覧（プリセット）を生成して返します。
        /// </summary>
        public static List<EmeraldAnimationEventsClass> GetEmeraldAnimationEvents()
        {
            List<EmeraldAnimationEventsClass> EmeraldAnimationEvents = new List<EmeraldAnimationEventsClass>();

            // Custom（カスタム）
            AnimationEvent Custom = new AnimationEvent();
            Custom.functionName = "---YOUR FUNCTION NAME HERE---"; // 呼び出したい関数名を記入（例：OnMyCustomEvent）
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "カスタム",
                    Custom,
                    "追加パラメータのないカスタム／デフォルトのイベント。任意の関数名を指定して使用します。"
                )
            );

            // Emerald Attack Event（アビリティ生成）
            AnimationEvent EmeraldAttack = new AnimationEvent();
            EmeraldAttack.functionName = "CreateAbility"; // 関数名は仕様どおり保持
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "アビリティ生成",
                    EmeraldAttack,
                    "AI の『現在のアビリティ』を生成するイベント（旧称: EmeraldAttackEvent）。アビリティオブジェクトを起動するために必要で、すべての攻撃アニメーションに設定してください。\n\n" +
                    "注意：AI が Attack Transform を利用している場合、このイベントの String パラメータに Attack Transform 名を記入してください。これによりアビリティはその Transform の位置から生成されます。"
                )
            );

            // Charge Ability（チャージ・エフェクト）
            AnimationEvent ChargeEffect = new AnimationEvent();
            ChargeEffect.functionName = "ChargeEffect"; // 関数名は仕様どおり保持
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "チャージ・エフェクト",
                    ChargeEffect,
                    "AI の現在アビリティにおける『チャージ用エフェクト』を起動します。どの Attack Transform で生成するかは String パラメータで指定します（Combat コンポーネントの Attack Transform リストに基づく）。" +
                    "アビリティオブジェクト側にチャージモジュールが存在し、有効化されている必要があります。無効な場合、このイベントはスキップされます。\n\n" +
                    "注意：このイベントはアビリティ自体は生成しません。『アビリティ生成（CreateAbility）』イベントを、通常この後に割り当ててください。このアニメーションイベントは任意です。"
                )
            );

            // Fade Out IK（IK をフェードアウト）
            AnimationEvent FadeOutIK = new AnimationEvent();
            FadeOutIK.functionName = "FadeOutIK"; // 関数名は仕様どおり保持
            FadeOutIK.floatParameter = 5f;
            FadeOutIK.stringParameter = "---フェードさせたいリグ名をここに---";
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "IK をフェードアウト",
                    FadeOutIK,
                    "AI の IK を時間経過で徐々に無効化します。被弾、装備、特定の攻撃、死亡アニメーションなどで IK が干渉する場合に有用です。\n\n" +
                    "FloatParameter = フェードアウト時間（秒）\n\n" +
                    "StringParameter = フェード対象の Rig 名"
                )
            );

            // Fade In IK（IK をフェードイン）
            AnimationEvent FadeInIK = new AnimationEvent();
            FadeInIK.functionName = "FadeInIK"; // 関数名は仕様どおり保持
            FadeInIK.floatParameter = 5f;
            FadeInIK.stringParameter = "---フェードさせたいリグ名をここに---";
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "IK をフェードイン",
                    FadeInIK,
                    "AI の IK を時間経過で徐々に有効化します。『IK をフェードアウト』を使用した後に使用してください。\n\n" +
                    "FloatParameter = フェードイン時間（秒）\n\n" +
                    "StringParameter = フェード対象の Rig 名"
                )
            );

            // Enable Weapon Collider（武器コライダーを有効化）
            AnimationEvent EnableWeaponCollider = new AnimationEvent();
            EnableWeaponCollider.functionName = "EnableWeaponCollider"; // 関数名は仕様どおり保持
            EnableWeaponCollider.stringParameter = "---AI の武器名をここに入力---";
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "武器コライダーを有効化",
                    EnableWeaponCollider,
                    "AI の武器オブジェクトに付与されたコライダーを有効化します（武器には WeaponCollider コンポーネントが必要で、EmeraldItems コンポーネントで設定済みであること）。\n\n" +
                    "注意：このアニメーションイベントの String パラメータに、AI の武器オブジェクト名を設定してください。Items コンポーネント内から、その名称で該当武器を検索します。詳しくは Emerald AI Wiki を参照してください。"
                )
            );

            // Disable Weapon Collider（武器コライダーを無効化）
            AnimationEvent DisableWeaponCollider = new AnimationEvent();
            DisableWeaponCollider.functionName = "DisableWeaponCollider"; // 関数名は仕様どおり保持
            DisableWeaponCollider.stringParameter = "---AI の武器名をここに入力---";
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "武器コライダーを無効化",
                    DisableWeaponCollider,
                    "AI の武器オブジェクトに付与されたコライダーを無効化します（武器には WeaponCollider コンポーネントが必要で、EmeraldItems コンポーネントで設定済みであること）。\n\n" +
                    "注意：このアニメーションイベントの String パラメータに、AI の武器オブジェクト名を設定してください。Items コンポーネント内から、その名称で該当武器を検索します。詳しくは Emerald AI Wiki を参照してください。"
                )
            );

            // Equip Weapon 1（武器タイプ1を装備）
            AnimationEvent EquipWeapon1 = new AnimationEvent();
            EquipWeapon1.functionName = "EquipWeapon"; // 関数名は仕様どおり保持
            EquipWeapon1.stringParameter = "Weapon Type 1"; // 仕様上の識別子の可能性があるため原文を維持
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "武器タイプ1を装備",
                    EquipWeapon1,
                    "AI の『Weapon Type 1』を装備します（武器は Emerald AI 上でセットアップされている必要があります）。"
                )
            );

            // Equip Weapon 2（武器タイプ2を装備）
            AnimationEvent EquipWeapon2 = new AnimationEvent();
            EquipWeapon2.functionName = "EquipWeapon"; // 関数名は仕様どおり保持
            EquipWeapon2.stringParameter = "Weapon Type 2"; // 仕様上の識別子の可能性があるため原文を維持
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "武器タイプ2を装備",
                    EquipWeapon2,
                    "AI の『Weapon Type 2』を装備します（武器は Emerald AI 上でセットアップされている必要があります）。"
                )
            );

            // Unequip Weapon 1（武器タイプ1を外す）
            AnimationEvent UnequipWeapon1 = new AnimationEvent();
            UnequipWeapon1.functionName = "UnequipWeapon"; // 関数名は仕様どおり保持
            UnequipWeapon1.stringParameter = "Weapon Type 1"; // 仕様上の識別子の可能性があるため原文を維持
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "武器タイプ1を外す",
                    UnequipWeapon1,
                    "AI の『Weapon Type 1』を外します（武器は Emerald AI 上でセットアップされている必要があります）。"
                )
            );

            // Unequip Weapon 2（武器タイプ2を外す）
            AnimationEvent UnequipWeapon2 = new AnimationEvent();
            UnequipWeapon2.functionName = "UnequipWeapon"; // 関数名は仕様どおり保持
            UnequipWeapon2.stringParameter = "Weapon Type 2"; // 仕様上の識別子の可能性があるため原文を維持
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "武器タイプ2を外す",
                    UnequipWeapon2,
                    "AI の『Weapon Type 2』を外します（武器は Emerald AI 上でセットアップされている必要があります）。"
                )
            );

            // Enable Item（アイテムを有効化）
            AnimationEvent EnableItem = new AnimationEvent();
            EnableItem.functionName = "EnableItem"; // 関数名は仕様どおり保持
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "アイテムを有効化",
                    EnableItem,
                    "ItemID を指定してアイテムを有効化します。AI の Item List に基づき、AI には EmeraldAIItem コンポーネントが必要です。\n\nIntParameter = ItemID"
                )
            );

            // Disable Item（アイテムを無効化）
            AnimationEvent DisableItem = new AnimationEvent();
            DisableItem.functionName = "DisableItem"; // 関数名は仕様どおり保持
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "アイテムを無効化",
                    DisableItem,
                    "ItemID を指定してアイテムを無効化します。AI の Item List に基づき、AI には EmeraldAIItem コンポーネントが必要です。\n\nIntParameter = ItemID"
                )
            );

            // Footstep Sound（フットステップ）
            AnimationEvent Footstep = new AnimationEvent();
            Footstep.functionName = "Footstep"; // 関数名は仕様どおり保持
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "フットステップ",
                    Footstep,
                    "Footstep コンポーネントを使用している場合：\n検出された接地面に応じて、足音エフェクトとサウンドを生成します（事前に Footstep コンポーネントのセットアップが必要）。\n\n" +
                    "Footstep コンポーネントを使用していない場合：\nAI の Walk Sound List に基づき、ランダムな足音を再生します。"
                )
            );

            // Play Attack Sound（攻撃音を再生）
            AnimationEvent PlayAttackSound = new AnimationEvent();
            PlayAttackSound.functionName = "PlayAttackSound"; // 関数名は仕様どおり保持
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "攻撃音を再生",
                    PlayAttackSound,
                    "AI の Attack Sound List に基づき、ランダムな攻撃サウンドを再生します。"
                )
            );

            // Play Sound Effect（効果音を再生）
            AnimationEvent PlaySoundEffect = new AnimationEvent();
            PlaySoundEffect.functionName = "PlaySoundEffect"; // 関数名は仕様どおり保持
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "効果音を再生",
                    PlaySoundEffect,
                    "AI の Sounds List から、指定した SoundEffectID のサウンドを再生します。\n\nIntParameter = SoundEffectID"
                )
            );

            // Play Warning Sound（警告音を再生）
            AnimationEvent PlayWarningSound = new AnimationEvent();
            PlayWarningSound.functionName = "PlayWarningSound"; // 関数名は仕様どおり保持
            EmeraldAnimationEvents.Add(
                new EmeraldAnimationEventsClass(
                    "警告音を再生",
                    PlayWarningSound,
                    "AI の Warning Sound List に基づき、ランダムな警告サウンドを再生します。"
                )
            );

            return EmeraldAnimationEvents;
        }
    }
}
