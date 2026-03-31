using System.Text;
using UnityEngine;

namespace Match3Puzzle.Inventory
{
    /// <summary>
    /// 장비 카드/정보 패널에 표시할 효과 문구 생성.
    /// </summary>
    public static class EquipmentDisplayText
    {
        private const float Epsilon = 0.001f;

        /// <summary>
        /// 효과 한 줄(또는 여러 줄). effectLineOverride가 있으면 그대로 사용.
        /// </summary>
        public static string GetEffectText(EquipmentData data)
        {
            if (data == null) return "";

            if (!string.IsNullOrWhiteSpace(data.effectLineOverride))
                return data.effectLineOverride.Trim();

            var sb = new StringBuilder();
            if (data.partyMaxHpPercent > Epsilon)
                sb.AppendLine($"파티 최대 체력 {FormatPercent(data.partyMaxHpPercent)}% 상승");
            if (data.attackPercent > Epsilon)
                sb.AppendLine($"공격력 {FormatPercent(data.attackPercent)}% 상승");
            if (data.healPercent > Epsilon)
                sb.AppendLine($"회복량 {FormatPercent(data.healPercent)}% 상승");

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// 카드용: 효과(위) + 설명(아래). 효과가 없으면 설명만.
        /// </summary>
        public static string FormatCardDescriptionBody(EquipmentData data)
        {
            if (data == null) return "";
            string effect = GetEffectText(data);
            string desc = data.description ?? "";
            if (string.IsNullOrEmpty(effect))
                return desc;
            // 레퍼런스: 효과 30 / 설명 26
            return $"<size=30>{effect}</size>\n<size=26>{desc}</size>";
        }

        /// <summary>
        /// 정보 패널 본문: 효과 + 빈 줄 + 설명.
        /// </summary>
        public static string FormatInfoPanelBody(EquipmentData data)
        {
            if (data == null) return "";
            string effect = GetEffectText(data);
            string desc = data.description ?? "";
            if (string.IsNullOrEmpty(effect))
                return desc;
            return effect + "\n\n" + desc;
        }

        private static string FormatPercent(float value)
        {
            if (Mathf.Approximately(value, Mathf.Round(value)))
                return Mathf.RoundToInt(value).ToString();
            return value.ToString("0.##");
        }
    }
}
