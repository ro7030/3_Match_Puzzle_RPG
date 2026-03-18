using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Match3Puzzle.Map
{
    /// <summary>
    /// 맵씬에서 챕터 하나를 나타내는 UI 노드.
    /// 해금 상태에 따라 컬러/흑백 이미지를 전환하고 버튼 클릭 가능 여부를 제어한다.
    /// 해금된 챕터에 마우스를 올리면 hoverTarget이 위로 살짝 올라가는 호버 효과가 적용된다.
    /// </summary>
    public class ChapterNodeUI : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("버튼 & 클릭 영역")]
        [Tooltip("챕터 클릭 영역 버튼 (이 버튼 하나로 상호작용 처리)")]
        [SerializeField] private Button chapterButton;

        [Header("이미지")]
        [Tooltip("클릭 판정에 사용할 이미지 (이 이미지의 알파값 기준으로 클릭 영역이 결정됨).\n" +
                 "스프라이트 Import Settings에서 Read/Write Enabled를 반드시 체크해야 함.")]
        [SerializeField] private Image hitAreaImage;

        [Tooltip("잠금 상태일 때 표시할 흑백 이미지 (해당 챕터 지역 흑백 스프라이트)")]
        [SerializeField] private Image grayscaleImage;

        [Header("알파 히트 테스트")]
        [Tooltip("이 알파값 미만인 픽셀은 클릭 무시. 0으로 설정 시 전체 Rect 판정(기본값).")]
        [Range(0f, 1f)]
        [SerializeField] private float alphaHitThreshold = 0.1f;

        [Header("호버 효과 (해금된 챕터에만 적용)")]
        [Tooltip("마우스 오버 시 위로 올라갈 오브젝트. 비워두면 이 GameObject 전체가 이동.")]
        [SerializeField] private RectTransform hoverTarget;

        [Tooltip("호버 시 올라갈 Y 픽셀 거리")]
        [SerializeField] private float hoverOffsetY = 12f;

        [Tooltip("올라가고 내려오는 데 걸리는 시간 (초)")]
        [SerializeField] private float hoverDuration = 0.15f;

        [Header("잠금 UI")]
        [Tooltip("자물쇠 아이콘 오브젝트 (선택 사항)")]
        [SerializeField] private GameObject lockIcon;
        [Tooltip("\"LOCKED\" 텍스트 오브젝트 (선택 사항)")]
        [SerializeField] private GameObject lockLabel;

        [Header("챕터 정보 표시 (선택 사항)")]
        [SerializeField] private TMP_Text chapterNameText;
        [SerializeField] private string chapterDisplayName = "Chapter 1";

        private Action _onClickCallback;
        private Vector2 _originPosition;
        private Coroutine _hoverCoroutine;

        /// <summary>현재 챕터 번호 (1~3)</summary>
        public int ChapterNumber { get; private set; }

        /// <summary>해금 여부</summary>
        public bool IsUnlocked { get; private set; }

        private void Awake()
        {
            // hoverTarget이 비어있으면 자신의 RectTransform을 사용
            if (hoverTarget == null)
                hoverTarget = GetComponent<RectTransform>();

            _originPosition = hoverTarget.anchoredPosition;

            ApplyAlphaHitTestThresholdSafely();
        }

        private void ApplyAlphaHitTestThresholdSafely()
        {
            if (hitAreaImage == null) return;

            // 0이면 Rect 전체 판정이므로 굳이 설정하지 않음
            if (alphaHitThreshold <= 0f)
            {
                hitAreaImage.alphaHitTestMinimumThreshold = 0f;
                return;
            }

            // alphaHitTestMinimumThreshold는 스프라이트 텍스처가 Read/Write 가능해야 설정 가능.
            // (Readable이 아니면 Unity 내부에서 InvalidOperationException 발생)
            var sprite = hitAreaImage.sprite;
            var tex = sprite != null ? sprite.texture : null;
            bool readable = tex != null && tex.isReadable;
            if (!readable)
            {
                hitAreaImage.alphaHitTestMinimumThreshold = 0f;
                Debug.LogWarning(
                    $"[ChapterNodeUI] '{gameObject.name}': hitAreaImage 스프라이트 텍스처가 Read/Write Enabled가 아니어서 알파 히트 테스트를 비활성화했습니다. " +
                    $"(Import Settings에서 Read/Write Enabled를 켜거나 alphaHitThreshold를 0으로 설정하세요.)",
                    this);
                return;
            }

            hitAreaImage.alphaHitTestMinimumThreshold = alphaHitThreshold;
        }

        // ──────────────────────────────────────────────────────────────
        // 초기화
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// 외부에서 초기화 시 호출. MapSceneController.Start()에서 사용.
        /// </summary>
        public void Initialize(int chapterNumber, bool isUnlocked, Action onClickCallback)
        {
            ChapterNumber    = chapterNumber;
            IsUnlocked       = isUnlocked;
            _onClickCallback = onClickCallback;

            ApplyVisualState();
            RegisterButtonCallback();
        }

        private void ApplyVisualState()
        {
            if (grayscaleImage != null)
                grayscaleImage.gameObject.SetActive(!IsUnlocked);

            if (lockIcon != null)
                lockIcon.SetActive(!IsUnlocked);

            if (lockLabel != null)
                lockLabel.SetActive(!IsUnlocked);

            if (chapterButton != null)
                chapterButton.interactable = IsUnlocked;

            if (chapterNameText != null)
                chapterNameText.text = chapterDisplayName;
        }

        private void RegisterButtonCallback()
        {
            if (chapterButton == null) return;

            chapterButton.onClick.RemoveAllListeners();
            if (IsUnlocked && _onClickCallback != null)
                chapterButton.onClick.AddListener(() => _onClickCallback());
        }

        // ──────────────────────────────────────────────────────────────
        // 호버 효과
        // ──────────────────────────────────────────────────────────────

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsUnlocked) return;
            StartHoverMove(_originPosition + Vector2.up * hoverOffsetY);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!IsUnlocked) return;
            StartHoverMove(_originPosition);
        }

        private void StartHoverMove(Vector2 targetPos)
        {
            if (_hoverCoroutine != null)
                StopCoroutine(_hoverCoroutine);
            _hoverCoroutine = StartCoroutine(MoveToPosition(targetPos));
        }

        private IEnumerator MoveToPosition(Vector2 targetPos)
        {
            Vector2 startPos = hoverTarget.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < hoverDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / hoverDuration);
                hoverTarget.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            hoverTarget.anchoredPosition = targetPos;
        }

        // ──────────────────────────────────────────────────────────────
        // 런타임 동적 해금
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// 런타임 중 챕터를 동적으로 해금할 때 호출 (예: 스테이지 클리어 직후).
        /// </summary>
        public void Unlock()
        {
            if (IsUnlocked) return;
            IsUnlocked = true;
            ApplyVisualState();
            RegisterButtonCallback();
        }
    }
}
