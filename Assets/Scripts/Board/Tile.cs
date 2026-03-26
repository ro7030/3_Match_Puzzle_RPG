using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Match3Puzzle.Swap;

namespace Match3Puzzle.Board
{
    /// <summary>
    /// 10x10 슬롯 중 하나. UI Image 기반. 사용자가 미리 배치한 Image에 붙임.
    /// </summary>
    public class Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Tile Properties")]
        [SerializeField] private int tileType = -1;
        [SerializeField] private int x = -1;
        [SerializeField] private int y = -1;
        [SerializeField] private bool isSpecial = false;
        [SerializeField] private SpecialTileType specialType = SpecialTileType.None;
        [SerializeField] private bool isEnhanced = false;

        [Header("Visual")]
        [SerializeField] private Image image;

        private bool isSelected = false;
        private bool isClearing = false;
        private bool isMoving = false;
        private InputHandler _inputHandler;

        public int TileType => tileType;
        public int X => x;
        public int Y => y;
        public bool IsSpecial => isSpecial;
        public bool IsEnhanced => isEnhanced;
        public SpecialTileType SpecialType => specialType;
        public bool IsSelected => isSelected;
        public bool IsClearing => isClearing;
        public bool IsMoving => isMoving;
        /// <summary>슬롯이 비어있는지 (제거됨/스폰 대기)</summary>
        public bool IsEmpty => tileType < 0;
        /// <summary>Inspector에서 스프라이트가 미리 할당되어 있는지. 튜토리얼 보드 초기화에서 사용.</summary>
        public bool HasSprite => image != null && image.sprite != null;
        public RectTransform RectTransform => image != null ? image.rectTransform : GetComponent<RectTransform>();

        private void Awake()
        {
            if (image == null)
                image = GetComponent<Image>();
            if (image != null)
                image.raycastTarget = true;
            EnsureInputHandler();
        }

        private void EnsureInputHandler()
        {
            if (_inputHandler == null)
                _inputHandler = FindFirstObjectByType<InputHandler>();
        }

        /// <summary>
        /// 타일 초기화 (0~3 타입, 스프라이트 할당)
        /// </summary>
        public void Initialize(int type, int posX, int posY, Sprite sprite, bool enhanced = false)
        {
            tileType = type;
            x = posX;
            y = posY;
            isSpecial = false;
            specialType = SpecialTileType.None;
            isEnhanced = enhanced;

            if (image != null)
            {
                image.enabled = true;
                image.sprite = sprite;
            }

            isSelected = false;
            isClearing = false;
            isMoving = false;
        }

        /// <summary>
        /// 슬롯 비우기 (클리어 시)
        /// </summary>
        public void SetEmpty()
        {
            tileType = -1;
            if (image != null)
            {
                image.sprite = null;
                image.enabled = false;
            }
        }

        /// <summary>
        /// 슬롯 채우기 (스폰/중력 후)
        /// </summary>
        public void SetFilled(int type, Sprite sprite, bool enhanced = false)
        {
            tileType = type;
            isEnhanced = enhanced;
            if (image != null)
            {
                image.enabled = true;
                image.sprite = sprite;
            }
        }

        public void SetPosition(int posX, int posY)
        {
            x = posX;
            y = posY;
        }

        /// <summary>
        /// 스프라이트를 변경하지 않고 타일 타입만 설정. 튜토리얼에서 Inspector 이미지를 유지할 때 사용.
        /// Image가 비활성화되어 있을 경우 활성화하고, raycastTarget도 보장.
        /// </summary>
        public void SetTileTypeOnly(int type)
        {
            tileType = type;
            isSelected = false;
            isClearing = false;

            if (image != null)
            {
                image.enabled = true;       // SetEmpty로 비활성화됐을 경우 복원
                image.raycastTarget = true; // 드래그 입력 수신 보장
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
        }

        public void StartClearing()
        {
            isClearing = true;
        }

        public void StartMoving() { isMoving = true; }
        public void FinishMoving() { isMoving = false; }

        public void OnClearAnimationComplete()
        {
            SetEmpty();
        }

        public void SetSpecial(SpecialTileType type)
        {
            isSpecial = true;
            specialType = type;
        }

        /// <summary>
        /// 같은 종류로 취급: 0↔4(검), 1↔5(활), 2↔6(십자가), 3↔7(지팡이)
        /// </summary>
        public bool IsSameType(Tile other)
        {
            if (other == null || tileType < 0) return false;
            return GetBaseType(tileType) == GetBaseType(other.tileType);
        }

        /// <summary>0~7 → 0~3 (4→0, 5→1, 6→2, 7→3)</summary>
        public static int GetBaseType(int t)
        {
            return (t >= 0 && t <= 7) ? (t % 4) : t;
        }

        public bool IsAdjacent(Tile other)
        {
            if (other == null) return false;
            int dx = Mathf.Abs(x - other.x);
            int dy = Mathf.Abs(y - other.y);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            EnsureInputHandler();
            _inputHandler?.OnTilePointerDown(this, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            EnsureInputHandler();
            _inputHandler?.OnTilePointerUp(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            EnsureInputHandler();
            _inputHandler?.OnTileDrag(this, eventData);
        }
    }

    public enum SpecialTileType
    {
        None,
        Bomb,
        Rainbow,
        Rocket
    }
}
