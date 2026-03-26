using System.Linq;
using UnityEngine;

namespace Match3Puzzle.Skill
{
    /// <summary>
    /// 모든 스킬 데이터를 모아두는 DB.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillDatabase", menuName = "Match3/Skill Database", order = 1)]
    public class SkillDatabase : ScriptableObject
    {
        [SerializeField] private SkillData[] skills;

        public SkillData GetById(string skillId)
        {
            if (skills == null || string.IsNullOrEmpty(skillId)) return null;
            return skills.FirstOrDefault(s => s != null && s.skillId == skillId);
        }

        /// <summary>
        /// 장착/저장에 사용하는 "키"로 스킬을 조회합니다.
        /// SkillData의 에셋 이름(name)이 가장 우선이며, (name이 비어있지 않으면) skillId 중복 문제를 피할 수 있습니다.
        /// - 신규 저장: SkillData.name 사용
        /// - 레거시 저장: SkillData.skillId 사용
        /// </summary>
        public SkillData GetByKey(string key)
        {
            if (skills == null || string.IsNullOrEmpty(key)) return null;

            // 신규(에셋 이름) 우선
            var byName = skills.FirstOrDefault(s => s != null && s.name == key);
            if (byName != null) return byName;

            // 레거시(중복될 수 있는 skillId) fallback
            return skills.FirstOrDefault(s => s != null && s.skillId == key);
        }

        public SkillData[] GetAllForCharacter(int characterIndex)
        {
            if (skills == null) return new SkillData[0];
            return skills.Where(s => s != null && s.ownerCharacterIndex == characterIndex).ToArray();
        }

        /// <summary>
        /// 인덱스로 스킬 조회. 기본 장착 스킬: 캐릭터0=0, 캐릭터1=3, 캐릭터2=6, 캐릭터3=9
        /// </summary>
        public SkillData GetByIndex(int index)
        {
            if (skills == null || index < 0 || index >= skills.Length) return null;
            return skills[index];
        }
    }
}

