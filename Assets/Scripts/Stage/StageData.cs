using UnityEngine;

namespace Match3Puzzle.Stage
{
    /// <summary>
    /// 몬스터 공격 대상 타입. 인스펙터에서 스테이지별로 선택 가능.
    /// </summary>
    public enum MonsterAttackTargetType
    {
        /// <summary>4명 전체 공격</summary>
        All,
        /// <summary>랜덤 1명</summary>
        SingleRandom,
        /// <summary>랜덤 2명</summary>
        Random2,
        /// <summary>랜덤 3명</summary>
        Random3
    }

    /// <summary>
    /// 전투 시작 시 몬스터 선공 동작 모드.
    /// </summary>
    public enum BattleStartAttackMode
    {
        /// <summary>시작 즉시 공격하지 않음 (기본: 설정 턴 간격부터)</summary>
        Disabled,
        /// <summary>시작 즉시 1회 공격, 턴 카운트는 증가시키지 않음</summary>
        AttackOnly,
        /// <summary>시작 즉시 1회 공격 후 턴 카운트도 +1</summary>
        AttackAndCountTurn
    }

    /// <summary>
    /// 페이즈2 전환 트리거 기준.
    /// </summary>
    public enum Phase2TriggerTargetType
    {
        /// <summary>몬스터 현재 HP가 기준 이하일 때 발동</summary>
        MonsterHp,
        /// <summary>파티원(중 아무나)의 현재 HP가 기준 이하일 때 발동</summary>
        PartyAnyMemberHp
    }

    /// <summary>
    /// 스테이지별 설정. 배경·보스 이미지, 턴 수, 몬스터 체력·공격 등.
    /// </summary>
    [CreateAssetMenu(fileName = "StageData", menuName = "Match3/Stage Data", order = 0)]
    public class StageData : ScriptableObject
    {
        [Header("표시")]
        public string stageName = "Stage 1";
        [TextArea(2, 4)]
        public string description = "";

        [Header("배경")]
        public Sprite backgroundSprite;

        [Header("페이즈1 (기본) 보스/몬스터")]
        public Sprite bossSprite;
        public int monsterMaxHp = 500;

        [Header("페이즈1 몬스터 공격 (인스펙터에서 조절)")]
        [Tooltip("전투 시작 직후 선공 여부/턴 카운트 처리 방식")]
        public BattleStartAttackMode startAttackMode = BattleStartAttackMode.Disabled;
        [Tooltip("몬스터가 공격하는 턴 간격 (예: 4 = 4턴마다 공격)")]
        public int attackIntervalTurns = 4;
        [Tooltip("공격 데미지 (1인당)")]
        public int attackDamage = 20;
        [Tooltip("공격 대상: 전체/단일/2명/3명")]
        public MonsterAttackTargetType attackTargetType = MonsterAttackTargetType.All;

        [Header("파티 (플레이어)")]
        [Tooltip("전투 시작 시 파티원 1인당, 최대 체력에서 깎인 만큼 현재 체력으로 시작합니다. 예: 최대 100, 이 값 5 → 95로 시작. 0이면 풀 체력으로 시작.")]
        [Min(0)]
        public int partyStartHpDeduction = 0;
        [Tooltip("퍼즐에서 매칭 성공으로 턴이 1회 소모될 때마다, 살아있는 파티원 1인당 감소하는 체력. 0이면 비활성.")]
        [Min(0)]
        public int partyHpLossPerTurn = 0;

        [Header("난이도")]
        [Tooltip("최대 턴 수 (예: 40). 40턴 안에 보스 처치 실패 시 패배")]
        public int maxTurns = 40;

        [Header("스테이지 시작 인트로 (선택)")]
        [Tooltip("스테이지 진입 직전에 StoryScene에서 재생할 컷씬 데이터.\n"
                 + "비워두면 컷신 없이 바로 BattleScene으로 진입합니다.")]
        public Story.CutsceneData introCutscene;

        [Header("클리어 보상")]
        [Tooltip("이 스테이지를 처음 클리어했을 때 지급할 골드")]
        public int clearGoldReward = 300;

        [Header("페이즈1 몬스터 저항력 (타입별, 0=무감소, 1=완전 저항)")]
        [Range(0f, 1f)]
        [Tooltip("검 공격 저항")]
        public float swordResistance = 0f;
        [Range(0f, 1f)]
        [Tooltip("활 공격 저항")]
        public float bowResistance = 0f;
        [Range(0f, 1f)]
        [Tooltip("마법(지팡이) 공격 저항")]
        public float wandResistance = 0f;

        [Header("페이즈2 (선택)")]
        [Tooltip("체크 시 페이즈2가 존재하며, 선택된 트리거 대상의 HP가 조건 이하가 되면 컷씬 후 페이즈2가 시작됩니다.")]
        public bool hasPhase2 = false;

        [Header("페이즈2 트리거")]
        [Tooltip("페이즈2 전환을 판단할 대상입니다. (몬스터 HP / 파티원 HP)")]
        public Phase2TriggerTargetType phase2TriggerTarget = Phase2TriggerTargetType.MonsterHp;

        [Tooltip("몬스터 현재 HP가 이 값 이하가 되면 페이즈2로 전환됩니다.\n"
                 + "기본값 0은 트리거되지 않는 용도로 두었습니다.\n"
                 + "※ Target이 '파티원 HP'이면 '파티원 1명이라도 현재 HP가 이 값 이하'일 때 발동합니다.")]
        [Min(0)] public int phase2TriggerHp = 0;

        [Tooltip("페이즈2 전환 전에 StoryScene에서 재생할 컷씬 데이터.\n"
                 + "비워두면 컷씬 없이 바로 페이즈2로 전환됩니다.")]
        public Story.CutsceneData phase2Cutscene;

        [Header("페이즈2 보스/몬스터")]
        public Sprite bossSpritePhase2;
        public int monsterMaxHpPhase2 = 500;

        [Header("페이즈2 몬스터 공격 (인스펙터에서 조절)")]
        [Tooltip("몬스터가 공격하는 턴 간격 (예: 4 = 4턴마다 공격)")]
        public int attackIntervalTurnsPhase2 = 4;
        [Tooltip("공격 데미지 (1인당)")]
        public int attackDamagePhase2 = 20;
        [Tooltip("공격 대상: 전체/단일/2명/3명")]
        public MonsterAttackTargetType attackTargetTypePhase2 = MonsterAttackTargetType.All;

        [Header("페이즈2 몬스터 저항력 (타입별, 0=무감소, 1=완전 저항)")]
        [Range(0f, 1f)]
        [Tooltip("검 공격 저항")]
        public float swordResistancePhase2 = 0f;
        [Range(0f, 1f)]
        [Tooltip("활 공격 저항")]
        public float bowResistancePhase2 = 0f;
        [Range(0f, 1f)]
        [Tooltip("마법(지팡이) 공격 저항")]
        public float wandResistancePhase2 = 0f;

        [Header("클리어 후 컷씬 (선택 사항)")]
        [Tooltip("스테이지 클리어 후 재생할 컷씬 데이터.\n" +
                 "비워두면 바로 MapScene으로 이동.\n" +
                 "할당 시 StoryScene에서 컷씬 재생 후 CutsceneData.nextSceneName으로 이동.\n" +
                 "CutsceneData의 nextSceneName을 MapScene으로, clearSaveOnComplete를 false로 설정할 것.")]
        public Story.CutsceneData clearCutscene;
    }
}
