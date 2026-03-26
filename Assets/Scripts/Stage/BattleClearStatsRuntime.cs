using UnityEngine;

namespace Match3Puzzle.Stage
{
    /// <summary>
    /// 배틀 중 누적되는 클리어용 통계(전투 종료 후 UI 표시용).
    /// </summary>
    public static class BattleClearStatsRuntime
    {
        public static int EnhancedSummonCount { get; private set; }

        public static int DamageSword { get; private set; }
        public static int DamageBow { get; private set; }
        public static int DamageWand { get; private set; }
        public static int HealCross { get; private set; }

        public static void ResetForNewBattle()
        {
            EnhancedSummonCount = 0;
            DamageSword = 0;
            DamageBow = 0;
            DamageWand = 0;
            HealCross = 0;
        }

        public static void AddEnhancedSummon()
        {
            EnhancedSummonCount++;
        }

        public static void AddDamageSword(int amount)
        {
            if (amount <= 0) return;
            DamageSword += amount;
        }

        public static void AddDamageBow(int amount)
        {
            if (amount <= 0) return;
            DamageBow += amount;
        }

        public static void AddDamageWand(int amount)
        {
            if (amount <= 0) return;
            DamageWand += amount;
        }

        public static void AddHealCross(int amount)
        {
            if (amount <= 0) return;
            HealCross += amount;
        }
    }
}

