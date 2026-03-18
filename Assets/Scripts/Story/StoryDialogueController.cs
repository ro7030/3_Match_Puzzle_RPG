using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using MainMenu;

namespace Story
{
    /// <summary>
    /// 스토리/프롤로그 씬 대화 컨트롤러.
    /// - 타이핑, 스킵, 씬 전환 지원
    /// - CutsceneData + CharacterDatabase (캐릭터 재사용)
    /// - DataDialogue 스타일 병렬 리스트 연동 가능
    /// </summary>
    public class StoryDialogueController : MonoBehaviour
    {
        [Header("Dialogue UI")]
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private Button clickAreaButton;

        [Header("Background & Character")]
        [SerializeField] private UnityEngine.UI.Image backgroundImage;
        [SerializeField] private UnityEngine.UI.Image characterImage;

        [Header("Data")]
        [Tooltip("CutsceneData 재생 시 캐릭터 조회에 사용. 필수")]
        [SerializeField] private CharacterDatabase characterDatabase;

        [Header("VN Text Typer")]
        [SerializeField] private VNTextTyper textTyper;

        [Header("UI Buttons")]
        [SerializeField] private Button skipButton;
        [SerializeField] private Button scriptButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button uiToggleButton;

        [Header("UI Hide / Restore")]
        [Tooltip("UI 숨김 시 화면에 표시할 투명 전체화면 패널. Canvas 직속 자식으로 배치, DialogueBox 밖에 있어야 함")]
        [SerializeField] private GameObject uiRestorePanel;

        [Header("Script Panel")]
        [SerializeField] private GameObject scriptPanel;
        [SerializeField] private ScrollRect scriptScrollRect;
        [SerializeField] private TextMeshProUGUI scriptScrollText;
        [SerializeField] private Button scriptCloseButton;

        [Header("Settings (Inspector 모드 기본값)")]
        [SerializeField] private string nextSceneName = "GameScene";
        [SerializeField] private bool isPrologueMode = true;

