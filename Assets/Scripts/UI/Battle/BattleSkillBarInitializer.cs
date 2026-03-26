using UnityEngine;
using Match3Puzzle.Skill;
using Match3Puzzle.Stage;
using Match3Puzzle.Stats;
using Match3Puzzle.Inventory;
using Match3Puzzle.Clear;
using Match3Puzzle.Board;
using Match3Puzzle.Matching;
using System.Collections.Generic;
using System;
using System.Linq;
using Match3Puzzle.Level;
using System.Collections;

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

        [Header("턴 소스 (없으면 자동 탐색)")]
        [SerializeField] private LevelManager levelManager;

        [Header("매치 소스 (쓸 때마다 쿨다운 감소)")]
        [SerializeField] private TileClearer tileClearer;

        [Header("Slots (캐릭터 0~3 순서)")]
        [SerializeField] private SkillSlotUI[] slots = new SkillSlotUI[4];

        [Header("전투 참조 (자동 탐색, 없으면 직접 연결)")]
        [SerializeField] private MonsterHealthUI monsterHealthUI;
        [SerializeField] private PartyHealthUI partyHealthUI;

        private StageData _currentStageData;
        private readonly Action[] _slotHandlers = new Action[EquippedSkillsHolder.CharacterCount];
        private readonly SkillEffectType[] _slotEffectTypes = new SkillEffectType[EquippedSkillsHolder.CharacterCount];
        private TileClearer _subscribedTileClearer;

        private void Awake()
        {
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelManager>();
            if (tileClearer == null)
                tileClearer = FindTileClearerInSameScene();
            if (monsterHealthUI == null)
                monsterHealthUI = FindFirstObjectByType<MonsterHealthUI>();
            if (partyHealthUI == null)
                partyHealthUI = FindFirstObjectByType<PartyHealthUI>();
            if (stageDatabase == null)
                stageDatabase = Resources.Load<StageDatabase>("StageDatabase");
            if (skillDatabase == null)
                skillDatabase = Resources.Load<SkillDatabase>("SkillDatabase");
            if (characterStats == null)
                characterStats = CharacterStatsResolver.LoadFromResources();

            AutoWireSlotsIfMissing();
        }

        private void OnEnable()
        {
            // 씬 전환 중 GameManager 교체(DontDestroyOnLoad) 타이밍으로
            // FindFirstObjectByType 결과가 이전 씬의 TileClearer를 잡을 수 있어,
            // "현재 씬에 있는 TileClearer만" 다시 찾아 구독합니다.
            StartCoroutine(ResubscribeTileClearerNextFrame());
        }

        private void OnDisable()
        {
            UnsubscribeTileClearer();
        }

        private void Start()
        {
            int index = BattleStageHolder.CurrentStageIndex;
            _currentStageData = stageDatabase != null ? stageDatabase.GetStage(index) : null;

            ApplyEquippedSkills();
        }

        private void ResubscribeTileClearer()
        {
            UnsubscribeTileClearer();

            _subscribedTileClearer = FindTileClearerInSameScene();
            if (_subscribedTileClearer != null)
                _subscribedTileClearer.OnMatchGroupsCleared += HandleMatchGroupsCleared;
        }

        private IEnumerator ResubscribeTileClearerNextFrame()
        {
            // 씬 로드 직후 DontDestroyOnLoad 교체 과정이 끝난 뒤에 다시 찾기
            yield return null;
            ResubscribeTileClearer();
        }

        private void UnsubscribeTileClearer()
        {
            if (_subscribedTileClearer != null)
                _subscribedTileClearer.OnMatchGroupsCleared -= HandleMatchGroupsCleared;
            _subscribedTileClearer = null;
        }

        private TileClearer FindTileClearerInSameScene()
        {
            // 현재 씬을 기준으로 후보를 좁혀 레이스 컨디션을 줄입니다.
            // (이전 씬의 DontDestroyOnLoad 오브젝트가 잠깐 남아있을 때 잘못 구독하는 문제 방지)
            var myScene = gameObject.scene;
            var candidates = FindObjectsByType<TileClearer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < candidates.Length; i++)
            {
                var c = candidates[i];
                if (c == null) continue;
                if (c.gameObject.scene == myScene)
                    return c;
            }

            // 같은 씬에서 못 찾는 특이 케이스(비활성/씬 구성 문제)가 있으면
            // 마지막 안전망으로 첫 후보를 사용합니다.
            return candidates != null && candidates.Length > 0 ? candidates[0] : null;
        }

        private void ApplyEquippedSkills()
        {
            AutoWireSlotsIfMissing();

            if (slots == null || slots.Length == 0 || skillDatabase == null)
                return;

            for (int i = 0; i < slots.Length && i < EquippedSkillsHolder.CharacterCount; i++)
            {
                SkillSlotUI slot = slots[i];
                if (slot == null) continue;

                // 중복 구독 방지 (씬 재시작/재초기화 시 OnSkillUsed 누적 방지)
                if (_slotHandlers[i] != null)
                    slot.OnSkillUsed -= _slotHandlers[i];

                string skillKey = EquippedSkillsHolder.GetEquippedSkillId(i);
                SkillData data = null;
                if (!string.IsNullOrEmpty(skillKey))
                    data = skillDatabase.GetByKey(skillKey);
                if (data == null)
                    data = skillDatabase.GetByIndex(GetDefaultSkillIndex(i));

                if (data == null)
                {
                    slot.gameObject.SetActive(false);
                    continue;
                }

                slot.gameObject.SetActive(true);
                int cooldownTurns = data.cooldownTurns > 0 ? data.cooldownTurns : Mathf.Max(0, Mathf.RoundToInt(data.cooldownSeconds));
                slot.Configure(data.skillId, data.icon, cooldownTurns);

                _slotEffectTypes[i] = data.effectType;

                // 클로저로 SkillData 캡처 후 OnSkillUsed에 효과 등록
                SkillData captured = data;
                _slotHandlers[i] = () => ApplySkillEffect(captured);
                slot.OnSkillUsed += _slotHandlers[i];
            }
        }

        private void HandleMatchGroupsCleared(System.Collections.Generic.List<MatchGroup> matches)
        {
            if (slots == null || slots.Length == 0) return;
            if (matches == null || matches.Count == 0) return;

            // baseType별로 "매치 그룹 수"를 카운트합니다.
            // Sword(0) / Wand(1) / Bow(2) / Cross(3)
            var matchCounts = new int[4];

            for (int m = 0; m < matches.Count; m++)
            {
                var match = matches[m];
                if (match == null) continue;
                if (match.Count < 3) continue;

                int baseType = Tile.GetBaseType(match.TileType);
                if (baseType < 0 || baseType >= 4) continue;
                matchCounts[baseType] += 1;
            }

            for (int i = 0; i < slots.Length && i < EquippedSkillsHolder.CharacterCount; i++)
            {
                var slot = slots[i];
                if (slot == null || !slot.gameObject.activeSelf) continue;

                int neededBaseType = _slotEffectTypes[i] switch
                {
                    SkillEffectType.Sword => 0,
                    // 타일 baseType 인덱스 기준(사용자 정의):
                    // 칼=0, 완드=1, 활=2, 십자가=3
                    SkillEffectType.Wand => 1,
                    SkillEffectType.Bow => 2,
                    SkillEffectType.Heal => 3,
                    _ => -1
                };

                if (neededBaseType < 0) continue;

                int tick = matchCounts[neededBaseType];
                if (tick > 0)
                    slot.OnMatchAdvanced(tick);
            }
        }

        private void AutoWireSlotsIfMissing()
        {
            if (slots != null && slots.Length == 4 && slots.All(s => s != null))
                return;

            // 씬에 SkillSlotUI가 존재하는데 초기화 스크립트가 slots에 연결이 안 된 경우를 대비해 자동 탐색
            var found = FindObjectsByType<SkillSlotUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (found == null || found.Length == 0) return;

            var ordered = found
                .Where(s => s != null && s.gameObject != null)
                .OrderBy(s => s.gameObject.name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (slots == null || slots.Length != 4)
                slots = new SkillSlotUI[4];

            // 이름이 SkillSlot0~3이면 그걸 우선 매칭
            for (int i = 0; i < 4; i++)
            {
                var exact = ordered.FirstOrDefault(s => string.Equals(s.gameObject.name, $"SkillSlot{i}", StringComparison.OrdinalIgnoreCase));
                if (exact != null) slots[i] = exact;
            }

            // 남은 칸은 정렬된 순서대로 채움
            int cursor = 0;
            for (int i = 0; i < 4; i++)
            {
                if (slots[i] != null) continue;
                while (cursor < ordered.Length && Array.IndexOf(slots, ordered[cursor]) >= 0) cursor++;
                if (cursor < ordered.Length) slots[i] = ordered[cursor++];
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
                    float attackMul = EquipmentStatModifier.GetAttackMultiplier();
                    float resistance = BattlePhaseRuntime.ActivePhaseIndex == BattlePhaseRuntime.Phase2
                        ? _currentStageData.swordResistancePhase2
                        : _currentStageData.swordResistance;
                    int dmg = Mathf.RoundToInt(
                        CharacterStatsResolver.GetSwordDamage(characterStats) * attackMul * data.effectMultiplier
                        * (1f - resistance));
                    if (dmg > 0) monsterHealthUI.TakeDamage(dmg);
                    break;
                }
                case SkillEffectType.Bow:
                {
                    if (monsterHealthUI == null) return;
                    float attackMul = EquipmentStatModifier.GetAttackMultiplier();
                    float resistance = BattlePhaseRuntime.ActivePhaseIndex == BattlePhaseRuntime.Phase2
                        ? _currentStageData.bowResistancePhase2
                        : _currentStageData.bowResistance;
                    int dmg = Mathf.RoundToInt(
                        CharacterStatsResolver.GetBowDamage(characterStats) * attackMul * data.effectMultiplier
                        * (1f - resistance));
                    if (dmg > 0) monsterHealthUI.TakeDamage(dmg);
                    break;
                }
                case SkillEffectType.Wand:
                {
                    if (monsterHealthUI == null) return;
                    float attackMul = EquipmentStatModifier.GetAttackMultiplier();
                    float resistance = BattlePhaseRuntime.ActivePhaseIndex == BattlePhaseRuntime.Phase2
                        ? _currentStageData.wandResistancePhase2
                        : _currentStageData.wandResistance;
                    int dmg = Mathf.RoundToInt(
                        CharacterStatsResolver.GetWandDamage(characterStats) * attackMul * data.effectMultiplier
                        * (1f - resistance));
                    if (dmg > 0) monsterHealthUI.TakeDamage(dmg);
                    break;
                }
                case SkillEffectType.Heal:
                {
                    if (partyHealthUI == null) return;
                    float healMul = EquipmentStatModifier.GetHealMultiplier();
                    int heal = Mathf.RoundToInt(
                        CharacterStatsResolver.GetHealAmount(characterStats) * healMul * data.effectMultiplier);
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
