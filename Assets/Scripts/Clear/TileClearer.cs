using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Match3Puzzle.Board;
using Match3Puzzle.Matching;
using Match3Puzzle.Gravity;
using Match3Puzzle.Spawn;
using Match3Puzzle.Core;

namespace Match3Puzzle.Clear
{
    public class TileClearer : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private GravityController gravityController;
        [SerializeField] private TileSpawner tileSpawner;
        [SerializeField] private MatchDetector matchDetector;
        [SerializeField] private float clearDelay = 0.1f;
        [SerializeField] private float clearAnimationDuration = 0.3f;
        [SerializeField] private MatchEffectHandler matchEffectHandler;

        [Tooltip("true로 설정 시 매칭 후 중력(낙하)·스폰·연쇄 매칭을 건너뜀. 튜토리얼에서 빈 칸을 유지할 때 사용.")]
        [SerializeField] private bool skipGravityAndSpawn = false;

        /// <summary>매칭 발생 및 제거 시작 시 발생. TutorialManager 등에서 구독 가능.</summary>
        public event System.Action OnMatchCleared;

        private void Awake()
        {
            if (gameBoard == null) gameBoard = FindFirstObjectByType<GameBoard>();
            if (gravityController == null) gravityController = GetComponent<GravityController>();
            if (tileSpawner == null) tileSpawner = GetComponent<TileSpawner>();
            if (matchDetector == null) matchDetector = GetComponent<MatchDetector>();
            if (matchEffectHandler == null) matchEffectHandler = GetComponent<MatchEffectHandler>();
        }

        public IEnumerator ClearMatches(List<MatchGroup> matches)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ChangeState(GameState.Clearing);

            var tilesToClear = new HashSet<Tile>();
            var enhancedSpawns = new List<(int x, int y, int tileType)>();

            foreach (var match in matches)
            {
                foreach (var tile in match.Tiles)
                    tilesToClear.Add(tile);
                if (match.Count >= 4 && match.Tiles.Count > 0)
                    enhancedSpawns.Add((match.Tiles[0].X, match.Tiles[0].Y, Tile.GetBaseType(match.TileType)));
            }

            if (matchEffectHandler != null)
                matchEffectHandler.ApplyMatchEffects(matches);

            OnMatchCleared?.Invoke();

            foreach (var tile in tilesToClear)
            {
                tile.StartClearing();
                yield return new WaitForSeconds(clearDelay);
            }

            yield return new WaitForSeconds(clearAnimationDuration);

            foreach (var tile in tilesToClear)
                tile.SetEmpty();

            foreach (var (x, y, tileType) in enhancedSpawns)
                gameBoard.SpawnEnhancedTileAt(x, y, tileType);

            if (skipGravityAndSpawn)
            {
                // 튜토리얼 모드: 빈 칸을 채우지 않고 그대로 유지
                if (GameManager.Instance != null)
                    GameManager.Instance.ChangeState(GameState.Playing);
                yield break;
            }

            if (gravityController != null)
                yield return StartCoroutine(gravityController.ApplyGravity());

            if (tileSpawner != null)
                yield return StartCoroutine(tileSpawner.SpawnNewTiles());

            var newMatches = matchDetector.FindAllMatches(gameBoard.Tiles);
            if (newMatches.Count > 0)
                yield return StartCoroutine(ClearMatches(newMatches));
            else if (GameManager.Instance != null)
                GameManager.Instance.ChangeState(GameState.Playing);
        }
    }
}
