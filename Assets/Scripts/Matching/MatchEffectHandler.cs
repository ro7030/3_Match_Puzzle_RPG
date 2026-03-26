using System.Collections.Generic;
using UnityEngine;
using Match3Puzzle.Board;
using Match3Puzzle.Matching;
using Match3Puzzle.Stage;
using Match3Puzzle.Stats;
using Match3Puzzle.Tutorial;
using Match3Puzzle.UI.Battle;
using Match3Puzzle.Inventory;

namespace Match3Puzzle.Matching
{
    /// <summary>
    /// 매칭된 타일에 따라 전투 효과 적용.
    /// 데미지/힐 수치: CharacterStatsData(기본) + CharacterUpgradeHolder(업그레이드 레벨)
    /// 저항력: StageData(스테이지별 몬스터 저항)
    /// 타일 인덱스 매핑:
    /// 0=검(광역), 1=지팡이(마법 광역), 2=활(단일 강력), 3=십자가(파티 힐)
    /// 강화 타일도 동일 매핑(+4): 4=검, 5=지팡이, 6=활, 7=십자가
    /// </summary>
    public class MatchEffectHandler : MonoBehaviour
    {
        [SerializeField] private MonsterHealthUI monsterHealthUI;
        [SerializeField] private PartyHealthUI partyHealthUI;
        [SerializeField] private StageDatabase stageDatabase;

        [Header("플레이어 스탯 (없으면 Resources/CharacterStats 자동 로드)")]
        [SerializeField] private CharacterStatsData characterStats;

        private StageData _stageData;

        private void Awake()
        {
            if (monsterHealthUI == null) monsterHealthUI = FindFirstObjectByType<MonsterHealthUI>();
            if (partyHealthUI == null) partyHealthUI = FindFirstObjectByType<PartyHealthUI>();
            if (stageDatabase == null) stageDatabase = Resources.Load<StageDatabase>("StageDatabase");
            if (characterStats == null) characterStats = CharacterStatsResolver.LoadFromResources();
        }

        private void Start()
        {
            int idx = BattleStageHolder.CurrentStageIndex;
            _stageData = stageDatabase != null ? stageDatabase.GetStage(idx) : null;
        }

        /// <summary>
        /// 매칭 그룹들에 대해 전투 효과 적용.
        /// 강화 타일 1개당 추가 데미지 포함.
        /// </summary>
        public void ApplyMatchEffects(List<MatchGroup> matches)
        {
            // 튜토리얼은 TutorialManager가 TileClearer.OnMatchCleared에서 고정 데미지만 쓰도록 설계됨.
            // 여기서 스탯 기반 데미지까지 넣으면 일반 배틀과 동일한 큰 피해가 들어가 체력이 바로 0이 됨.
            if (FindFirstObjectByType<TutorialManager>() != null)
                return;

            if (_stageData == null || characterStats == null) return;

            foreach (var match in matches)
            {
                if (BattlePhaseRuntime.IsBattleCutsceneActive) return;
                if (match == null || match.Count < 3) continue;

                int baseType = Tile.GetBaseType(match.TileType);
                int count = match.Count;
                int enhancedCount = CountEnhancedTiles(match);

                switch (baseType)
                {
                    case 0: ApplySwordMatch(count, enhancedCount); break;
                    case 1: ApplyWandMatch(count, enhancedCount);  break;
                    case 2: ApplyBowMatch(count, enhancedCount);  break;
                    case 3: ApplyCrossMatch(count);                break;
                }
            }
        }

        private int CountEnhancedTiles(MatchGroup match)
        {
            int n = 0;
            if (match?.Tiles == null) return n;
            foreach (var t in match.Tiles)
            {
                if (t != null && (t.IsEnhanced || (t.TileType >= 4 && t.TileType <= 7))) n++;
            }
            return n;
        }

