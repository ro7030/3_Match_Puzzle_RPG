using UnityEngine;
using MainMenu;

namespace Match3Puzzle.Skill
{
    /// <summary>
    /// 플레이어가 장착한 스킬 4개(캐릭터별 1개)를 임시로 보관.
    /// 스킬 착용 씬에서 EquipSkill을 호출하고, 배틀 씬에서 읽어 사용.
    /// GameSaveData.equippedSkillIds와 연동하여 세이브/로드 지원.
    /// </summary>
    public static class EquippedSkillsHolder
    {
        public const int CharacterCount = 4;

        // 캐릭터 인덱스 0~3에 대한 장착 스킬 ID (없으면 빈 문자열)
        private static readonly string[] _equippedSkillIds = new string[CharacterCount];

        public static void EquipSkill(int characterIndex, string skillId)
        {
            if (characterIndex < 0 || characterIndex >= CharacterCount) return;
            _equippedSkillIds[characterIndex] = skillId ?? string.Empty;
        }

        public static string GetEquippedSkillId(int characterIndex)
        {
            if (characterIndex < 0 || characterIndex >= CharacterCount) return string.Empty;
            return _equippedSkillIds[characterIndex] ?? string.Empty;
        }

        public static void ClearAll()
        {
            for (int i = 0; i < CharacterCount; i++)
                _equippedSkillIds[i] = string.Empty;
        }

        /// <summary>
        /// GameSaveData에서 장착 스킬 복원. SkillScene 진입 시 호출.
        /// </summary>
        public static void LoadFromSave(GameSaveData save)
        {
            if (save == null) return;
            if (save.equippedSkillIds == null || save.equippedSkillIds.Length == 0)
            {
                ClearAll();
                return;
            }
            for (int i = 0; i < CharacterCount && i < save.equippedSkillIds.Length; i++)
                _equippedSkillIds[i] = save.equippedSkillIds[i] ?? string.Empty;
        }

        /// <summary>
        /// 현재 장착 스킬을 GameSaveData에 반영. Save 전에 호출.
        /// </summary>
        public static void ApplyToSave(GameSaveData save)
        {
            if (save == null) return;
            if (save.equippedSkillIds == null || save.equippedSkillIds.Length != CharacterCount)
                save.equippedSkillIds = new string[CharacterCount];
            for (int i = 0; i < CharacterCount; i++)
                save.equippedSkillIds[i] = _equippedSkillIds[i] ?? string.Empty;
        }
    }
}

