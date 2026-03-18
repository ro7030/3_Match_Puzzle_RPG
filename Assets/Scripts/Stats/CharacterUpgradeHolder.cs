using MainMenu;

namespace Match3Puzzle.Stats
{
    /// <summary>
    /// 플레이어 스탯 업그레이드 레벨 보관 (런타임 정적 저장소).
    /// 씬 전환 시에도 유지. 게임 시작 시 SaveSystem에서 LoadFromSave()로 복원.
    /// 업그레이드 시 레벨을 올리고 SaveSystem으로 저장.
    /// </summary>
    public static class CharacterUpgradeHolder
    {
        // 인덱스 매핑 (GameSaveData.statUpgradeLevels 배열과 동일 순서)
        public const int IndexSword    = 0;
        public const int IndexBow      = 1;
        public const int IndexWand     = 2;
        public const int IndexHeal     = 3;
        public const int IndexEnhanced = 4;
        public const int StatCount     = 5;

        /// <summary>검 업그레이드 레벨 (0 = 기본)</summary>
        public static int SwordLevel    { get; set; } = 0;
        /// <summary>활 업그레이드 레벨</summary>
        public static int BowLevel      { get; set; } = 0;
        /// <summary>마법 업그레이드 레벨</summary>
        public static int WandLevel     { get; set; } = 0;
        /// <summary>힐 업그레이드 레벨</summary>
        public static int HealLevel     { get; set; } = 0;
        /// <summary>강화 타일 업그레이드 레벨</summary>
        public static int EnhancedLevel { get; set; } = 0;

        /// <summary>
        /// 세이브 데이터에서 업그레이드 레벨 복원. 게임 시작/로드 시 호출.
        /// </summary>
        public static void LoadFromSave(GameSaveData save)
        {
            if (save?.statUpgradeLevels == null || save.statUpgradeLevels.Length < StatCount)
                return;

            SwordLevel    = save.statUpgradeLevels[IndexSword];
            BowLevel      = save.statUpgradeLevels[IndexBow];
            WandLevel     = save.statUpgradeLevels[IndexWand];
            HealLevel     = save.statUpgradeLevels[IndexHeal];
            EnhancedLevel = save.statUpgradeLevels[IndexEnhanced];
        }

        /// <summary>
        /// 현재 업그레이드 레벨을 세이브 데이터에 기록. SaveSystem.Save() 직전에 호출.
        /// </summary>
        public static void ApplyToSave(GameSaveData save)
        {
            if (save == null) return;

            if (save.statUpgradeLevels == null || save.statUpgradeLevels.Length < StatCount)
                save.statUpgradeLevels = new int[StatCount];

            save.statUpgradeLevels[IndexSword]    = SwordLevel;
            save.statUpgradeLevels[IndexBow]      = BowLevel;
            save.statUpgradeLevels[IndexWand]      = WandLevel;
            save.statUpgradeLevels[IndexHeal]      = HealLevel;
            save.statUpgradeLevels[IndexEnhanced]  = EnhancedLevel;
        }

        /// <summary>모든 레벨 초기화 (디버그용)</summary>
        public static void ResetAll()
        {
            SwordLevel = BowLevel = WandLevel = HealLevel = EnhancedLevel = 0;
        }
    }
}
