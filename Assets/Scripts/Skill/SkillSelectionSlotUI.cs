using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Match3Puzzle.Skill
{
    /// <summary>
    /// SkillScene 하단 스킬 선택 슬롯. 호버 시 이름·설명, 클릭 시 장착.
    /// </summary>
    public class SkillSelectionSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;

        [Header("알파 히트 테스트")]
        [Tooltip("Sprite의 알파값 기준으로 클릭 인식. 0이면 Rect 전체 판정(기본값).")]
        [SerializeField] private float alphaHitThreshold = 0.1f;

        private SkillData _skillData;
        private bool _locked;
        private Action _clickCallback;
        private Action<SkillData> _hoverEnter;
        private Action _hoverExit;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
            if (button == null)
                button = GetComponentInChildren<Button>();

            float threshold = alphaHitThreshold <= 0f ? 0f : alphaHitThreshold;
            if (iconImage != null)
                iconImage.alphaHitTestMinimumThreshold = threshold;

            // Button이 사용하는 타겟 그래픽(Image)에도 동일하게 적용합니다. (Graphic에는 alphaHitTestMinimumThreshold 없음)
            if (button != null && button.targetGraphic is Image targetImage)
                targetImage.alphaHitTestMinimumThreshold = threshold;

            if (button != null)
                button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            _clickCallback?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_skillData == null) return;
            _hoverEnter?.Invoke(_skillData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hoverExit?.Invoke();
        }

        /// <summary>
        /// 클릭 시 실행할 콜백 등록.
        /// </summary>
        public void SetClickCallback(Action callback)
        {
            _clickCallback = callback;
        }

        /// <summary>
        /// 호버 시 스킬 정보 표시 / 이탈 시 숨김.
        /// </summary>
        public void SetHoverCallbacks(Action<SkillData> onEnter, Action onExit)
        {
            _hoverEnter = onEnter;
            _hoverExit = onExit;
        }

        /// <summary>
        /// 슬롯에 표시할 스킬 데이터 설정.
        /// </summary>
        public void SetSkill(SkillData data)
        {
            SetSkill(data, locked: false);
        }

        /// <summary>
        /// 슬롯에 표시할 스킬 데이터 설정 (locked면 회색 아이콘 + 클릭 불가).
        /// </summary>
        public void SetSkill(SkillData data, bool locked)
        {
            _skillData = data;
            _locked = locked;

            if (iconImage != null)
            {
                if (data != null)
                {
                    Sprite spriteToUse = null;
                    if (locked && data.iconGrayscale != null)
                        spriteToUse = data.iconGrayscale;
                    else
                        spriteToUse = data.icon;

                    if (spriteToUse != null)
                    {
                        iconImage.sprite = spriteToUse;
                        iconImage.enabled = true;
                        // 회색 스프라이트를 쓰는 경우에도 색은 원본 유지.
                        iconImage.color = Color.white;
                    }
                    else
                    {
                        // 아이콘이 없으면 숨김.
                        iconImage.enabled = false;
                    }
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            if (button != null)
                button.interactable = data != null && !locked;
        }

        /// <summary>
        /// 현재 슬롯에 설정된 스킬 데이터 반환.
        /// </summary>
        public SkillData GetSkillData() => _skillData;
    }
}
