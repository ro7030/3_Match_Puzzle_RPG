using System;
using System.Collections.Generic;
using System.Linq;
using MainMenu;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Match3Puzzle.Inventory
{
    /// <summary>
    /// InventoryScene(이미지 기반) 컨트롤러.
    /// - 좌측: 장비 슬롯 2개
    /// - 중앙: 상점 카드 4개 (0,2 => 슬롯0 / 1,3 => 슬롯1)
    /// - 우측: 구매/장비 버튼 상태 안내 + 버튼(클릭 시 구매 또는 장착)
    /// </summary>
    public class InventorySceneController : MonoBehaviour
    {
        [Header("데이터베이스")]
        [SerializeField] private EquipmentDatabase equipmentDatabase;

        [Header("뒤로가기")]
        [SerializeField] private Button backButton;
        [SerializeField] private string backSceneName = "KingdomScene";

        [Header("좌측 장비 슬롯")]
        [SerializeField] private EquipmentSlotUI[] equipmentSlots = new EquipmentSlotUI[2];

        [Header("중앙 상점 카드")]
        [SerializeField] private SkillShopItemCardUI[] shopItemCards = new SkillShopItemCardUI[4];

        [Header("우측 패널")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI purchaseHelpText;
        [SerializeField] private Button actionButton;
        [SerializeField] private TextMeshProUGUI actionButtonText;

        private GameSaveData _save;

        private int _selectedCharacterIndex;
        private int _selectedCardIndex = -1;
        private EquipmentData _selectedItem;

        private void Awake()
        {
            if (backButton != null)
                backButton.onClick.AddListener(() => SceneManager.LoadScene(backSceneName));
        }

        private void Start()
        {
            _save = SaveSystem.Load() ?? new GameSaveData();

            // 장착 장비 복원(세이브 연동)
            EquippedEquipmentHolder.LoadFromSave(_save);

            if (equipmentDatabase == null)
                equipmentDatabase = Resources.Load<EquipmentDatabase>("EquipmentDatabase");

            AutoWireIfMissing();
            BindCallbacks();
            RefreshAll();
        }

        private void AutoWireIfMissing()
        {
            if (equipmentSlots == null || equipmentSlots.Length == 0 || equipmentSlots.All(s => s == null))
                equipmentSlots = GetComponentsInChildren<EquipmentSlotUI>(true);

            if (shopItemCards == null || shopItemCards.Length == 0 || shopItemCards.All(s => s == null))
                shopItemCards = GetComponentsInChildren<SkillShopItemCardUI>(true);

            if (goldText == null) goldText = FindTMP("GoldText");
            if (purchaseHelpText == null) purchaseHelpText = FindTMP("PurchaseHelpText");
            if (backButton == null) backButton = FindButton("BackButton");
            if (actionButton == null)
            {
                actionButton = FindButton("ActionButton");
            }

            if (actionButtonText == null)
            {
                if (actionButton != null)
                {
                    var tmpCandidates = actionButton.GetComponentsInChildren<TextMeshProUGUI>(true);
                    for (int i = 0; i < tmpCandidates.Length; i++)
                    {
                        if (tmpCandidates[i] != null && tmpCandidates[i].name == "ActionButtonText")
                        {
                            actionButtonText = tmpCandidates[i];
                            break;
                        }
                    }

                    // 이름이 정확히 안 맞는 경우 대비(가장 첫 번째 TMP)
                    if (actionButtonText == null && tmpCandidates != null && tmpCandidates.Length > 0)
                        actionButtonText = tmpCandidates[0];
                }
            }
        }

        private void BindCallbacks()
        {
            // 장비 슬롯 인덱스(EquipmentSlot_0~1) 자동 매핑
            if (equipmentSlots != null)
            {
                for (int i = 0; i < equipmentSlots.Length; i++)
                {
                    var slot = equipmentSlots[i];
                    if (slot == null) continue;
                    int idx = ParseIndexFromName(slot.name);
                    if (idx >= 0) slot.SetCharacterIndex(idx);
                    slot.SetClickCallback(OnEquipmentSlotClicked);
                }
            }

            // 상점 카드 클릭 콜백 (카드 인덱스 포함)
            if (shopItemCards != null)
            {
                for (int i = 0; i < shopItemCards.Length; i++)
                {
                    var card = shopItemCards[i];
                    if (card == null) continue;
                    card.SetClickCallback(OnShopCardClicked);
                }
            }

            if (actionButton != null)
                actionButton.onClick.AddListener(OnActionButtonClicked);
        }

        private void RefreshAll()
        {
            if (_save.unlockedEquipmentIds == null)
                _save.unlockedEquipmentIds = new List<string>();

            // 기본 선택 슬롯: 0
            _selectedCharacterIndex = Mathf.Clamp(_selectedCharacterIndex, 0, 1);

            RefreshGoldUI();
            RefreshEquipmentSlots();
            RefreshShopCards();
            RefreshActionPanel();
        }

        private void RefreshGoldUI()
        {
            if (goldText != null)
                goldText.text = $"골드: {_save.gold}";
        }

        private void RefreshEquipmentSlots()
        {
            if (equipmentSlots == null) return;

            // 장착 장비를 EquipmentDatabase에서 찾아 아이콘 표시
            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                var slot = equipmentSlots[i];
                if (slot == null) continue;

                int characterIndex = slot.CharacterIndex;
                if (characterIndex < 0 || characterIndex > 1) characterIndex = i;

                string equippedItemId = EquippedEquipmentHolder.GetEquippedId(characterIndex);
                var equippedItem = equipmentDatabase != null ? equipmentDatabase.GetById(equippedItemId) : null;

                slot.Refresh(equippedItem, characterIndex == _selectedCharacterIndex);
            }
        }

        private void RefreshShopCards()
        {
            if (equipmentDatabase == null) return;
            if (shopItemCards == null) return;

            var slot0Items = equipmentDatabase.GetAllBySlot(EquipmentSlotType.Slot0)?.Where(e => e != null).ToList() ?? new List<EquipmentData>();
            var slot1Items = equipmentDatabase.GetAllBySlot(EquipmentSlotType.Slot1)?.Where(e => e != null).ToList() ?? new List<EquipmentData>();

            EquipmentData[] mapped = new EquipmentData[4];
            // 요구사항: 0,2번 카드는 slot0 대상 / 1,3번 카드는 slot1 대상
            mapped[0] = slot0Items.Count > 0 ? slot0Items[0] : null;
            mapped[2] = slot0Items.Count > 1 ? slot0Items[1] : null;
            mapped[1] = slot1Items.Count > 0 ? slot1Items[0] : null;
            mapped[3] = slot1Items.Count > 1 ? slot1Items[1] : null;

            for (int i = 0; i < shopItemCards.Length; i++)
            {
                var card = shopItemCards[i];
                if (card == null) continue;

                var data = i < mapped.Length ? mapped[i] : null;
                if (data != null)
                {
                    int cost = GetItemCostG(data);
                    bool isOwned = IsItemUnlocked(data.equipmentId);
                    bool meetsChapter = _save.lastClearedChapter >= data.requiredChapter;
                    bool canAfford = _save.gold >= cost;
                    card.SetItem(data, cost, isOwned, canAfford, meetsChapter);
                    card.gameObject.SetActive(true);
                }
                else
                {
                    card.SetItem(null, 0, false, false, false);
                    card.gameObject.SetActive(false);
                }
            }

            // 선택 스킬이 유효하지 않으면 첫 번째 유효 카드로 보정
            bool selectedIsVisible = _selectedCardIndex >= 0 &&
                                     _selectedCardIndex < shopItemCards.Length &&
                                     shopItemCards[_selectedCardIndex] != null &&
                                     shopItemCards[_selectedCardIndex].gameObject.activeSelf;
            if (!selectedIsVisible)
            {
                _selectedCardIndex = -1;
                for (int i = 0; i < shopItemCards.Length; i++)
                {
                    if (shopItemCards[i] != null && shopItemCards[i].gameObject.activeSelf)
                    {
                        _selectedCardIndex = i;
                        _selectedItem = shopItemCards[i].GetItemData();
                        _selectedCharacterIndex = GetTargetSlotByCardIndex(i);
                        break;
                    }
                }
            }
        }

        private void RefreshActionPanel()
        {
            if (actionButton == null || actionButtonText == null) return;

            if (_selectedItem == null)
            {
                actionButton.interactable = false;
                actionButtonText.text = "선택 없음";
                if (purchaseHelpText != null) purchaseHelpText.text = "카드를 선택하면 구매/장비 버튼이 활성화됩니다.";
                return;
            }

            bool isOwned = IsItemUnlocked(_selectedItem.equipmentId);
            int cost = GetItemCostG(_selectedItem);
            bool meetsChapter = _save.lastClearedChapter >= _selectedItem.requiredChapter;
            bool canAfford = _save.gold >= cost;

            if (isOwned)
            {
                actionButtonText.text = "장비";
                actionButton.interactable = true;
            }
            else
            {
                actionButtonText.text = "구매";
                actionButton.interactable = meetsChapter && canAfford;
            }

            if (purchaseHelpText != null)
            {
                purchaseHelpText.text =
                    "구매 버튼: 해당하는 골드 이상 존재할 시 구매할 수 있는 버튼이다.\n" +
                    "구매 시 장비 버튼으로 바뀌고 장비를 장착할 수 있게 된다.\n\n" +
                    $"선택: {_selectedItem.displayName}\n" +
                    (isOwned ? "상태: 보유 중" : $"필요 골드: {cost}G" ) +
                    (meetsChapter ? "" : $"\n해금 조건: 챕터 {_selectedItem.requiredChapter} 이후");
            }
        }

        private void OnEquipmentSlotClicked(int characterIndex)
        {
            _selectedCharacterIndex = Mathf.Clamp(characterIndex, 0, 1);

            RefreshEquipmentSlots();
            RefreshShopCards();
            RefreshActionPanel();
        }

        private void OnShopCardClicked(EquipmentData data, int cardIndex)
        {
            _selectedCardIndex = cardIndex;
            _selectedItem = data;
            _selectedCharacterIndex = GetTargetSlotByCardIndex(cardIndex);
            RefreshEquipmentSlots();
            RefreshActionPanel();
        }

        private void OnActionButtonClicked()
        {
            if (_selectedItem == null) return;

            bool isOwned = IsItemUnlocked(_selectedItem.equipmentId);
            int cost = GetItemCostG(_selectedItem);

            if (isOwned)
            {
                // 장착
                EquippedEquipmentHolder.Equip(_selectedCharacterIndex, _selectedItem.equipmentId);
                EquippedEquipmentHolder.ApplyToSave(_save);
                SaveSystem.Save(_save);

                RefreshEquipmentSlots();
                RefreshActionPanel();
                return;
            }

            // 구매
            bool meetsChapter = _save.lastClearedChapter >= _selectedItem.requiredChapter;
            bool canAfford = _save.gold >= cost;
            if (!meetsChapter || !canAfford) return;

            _save.gold -= cost;
            if (!_save.unlockedEquipmentIds.Contains(_selectedItem.equipmentId))
                _save.unlockedEquipmentIds.Add(_selectedItem.equipmentId);

            // 저장에 장착 스킬은 아직 반영 안 하므로, 구매 후 "장비"를 누르면 장착된다.
            SaveSystem.Save(_save);

            RefreshShopCards(); // 카드 상태(owned) 반영
            RefreshActionPanel();
        }

        private bool IsItemUnlocked(string equipmentId)
        {
            if (string.IsNullOrEmpty(equipmentId)) return false;
            if (_save.unlockedEquipmentIds == null) return false;
            return _save.unlockedEquipmentIds.Contains(equipmentId);
        }

        private static int GetTargetSlotByCardIndex(int cardIndex)
        {
            // 요구사항 매핑:
            // 0,2 -> EquipmentSlot_0
            // 1,3 -> EquipmentSlot_1
            if (cardIndex == 0 || cardIndex == 2) return 0;
            return 1;
        }

        private static int GetItemCostG(EquipmentData data)
        {
            if (data == null) return 0;
            return Mathf.Max(0, data.goldCost);
        }

        private static int ParseIndexFromName(string name)
        {
            // 예: EquipmentSlot_0 / ShopItemCard_2
            if (string.IsNullOrEmpty(name)) return -1;
            int underscore = name.LastIndexOf('_');
            if (underscore < 0 || underscore >= name.Length - 1) return -1;
            var suffix = name.Substring(underscore + 1);
            if (int.TryParse(suffix, out var idx)) return idx;
            return -1;
        }

        private static TextMeshProUGUI FindTMP(string name)
        {
            var tmps = GameObject.FindObjectsOfType<TextMeshProUGUI>(true);
            for (int i = 0; i < tmps.Length; i++)
            {
                if (tmps[i] != null && tmps[i].name == name)
                    return tmps[i];
            }
            return null;
        }

        private static Button FindButton(string name)
        {
            var buttons = GameObject.FindObjectsOfType<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null && buttons[i].name == name)
                    return buttons[i];
            }
            return null;
        }
    }
}

