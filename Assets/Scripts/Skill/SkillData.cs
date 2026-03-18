using UnityEngine;

namespace Match3Puzzle.Skill
{
    /// <summary>
    /// 스킬 효과 타입. 스킬 버튼 클릭 시 어떤 종류의 효과를 발동할지 결정.
    /// </summary>
    public enum SkillEffectType
    {
        /// <summary>검 기본 데미지 × 배율로 몬스터에게 피해 (검 저항 적용)</summary>
        Sword,
        /// <summary>활 기본 데미지 × 배율로 몬스터에게 피해 (활 저항 적용)</summary>
        Bow,
        /// <summary>지팡이 기본 데미지 × 배율로 몬스터에게 피해 (마법 저항 적용)</summary>
        Wand,
        /// <summary>십자가 기본 회복량 × 배율로 파티 전원 HP 회복</summary>
        Heal
    }

    /// <summary>
    /// 스킬 데이터 (아이콘, 쿨타임, 효과 타입·배율). 스킬 1개에 해당.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillData", menuName = "Match3/Skill Data", order = 0)]
    public class SkillData : ScriptableObject
    {
        [Header("기본 정보")]
        public string skillId = "skill_001";
        [Tooltip("스킬 이름. UI에 표시됨")]
        public string displayName = "스킬 이름";
        [Tooltip("스킬 설명. 스킬 선택 시 설명 패널에 표시됨 (예: 500%의 광역 피해)")]
        [TextArea(2, 4)]
        public string description;

        [Header("표시")]
        public Sprite icon;

        [Header("쿨타임")]
        [Tooltip("스킬 쿨타임(초). 스킬 사용 후 다시 쓸 수 있을 때까지 대기 시간.")]
        public float cooldownSeconds = 5f;

        [Header("스킬 효과")]
        [Tooltip("효과 타입: Sword=검 데미지 기반, Bow=활 데미지 기반, Wand=마법 데미지 기반, Heal=힐량 기반")]
        public SkillEffectType effectType = SkillEffectType.Sword;

        [Tooltip("StageData 기본 수치 대비 배율. 예: 1.5 = 150%. 스테이지별 저항력도 그대로 적용됨.")]
        public float effectMultiplier = 1.0f;

        [Tooltip("어느 캐릭터의 스킬인지 (0~3). 0=첫 번째 캐릭터, 3=네 번째 캐릭터")]
        public int ownerCharacterIndex = 0;

        [Header("해금 조건")]
        [Tooltip("0=항상, 2=챕터1 클리어 이후, 3=챕터2 클리어 이후 (lastClearedChapter 기준)")]
        public int requiredChapter = 0;
    }
}
