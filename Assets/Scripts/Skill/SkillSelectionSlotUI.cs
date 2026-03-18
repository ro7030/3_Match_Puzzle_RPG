using System;
using UnityEngine;
using UnityEngine.UI;

namespace Match3Puzzle.Skill
{
    /// <summary>
    /// SkillScene 스킬 선택 패널 내 개별 스킬 슬롯.
    /// 클릭 시 해당 스킬을 현재 선택 캐릭터에 장착. 스킬 이름·효과는 Controller에서 표시.
    /// </summary>
    public class SkillSelectionSlotUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;

        private SkillData _skillData;
        private Action _clickCallback;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
            if (button == null)
                button = GetComponentInChildren<Button>();

            if (button != null)
                button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            _clickCallback?.Invoke();
        }

        /// <summary>
        /// 클릭 시 실행할 콜백 등록.
        /// </summary>
        public void SetClickCallback(Action callback)
        {
            _clickCallback = callback;
        }

        /// <summary>
        /// 슬롯에 표시할 스킬 데이터 설정.
        /// </summary>
        public void SetSkill(SkillData data)
        {
            _skillData = data;
            if (iconImage != null)
            {
                if (data != null && data.icon != null)
                {
                    iconImage.sprite = data.icon;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }
        }

        /// <summary>
        /// 현재 슬롯에 설정된 스킬 데이터 반환.
        /// </summary>
        public SkillData GetSkillData() => _skillData;
    }
}
