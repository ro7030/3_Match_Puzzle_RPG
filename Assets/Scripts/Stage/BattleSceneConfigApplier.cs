using UnityEngine;
using UnityEngine.UI;
using Match3Puzzle.Stage;
using Match3Puzzle.Level;
using Match3Puzzle.UI.Battle;

namespace Match3Puzzle.Stage
{
    /// <summary>
    /// 배틀 씬 로드 시, 선택된 스테이지에 맞게 배경·보스·턴·몬스터 체력 등을 적용.
    /// 배틀 씬에 하나만 두고, BattleStageHolder.CurrentStageIndex 또는 StageDatabase 참조로 설정.
    /// </summary>
    public class BattleSceneConfigApplier : MonoBehaviour
    {
        [SerializeField] private StageDatabase stageDatabase;
        [Header("배경")]
        [SerializeField] private Image backgroundImage;
        [Header("몬스터")]
        [SerializeField] private Image monsterImage;
        [SerializeField] private MonsterHealthUI monsterHealthUI;
        [Header("턴/난이도 (선택)")]
        [SerializeField] private LevelManager levelManager;

        private void Start()
        {
            ApplyStageConfig();
        }

        private void ApplyStageConfig()
        {
            if (stageDatabase == null)
                stageDatabase = Resources.Load<StageDatabase>("StageDatabase");

            int index = BattleStageHolder.CurrentStageIndex;
            StageData data = stageDatabase != null ? stageDatabase.GetStage(index) : null;

            Debug.Log($"[BattleConfig] 인덱스:{index} / DB:{stageDatabase != null} / Data:{data?.stageName ?? "NULL"} / BG:{data?.backgroundSprite?.name ?? "NULL"} / Boss:{data?.bossSprite?.name ?? "NULL"}");

            if (data == null)
            {
                Debug.LogWarning($"[BattleConfig] StageData가 null입니다. StageDatabase의 Element {index}를 확인하세요.");
                return;
            }

            if (backgroundImage != null && data.backgroundSprite != null)
                backgroundImage.sprite = data.backgroundSprite;

            if (monsterImage != null && data.bossSprite != null)
                monsterImage.sprite = data.bossSprite;

            if (monsterHealthUI != null)
                monsterHealthUI.SetHP(data.monsterMaxHp, data.monsterMaxHp);

            if (levelManager != null)
                levelManager.SetBattleConfig(data.maxTurns);
        }
    }
}
