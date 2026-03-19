using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Match3Puzzle.Inventory
{
    /// <summary>
    /// 중앙 "상점 아이템 카드" UI 1개.
    /// 클릭 시 선택 장비를 InventorySceneController에 전달한다.
    /// </summary>
    public class SkillShopItemCardUI : MonoBehaviour
    {
        [SerializeField] private Button cardButton;
        [SerializeField] private Image itemIconImage;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemCostText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;

        private EquipmentData _itemData;
        private Action<EquipmentData, int> _clickCallback;

        public void SetClickCallback(Action<EquipmentData, int> callback) => _clickCallback = callback;

        public EquipmentData GetItemData() => _itemData;

        private void Awake()
        {
            if (cardButton == null) cardButton = GetComponent<Button>();
            if (itemIconImage == null) itemIconImage = FindChildImage("ItemIconImage") ?? FindChildImage("SkillIconImage");
            if (itemNameText == null) itemNameText = FindChildTMP("ItemNameText") ?? FindChildTMP("SkillNameText");
            if (itemCostText == null) itemCostText = FindChildTMP("ItemCostText") ?? FindChildTMP("SkillCostText");
            if (itemDescriptionText == null) itemDescriptionText = FindChildTMP("ItemDescriptionText");

            if (cardButton != null)
                cardButton.onClick.AddListener(() => _clickCallback?.Invoke(_itemData, ParseIndexFromName(gameObject.name)));
        }

        public void SetItem(EquipmentData data, int cost, bool isOwned, bool canAfford, bool meetsChapter)
        {
            _itemData = data;

            bool hasData = data != null;

            if (itemIconImage != null)
            {
                if (hasData && data.icon != null)
                {
                    itemIconImage.sprite = data.icon;
                    itemIconImage.enabled = true;
                }
                else
                {
                    itemIconImage.enabled = false;
                }
            }

            if (itemNameText != null)
            {
                itemNameText.text = hasData ? data.displayName : "";
                itemNameText.enabled = hasData;
            }

            if (itemCostText != null)
            {
                itemCostText.text = hasData ? $"{cost}G" : "";
                itemCostText.enabled = hasData;
            }

            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = hasData ? data.description : "";
                itemDescriptionText.enabled = hasData;
            }

            // 카드 자체는 "선택"만 목적이므로 클릭 가능 여부는 controller가 최종 판단한다.
            if (cardButton != null)
                cardButton.interactable = hasData;

            // 시각적 힌트(회색/알파) - 완전히 필수는 아니지만 이미지 가독성을 위해 간단 적용
            if (cardButton != null)
            {
                // isOwned면 밝게, 아니면 기본/부족 상태를 알파로만 반영
                float alpha = isOwned ? 1f : (canAfford && meetsChapter ? 1f : 0.55f);
                var colors = cardButton.colors;
                colors.normalColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, alpha);
                colors.highlightedColor = new Color(colors.highlightedColor.r, colors.highlightedColor.g, colors.highlightedColor.b, alpha);
                colors.pressedColor = new Color(colors.pressedColor.r, colors.pressedColor.g, colors.pressedColor.b, alpha);
                colors.selectedColor = new Color(colors.selectedColor.r, colors.selectedColor.g, colors.selectedColor.b, alpha);
                colors.disabledColor = new Color(colors.disabledColor.r, colors.disabledColor.g, colors.disabledColor.b, alpha);
                cardButton.colors = colors;
            }
        }

        private Image FindChildImage(string childName)
        {
            var images = GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].name == childName)
                    return images[i];
            }
            return null;
        }

        private TextMeshProUGUI FindChildTMP(string childName)
        {
            var tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < tmps.Length; i++)
            {
                if (tmps[i] != null && tmps[i].name == childName)
                    return tmps[i];
            }
            return null;
        }

        private static int ParseIndexFromName(string name)
        {
            if (string.IsNullOrEmpty(name)) return -1;
            int underscore = name.LastIndexOf('_');
            if (underscore < 0 || underscore >= name.Length - 1) return -1;
            var suffix = name.Substring(underscore + 1);
            return int.TryParse(suffix, out var idx) ? idx : -1;
        }
    }
}