        [Header("Inspector Dialogue Lines (CutsceneContext 없을 때 사용)")]
        [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();

        private int _currentIndex;
        private Action _onDialogueComplete;
        private string _runtimeNextSceneName;
        private bool? _runtimeIsPrologueMode;
        private bool? _runtimeClearSave;
        private Sprite _defaultBackgroundSprite;
        private bool _uiHidden;
        private readonly List<string> _dialogueHistory = new List<string>();

        [Serializable]
        public class DialogueLine
        {
            public string speakerName = "";
            public string text = "";
            public Sprite characterSprite;
            public Sprite backgroundSprite;
        }

        private void Awake()
        {
            if (backgroundImage != null)
                _defaultBackgroundSprite = backgroundImage.sprite;

            EnsureTextTyper();
            if (textTyper != null && clickAreaButton != null)
                clickAreaButton.onClick.AddListener(textTyper.OnClick);

            if (skipButton != null) skipButton.onClick.AddListener(OnSkipClicked);
            if (scriptButton != null) scriptButton.onClick.AddListener(OnScriptClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
            if (uiToggleButton != null) uiToggleButton.onClick.AddListener(OnUIToggleClicked);
            if (scriptCloseButton != null) scriptCloseButton.onClick.AddListener(CloseScriptPanel);
            if (scriptPanel != null) scriptPanel.SetActive(false);
            if (uiRestorePanel != null)
            {
                var btn = uiRestorePanel.GetComponent<Button>();
                if (btn == null) btn = uiRestorePanel.AddComponent<Button>();
                btn.onClick.AddListener(OnClickAreaRestoreUI);
                uiRestorePanel.SetActive(false);
            }
        }

        private void EnsureTextTyper()
        {
            if (textTyper != null) return;
            if (dialogueText == null) return;
            textTyper = dialogueText.GetComponent<VNTextTyper>();
            if (textTyper == null) textTyper = dialogueText.gameObject.AddComponent<VNTextTyper>();
        }

        #region 재생 시작 API

        /// <summary>CutsceneData로 재생 (씬 전환 옵션 포함)</summary>
        public void PlayCutscene(CutsceneData cutscene, Action onComplete = null)
        {
            if (cutscene == null || cutscene.dialogueLines == null || cutscene.dialogueLines.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }
            var lines = cutscene.ToDialogueLines(characterDatabase);
            PlayDialogue(lines, onComplete, cutscene.nextSceneName, cutscene.isPrologueMode, cutscene.clearSaveOnComplete);
        }

        /// <summary>캐릭터ID·텍스트 병렬 리스트로 재생 (DataDialogue 연동용)</summary>
        public void PlayFromParallelLists(List<int> characterIds, List<string> texts,
            Action onComplete = null, string nextScene = null, bool? isPrologue = null)
        {
            var lines = CutsceneData.FromParallelLists(characterIds, texts, characterDatabase);
            PlayDialogue(lines, onComplete, nextScene, isPrologue, null);
        }

        /// <summary>대화 라인 리스트로 재생</summary>
        public void PlayDialogue(List<DialogueLine> lines, Action onComplete = null,
            string nextSceneOverride = null, bool? isPrologueOverride = null, bool? clearSaveOverride = null)
        {
            dialogueLines = lines ?? new List<DialogueLine>();
            _currentIndex = 0;
            _onDialogueComplete = onComplete;
            _runtimeNextSceneName = nextSceneOverride;
            _runtimeIsPrologueMode = isPrologueOverride;
            _runtimeClearSave = clearSaveOverride;
            ShowCurrentLine();
        }

        /// <summary>Inspector Dialogue Lines로 재생</summary>
        public void PlayFromInspector(Action onComplete = null)
        {
            _onDialogueComplete = onComplete;
            _currentIndex = 0;
            ShowCurrentLine();
        }

        #endregion

        private void ShowCurrentLine()
        {
            if (_currentIndex >= dialogueLines.Count)
            {
                OnAllDialogueComplete();
                return;
            }

            var line = dialogueLines[_currentIndex];
            if (speakerNameText != null) speakerNameText.text = line.speakerName ?? "";
            if (backgroundImage != null)
            {
                var targetSprite = line.backgroundSprite != null ? line.backgroundSprite : _defaultBackgroundSprite;
                backgroundImage.sprite = targetSprite;
                backgroundImage.enabled = true;
                backgroundImage.gameObject.SetActive(true);
            }
            if (characterImage != null)
            {
                characterImage.gameObject.SetActive(line.characterSprite != null);
                if (line.characterSprite != null) characterImage.sprite = line.characterSprite;
            }

            if (!string.IsNullOrWhiteSpace(line.text))
                AddToHistory(line.speakerName, line.text);

            EnsureTextTyper();
            if (textTyper != null)
            {
                if (string.IsNullOrWhiteSpace(line.text))
                {
                    // 빈/공백/줄바꿈만 있는 줄: 화자/배경만 표시, 클릭 시 다음 줄로 (즉시 콜백으로 한 칸 밀림 방지)
                    if (dialogueText != null) dialogueText.text = "";
                    textTyper.Clear();
                    void OnClickOnce()
                    {
                        if (clickAreaButton != null) clickAreaButton.onClick.RemoveListener(OnClickOnce);
                        _currentIndex++;
                        ShowCurrentLine();
                    }
                    if (clickAreaButton != null) clickAreaButton.onClick.AddListener(OnClickOnce);
                }
                else
                {
                    textTyper.Play(line.text, () => { _currentIndex++; ShowCurrentLine(); });
                }
            }
            else if (dialogueText != null)
            {
                dialogueText.text = line.text ?? "";
                _currentIndex++;
                ShowCurrentLine();
            }
        }

        private void OnScriptClicked()
        {
            if (scriptPanel == null) return;

            if (scriptScrollText != null)
            {
                scriptScrollText.text = _dialogueHistory.Count == 0
                    ? "(아직 대화 내역이 없습니다)"
                    : string.Join("\n\n", _dialogueHistory);

                // TMP가 텍스트 높이를 즉시 계산하도록 강제
                scriptScrollText.ForceMeshUpdate();

                // Content sizeDelta.y를 텍스트 preferredHeight로 직접 설정
                if (scriptScrollRect != null)
                {
                    var content = scriptScrollRect.content;
                    if (content != null)
                    {
                        Vector2 size = content.sizeDelta;
                        size.y = scriptScrollText.preferredHeight;
                        content.sizeDelta = size;
                    }
                    // 스크롤을 가장 위(오래된 대화)로 초기화
                    scriptScrollRect.verticalNormalizedPosition = 1f;
                }
            }

            if (clickAreaButton != null) clickAreaButton.gameObject.SetActive(false);
            scriptPanel.SetActive(true);
        }

        private void CloseScriptPanel()
        {
            if (scriptPanel != null) scriptPanel.SetActive(false);
            if (clickAreaButton != null) clickAreaButton.gameObject.SetActive(true);
        }

        private void AddToHistory(string speaker, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            var entry = string.IsNullOrWhiteSpace(speaker)
                ? text.Trim()
                : $"【{speaker.Trim()}】\n{text.Trim()}";
            _dialogueHistory.Add(entry);
        }

        private void OnSettingsClicked() => Debug.Log("[Story] 설정");

        private void OnUIToggleClicked()
        {
            SetUIVisible(false);
        }

        private void SetUIVisible(bool visible)
        {
            _uiHidden = !visible;

            if (dialogueBox != null) dialogueBox.SetActive(visible);
            if (skipButton != null) skipButton.gameObject.SetActive(visible);
            if (scriptButton != null) scriptButton.gameObject.SetActive(visible);
            if (settingsButton != null) settingsButton.gameObject.SetActive(visible);
            if (uiToggleButton != null) uiToggleButton.gameObject.SetActive(visible);

            // UI가 숨겨진 동안에만 복원용 투명 패널 활성화
            if (uiRestorePanel != null) uiRestorePanel.SetActive(!visible);
        }

        private void OnClickAreaRestoreUI()
        {
            SetUIVisible(true);
        }

        private string EffectiveNextScene => !string.IsNullOrEmpty(_runtimeNextSceneName) ? _runtimeNextSceneName : nextSceneName;
        private bool EffectiveIsPrologueMode => _runtimeIsPrologueMode ?? isPrologueMode;
        private bool EffectiveClearSave => _runtimeClearSave ?? isPrologueMode;

        private void OnSkipClicked()
        {
            if (EffectiveIsPrologueMode && !string.IsNullOrEmpty(EffectiveNextScene))
            {
                if (EffectiveClearSave) SaveSystem.DeleteSave();
                SceneManager.LoadScene(EffectiveNextScene);
            }
            else _onDialogueComplete?.Invoke();
        }

        private void OnAllDialogueComplete()
        {
            if (EffectiveIsPrologueMode && !string.IsNullOrEmpty(EffectiveNextScene))
            {
                if (EffectiveClearSave) SaveSystem.DeleteSave();
                SceneManager.LoadScene(EffectiveNextScene);
            }
            else _onDialogueComplete?.Invoke();
        }

        private void Start()
        {
            if (CutsceneContext.HasNext)
            {
                LoadAndPlayFromContext();
                return;
            }
            if (dialogueLines != null && dialogueLines.Count > 0)
                PlayFromInspector();
        }

        private void LoadAndPlayFromContext()
        {
            var so = Resources.Load<CutsceneData>($"{CutsceneContext.ScriptableObjectPathPrefix}{CutsceneContext.CutsceneId}");
            CutsceneContext.Clear();

            if (so != null && so.dialogueLines != null && so.dialogueLines.Count > 0)
                PlayCutscene(so);
        }
    }
}
