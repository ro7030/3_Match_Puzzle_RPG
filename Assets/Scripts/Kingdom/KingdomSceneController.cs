using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MainMenu;
using Match3Puzzle.Skill;
using Match3Puzzle.Inventory;
using Match3Puzzle.Stats;

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

        private void Start()
        {
            // Continue로 KingdomScene에 진입했을 때,
            // GameSaveApplier 컴포넌트가 씬에 없더라도 최소 복원을 보장한다.
            var data = LoadedSaveDataHolder.Data;
            if (data == null) return;

            Match3Puzzle.Skill.EquippedSkillsHolder.LoadFromSave(data);
            Match3Puzzle.Inventory.EquippedEquipmentHolder.LoadFromSave(data);
            CharacterUpgradeHolder.LoadFromSave(data);

            LoadedSaveDataHolder.Data = null;
        }
    }
}
