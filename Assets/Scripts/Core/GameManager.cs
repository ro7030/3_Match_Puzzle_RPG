using UnityEngine;
using UnityEngine.SceneManagement;
using Match3Puzzle.Core;
using Match3Puzzle.Board;
using Match3Puzzle.UI;
using Match3Puzzle.Score;
using Match3Puzzle.Level;

namespace Match3Puzzle.Core
{
    /// <summary>
    /// 게임 전체를 관리하는 메인 매니저
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private LevelManager levelManager;

        [Header("Game Settings")]
        [SerializeField] private int boardWidth = 10;
        [SerializeField] private int boardHeight = 10;
        [SerializeField] private int tileTypes = 4; // 검(0), 활(1), 십자가(2), 지팡이(3)

        private GameState currentState = GameState.None;
        private GameState previousState = GameState.None;

        public GameState CurrentState => currentState;
        public int BoardWidth => boardWidth;
        public int BoardHeight => boardHeight;
        public int TileTypes => tileTypes;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeManagers();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>씬이 로드될 때마다 씬 내 컴포넌트 참조를 갱신</summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshManagers();
        }

        /// <summary>현재 씬에서 매니저 참조를 새로 탐색 (씬 전환 후 참조 갱신용)</summary>
        private void RefreshManagers()
        {
            gameBoard   = FindFirstObjectByType<GameBoard>();
            uiManager   = FindFirstObjectByType<UIManager>();
            scoreManager = FindFirstObjectByType<ScoreManager>();
            levelManager = FindFirstObjectByType<LevelManager>();
        }

        private void Start()
        {
            ChangeState(GameState.Menu);
        }

        private void InitializeManagers()
        {
            if (gameBoard == null)
                gameBoard = FindFirstObjectByType<GameBoard>();

            if (uiManager == null)
                uiManager = FindFirstObjectByType<UIManager>();

            if (scoreManager == null)
                scoreManager = FindFirstObjectByType<ScoreManager>();

            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelManager>();
        }

        /// <summary>
        /// 게임 상태 변경
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (currentState == newState) return;

            // 종료 상태(LevelComplete/GameOver)는 다른 상태로 덮어쓸 수 없음
            if (currentState == GameState.LevelComplete || currentState == GameState.GameOver)
            {
                Debug.Log($"[GameManager] 상태 변경 차단: {currentState} → {newState} 무시 (종료 상태 보호)");
                return;
            }

            previousState = currentState;
            currentState = newState;

            OnStateExit(previousState);
            OnStateEnter(currentState);

            Debug.Log($"[GameManager] 상태 변경: {previousState} -> {currentState}");
        }

        private void OnStateEnter(GameState state)
        {
            switch (state)
            {
                case GameState.Menu:
                    Time.timeScale = 1f;
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;

                case GameState.GameOver:
                case GameState.LevelComplete:
                    Time.timeScale = 1f;
                    break;
            }
        }

        private void OnStateExit(GameState state)
        {
            // 상태 종료 시 처리
        }

        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame()
        {
            if (gameBoard != null)
            {
                gameBoard.InitializeBoard(boardWidth, boardHeight, tileTypes);
            }

            if (scoreManager != null)
            {
                scoreManager.ResetScore();
            }

            ChangeState(GameState.Playing);
        }

        /// <summary>
        /// 게임 재시작
        /// </summary>
        public void RestartGame()
        {
            StartGame();
        }

        /// <summary>
        /// 게임 일시정지
        /// </summary>
        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
            }
        }

        /// <summary>
        /// 게임 재개
        /// </summary>
        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
            }
        }

        /// <summary>
        /// 게임 오버 처리
        /// </summary>
        public void GameOver()
        {
            ChangeState(GameState.GameOver);
            EnsureUIManager();
            if (uiManager != null)
                uiManager.ShowGameOver();
        }

        /// <summary>
        /// 레벨 클리어 처리
        /// </summary>
        public void LevelComplete()
        {
            ChangeState(GameState.LevelComplete);
            EnsureUIManager();

            if (uiManager != null)
            {
                uiManager.ShowLevelComplete();
            }
            else
            {
                Debug.LogWarning("[GameManager] UIManager 없음 → 세이브 후 MapScene으로 직접 이동");
                SaveClearedStageAndLoadMap();
            }
        }

        /// <summary>UIManager 없을 때 세이브 + MapScene 직접 이동</summary>
        private void SaveClearedStageAndLoadMap()
        {
            int clearedIndex = Match3Puzzle.Stage.BattleStageHolder.CurrentStageIndex;
            var saveData = MainMenu.SaveSystem.Load() ?? new MainMenu.GameSaveData();
            if (clearedIndex > saveData.lastClearedStageIndex)
            {
                saveData.lastClearedStageIndex = clearedIndex;
                saveData.lastClearedChapter = (clearedIndex / 3) + 1;

                // 신규 스테이지 클리어 → 플레이어 레벨 +1, 업그레이드 포인트 +1
                saveData.playerLevel++;
                saveData.upgradePoints++;

                MainMenu.SaveSystem.Save(saveData);
            }
            SceneManager.LoadScene("MapScene");
        }

        /// <summary>uiManager가 null이면 현재 씬에서 즉시 재탐색</summary>
        private void EnsureUIManager()
        {
            if (uiManager == null)
                uiManager = FindFirstObjectByType<UIManager>();
        }

        /// <summary>
        /// 이전 상태로 복귀
        /// </summary>
        public void ReturnToPreviousState()
        {
            ChangeState(previousState);
        }
    }
}
