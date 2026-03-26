using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Match3Puzzle.Score;
using Match3Puzzle.Level;
using Match3Puzzle.Stage;
using Match3Puzzle.Map; // (stage select auto-open 계산에는 BattleStageHolder 사용)
using Story;
using MainMenu;
using Match3Puzzle.UI.Battle;

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

        [Header("Defeat UI")]
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private Button defeatReplayButton;
        [SerializeField] private Button defeatStageSelectButton;
        [SerializeField] private string defeatPanelName = "DefeatPanel";
        [SerializeField] private TextMeshProUGUI defeatScoreText;
        [SerializeField] private TextMeshProUGUI defeatGoldText;
        [SerializeField] private TextMeshProUGUI defeatTurnsText;
        [SerializeField] private TextMeshProUGUI defeatEnhancedSummonText;
        [SerializeField] private TextMeshProUGUI defeatSwordDamageText;
        [SerializeField] private TextMeshProUGUI defeatBowDamageText;
        [SerializeField] private TextMeshProUGUI defeatWandDamageText;
        [SerializeField] private TextMeshProUGUI defeatCrossHealText;
        [SerializeField] private Image defeatRandomCharacterImage;

        [Header("Level Complete Stats UI")]
        [SerializeField] private Button stageSelectButton;
        [SerializeField] private Image clearRandomCharacterImage;

        [SerializeField] private TextMeshProUGUI clearGoldText;
        [SerializeField] private TextMeshProUGUI clearTurnsText;
        [SerializeField] private TextMeshProUGUI clearEnhancedSummonText;

        [SerializeField] private TextMeshProUGUI clearSwordDamageText;
        [SerializeField] private TextMeshProUGUI clearBowDamageText;
        [SerializeField] private TextMeshProUGUI clearWandDamageText;
        [SerializeField] private TextMeshProUGUI clearCrossHealText;

        [Header("클리어 후 씬 이동")]
        [Tooltip("컷씬 없을 때 이동할 씬")]
        [SerializeField] private string mapSceneName = "MapScene";
        [Tooltip("컷씬 있을 때 이동할 씬")]
        [SerializeField] private string storySceneName = "StoryScene";
        [Tooltip("다음 스테이지 시작 씬")]
        [SerializeField] private string battleSceneName = "BattleScene";

        [Header("스테이지 데이터 (없으면 Resources 자동 로드)")]
        [SerializeField] private StageDatabase stageDatabase;

        [Header("Pause UI")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;

        private ScoreManager scoreManager;
        private LevelManager levelManager;

        private bool _clearPanelRuntimeBuilt;
        private bool _clearRewardApplied;
        private int _goldEarnedThisClear;

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
                nextLevelButton.onClick.AddListener(OnNextStageClicked);

            if (stageSelectButton != null)
                stageSelectButton.onClick.AddListener(OnStageSelectClicked);

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
            EnsureLevelCompletePanelUI();

            // 패널 표시 직전에 보상/골드를 확정(스테이지 선택 버튼 눌러도 보상이 적용되도록)
            ApplyClearRewardOnce();

            if (levelCompletePanel != null)
            {
                EnsureLevelCompleteInputCapture();
                levelCompletePanel.SetActive(true);
                levelCompletePanel.transform.SetAsLastSibling(); // 최상단으로 올려 뒤 UI 클릭 방지
                UpdateClearPanelTexts();
                UpdateNextLevelButtonLabel();

                // 버튼 리스너는 씬 전환/재클릭 상황을 고려해 매번 최신 콜백으로 교체
                if (nextLevelButton != null)
                {
                    nextLevelButton.onClick.RemoveAllListeners();
                    nextLevelButton.onClick.AddListener(OnNextStageClicked);
                }
                if (stageSelectButton != null)
                {
                    stageSelectButton.onClick.RemoveAllListeners();
                    stageSelectButton.onClick.AddListener(OnStageSelectClicked);
                }
            }
            else
            {
                // UI 생성 실패 시 최소한 맵으로 이동 (보상은 이미 적용됨)
                SceneManager.LoadScene(mapSceneName);
            }
        }

        private void UpdateNextLevelButtonLabel()
        {
            if (nextLevelButton == null) return;

            int currentIndex = BattleStageHolder.CurrentStageIndex;
            bool isLastStage = currentIndex >= StageDatabase.StageCount - 1; // Tutorial 제외한 마지막 스테이지

            SetButtonText(nextLevelButton, isLastStage ? "맵으로" : "다음 스테이지 시작");
        }

        private void SetButtonText(Button button, string text)
        {
            if (button == null) return;

            var tmp = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
                tmp.text = text;
        }

        /// <summary>
        /// 클리어 패널이 떠 있을 때 뒤쪽 UI로 클릭이 관통되지 않도록 입력 차단 레이어를 보장.
        /// </summary>
        private void EnsureLevelCompleteInputCapture()
        {
            if (levelCompletePanel == null) return;

            var rootRt = levelCompletePanel.GetComponent<RectTransform>();
            if (rootRt != null)
            {
                rootRt.anchorMin = Vector2.zero;
                rootRt.anchorMax = Vector2.one;
                rootRt.offsetMin = Vector2.zero;
                rootRt.offsetMax = Vector2.zero;
            }

            var group = levelCompletePanel.GetComponent<CanvasGroup>();
            if (group == null)
                group = levelCompletePanel.AddComponent<CanvasGroup>();
            group.interactable = true;
            group.blocksRaycasts = true;
            group.ignoreParentGroups = false;

            var blocker = levelCompletePanel.GetComponent<Image>();
            if (blocker == null)
                blocker = levelCompletePanel.AddComponent<Image>();
            blocker.raycastTarget = true;
            if (blocker.color.a <= 0f)
                blocker.color = new Color(0f, 0f, 0f, 0.65f);
        }

        private void OnRestartClicked()
        {
            Core.GameManager.Instance?.RestartGame();
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        private void OnNextStageClicked()
        {
            if (levelCompletePanel != null)
                levelCompletePanel.SetActive(false);

            int currentIndex = BattleStageHolder.CurrentStageIndex;
            int nextIndex = currentIndex + 1;

            var currentStageData = GetCurrentStageData();
            string clearCutsceneId = ResolveCutsceneId(
                currentStageData != null ? currentStageData.clearCutscene : null);

            // 마지막 스테이지 클리어 후: 에프터(clearCutscene)가 있으면 먼저 재생 → 맵 (바로 맵으로 가면 컷신이 스킵되던 버그 수정)
            if (nextIndex >= StageDatabase.StageCount)
            {
                if (!string.IsNullOrEmpty(clearCutsceneId))
                {
                    CutsceneContext.SetNextForBattle(clearCutsceneId, () =>
                    {
                        SceneManager.LoadScene(mapSceneName);
                    });
                    SceneManager.LoadScene(storySceneName);
                    return;
                }

                SceneManager.LoadScene(mapSceneName);
                return;
            }

            // 현재 스테이지 클리어 컷신 → 끝나면 다음 스테이지 인트로(있으면) 또는 배틀
            if (!string.IsNullOrEmpty(clearCutsceneId))
            {
                CutsceneContext.SetNextForBattle(clearCutsceneId, () =>
                {
                    TryStartNextStageBattle(nextIndex);
                });
                SceneManager.LoadScene(storySceneName);
                return;
            }

            TryStartNextStageBattle(nextIndex);
        }

        /// <summary>
        /// CutsceneData.cutsceneId가 비어 있으면 에셋 이름으로 로드 (StageSelectPanel과 동일).
        /// </summary>
        private static string ResolveCutsceneId(Story.CutsceneData cutscene)
        {
            if (cutscene == null) return null;
            return !string.IsNullOrEmpty(cutscene.cutsceneId) ? cutscene.cutsceneId : cutscene.name;
        }

        /// <summary>
        /// 다음 스테이지 인덱스를 저장한 뒤, 인트로 컷신이 있으면 StoryScene → 끝나면 BattleScene, 없으면 바로 BattleScene.
        /// </summary>
        private void TryStartNextStageBattle(int nextIndex)
        {
            BattleStageHolder.SetStage(nextIndex);

            if (stageDatabase == null)
                stageDatabase = Resources.Load<StageDatabase>("StageDatabase");
            var nextStageData = stageDatabase != null ? stageDatabase.GetStage(nextIndex) : null;
            string introCutsceneId = ResolveCutsceneId(
                nextStageData != null ? nextStageData.introCutscene : null);

            if (!string.IsNullOrEmpty(introCutsceneId))
            {
                CutsceneContext.SetNextForBattle(introCutsceneId, () =>
                {
                    SceneManager.LoadScene(battleSceneName);
                });
                SceneManager.LoadScene(storySceneName);
                return;
            }

            SceneManager.LoadScene(battleSceneName);
        }

        private void OnStageSelectClicked()
        {
            if (levelCompletePanel != null)
                levelCompletePanel.SetActive(false);
            if (defeatPanel != null)
                defeatPanel.SetActive(false);

            // 클리어 패널에서 "스테이지 선택"을 눌렀을 때도, 현재 스테이지의 clearCutscene이 있으면 먼저 재생한다.
            // (다음 스테이지로 가기 버튼과 동일한 스토리 흐름 유지)
            var currentStageData = GetCurrentStageData();
            string clearCutsceneId = ResolveCutsceneId(
                currentStageData != null ? currentStageData.clearCutscene : null);

            if (!string.IsNullOrEmpty(clearCutsceneId))
            {
                CutsceneContext.SetNextForBattle(clearCutsceneId, () =>
                {
                    BattleStageHolder.AutoOpenStageSelectOnMap = true;
                    SceneManager.LoadScene(mapSceneName);
                });
                SceneManager.LoadScene(storySceneName);
                return;
            }

            // MapScene 진입 후 StageSelectPanel을 자동 오픈하기 위한 값 설정
            // (프로젝트 내 StageSelectPanel은 MapSceneController가 실제로 Show() 호출)
            BattleStageHolder.AutoOpenStageSelectOnMap = true;
            SceneManager.LoadScene(mapSceneName);
        }

        /// <summary>
        /// 파티원 사망(패배) 시 DefeatPanel 표시.
        /// 버튼 동작: 다시 플레이 / 스테이지 선택
        /// </summary>
        public void ShowPartyDefeatPanel()
        {
            EnsureDefeatPanelBindings();
            if (defeatPanel == null)
            {
                // DefeatPanel이 없으면 기존 GameOver 패널로 폴백
                ShowGameOver();
                return;
            }

            EnsureInputCaptureForPanel(defeatPanel);
            defeatPanel.SetActive(true);
            defeatPanel.transform.SetAsLastSibling();
            UpdateDefeatPanelTexts();

            if (defeatReplayButton != null)
            {
                defeatReplayButton.onClick.RemoveAllListeners();
                defeatReplayButton.onClick.AddListener(OnReplayCurrentStageClicked);
            }

            if (defeatStageSelectButton != null)
            {
                defeatStageSelectButton.onClick.RemoveAllListeners();
                defeatStageSelectButton.onClick.AddListener(OnStageSelectClicked);
            }
        }

        private void OnReplayCurrentStageClicked()
        {
            if (defeatPanel != null)
                defeatPanel.SetActive(false);

            // 현재 스테이지를 그대로 다시 시작
            SceneManager.LoadScene(battleSceneName);
        }

        private void EnsureDefeatPanelBindings()
        {
            if (defeatPanel == null)
                defeatPanel = FindGameObjectIncludingInactive(defeatPanelName);
            if (defeatPanel == null) return;

            if (defeatReplayButton == null || defeatStageSelectButton == null)
            {
                var buttons = defeatPanel.GetComponentsInChildren<Button>(true);
                foreach (var b in buttons)
                {
                    if (b == null) continue;
                    string n = b.gameObject.name;
                    if (defeatReplayButton == null &&
                        (n.Contains("다시") || n.Contains("Replay") || n.Contains("retry") || n.Contains("Retry")))
                    {
                        defeatReplayButton = b;
                        continue;
                    }
                    if (defeatStageSelectButton == null &&
                        (n.Contains("스테이지") || n.Contains("Stage") || n.Contains("Select")))
                    {
                        defeatStageSelectButton = b;
                    }
                }

                // 이름으로 못 찾은 경우 첫/둘째 버튼으로 폴백
                if (defeatReplayButton == null && buttons.Length > 0)
                    defeatReplayButton = buttons[0];
                if (defeatStageSelectButton == null && buttons.Length > 1)
                    defeatStageSelectButton = buttons[1];
            }

            if (defeatScoreText == null)
                defeatScoreText = FindChildComponent<TextMeshProUGUI>(defeatPanel.transform, "ClearScoreText");
            if (defeatGoldText == null)
                defeatGoldText = FindChildComponent<TextMeshProUGUI>(defeatPanel.transform, "ClearGoldText");
            if (defeatTurnsText == null)
                defeatTurnsText = FindChildComponent<TextMeshProUGUI>(defeatPanel.transform, "ClearTurnsText");
            if (defeatEnhancedSummonText == null)
                defeatEnhancedSummonText = FindChildComponent<TextMeshProUGUI>(defeatPanel.transform, "ClearEnhancedSummonText");

            if (defeatSwordDamageText == null)
                defeatSwordDamageText = FindChildComponent<TextMeshProUGUI>(defeatPanel.transform, "ClearSwordDamageText");
            if (defeatBowDamageText == null)
                defeatBowDamageText = FindChildComponent<TextMeshProUGUI>(defeatPanel.transform, "ClearBowDamageText");
            if (defeatWandDamageText == null)
                defeatWandDamageText = FindChildComponent<TextMeshProUGUI>(defeatPanel.transform, "ClearWandDamageText");
            if (defeatCrossHealText == null)
                defeatCrossHealText = FindChildComponent<TextMeshProUGUI>(defeatPanel.transform, "ClearCrossHealText");

            if (defeatRandomCharacterImage == null)
                defeatRandomCharacterImage = FindChildComponent<Image>(defeatPanel.transform, "ClearRandomCharacterImage");
        }

        private void EnsureInputCaptureForPanel(GameObject panel)
        {
            if (panel == null) return;

            var rt = panel.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            var group = panel.GetComponent<CanvasGroup>();
            if (group == null)
                group = panel.AddComponent<CanvasGroup>();
            group.interactable = true;
            group.blocksRaycasts = true;
            group.ignoreParentGroups = false;

            var blocker = panel.GetComponent<Image>();
            if (blocker == null)
                blocker = panel.AddComponent<Image>();
            blocker.raycastTarget = true;
        }

        private void UpdateDefeatPanelTexts()
        {
            if (defeatScoreText != null && scoreManager != null)
                defeatScoreText.text = $"패배...  (Score: {scoreManager.CurrentScore:N0})";

            if (defeatGoldText != null)
                defeatGoldText.text = "획득 골드: 0G";

            int turnsUsed = levelManager != null ? levelManager.MovesUsed : 0;
            if (defeatTurnsText != null)
                defeatTurnsText.text = $"사용 턴 수: {turnsUsed}턴";

            if (defeatEnhancedSummonText != null)
                defeatEnhancedSummonText.text = $"강화타일 소환(4매치+): {BattleClearStatsRuntime.EnhancedSummonCount:N0}회";

            if (defeatSwordDamageText != null)
                defeatSwordDamageText.text = $"검 데미지: {BattleClearStatsRuntime.DamageSword:N0}";
            if (defeatBowDamageText != null)
                defeatBowDamageText.text = $"활 데미지: {BattleClearStatsRuntime.DamageBow:N0}";
            if (defeatWandDamageText != null)
                defeatWandDamageText.text = $"마법 데미지: {BattleClearStatsRuntime.DamageWand:N0}";
            if (defeatCrossHealText != null)
                defeatCrossHealText.text = $"십자가 회복: {BattleClearStatsRuntime.HealCross:N0}";

            if (defeatRandomCharacterImage != null)
            {
                var party = FindFirstObjectByType<PartyHealthUI>();
                Sprite sprite = null;
                if (party != null && party.CharacterCount > 0)
                {
                    int idx = Random.Range(0, party.CharacterCount);
                    sprite = party.GetPortraitSprite(idx);
                }

                defeatRandomCharacterImage.sprite = sprite;
                defeatRandomCharacterImage.gameObject.SetActive(sprite != null);
            }
        }

        private void ApplyClearRewardOnce()
        {
            if (_clearRewardApplied) return;
            _clearRewardApplied = true;

            int clearedIndex = BattleStageHolder.CurrentStageIndex;
            var saveData = SaveSystem.Load() ?? new GameSaveData();

            int clearGold = 0;
            var stageData = GetCurrentStageData();
            clearGold = stageData != null ? Mathf.Max(0, stageData.clearGoldReward) : 0;

            if (clearedIndex > saveData.lastClearedStageIndex)
            {
                saveData.lastClearedStageIndex = clearedIndex;
                saveData.lastClearedChapter = (clearedIndex / 3) + 1; // 기존 로직 유지

                // 스테이지 첫 클리어 골드 보상
                saveData.gold += clearGold;

                // 신규 스테이지 클리어 → 플레이어 레벨 +1, 업그레이드 포인트 +1
                saveData.playerLevel++;
                saveData.upgradePoints++;

                SaveSystem.Save(saveData);
                Debug.Log($"[UIManager] 스테이지 클리어 저장: 인덱스 {clearedIndex} / 골드 +{clearGold} / Lv {saveData.playerLevel} / 포인트 {saveData.upgradePoints}");
            }
            else
            {
                clearGold = 0; // 재클리어면 보상 표시 0
            }

            _goldEarnedThisClear = clearGold;
        }

        /// <summary>현재 스테이지의 StageData 반환</summary>
        private StageData GetCurrentStageData()
        {
            if (stageDatabase == null)
                stageDatabase = Resources.Load<StageDatabase>("StageDatabase");

            return stageDatabase?.GetStage(BattleStageHolder.CurrentStageIndex);
        }

        private void EnsureLevelCompletePanelUI()
        {
            if (_clearPanelRuntimeBuilt && levelCompletePanel != null) return;

            // 먼저 씬에 미리 배치된 Clear 패널을 이름 기반으로 바인딩(인스펙터 수정 가능 목적)
            if (TryBindClearPanelFromScene())
            {
                _clearPanelRuntimeBuilt = true;
                return;
            }

            // 이미 인스펙터 연결이 되어 있다면 그대로 사용
            if (levelCompletePanel != null &&
                clearGoldText != null &&
                clearTurnsText != null &&
                clearEnhancedSummonText != null &&
                nextLevelButton != null &&
                stageSelectButton != null &&
                clearRandomCharacterImage != null)
            {
                _clearPanelRuntimeBuilt = true;
                return;
            }

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[UIManager] ClearPanel 생성 실패: Canvas를 찾지 못했습니다.");
                return;
            }

            if (levelCompletePanel == null)
            {
                levelCompletePanel = new GameObject("LevelCompletePanel");
                levelCompletePanel.transform.SetParent(canvas.transform, false);

                var rt = levelCompletePanel.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                var overlay = levelCompletePanel.AddComponent<Image>();
                overlay.color = new Color(0f, 0f, 0f, 0.65f);
                overlay.raycastTarget = true; // 패널 위에서 입력 차단
            }

            // 박스(배경 + 레이아웃)
            Transform boxParent = levelCompletePanel.transform;
            var existingBox = boxParent.Find("ClearPanelBox_Runtime");
            GameObject box = existingBox != null ? existingBox.gameObject : new GameObject("ClearPanelBox_Runtime");
            if (existingBox == null)
            {
                box.transform.SetParent(boxParent, false);

                var boxRt = box.AddComponent<RectTransform>();
                boxRt.anchorMin = new Vector2(0.5f, 0.5f);
                boxRt.anchorMax = new Vector2(0.5f, 0.5f);
                boxRt.pivot = new Vector2(0.5f, 0.5f);
                boxRt.anchoredPosition = Vector2.zero;
                boxRt.sizeDelta = new Vector2(720f, 520f);

                var img = box.AddComponent<Image>();
                img.color = new Color(0.12f, 0.12f, 0.12f, 0.92f);

                var vlg = box.AddComponent<VerticalLayoutGroup>();
                vlg.padding = new RectOffset(20, 20, 20, 20);
                vlg.spacing = 10f;
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandHeight = false;
            }

            // Title
            if (levelCompleteScoreText == null)
                levelCompleteScoreText = CreateTMPText(box.transform, "클리어!", 34);

            // 캐릭터 이미지
            if (clearRandomCharacterImage == null)
            {
                GameObject charObj = new GameObject("ClearRandomCharacterImage");
                charObj.transform.SetParent(box.transform, false);

                var rt = charObj.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(220, 220);

                clearRandomCharacterImage = charObj.AddComponent<Image>();
                clearRandomCharacterImage.preserveAspect = true;
                clearRandomCharacterImage.raycastTarget = false;
            }

            // 스탯 텍스트들
            if (clearGoldText == null)
                clearGoldText = CreateTMPText(box.transform, "골드: 0G", 24);
            if (clearTurnsText == null)
                clearTurnsText = CreateTMPText(box.transform, "사용한 턴 수: 0", 24);
            if (clearEnhancedSummonText == null)
                clearEnhancedSummonText = CreateTMPText(box.transform, "강화타일 소환: 0회", 24);
            if (clearSwordDamageText == null)
                clearSwordDamageText = CreateTMPText(box.transform, "검 데미지: 0", 22);
            if (clearBowDamageText == null)
                clearBowDamageText = CreateTMPText(box.transform, "활 데미지: 0", 22);
            if (clearWandDamageText == null)
                clearWandDamageText = CreateTMPText(box.transform, "마법 데미지: 0", 22);
            if (clearCrossHealText == null)
                clearCrossHealText = CreateTMPText(box.transform, "십자가 회복: 0", 22);

            // 버튼 영역
            if (nextLevelButton == null || stageSelectButton == null)
            {
                Transform buttonsParent = EnsureButtonsRow(box.transform);
                if (nextLevelButton == null)
                    nextLevelButton = CreateButton(buttonsParent, "다음 스테이지 시작");
                if (stageSelectButton == null)
                    stageSelectButton = CreateButton(buttonsParent, "스테이지 선택으로");
            }

            _clearPanelRuntimeBuilt = true;
        }

        private bool TryBindClearPanelFromScene()
        {
            // Root
            var root = FindGameObjectIncludingInactive("LevelCompletePanel");
            if (root == null) return false;

            levelCompletePanel = root;

            // Texts / images / buttons
            clearRandomCharacterImage = FindChildComponent<Image>(root.transform, "ClearRandomCharacterImage");
            clearGoldText = FindChildComponent<TextMeshProUGUI>(root.transform, "ClearGoldText");
            clearTurnsText = FindChildComponent<TextMeshProUGUI>(root.transform, "ClearTurnsText");
            clearEnhancedSummonText = FindChildComponent<TextMeshProUGUI>(root.transform, "ClearEnhancedSummonText");

            clearSwordDamageText = FindChildComponent<TextMeshProUGUI>(root.transform, "ClearSwordDamageText");
            clearBowDamageText = FindChildComponent<TextMeshProUGUI>(root.transform, "ClearBowDamageText");
            clearWandDamageText = FindChildComponent<TextMeshProUGUI>(root.transform, "ClearWandDamageText");
            clearCrossHealText = FindChildComponent<TextMeshProUGUI>(root.transform, "ClearCrossHealText");

            levelCompleteScoreText = FindChildComponent<TextMeshProUGUI>(root.transform, "ClearScoreText");

            nextLevelButton = FindChildComponent<Button>(root.transform, "Button_다음 스테이지 시작");
            stageSelectButton = FindChildComponent<Button>(root.transform, "Button_스테이지 선택으로");

            return (levelCompleteScoreText != null &&
                    clearGoldText != null &&
                    clearTurnsText != null &&
                    clearEnhancedSummonText != null &&
                    clearSwordDamageText != null &&
                    clearBowDamageText != null &&
                    clearWandDamageText != null &&
                    clearCrossHealText != null &&
                    clearRandomCharacterImage != null &&
                    nextLevelButton != null &&
                    stageSelectButton != null);
        }

        private T FindChildComponent<T>(Transform parent, string childName) where T : Component
        {
            if (parent == null) return null;
            var all = parent.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].name == childName)
                    return all[i].GetComponent<T>();
            }
            return null;
        }

        private GameObject FindGameObjectIncludingInactive(string gameObjectName)
        {
            // GameObject.Find는 비활성 오브젝트를 찾지 못하므로,
            // Resources.FindObjectsOfTypeAll로 "비활성 포함" 탐색합니다.
            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < all.Length; i++)
            {
                var go = all[i];
                if (go == null) continue;
                if (go.name != gameObjectName) continue;
                if (!go.scene.IsValid()) continue; // 에셋 프리팹 같은 건 제외
                return go;
            }
            return null;
        }

        private Transform EnsureButtonsRow(Transform boxTransform)
        {
            var existing = boxTransform.Find("ClearPanelButtonsRow_Runtime");
            if (existing != null) return existing;

            GameObject row = new GameObject("ClearPanelButtonsRow_Runtime");
            row.transform.SetParent(boxTransform, false);

            var rt = row.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 60f);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.spacing = 10f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;

            return row.transform;
        }

        private TextMeshProUGUI CreateTMPText(Transform parent, string text, int fontSize)
        {
            GameObject obj = new GameObject($"TMPText_{text}");
            obj.transform.SetParent(parent, false);

            var rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, 32f);

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = false;

            // 글꼴이 프로젝트에 없을 경우 Unity 기본 TMP 글꼴로 표시될 수 있음
            return tmp;
        }

        private Button CreateButton(Transform parent, string label)
        {
            GameObject obj = new GameObject($"Button_{label}");
            obj.transform.SetParent(parent, false);

            var rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 50f);

            var img = obj.AddComponent<Image>();
            img.color = new Color(0.18f, 0.18f, 0.18f, 0.95f);

            var btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            var tmpObj = new GameObject("Text");
            tmpObj.transform.SetParent(obj.transform, false);
            var tmpRt = tmpObj.AddComponent<RectTransform>();
            tmpRt.anchorMin = Vector2.zero;
            tmpRt.anchorMax = Vector2.one;
            tmpRt.offsetMin = Vector2.zero;
            tmpRt.offsetMax = Vector2.zero;

            var tmp = tmpObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = false;

            return btn;
        }

        private void UpdateClearPanelTexts()
        {
            // scoreText는 기존 UI를 유지하려는 용도 (필요시 교체 가능)
            if (levelCompleteScoreText != null && scoreManager != null)
                levelCompleteScoreText.text = $"클리어!  (Score: {scoreManager.CurrentScore:N0})";

            if (clearGoldText != null)
                clearGoldText.text = $"획득 골드: {_goldEarnedThisClear:N0}G";

            int turnsUsed = levelManager != null ? levelManager.MovesUsed : 0;
            if (clearTurnsText != null)
                clearTurnsText.text = $"사용 턴 수: {turnsUsed}턴";

            if (clearEnhancedSummonText != null)
                clearEnhancedSummonText.text = $"강화타일 소환(4매치+): {BattleClearStatsRuntime.EnhancedSummonCount:N0}회";

            if (clearSwordDamageText != null)
                clearSwordDamageText.text = $"검 데미지: {BattleClearStatsRuntime.DamageSword:N0}";
            if (clearBowDamageText != null)
                clearBowDamageText.text = $"활 데미지: {BattleClearStatsRuntime.DamageBow:N0}";
            if (clearWandDamageText != null)
                clearWandDamageText.text = $"마법 데미지: {BattleClearStatsRuntime.DamageWand:N0}";
            if (clearCrossHealText != null)
                clearCrossHealText.text = $"십자가 회복: {BattleClearStatsRuntime.HealCross:N0}";

            if (clearRandomCharacterImage != null)
            {
                var party = FindFirstObjectByType<PartyHealthUI>();
                Sprite sprite = null;
                if (party != null && party.CharacterCount > 0)
                {
                    int idx = Random.Range(0, party.CharacterCount);
                    sprite = party.GetPortraitSprite(idx);
                }

                clearRandomCharacterImage.sprite = sprite;
                clearRandomCharacterImage.gameObject.SetActive(sprite != null);
            }
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
