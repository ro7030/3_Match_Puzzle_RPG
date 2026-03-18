using UnityEngine;

namespace Match3Puzzle.Stats
{
    /// <summary>
    /// CharacterStatsData(기본값) + CharacterUpgradeHolder(현재 레벨)로 최종 수치를 계산.
    /// final = base + (level × bonusPerLevel)
    /// </summary>
    public static class CharacterStatsResolver
    {
        /// <summary>검 매칭 1개당 최종 데미지</summary>
        public static int GetSwordDamage(CharacterStatsData stats)
            => stats.baseSwordDamage + CharacterUpgradeHolder.SwordLevel * stats.swordBonusPerLevel;

        /// <summary>활 매칭 최종 데미지</summary>
        public static int GetBowDamage(CharacterStatsData stats)
            => stats.baseBowDamage + CharacterUpgradeHolder.BowLevel * stats.bowBonusPerLevel;

        /// <summary>지팡이 매칭 1개당 최종 마법 데미지</summary>
        public static int GetWandDamage(CharacterStatsData stats)
            => stats.baseWandDamage + CharacterUpgradeHolder.WandLevel * stats.wandBonusPerLevel;

        /// <summary>십자가 매칭 1개당 최종 회복량</summary>
        public static int GetHealAmount(CharacterStatsData stats)
            => stats.baseHealAmount + CharacterUpgradeHolder.HealLevel * stats.healBonusPerLevel;

        /// <summary>강화 타일 1개당 최종 추가 데미지</summary>
        public static int GetEnhancedBonus(CharacterStatsData stats)
            => stats.baseEnhancedBonus + CharacterUpgradeHolder.EnhancedLevel * stats.enhancedBonusPerLevel;

        /// <summary>
        /// Resources 폴더에서 CharacterStatsData 에셋을 자동 로드.
        /// 에셋 이름이 "CharacterStats"이어야 함.
        /// </summary>
        public static CharacterStatsData LoadFromResources()
        {
            var data = Resources.Load<CharacterStatsData>("CharacterStats");
            if (data == null)
                UnityEngine.Debug.LogWarning("[CharacterStatsResolver] Resources/CharacterStats 에셋을 찾을 수 없습니다.");
            return data;
        }
    }
}
