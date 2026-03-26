using System.Collections;
using UnityEngine;
using Match3Puzzle.Board;
using Match3Puzzle.Matching;

namespace Match3Puzzle.Spawn
{
    /// <summary>
    /// 슬롯 기반: 빈 칸에 새 타입 할당 (스프라이트만 갱신)
    /// </summary>
    public class TileSpawner : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private MatchDetector matchDetector;

        private void Awake()
        {
            if (gameBoard == null) gameBoard = FindFirstObjectByType<GameBoard>();
            if (matchDetector == null) matchDetector = GetComponent<MatchDetector>();
        }

        public IEnumerator SpawnNewTiles()
        {
            if (gameBoard == null)
                gameBoard = FindFirstObjectByType<GameBoard>();
            if (gameBoard == null || gameBoard.Tiles == null)
            {
                Debug.LogWarning("[TileSpawner] SpawnNewTiles 중단: GameBoard 또는 Tiles가 null");
                yield break;
            }

            int width = gameBoard.Width;
            int height = gameBoard.Height;
            Tile[,] tiles = gameBoard.Tiles;
            int typeCount = Mathf.Min(4, gameBoard.TileSprites != null ? gameBoard.TileSprites.Length : 4);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var tile = tiles[x, y];
                    if (tile != null && tile.IsEmpty)
                    {
                        int tileType = GetRandomTileType(tiles, x, y, width, height, typeCount);
                        var sprite = gameBoard.GetTileSprite(tileType);
                        tile.SetFilled(tileType, sprite, false);
                    }
                }
            }

            yield return null;
        }

        private int GetRandomTileType(Tile[,] tiles, int x, int y, int width, int height, int typeCount)
        {
            int type = Random.Range(0, typeCount);
            int attempts = 0;
            while (attempts < 10)
            {
                bool valid = true;
                if (x > 0)
                {
                    var left = tiles[x - 1, y];
                    if (left != null && !left.IsEmpty && left.TileType == type) valid = false;
                }
                if (y > 0)
                {
                    var below = tiles[x, y - 1];
                    if (below != null && !below.IsEmpty && below.TileType == type) valid = false;
                }
                if (valid) break;
                type = Random.Range(0, typeCount);
                attempts++;
            }
            return type;
        }
    }
}
