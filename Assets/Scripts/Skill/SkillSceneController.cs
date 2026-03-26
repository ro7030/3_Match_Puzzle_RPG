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
    /// 진입 시 전원 흑백, 캐릭터 호버 시 위로 이동, 클릭 시 컬러+글로우+하단 스킬 목록 표시.
    /// 스킬 아이콘 호버 시 이름·설명, 클릭 시 장착 및 중앙 아이콘 갱신.
    /// </summary>
    public class SkillSceneController : MonoBehaviour
    {
        [Header("데이터베이스")]
        [SerializeField] private Story.CharacterDatabase characterDatabase;
        [SerializeField] private SkillDatabase skillDatabase;

        [Header("캐릭터 슬롯 UI (0~3)")]
        [SerializeField] private CharacterSkillSlotUI[] characterSlots = new CharacterSkillSlotUI[4];

        [Header("슬롯별 캐릭터 ID")]
        [SerializeField] private int[] characterIds = { 0, 1, 2, 3 };

        [Header("스킬 선택 (하단 슬롯 행)")]
        [Tooltip("비활성화했다가 캐릭터 선택 시 켜는 오브젝트 (기존 SkillSelectionPanel 또는 SkillSlots 루트)")]
        [SerializeField] private GameObject skillSelectionPanel;

        [Tooltip("캐릭터별 스킬 슬롯 그룹 (SkillSlots0~3). 비어 있으면 단일 skillSelectionSlots를 사용합니다.")]
        [SerializeField] private GameObject[] skillSlotGroups = new GameObject[4];

        [SerializeField] private SkillSelectionSlotUI[] skillSelectionSlots = new SkillSelectionSlotUI[3];

        [Header("스킬 정보 표시 (맨 아래)")]
        [SerializeField] private Image skillInfoIconImage;
        [SerializeField] private TextMeshProUGUI skillNameText;
        [SerializeField] private TextMeshProUGUI skillEffectText;

        [Header("뒤로가기")]
        [SerializeField] private Button backButton;
        [SerializeField] private string backSceneName = "KingdomScene";

        private GameSaveData _save;
        private int _selectingCharacterIndex = -1;
        private SkillSelectionSlotUI[][] _groupSlots;

        private void Awake()
        {
            backButton?.onClick.AddListener(() => SceneManager.LoadScene(backSceneName));
            if (skillSelectionPanel != null)
                skillSelectionPanel.SetActive(false);

            AutoWireSkillInfoReferencesIfMissing();
        }

        private void Start()
        {
            _save = SaveSystem.Load() ?? new GameSaveData();
            EquippedSkillsHolder.LoadFromSave(_save);

            if (skillDatabase == null)
                skillDatabase = Resources.Load<SkillDatabase>("SkillDatabase");

            AutoWireSkillInfoReferencesIfMissing();
            ApplyInitialEquippedSkillsIfNeeded();
            SetupCharacterSlotCallbacks();
            SetupSkillSelectionCallbacks();
            ClearSkillInfo();
            RefreshAll();
        }

        private void ApplyInitialEquippedSkillsIfNeeded()
        {
            if (skillDatabase == null) return;

            // 현재 장착이 전부 비어있거나(신규), 전부 0번 스킬로 동일하게 들어가 있는 경우에만 기본값(0,3,6,9)을 적용.
            string default0SkillId = skillDatabase.GetByIndex(0) != null ? skillDatabase.GetByIndex(0).skillId : string.Empty;
            bool allEquippedEmpty = true;
            bool allEquippedSameAs0 = true;
            if (string.IsNullOrEmpty(default0SkillId))
                allEquippedSameAs0 = false;

            for (int i = 0; i < EquippedSkillsHolder.CharacterCount; i++)
            {
                string id = EquippedSkillsHolder.GetEquippedSkillId(i);
                if (!string.IsNullOrEmpty(id))
                    allEquippedEmpty = false;
                if (!string.IsNullOrEmpty(default0SkillId) && id != default0SkillId)
                    allEquippedSameAs0 = false;
            }

            if (!allEquippedEmpty && !allEquippedSameAs0) return;

            for (int i = 0; i < EquippedSkillsHolder.CharacterCount; i++)
            {
                int defaultIndex = GetDefaultSkillIndex(i); // 0,3,6,9
                var sd = skillDatabase.GetByIndex(defaultIndex);
                EquippedSkillsHolder.EquipSkill(i, sd != null ? sd.name : string.Empty);
            }

            EquippedSkillsHolder.ApplyToSave(_save);
            SaveSystem.Save(_save);
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
            CacheGroupedSlots();

            if (_groupSlots != null && _groupSlots.Length > 0)
            {
                for (int g = 0; g < _groupSlots.Length; g++)
                {
                    var group = _groupSlots[g];
                    if (group == null) continue;
                    for (int i = 0; i < group.Length; i++)
                    {
                        var slot = group[i];
                        if (slot == null) continue;
                        slot.SetClickCallback(() => OnSkillSelectionSlotClicked(slot));
                        slot.SetHoverCallbacks(
                            data =>
                            {
                                if (data != null)
                                    ShowSkillInfo(data);
                            },
                            ClearSkillInfo);
                    }
                }

                return;
            }

            if (skillSelectionSlots == null) return;
            for (int i = 0; i < skillSelectionSlots.Length; i++)
            {
                var slot = skillSelectionSlots[i];
                if (slot == null) continue;
                slot.SetClickCallback(() => OnSkillSelectionSlotClicked(slot));
                slot.SetHoverCallbacks(
                    data =>
                    {
                        if (data != null)
                            ShowSkillInfo(data);
                    },
                    ClearSkillInfo);
            }
        }

        private void OnCharacterSlotClicked(int characterIndex)
        {
            _selectingCharacterIndex = characterIndex;
            OpenSkillSelectionPanel(characterIndex);
            RefreshAll();
        }

        private void OpenSkillSelectionPanel(int characterIndex)
        {
            if (skillSelectionPanel == null) return;

            var activeSlots = GetSlotsForCharacter(characterIndex);
            int slotCount = activeSlots != null ? activeSlots.Length : 0;
            var candidateSkills = GetSkillCandidatesForCharacter(characterIndex, slotCount);

            for (int i = 0; i < slotCount; i++)
            {
                var slot = activeSlots[i];
                if (slot == null) continue;

                var data = i < candidateSkills.Length ? candidateSkills[i] : null;
                bool locked = data == null || !IsSkillUnlocked(data);
                slot.SetSkill(data, locked);

                // 캐릭터 선택 시 슬롯 3칸은 항상 보이게 유지
                slot.gameObject.SetActive(true);
            }

            ClearSkillInfo();
            skillSelectionPanel.SetActive(true);
        }

        private SkillData[] GetSkillCandidatesForCharacter(int characterIndex, int desiredCount)
        {
            int count = desiredCount > 0 ? desiredCount : 3;
            var result = new SkillData[count];
            if (skillDatabase == null) return result;

            // 우선순위 1: SkillDatabase index 묶음(캐릭터별 3개)
            bool anyFromBlock = false;
            int baseIndex = characterIndex * 3;
            for (int i = 0; i < count; i++)
            {
                var s = skillDatabase.GetByIndex(baseIndex + i);
                result[i] = s;
                if (s != null) anyFromBlock = true;
            }

            // 전부 비어있으면 ownerCharacterIndex 기반으로 fallback
            if (!anyFromBlock)
            {
                var list = skillDatabase.GetAllForCharacter(characterIndex).ToList();
                for (int i = 0; i < count; i++)
                    result[i] = i < list.Count ? list[i] : null;
            }

            return result;
        }

        private bool IsSkillUnlocked(SkillData skill)
        {
            if (skill == null) return false;
            int lastChapter = _save?.lastClearedChapter ?? 0;
            var unlocked = _save?.unlockedSkillIds ?? new List<string>();

            if (lastChapter < skill.requiredChapter) return false;
            if (unlocked.Count > 0 && !unlocked.Contains(skill.skillId)) return false;
            return true;
        }

        private void OnSkillSelectionSlotClicked(SkillSelectionSlotUI slot)
        {
            if (_selectingCharacterIndex < 0) return;
            if (slot == null || !slot.gameObject.activeSelf) return;

            var data = slot.GetSkillData();
            if (data == null) return;

            EquippedSkillsHolder.EquipSkill(_selectingCharacterIndex, data.name);
            EquippedSkillsHolder.ApplyToSave(_save);
            SaveSystem.Save(_save);

            ShowSkillInfo(data);
            RefreshAll();
        }

        private void CacheGroupedSlots()
        {
            EnsureSkillSlotGroupsFound();

            if (skillSlotGroups == null || skillSlotGroups.Length == 0)
            {
                _groupSlots = null;
                return;
            }

            _groupSlots = new SkillSelectionSlotUI[skillSlotGroups.Length][];
            bool hasAny = false;
            for (int i = 0; i < skillSlotGroups.Length; i++)
            {
                var go = skillSlotGroups[i];
                if (go == null)
                {
                    _groupSlots[i] = null;
                    continue;
                }

                var slots = go.GetComponentsInChildren<SkillSelectionSlotUI>(true);
                _groupSlots[i] = slots;
                if (slots != null && slots.Length > 0)
                    hasAny = true;
            }

            if (!hasAny)
                _groupSlots = null;
        }

        private void EnsureSkillSlotGroupsFound()
        {
            if (skillSelectionPanel == null)
                return;

            if (skillSlotGroups == null || skillSlotGroups.Length != 4)
                skillSlotGroups = new GameObject[4];

            var panelTransform = skillSelectionPanel.transform;
            for (int i = 0; i < 4; i++)
            {
                if (skillSlotGroups[i] != null)
                    continue;

                // 사용자가 만들 때 이름 규칙이 조금 달라도 동작하도록 후보를 몇 개 허용
                var candidates = new string[]
                {
                    $"SkillSlots{i}",
                    $"SkillSlots_{i}",
                    $"SkillSlots_{i:D1}",
                    $"SkillSlots {i}",
                    $"skillslots{i}",
                    $"skillslots_{i}",
                };

                GameObject found = null;
                for (int c = 0; c < candidates.Length; c++)
                {
                    var tf = panelTransform.Find(candidates[c]);
                    if (tf != null)
                    {
                        found = tf.gameObject;
                        break;
                    }
                }

                if (found == null)
                {
                    var all = panelTransform.GetComponentsInChildren<Transform>(true);
                    for (int t = 0; t < all.Length; t++)
                    {
                        var name = all[t] != null ? all[t].name : "";
                        for (int c = 0; c < candidates.Length; c++)
                        {
                            if (string.Equals(name, candidates[c], System.StringComparison.OrdinalIgnoreCase))
                            {
                                found = all[t].gameObject;
                                break;
                            }
                        }
                        if (found != null) break;
                    }
                }

                // 없으면 조용히 두고, 대신 씬의 기본 skillSelectionSlots를 사용(기존 호환)
                skillSlotGroups[i] = found;
            }
        }

        private SkillSelectionSlotUI[] GetSlotsForCharacter(int characterIndex)
        {
            if (_groupSlots != null && characterIndex >= 0 && characterIndex < _groupSlots.Length)
            {
                for (int i = 0; i < skillSlotGroups.Length; i++)
                {
                    var group = skillSlotGroups[i];
                    if (group != null)
                        group.SetActive(i == characterIndex);
                }

                var selectedGroup = _groupSlots[characterIndex];
                if (selectedGroup != null && selectedGroup.Length > 0)
                    return selectedGroup;
            }

            return skillSelectionSlots ?? new SkillSelectionSlotUI[0];
        }

        private void ShowSkillInfo(SkillData data)
        {
            AutoWireSkillInfoReferencesIfMissing();
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
                skillNameText.text = data != null ? data.displayName : "";
            if (skillEffectText != null)
                skillEffectText.text = data != null
                    ? (!string.IsNullOrEmpty(data.description) ? data.description : GetEffectDescription(data))
                    : "";
        }

        private void AutoWireSkillInfoReferencesIfMissing()
        {
            Transform root = transform.root;

            if (skillInfoIconImage == null)
                skillInfoIconImage = FindImageByName(root, "SkillIconImage");
            if (skillNameText == null)
                skillNameText = FindTMPByName(root, "SkillNameText");
            if (skillEffectText == null)
                skillEffectText = FindTMPByName(root, "SkillEffectText");
        }

        private static Image FindImageByName(Transform root, string name)
        {
            var images = root.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].name == name)
                    return images[i];
            }

            return null;
        }

        private static TextMeshProUGUI FindTMPByName(Transform root, string name)
        {
            var tmps = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < tmps.Length; i++)
            {
                if (tmps[i] != null && tmps[i].name == name)
                    return tmps[i];
            }

            return null;
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
            if (skillSlotGroups != null)
            {
                for (int i = 0; i < skillSlotGroups.Length; i++)
                {
                    if (skillSlotGroups[i] != null)
                        skillSlotGroups[i].SetActive(false);
                }
            }
            _selectingCharacterIndex = -1;
            ClearSkillInfo();
            RefreshAll();
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
                        ? skillDatabase.GetByKey(skillId)
                        : skillDatabase.GetByIndex(GetDefaultSkillIndex(i));
                }

                bool selected = _selectingCharacterIndex == i;
                characterSlots[i].Refresh(character, skillData, selected);
            }
        }

        private static int GetDefaultSkillIndex(int characterIndex) => characterIndex * 3;
    }
}
