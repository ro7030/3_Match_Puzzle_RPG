using System;
using UnityEngine;
using UnityEngine.UI;

namespace Match3Puzzle.Inventory
{
    /// <summary>
    /// 좌측 "장비 칸" UI 1개.
    /// 클릭 시 해당 슬롯을 선택하고, 장착된 장비 아이콘을 표시한다.
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour
    {
        [SerializeField] private int characterIndex;
        [SerializeField] private Button slotButton;
        [SerializeField] private Image slotFrameImage;
        [SerializeField] private Image equippedItemIconImage;

        private Action<int> _clickCallback;

        public int CharacterIndex => characterIndex;

        public void SetCharacterIndex(int value) => characterIndex = value;

        public void SetClickCallback(Action<int> callback) => _clickCallback = callback;

        private void Awake()
        {
            if (slotButton == null) slotButton = GetComponent<Button>();
            if (slotFrameImage == null) slotFrameImage = FindChildImage("SlotFrameImage");
            if (equippedItemIconImage == null)
                equippedItemIconImage = FindChildImage("EquippedItemIconImage") ?? FindChildImage("EquippedSkillIconImage");

            if (slotButton != null)
                slotButton.onClick.AddListener(() => _clickCallback?.Invoke(characterIndex));
        }

        public void Refresh(EquipmentData equippedItem, bool isSelected)
        {
            if (slotFrameImage != null)
                slotFrameImage.color = isSelected ? new Color(1f, 0.95f, 0.55f, 0.95f) : new Color(1f, 1f, 1f, 0.5f);

            if (equippedItemIconImage == null) return;

            if (equippedItem != null && equippedItem.icon != null)
            {
                equippedItemIconImage.sprite = equippedItem.icon;
                equippedItemIconImage.enabled = true;
            }
            else
            {
                equippedItemIconImage.enabled = false;
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
    }
}

