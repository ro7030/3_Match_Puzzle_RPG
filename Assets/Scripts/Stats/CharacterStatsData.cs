using UnityEngine;

namespace Match3Puzzle.Stats
{
    /// <summary>
    /// 플레이어 파티의 기본 공격/힐 수치 + 업그레이드 1레벨당 증가량.
    /// 스테이지와 무관하게 독립 관리. Resources/CharacterStats 이름으로 저장하면 자동 로드.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterStats", menuName = "Match3/Character Stats", order = 2)]
    public class CharacterStatsData : ScriptableObject
    {
        [Header("기본 데미지/힐 (업그레이드 0단계)")]
        [Tooltip("검 타일 매칭 1개당 기본 데미지")]
        public int baseSwordDamage = 30;

        [Tooltip("활 타일 매칭 시 단일 기본 데미지")]
        public int baseBowDamage = 80;

        [Tooltip("지팡이 타일 매칭 1개당 기본 마법 데미지")]
        public int baseWandDamage = 35;

        [Tooltip("십자가 타일 매칭 1개당 기본 회복량 (파티 전원)")]
        public int baseHealAmount = 25;

        [Tooltip("강화 타일 1개당 추가 데미지 (검/활/지팡이 매칭 시)")]
        public int baseEnhancedBonus = 50;

        [Header("업그레이드 1레벨당 증가량")]
        [Tooltip("검 업그레이드 시 레벨당 데미지 증가")]
        public int swordBonusPerLevel = 10;

        [Tooltip("활 업그레이드 시 레벨당 데미지 증가")]
        public int bowBonusPerLevel = 20;

        [Tooltip("마법 업그레이드 시 레벨당 데미지 증가")]
        public int wandBonusPerLevel = 12;

        [Tooltip("힐 업그레이드 시 레벨당 회복량 증가")]
        public int healBonusPerLevel = 8;

        [Tooltip("강화 타일 업그레이드 시 레벨당 보너스 증가")]
        public int enhancedBonusPerLevel = 15;
    }
}
