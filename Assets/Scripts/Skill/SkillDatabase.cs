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

