﻿using UnityEngine; // Unity の基本API

namespace EmeraldAI.Utility // EmeraldAI のユーティリティ用名前空間
{
    [ExecuteInEditMode] // エディタ上でも実行（Gizmos描画や検証をエディタモードで実行可能）
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/target-position-modifier-component")] // 公式ドキュメントへのリンク

    // 【クラス概要】TargetPositionModifier：
    // 指定した Transform（TransformSource）を基準に、Y方向（上方向）へ任意のオフセット（PositionModifier）を加えた
    // 目標位置（ターゲット位置）を視覚化（Gizmos）するシンプルな補助コンポーネント。
    // エディタ選択時に球体Gizmoで位置を示し、設定UIの折りたたみ状況に応じて描画を制御します。
    public class TargetPositionModifier : MonoBehaviour // MonoBehaviour を継承
    {
        [Header("【Editor表示】設定群を隠す（折りたたみ制御）")] // インスペクタの設定折りたたみ全体を隠すか
        public bool HideSettingsFoldout; // true のとき設定UIを隠す想定

        [Header("【Editor表示】Target Position Modifier 設定セクションの折りたたみ")]
        public bool TPMSettingsFoldout = false; // セクションの開閉。true で開く

        [Header("基準となる Transform（ここを基点に上方向へオフセットしてGizmo描画）")]
        public Transform TransformSource; // 基準となる Transform。未指定時は OnEnable で自身の transform をセット

        [Header("基準Transformからの上方向（Y軸）オフセット量（メートル）")]
        public float PositionModifier = 0; // 上方向のオフセット値

        [Header("Gizmo の球の半径")]
        public float GizmoRadius = 0.15f; // Gizmoの見た目サイズ

        [Header("Gizmo の色（RGBA）")]
        public Color GizmoColor = new Color(1f, 0, 0, 0.8f); // 赤系・半透明

        void OnEnable() // 有効化時（エディタ/プレイ両方で呼ばれる）
        {
            // TransformSource 未指定かつプレイ中であれば、警告を出しつつ自身の transform を代入（望ましくない可能性あり）
            if (TransformSource == null && Application.isPlaying)
            {
                Debug.LogError("<b>Target Position Modifier:</b> " + "No Transform Source has been assigned on " + gameObject.name + ". The Transform Source will be set as this object instead (which may be undesirable). To resolve this, add a proper Transform Source through the Target Position Modifier editor."); // 英文原文ログ（仕様に忠実）
                TransformSource = transform; // フォールバックとして自身を参照
            }
        }

        private void OnDrawGizmosSelected() // シーン上でこのオブジェクトを選択しているときにのみ呼ばれるGizmos描画
        {
            // TransformSource 未設定、または UI 折りたたみが閉じている、あるいは UI 非表示指定の場合は描画しない
            if (TransformSource == null || !TPMSettingsFoldout || HideSettingsFoldout)
                return; // 何もしないで終了

            Gizmos.color = GizmoColor; // Gizmo の色を設定
            // 基準Transformの位置に PositionModifier（上方向）を加えた点に、Gizmo の球を描画
            Gizmos.DrawSphere(TransformSource.position + (Vector3.up * PositionModifier), GizmoRadius);
        }
    }
}
