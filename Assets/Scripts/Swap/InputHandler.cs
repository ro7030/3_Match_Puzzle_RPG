using UnityEngine;
using UnityEngine.EventSystems;
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

        private Tile selectedTile = null;
        private Vector2 dragStartPos;
        private bool hasSwappedThisPress;

        private void Awake()
        {
            if (gameBoard == null) gameBoard = FindFirstObjectByType<GameBoard>();
        }

        public void OnTilePointerDown(Tile tile, PointerEventData eventData)
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                return;
            if (tile == null || tile.IsEmpty) return;

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
            if (gameBoard == null) return;

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
        }
    }
}
