using System.Collections;
using UnityEngine;
using Match3Puzzle.Board;
using Match3Puzzle.Matching;
using Match3Puzzle.Core;
using Match3Puzzle.Clear;
using Match3Puzzle.Level;

namespace Match3Puzzle.Swap
{
    /// <summary>
    /// 타일 교환을 처리하는 클래스
    /// </summary>
    public class TileSwapper : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private MatchDetector matchDetector;
        [SerializeField] private TileClearer tileClearer;
        [SerializeField] private LevelManager levelManager;

        private bool isSwapping = false;

        private void Awake()
        {
            if (gameBoard == null)
                gameBoard = FindFirstObjectByType<GameBoard>();

            if (matchDetector == null)
                matchDetector = GetComponent<MatchDetector>();

            if (tileClearer == null)
                tileClearer = GetComponent<TileClearer>();

            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelManager>();
        }

        /// <summary>
        /// 타일 교환
        /// </summary>
        public void SwapTiles(Tile tile1, Tile tile2)
        {
            if (isSwapping) return;
            if (tile1 == null || tile2 == null) return;
            if (!tile1.IsAdjacent(tile2)) return;

            StartCoroutine(SwapTilesCoroutine(tile1, tile2));
        }

        private IEnumerator SwapTilesCoroutine(Tile tile1, Tile tile2)
        {
            isSwapping = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Swapping);
            }

            // 교환 실행
            gameBoard.SwapTiles(tile1, tile2);

            // 교환 애니메이션 대기
            yield return new WaitForSeconds(0.3f);

            // 매칭 체크
            var matches = matchDetector.FindAllMatches(gameBoard.Tiles);

            if (matches.Count > 0)
            {
                // 매칭이 있으면 턴 1회 소모 후 제거 시작
                if (levelManager != null)
                    levelManager.IncrementMoves();

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ChangeState(GameState.Matching);
                }

                if (tileClearer != null)
                {
                    yield return StartCoroutine(tileClearer.ClearMatches(matches));
                }
            }
            else
            {
                // 매칭이 없으면 원위치
                gameBoard.SwapTiles(tile1, tile2);
                yield return new WaitForSeconds(0.3f);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Playing);
            }

            isSwapping = false;
        }
    }
}
