using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Story;

namespace MainMenu
{
    /// <summary>
    /// Start Scene 메인 메뉴 버튼 처리.
    /// New Game → 프롤로그 표시 / Continue → 저장 로드 후 게임 씬 / Option → 옵션 팝업 / End → 게임 종료
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button optionButton;
        [SerializeField] private Button endButton;

        [Header("Panels")]

        [SerializeField] private OptionsPanel optionsPanel;

        [Header("Scene Names")]
        [SerializeField] private string gameSceneName = "GameScene";

        [Header("No Save Popup (Optional)")]
        [SerializeField] private GameObject noSaveDataPopup;
        [SerializeField] private Button noSavePopupCloseButton;

        private void Awake()
        {
            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGameClicked);

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);

            if (optionButton != null)
                optionButton.onClick.AddListener(OnOptionClicked);

            if (endButton != null)
                endButton.onClick.AddListener(OnEndClicked);

            if (noSaveDataPopup != null)
                noSaveDataPopup.SetActive(false);

            if (noSavePopupCloseButton != null)
                noSavePopupCloseButton.onClick.AddListener(() => { if (noSaveDataPopup != null) noSaveDataPopup.SetActive(false); });
        }

       private void OnNewGameClicked()
        {
            // Inspector의 gameSceneName(PrologueScene 등)으로 이동
            SaveSystem.DeleteSave();
            CutsceneContext.Clear();                  // Inspector Dialogue Lines 사용하도록 보장
            SceneManager.LoadScene(string.IsNullOrEmpty(gameSceneName) ? "PrologueScene" : gameSceneName);
        }

        private void OnContinueClicked()
        {
            if (!SaveSystem.HasSaveData())
            {
                if (noSaveDataPopup != null)
                    noSaveDataPopup.SetActive(true);
                else
                    Debug.Log("[MainMenu] 저장된 데이터가 없습니다.");
                return;
            }

            GameSaveData data = SaveSystem.Load();
            if (data == null)
            {
                if (noSaveDataPopup != null)
                    noSaveDataPopup.SetActive(true);
                return;
            }

            // 로드한 데이터를 게임 씬에서 사용할 수 있도록 보관
            LoadedSaveDataHolder.Data = data;
            string sceneToLoad = string.IsNullOrEmpty(data.currentSceneName) ? gameSceneName : data.currentSceneName;
            LoadedSaveDataHolder.Data = data;
            SceneManager.LoadScene(sceneToLoad);
        }

        private void OnOptionClicked()
        {
            if (optionsPanel != null)
                optionsPanel.Open();
            else
                Debug.LogWarning("[MainMenu] OptionsPanel이 할당되지 않았습니다.");
        }

        private void OnEndClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    /// <summary>
    /// Continue 시 로드한 데이터를 게임 씬에서 참조하기 위한 홀더
    /// </summary>
    public static class LoadedSaveDataHolder
    {
        public static GameSaveData Data { get; set; }
    }
}
