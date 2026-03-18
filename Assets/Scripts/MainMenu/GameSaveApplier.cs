using UnityEngine;
using UnityEngine.SceneManagement;

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

            Debug.Log($"[GameSaveApplier] 적용 - 챕터:{data.lastClearedChapter} 골드:{data.gold} 씬:{data.currentSceneName}");

            LoadedSaveDataHolder.Data = null; // 한 번 적용 후 비워도 됨
        }
    }
}
