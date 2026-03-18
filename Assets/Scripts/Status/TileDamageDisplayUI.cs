using UnityEngine;
using TMPro;
using Match3Puzzle.Stats;

namespace Match3Puzzle.Status
{
    /// <summary>
    /// 타일별 현재 데미지/회복량을 TextMeshPro로 표시하는 컴포넌트.
    /// StatusScene에서 캐릭터 이름 옆 등 원하는 위치에 배치.
    ///
    /// 표시 예시 (displayMode = All):
    ///   검  30  활  80  마법  35  회복  25
    ///
    /// 표시 예시 (displayMode = SingleStat, statIndex = 0):
    ///   검  30
    /// </summary>
    public class TileDamageDisplayUI : MonoBehaviour
    {
        public enum DisplayMode
        {
            /// <summary>검/활/마법/회복 4종 모두 표시</summary>
            All,
            /// <summary>지정한 스탯 1개만 표시</summary>
            SingleStat
        }

        [Header("표시 모드")]
        [SerializeField] private DisplayMode displayMode = DisplayMode.All;

        [Tooltip("SingleStat 모드일 때 표시할 스탯 (0=검 / 1=활 / 2=마법 / 3=회복)")]
        [SerializeField] [Range(0, 3)] private int statIndex = 0;

        [Header("텍스트 참조")]
        [Tooltip("데미지를 출력할 TextMeshProUGUI")]
        [SerializeField] private TextMeshProUGUI damageText;

        [Header("스탯 데이터 (비우면 Resources/CharacterStats 자동 로드)")]
        [SerializeField] private CharacterStatsData statsData;

        [Header("표시 형식")]
        [Tooltip("All 모드의 구분자 (예: \"  |  \")")]
        [SerializeField] private string separator = "   ";

        [Tooltip("스탯 이름 표시 여부")]
        [SerializeField] private bool showLabel = true;

        // ── Unity 생명 주기 ───────────────────────────────────────────────

        private void Start()
        {
            if (statsData == null)
                statsData = CharacterStatsResolver.LoadFromResources();

            Refresh();
        }

        // ── 퍼블릭 API ────────────────────────────────────────────────────

        /// <summary>텍스트를 현재 스탯 수치로 즉시 갱신합니다.</summary>
        public void Refresh()
        {
            if (damageText == null || statsData == null) return;

            damageText.text = displayMode == DisplayMode.All
                ? BuildAllText()
                : BuildSingleText(statIndex);
        }

        // ── 내부 빌더 ────────────────────────────────────────────────────

        private string BuildAllText()
        {
            string sword  = FormatStat("검",  CharacterStatsResolver.GetSwordDamage(statsData));
            string bow    = FormatStat("활",  CharacterStatsResolver.GetBowDamage(statsData));
            string wand   = FormatStat("마법", CharacterStatsResolver.GetWandDamage(statsData));
            string heal   = FormatStat("회복", CharacterStatsResolver.GetHealAmount(statsData));

            return $"{sword}{separator}{bow}{separator}{wand}{separator}{heal}";
        }

        private string BuildSingleText(int idx)
        {
            return idx switch
            {
                0 => FormatStat("검",  CharacterStatsResolver.GetSwordDamage(statsData)),
                1 => FormatStat("활",  CharacterStatsResolver.GetBowDamage(statsData)),
                2 => FormatStat("마법", CharacterStatsResolver.GetWandDamage(statsData)),
                3 => FormatStat("회복", CharacterStatsResolver.GetHealAmount(statsData)),
                _ => ""
            };
        }

        private string FormatStat(string label, int value)
            => showLabel ? $"{label}  {value}" : $"{value}";
    }
}
