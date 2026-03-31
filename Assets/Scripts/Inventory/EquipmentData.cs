using UnityEngine;

namespace Match3Puzzle.Inventory
{
    public enum EquipmentSlotType
    {
        Slot0 = 0,
        Slot1 = 1
    }

    /// <summary>
    /// 장비 1개 데이터.
    /// SkillData와 유사하게 이름/설명/아이콘/해금 조건/가격/효과 배율을 가진다.
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "Match3/Equipment Data", order = 0)]
    public class EquipmentData : ScriptableObject
    {
        [Header("기본 정보")]
        public string equipmentId = "equip_001";
        public string displayName = "장비 이름";

        [TextArea(2, 5)]
        public string description = "장비 설명";

        [Header("TMI(추가 설명)")]
        [Tooltip("옵션. InfoPanel의 TMI 영역에 표시됩니다. 비우면 숨김 처리됩니다.")]
        [TextArea(2, 4)]
        public string tmi = "";

        [Header("효과 표기(선택)")]
        [Tooltip("비우면 파티 체력/공격/회복 % 필드로 자동 문구 생성. 예: 방어력 1000% 상승")]
        [TextArea(1, 3)]
        public string effectLineOverride = "";

        [Header("표시")]
        public Sprite icon;

        [Header("장착 슬롯")]
        [Tooltip("Slot0: ShopItemCard_0/2 계열, Slot1: ShopItemCard_1/3 계열")]
        public EquipmentSlotType slotType = EquipmentSlotType.Slot0;

        [Header("구매/해금")]
        public int goldCost = 500;
        public int requiredChapter = 0;

        [Header("효과(%)")]
        [Tooltip("파티 캐릭터 전체 최대 체력 증가 비율(예: 15 = +15%)")]
        [Range(0f, 300f)] public float partyMaxHpPercent = 0f;

        [Tooltip("검/활/마법 공격 증가 비율(예: 20 = +20%)")]
        [Range(0f, 300f)] public float attackPercent = 0f;

        [Tooltip("수녀(힐) 회복량 증가 비율(예: 25 = +25%)")]
        [Range(0f, 300f)] public float healPercent = 0f;
    }
}

