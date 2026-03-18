using System;
using System.Collections.Generic;

namespace MainMenu
{
    /// <summary>
    /// 저장/로드에 사용할 게임 데이터 구조
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public int lastClearedChapter = 0;

        /// <summary>
        /// 마지막으로 클리어한 스테이지의 전역 인덱스.
        /// -1 = 아직 아무 스테이지도 클리어하지 않은 상태.
        /// Index 0=Tutorial / 챕터1: 1~3 / 챕터2: 4~6 / 챕터3: 7~9
        /// 챕터2 해금 조건: >= 3 (챕터1 Stage3 클리어)
        /// 챕터3 해금 조건: >= 6 (챕터2 Stage3 클리어)
        /// </summary>
        public int lastClearedStageIndex = -1;

        public int gold = 0;
        public List<string> unlockedSkillIds = new List<string>();
        public string lastPlayTimeUtc = ""; // ISO 8601 문자열
        public string currentSceneName = "";
        public int playTimeSeconds = 0;

        /// <summary>
        /// 플레이어 레벨. 신규 스테이지를 클리어할 때마다 +1.
        /// </summary>
        public int playerLevel = 0;

        /// <summary>
        /// 사용 가능한 업그레이드 포인트.
        /// 신규 스테이지 클리어 시 +1, 캐릭터 스탯 Lv UP 시 -1.
        /// </summary>
        public int upgradePoints = 0;

        /// <summary>
        /// 스탯 업그레이드 레벨 배열. 인덱스는 CharacterUpgradeHolder.Index* 상수와 동일.
        /// [0]=검, [1]=활, [2]=마법, [3]=힐, [4]=강화타일
        /// </summary>
        public int[] statUpgradeLevels = new int[5];

        /// <summary>
        /// 캐릭터별 장착 스킬 ID. [0]=캐릭터0, [1]=캐릭터1, [2]=캐릭터2, [3]=캐릭터3.
        /// 빈 문자열 = 미장착.
        /// </summary>
        public string[] equippedSkillIds = new string[4];

        public GameSaveData() { }

        public GameSaveData(int chapter, int goldAmount, List<string> skills, string sceneName, int totalPlaySeconds = 0)
        {
            lastClearedChapter = chapter;
            gold = goldAmount;
            unlockedSkillIds = skills ?? new List<string>();
            lastPlayTimeUtc = DateTime.UtcNow.ToString("o");
            currentSceneName = sceneName;
            playTimeSeconds = totalPlaySeconds;
        }
    }
}
