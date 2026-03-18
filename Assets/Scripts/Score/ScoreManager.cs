using UnityEngine;
using Match3Puzzle.Matching;

namespace Match3Puzzle.Score
{
    /// <summary>
    /// 점수를 관리하는 클래스
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [Header("Score Settings")]
        [SerializeField] private int baseMatchScore = 100;
        [SerializeField] private int comboMultiplier = 50;
        [SerializeField] private int specialTileBonus = 200;
        [SerializeField] private int enhancedTileBonus = 80; // 강화 타일(4개 이상 매칭) 터뜨릴 때 추가

        private int currentScore = 0;
        private int comboCount = 0;
        private int bestScore = 0;

        public int CurrentScore => currentScore;
        public int ComboCount => comboCount;
        public int BestScore => bestScore;

        private void Awake()
        {
            LoadBestScore();
        }

        /// <summary>
        /// 점수 초기화
        /// </summary>
        public void ResetScore()
        {
            currentScore = 0;
            comboCount = 0;
            OnScoreChanged?.Invoke(currentScore);
        }

        /// <summary>
        /// 매칭으로 점수 추가
        /// </summary>
        public void AddMatchScore(MatchGroup match)
        {
            if (match == null) return;

            int matchScore = baseMatchScore * match.Count;
            
            // 콤보 보너스
            if (comboCount > 0)
            {
                matchScore += comboMultiplier * comboCount;
            }

            // 특수 타일 / 강화 타일 보너스
            foreach (var tile in match.Tiles)
            {
                if (tile.IsSpecial)
                    matchScore += specialTileBonus;
                if (tile.IsEnhanced)
                    matchScore += enhancedTileBonus;
            }

            currentScore += matchScore;
            comboCount++;

            OnScoreChanged?.Invoke(currentScore);
            OnComboChanged?.Invoke(comboCount);

            // 최고 점수 업데이트
            if (currentScore > bestScore)
            {
                bestScore = currentScore;
                SaveBestScore();
                OnBestScoreChanged?.Invoke(bestScore);
            }
        }

        /// <summary>
        /// 콤보 리셋
        /// </summary>
        public void ResetCombo()
        {
            comboCount = 0;
            OnComboChanged?.Invoke(comboCount);
        }

        /// <summary>
        /// 최고 점수 저장
        /// </summary>
        private void SaveBestScore()
        {
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 최고 점수 로드
        /// </summary>
        private void LoadBestScore()
        {
            bestScore = PlayerPrefs.GetInt("BestScore", 0);
        }

        // 이벤트
        public System.Action<int> OnScoreChanged;
        public System.Action<int> OnComboChanged;
        public System.Action<int> OnBestScoreChanged;
    }
}
