using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using MainMenu;

namespace Match3Puzzle.Skill
{
    /// <summary>
    /// SkillScene 메인 컨트롤러.
    ///
    /// 상단: 캐릭터 4명 + 각 캐릭터의 장착 스킬. 클릭 시 스킬 선택 패널 표시.
    /// 하단: 선택 가능 스킬 목록. 클릭 시 해당 캐릭터에 장착.
    /// </summary>
    public class SkillSceneController : MonoBehaviour
    {
        [Header("데이터베이스")]
        [SerializeField] private Story.CharacterDatabase characterDatabase;
        [SerializeField] private SkillDatabase skillDatabase;

        [Header("캐릭터 슬롯 UI (0~3)")]
        [SerializeField] private CharacterSkillSlotUI[] characterSlots = new CharacterSkillSlotUI[4];

        [Header("슬롯별 캐릭터 ID")]
        [SerializeField] private int[] characterIds = { 0, 2, 1, 3 }; // 0=검사, 1=마법사, 2=궁수, 3=힐러

        [Header("스킬 선택 패널")]
        [Tooltip("상단 슬롯 클릭 시 표시되는 패널")]
        [SerializeField] private GameObject skillSelectionPanel;

        [Tooltip("선택 가능 스킬 슬롯 (해당 캐릭터의 스킬 중 unlockedSkillIds에 포함된 것)")]
        [SerializeField] private SkillSelectionSlotUI[] skillSelectionSlots = new SkillSelectionSlotUI[3];

        [Header("스킬 정보 표시 (선택한 스킬)")]
        [Tooltip("스킬 이름·효과 왼쪽에 표시할 작은 아이콘")]
        [SerializeField] private Image skillInfoIconImage;
        [SerializeField] private TextMeshProUGUI skillNameText;
        [SerializeField] private TextMeshProUGUI skillEffectText;

        [Header("뒤로가기")]
        [SerializeField] private Button backButton;
        [SerializeField] private string backSceneName = "KingdomScene";

        private GameSaveData _save;
        private int _selectingCharacterIndex = -1;

        private void Awake()
        {
            backButton?.onClick.AddListener(() => SceneManager.LoadScene(backSceneName));
            if (skillSelectionPanel != null)
                skillSelectionPanel.SetActive(false);
        }

        private void Start()
        {
            _save = SaveSystem.Load() ?? new GameSaveData();
            EquippedSkillsHolder.LoadFromSave(_save);

            if (skillDatabase == null)
                skillDatabase = Resources.Load<SkillDatabase>("SkillDatabase");

            RefreshAll();
            SetupCharacterSlotCallbacks();
            SetupSkillSelectionCallbacks();
        }

        private void SetupCharacterSlotCallbacks()
        {
            if (characterSlots == null) return;
            for (int i = 0; i < characterSlots.Length; i++)
            {
                if (characterSlots[i] == null) continue;
                int idx = i;
                characterSlots[i].SetClickCallback(() => OnCharacterSlotClicked(idx));
            }
        }

        private void SetupSkillSelectionCallbacks()
        {
            if (skillSelectionSlots == null) return;
            for (int i = 0; i < skillSelectionSlots.Length; i++)
            {
                if (skillSelectionSlots[i] == null) continue;
                int idx = i;
                skillSelectionSlots[i].SetClickCallback(() => OnSkillSelectionSlotClicked(idx));
            }
        }

        private void OnCharacterSlotClicked(int characterIndex)
        {
            _selectingCharacterIndex = characterIndex;
            OpenSkillSelectionPanel(characterIndex);
        }

        private void OpenSkillSelectionPanel(int characterIndex)
        {
            if (skillSelectionPanel == null) return;

            var availableSkills = GetAvailableSkillsForCharacter(characterIndex);
            for (int i = 0; i < skillSelectionSlots.Length; i++)
            {
                if (skillSelectionSlots[i] == null) continue;
                if (i < availableSkills.Count)
                {
                    var data = availableSkills[i];
                    skillSelectionSlots[i].SetSkill(data);
                    skillSelectionSlots[i].gameObject.SetActive(true);
                }
                else
                {
                    skillSelectionSlots[i].SetSkill(null);
                    skillSelectionSlots[i].gameObject.SetActive(false);
                }
            }

            ClearSkillInfo();
            skillSelectionPanel.SetActive(true);
        }

