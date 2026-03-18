using UnityEngine;

namespace Match3Puzzle.Board
{
    /// <summary>
    /// 그리드 좌표 시스템을 관리하는 클래스
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector2 gridOffset = Vector2.zero;

        private Vector2 _cellSize = Vector2.one; // 런타임에 SetBoundsToFitRect로 변경 가능

        public float CellSize => _cellSize.x; // 호환용
        public Vector2 CellSizeXY => _cellSize;
        public Vector2 GridOffset => gridOffset;

        private void Awake()
        {
            _cellSize = new Vector2(cellSize, cellSize);
        }

        /// <summary>
        /// 퍼즐 보드 프레임(RectTransform) 영역에 맞게 그리드 크기·오프셋 설정.
        /// cols x rows 타일이 프레임 안의 체커보드 칸에 딱 맞게 배치됨.
        /// </summary>
        public void SetBoundsToFitRect(Rect worldRect, int cols, int rows)
        {
            float cellX = worldRect.width / Mathf.Max(1, cols);
            float cellY = worldRect.height / Mathf.Max(1, rows);
            _cellSize = new Vector2(cellX, cellY);
            gridOffset = new Vector2(worldRect.xMin, worldRect.yMin);
        }

        /// <summary>
        /// 그리드 좌표를 월드 좌표로 변환
        /// </summary>
        public Vector3 GridToWorld(int x, int y)
        {
            float worldX = x * _cellSize.x + gridOffset.x;
            float worldY = y * _cellSize.y + gridOffset.y;
            return new Vector3(worldX, worldY, 0f);
        }

        /// <summary>
        /// 월드 좌표를 그리드 좌표로 변환
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int gridX = Mathf.RoundToInt((worldPos.x - gridOffset.x) / _cellSize.x);
            int gridY = Mathf.RoundToInt((worldPos.y - gridOffset.y) / _cellSize.y);
            return new Vector2Int(gridX, gridY);
        }

        /// <summary>
        /// 그리드 좌표가 유효한지 확인
        /// </summary>
        public bool IsValidGridPosition(int x, int y, int width, int height)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        /// <summary>
        /// 두 그리드 위치가 인접한지 확인
        /// </summary>
        public bool IsAdjacent(int x1, int y1, int x2, int y2)
        {
            int dx = Mathf.Abs(x1 - x2);
            int dy = Mathf.Abs(y1 - y2);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }
    }
}
