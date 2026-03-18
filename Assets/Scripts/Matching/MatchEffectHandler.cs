using System.Collections.Generic;
using UnityEngine;
using Match3Puzzle.Board;
using Match3Puzzle.Matching;
using Match3Puzzle.Stage;
using Match3Puzzle.Stats;
using Match3Puzzle.UI.Battle;

namespace Match3Puzzle.Matching
{
    /// <summary>
    /// 매칭된 타일에 따라 전투 효과 적용.
    /// 데미지/힐 수치: CharacterStatsData(기본) + CharacterUpgradeHolder(업그레이드 레벨)
    /// 저항력: StageData(스테이지별 몬스터 저항)
    /// 0=검(광역), 1=활(단일 강력), 2=십자가(파티 힐), 3=지팡이(마법 광역)
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
            if (_stageData == null || characterStats == null) return;

            foreach (var match in matches)
            {
                if (match == null || match.Count < 3) continue;

                int baseType = Tile.GetBaseType(match.TileType);
                int count = match.Count;
                int enhancedCount = CountEnhancedTiles(match);

                switch (baseType)
                {
                    case 0: ApplySwordMatch(count, enhancedCount); break;
                    case 1: ApplyBowMatch(enhancedCount);          break;
                    case 2: ApplyCrossMatch(count);                break;
                    case 3: ApplyWandMatch(count, enhancedCount);  break;
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
            int baseDmg = CharacterStatsResolver.GetSwordDamage(characterStats) * matchCount;
            int bonus   = CharacterStatsResolver.GetEnhancedBonus(characterStats) * enhancedCount;
            int final   = Mathf.RoundToInt((baseDmg + bonus) * (1f - _stageData.swordResistance));
            if (final > 0) monsterHealthUI.TakeDamage(final);
        }

        private void ApplyBowMatch(int enhancedCount)
        {
            if (monsterHealthUI == null) return;
            int baseDmg = CharacterStatsResolver.GetBowDamage(characterStats);
            int bonus   = CharacterStatsResolver.GetEnhancedBonus(characterStats) * enhancedCount;
            int final   = Mathf.RoundToInt((baseDmg + bonus) * (1f - _stageData.bowResistance));
            if (final > 0) monsterHealthUI.TakeDamage(final);
        }

        private void ApplyCrossMatch(int matchCount)
        {
            if (partyHealthUI == null) return;
            int heal = CharacterStatsResolver.GetHealAmount(characterStats) * matchCount;
            for (int i = 0; i < partyHealthUI.CharacterCount; i++)
            {
                if (partyHealthUI.IsAlive(i))
                    partyHealthUI.Heal(i, heal);
            }
        }

        private void ApplyWandMatch(int matchCount, int enhancedCount)
        {
            if (monsterHealthUI == null) return;
            int baseDmg = CharacterStatsResolver.GetWandDamage(characterStats) * matchCount;
            int bonus   = CharacterStatsResolver.GetEnhancedBonus(characterStats) * enhancedCount;
            int final   = Mathf.RoundToInt((baseDmg + bonus) * (1f - _stageData.wandResistance));
            if (final > 0) monsterHealthUI.TakeDamage(final);
        }
    }
}
