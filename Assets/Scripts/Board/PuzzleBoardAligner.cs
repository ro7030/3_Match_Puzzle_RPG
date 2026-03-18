using UnityEngine;

namespace Match3Puzzle.Board
{
    /// <summary>
    /// PuzzleBoardFrame(RectTransform) 영역에 타일 그리드가 딱 맞게 들어가도록
    /// GridManager의 cellSize, gridOffset을 설정합니다.
    /// </summary>
    public class PuzzleBoardAligner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform puzzleBoardFrame;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private GridManager gridManager;

        [Header("Grid Size")]
        [Tooltip("타일 가로 개수 (GameManager boardWidth와 동일하게)")]
        [SerializeField] private int gridColumns = 10;
        [Tooltip("타일 세로 개수 (GameManager boardHeight와 동일하게)")]
        [SerializeField] private int gridRows = 10;

        [Header("Z Plane")]
        [Tooltip("타일이 놓일 Z 평면. 카메라로부터의 거리")]
        [SerializeField] private float cameraDistance = 10f;

        private void Awake()
        {
            if (puzzleBoardFrame == null)
            {
                var go = GameObject.Find("PuzzleBoardFrame");
                if (go != null)
                    puzzleBoardFrame = go.GetComponent<RectTransform>();
            }

            if (targetCamera == null)
                targetCamera = Camera.main;

            if (gridManager == null)
                gridManager = FindFirstObjectByType<GridManager>();
        }

        /// <summary>
        /// 프레임 영역을 월드 좌표로 변환 후 GridManager에 적용.
        /// StartGame() 전에 호출되어야 합니다.
        /// </summary>
        /// <param name="cols">타일 가로 개수 (null이면 Inspector 값 사용)</param>
        /// <param name="rows">타일 세로 개수 (null이면 Inspector 값 사용)</param>
        public void AlignBoard(int? cols = null, int? rows = null)
        {
            if (puzzleBoardFrame == null || targetCamera == null || gridManager == null)
            {
                Debug.LogWarning("[PuzzleBoardAligner] Frame, Camera, GridManager 중 하나가 비어 있습니다.");
                return;
            }

            int c = cols ?? gridColumns;
            int r = rows ?? gridRows;
            Rect worldRect = GetFrameWorldRect();
            gridManager.SetBoundsToFitRect(worldRect, c, r);
            Debug.Log($"[PuzzleBoardAligner] 그리드 정렬 완료: {c}x{r} → 월드 Rect {worldRect}");
        }

        private Rect GetFrameWorldRect()
        {
            Vector3[] corners = new Vector3[4];
            puzzleBoardFrame.GetWorldCorners(corners);

            // Overlay Canvas: GetWorldCorners는 캔버스 스케일이 적용된 좌표. 화면 픽셀로 변환
            Camera cam = targetCamera;
            Canvas canvas = puzzleBoardFrame.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // Overlay: WorldToScreenPoint에 null 전달 시 화면 픽셀 반환
                Vector2 screenMin = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
                Vector2 screenMax = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
                Vector3 worldMin = cam.ScreenToWorldPoint(new Vector3(screenMin.x, screenMin.y, cameraDistance));
                Vector3 worldMax = cam.ScreenToWorldPoint(new Vector3(screenMax.x, screenMax.y, cameraDistance));
                return Rect.MinMaxRect(worldMin.x, worldMin.y, worldMax.x, worldMax.y);
            }
            else
            {
                // Screen Space - Camera / World Space: corners가 이미 월드에 가까움
                Vector2 min = corners[0];
                Vector2 max = corners[2];
                return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            }
        }
    }
}
