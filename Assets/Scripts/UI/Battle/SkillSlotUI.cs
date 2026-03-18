using UnityEngine;
using UnityEngine.UI;

namespace Match3Puzzle.UI.Battle
{
    /// <summary>
    /// 스킬 아이콘: 클릭 시 스킬 사용, 사용 시 회색 + 시계 방향 쿨타임 연출.
    /// </summary>
    public class SkillSlotUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay; // 시계 방향 fillAmount (0→1이면 쿨 진행)
        [SerializeField] private Image grayOverlay;    // 사용 중 회색 처리용
        [SerializeField] private float cooldownDuration = 5f; // 레거시(초) 모드용

        private float cooldownRemaining = 0f;
        private bool isOnCooldown => cooldownRemaining > 0f;

        private enum CooldownMode { Turns, Seconds }
        private CooldownMode _cooldownMode = CooldownMode.Turns;
        private int _cooldownTurnsTotal = 0;
        private int _cooldownTurnsRemaining = 0;

        /// <summary>
        /// 이 슬롯에 장착된 스킬 ID (EquippedSkillsHolder에서 전달). 비워두면 빈 슬롯으로 취급.
        /// </summary>
        [SerializeField] private string skillId;
        public string SkillId => skillId;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();

            // Inspector에서 연결 안 됐을 경우 자식에서 탐색
            if (button == null)
                button = GetComponentInChildren<Button>();

            // 아이콘/오버레이가 Raycast를 먹으면 버튼 클릭이 안 들어오는 경우가 있어서 항상 통과시키도록 처리
            if (iconImage != null) iconImage.raycastTarget = false;
            if (cooldownOverlay != null) cooldownOverlay.raycastTarget = false;
            if (grayOverlay != null) grayOverlay.raycastTarget = false;

            if (button != null)
                button.onClick.AddListener(OnClick);
            else
                Debug.LogWarning($"[SkillSlotUI] {gameObject.name}: Button을 찾을 수 없습니다. Inspector에서 Button 필드를 연결하세요.");
        }

        private void Start()
        {
            SetCooldownVisual(0f);
            if (grayOverlay != null)
                grayOverlay.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_cooldownMode != CooldownMode.Seconds) return;
            if (cooldownRemaining <= 0f) return;

            cooldownRemaining -= Time.deltaTime;
            if (cooldownRemaining <= 0f)
            {
                cooldownRemaining = 0f;
                SetCooldownVisual(0f);
                if (grayOverlay != null)
                    grayOverlay.gameObject.SetActive(false);
                if (button != null)
                    button.interactable = true;
            }
            else
            {
                float fill = 1f - (cooldownRemaining / cooldownDuration);
                SetCooldownVisual(fill);
            }
        }

        private void OnClick()
        {
            Debug.Log($"[SkillSlotUI] {gameObject.name} 클릭됨. isOnCooldown={isOnCooldown}");
            if (IsBlockedByCooldown()) return;

            UseSkill();
        }

        private bool IsBlockedByCooldown()
        {
            if (_cooldownMode == CooldownMode.Seconds)
                return isOnCooldown;
            return _cooldownTurnsRemaining > 0;
        }

        /// <summary>
        /// 스킬 사용 처리. 쿨타임이 0이면 쿨 없이 즉시 발동만 하고 버튼은 그대로.
        /// </summary>
        private void UseSkill()
        {
            bool hasCooldown = _cooldownMode == CooldownMode.Seconds
                ? cooldownDuration > 0f
                : _cooldownTurnsTotal > 0;

            if (hasCooldown)
            {
                if (grayOverlay != null)
                    grayOverlay.gameObject.SetActive(true);
                if (button != null)
                    button.interactable = false;
                SetCooldownVisual(0f);

                if (_cooldownMode == CooldownMode.Seconds)
                {
                    cooldownRemaining = cooldownDuration;
                }
                else
                {
                    _cooldownTurnsRemaining = _cooldownTurnsTotal;
                    RefreshTurnCooldownVisual();
                }
            }

            Debug.Log($"[SkillSlotUI] {gameObject.name} OnSkillUsed 발동. 구독자 수={OnSkillUsed?.GetInvocationList()?.Length ?? 0}");
            OnSkillUsed?.Invoke();
        }

        /// <summary>
        /// 시계 방향 쿨 연출. cooldownOverlay는 Image Fill Method: Radial 360, Origin: Top, Clockwise 권장.
        /// </summary>
        private void SetCooldownVisual(float fill)
        {
            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = Mathf.Clamp01(fill);
        }

        public void SetCooldownDuration(float seconds)
        {
            cooldownDuration = Mathf.Max(0.1f, seconds);
            _cooldownMode = CooldownMode.Seconds;
        }

        public void SetCooldownTurns(int turns)
        {
            _cooldownTurnsTotal = Mathf.Max(0, turns);
            _cooldownTurnsRemaining = 0;
            _cooldownMode = CooldownMode.Turns;
            RefreshTurnCooldownVisual();
        }

        /// <summary>
        /// 버튼 활성/비활성 직접 제어. TutorialManager 등 외부에서 사용.
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (button != null)
                button.interactable = interactable;
        }

        /// <summary>
        /// 외부에서 스킬 정보를 적용할 때 사용 (아이콘/턴 쿨타임/ID 설정).
        /// </summary>
        public void Configure(string newSkillId, Sprite icon, int cooldownTurns)
        {
            skillId = newSkillId;
            if (iconImage != null)
                iconImage.sprite = icon;
            SetCooldownTurns(cooldownTurns);
            // 쿨다운이 없는 경우 버튼은 항상 누를 수 있게
            if (button != null)
                button.interactable = true;
            if (grayOverlay != null)
                grayOverlay.gameObject.SetActive(false);
        }

        /// <summary>
        /// 턴이 1 증가(= 1턴 소모)할 때 호출해 턴 쿨다운을 진행.
        /// </summary>
        public void OnTurnAdvanced()
        {
            if (_cooldownMode != CooldownMode.Turns) return;
            if (_cooldownTurnsRemaining <= 0) return;

            _cooldownTurnsRemaining = Mathf.Max(0, _cooldownTurnsRemaining - 1);
            RefreshTurnCooldownVisual();

            if (_cooldownTurnsRemaining <= 0)
            {
                if (grayOverlay != null)
                    grayOverlay.gameObject.SetActive(false);
                if (button != null)
                    button.interactable = true;
                SetCooldownVisual(0f);
            }
        }

        private void RefreshTurnCooldownVisual()
        {
            if (_cooldownTurnsTotal <= 0)
            {
                SetCooldownVisual(0f);
                return;
            }
            float fill = 1f - (_cooldownTurnsRemaining / (float)_cooldownTurnsTotal);
            SetCooldownVisual(fill);
        }

        public event System.Action OnSkillUsed;
    }
}
