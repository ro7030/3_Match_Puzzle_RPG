using UnityEngine;
using TMPro;
using Match3Puzzle.Level;

namespace Match3Puzzle.UI.Battle
{
    /// <summary>
    /// 배틀 턴 UI. 퍼즐을 한 번 이동해 맞출 때마다 1씩 증가하는 턴 표시 (예: 1/20).
    /// </summary>
    public class BattleTurnUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private string format = "{0}/{1}";

        private LevelManager levelManager;

        private void Awake()
        {
            levelManager = FindFirstObjectByType<LevelManager>();
        }

        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (turnText == null || levelManager == null) return;

            int current = levelManager.CurrentTurnNumber;
            int max = levelManager.MaxTurns;
            turnText.text = string.Format(format, current, max);
        }
    }
}
