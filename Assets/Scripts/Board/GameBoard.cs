using System.Collections.Generic;
using UnityEngine;
using Match3Puzzle.Core;
using Match3Puzzle.Matching;
using Match3Puzzle.Spawn;

namespace Match3Puzzle.Board
{
    /// <summary>
    /// 10x10 슬롯 기반 보드. 사용자가 미리 배치한 100개 Tile(Image)을 찾아 초기화.
    /// </summary>
    public class GameBoard : MonoBehaviour
    {
        [Header("Board Settings")]
        [Tooltip("100개 Tile이 자식으로 있는 부모 (PuzzleBoardFrame 내부 등)")]
        [SerializeField] private Transform tileSlotParent;
        [Tooltip("0~3: 기본 / 4~7: 강화 스프라이트")]
        [SerializeField] private Sprite[] tileSprites;

        [Header("Board Size")]
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private int tileTypeCount = 4;

        private Tile[,] tiles;
        private bool isInitialized = false;

        public int Width => width;
        public int Height => height;
        public Tile[,] Tiles => tiles;
        public bool IsInitialized => isInitialized;
        public Sprite[] TileSprites => tileSprites;

        /// <summary>
        /// 타일 스프라이트 가져오기
        /// </summary>
        public Sprite GetTileSprite(int tileType, bool isEnhanced = false)
        {
            if (tileSprites == null) return null;
            int index = isEnhanced && (tileType + 4) < tileSprites.Length ? tileType + 4 : tileType;
            if (index >= 0 && index < tileSprites.Length) return tileSprites[index];
            return null;
        }

        private void Awake()
        {
            if (tileSlotParent == null)
                tileSlotParent = transform;
        }

        public void InitializeBoard(int boardWidth, int boardHeight, int types)
        {
            width = boardWidth;
            height = boardHeight;
            tileTypeCount = types;

            CollectTilesFromParent();
            ClearBoard();
            FillBoard();

            isInitialized = true;
            Debug.Log($"[GameBoard] 보드 초기화 완료: {width}x{height} (슬롯 기반)");
        }

        /// <summary>
        /// tileSlotParent 자식에서 Tile 수집. 100개 있으면 10x10, 64개면 8x8 등으로 자동 적용.
        /// </summary>
        private void CollectTilesFromParent()
        {
            var tileList = new List<Tile>();
            if (tileSlotParent != null)
                tileList.AddRange(tileSlotParent.GetComponentsInChildren<Tile>(true));

            // 재진입 시 참조가 끊어졌거나(파괴된 씬 오브젝트 참조), 슬롯 부모가 비어있는 경우
            // 씬에서 Tile들을 다시 찾아 슬롯 부모를 런타임에 복구한다.
            if (tileList.Count == 0)
            {
                var allTiles = FindObjectsByType<Tile>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (allTiles != null && allTiles.Length > 0)
                {
                    tileList.AddRange(allTiles);

                    Transform bestParent = null;
                    int bestCount = 0;
                    var countByParent = new Dictionary<Transform, int>();
                    for (int i = 0; i < allTiles.Length; i++)
                    {
                        var t = allTiles[i];
                        if (t == null) continue;
                        var p = t.transform.parent;
                        if (p == null) continue;
                        if (!countByParent.ContainsKey(p))
                            countByParent[p] = 0;
                        countByParent[p]++;
                    }

                    foreach (var kv in countByParent)
                    {
                        if (kv.Value > bestCount)
                        {
                            bestCount = kv.Value;
                            bestParent = kv.Key;
                        }
                    }

                    if (bestParent != null)
                        tileSlotParent = bestParent;
                }
            }

            int total = tileList.Count;
            if (total == 0)
            {
                string parentName = tileSlotParent != null ? tileSlotParent.name : "NULL";
                Debug.LogError($"[GameBoard] Tile 슬롯이 없습니다. tileSlotParent={parentName}");
                return;
            }

            int gridSize = Mathf.RoundToInt(Mathf.Sqrt(total));
            if (gridSize * gridSize != total)
            {
                if (total >= 100) gridSize = 10;
                else if (total >= 64) gridSize = 8;
                else gridSize = Mathf.Max(1, Mathf.FloorToInt(Mathf.Sqrt(total)));
            }
            width = gridSize;
            height = total / width;
            // 슬롯 수가 타일 수보다 적으면 일부 Tile이 배열에 안 들어가 ContainsTile이 실패함
            while (width * height < total)
                height++;

            tiles = new Tile[width, height];
            tileList.Sort((a, b) =>
            {
                var rtA = a.RectTransform;
                var rtB = b.RectTransform;
                if (rtA == null || rtB == null) return 0;
                Vector2 pA = rtA.anchoredPosition;
                Vector2 pB = rtB.anchoredPosition;
                if (Mathf.Abs(pA.y - pB.y) > 5f)
                    return pA.y.CompareTo(pB.y);
                return pA.x.CompareTo(pB.x);
            });

            int idx = 0;
            for (int y = 0; y < height && idx < tileList.Count; y++)
            {
                for (int x = 0; x < width && idx < tileList.Count; x++)
                {
                    var tile = tileList[idx++];
                    tile.SetPosition(x, y);
                    tiles[x, y] = tile;
                }
            }
        }

