using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Match3Puzzle.Score;
using Match3Puzzle.Level;
using Match3Puzzle.Stage;
using Story;
using MainMenu;

namespace Match3Puzzle.UI
{
    /// <summary>
    /// UI를 관리하는 클래스
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Score UI")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private TextMeshProUGUI comboText;

        [Header("Level UI")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI targetScoreText;

        [Header("Game Over UI")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverScoreText;
        [SerializeField] private Button restartButton;

        [Header("Level Complete UI")]
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private TextMeshProUGUI levelCompleteScoreText;
        [SerializeField] private Button nextLevelButton;

        [Header("클리어 후 씬 이동")]
        [Tooltip("컷씬 없을 때 이동할 씬")]
        [SerializeField] private string mapSceneName = "MapScene";
        [Tooltip("컷씬 있을 때 이동할 씬")]
        [SerializeField] private string storySceneName = "StoryScene";

        [Header("스테이지 데이터 (없으면 Resources 자동 로드)")]
        [SerializeField] private StageDatabase stageDatabase;

        [Header("Pause UI")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;

        private ScoreManager scoreManager;
        private LevelManager levelManager;

        private void Awake()
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
            levelManager = FindFirstObjectByType<LevelManager>();

            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged += UpdateScore;
                scoreManager.OnComboChanged += UpdateCombo;
                scoreManager.OnBestScoreChanged += UpdateBestScore;
            }

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);

            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseClicked);

            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);
        }

        private void Start()
        {
            UpdateAllUI();
        }

        private void Update()
        {
            UpdateTimeUI();
        }

        /// <summary>
        /// 모든 UI 업데이트
        /// </summary>
        private void UpdateAllUI()
        {
            UpdateScore(scoreManager != null ? scoreManager.CurrentScore : 0);
            UpdateBestScore(scoreManager != null ? scoreManager.BestScore : 0);
            UpdateCombo(scoreManager != null ? scoreManager.ComboCount : 0);
            UpdateLevelUI();
        }

        /// <summary>
        /// 점수 UI 업데이트
        /// </summary>
        private void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score:N0}";
        }

        /// <summary>
        /// 최고 점수 UI 업데이트
        /// </summary>
        private void UpdateBestScore(int bestScore)
        {
            if (bestScoreText != null)
                bestScoreText.text = $"Best: {bestScore:N0}";
        }

        /// <summary>
        /// 콤보 UI 업데이트
        /// </summary>
        private void UpdateCombo(int combo)
        {
            if (comboText != null)
            {
                if (combo > 0)
                    comboText.text = $"Combo x{combo}!";
                else
                    comboText.text = "";
            }
        }

        /// <summary>
        /// 레벨 UI 업데이트
        /// </summary>
        private void UpdateLevelUI()
        {
            if (levelManager != null)
            {
                if (levelText != null)
                    levelText.text = $"Level {levelManager.CurrentLevel}";

                if (movesText != null)
                    movesText.text = $"Moves: {levelManager.MovesRemaining}";

                // targetScoreText: 목표 점수 미사용 (승패는 몬스터 체력으로만 판정). 비워둠.
                if (targetScoreText != null)
                    targetScoreText.text = "";
            }
        }

        /// <summary>
        /// 시간 UI 업데이트
        /// </summary>
        private void UpdateTimeUI()
        {
            if (timeText != null && levelManager != null)
            {
                float timeRemaining = levelManager.TimeRemaining;
                if (timeRemaining >= 0f)
                {
                    int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                    int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                    timeText.text = $"Time: {minutes:00}:{seconds:00}";
                }
                else
                {
                    timeText.text = "";
                }
            }
        }

        /// <summary>
        /// 게임 오버 화면 표시
        /// </summary>
        public void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                if (gameOverScoreText != null && scoreManager != null)
                {
                    gameOverScoreText.text = $"Final Score: {scoreManager.CurrentScore:N0}";
                }
            }
        }

        /// <summary>
        /// 레벨 클리어 화면 표시
        /// </summary>
        public void ShowLevelComplete()
        {
            Debug.Log($"[UIManager] ShowLevelComplete → panel:{levelCompletePanel != null}");
            if (levelCompletePanel != null)
            {
                levelCompletePanel.SetActive(true);
                if (levelCompleteScoreText != null && scoreManager != null)
                    levelCompleteScoreText.text = $"Score: {scoreManager.CurrentScore:N0}";
            }
            else
            {
                Debug.LogWarning("[UIManager] levelCompletePanel이 null → Inspector에서 연결 필요. MapScene으로 이동합니다.");
                UnityEngine.SceneManagement.SceneManager.LoadScene("MapScene");
            }
        }

        private void OnRestartClicked()
        {
            Core.GameManager.Instance?.RestartGame();
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        private void OnNextLevelClicked()
        {
            if (levelCompletePanel != null)
                levelCompletePanel.SetActive(false);

            // 1. 클리어한 스테이지 인덱스 세이브
            SaveClearedStage();

            // 2. 클리어 컷씬 여부 확인
            var stageData = GetCurrentStageData();
            if (stageData != null && stageData.clearCutscene != null)
            {
                // 컷씬 있음 → StoryScene 로드 (컷씬 끝나면 CutsceneData.nextSceneName으로 이동)
                CutsceneContext.SetNext(stageData.clearCutscene.cutsceneId);
                SceneManager.LoadScene(storySceneName);
            }
            else
            {
                // 컷씬 없음 → MapScene으로 바로 이동
                SceneManager.LoadScene(mapSceneName);
            }
        }

        /// <summary>현재 클리어한 스테이지를 세이브 데이터에 저장</summary>
        private void SaveClearedStage()
        {
            int clearedIndex = BattleStageHolder.CurrentStageIndex;
            var saveData = SaveSystem.Load() ?? new GameSaveData();

            if (clearedIndex > saveData.lastClearedStageIndex)
            {
                saveData.lastClearedStageIndex = clearedIndex;
                saveData.lastClearedChapter = (clearedIndex / 3) + 1;

                // 신규 스테이지 클리어 → 플레이어 레벨 +1, 업그레이드 포인트 +1
                saveData.playerLevel++;
                saveData.upgradePoints++;

                SaveSystem.Save(saveData);
                Debug.Log($"[UIManager] 스테이지 클리어 저장: 인덱스 {clearedIndex} / Lv {saveData.playerLevel} / 포인트 {saveData.upgradePoints}");
            }
        }

        /// <summary>현재 스테이지의 StageData 반환</summary>
        private StageData GetCurrentStageData()
        {
            if (stageDatabase == null)
                stageDatabase = Resources.Load<StageDatabase>("StageDatabase");

            return stageDatabase?.GetStage(BattleStageHolder.CurrentStageIndex);
        }

        private void OnPauseClicked()
        {
            Core.GameManager.Instance?.PauseGame();
            if (pausePanel != null)
                pausePanel.SetActive(true);
        }

        private void OnResumeClicked()
        {
            Core.GameManager.Instance?.ResumeGame();
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged -= UpdateScore;
                scoreManager.OnComboChanged -= UpdateCombo;
                scoreManager.OnBestScoreChanged -= UpdateBestScore;
            }
        }
    }
}
