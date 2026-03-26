using UnityEngine;
using UnityEngine.SceneManagement;
using Match3Puzzle.Stats;

namespace MainMenu
{
    /// <summary>
    /// 게임 씬에 배치. Continue로 진입했을 때 LoadedSaveDataHolder.Data를 적용하고,
    /// 필요 시 챕터/골드/스킬 등 게임 로직에 반영.
    /// </summary>
    public class GameSaveApplier : MonoBehaviour
    {
        [SerializeField] private bool applyOnStart = true;

        private void Start()
        {
            if (applyOnStart)
                ApplyLoadedSave();
        }

        /// <summary>
        /// Continue로 들어왔을 때 저장된 데이터 적용
        /// </summary>
        public void ApplyLoadedSave()
        {
            GameSaveData data = LoadedSaveDataHolder.Data;
            if (data == null) return;

            // 장착 스킬 복원 (배틀/스킬씬에서 사용)
            Match3Puzzle.Skill.EquippedSkillsHolder.LoadFromSave(data);
            // 장착 장비 복원 (배틀/인벤토리씬에서 사용)
            Match3Puzzle.Inventory.EquippedEquipmentHolder.LoadFromSave(data);
            // 현재 스탯 업그레이드 레벨 복원 (배틀 등에서 CharacterUpgradeHolder를 참조하는 경우 대비)
            CharacterUpgradeHolder.LoadFromSave(data);

            Debug.Log($"[GameSaveApplier] 적용 - 챕터:{data.lastClearedChapter} 골드:{data.gold} 씬:{data.currentSceneName}");

            LoadedSaveDataHolder.Data = null; // 한 번 적용 후 비워도 됨
        }
    }
}
