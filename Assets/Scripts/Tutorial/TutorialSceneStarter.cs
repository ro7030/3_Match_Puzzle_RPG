using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Match3Puzzle.Core;
using Match3Puzzle.Board;
using Match3Puzzle.Stage;
using Match3Puzzle.Swap;
using Match3Puzzle.UI.Battle;

namespace Match3Puzzle.Tutorial
{
    /// <summary>
    /// 튜토리얼 씬 초기화. BattleSceneStarter 대신 GameManager 오브젝트에 붙임.
    /// InitializeTutorialBoard()를 호출해 Inspector 이미지를 그대로 유지하면서 모든 타일을 칼 타입으로 설정.
    /// GameManager.Start()가 Menu 상태로 초기화하므로, 한 프레임 뒤에 Playing 상태로 전환.
    /// StageData를 연결하면 몬스터 HP 등 배틀 설정을 Inspector에서 직접 적용 가능.
    /// </summary>
    public class TutorialSceneStarter : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool debugStateLog = true;

        [Header("스테이지 데이터 (선택 - 몬스터 HP 등 적용 시 연결)")]
        [Tooltip("StageData_Tu 에셋을 여기에 드래그. 비워두면 MonsterHealthUI를 건드리지 않음.")]
        [SerializeField] private StageData stageData;

        [Header("배틀 UI 참조 (stageData 연결 시 자동 탐색)")]
        [SerializeField] private MonsterHealthUI monsterHealthUI;
        [SerializeField] private PartyHealthUI partyHealthUI;
        [SerializeField] private MonsterAttackController monsterAttackController;

        [Header("Tile (1) 시각 (TileSlots 하위)")]
        [Tooltip("TileSlots 아래에서 찾을 자식 이름. 기본: Tile (1)")]
        [SerializeField] private string tutorialTileOneName = "Tile (1)";
        [Tooltip("스왑 후 Tile (1) 슬롯에 다시 깔 스프라이트(예: Chroma Key_0). 비우면 시작 시 Tile (1)에 있던 스프라이트로 복구합니다.")]
        [SerializeField] private Sprite tileOneSpriteAfterMove;

        private GameBoard _cachedGameBoard;
        private Image _tutorialTileOneImage;
        private Sprite _tutorialTileOneInspectorSprite;

        private void OnEnable()
        {
            TileSwapper.SwapCompleted += OnTutorialSwapCompleted;
        }

        private void OnDisable()
        {
            TileSwapper.SwapCompleted -= OnTutorialSwapCompleted;
        }

        private void Start()
        {
            // 배틀 씬에서 남아있는 static 컷씬 플래그가 튜토리얼 스왑 흐름을 막을 수 있어 초기화합니다.
            // (TileSwapper가 매칭 후 Playing으로 되돌리는 조건에 IsBattleCutsceneActive를 사용)
            BattlePhaseRuntime.ResetForNewBattle();

            var board = FindFirstObjectByType<GameBoard>();
            if (board == null)
            {
                Debug.LogError("[TutorialSceneStarter] GameBoard를 찾을 수 없습니다.");
                return;
            }

            board.InitializeTutorialBoard();
            _cachedGameBoard = board;
            ApplyTutorialTileOneSprite(board);
            DisableNonInteractiveUIRaycasts();
            LowerTutorialDragThreshold();

            if (stageData != null)
                ApplyStageData();

            if (debugStateLog)
            {
                var t00 = board.GetTile(0, 0);
                Debug.Log($"[TutorialSceneStarter] Board init: size={board.Width}x{board.Height} isInitialized={board.IsInitialized} tile(0,0) empty={(t00 == null ? true : t00.IsEmpty)} type={(t00 == null ? -999 : t00.TileType)}");
            }

            // GameManager.Start()가 ChangeState(Menu)를 호출하므로,
            // 한 프레임 뒤에 Playing으로 전환해 Menu 상태 덮어쓰기를 방지.
            StartCoroutine(EnsurePlayingForAShortTime());
        }

        /// <summary>
        /// 튜토리얼에서 대사 텍스트/장식 UI가 타일 입력을 가로채지 않도록 raycastTarget을 비활성화합니다.
        /// 버튼/슬라이더 등 Selectable 계열은 유지합니다.
        /// </summary>
        private void DisableNonInteractiveUIRaycasts()
        {
            int changed = 0;
            var graphics = FindObjectsByType<Graphic>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < graphics.Length; i++)
            {
                var g = graphics[i];
                if (g == null || !g.raycastTarget) continue;

                // 타일 이미지는 드래그 입력 대상이므로 유지
                if (g.GetComponent<Tile>() != null) continue;

                // 버튼/슬라이더/입력필드 등 실제 상호작용 UI는 유지
                if (g.GetComponentInParent<Selectable>() != null) continue;

                // 그 외 텍스트/이미지/그래픽은 보드 입력 통과
                g.raycastTarget = false;
                changed++;
            }

