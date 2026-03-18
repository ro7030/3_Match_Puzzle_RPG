using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Match3Puzzle.Stats;
using MainMenu;

namespace Match3Puzzle.Status
{
    /// <summary>
    /// StatusScene 메인 컨트롤러.
    ///
    /// 레벨 시스템
    ///   - 신규 스테이지를 클리어할 때마다 playerLevel +1, upgradePoints +1
    ///   - StatusScene에서 Lv UP 버튼을 누르면 upgradePoints -1, 해당 스탯 레벨 +1
    ///   - 포인트가 0이면 업그레이드 불가
    ///
    /// 슬롯 인덱스 매핑
    ///   0 → 검(Sword)   데미지
    ///   1 → 활(Bow)     데미지
    ///   2 → 마법(Wand)  데미지
    ///   3 → 힐(Heal)    회복량
    /// </summary>
    public class StatusSceneController : MonoBehaviour
    {
        [Header("데이터베이스")]
        [Tooltip("CharacterDatabase ScriptableObject 연결")]
        [SerializeField] private Story.CharacterDatabase characterDatabase;

        [Tooltip("CharacterStats ScriptableObject 연결. 비워 두면 Resources/CharacterStats 자동 로드")]
        [SerializeField] private CharacterStatsData statsData;

        [Header("캐릭터 슬롯 UI (0=검, 1=활, 2=마법, 3=힐)")]
        [SerializeField] private CharacterStatusSlotUI[] slots = new CharacterStatusSlotUI[4];

        [Header("슬롯별 캐릭터 ID (CharacterDatabase 기준)")]
        [SerializeField] private int[] characterIds = { 0, 2, 1, 3 }; // 0=검사, 1=마법사, 2=궁수, 3=힐러

        [Header("플레이어 정보 표시 (선택)")]
        [Tooltip("플레이어 레벨 텍스트 (예: Lv.5)")]
        [SerializeField] private TextMeshProUGUI playerLevelText;

        [Tooltip("남은 업그레이드 포인트 텍스트 (예: 포인트: 2)")]
        [SerializeField] private TextMeshProUGUI upgradePointsText;

        [Header("뒤로가기")]
        [SerializeField] private Button backButton;
        [SerializeField] private string backSceneName = "KingdomScene";

        private GameSaveData _save;

        // ── 초기화 ────────────────────────────────────────────────────────

        private void Awake()
        {
            backButton?.onClick.AddListener(() => SceneManager.LoadScene(backSceneName));
        }

        private void Start()
        {
            _save = SaveSystem.Load() ?? new GameSaveData();
            CharacterUpgradeHolder.LoadFromSave(_save);

            if (statsData == null)
                statsData = CharacterStatsResolver.LoadFromResources();

            RefreshAll();
        }

        // ── UI 갱신 ──────────────────────────────────────────────────────

        private void RefreshAll()
        {
            if (slots == null) return;

            bool hasPoint = _save.upgradePoints > 0;

            for (int i = 0; i < slots.Length && i < 4; i++)
            {
                if (slots[i] == null) continue;

                int capturedIndex = i;
                slots[i].Refresh(
                    character:    GetCharacter(i),
                    statLabel:    GetStatLabel(i),
                    level:        GetLevel(i),
                    currentValue: GetCurrentValue(i),
                    nextValue:    GetNextValue(i),
                    canUpgrade:   hasPoint
                );
                slots[i].SetUpgradeCallback(() => OnLevelUp(capturedIndex));
            }

            RefreshPlayerInfo();
        }

        private void RefreshPlayerInfo()
        {
            if (playerLevelText   != null) playerLevelText.text   = $"Lv.{_save.playerLevel}";
            if (upgradePointsText != null) upgradePointsText.text = $"업그레이드 포인트: {_save.upgradePoints}";
        }

        // ── 업그레이드 처리 ───────────────────────────────────────────────

        private void OnLevelUp(int statIndex)
        {
            if (_save.upgradePoints <= 0)
            {
                Debug.Log("[StatusScene] 업그레이드 포인트 부족. 스테이지를 클리어하면 포인트가 지급됩니다.");
                return;
            }

            _save.upgradePoints--;
            SetLevel(statIndex, GetLevel(statIndex) + 1);

            CharacterUpgradeHolder.ApplyToSave(_save);
            SaveSystem.Save(_save);

            RefreshAll();
        }

        // ── 헬퍼: 캐릭터 조회 ────────────────────────────────────────────

        private Story.DataCharacter GetCharacter(int slotIndex)
        {
            if (characterDatabase == null) return null;
            if (slotIndex < 0 || slotIndex >= characterIds.Length) return null;
            return characterDatabase.Get(characterIds[slotIndex]);
        }

        // ── 헬퍼: 레벨 ───────────────────────────────────────────────────

        private static int GetLevel(int idx) => idx switch
        {
            0 => CharacterUpgradeHolder.SwordLevel,
            1 => CharacterUpgradeHolder.BowLevel,
            2 => CharacterUpgradeHolder.WandLevel,
            3 => CharacterUpgradeHolder.HealLevel,
            _ => 0
        };

        private static void SetLevel(int idx, int value)
        {
            switch (idx)
            {
                case 0: CharacterUpgradeHolder.SwordLevel = value; break;
                case 1: CharacterUpgradeHolder.BowLevel   = value; break;
                case 2: CharacterUpgradeHolder.WandLevel  = value; break;
                case 3: CharacterUpgradeHolder.HealLevel  = value; break;
            }
        }

        // ── 헬퍼: 수치 계산 ──────────────────────────────────────────────

        private int GetCurrentValue(int idx)
        {
            if (statsData == null) return 0;
            return idx switch
            {
                0 => CharacterStatsResolver.GetSwordDamage(statsData),
                1 => CharacterStatsResolver.GetBowDamage(statsData),
                2 => CharacterStatsResolver.GetWandDamage(statsData),
                3 => CharacterStatsResolver.GetHealAmount(statsData),
                _ => 0
            };
        }

        /// <summary>레벨을 1 올렸을 때의 수치를 상태 변경 없이 계산</summary>
        private int GetNextValue(int idx)
        {
            if (statsData == null) return 0;
            return idx switch
            {
                0 => statsData.baseSwordDamage + (CharacterUpgradeHolder.SwordLevel + 1) * statsData.swordBonusPerLevel,
                1 => statsData.baseBowDamage   + (CharacterUpgradeHolder.BowLevel   + 1) * statsData.bowBonusPerLevel,
                2 => statsData.baseWandDamage  + (CharacterUpgradeHolder.WandLevel  + 1) * statsData.wandBonusPerLevel,
                3 => statsData.baseHealAmount  + (CharacterUpgradeHolder.HealLevel  + 1) * statsData.healBonusPerLevel,
                _ => 0
            };
        }

        // ── 헬퍼: 라벨 ───────────────────────────────────────────────────

        private static string GetStatLabel(int idx) => idx switch
        {
            0 => "검 데미지",
            1 => "활 데미지",
            2 => "마법 데미지",
            3 => "회복량",
            _ => "?"
        };
    }
}
