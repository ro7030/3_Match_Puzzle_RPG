using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3Puzzle.Status
{
    /// <summary>
    /// StatusScene 캐릭터 1개 슬롯 UI.
    /// StatusSceneController가 Refresh()로 데이터를 주입하고
    /// SetUpgradeCallback()으로 Lv UP 버튼 콜백을 등록한다.
    /// </summary>
    public class CharacterStatusSlotUI : MonoBehaviour
    {
        [Header("초상화 / 이름")]
        [Tooltip("캐릭터 초상화 Image")]
        [SerializeField] private Image portraitImage;

        [Tooltip("캐릭터 이름 텍스트")]
        [SerializeField] private TextMeshProUGUI nameText;

        [Header("스탯 정보")]
        [Tooltip("스탯 종류 라벨 (검 데미지 / 활 데미지 / 마법 데미지 / 회복량)")]
        [SerializeField] private TextMeshProUGUI statLabelText;

        [Tooltip("현재 업그레이드 레벨 (예: Lv.3)")]
        [SerializeField] private TextMeshProUGUI levelText;

        [Tooltip("현재 수치 (예: 60)")]
        [SerializeField] private TextMeshProUGUI currentValueText;

        [Tooltip("업그레이드 후 수치 미리보기 (예: → 72). 선택 사항")]
        [SerializeField] private TextMeshProUGUI nextValueText;

        [Header("Lv UP 버튼")]
        [SerializeField] private Button levelUpButton;

        // ── 퍼블릭 API ────────────────────────────────────────────────────

        /// <summary>
        /// 슬롯 UI를 최신 데이터로 갱신한다.
        /// </summary>
        /// <param name="character">캐릭터 데이터 (null이면 초상화/이름 유지)</param>
        /// <param name="statLabel">스탯 종류 설명 문자열</param>
        /// <param name="level">현재 업그레이드 레벨</param>
        /// <param name="currentValue">현재 수치 (데미지 또는 회복량)</param>
        /// <param name="nextValue">업그레이드 후 예상 수치</param>
        /// <param name="canUpgrade">업그레이드 포인트 보유 여부 (false면 버튼 비활성화)</param>
        public void Refresh(Story.DataCharacter character, string statLabel,
                            int level, int currentValue, int nextValue, bool canUpgrade)
        {
            if (character != null)
            {
                if (portraitImage != null && character.chrImage != null)
                    portraitImage.sprite = character.chrImage;

                if (nameText != null)
                    nameText.text = character.displayName;
            }

            if (statLabelText    != null) statLabelText.text    = statLabel;
            if (levelText        != null) levelText.text        = $"Lv.{level}";
            if (currentValueText != null) currentValueText.text = $"{currentValue}";
            if (nextValueText    != null) nextValueText.text    = $"→ {nextValue}";

            if (levelUpButton != null)
                levelUpButton.interactable = canUpgrade;
        }

        /// <summary>
        /// Lv UP 버튼 클릭 시 실행할 콜백을 등록한다.
        /// 기존 리스너는 모두 제거된 뒤 등록된다.
        /// </summary>
        public void SetUpgradeCallback(Action onLevelUp)
        {
            if (levelUpButton == null) return;
            levelUpButton.onClick.RemoveAllListeners();
            levelUpButton.onClick.AddListener(() => onLevelUp?.Invoke());
        }
    }
}
