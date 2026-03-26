using System;

namespace Match3Puzzle.Stage
{
    /// <summary>
    /// 전투 중 활성 페이즈(1/2)를 공유하기 위한 런타임 상태.
    /// </summary>
    public static class BattlePhaseRuntime
    {
        public const int Phase1 = 1;
        public const int Phase2 = 2;

        public static int ActivePhaseIndex { get; private set; } = Phase1;

        public static event Action<int> OnPhaseChanged;

        /// <summary>
        /// 배틀 중 컷씬(StoryScene addititve 등) 진행 중 여부.
        /// 이 동안에는 다른 코루틴이 `GameState.Playing`으로 되돌리지 않게 막습니다.
        /// </summary>
        public static bool IsBattleCutsceneActive { get; private set; }

        public static void ResetForNewBattle()
        {
            SetPhaseInternal(Phase1);
            IsBattleCutsceneActive = false;
        }

        public static void SetCutsceneActive(bool isActive)
        {
            IsBattleCutsceneActive = isActive;
        }

        public static void SetPhase(int phaseIndex)
        {
            if (phaseIndex != Phase1 && phaseIndex != Phase2)
                phaseIndex = Phase1;

            SetPhaseInternal(phaseIndex);
        }

        private static void SetPhaseInternal(int phaseIndex)
        {
            if (ActivePhaseIndex == phaseIndex) return;
            ActivePhaseIndex = phaseIndex;
            OnPhaseChanged?.Invoke(ActivePhaseIndex);
        }
    }
}

