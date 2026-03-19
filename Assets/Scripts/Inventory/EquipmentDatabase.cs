using System.Linq;
using UnityEngine;

namespace Match3Puzzle.Inventory
{
    [CreateAssetMenu(fileName = "EquipmentDatabase", menuName = "Match3/Equipment Database", order = 1)]
    public class EquipmentDatabase : ScriptableObject
    {
        [SerializeField] private EquipmentData[] equipments;

        public EquipmentData GetById(string equipmentId)
        {
            if (equipments == null || string.IsNullOrEmpty(equipmentId)) return null;
            return equipments.FirstOrDefault(e => e != null && e.equipmentId == equipmentId);
        }

        public EquipmentData[] GetAllBySlot(EquipmentSlotType slotType)
        {
            if (equipments == null) return new EquipmentData[0];
            return equipments.Where(e => e != null && e.slotType == slotType).ToArray();
        }

        public EquipmentData[] GetAll()
        {
            if (equipments == null) return new EquipmentData[0];
            return equipments.Where(e => e != null).ToArray();
        }
    }
}

