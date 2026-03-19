using UnityEngine;
using MainMenu;

namespace Match3Puzzle.Inventory
{
    /// <summary>
    /// 현재 장착 장비 2개를 합산해 전투 배율을 제공.
    /// </summary>
    public static class EquipmentStatModifier
    {
        private static EquipmentDatabase _cachedDb;
        private static bool _initializedFromSave;

        public static float GetHpMultiplier()
        {
            float percent = SumPercent(e => e.partyMaxHpPercent);
            return 1f + (percent * 0.01f);
        }

        public static float GetAttackMultiplier()
        {
            float percent = SumPercent(e => e.attackPercent);
            return 1f + (percent * 0.01f);
        }

        public static float GetHealMultiplier()
        {
            float percent = SumPercent(e => e.healPercent);
            return 1f + (percent * 0.01f);
        }

        private static float SumPercent(System.Func<EquipmentData, float> selector)
        {
            EnsureInitializedFromSave();
            var db = GetDb();
            if (db == null) return 0f;

            float sum = 0f;
            for (int slot = 0; slot < EquippedEquipmentHolder.SlotCount; slot++)
            {
                string id = EquippedEquipmentHolder.GetEquippedId(slot);
                if (string.IsNullOrEmpty(id)) continue;
                var data = db.GetById(id);
                if (data == null) continue;
                sum += selector(data);
            }
            return sum;
        }

        private static EquipmentDatabase GetDb()
        {
            if (_cachedDb == null)
                _cachedDb = Resources.Load<EquipmentDatabase>("EquipmentDatabase");
            return _cachedDb;
        }

        private static void EnsureInitializedFromSave()
        {
            if (_initializedFromSave) return;
            _initializedFromSave = true;

            // 전투 씬에서 바로 진입해도 장비 효과가 적용되도록 안전 로딩
            var save = SaveSystem.Load();
            if (save != null)
                EquippedEquipmentHolder.LoadFromSave(save);
        }
    }
}

