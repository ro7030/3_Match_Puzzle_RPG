using UnityEngine;
using Match3Puzzle.Core;

namespace Match3Puzzle.Level
{
    /// <summary>
    /// 턴(이동 횟수)을 관리하는 클래스.
    /// 한 턴 = 퍼즐 한 번 이동(스왑)해서 매칭 성공 시 1턴 소모. 턴당 시간 제한 없음.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [Header("Level Settings")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private LevelData[] levelDataArray;

        [Header("Goal Settings")]
        [SerializeField] private int movesRemaining = 30;
        [SerializeField] private float timeLimit = 60f;

        private int currentMoves = 0;
        private float currentTime = 0f;
        private bool isTimeLimitMode = false;

        public int CurrentLevel => currentLevel;
        public int MovesRemaining => movesRemaining - currentMoves;
        /// <summary>배틀에서 실제로 사용된 턴 수(= currentMoves)</summary>
        public int MovesUsed => currentMoves;
        /// <summary>배틀 턴 UI용: 현재 턴 번호 (1~MaxTurns)</summary>
        public int CurrentTurnNumber => Mathf.Clamp(currentMoves + 1, 1, movesRemaining);
        /// <summary>배틀 턴 UI용: 최대 턴 수 (예: 20)</summary>
        public int MaxTurns => movesRemaining;
        public float TimeRemaining => isTimeLimitMode ? Mathf.Max(0f, timeLimit - currentTime) : -1f;

        /// <summary>턴 변경 시 (매칭 후). 인자: 현재 사용한 턴 수</summary>
        public event System.Action<int> OnTurnChanged;
        /// <summary>최대 턴 소진 시. 구독자가 보스 체력 확인 후 승/패 처리</summary>
        public event System.Action OnTurnsExhausted;

        private void Update()
        {
            if (isTimeLimitMode && GameManager.Instance != null && 
                GameManager.Instance.CurrentState == GameState.Playing)
            {
                currentTime += Time.deltaTime;

                if (currentTime >= timeLimit)
                {
                    GameManager.Instance.GameOver();
                }
            }
        }

        /// <summary>
        /// 레벨 초기화
        /// </summary>
        public void InitializeLevel(int level)
        {
            currentLevel = level;
            currentMoves = 0;
            currentTime = 0f;

            if (levelDataArray != null && level >= 1 && level <= levelDataArray.Length)
            {
                LevelData data = levelDataArray[level - 1];
                if (data != null)
                {
                    movesRemaining = data.movesLimit;
                    timeLimit = data.timeLimit;
                    isTimeLimitMode = data.hasTimeLimit;
                }
            }
        }

        /// <summary>
        /// 이동 횟수 증가. 퍼즐 한 번 스왑 후 매칭 성공 시 1회 호출 (= 1턴 소모).
        /// </summary>
        public void IncrementMoves()
        {
            currentMoves++;
            OnTurnChanged?.Invoke(currentMoves);

            if (movesRemaining > 0 && currentMoves >= movesRemaining)
            {
                OnTurnsExhausted?.Invoke();
            }
        }

        /// <summary>
        /// 배틀 스테이지용 설정. 최대 턴 수만 지정 (스테이지 선택 시 호출).
        /// 승패는 몬스터 체력(0 이하)과 40턴 소진 여부로만 판정됨.
        /// </summary>
        public void SetBattleConfig(int maxTurns)
        {
            currentMoves = 0;
            currentTime = 0f;
            movesRemaining = Mathf.Max(1, maxTurns);
            OnTurnChanged?.Invoke(currentMoves); // 새 스테이지 진입 시 UI를 1턴(내부값 0)으로 즉시 동기화
        }
    }

    /// <summary>
    /// 레벨 데이터 (ScriptableObject로 만들 수 있음)
    /// </summary>
    [System.Serializable]
    public class LevelData
    {
        public int movesLimit = 30;
        public float timeLimit = 60f;
        public bool hasTimeLimit = false;
    }
}
