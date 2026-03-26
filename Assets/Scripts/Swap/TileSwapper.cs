using System;
using System.Collections;
using System.Collections.Generic;
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
        /// <summary>스왑 코루틴이 끝난 뒤(매칭/되돌리기 포함). 튜토리얼 등에서 구독.</summary>
        public static event Action<Tile, Tile> SwapCompleted;

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
            var active = SceneManager.GetActiveScene();
            var boards = FindObjectsByType<GameBoard>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            gameBoard = null;
            for (int i = 0; i < boards.Length; i++)
            {
                var b = boards[i];
                if (b == null) continue;
                if (b.gameObject.scene == active)
                {
                    gameBoard = b;
                    break;
                }
            }
            if (gameBoard == null && boards.Length > 0)
                gameBoard = boards[0];

            matchDetector = GetComponent<MatchDetector>();
            tileClearer = GetComponent<TileClearer>();
            var levels = FindObjectsByType<LevelManager>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            levelManager = null;
            for (int i = 0; i < levels.Length; i++)
            {
                var l = levels[i];
                if (l == null) continue;
                if (l.gameObject.scene == active)
                {
                    levelManager = l;
                    break;
                }
            }
            if (levelManager == null && levels.Length > 0)
                levelManager = levels[0];
        }

        /// <summary>
        /// 타일 교환
        /// </summary>
        public void SwapTiles(Tile tile1, Tile tile2)
        {
            EnsureRuntimeReferences(tile1, tile2);
            if (isSwapping) return;
            if (tile1 == null || tile2 == null) return;
            if (!tile1.IsAdjacent(tile2)) return;

            var boardForSwap = GameBoard.FindBoardForSwap(tile1, tile2);
            boardForSwap?.EnsureTilesArrayFromHierarchy();
            if (boardForSwap == null || boardForSwap.Tiles == null || matchDetector == null)
            {
                Debug.LogWarning("[TileSwapper] 스왑 불가: 타일이 속한 GameBoard를 찾지 못했거나 Tiles가 비어 있습니다.");
                return;
            }

            StartCoroutine(SwapTilesCoroutine(tile1, tile2, boardForSwap));
        }

        private IEnumerator SwapTilesCoroutine(Tile tile1, Tile tile2, GameBoard board)
        {
            isSwapping = true;
            // yield 이후에도 동일한 보드를 쓰도록 로컬로 고정 (필드 gameBoard가 다른 씬/리바인딩으로 바뀌는 경우 방지)
            GameBoard swapBoard = board;
            if (swapBoard == null || swapBoard.Tiles == null)
            {
                Debug.LogWarning("[TileSwapper] 보드 참조가 무효입니다. 스왑을 취소합니다.");
                RestorePlayingStateAfterSwapFailure();
                yield break;
            }
            gameBoard = swapBoard;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Swapping);
            }

            // 교환 실행
            swapBoard.SwapTiles(tile1, tile2);

            // WaitForSeconds 동안 다른 스크립트가 tiles 참조를 비우는 경우 대비해 스냅샷 유지
            Tile[,] boardTiles = swapBoard.Tiles;

            // 교환 애니메이션 대기
            yield return new WaitForSeconds(0.3f);

            if (boardTiles == null)
            {
                Debug.LogWarning("[TileSwapper] 보드 타일 배열이 null이라 스왑을 취소합니다.");
                swapBoard.SwapTiles(tile1, tile2);
                RestorePlayingStateAfterSwapFailure();
                yield break;
            }

            List<MatchGroup> matches = null;
            try
            {
                // 매칭 체크
                matches = matchDetector.FindAllMatches(boardTiles);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[TileSwapper] 매칭 검사 중 예외: {ex.Message}");
                swapBoard.SwapTiles(tile1, tile2);
                RestorePlayingStateAfterSwapFailure();
                yield break;
            }

            if (matches != null && matches.Count > 0)
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
                swapBoard.SwapTiles(tile1, tile2);
                yield return new WaitForSeconds(0.3f);
            }

            if (GameManager.Instance != null)
            {
                if (!BattlePhaseRuntime.IsBattleCutsceneActive)
                    GameManager.Instance.ChangeState(GameState.Playing);
            }

            SwapCompleted?.Invoke(tile1, tile2);
            isSwapping = false;
        }

        private void RestorePlayingStateAfterSwapFailure()
        {
            if (GameManager.Instance != null && !BattlePhaseRuntime.IsBattleCutsceneActive)
                GameManager.Instance.ChangeState(GameState.Playing);
            isSwapping = false;
        }

        private void EnsureRuntimeReferences(Tile tile1, Tile tile2)
        {
            if (matchDetector == null)
                matchDetector = GetComponent<MatchDetector>();
            if (tileClearer == null)
                tileClearer = GetComponent<TileClearer>();

            // 스왑 대상 타일이 실제로 등록된 보드로 항상 맞춤 (다른 씬/더미 GameBoard 캐시 방지)
            if (tile1 != null && tile2 != null)
            {
                var resolved = GameBoard.FindBoardForSwap(tile1, tile2);
                if (resolved != null)
                {
                    gameBoard = resolved;
                    return;
                }
            }

            if (gameBoard != null && gameBoard.Tiles != null)
                return;

            // 폴백: 활성 씬 우선 + Tiles가 채워진 보드
            var active = SceneManager.GetActiveScene();
            var boards = FindObjectsByType<GameBoard>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < boards.Length; i++)
            {
                var b = boards[i];
                if (b == null) continue;
                if (b.gameObject.scene == active && b.Tiles != null)
                {
                    gameBoard = b;
                    return;
                }
            }
            for (int i = 0; i < boards.Length; i++)
            {
                var b = boards[i];
                if (b == null) continue;
                if (b.Tiles != null)
                {
                    gameBoard = b;
                    break;
                }
            }
        }
    }
}
