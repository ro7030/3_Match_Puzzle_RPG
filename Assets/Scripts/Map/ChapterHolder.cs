namespace Match3Puzzle.Map
{
    /// <summary>
    /// MapScene → StageSelectPanel 간 선택된 챕터 번호를 전달하는 정적 홀더.
    /// 챕터 번호 범위: 1(Monster Forest), 2(Ancient Legendary Ruins), 3(Dragon's Fortress)
    ///
    /// ─ 전역 스테이지 인덱스 구조 ──────────────────────────────
    ///  Index 0          = Tutorial
    ///  Index 1, 2, 3    = 챕터1 Stage1, Stage2, Stage3
    ///  Index 4, 5, 6    = 챕터2 Stage1, Stage2, Stage3
    ///  Index 7, 8, 9    = 챕터3 Stage1, Stage2, Stage3
    /// ────────────────────────────────────────────────────────────
    /// </summary>
    public static class ChapterHolder
    {
        /// <summary>현재 선택된 챕터 번호 (1~3)</summary>
        public static int CurrentChapter { get; set; } = 1;

        /// <summary>
        /// MapScene 진입 시 StageSelectPanel을 자동으로 열지 여부.
        /// (클리어 패널의 “스테이지 선택” 버튼에서 설정)
        /// </summary>
        public static bool AutoOpenStageSelect { get; set; } = false;

        /// <summary>
        /// 챕터 번호를 전역 스테이지 인덱스의 시작 값으로 변환.
        /// Index 0은 Tutorial이므로 챕터 스테이지는 1부터 시작.
        /// 챕터1 → 1, 챕터2 → 4, 챕터3 → 7
        /// </summary>
        public static int GetFirstStageIndex(int chapterNumber)
        {
            return (chapterNumber - 1) * 3 + 1;
        }
    }
}
