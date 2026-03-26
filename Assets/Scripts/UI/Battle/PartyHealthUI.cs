using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Match3Puzzle.Inventory;

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
        [SerializeField] private bool applyEquipmentBonusOnAwake = true;

        private int[] currentHp;
        public event System.Action<int> OnCharacterHpZero;
        public event System.Action<int, int, int> OnCharacterHpChanged; // (index, currentHp, maxHp)

        public int CharacterCount => slots?.Length ?? 0;
        public int MaxHpPerCharacter => maxHpPerCharacter;

        private void Awake()
        {
            if (applyEquipmentBonusOnAwake)
            {
                float hpMul = EquipmentStatModifier.GetHpMultiplier();
                maxHpPerCharacter = Mathf.Max(1, Mathf.RoundToInt(maxHpPerCharacter * hpMul));
            }

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
        /// 스테이지 설정: 최대 체력은 유지하고, 1인당 현재 체력만 (최대 − deduction)으로 시작합니다.
        /// </summary>
        /// <param name="deductionFromMax">0이면 무시(풀 체력). 1 이상이면 각자 최대 체력에서 이 만큼 깎인 상태로 시작.</param>
        public void ApplyBattleStartingHpDeduction(int deductionFromMax)
        {
            if (deductionFromMax <= 0) return;

            int count = CharacterCount;
            if (currentHp == null || currentHp.Length != count)
                return;

            int max = maxHpPerCharacter;
            int target = Mathf.Max(0, max - deductionFromMax);

            for (int i = 0; i < count; i++)
            {
                int before = currentHp[i];
                currentHp[i] = target;
                RefreshBar(i);
                OnCharacterHpChanged?.Invoke(i, currentHp[i], maxHpPerCharacter);
                if (before > 0 && currentHp[i] == 0)
                    OnCharacterHpZero?.Invoke(i);
            }
        }

        /// <summary>
        /// 해당 캐릭터에 피격 처리 (HP 감소). 체력 바 Image는 fillAmount로 자동 반영됨.
        /// </summary>
        public void TakeDamage(int characterIndex, int amount)
        {
            if (characterIndex < 0 || characterIndex >= CharacterCount) return;

            int before = currentHp[characterIndex];
            currentHp[characterIndex] = Mathf.Max(0, currentHp[characterIndex] - amount);
            RefreshBar(characterIndex);
            OnCharacterHpChanged?.Invoke(characterIndex, currentHp[characterIndex], maxHpPerCharacter);

            if (before > 0 && currentHp[characterIndex] == 0)
                OnCharacterHpZero?.Invoke(characterIndex);
        }

        /// <summary>
        /// 해당 캐릭터 힐 (HP 회복)
        /// </summary>
        public void Heal(int characterIndex, int amount)
        {
            if (characterIndex < 0 || characterIndex >= CharacterCount) return;

            currentHp[characterIndex] = Mathf.Min(maxHpPerCharacter, currentHp[characterIndex] + amount);
            RefreshBar(characterIndex);
            OnCharacterHpChanged?.Invoke(characterIndex, currentHp[characterIndex], maxHpPerCharacter);
        }

        /// <summary>
        /// 특정 캐릭터 HP 직접 설정
        /// </summary>
        public void SetHP(int characterIndex, int current, int max = -1)
        {
            if (characterIndex < 0 || characterIndex >= CharacterCount) return;

            int before = currentHp[characterIndex];
            currentHp[characterIndex] = Mathf.Clamp(current, 0, max > 0 ? max : maxHpPerCharacter);
            if (max > 0)
                maxHpPerCharacter = max;
            RefreshBar(characterIndex);
            OnCharacterHpChanged?.Invoke(characterIndex, currentHp[characterIndex], maxHpPerCharacter);

            if (before > 0 && currentHp[characterIndex] == 0)
                OnCharacterHpZero?.Invoke(characterIndex);
        }

        public int GetCurrentHP(int characterIndex)
        {
            if (characterIndex < 0 || characterIndex >= CharacterCount) return 0;
            return currentHp[characterIndex];
        }

        /// <summary>
        /// 파티 캐릭터 슬롯의 초상화 Sprite를 반환.
        /// (클리어 패널에서 랜덤 캐릭터를 띄우기 위해 사용)
        /// </summary>
        public Sprite GetPortraitSprite(int characterIndex)
        {
            if (slots == null || characterIndex < 0 || characterIndex >= slots.Length) return null;
            var img = slots[characterIndex].portraitImage;
            return img != null ? img.sprite : null;
        }

        /// <summary>
        /// 파티 캐릭터 슬롯의 초상화 Image를 반환.
        /// </summary>
        public Image GetPortraitImage(int characterIndex)
        {
            if (slots == null || characterIndex < 0 || characterIndex >= slots.Length) return null;
            return slots[characterIndex].portraitImage;
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
