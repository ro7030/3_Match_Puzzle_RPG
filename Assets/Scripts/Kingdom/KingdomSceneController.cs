using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Match3Puzzle.Kingdom
{
    public class KingdomSceneController : MonoBehaviour
    {
        [Header("버튼 연결")]
        [SerializeField] private Button statusButton;
        [SerializeField] private Button skillButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button mapButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button settingsButton;

        [Header("이동할 씬 이름")]
        [SerializeField] private string statusSceneName    = "StatusScene";
        [SerializeField] private string skillSceneName     = "SkillScene";
        [SerializeField] private string inventorySceneName = "InventoryScene";
        [SerializeField] private string mapSceneName       = "MapScene";
        [SerializeField] private string backSceneName      = "MapScene";

        private void Awake()
        {
            if (statusButton    != null) statusButton.onClick.AddListener(OnStatusButton);
            if (skillButton     != null) skillButton.onClick.AddListener(OnSkillButton);
            if (inventoryButton != null) inventoryButton.onClick.AddListener(OnInventoryButton);
            if (mapButton       != null) mapButton.onClick.AddListener(OnMapButton);
            if (backButton      != null) backButton.onClick.AddListener(OnBackButton);
            if (settingsButton  != null) settingsButton.onClick.AddListener(OnSettingsButton);
        }

        private void OnStatusButton()    => SceneManager.LoadScene(statusSceneName);
        private void OnSkillButton()     => SceneManager.LoadScene(skillSceneName);
        private void OnInventoryButton() => SceneManager.LoadScene(inventorySceneName);
        private void OnMapButton()       => SceneManager.LoadScene(mapSceneName);
        private void OnBackButton()      => SceneManager.LoadScene(backSceneName);
        private void OnSettingsButton()
        {
            Debug.Log("[KingdomScene] 설정 버튼 클릭");
        }
    }
}
