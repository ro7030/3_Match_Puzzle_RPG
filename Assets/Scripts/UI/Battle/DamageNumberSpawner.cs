using UnityEngine;

namespace Match3Puzzle.UI.Battle
{
    /// <summary>
    /// 데미지/힐 숫자 이미지를 화면에 스폰하는 관리자.
    /// MonsterHealthUI.OnDamageReceived 와 PartyHealthUI.OnDamageReceived 이벤트를 구독해
    /// 데미지가 발생할 때 자동으로 숫자를 표시합니다.
    ///
    /// [설정 방법]
    /// 1. Battle 씬의 UI Canvas 아래에 빈 GameObject를 만들고 이 컴포넌트를 붙입니다.
    /// 2. Inspector에서 0~9 Digit Sprites를 순서대로 등록합니다.
    /// 3. Minus Sprite에 '-' 이미지를 등록합니다.
    /// 4. Monster Anchor: 몬스터 체력 바(또는 몬스터 이미지)의 RectTransform을 연결합니다.
    /// 5. Party Slot Anchors(크기 4): 파티원 초상화 각각의 RectTransform을 연결합니다.
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        public static DamageNumberSpawner Instance { get; private set; }

        [Header("데미지 숫자 스프라이트")]
        [Tooltip("데미지용 0~9 순서로 10개 등록")]
        [SerializeField] private Sprite[] digitSprites = new Sprite[10];
        [Tooltip("마이너스(-) 기호 스프라이트 (데미지 앞에 표시)")]
        [SerializeField] private Sprite minusSprite;

        [Header("회복 숫자 스프라이트")]
        [Tooltip("회복용 0~9 순서로 10개 등록. 비워두면 데미지 숫자 이미지를 그대로 사용")]
        [SerializeField] private Sprite[] healDigitSprites = new Sprite[10];
        [Tooltip("플러스(+) 기호 스프라이트 (회복 앞에 표시). 없으면 기호 생략")]
        [SerializeField] private Sprite plusSprite;

        [Header("스폰 기준 위치")]
        [Tooltip("몬스터 데미지 숫자가 나타날 기준 RectTransform (예: 몬스터 초상화 또는 체력 바)")]
        [SerializeField] private RectTransform monsterAnchor;
        [Tooltip("파티원 4명의 데미지 숫자가 나타날 기준 RectTransform 배열 (인덱스 0~3)")]
        [SerializeField] private RectTransform[] partySlotAnchors = new RectTransform[4];

        [Header("숫자 표시 설정")]
        [Tooltip("각 숫자 이미지의 너비 (픽셀)")]
        [SerializeField] private float digitWidth = 30f;
        [Tooltip("각 숫자 이미지의 높이 (픽셀)")]
        [SerializeField] private float digitHeight = 38f;
        [Tooltip("위로 떠오를 최대 거리 (픽셀)")]
        [SerializeField] private float floatDistance = 90f;
        [Tooltip("숫자가 표시되고 사라지는 데 걸리는 시간 (초)")]
        [SerializeField] private float displayDuration = 1.1f;
        [Tooltip("스폰 위치에 적용할 수평 랜덤 오프셋 (픽셀). 여러 숫자가 겹치지 않도록 합니다")]
        [SerializeField] private float randomOffsetX = 25f;
        [Tooltip("스폰 위치의 수직 오프셋 (픽셀). 양수면 위로 이동")]
        [SerializeField] private float spawnOffsetY = 20f;

        private Canvas rootCanvas;
        private RectTransform spawnParent;

        private MonsterHealthUI monsterHealthUI;
        private PartyHealthUI partyHealthUI;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            spawnParent = GetComponent<RectTransform>();

