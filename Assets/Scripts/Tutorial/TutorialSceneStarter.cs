using System.Collections;
using UnityEngine;
using Match3Puzzle.Core;
using Match3Puzzle.Board;
using Match3Puzzle.Stage;
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
        [Header("스테이지 데이터 (선택 - 몬스터 HP 등 적용 시 연결)")]
        [Tooltip("StageData_Tu 에셋을 여기에 드래그. 비워두면 MonsterHealthUI를 건드리지 않음.")]
        [SerializeField] private StageData stageData;

        [Header("배틀 UI 참조 (stageData 연결 시 자동 탐색)")]
        [SerializeField] private MonsterHealthUI monsterHealthUI;
        [SerializeField] private PartyHealthUI partyHealthUI;
        [SerializeField] private MonsterAttackController monsterAttackController;

        private void Start()
        {
            var board = FindFirstObjectByType<GameBoard>();
            if (board == null)
            {
                Debug.LogError("[TutorialSceneStarter] GameBoard를 찾을 수 없습니다.");
                return;
            }

            board.InitializeTutorialBoard();

            if (stageData != null)
                ApplyStageData();

            // GameManager.Start()가 ChangeState(Menu)를 호출하므로,
            // 한 프레임 뒤에 Playing으로 전환해 Menu 상태 덮어쓰기를 방지.
            StartCoroutine(SetPlayingNextFrame());
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

        private IEnumerator SetPlayingNextFrame()
        {
            yield return null; // 한 프레임 대기

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Playing);
                Debug.Log("[TutorialSceneStarter] 상태를 Playing으로 전환");
            }
            else
            {
                Debug.LogError("[TutorialSceneStarter] GameManager.Instance가 null입니다.");
            }
        }
    }
}
