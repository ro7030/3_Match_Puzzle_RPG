using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Story;

namespace MainMenu
{
    /// <summary>
    /// 뉴 게임 선택 시 표시되는 프롤로그 패널.
    /// 비주얼 노벨 스타일: 글자 타이핑 → 클릭 시 한 줄 완성 → 한 번 더 클릭 시 다음 텍스트.
    /// 스킵 버튼으로 즉시 게임 시작, 모든 텍스트 후 시작 버튼으로 게임 씬 로드.
    /// </summary>
    public class ProloguePanel : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI prologueText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button skipButton;
        [Tooltip("클릭으로 텍스트 진행할 영역. 없으면 PrologueText에 TextTyping만 사용")]
        [SerializeField] private Button clickAreaButton;

        [Header("Settings")]
        [TextArea(3, 10)]
        [SerializeField] private string prologueContent = "옛날 옛적에...\n한 편의 이야기가 시작되었다.";
        [SerializeField] private string gameSceneName = "GameScene";

        private TextTyping _textTyper;

        private void Awake()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);

            _textTyper = prologueText != null ? prologueText.GetComponent<TextTyping>() : null;
            if (_textTyper == null && prologueText != null)
                _textTyper = prologueText.gameObject.AddComponent<TextTyping>();

            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartClicked);
                startButton.gameObject.SetActive(false);
            }

            if (skipButton != null)
                skipButton.onClick.AddListener(OnSkipClicked);

            if (clickAreaButton != null && _textTyper != null)
                clickAreaButton.onClick.AddListener(_textTyper.OnClick);
        }

        /// <summary>
        /// 패널 표시 (메인 메뉴에서 New Game 클릭 시 호출)
        /// </summary>
        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            if (startButton != null)
                startButton.gameObject.SetActive(false);

            if (prologueText != null)
                prologueText.text = "";

            if (_textTyper != null && !string.IsNullOrEmpty(prologueContent))
            {
                string[] lines = prologueContent.Split(new[] { '\n' }, System.StringSplitOptions.None);
                _textTyper.Play(lines, OnPrologueComplete);
            }
            else if (prologueText != null && !string.IsNullOrEmpty(prologueContent))
            {
                prologueText.text = prologueContent;
                OnPrologueComplete();
            }
        }

        private void OnPrologueComplete()
        {
            if (startButton != null)
                startButton.gameObject.SetActive(true);
        }

        private void OnStartClicked()
        {
            StartGame();
        }

        private void OnSkipClicked()
        {
            StartGame();
        }

        private void StartGame()
        {
            SaveSystem.DeleteSave();
            Hide();
            SceneManager.LoadScene(gameSceneName);
        }

        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        public void SetPrologueText(string text)
        {
            prologueContent = text;
        }

        public void SetGameSceneName(string sceneName)
        {
            gameSceneName = sceneName;
        }
    }
}
