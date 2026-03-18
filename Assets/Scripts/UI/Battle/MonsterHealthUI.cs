using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3Puzzle.UI.Battle
{
    /// <summary>
    /// 몬스터 체력 바 UI (예: 63% 표시).
    /// </summary>
    public class MonsterHealthUI : MonoBehaviour
    {
        [SerializeField] private Image healthBarFill;
        [SerializeField] private TextMeshProUGUI percentText;
        [Tooltip("체력 바 위에 현재/최대 숫자 표시 (예: 315/500). 없으면 percentText만 사용")]
        [SerializeField] private TextMeshProUGUI healthNumbersText;
        [SerializeField] private bool showPercent = true;
        [Tooltip("healthNumbersText 사용 시 형식. {0}=현재, {1}=최대")]
        [SerializeField] private string healthNumbersFormat = "{0}/{1}";

        private int currentHp = 100;
        private int maxHp = 100;

        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;

        /// <summary>몬스터 HP가 0이 되는 순간 발생. MonsterAttackController 등에서 구독.</summary>
        public event System.Action OnDied;

        private void Start()
        {
            Refresh();
        }

        /// <summary>
        /// 체력 설정 (숫자)
        /// </summary>
        public void SetHP(int current, int max)
        {
            currentHp = Mathf.Clamp(current, 0, max);
            maxHp = Mathf.Max(1, max);
            Refresh();
        }

        /// <summary>
        /// 퍼센트로 설정 (0~1)
        /// </summary>
        public void SetPercent(float percent)
        {
            percent = Mathf.Clamp01(percent);
            if (healthBarFill != null)
                healthBarFill.fillAmount = percent;
            if (percentText != null && showPercent)
                percentText.text = Mathf.RoundToInt(percent * 100f) + "%";
            if (healthNumbersText != null && !string.IsNullOrEmpty(healthNumbersFormat))
                healthNumbersText.text = string.Format(healthNumbersFormat, currentHp, maxHp);
        }

        /// <summary>
        /// 데미지 적용
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (currentHp <= 0) return;
            currentHp = Mathf.Max(0, currentHp - amount);
            Refresh();

            Debug.Log($"[MonsterHP] TakeDamage:{amount} → 남은HP:{currentHp}/{maxHp} / OnDied구독수:{OnDied?.GetInvocationList().Length ?? 0}");

            if (currentHp <= 0)
            {
                Debug.Log("[MonsterHP] HP=0 → OnDied 발동");
                OnDied?.Invoke();
            }
        }

        /// <summary>
        /// 힐 (HP 회복)
        /// </summary>
        public void Heal(int amount)
        {
            currentHp = Mathf.Min(maxHp, currentHp + amount);
            Refresh();
        }

        private void Refresh()
        {
            float percent = maxHp > 0 ? (float)currentHp / maxHp : 0f;
            if (healthBarFill != null)
                healthBarFill.fillAmount = Mathf.Clamp01(percent);
            if (percentText != null && showPercent)
                percentText.text = Mathf.RoundToInt(percent * 100f) + "%";
            if (healthNumbersText != null && !string.IsNullOrEmpty(healthNumbersFormat))
                healthNumbersText.text = string.Format(healthNumbersFormat, currentHp, maxHp);
        }
    }
}