            if (debugStateLog)
                Debug.Log($"[TutorialSceneStarter] 비상호작용 UI raycast 비활성화: {changed}개");
        }

        /// <summary>
        /// TileSlots / Tile (1) Image에 Tile Sprites[0] 적용. 스왑 후에는 Chroma Key 등 원래 슬롯 이미지로 복구.
        /// </summary>
        private void ApplyTutorialTileOneSprite(GameBoard board)
        {
            _tutorialTileOneImage = null;
            _tutorialTileOneInspectorSprite = null;

            var root = board.TileSlotsRoot;
            if (root == null)
            {
                if (debugStateLog)
                    Debug.LogWarning("[TutorialSceneStarter] TileSlotsRoot를 찾을 수 없어 Tile (1) 스프라이트를 건너뜁니다.");
                return;
            }

            var tileTr = FindDeepChildByName(root, tutorialTileOneName);
            if (tileTr == null)
            {
                if (debugStateLog)
                    Debug.LogWarning($"[TutorialSceneStarter] '{tutorialTileOneName}' 오브젝트를 TileSlots 하위에서 찾지 못했습니다.");
                return;
            }

            _tutorialTileOneImage = tileTr.GetComponent<Image>();
            if (_tutorialTileOneImage == null)
            {
                if (debugStateLog)
                    Debug.LogWarning($"[TutorialSceneStarter] '{tutorialTileOneName}'에 Image가 없습니다.");
                return;
            }

            _tutorialTileOneInspectorSprite = _tutorialTileOneImage.sprite;
            var sprite0 = board.GetTileSprite(0, false);
            if (sprite0 != null)
                _tutorialTileOneImage.sprite = sprite0;
            else if (debugStateLog)
                Debug.LogWarning("[TutorialSceneStarter] GameBoard Tile Sprites[0]이 비어 있어 Tile (1) 초기 스프라이트를 바꾸지 않습니다.");
        }

        private void OnTutorialSwapCompleted(Tile a, Tile b)
        {
            var board = _cachedGameBoard != null ? _cachedGameBoard : FindFirstObjectByType<GameBoard>();
            if (board == null)
                return;

            RestoreTileOneSlotToChroma(board);
            StartCoroutine(RestoreTileOneSlotToChromaAfterLayout(board));
        }

        /// <summary>
        /// 스왑으로 칼 스프라이트가 다른 칸으로 옮겨간 뒤에도, 'Tile (1)' 슬롯의 Image는 Chroma Key(원래 배경)로 되돌립니다.
        /// </summary>
        private void RestoreTileOneSlotToChroma(GameBoard board)
        {
            var chroma = tileOneSpriteAfterMove != null ? tileOneSpriteAfterMove : _tutorialTileOneInspectorSprite;
            if (chroma == null)
                return;

            var root = board.TileSlotsRoot;
            if (root == null)
                return;

            var tileTr = FindDeepChildByName(root, tutorialTileOneName);
            if (tileTr == null)
                return;

            var img = tileTr.GetComponent<Image>();
            if (img == null)
                return;

            img.sprite = chroma;
            img.enabled = true;
            _tutorialTileOneImage = img;
        }

        private IEnumerator RestoreTileOneSlotToChromaAfterLayout(GameBoard board)
        {
            yield return null;
            yield return null;
            RestoreTileOneSlotToChroma(board);
        }

        private static Transform FindDeepChildByName(Transform root, string objectName)
        {
            if (root == null || string.IsNullOrEmpty(objectName))
                return null;
            if (root.name == objectName)
                return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindDeepChildByName(root.GetChild(i), objectName);
                if (found != null)
                    return found;
            }
            return null;
        }

        private void LowerTutorialDragThreshold()
        {
            var input = FindFirstObjectByType<Match3Puzzle.Swap.InputHandler>();
            if (input != null)
            {
                input.SetDragThreshold(10f);
                if (debugStateLog)
                    Debug.Log("[TutorialSceneStarter] InputHandler.dragThreshold=10 적용");
            }
        }

        /// <summary>
        /// StageData를 씬의 MonsterHealthUI 등에 직접 적용.
        /// </summary>
        private void ApplyStageData()
        {
            if (monsterHealthUI == null)
                monsterHealthUI = FindFirstObjectByType<MonsterHealthUI>();

            if (monsterHealthUI != null)
                monsterHealthUI.SetHP(stageData.monsterMaxHp, stageData.monsterMaxHp);

            if (partyHealthUI == null)
                partyHealthUI = FindFirstObjectByType<PartyHealthUI>();
            if (partyHealthUI != null && stageData.partyStartHpDeduction > 0)
                partyHealthUI.ApplyBattleStartingHpDeduction(stageData.partyStartHpDeduction);
        }

        private IEnumerator EnsurePlayingForAShortTime()
        {
            const int maxFrames = 4;
            for (int i = 0; i < maxFrames; i++)
            {
                yield return null; // 프레임 대기

                if (GameManager.Instance == null)
                {
                    if (debugStateLog)
                        Debug.LogError("[TutorialSceneStarter] GameManager.Instance가 null입니다.");
                    continue;
                }

                if (debugStateLog)
                    Debug.Log($"[TutorialSceneStarter] EnsurePlaying frame={i} currentState={GameManager.Instance.CurrentState} timeScale={Time.timeScale}");

                GameManager.Instance.ChangeState(GameState.Playing);

                // WaitForSeconds 계열 애니/코루틴이 timeScale=0에서 멈추는 경우를 즉시 방어
                if (Time.timeScale == 0f)
                    Time.timeScale = 1f;

                if (GameManager.Instance.CurrentState == GameState.Playing && Time.timeScale > 0f)
                {
                    if (debugStateLog)
                        Debug.Log("[TutorialSceneStarter] 상태가 Playing으로 안정화되었습니다.");
                    break;
                }
            }
        }
    }
}
