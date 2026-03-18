using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MainMenu;

namespace Match3Puzzle.Map
{
    /// <summary>
    /// MapScene 전체를 관리하는 컨트롤러.
    ///
    /// ─ 챕터 해금 조건 ─────────────────────────────────────────
    ///  챕터1 (Monster Forest)         : 항상 해금
    ///  챕터2 (Ancient Legendary Ruins): lastClearedStageIndex >= 2  (챕터1 스테이지3 클리어)
    ///  챕터3 (Dragon's Fortress)      : lastClearedStageIndex >= 5  (챕터2 스테이지3 클리어)
    /// ────────────────────────────────────────────────────────────
    ///
    /// ─ 전역 스테이지 인덱스 매핑 ──────────────────────────────
    ///  Index 0          = Tutorial
    ///  Index 1, 2, 3    = 챕터1 Stage1, Stage2, Stage3
    ///  Index 4, 5, 6    = 챕터2 Stage1, Stage2, Stage3
    ///  Index 7, 8, 9    = 챕터3 Stage1, Stage2, Stage3
    /// ────────────────────────────────────────────────────────────
    /// </summary>
    public class MapSceneController : MonoBehaviour
    {
        // ──────────────────────────────────────────────────────────────
        // Inspector 연결
        // ──────────────────────────────────────────────────────────────

        [Header("챕터 노드 (ChapterNodeUI 컴포넌트가 붙은 오브젝트 연결)")]
        [SerializeField] private ChapterNodeUI chapter1Node; // Monster Forest
        [SerializeField] private ChapterNodeUI chapter2Node; // Ancient Legendary Ruins
        [SerializeField] private ChapterNodeUI chapter3Node; // Dragon's Fortress

        [Header("스테이지 선택 패널")]
        [Tooltip("MapScene 위에 떠 있는 스테이지 선택 패널 오브젝트")]
        [SerializeField] private StageSelectPanel stageSelectPanel;

        [Header("이동할 씬 이름")]
        [SerializeField] private string kingdomSceneName = "KingdomScene";

        [Header("킹덤/뒤로가기 버튼")]
        [Tooltip("KingdomScene으로 이동하는 버튼. On Click() 이벤트로 OnKingdomButton을 직접 연결해도 됩니다.")]
        [SerializeField] private Button kingdomButton;
        [Tooltip("뒤로가기 버튼. kingdomButton과 동일한 동작 (선택 사항 — 둘 중 하나만 써도 무방).")]
        [SerializeField] private Button backButton;

        // ──────────────────────────────────────────────────────────────
        // 내부 상태
        // ──────────────────────────────────────────────────────────────

        private GameSaveData _saveData;

        // ──────────────────────────────────────────────────────────────
        // 챕터 해금 조건 상수
        // ──────────────────────────────────────────────────────────────

        /// <summary>챕터2를 해금하는 데 필요한 최소 전역 스테이지 인덱스 (챕터1 Stage3 = 3)</summary>
        private const int UnlockCh2RequiredIndex = 3;

        /// <summary>챕터3를 해금하는 데 필요한 최소 전역 스테이지 인덱스 (챕터2 Stage3 = 6)</summary>
        private const int UnlockCh3RequiredIndex = 6;

        // ──────────────────────────────────────────────────────────────
        // Unity 생명주기
        // ──────────────────────────────────────────────────────────────

        private void Start()
        {
            _saveData = SaveSystem.Load() ?? new GameSaveData();
            InitializeChapterNodes();

            if (kingdomButton != null)
                kingdomButton.onClick.AddListener(OnKingdomButton);

            if (backButton != null)
                backButton.onClick.AddListener(OnKingdomButton);
        }

        // ──────────────────────────────────────────────────────────────
        // 챕터 노드 초기화
        // ──────────────────────────────────────────────────────────────

        private void InitializeChapterNodes()
        {
            int lastCleared = _saveData.lastClearedStageIndex;

            // 챕터1: 항상 해금
            if (chapter1Node != null)
                chapter1Node.Initialize(1, true, () => OnChapterClicked(1));

            // 챕터2: 챕터1 스테이지3 클리어 여부
            bool ch2Unlocked = lastCleared >= UnlockCh2RequiredIndex;
            if (chapter2Node != null)
                chapter2Node.Initialize(2, ch2Unlocked, () => OnChapterClicked(2));

            // 챕터3: 챕터2 스테이지3 클리어 여부
            bool ch3Unlocked = lastCleared >= UnlockCh3RequiredIndex;
            if (chapter3Node != null)
                chapter3Node.Initialize(3, ch3Unlocked, () => OnChapterClicked(3));

            Debug.Log($"[MapScene] 마지막 클리어 스테이지 인덱스: {lastCleared} | " +
                      $"Ch2 해금: {ch2Unlocked} | Ch3 해금: {ch3Unlocked}");
        }

        // ──────────────────────────────────────────────────────────────
        // 버튼 콜백
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// 챕터 클릭 시 호출. 스테이지 선택 패널을 열어 해당 챕터의 스테이지를 표시.
        /// </summary>
        private void OnChapterClicked(int chapterNumber)
        {
            Debug.Log($"[MapScene] 챕터 {chapterNumber} 선택");
            ChapterHolder.CurrentChapter = chapterNumber;

            if (stageSelectPanel != null)
                stageSelectPanel.Show(chapterNumber, _saveData);
        }

        /// <summary>킹덤 버튼 / 뒤로가기 → KingdomScene</summary>
        public void OnKingdomButton()
        {
            SceneManager.LoadScene(kingdomSceneName);
        }

        /// <summary>OnKingdomButton의 별칭. 유니티 On Click() 이벤트에서 직접 연결 시 사용 가능.</summary>
        public void OnBackButton() => OnKingdomButton();

        // ──────────────────────────────────────────────────────────────
        // 런타임 동적 해금 (스테이지 클리어 후 MapScene이 유지될 경우 호출)
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// 스테이지 클리어 직후 MapScene으로 돌아왔을 때 세이브 데이터를 다시 읽어 노드를 갱신.
        /// </summary>
        public void RefreshUnlockStates()
        {
            _saveData = SaveSystem.Load() ?? new GameSaveData();
            InitializeChapterNodes();
        }
    }
}
