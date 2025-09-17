using System.Collections.Generic;                       // List<T> 等、汎用コレクションを使用
using UnityEngine;                                      // Unity の基本API
using EmeraldAI.Utility;                                // Emerald のユーティリティ（ObjectPool など）

namespace EmeraldAI                                     // EmeraldAI の名前空間
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/items-component")] // 公式WikiへのヘルプURL

    // 【クラス概要】EmeraldItems：
    // AIが装備する武器や所持アイテムの有効/無効切替、死亡時のドロップ武器生成、
    // 装備・収納（ホルスター）の表示切替、武器コライダー有効/無効化などを制御するコンポーネント。
    public class EmeraldItems : MonoBehaviour           // MonoBehaviour を継承したアイテム管理クラス
    {
        #region Items Variables                         // —— アイテム／武器関連の変数群 ——

        [Header("死亡時に武器をドロップ可能にするか（Yes=ドロップする）")]
        public YesOrNo UseDroppableWeapon = YesOrNo.No; // ドロップ武器機能の有効/無効

        [Header("Weapon Type 1 の装備定義リスト（Held/Holstered/Droppable の各オブジェクト設定）")]
        [SerializeField]                                 // インスペクタ表示用にシリアライズ
        public List<EquippableWeapons> Type1EquippableWeapons = new List<EquippableWeapons>(); // タイプ1武器の設定配列

        [Header("Weapon Type 2 の装備定義リスト（Held/Holstered/Droppable の各オブジェクト設定）")]
        [SerializeField]                                 // インスペクタ表示用にシリアライズ
        public List<EquippableWeapons> Type2EquippableWeapons = new List<EquippableWeapons>(); // タイプ2武器の設定配列

        [System.Serializable]                            // インスペクタから編集可能にする
        public class EquippableWeapons                   // 武器1件分の装備定義
        {
            [Header("手に持つ（Held）武器を使用するか（On/Off）")]
            public bool HeldToggle;                      // 手持ち武器の使用フラグ

            [Header("手に持つ（Held）武器の GameObject 参照")]
            public GameObject HeldObject;                // 手持ち武器の実体

            [Header("ホルスター（収納）武器を使用するか（On/Off）")]
            public bool HolsteredToggle;                 // ホルスター武器の使用フラグ

            [Header("ホルスター（収納）武器の GameObject 参照")]
            public GameObject HolsteredObject;           // 収納時に表示する武器

            [Header("死亡時ドロップ用に使用するか（On/Off）")]
            public bool DroppableToggle;                 // ドロップ武器の使用フラグ

            [Header("死亡時に生成するドロップ武器のプレハブ参照")]
            public GameObject DroppableObject;           // ドロップ生成する武器のPrefab
        }

        // —— 装備/非装備通知用のデリゲート/イベント（※属性は付けない：フィールドではないため）——
        public delegate void OnEquipWeaponHandler(string WeaponType); // 装備時コールバックの型
        public event OnEquipWeaponHandler OnEquipWeapon;             // 装備時に通知されるイベント

        public delegate void OnUnequipWeaponHandler(string WeaponType); // 非装備時コールバックの型
        public event OnUnequipWeaponHandler OnUnequipWeapon;            // 非装備時に通知されるイベント

        [System.Serializable]                            // インスペクタで編集可能
        public class ItemClass                           // 任意アイテム（ID紐付けで有効化/無効化）
        {
            [Header("アイテムID（有効/無効の切替に使用）")]
            public int ItemID = 1;                       // 識別子（重複に注意）

            [Header("対象アイテムの GameObject 参照")]
            public GameObject ItemObject;                // 実体オブジェクト
        }

        [Header("AIが所持するアイテム一覧（IDで有効/無効化する）")]
        [SerializeField]                                 // インスペクタ表示用
        public List<ItemClass> ItemList = new List<ItemClass>(); // 登録済みアイテム群

        [Header("（内部参照）EmeraldSystem への参照")]
        EmeraldSystem EmeraldComponent;                  // 実行時に取得するAI本体参照
        #endregion

        #region Editor Variables                         // —— エディタ表示制御用の変数 ——

        [Header("【Editor表示】設定群を隠す（折りたたみ制御）")]
        public bool HideSettingsFoldout;                 // インスペクタで非表示にするか

        [Header("【Editor表示】Weapons セクションの折りたたみ状態")]
        public bool WeaponsFoldout;                      // 武器セクションの開閉

        [Header("【Editor表示】Items セクションの折りたたみ状態")]
        public bool ItemsFoldout;                        // アイテムセクションの開閉
        #endregion

        void Start()                                     // Unity ライフサイクル：開始時
        {
            InitializeDroppableWeapon();                 // ドロップ武器の初期化（死亡時ハンドラ登録）
        }

        /// <summary>
        /// （日本語）AIの死亡時にドロップさせる武器の初期化。
        /// OnDeath イベントへドロップ生成処理を購読する。
        /// </summary>
        public void InitializeDroppableWeapon()          // ドロップ機能のセットアップ
        {
            GetComponent<EmeraldHealth>().OnDeath += CreateDroppableWeapon; // 死亡時イベントへ購読（ドロップ生成）
        }

        /// <summary>
        /// （日本語）AIの死亡時に、設定されたドロップ武器プレハブを生成する。
        /// </summary>
        public void CreateDroppableWeapon()              // 実際のドロップ生成
        {
            EmeraldSystem EmeraldComponent = GetComponent<EmeraldSystem>(); // AI本体参照を取得

            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1) // 現在が武器タイプ1なら
            {
                for (int i = 0; i < Type1EquippableWeapons.Count; i++) // 全タイプ1定義を走査
                {
                    if (Type1EquippableWeapons[i].DroppableObject != null && Type1EquippableWeapons[i].DroppableToggle) // ドロップ有効か
                    {
                        if (Type1EquippableWeapons[i].HeldObject != null) // 手持ち武器の参照があるか
                        {
                            // DroppableObject の複製を生成し、手持ち武器の位置・回転に合わせる
                            var DroppableMeleeWeapon = EmeraldObjectPool.Spawn(Type1EquippableWeapons[i].DroppableObject, Type1EquippableWeapons[i].HeldObject.transform.position, Type1EquippableWeapons[i].HeldObject.transform.rotation);
                            DroppableMeleeWeapon.transform.localScale = Type1EquippableWeapons[i].HeldObject.transform.lossyScale;          // スケールを合わせる
                            DroppableMeleeWeapon.transform.SetParent(Type1EquippableWeapons[i].HeldObject.transform.parent);               // 一旦同じ親へ
                            DroppableMeleeWeapon.transform.localPosition = Type1EquippableWeapons[i].HeldObject.transform.localPosition;   // ローカル位置も合わせる
                            DroppableMeleeWeapon.gameObject.name = Type1EquippableWeapons[i].HeldObject.gameObject.name + " (Droppable Copy)"; // 名前付け
                            DroppableMeleeWeapon.transform.SetParent(EmeraldSystem.ObjectPool.transform);                                   // ObjectPool 配下へ移動

                            // 衝突用Colliderが無ければ追加
                            if (DroppableMeleeWeapon.GetComponent<Collider>() == null)
                                DroppableMeleeWeapon.AddComponent<BoxCollider>();

                            // 物理挙動用Rigidbodyが無ければ追加
                            if (DroppableMeleeWeapon.GetComponent<Rigidbody>() == null)
                                DroppableMeleeWeapon.AddComponent<Rigidbody>();

                            // 物理補間を有効化（見た目の滑らかさ）
                            Rigidbody WeaponRigidbody = DroppableMeleeWeapon.GetComponent<Rigidbody>();
                            WeaponRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                            // 最後にAIへダメージを与えた相手方向へ、受けたラグドール力を反映した反動を加える
                            Transform LastAttacker = EmeraldComponent.CombatComponent.LastAttacker;

                            if (EmeraldComponent.CombatTarget != null) // 戦闘相手がいる場合
                            {
                                if (LastAttacker != null && LastAttacker != EmeraldComponent.CombatTarget)
                                    WeaponRigidbody.AddForce((LastAttacker.position - transform.position).normalized * EmeraldComponent.CombatComponent.ReceivedRagdollForceAmount * 0.01f, ForceMode.Impulse); // 最終攻撃者方向
                                else
                                    WeaponRigidbody.AddForce((EmeraldComponent.CombatTarget.position - transform.position).normalized * EmeraldComponent.CombatComponent.ReceivedRagdollForceAmount * 0.01f, ForceMode.Impulse); // 現在のターゲット方向
                            }

                            // 手持ち武器を非表示（ドロップ後は手に持たない）
                            Type1EquippableWeapons[i].HeldObject.SetActive(false);
                        }
                        else
                        {
                            Debug.LogError("The AI '" + gameObject.name + "' does not have a held object for the '" + Type1EquippableWeapons[i].DroppableObject.name + "' Droppable Object (Type 1) with the Items Component, please assign one."); // 参照未設定のエラー
                        }
                    }
                }

                for (int i = 0; i < Type2EquippableWeapons.Count; i++) // さらにタイプ2定義も確認
                {
                    if (Type2EquippableWeapons[i].DroppableObject != null && Type2EquippableWeapons[i].DroppableToggle)
                    {
                        if (Type2EquippableWeapons[i].HeldObject != null)
                        {
                            // DroppableObject の複製生成（タイプ2）
                            var DroppableMeleeWeapon = EmeraldObjectPool.Spawn(Type2EquippableWeapons[i].DroppableObject, Type2EquippableWeapons[i].HeldObject.transform.position, Type2EquippableWeapons[i].HeldObject.transform.rotation);
                            DroppableMeleeWeapon.transform.localScale = Type2EquippableWeapons[i].HeldObject.transform.lossyScale;
                            DroppableMeleeWeapon.transform.SetParent(Type2EquippableWeapons[i].HeldObject.transform.parent);
                            DroppableMeleeWeapon.transform.localPosition = Type2EquippableWeapons[i].HeldObject.transform.localPosition;
                            DroppableMeleeWeapon.gameObject.name = Type2EquippableWeapons[i].HeldObject.gameObject.name + " (Droppable Copy)";
                            DroppableMeleeWeapon.transform.SetParent(EmeraldSystem.ObjectPool.transform);

                            // Collider / Rigidbody を保証
                            if (DroppableMeleeWeapon.GetComponent<Collider>() == null)
                                DroppableMeleeWeapon.AddComponent<BoxCollider>();

                            if (DroppableMeleeWeapon.GetComponent<Rigidbody>() == null)
                                DroppableMeleeWeapon.AddComponent<Rigidbody>();

                            // 物理補間
                            Rigidbody WeaponRigidbody = DroppableMeleeWeapon.GetComponent<Rigidbody>();
                            WeaponRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                            // 受けた力の反対方向へ反動を与える（タイプ2は負方向の係数）
                            Transform LastAttacker = EmeraldComponent.CombatComponent.LastAttacker;

                            if (EmeraldComponent.CombatTarget != null)
                            {
                                if (LastAttacker != null && LastAttacker != EmeraldComponent.CombatTarget)
                                    WeaponRigidbody.AddForce((LastAttacker.position - transform.position).normalized * -EmeraldComponent.CombatComponent.ReceivedRagdollForceAmount, ForceMode.Impulse);
                                else
                                    WeaponRigidbody.AddForce((EmeraldComponent.CombatTarget.position - transform.position).normalized * -EmeraldComponent.CombatComponent.ReceivedRagdollForceAmount, ForceMode.Impulse);
                            }

                            // 手持ち武器を非表示
                            Type2EquippableWeapons[i].HeldObject.SetActive(false);
                        }
                        else
                        {
                            Debug.LogError("The AI '" + gameObject.name + "' does not have a held object for the '" + Type2EquippableWeapons[i].DroppableObject.name + "' Droppable Object (Type 2) with the Items Component, please assign one."); // 参照未設定エラー
                        }
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）指定の武器タイプを装備状態にし、必要であれば装備SEを再生する。
        /// </summary>
        public void EquipWeapon(string WeaponTypeToEnable) // アニメーションイベントから呼ぶ想定の装備処理
        {
            EmeraldSystem EmeraldComponent = GetComponent<EmeraldSystem>(); // 参照取得（未使用だが原文通り保持）

            if (WeaponTypeToEnable == "Weapon Type 1")   // 文字列は厳密一致（スペース/大文字小文字に注意）
            {
                for (int i = 0; i < Type1EquippableWeapons.Count; i++)
                {
                    if (!Type1EquippableWeapons[i].HolsteredToggle) return; // ホルスター切替が無効なら何もしない

                    if (Type1EquippableWeapons[i].HeldObject != null) Type1EquippableWeapons[i].HeldObject.SetActive(true);  // 手持ち武器を表示
                    if (Type1EquippableWeapons[i].HolsteredObject != null) Type1EquippableWeapons[i].HolsteredObject.SetActive(false); // ホルスター武器を非表示
                }

                OnEquipWeapon?.Invoke(WeaponTypeToEnable); // 装備イベントを発火
            }
            else if (WeaponTypeToEnable == "Weapon Type 2")
            {
                for (int i = 0; i < Type2EquippableWeapons.Count; i++)
                {
                    if (!Type2EquippableWeapons[i].HolsteredToggle) return; // 同上

                    if (Type2EquippableWeapons[i].HeldObject != null) Type2EquippableWeapons[i].HeldObject.SetActive(true);   // 手持ち表示
                    if (Type2EquippableWeapons[i].HolsteredObject != null) Type2EquippableWeapons[i].HolsteredObject.SetActive(false); // ホルスター非表示
                }

                OnEquipWeapon?.Invoke(WeaponTypeToEnable); // 装備イベントを発火
            }
            else
            {
                Debug.Log("This string withing the EquipWeapon Animation Event is blank or incorrect. Ensure that it's either Weapon Type 1 or Weapon Type 2."); // 入力文字列不正
            }
        }

        /// <summary>
        /// （日本語）指定の武器タイプを非装備状態にし、必要であれば収納SEを再生する。
        /// </summary>
        public void UnequipWeapon(string WeaponTypeToDisable) // アニメーションイベントから呼ぶ想定の非装備処理
        {
            EmeraldSystem EmeraldComponent = GetComponent<EmeraldSystem>(); // 参照取得（未使用だが原文通り保持）

            if (WeaponTypeToDisable == "Weapon Type 1")
            {
                for (int i = 0; i < Type1EquippableWeapons.Count; i++)
                {
                    if (!Type1EquippableWeapons[i].HolsteredToggle) return; // ホルスター切替が無効なら何もしない

                    if (Type1EquippableWeapons[i].HeldObject != null) Type1EquippableWeapons[i].HeldObject.SetActive(false); // 手持ち武器を非表示
                    if (Type1EquippableWeapons[i].HolsteredObject != null) Type1EquippableWeapons[i].HolsteredObject.SetActive(true); // ホルスター武器を表示
                }

                OnUnequipWeapon?.Invoke(WeaponTypeToDisable); // 非装備イベントを発火
            }
            else if (WeaponTypeToDisable == "Weapon Type 2")
            {
                for (int i = 0; i < Type2EquippableWeapons.Count; i++)
                {
                    if (!Type2EquippableWeapons[i].HolsteredToggle) return; // 同上

                    if (Type2EquippableWeapons[i].HeldObject != null) Type2EquippableWeapons[i].HeldObject.SetActive(false); // 手持ち非表示
                    if (Type2EquippableWeapons[i].HolsteredObject != null) Type2EquippableWeapons[i].HolsteredObject.SetActive(true); // ホルスター表示
                }

                OnUnequipWeapon?.Invoke(WeaponTypeToDisable); // 非装備イベントを発火
            }
            else
            {
                Debug.Log("This string withing the UnequipWeapon Animation Event is blank or incorrect. Ensure that it's either Type 2 or Type 1."); // 入力文字列不正
            }
        }

        /// <summary>
        /// （日本語）ItemList 内から指定IDのアイテムを探し、該当オブジェクトを有効化する。
        /// </summary>
        public void EnableItem(int ItemID)                // アイテムを有効化
        {
            // ItemList を走査して該当IDを検索し、見つかれば同インデックスのオブジェクトを有効化
            for (int i = 0; i < ItemList.Count; i++)
            {
                if (ItemList[i].ItemID == ItemID)
                {
                    if (ItemList[i].ItemObject != null)
                    {
                        ItemList[i].ItemObject.SetActive(true); // オブジェクトを有効化
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）ItemList 内から指定IDのアイテムを探し、該当オブジェクトを無効化する。
        /// </summary>
        public void DisableItem(int ItemID)               // アイテムを無効化
        {
            // ItemList を走査して該当IDを検索し、見つかれば同インデックスのオブジェクトを無効化
            for (int i = 0; i < ItemList.Count; i++)
            {
                if (ItemList[i].ItemID == ItemID)
                {
                    if (ItemList[i].ItemObject != null)
                    {
                        ItemList[i].ItemObject.SetActive(false); // オブジェクトを無効化
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）ItemList に登録されたすべてのアイテムを無効化する。
        /// </summary>
        public void DisableAllItems()                     // 全アイテム無効化
        {
            // すべての ItemObject を非アクティブ化
            for (int i = 0; i < ItemList.Count; i++)
            {
                if (ItemList[i].ItemObject != null)
                {
                    ItemList[i].ItemObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// （日本語）装備状態を初期状態へリセットする（アニメーション設定の有無により挙動分岐）。
        /// </summary>
        public void ResetSettings()                       // 装備/収納表示の初期化
        {
            EmeraldSystem EmeraldComponent = GetComponent<EmeraldSystem>(); // 参照取得

            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1) // タイプ1装備時
            {
                for (int i = 0; i < Type1EquippableWeapons.Count; i++)
                {
                    if (EmeraldComponent.AnimationComponent.m_AnimationProfile.Type1Animations.PullOutWeapon.AnimationClip != null && EmeraldComponent.AnimationComponent.m_AnimationProfile.Type1Animations.PutAwayWeapon.AnimationClip != null)
                    {
                        if (Type1EquippableWeapons[i].HeldObject != null && Type1EquippableWeapons[i].HolsteredObject != null)
                        {
                            Type1EquippableWeapons[i].HeldObject.SetActive(false);   // 手持ち武器を隠す
                            Type1EquippableWeapons[i].HolsteredObject.SetActive(true); // ホルスター武器を表示
                        }
                        else
                        {
                            if (Type1EquippableWeapons[i].HeldObject) Type1EquippableWeapons[i].HeldObject.SetActive(true); // どちらか欠けていれば手持ちを表示
                        }
                    }
                    else
                    {
                        if (Type1EquippableWeapons[i].HeldObject) Type1EquippableWeapons[i].HeldObject.SetActive(true); // アニメ設定がなければ手持ちを表示
                    }
                }
            }
            else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2) // タイプ2装備時
            {
                for (int i = 0; i < Type2EquippableWeapons.Count; i++)
                {
                    if (EmeraldComponent.AnimationComponent.m_AnimationProfile.Type2Animations.PullOutWeapon.AnimationClip != null && EmeraldComponent.AnimationComponent.m_AnimationProfile.Type2Animations.PutAwayWeapon.AnimationClip != null)
                    {
                        if (Type2EquippableWeapons[i].HeldObject != null && Type2EquippableWeapons[i].HolsteredObject != null)
                        {
                            Type2EquippableWeapons[i].HeldObject.SetActive(false);   // 手持ち隠す
                            Type2EquippableWeapons[i].HolsteredObject.SetActive(true); // ホルスター表示
                        }
                        else
                        {
                            if (Type2EquippableWeapons[i].HeldObject) Type2EquippableWeapons[i].HeldObject.SetActive(true); // どちらか欠けていれば手持ち表示
                        }
                    }
                    else
                    {
                        if (Type2EquippableWeapons[i].HeldObject) Type2EquippableWeapons[i].HeldObject.SetActive(true); // アニメ設定がなければ手持ち表示
                    }
                }
            }
        }

        public void EnableWeaponCollider(string Name)     // 武器当たり判定を有効化（アニメイベント等から呼ぶ）
        {
            EmeraldSystem EmeraldComponent = GetComponent<EmeraldSystem>(); // 参照取得

            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1) // タイプ1
            {
                for (int i = 0; i < Type1EquippableWeapons.Count; i++)
                {
                    if (Type1EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>() != null) // コリジョンコンポーネントが付いていれば
                    {
                        Type1EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>().EnableWeaponCollider(Name); // 名称に対応したコライダーを有効化
                    }
                }
            }
            else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2) // タイプ2
            {
                for (int i = 0; i < Type2EquippableWeapons.Count; i++)
                {
                    if (Type2EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>() != null)
                    {
                        Type2EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>().EnableWeaponCollider(Name);
                    }
                }
            }
        }

        public void DisableWeaponCollider(string Name)    // 武器当たり判定を無効化
        {
            EmeraldSystem EmeraldComponent = GetComponent<EmeraldSystem>(); // 参照取得

            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1) // タイプ1
            {
                for (int i = 0; i < Type1EquippableWeapons.Count; i++)
                {
                    if (Type1EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>() != null)
                    {
                        Type1EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>().DisableWeaponCollider(Name); // 名称に対応したコライダーを無効化
                    }
                }
            }
            else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2) // タイプ2
            {
                for (int i = 0; i < Type2EquippableWeapons.Count; i++)
                {
                    if (Type2EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>() != null)
                    {
                        Type2EquippableWeapons[i].HeldObject.GetComponent<EmeraldWeaponCollision>().DisableWeaponCollider(Name);
                    }
                }
            }
        }
    }
}
