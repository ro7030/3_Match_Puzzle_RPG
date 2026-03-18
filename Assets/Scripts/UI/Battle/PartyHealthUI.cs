using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3Puzzle.UI.Battle
{
    /// <summary>
    /// 파티 캐릭터 4명 체력 UI. 각 캐릭터별 피격 판정·체력 바·체력 숫자 표시.
    /// </summary>
    public class PartyHealthUI : MonoBehaviour
    {
        [System.Serializable]
        public class CharacterSlot
        {
            public Image portraitImage;
            public Image healthBarFill; // fillAmount 0~1
            [Tooltip("체력 바 위에 현재/최대 숫자 표시 (예: 80/100). 없으면 표시 안 함")]
            public TextMeshProUGUI healthText;
            public RectTransform hitArea; // 피격 판정용 (선택 시 클릭 영역)
        }

        [Tooltip("체력 숫자 형식. {0}=현재, {1}=최대")]
        [SerializeField] private string healthTextFormat = "{0}/{1}";

        [SerializeField] private CharacterSlot[] slots = new CharacterSlot[4];
        [SerializeField] private int maxHpPerCharacter = 100;

        private int[] currentHp;

        public int CharacterCount => slots?.Length ?? 0;

        private void Awake()
        {
            int count = CharacterCount;
            currentHp = new int[count];
            for (int i = 0; i < count; i++)
            {
                currentHp[i] = maxHpPerCharacter;
            }
        }

        private void Start()
        {
            RefreshAllBars();
        }

        /// <summary>
        /// 해당 캐릭터에 피격 처리 (HP 감소). 체력 바 Image는 fillAmount로 자동 반영됨.
        /// </summary>
        public void TakeDamage(int characterIndex, int amount)
        {
            if (characterIndex < 0 || characterIndex >= CharacterCount) return;

            currentHp[characterIndex] = Mathf.Max(0, currentHp[characterIndex] - amount);
            RefreshBar(characterIndex);
        }

        /// <summary>
        /// 해당 캐릭터 힐 (HP 회복)
        /// </summary>
        public void Heal(int characterIndex, int amount)
        {
            if (characterIndex < 0 || characterIndex >= CharacterCount) return;

            currentHp[characterIndex] = Mathf.Min(maxHpPerCharacter, currentHp[characterIndex] + amount);
            RefreshBar(characterIndex);
        }

        /// <summary>
        /// 특정 캐릭터 HP 직접 설정
        /// </summary>
        public void SetHP(int characterIndex, int current, int max = -1)
        {
            if (characterIndex < 0 || characterIndex >= CharacterCount) return;

            currentHp[characterIndex] = Mathf.Clamp(current, 0, max > 0 ? max : maxHpPerCharacter);
            if (max > 0)
                maxHpPerCharacter = max;
            RefreshBar(characterIndex);
        }

        public int GetCurrentHP(int characterIndex)
        {
            if (characterIndex < 0 || characterIndex >= CharacterCount) return 0;
            return currentHp[characterIndex];
        }

        public bool IsAlive(int characterIndex)
        {
            return GetCurrentHP(characterIndex) > 0;
        }

        private void RefreshBar(int index)
        {
            if (slots == null || index >= slots.Length) return;

            var slot = slots[index];
            int current = currentHp[index];
            int max = maxHpPerCharacter;

            if (slot.healthBarFill != null)
            {
                float fill = max > 0 ? (float)current / max : 0f;
                slot.healthBarFill.fillAmount = Mathf.Clamp01(fill);
            }

            if (slot.healthText != null && !string.IsNullOrEmpty(healthTextFormat))
                slot.healthText.text = string.Format(healthTextFormat, current, max);
        }

        private void RefreshAllBars()
        {
            if (slots == null) return;
            for (int i = 0; i < slots.Length; i++)
                RefreshBar(i);
        }
    }
}