        private void ApplySwordMatch(int matchCount, int enhancedCount)
        {
            if (monsterHealthUI == null) return;
            float attackMul = EquipmentStatModifier.GetAttackMultiplier();
            int baseDmg = Mathf.RoundToInt(CharacterStatsResolver.GetSwordDamage(characterStats) * attackMul) * matchCount;
            int bonus   = CharacterStatsResolver.GetEnhancedBonus(characterStats) * enhancedCount;
            float resistance = BattlePhaseRuntime.ActivePhaseIndex == BattlePhaseRuntime.Phase2
                ? _stageData.swordResistancePhase2
                : _stageData.swordResistance;
            int final   = Mathf.RoundToInt((baseDmg + bonus) * (1f - resistance));
            if (final <= 0) return;

            // 이미 몬스터가 죽었을 경우 과집계를 방지하기 위해 실제 적용 가능한 HP만 누적
            int remainingHp = monsterHealthUI.CurrentHp;
            int actualDamage = remainingHp > 0 ? Mathf.Min(final, remainingHp) : 0;
            BattleClearStatsRuntime.AddDamageSword(actualDamage);
            monsterHealthUI.TakeDamage(final);
        }

        private void ApplyBowMatch(int matchCount, int enhancedCount)
        {
            if (monsterHealthUI == null) return;
            float attackMul = EquipmentStatModifier.GetAttackMultiplier();
            // 활도 3매치면 matchCount만큼 기본 데미지를 누적
            int baseDmg = Mathf.RoundToInt(CharacterStatsResolver.GetBowDamage(characterStats) * attackMul) * matchCount;
            int bonus   = CharacterStatsResolver.GetEnhancedBonus(characterStats) * enhancedCount;
            float resistance = BattlePhaseRuntime.ActivePhaseIndex == BattlePhaseRuntime.Phase2
                ? _stageData.bowResistancePhase2
                : _stageData.bowResistance;
            int final   = Mathf.RoundToInt((baseDmg + bonus) * (1f - resistance));
            if (final <= 0) return;

            int remainingHp = monsterHealthUI.CurrentHp;
            int actualDamage = remainingHp > 0 ? Mathf.Min(final, remainingHp) : 0;
            BattleClearStatsRuntime.AddDamageBow(actualDamage);
            monsterHealthUI.TakeDamage(final);
        }

        private void ApplyCrossMatch(int matchCount)
        {
            if (partyHealthUI == null) return;
            float healMul = EquipmentStatModifier.GetHealMultiplier();
            int heal = Mathf.RoundToInt(CharacterStatsResolver.GetHealAmount(characterStats) * healMul) * matchCount;
            for (int i = 0; i < partyHealthUI.CharacterCount; i++)
            {
                if (partyHealthUI.IsAlive(i))
                {
                    int current = partyHealthUI.GetCurrentHP(i);
                    int missing = Mathf.Max(0, partyHealthUI.MaxHpPerCharacter - current);
                    int actualHeal = missing > 0 ? Mathf.Min(heal, missing) : 0;
                    BattleClearStatsRuntime.AddHealCross(actualHeal);
                    partyHealthUI.Heal(i, heal);
                }
            }
        }

        private void ApplyWandMatch(int matchCount, int enhancedCount)
        {
            if (monsterHealthUI == null) return;
            float attackMul = EquipmentStatModifier.GetAttackMultiplier();
            int baseDmg = Mathf.RoundToInt(CharacterStatsResolver.GetWandDamage(characterStats) * attackMul) * matchCount;
            int bonus   = CharacterStatsResolver.GetEnhancedBonus(characterStats) * enhancedCount;
            float resistance = BattlePhaseRuntime.ActivePhaseIndex == BattlePhaseRuntime.Phase2
                ? _stageData.wandResistancePhase2
                : _stageData.wandResistance;
            int final   = Mathf.RoundToInt((baseDmg + bonus) * (1f - resistance));
            if (final <= 0) return;

            int remainingHp = monsterHealthUI.CurrentHp;
            int actualDamage = remainingHp > 0 ? Mathf.Min(final, remainingHp) : 0;
            BattleClearStatsRuntime.AddDamageWand(actualDamage);
            monsterHealthUI.TakeDamage(final);
        }
    }
}
