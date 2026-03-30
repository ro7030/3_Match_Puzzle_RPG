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

            if (gameBoard == null)
            {
                for (int i = 0; i < boards.Length; i++)
                {
                    var b = boards[i];
                    if (b != null && b.Tiles != null)
                    {
                        gameBoard = b;
                        break;
                    }
                }
            }

            if (gameBoard == null && boards.Length > 0)
                gameBoard = boards[0];
        }

        public void SetDragThreshold(float threshold)
        {
            dragThreshold = Mathf.Max(1f, threshold);
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

            // 드래그 인식이 어려운 환경(튜토리얼/트랙패드) 대응:
            // 이미 선택된 타일이 있고, 이번에 누른 타일이 인접하면 즉시 스왑.
            if (selectedTile != null && selectedTile != tile)
            {
                if (!selectedTile.IsEmpty && !tile.IsEmpty && selectedTile.IsAdjacent(tile))
                {
                    TrySwapTiles(selectedTile, tile);
                    hasSwappedThisPress = true;
                    selectedTile.SetSelected(false);
                    selectedTile = null;
                    return;
                }

                selectedTile.SetSelected(false);
                selectedTile = null;
            }

            selectedTile = tile;
            selectedTile.SetSelected(true);
            dragStartPos = eventData.position;
            hasSwappedThisPress = false;
        }

        public void OnTilePointerUp(Tile tile)
        {
            // 2-클릭 스왑을 위해 단순 클릭만으로는 선택을 유지합니다.
            // (드래그 스왑이 발생한 경우에만 선택 해제)
            if (hasSwappedThisPress && selectedTile != null)
            {
                selectedTile.SetSelected(false);
                selectedTile = null;
            }
        }

        public void OnTileDrag(Tile tile, PointerEventData eventData)
        {
            if (selectedTile == null || hasSwappedThisPress) return;

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

        /// <summary>
        /// 드래그 이벤트가 플랫폼/입력 모듈에서 누락되는 경우를 대비한 보조 경로.
        /// 마우스를 누른 채 인접 타일 위로 들어오면 즉시 스왑합니다.
        /// </summary>
        public void OnTilePointerEnter(Tile tile)
        {
            if (selectedTile == null || hasSwappedThisPress) return;
            if (tile == null || tile == selectedTile) return;
            if (tile.IsEmpty || selectedTile.IsEmpty) return;
            if (!Input.GetMouseButton(0)) return;
            if (!selectedTile.IsAdjacent(tile)) return;

            TrySwapTiles(selectedTile, tile);
            hasSwappedThisPress = true;
            selectedTile.SetSelected(false);
            selectedTile = null;
        }

        private Tile GetAdjacentInDirection(Tile tile, Vector2 screenDirection)
        {
            if (tile == null) return null;
            // 캐시된 gameBoard가 클릭 타일과 다른 보드를 가리킬 수 있어, 타일이 실제 등록된 보드 사용
            var board = GameBoard.FindBoardForTile(tile);
            if (board == null)
            {
                RebindReferences();
                board = gameBoard;
            }
            if (board == null) return null;

            int x = tile.X, y = tile.Y;

            if (Mathf.Abs(screenDirection.x) > Mathf.Abs(screenDirection.y))
                return screenDirection.x > 0 ? board.GetTile(x + 1, y) : board.GetTile(x - 1, y);
            else
                return screenDirection.y > 0 ? board.GetTile(x, y + 1) : board.GetTile(x, y - 1);
        }

        private void TrySwapTiles(Tile tile1, Tile tile2)
        {
            if (tile1 == null || tile2 == null) return;
            if (tile1.IsEmpty || tile2.IsEmpty) return;
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
