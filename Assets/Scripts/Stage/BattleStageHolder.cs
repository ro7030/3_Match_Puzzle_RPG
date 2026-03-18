using UnityEngine;
using Match3Puzzle.Stage;

namespace Match3Puzzle.Stage
{
    /// <summary>
    /// 스테이지 선택 시 "진입할 스테이지 번호"를 저장. 배틀 씬 로드 후 여기서 읽어서 적용.
    /// 스테이지 선택 화면에서 클릭 시: BattleStageHolder.SetStage(index); 후 씬 로드.
    /// </summary>
    public static class BattleStageHolder
    {
        /// <summary>진입할 스테이지 인덱스 (0~8 = 스테이지 1~9)</summary>
        public static int CurrentStageIndex { get; set; }

        /// <summary>스테이지 선택 시 호출. 배틀 씬 로드 전에 호출.</summary>
        public static void SetStage(int stageIndex)
        {
            CurrentStageIndex = Mathf.Clamp(stageIndex, 0, StageDatabase.StageCount - 1);
        }

        /// <summary>스테이지 번호 (1~9)</summary>
        public static int CurrentStageNumber => CurrentStageIndex + 1;
    }
}
