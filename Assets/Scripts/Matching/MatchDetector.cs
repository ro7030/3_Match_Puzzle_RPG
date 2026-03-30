using System.Collections.Generic;
using UnityEngine;
using Match3Puzzle.Board;

namespace Match3Puzzle.Matching
{
    /// <summary>
    /// 매칭을 감지하는 클래스
    /// </summary>
    public class MatchDetector : MonoBehaviour
    {
        /// <summary>
        /// 모든 매칭 찾기
        /// </summary>
        public List<MatchGroup> FindAllMatches(Tile[,] tiles)
        {
            List<MatchGroup> allMatches = new List<MatchGroup>();
            if (tiles == null) return allMatches;
            if (tiles.GetLength(0) == 0 || tiles.GetLength(1) == 0) return allMatches;
            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);

            HashSet<Tile> processedTiles = new HashSet<Tile>();

            // 가로 매칭 찾기
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    MatchGroup match = FindHorizontalMatch(tiles, x, y, width);
                    if (match != null && match.Count >= 3)
                    {
                        bool isNewMatch = false;
                        foreach (var tile in match.Tiles)
                        {
                            if (!processedTiles.Contains(tile))
                            {
                                processedTiles.Add(tile);
                                isNewMatch = true;
                            }
                        }

                        if (isNewMatch)
                        {
                            allMatches.Add(match);
                        }
                    }
                }
            }

            // 세로 매칭 찾기
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    MatchGroup match = FindVerticalMatch(tiles, x, y, height);
                    if (match != null && match.Count >= 3)
                    {
                        bool isNewMatch = false;
                        foreach (var tile in match.Tiles)
                        {
                            if (!processedTiles.Contains(tile))
                            {
                                processedTiles.Add(tile);
                                isNewMatch = true;
                            }
                        }

                        if (isNewMatch)
                        {
                            allMatches.Add(match);
                        }
                    }
                }
            }

            return allMatches;
        }

        /// <summary>
        /// 특정 위치에서 가로 매칭 찾기
        /// </summary>
        public MatchGroup FindHorizontalMatch(Tile[,] tiles, int startX, int y, int width)
        {
            if (tiles == null) return null;
            if (tiles.GetLength(0) == 0 || tiles.GetLength(1) == 0) return null;
            if (y < 0 || y >= tiles.GetLength(1)) return null;
            if (startX < 0 || startX >= width) return null;

            Tile startTile = tiles[startX, y];
            if (startTile == null || startTile.IsEmpty) return null;

            MatchGroup match = new MatchGroup(MatchType.Horizontal, startTile.TileType);
            match.AddTile(startTile);

            // 오른쪽으로 확장
            for (int x = startX + 1; x < width; x++)
            {
                Tile tile = tiles[x, y];
                if (tile != null && !tile.IsEmpty && tile.IsSameType(startTile))
                {
                    match.AddTile(tile);
                }
                else
                {
                    break;
                }
            }

            return match.Count >= 3 ? match : null;
        }

        /// <summary>
        /// 특정 위치에서 세로 매칭 찾기
        /// </summary>
        public MatchGroup FindVerticalMatch(Tile[,] tiles, int x, int startY, int height)
        {
            if (tiles == null) return null;
            if (tiles.GetLength(0) == 0 || tiles.GetLength(1) == 0) return null;
            if (x < 0 || x >= tiles.GetLength(0)) return null;
            if (startY < 0 || startY >= height) return null;

            Tile startTile = tiles[x, startY];
            if (startTile == null || startTile.IsEmpty) return null;

            MatchGroup match = new MatchGroup(MatchType.Vertical, startTile.TileType);
            match.AddTile(startTile);

            // 아래로 확장
            for (int y = startY + 1; y < height; y++)
            {
                Tile tile = tiles[x, y];
                if (tile != null && !tile.IsEmpty && tile.IsSameType(startTile))
                {
                    match.AddTile(tile);
                }
                else
                {
                    break;
                }
            }

            return match.Count >= 3 ? match : null;
        }

        /// <summary>
        /// 특정 위치에서 매칭이 있는지 확인
        /// </summary>
        public bool HasMatchAt(Tile[,] tiles, int x, int y)
        {
            if (tiles == null) return false;
            if (tiles.GetLength(0) == 0 || tiles.GetLength(1) == 0) return false;
            MatchGroup horizontal = FindHorizontalMatch(tiles, x, y, tiles.GetLength(0));
            MatchGroup vertical = FindVerticalMatch(tiles, x, y, tiles.GetLength(1));

            return (horizontal != null && horizontal.Count >= 3) ||
                   (vertical != null && vertical.Count >= 3);
        }

        /// <summary>
        /// 두 타일 교환 후 매칭이 있는지 확인
        /// </summary>
        public bool WouldCreateMatch(Tile[,] tiles, Tile tile1, Tile tile2)
        {
            if (tiles == null) return false;
            if (tiles.GetLength(0) == 0 || tiles.GetLength(1) == 0) return false;
            if (tile1 == null || tile2 == null) return false;
            if (!tile1.IsAdjacent(tile2)) return false;

            // 임시로 교환
            int x1 = tile1.X;
            int y1 = tile1.Y;
            int x2 = tile2.X;
            int y2 = tile2.Y;

            Tile temp = tiles[x1, y1];
            tiles[x1, y1] = tiles[x2, y2];
            tiles[x2, y2] = temp;

            // 매칭 체크
            bool hasMatch = HasMatchAt(tiles, x1, y1) || HasMatchAt(tiles, x2, y2);

            // 원위치
            tiles[x2, y2] = tiles[x1, y1];
            tiles[x1, y1] = temp;

            return hasMatch;
        }
    }
}