        private void ClearBoard()
        {
            if (tiles == null) return;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (tiles[x, y] != null)
                        tiles[x, y].SetEmpty();
                }
            }
        }

        private void CreateBoard()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    InitSlot(x, y);
                }
            }
        }

        private void InitSlot(int x, int y)
        {
            var tile = tiles[x, y];
            if (tile == null) return;

            int tileType = GetRandomTileType(x, y);
            Sprite sprite = GetTileSprite(tileType);
            tile.Initialize(tileType, x, y, sprite);
        }

        private void FillBoard()
        {
            CreateBoard();
            int attempts = 0;
            while (attempts < 100)
            {
                bool hasMatches = false;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int t = GetRandomTileType(x, y);
                        var tile = tiles[x, y];
                        if (tile != null)
                            tile.Initialize(t, x, y, GetTileSprite(t));
                    }
                }

                var detector = GetComponent<MatchDetector>();
                if (detector != null && detector.FindAllMatches(tiles).Count > 0)
                    hasMatches = true;

                if (!hasMatches) break;
                attempts++;
            }
        }

        private int GetRandomTileType(int x, int y)
        {
            int type = Random.Range(0, tileTypeCount);
            int attempts = 0;
            while (attempts < 10)
            {
                bool valid = true;
                if (x > 0 && tiles[x - 1, y] != null && !tiles[x - 1, y].IsEmpty && tiles[x - 1, y].TileType == type) valid = false;
                if (y > 0 && tiles[x, y - 1] != null && !tiles[x, y - 1].IsEmpty && tiles[x, y - 1].TileType == type) valid = false;
                if (valid) break;
                type = Random.Range(0, tileTypeCount);
                attempts++;
            }
            return type;
        }

        public void SpawnEnhancedTileAt(int x, int y, int tileType)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return;
            var tile = tiles[x, y];
            if (tile == null) return;

            Sprite sprite = GetTileSprite(tileType, true);
            tile.SetFilled(tileType, sprite, true);
        }

        public Tile GetTile(int x, int y)
        {
            if (tiles == null)
                return null;

            int maxX = tiles.GetLength(0);
            int maxY = tiles.GetLength(1);
            if (x >= 0 && x < maxX && y >= 0 && y < maxY)
                return tiles[x, y];
            return null;
        }

        /// <summary>
        /// 이 보드의 타일 배열에 해당 타일 인스턴스가 들어있는지 (슬롯이 부모 밖에 있어도 판별 가능)
        /// </summary>
        public bool ContainsTile(Tile tile)
        {
            if (tile == null || tiles == null) return false;
            int w = tiles.GetLength(0);
            int h = tiles.GetLength(1);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (tiles[x, y] == tile)
                        return true;
                }
            }
            return false;
        }

        /// <summary>슬롯 루트(타일이 배열에 없을 때도 계층으로 소속 판별)</summary>
        public Transform TileSlotsRoot => tileSlotParent != null ? tileSlotParent : transform;

        public bool OwnsTileByHierarchy(Tile tile)
        {
            if (tile == null) return false;
            var root = TileSlotsRoot;
            if (root == null) return false;
            return tile.transform == root || tile.transform.IsChildOf(root);
        }

        /// <summary>배열이 비어 있을 때만 슬롯에서 다시 수집 (런타임 복구)</summary>
        public void EnsureTilesArrayFromHierarchy()
        {
            if (tiles != null) return;
            CollectTilesFromParent();
        }

        /// <summary>
        /// 클릭/스왑에 사용할 올바른 GameBoard 찾기.
        /// tileSlotParent가 GameBoard 밖에 있으면 GetComponentInParent만으로는 실패할 수 있어 배열 검색을 병행합니다.
        /// </summary>
        public static GameBoard FindBoardForTile(Tile tile)
        {
            if (tile == null) return null;

            var fromParent = tile.GetComponentInParent<GameBoard>();
            if (fromParent != null && (fromParent.ContainsTile(tile) || fromParent.OwnsTileByHierarchy(tile)))
                return fromParent;

            var boards = FindObjectsByType<GameBoard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < boards.Length; i++)
            {
                var b = boards[i];
                if (b != null && b.ContainsTile(tile))
                    return b;
            }

            for (int i = 0; i < boards.Length; i++)
            {
                var b = boards[i];
                if (b != null && b.OwnsTileByHierarchy(tile))
                    return b;
            }
            return null;
        }

        /// <summary>인접 스왑 시 두 타일이 속한 단일 보드 (다르면 null 아닌 쪽 우선)</summary>
        public static GameBoard FindBoardForSwap(Tile a, Tile b)
        {
            if (a == null || b == null) return null;

            var boardA = FindBoardForTile(a);
            if (boardA != null && (boardA.ContainsTile(b) || boardA.OwnsTileByHierarchy(b)))
                return boardA;

            var boardB = FindBoardForTile(b);
            if (boardB != null && (boardB.ContainsTile(a) || boardB.OwnsTileByHierarchy(a)))
                return boardB;

            return boardA != null ? boardA : boardB;
        }

        /// <summary>
        /// 튜토리얼 전용 보드 초기화.
        /// Inspector에서 미리 설정한 스프라이트(흰색 이미지 포함)를 그대로 유지하면서
        /// 모든 타일을 tileType=0(칼)으로 설정. FillBoard/ClearBoard를 호출하지 않음.
        /// </summary>
        public void InitializeTutorialBoard()
        {
            CollectTilesFromParent();

            if (tiles == null) return;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var tile = tiles[x, y];
                    if (tile == null) continue;

                    // 스프라이트 유무에 관계없이 모두 칼 타입으로 설정
                    // (image.sprite는 절대 변경하지 않음 → Inspector 이미지 유지)
                    tile.SetTileTypeOnly(0);
                }
            }

            isInitialized = true;
            Debug.Log("[GameBoard] 튜토리얼 보드 초기화 완료 (Inspector 스프라이트 유지)");
        }

        /// <summary>
        /// 두 타일 내용 교환 (스프라이트·타입만, 슬롯 위치는 고정)
        /// </summary>
        public void SwapTiles(Tile tile1, Tile tile2)
        {
            if (tile1 == null || tile2 == null) return;
            if (!tile1.IsAdjacent(tile2)) return;

            int t1 = tile1.TileType;
            bool e1 = tile1.IsEnhanced;
            Sprite s1 = GetTileSprite(t1, e1);

            tile1.Initialize(tile2.TileType, tile1.X, tile1.Y, GetTileSprite(tile2.TileType, tile2.IsEnhanced), tile2.IsEnhanced);
            tile2.Initialize(t1, tile2.X, tile2.Y, s1, e1);
        }
    }
}
