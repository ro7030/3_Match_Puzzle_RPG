using MainMenu;
using TMPro;
using UnityEngine;

namespace Match3Puzzle.UI
{
    /// <summary>
    /// 저장 데이터의 현재 골드를 TMP 텍스트에 표시한다.
    /// 하이어라키 연결 후 OnEnable/주기 갱신으로 자동 반영된다.
    /// </summary>
    public class GoldTextUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private string prefix = "골드 ";
        [SerializeField] private bool refreshEveryFrame = true;

        private int _lastGold = int.MinValue;

        private void Awake()
        {
            if (goldText == null)
                goldText = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Update()
        {
            if (!refreshEveryFrame) return;
            Refresh();
        }

        /// <summary>
        /// 필요 시 버튼 이벤트에서도 수동 호출 가능.
        /// </summary>
        public void Refresh()
        {
            var save = SaveSystem.Load();
            int gold = save != null ? save.gold : 0;

            if (gold == _lastGold && goldText != null) return;
            _lastGold = gold;

            if (goldText != null)
                goldText.text = $"{prefix}{gold}";
        }
    }
}