            // 루트 Canvas 탐색 (좌표 변환에 사용)
            rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas != null)
            {
                Canvas parent = rootCanvas.transform.parent?.GetComponentInParent<Canvas>();
                while (parent != null)
                {
                    rootCanvas = parent;
                    parent = rootCanvas.transform.parent?.GetComponentInParent<Canvas>();
                }
            }
        }

        private void Start()
        {
            monsterHealthUI = FindFirstObjectByType<MonsterHealthUI>();
            partyHealthUI   = FindFirstObjectByType<PartyHealthUI>();

            if (monsterHealthUI != null)
            {
                monsterHealthUI.OnDamageReceived += HandleMonsterDamage;
                monsterHealthUI.OnHealReceived   += HandleMonsterHeal;
            }
            if (partyHealthUI != null)
            {
                partyHealthUI.OnDamageReceived += HandlePartyDamage;
                partyHealthUI.OnHealReceived   += HandlePartyHeal;
            }
        }

        private void OnDestroy()
        {
            if (monsterHealthUI != null)
            {
                monsterHealthUI.OnDamageReceived -= HandleMonsterDamage;
                monsterHealthUI.OnHealReceived   -= HandleMonsterHeal;
            }
            if (partyHealthUI != null)
            {
                partyHealthUI.OnDamageReceived -= HandlePartyDamage;
                partyHealthUI.OnHealReceived   -= HandlePartyHeal;
            }
        }

        // ── 이벤트 핸들러 ──────────────────────────────────────────────────

        private void HandleMonsterDamage(int amount)
        {
            if (amount <= 0) return;
            Vector2 pos = GetLocalPosition(monsterAnchor);
            SpawnNumber(-amount, pos, isHeal: false);
        }

        private void HandlePartyDamage(int characterIndex, int amount)
        {
            if (amount <= 0) return;
            RectTransform anchor = (partySlotAnchors != null && characterIndex < partySlotAnchors.Length)
                ? partySlotAnchors[characterIndex]
                : null;
            Vector2 pos = GetLocalPosition(anchor);
            SpawnNumber(-amount, pos, isHeal: false);
        }

        private void HandleMonsterHeal(int amount)
        {
            if (amount <= 0) return;
            Vector2 pos = GetLocalPosition(monsterAnchor);
            SpawnNumber(amount, pos, isHeal: true);
        }

        private void HandlePartyHeal(int characterIndex, int amount)
        {
            if (amount <= 0) return;
            RectTransform anchor = (partySlotAnchors != null && characterIndex < partySlotAnchors.Length)
                ? partySlotAnchors[characterIndex]
                : null;
            Vector2 pos = GetLocalPosition(anchor);
            SpawnNumber(amount, pos, isHeal: true);
        }

        // ── 내부 메서드 ────────────────────────────────────────────────────

        /// <summary>
        /// anchor RectTransform의 중심 좌표를 spawnParent 로컬 좌표로 변환합니다.
        /// </summary>
        private Vector2 GetLocalPosition(RectTransform anchor)
        {
            if (anchor == null || spawnParent == null)
                return Vector2.zero;

            Vector3[] corners = new Vector3[4];
            anchor.GetWorldCorners(corners);
            Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;

            Camera uiCamera = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? rootCanvas.worldCamera
                : null;

            // Screen Space - Overlay에서는 world position이 곧 스크린 좌표
            Vector2 screenPoint = (uiCamera != null)
                ? RectTransformUtility.WorldToScreenPoint(uiCamera, worldCenter)
                : new Vector2(worldCenter.x, worldCenter.y);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                spawnParent, screenPoint, uiCamera, out Vector2 localPos);

            return localPos;
        }

        /// <summary>
        /// 지정 위치에 DamageNumberDisplay를 생성합니다.
        /// </summary>
        private void SpawnNumber(int value, Vector2 localPosition, bool isHeal)
        {
            if (digitSprites == null || digitSprites.Length < 10 || spawnParent == null)
                return;

            float offsetX = Random.Range(-randomOffsetX, randomOffsetX);

            var go = new GameObject(isHeal ? "HealNumber" : "DamageNumber", typeof(RectTransform));
            go.transform.SetParent(spawnParent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta        = Vector2.zero;
            rt.anchoredPosition = localPosition + new Vector2(offsetX, spawnOffsetY);

            // 회복이면 healDigitSprites 사용, 비어있으면 데미지용 digitSprites로 폴백
            bool useHealSprites = isHeal && healDigitSprites != null && healDigitSprites.Length >= 10 && healDigitSprites[0] != null;
            Sprite[] selectedDigits = useHealSprites ? healDigitSprites : digitSprites;

            // 데미지는 minusSprite, 회복은 plusSprite를 앞에 붙임
            Sprite prefixSprite = isHeal ? plusSprite : minusSprite;

            var display = go.AddComponent<DamageNumberDisplay>();
            display.Setup(value, selectedDigits, prefixSprite, isHeal,
                digitWidth, digitHeight, floatDistance, displayDuration);
        }

        // ── 외부에서 직접 호출 가능한 public API ──────────────────────────

        /// <summary>
        /// 외부에서 직접 몬스터 데미지 숫자를 표시합니다 (이벤트 없이 수동 호출 시).
        /// </summary>
        public void ShowMonsterDamage(int amount)
        {
            if (amount <= 0) return;
            Vector2 pos = GetLocalPosition(monsterAnchor);
            SpawnNumber(-amount, pos, isHeal: false);
        }

        /// <summary>
        /// 외부에서 직접 파티원 데미지 숫자를 표시합니다 (이벤트 없이 수동 호출 시).
        /// </summary>
        public void ShowPartyDamage(int characterIndex, int amount)
        {
            if (amount <= 0) return;
            RectTransform anchor = (partySlotAnchors != null && characterIndex < partySlotAnchors.Length)
                ? partySlotAnchors[characterIndex]
                : null;
            Vector2 pos = GetLocalPosition(anchor);
            SpawnNumber(-amount, pos, isHeal: false);
        }
    }
}
