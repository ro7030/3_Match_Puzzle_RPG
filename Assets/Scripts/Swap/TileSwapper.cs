using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Match3Puzzle.Board;
using Match3Puzzle.Matching;
using Match3Puzzle.Core;
using Match3Puzzle.Clear;
using Match3Puzzle.Level;
using Match3Puzzle.Stage;

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
            RebindReferences();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RebindReferences();
            isSwapping = false;
        }

        private void RebindReferences()
        {
            gameBoard = FindFirstObjectByType<GameBoard>();
            matchDetector = GetComponent<MatchDetector>();
            tileClearer = GetComponent<TileClearer>();
            levelManager = FindFirstObjectByType<LevelManager>();
        }

        /// <summary>
        /// 타일 교환
        /// </summary>
        public void SwapTiles(Tile tile1, Tile tile2)
        {
            if (gameBoard == null || matchDetector == null)
                RebindReferences();
            if (isSwapping) return;
            if (tile1 == null || tile2 == null) return;
            if (!tile1.IsAdjacent(tile2)) return;
            if (gameBoard == null || matchDetector == null) return;

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
                // 플레이어의 직접 스왑으로 매칭이 성립한 경우에만 턴 +1
                if (levelManager == null)
                    levelManager = FindFirstObjectByType<LevelManager>();
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
                if (!BattlePhaseRuntime.IsBattleCutsceneActive)
                    GameManager.Instance.ChangeState(GameState.Playing);
            }

            isSwapping = false;
        }
    }
}
