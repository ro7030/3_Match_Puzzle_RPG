using UnityEngine;

namespace Match3Puzzle.Core
{
    /// <summary>
    /// 게임 상태 열거형
    /// </summary>
    public enum GameState
    {
        None,
        Menu,           // 메뉴 화면
        Playing,        // 게임 진행 중
        Paused,         // 일시정지
        Swapping,       // 타일 교환 중
        Matching,       // 매칭 처리 중
        Clearing,      // 타일 제거 중
        Falling,        // 타일 낙하 중
        Spawning,       // 새 타일 생성 중
        GameOver,       // 게임 오버
        LevelComplete   // 레벨 클리어
    }
}
