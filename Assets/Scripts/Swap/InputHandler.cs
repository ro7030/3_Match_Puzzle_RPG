using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Match3Puzzle.Board;
using Match3Puzzle.Core;

namespace Match3Puzzle.Swap
{
    /// <summary>
    /// 꾹 누른 상태로 위/아래/좌/우 드래그 시 인접 1칸 스왑.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private float dragThreshold = 50f;
        [SerializeField] private bool debugInput = true;

        private Tile selectedTile = null;
        private Vector2 dragStartPos;
        private bool hasSwappedThisPress;
        private float _lastDebugLogTime = -999f;

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
            selectedTile = null;
            hasSwappedThisPress = false;
        }

        private void RebindReferences()
        {
            gameBoard = FindFirstObjectByType<GameBoard>();
        }

        private void Update()
        {
            if (!debugInput) return;
            if (!Input.GetMouseButtonDown(0)) return;
            if (EventSystem.current == null) return;

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            if (results.Count == 0)
            {
                DebugInputOnce("[InputHandler] Raycast 결과 없음 (클릭 대상 UI 없음)");
                return;
            }

            var top = results[0].gameObject;
            bool topIsTile = top != null && top.GetComponent<Tile>() != null;
            DebugInputOnce($"[InputHandler] TopRaycast={top?.name} / isTile={topIsTile} / hits={results.Count}");
        }

        public void OnTilePointerDown(Tile tile, PointerEventData eventData)
        {
            if (GameManager.Instance == null)
            {
                DebugInputOnce("[InputHandler] PointerDown 차단: GameManager.Instance == null");
                return;
            }

            if (GameManager.Instance.CurrentState != GameState.Playing)
            {
                DebugInputOnce($"[InputHandler] PointerDown 차단: state={GameManager.Instance.CurrentState}");
                return;
            }
            if (tile == null)
            {
                DebugInputOnce("[InputHandler] PointerDown 차단: tile == null");
                return;
            }
            if (tile.IsEmpty)
            {
                DebugInputOnce($"[InputHandler] PointerDown 차단: tile.IsEmpty=true (x={tile.X}, y={tile.Y}, type={tile.TileType})");
                return;
            }

            selectedTile = tile;
            selectedTile.SetSelected(true);
            dragStartPos = eventData.position;
            hasSwappedThisPress = false;
        }

        public void OnTilePointerUp(Tile tile)
        {
            if (selectedTile != null)
            {
                selectedTile.SetSelected(false);
                selectedTile = null;
            }
        }

        public void OnTileDrag(Tile tile, PointerEventData eventData)
        {
            if (selectedTile == null || hasSwappedThisPress) return;
            if (gameBoard == null)
                RebindReferences();
            if (gameBoard == null)
            {
                DebugInputOnce("[InputHandler] Drag 차단: gameBoard == null");
                return;
            }

            Vector2 delta = eventData.position - dragStartPos;
            if (delta.sqrMagnitude < dragThreshold * dragThreshold) return;

            Vector2 dir = delta.normalized;
            Tile targetTile = GetAdjacentInDirection(selectedTile, dir);
            if (targetTile != null && !targetTile.IsEmpty)
            {
                TrySwapTiles(selectedTile, targetTile);
                hasSwappedThisPress = true;
                selectedTile.SetSelected(false);
                selectedTile = null;
            }
        }

        private Tile GetAdjacentInDirection(Tile tile, Vector2 screenDirection)
        {
            if (tile == null || gameBoard == null) return null;
            int x = tile.X, y = tile.Y;

            if (Mathf.Abs(screenDirection.x) > Mathf.Abs(screenDirection.y))
                return screenDirection.x > 0 ? gameBoard.GetTile(x + 1, y) : gameBoard.GetTile(x - 1, y);
            else
                return screenDirection.y > 0 ? gameBoard.GetTile(x, y + 1) : gameBoard.GetTile(x, y - 1);
        }

        private void TrySwapTiles(Tile tile1, Tile tile2)
        {
            if (tile1 == null || tile2 == null) return;
            if (!tile1.IsAdjacent(tile2)) return;

            var swapper = GetComponent<TileSwapper>();
            if (swapper != null)
                swapper.SwapTiles(tile1, tile2);
            else
                DebugInputOnce("[InputHandler] Swap 차단: TileSwapper 컴포넌트 없음");
        }

        private void DebugInputOnce(string message)
        {
            if (!debugInput) return;
            if (Time.unscaledTime - _lastDebugLogTime < 0.5f) return;
            _lastDebugLogTime = Time.unscaledTime;
            Debug.Log(message);
        }
    }
}
