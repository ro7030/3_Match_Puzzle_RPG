using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using MainMenu;
using Match3Puzzle.Stage;
using Story;

namespace Match3Puzzle.Map
{
    /// <summary>
    /// MapScene 위에 떠 있는 스테이지 선택 패널.
    /// 챕터 노드를 클릭하면 Show(), 닫기 버튼이나 바깥 클릭 시 Hide().
    /// </summary>
    public class StageSelectPanel : MonoBehaviour
    {
        [Header("패널 루트 (활성/비활성으로 표시 제어)")]
        [SerializeField] private GameObject panelRoot;

        [Header("챕터 제목 텍스트 (선택 사항)")]
        [SerializeField] private TMP_Text chapterTitleText;

        [Header("스테이지 버튼 3개 (순서: Stage1 → Stage2 → Stage3)")]
        [SerializeField] private Button stage1Button;
        [SerializeField] private Button stage2Button;
        [SerializeField] private Button stage3Button;

        [Header("스테이지 버튼 텍스트 (선택 사항)")]
        [SerializeField] private TMP_Text stage1Text;
        [SerializeField] private TMP_Text stage2Text;
        [SerializeField] private TMP_Text stage3Text;

        [Header("닫기 버튼 (선택 사항 — 패널 바깥 배경 버튼도 사용 가능)")]
        [SerializeField] private Button closeButton;

        [Header("이동할 씬 이름")]
        [SerializeField] private string battleSceneName = "BattleScene";
        [SerializeField] private string storySceneName = "StoryScene";

        // ──────────────────────────────────────────────────────────────
        // 내부 상태
        // ──────────────────────────────────────────────────────────────

        private Button[]   _stageButtons;
        private TMP_Text[] _stageTexts;
        private int      _currentChapter;
        private GameSaveData _saveData;

        private void Awake()
        {
            _stageButtons = new[] { stage1Button, stage2Button, stage3Button };
            _stageTexts   = new[] { stage1Text,   stage2Text,   stage3Text   };

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            // 시작 시 패널 숨김
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        // ──────────────────────────────────────────────────────────────
        // 공개 API
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// 챕터 노드 클릭 시 MapSceneController에서 호출.
        /// </summary>
        /// <param name="chapterNumber">선택된 챕터 번호 (1~3)</param>
        /// <param name="saveData">현재 세이브 데이터 (스테이지 해금 판정에 사용)</param>
        public void Show(int chapterNumber, GameSaveData saveData)
        {
            _currentChapter = chapterNumber;
            _saveData = saveData;

            // 패널을 먼저 활성화해야 Awake()가 실행되어 배열이 초기화됨
            if (panelRoot != null)
                panelRoot.SetActive(true);

            UpdateChapterTitle(chapterNumber);
            SetupStageButtons();
        }

        /// <summary>패널 닫기. 닫기 버튼 On Click() 에 직접 연결 가능.</summary>
        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        // ──────────────────────────────────────────────────────────────
        // 내부 로직
        // ──────────────────────────────────────────────────────────────

        private void UpdateChapterTitle(int chapterNumber)
        {
            if (chapterTitleText == null) return;
            chapterTitleText.text = chapterNumber switch
            {
                1 => "Monster Forest",
                2 => "Ancient Legendary Ruins",
                3 => "Dragon's Fortress",
                _ => $"Chapter {chapterNumber}"
            };
        }

        private void SetupStageButtons()
        {
            // Awake가 아직 안 불렸을 경우 직접 초기화 (방어 코드)
            if (_stageButtons == null)
            {
                _stageButtons = new[] { stage1Button, stage2Button, stage3Button };
                _stageTexts   = new[] { stage1Text,   stage2Text,   stage3Text   };
            }

            int firstIndex  = ChapterHolder.GetFirstStageIndex(_currentChapter); // 챕터1→0, 챕터2→3, 챕터3→6
            int lastCleared = _saveData?.lastClearedStageIndex ?? -1;

            for (int i = 0; i < _stageButtons.Length; i++)
            {
                if (_stageButtons[i] == null) continue;

                int globalIndex = firstIndex + i; // 전역 스테이지 인덱스 (0~8)
                int stageNumber = i + 1;          // 스테이지 번호 표시용 (1~3)

                // 해금 조건: 챕터 내 첫 스테이지는 항상 열림 / 이후는 바로 앞 스테이지 클리어 필요
                bool isUnlocked = (i == 0) || (lastCleared >= globalIndex - 1);

                _stageButtons[i].interactable = isUnlocked;

                if (_stageTexts[i] != null)
                    _stageTexts[i].text = isUnlocked
                        ? $"스테이지 {stageNumber}"
                        : $"스테이지 {stageNumber}  🔒";

                // 이전 리스너 제거 후 재등록 (Show가 여러 번 호출될 때 중복 방지)
                int captured = globalIndex;
                _stageButtons[i].onClick.RemoveAllListeners();
                if (isUnlocked)
                    _stageButtons[i].onClick.AddListener(() => EnterStage(captured));
            }
        }

        private void EnterStage(int globalStageIndex)
        {
            Debug.Log($"[StageSelectPanel] 스테이지 진입: 전역 인덱스 {globalStageIndex} (챕터{_currentChapter} Stage{(globalStageIndex - 1) % 3 + 1})");
            BattleStageHolder.SetStage(globalStageIndex);

            // 스테이지 시작 인트로 컷신 재생 후 배틀로 진입
            var stageDatabase = Resources.Load<StageDatabase>("StageDatabase");
            var stageData = stageDatabase != null ? stageDatabase.GetStage(globalStageIndex) : null;
            var introCutscene = stageData != null ? stageData.introCutscene : null;
            // CutsceneData.cutsceneId를 비워두는 실수가 있어, 비어있으면 asset name으로 자동 보정한다.
            string cutsceneId = introCutscene != null
                ? (!string.IsNullOrEmpty(introCutscene.cutsceneId) ? introCutscene.cutsceneId : introCutscene.name)
                : null;

            Debug.Log(
                $"[StageSelectPanel] introCutscene check: " +
                $"stageData={(stageData != null ? stageData.stageName : "NULL")} " +
                $"introCutscene={(introCutscene != null ? introCutscene.name : "NULL")} " +
                $"resolvedCutsceneId={(string.IsNullOrEmpty(cutsceneId) ? "NULL/EMPTY" : cutsceneId)}"
            );

            if (!string.IsNullOrEmpty(cutsceneId))
            {
                CutsceneContext.SetNextForBattle(cutsceneId, () =>
                {
                    SceneManager.LoadScene(battleSceneName);
                });
                SceneManager.LoadScene(storySceneName);
                return;
            }

            SceneManager.LoadScene(battleSceneName);
        }
    }
}
