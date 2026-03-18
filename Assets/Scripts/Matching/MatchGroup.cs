using System.Collections.Generic;
using UnityEngine;
using Match3Puzzle.Board;

namespace Match3Puzzle.Matching
{
    /// <summary>
    /// 매칭된 타일들의 그룹을 나타내는 클래스
    /// </summary>
    public class MatchGroup
    {
        public List<Tile> Tiles { get; private set; }
        public MatchType Type { get; private set; }
        public int TileType { get; private set; }

        public MatchGroup(MatchType type, int tileType)
        {
            Tiles = new List<Tile>();
            Type = type;
            TileType = tileType;
        }

        public void AddTile(Tile tile)
        {
            if (tile != null && !Tiles.Contains(tile))
            {
                Tiles.Add(tile);
            }
        }

        public int Count => Tiles.Count;
    }

    /// <summary>
    /// 매칭 타입
    /// </summary>
    public enum MatchType
    {
        Horizontal,    // 가로 매칭
        Vertical,      // 세로 매칭
        LShape,        // L자 모양
        TShape,        // T자 모양
        Cross          // 십자 모양
    }
}
