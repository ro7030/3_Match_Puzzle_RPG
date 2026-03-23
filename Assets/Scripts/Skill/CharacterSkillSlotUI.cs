using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Match3Puzzle.Skill
{
    /// <summary>
    /// SkillScene 캐릭터 1슬롯. 미선택 시 흑백, 선택 시 컬러+글로우+장착 스킬 표시.
    /// 초상화에 마우스를 올리면 MapScene 챕터 노드와 같은 방식으로 위로 살짝 이동합니다.
    /// </summary>
    public class CharacterSkillSlotUI : MonoBehaviour
    {
        [Header("캐릭터 초상화")]
        [Tooltip("컬러 초상화 Image (raycast Target 켜두세요)")]
        [SerializeField] private Image portraitImage;

        [Header("알파 히트 테스트")]
        [Tooltip("Sprite의 알파값 기준으로 클릭 인식. 0이면 Rect 전체 판정(기본값).")]
        [SerializeField] private float alphaHitThreshold = 0.1f;

        [Header("호버 (MapScene ChapterNodeUI와 동일)")]
        [Tooltip("비우면 이 슬롯의 RectTransform이 이동합니다.")]
        [SerializeField] private RectTransform hoverTarget;

        [SerializeField] private float hoverOffsetY = 12f;
        [SerializeField] private float hoverDuration = 0.15f;

        [Header("선택 효과")]
        [Tooltip("캐릭터 클릭 시 켜지는 빛나는 배경 (씬에 4개 배치 후 비활성화)")]
        [SerializeField] private GameObject glowBackground;

        [Header("스킬 아이콘 (선택된 캐릭터만 표시)")]
        [SerializeField] private GameObject skillEquippedRoot;

        [Tooltip("레거시: 장착 스킬 표시용. Button은 비활성화하고 Image만 사용합니다.")]
        [SerializeField] private Button skillIconButton;

        [SerializeField] private Image skillIconImage;

        private Action _clickCallback;
        private Vector2 _originHoverPosition;
        private Coroutine _hoverCoroutine;
        private bool _selected;
        private bool _pointerOver;

        private RectTransform HoverTargetRect => hoverTarget != null ? hoverTarget : transform as RectTransform;

        private void Awake()
        {
            if (hoverTarget == null)
                hoverTarget = transform as RectTransform;

            _originHoverPosition = HoverTargetRect != null ? HoverTargetRect.anchoredPosition : Vector2.zero;

            if (portraitImage != null)
            {
                portraitImage.raycastTarget = true;
                portraitImage.alphaHitTestMinimumThreshold = alphaHitThreshold <= 0f ? 0f : alphaHitThreshold;
                var proxy = portraitImage.GetComponent<SkillSlotPointerProxy>();
                if (proxy == null)
                    proxy = portraitImage.gameObject.AddComponent<SkillSlotPointerProxy>();
                proxy.Init(this);
            }

            if (skillIconButton != null)
            {
                skillIconButton.onClick.RemoveAllListeners();
                skillIconButton.interactable = false;
                var btnImg = skillIconButton.GetComponent<Image>();
                if (btnImg != null)
                    btnImg.raycastTarget = false;
            }

            if (skillIconImage != null)
                skillIconImage.raycastTarget = false;

            if (skillEquippedRoot == null && skillIconButton != null)
                skillEquippedRoot = skillIconButton.gameObject;

            if (glowBackground != null)
                glowBackground.SetActive(false);

            if (skillEquippedRoot != null)
                skillEquippedRoot.SetActive(false);
        }

        internal void NotifyPointerEnter()
        {
            // 회색(미선택) 상태에서만 호버 이동
            if (_selected) return;
            _pointerOver = true;
            StartHoverMove(_originHoverPosition + Vector2.up * hoverOffsetY);
        }

        internal void NotifyPointerExit()
        {
            // 선택 상태에서는 호버를 사용하지 않음
            if (_selected) return;
            _pointerOver = false;
            StartHoverMove(_originHoverPosition);
        }

        internal void NotifyPointerClick()
        {
            _clickCallback?.Invoke();
        }

        private void StartHoverMove(Vector2 targetPos)
        {
            if (HoverTargetRect == null) return;
            if (_hoverCoroutine != null)
                StopCoroutine(_hoverCoroutine);
            _hoverCoroutine = StartCoroutine(MoveToPosition(targetPos));
        }

        private IEnumerator MoveToPosition(Vector2 targetPos)
        {
            var rt = HoverTargetRect;
            Vector2 startPos = rt.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < hoverDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / hoverDuration);
                rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            rt.anchoredPosition = targetPos;
        }

        /// <summary>
        /// 스킬 아이콘 클릭 시 실행할 콜백 (캐릭터 영역 클릭 = 캐릭터 선택).
        /// </summary>
        public void SetClickCallback(Action callback)
        {
            _clickCallback = callback;
        }

        /// <summary>
        /// 선택 여부 및 데이터로 UI 갱신.
        /// </summary>
        public void Refresh(Story.DataCharacter character, SkillData equippedSkill, bool selected)
        {
            _selected = selected;

            // 선택(컬러)으로 바뀌는 순간 호버 상태를 강제 해제
            if (_selected)
                _pointerOver = false;

            if (glowBackground != null)
                glowBackground.SetActive(selected);

            if (skillEquippedRoot != null)
                skillEquippedRoot.SetActive(selected);

            if (portraitImage != null && character != null)
            {
                if (selected)
                {
                    if (character.chrImage != null)
                        portraitImage.sprite = character.chrImage;
                }
                else
                {
                    var gray = character.chrImageGrayscale;
                    portraitImage.sprite = gray != null ? gray : character.chrImage;
                }
            }

            if (skillIconImage != null)
            {
                if (selected && equippedSkill != null && equippedSkill.icon != null)
                {
                    skillIconImage.sprite = equippedSkill.icon;
                    skillIconImage.enabled = true;
                }
                else
                {
                    skillIconImage.enabled = false;
                }
            }

            if (!_pointerOver && HoverTargetRect != null)
                HoverTargetRect.anchoredPosition = _originHoverPosition;
        }

        /// <summary>
        /// 호버 기준 위치를 현재 앵커 위치로 다시 잡습니다 (레이아웃 변경 후 호출).
        /// </summary>
        public void RebindHoverOrigin()
        {
            if (HoverTargetRect != null)
                _originHoverPosition = HoverTargetRect.anchoredPosition;
        }
    }
}
