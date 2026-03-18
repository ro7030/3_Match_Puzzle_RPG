using UnityEngine;
using Match3Puzzle.Skill;
using Match3Puzzle.Stage;
using Match3Puzzle.Stats;

namespace Match3Puzzle.UI.Battle
{
    /// <summary>
    /// 배틀 씬 시작 시, 캐릭터별 장착 스킬 4개를 SkillSlotUI 4칸에 적용.
    /// 스킬 버튼 클릭 시 SkillData의 effectType × effectMultiplier로 즉시 효과를 발동.
    /// 데미지/힐 수치: CharacterStatsData + CharacterUpgradeHolder / 저항: StageData
    /// </summary>
    public class BattleSkillBarInitializer : MonoBehaviour
    {
        [Header("DB")]
        [SerializeField] private SkillDatabase skillDatabase;
        [SerializeField] private StageDatabase stageDatabase;

        [Header("플레이어 스탯 (없으면 Resources/CharacterStats 자동 로드)")]
        [SerializeField] private CharacterStatsData characterStats;

        [Header("Slots (캐릭터 0~3 순서)")]
        [SerializeField] private SkillSlotUI[] slots = new SkillSlotUI[4];

        [Header("전투 참조 (자동 탐색, 없으면 직접 연결)")]
        [SerializeField] private MonsterHealthUI monsterHealthUI;
        [SerializeField] private PartyHealthUI partyHealthUI;

        private StageData _currentStageData;

        private void Awake()
        {
            if (monsterHealthUI == null)
                monsterHealthUI = FindFirstObjectByType<MonsterHealthUI>();
            if (partyHealthUI == null)
                partyHealthUI = FindFirstObjectByType<PartyHealthUI>();
            if (stageDatabase == null)
                stageDatabase = Resources.Load<StageDatabase>("StageDatabase");
            if (characterStats == null)
                characterStats = CharacterStatsResolver.LoadFromResources();
        }

        private void Start()
        {
            int index = BattleStageHolder.CurrentStageIndex;
            _currentStageData = stageDatabase != null ? stageDatabase.GetStage(index) : null;

            ApplyEquippedSkills();
        }

        private void ApplyEquippedSkills()
        {
            if (slots == null || slots.Length == 0 || skillDatabase == null)
                return;

            for (int i = 0; i < slots.Length && i < EquippedSkillsHolder.CharacterCount; i++)
            {
                SkillSlotUI slot = slots[i];
                if (slot == null) continue;

                string skillId = EquippedSkillsHolder.GetEquippedSkillId(i);
                SkillData data = null;
                if (!string.IsNullOrEmpty(skillId))
                    data = skillDatabase.GetById(skillId);
                if (data == null)
                    data = skillDatabase.GetByIndex(GetDefaultSkillIndex(i));

                if (data == null)
                {
                    slot.gameObject.SetActive(false);
                    continue;
                }

                slot.gameObject.SetActive(true);
                slot.Configure(data.skillId, data.icon, data.cooldownSeconds);

                // 클로저로 SkillData 캡처 후 OnSkillUsed에 효과 등록
                SkillData captured = data;
                slot.OnSkillUsed += () => ApplySkillEffect(captured);
            }
        }

        /// <summary>미장착 시 기본 스킬 인덱스: 캐릭터0=0, 캐릭터1=3, 캐릭터2=6, 캐릭터3=9</summary>
        private static int GetDefaultSkillIndex(int characterIndex) => characterIndex * 3;

        /// <summary>
        /// 스킬 버튼 클릭 시 즉시 발동되는 효과.
        /// StageData의 기본 데미지/힐 수치에 effectMultiplier를 곱해서 적용.
        /// </summary>
        private void ApplySkillEffect(SkillData data)
        {
            if (_currentStageData == null || characterStats == null)
            {
                Debug.LogWarning("[BattleSkillBarInitializer] StageData 또는 CharacterStats가 없어 스킬 효과를 적용할 수 없습니다.");
                return;
            }

            switch (data.effectType)
            {
                case SkillEffectType.Sword:
                {
                    if (monsterHealthUI == null) return;
                    int dmg = Mathf.RoundToInt(
                        CharacterStatsResolver.GetSwordDamage(characterStats) * data.effectMultiplier
                        * (1f - _currentStageData.swordResistance));
                    if (dmg > 0) monsterHealthUI.TakeDamage(dmg);
                    break;
                }
                case SkillEffectType.Bow:
                {
                    if (monsterHealthUI == null) return;
                    int dmg = Mathf.RoundToInt(
                        CharacterStatsResolver.GetBowDamage(characterStats) * data.effectMultiplier
                        * (1f - _currentStageData.bowResistance));
                    if (dmg > 0) monsterHealthUI.TakeDamage(dmg);
                    break;
                }
                case SkillEffectType.Wand:
                {
                    if (monsterHealthUI == null) return;
                    int dmg = Mathf.RoundToInt(
                        CharacterStatsResolver.GetWandDamage(characterStats) * data.effectMultiplier
                        * (1f - _currentStageData.wandResistance));
                    if (dmg > 0) monsterHealthUI.TakeDamage(dmg);
                    break;
                }
                case SkillEffectType.Heal:
                {
                    if (partyHealthUI == null) return;
                    int heal = Mathf.RoundToInt(
                        CharacterStatsResolver.GetHealAmount(characterStats) * data.effectMultiplier);
                    if (heal <= 0) return;
                    for (int i = 0; i < partyHealthUI.CharacterCount; i++)
                    {
                        if (partyHealthUI.IsAlive(i))
                            partyHealthUI.Heal(i, heal);
                    }
                    break;
                }
            }
        }
    }
}
