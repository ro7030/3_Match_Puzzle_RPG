using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Match3Puzzle.Board;

namespace Match3Puzzle.Gravity
{
    /// <summary>
    /// 슬롯 기반: 각 열에서 타입을 아래로 내리고 스프라이트만 갱신 (애니 없음)
    /// </summary>
    public class GravityController : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;

        private void Awake()
        {
            if (gameBoard == null)
                gameBoard = FindFirstObjectByType<GameBoard>();
        }

        /// <summary>
        /// 좌표계: y=0 = 아래(바닥), y=height-1 = 위(천장).
        /// 매칭된 칸이 비면 위에 있던 타일이 중력으로 아래로 내려와 채움.
        /// </summary>
        public IEnumerator ApplyGravity()
        {
            int width = gameBoard.Width;
            int height = gameBoard.Height;
            Tile[,] tiles = gameBoard.Tiles;

            for (int x = 0; x < width; x++)
            {
                // 각 열에서 비어 있지 않은 타일만 아래(y=0)부터 위(y=height-1) 순으로 수집
                var column = new List<(int type, bool enhanced)>();
                for (int y = 0; y < height; y++)
                {
                    var t = tiles[x, y];
                    if (t != null && !t.IsEmpty)
                        column.Add((t.TileType, t.IsEnhanced));
                }

                // 아래 슬롯(y=0)부터 채움 → 위 타일이 아래로 떨어지는 효과
                int fillY = 0;
                foreach (var (type, enhanced) in column)
                {
                    var slot = tiles[x, fillY];
                    if (slot != null)
                    {
                        var sprite = gameBoard.GetTileSprite(type, enhanced);
                        slot.SetFilled(type, sprite, enhanced);
                    }
                    fillY++;
                }
                // 위쪽 빈 칸은 비움 (나중에 TileSpawner가 새 타일로 채움)
                for (; fillY < height; fillY++)
                {
                    var slot = tiles[x, fillY];
                    if (slot != null)
                        slot.SetEmpty();
                }
            }

            yield return null;
        }
    }
}
