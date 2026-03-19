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

        [Header("보스/몬스터")]
        public Sprite bossSprite;
        public int monsterMaxHp = 500;

        [Header("몬스터 공격 (인스펙터에서 조절)")]
        [Tooltip("몬스터가 공격하는 턴 간격 (예: 4 = 4턴마다 공격)")]
        public int attackIntervalTurns = 4;
        [Tooltip("공격 데미지 (1인당)")]
        public int attackDamage = 20;
        [Tooltip("공격 대상: 전체/단일/2명/3명")]
        public MonsterAttackTargetType attackTargetType = MonsterAttackTargetType.All;

        [Header("난이도")]
        [Tooltip("최대 턴 수 (예: 40). 40턴 안에 보스 처치 실패 시 패배")]
        public int maxTurns = 40;

        [Header("클리어 보상")]
        [Tooltip("이 스테이지를 처음 클리어했을 때 지급할 골드")]
        public int clearGoldReward = 300;

        [Header("몬스터 저항력 (타입별, 0=무감소, 1=완전 저항)")]
        [Range(0f, 1f)]
        [Tooltip("검 공격 저항")]
        public float swordResistance = 0f;
        [Range(0f, 1f)]
        [Tooltip("활 공격 저항")]
        public float bowResistance = 0f;
        [Range(0f, 1f)]
        [Tooltip("마법(지팡이) 공격 저항")]
        public float wandResistance = 0f;

        [Header("클리어 후 컷씬 (선택 사항)")]
        [Tooltip("스테이지 클리어 후 재생할 컷씬 데이터.\n" +
                 "비워두면 바로 MapScene으로 이동.\n" +
                 "할당 시 StoryScene에서 컷씬 재생 후 CutsceneData.nextSceneName으로 이동.\n" +
                 "CutsceneData의 nextSceneName을 MapScene으로, clearSaveOnComplete를 false로 설정할 것.")]
        public Story.CutsceneData clearCutscene;
    }
}
