namespace EmeraldAI                             // EmeraldAI 用の名前空間を宣言（プロジェクト内の論理的な区分）
{
    [System.Serializable]                       // インスペクタ表示やシリアライズを可能にする属性
    public class FactionClass                   // 【FactionClass】派閥IDと関係タイプを1組で保持するデータクラス
    {
        [UnityEngine.Header("派閥を識別するインデックス（EmeraldFactionData のリスト要素と対応させて使用）")]
        public int FactionIndex;                // 派閥のID（例：0=Player, 1=Neutral, 2=Enemy などプロジェクト固有の定義に合わせる）

        //[UnityEngine.Header("この派閥に対する関係タイプ（RelationTypes：Friendly/Neutral/Enemy などの列挙型）")]
        public RelationTypes RelationType;      // 当該派閥への関係（友好/中立/敵対 等）

        /// <summary>
        /// コンストラクタ：派閥インデックスと関係タイプ（int）を受け取り、メンバーへ格納します。
        /// m_RelationType は int 値として渡され、RelationTypes へキャストされます。
        /// </summary>
        public FactionClass(int m_FactionIndex, int m_RelationType)   // 2つの引数で初期化するコンストラクタ
        {
            FactionIndex = m_FactionIndex;                            // 受け取った派閥インデックスをそのまま代入
            RelationType = (RelationTypes)m_RelationType;             // int を RelationTypes 列挙型へキャストして保持
        }
    }
}
