using MainMenu;

namespace Match3Puzzle.Inventory
{
    /// <summary>
    /// 장착 장비 2칸(Slot0, Slot1) 런타임 보관 + 세이브 연동.
    /// </summary>
    public static class EquippedEquipmentHolder
    {
        public const int SlotCount = 2;
        private static readonly string[] EquippedIds = new string[SlotCount];

        public static void Equip(int slotIndex, string equipmentId)
        {
            if (slotIndex < 0 || slotIndex >= SlotCount) return;
            EquippedIds[slotIndex] = equipmentId ?? string.Empty;
        }

        public static string GetEquippedId(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SlotCount) return string.Empty;
            return EquippedIds[slotIndex] ?? string.Empty;
        }

        public static void ClearAll()
        {
            for (int i = 0; i < SlotCount; i++)
                EquippedIds[i] = string.Empty;
        }

        public static void LoadFromSave(GameSaveData save)
        {
            if (save == null || save.equippedEquipmentIds == null || save.equippedEquipmentIds.Length == 0)
            {
                ClearAll();
                return;
            }

            for (int i = 0; i < SlotCount && i < save.equippedEquipmentIds.Length; i++)
                EquippedIds[i] = save.equippedEquipmentIds[i] ?? string.Empty;
        }

        public static void ApplyToSave(GameSaveData save)
        {
            if (save == null) return;
            if (save.equippedEquipmentIds == null || save.equippedEquipmentIds.Length != SlotCount)
                save.equippedEquipmentIds = new string[SlotCount];

            for (int i = 0; i < SlotCount; i++)
                save.equippedEquipmentIds[i] = EquippedIds[i] ?? string.Empty;
        }
    }
}

