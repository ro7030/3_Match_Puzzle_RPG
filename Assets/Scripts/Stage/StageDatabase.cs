using UnityEngine;

namespace Match3Puzzle.Stage
{
    /// <summary>
    /// 스테이지 데이터를 한 번에 보관.
    /// Index 0 = Tutorial, Index 1~9 = 챕터1~3 각 Stage1~3
    /// </summary>
    [CreateAssetMenu(fileName = "StageDatabase", menuName = "Match3/Stage Database", order = 1)]
    public class StageDatabase : ScriptableObject
    {
        /// <summary>Tutorial(1) + 챕터3개 × 스테이지3개(9) = 총 10개</summary>
        public const int StageCount = 10;

        [SerializeField] private StageData[] stages = new StageData[StageCount];

        public StageData GetStage(int index)
        {
            if (stages == null || index < 0 || index >= stages.Length)
                return null;
            return stages[index];
        }

        public int Count => stages?.Length ?? 0;
    }
}