        private List<SkillData> GetAvailableSkillsForCharacter(int characterIndex)
        {
            if (skillDatabase == null) return new List<SkillData>();
            var all = skillDatabase.GetAllForCharacter(characterIndex);
            int lastChapter = _save?.lastClearedChapter ?? 0;
            var unlocked = _save?.unlockedSkillIds ?? new List<string>();

            var filtered = all.Where(s =>
            {
                if (s == null) return false;
                // 챕터 해금 조건: requiredChapter <= lastClearedChapter
                if (lastChapter < s.requiredChapter) return false;
                // unlockedSkillIds가 있으면 목록에 포함된 것만, 없으면 전부
                if (unlocked.Count > 0 && !unlocked.Contains(s.skillId)) return false;
                return true;
            });
            return filtered.Take(skillSelectionSlots?.Length ?? 3).ToList();
        }

        private void OnSkillSelectionSlotClicked(int slotIndex)
        {
            if (_selectingCharacterIndex < 0 || slotIndex >= skillSelectionSlots.Length) return;
            var slot = skillSelectionSlots[slotIndex];
            if (slot == null || !slot.gameObject.activeSelf) return;

            var data = slot.GetSkillData();
            if (data == null) return;

            EquippedSkillsHolder.EquipSkill(_selectingCharacterIndex, data.skillId);
            EquippedSkillsHolder.ApplyToSave(_save);
            SaveSystem.Save(_save);

            ShowSkillInfo(data);
            RefreshAll();
            CloseSkillSelectionPanel();
        }

        /// <summary>
        /// 스킬 선택 시 하단에 이름·효과 표시. (클릭만 해도 표시, 장착은 별도)
        /// 실제로는 OnSkillSelectionSlotClicked에서 장착 + 표시 동시 처리.
        /// </summary>
        private void ShowSkillInfo(SkillData data)
        {
            if (skillInfoIconImage != null)
            {
                if (data != null && data.icon != null)
                {
                    skillInfoIconImage.sprite = data.icon;
                    skillInfoIconImage.enabled = true;
                }
                else
                {
                    skillInfoIconImage.enabled = false;
                }
            }
            if (skillNameText != null)
                skillNameText.text = data.displayName;
            if (skillEffectText != null)
                skillEffectText.text = !string.IsNullOrEmpty(data.description) ? data.description : GetEffectDescription(data);
        }

        private void ClearSkillInfo()
        {
            if (skillInfoIconImage != null)
            {
                skillInfoIconImage.sprite = null;
                skillInfoIconImage.enabled = false;
            }
            if (skillNameText != null) skillNameText.text = "";
            if (skillEffectText != null) skillEffectText.text = "";
        }

        private static string GetEffectDescription(SkillData data)
        {
            if (data == null) return "";
            int percent = Mathf.RoundToInt(data.effectMultiplier * 100f);
            return data.effectType switch
            {
                SkillEffectType.Sword => $"{percent}% 검 데미지",
                SkillEffectType.Bow => $"{percent}% 활 데미지",
                SkillEffectType.Wand => $"{percent}% 마법 데미지",
                SkillEffectType.Heal => $"{percent}% 회복량",
                _ => $"{percent}%"
            };
        }

        public void CloseSkillSelectionPanel()
        {
            if (skillSelectionPanel != null)
                skillSelectionPanel.SetActive(false);
            _selectingCharacterIndex = -1;
        }

        private Story.DataCharacter GetCharacter(int slotIndex)
        {
            if (characterDatabase == null) return null;
            if (slotIndex < 0 || slotIndex >= characterIds.Length) return null;
            return characterDatabase.Get(characterIds[slotIndex]);
        }

        private void RefreshAll()
        {
            if (characterSlots == null) return;

            for (int i = 0; i < characterSlots.Length && i < 4; i++)
            {
                if (characterSlots[i] == null) continue;

                var character = GetCharacter(i);
                string skillId = EquippedSkillsHolder.GetEquippedSkillId(i);
                SkillData skillData = null;
                if (skillDatabase != null)
                {
                    skillData = !string.IsNullOrEmpty(skillId)
                        ? skillDatabase.GetById(skillId)
                        : skillDatabase.GetByIndex(GetDefaultSkillIndex(i));
                }

                characterSlots[i].Refresh(character, skillData);
            }
        }

        /// <summary>미장착 시 기본 스킬 인덱스: 캐릭터0=0, 캐릭터1=3, 캐릭터2=6, 캐릭터3=9</summary>
        private static int GetDefaultSkillIndex(int characterIndex) => characterIndex * 3;
    }
}
