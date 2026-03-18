using System;
using UnityEngine;
using UnityEngine.UI;

namespace Match3Puzzle.Skill
{
    /// <summary>
    /// SkillScene 상단 캐릭터 1개 슬롯.
    /// 초상화 + 장착 스킬 아이콘. 클릭 시 스킬 선택 패널을 여는 콜백 호출.
    /// </summary>
    public class CharacterSkillSlotUI : MonoBehaviour
    {
        [Header("캐릭터")]
        [Tooltip("캐릭터 초상화 Image")]
        [SerializeField] private Image portraitImage;

        [Header("스킬 아이콘")]
        [Tooltip("클릭 시 스킬 선택 패널 열기. Button 또는 Button이 있는 자식 오브젝트")]
        [SerializeField] private Button skillIconButton;

        [Tooltip("스킬 아이콘 Image (Button 자식 권장)")]
        [SerializeField] private Image skillIconImage;

        private Action _clickCallback;

        private void Awake()
        {
            if (skillIconButton == null)
                skillIconButton = GetComponentInChildren<Button>();

            if (skillIconButton != null)
                skillIconButton.onClick.AddListener(OnSkillIconClicked);
        }

        private void OnSkillIconClicked()
        {
            _clickCallback?.Invoke();
        }

        /// <summary>
        /// 스킬 아이콘 클릭 시 실행할 콜백 등록.
        /// </summary>
        public void SetClickCallback(Action callback)
        {
            _clickCallback = callback;
        }

        /// <summary>
        /// 슬롯 UI를 최신 데이터로 갱신.
        /// </summary>
        public void Refresh(Story.DataCharacter character, SkillData equippedSkill)
        {
            if (character != null)
            {
                if (portraitImage != null && character.chrImage != null)
                    portraitImage.sprite = character.chrImage;
            }

            if (skillIconImage != null)
            {
                if (equippedSkill != null && equippedSkill.icon != null)
                {
                    skillIconImage.sprite = equippedSkill.icon;
                    skillIconImage.enabled = true;
                }
                else
                {
                    skillIconImage.enabled = false;
                }
            }

            if (skillIconButton != null)
                skillIconButton.interactable = true;
        }
    }
}
