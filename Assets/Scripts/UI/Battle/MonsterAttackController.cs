using System.Collections.Generic;
using UnityEngine;
using Match3Puzzle.Stage;
using Match3Puzzle.Core;
using Match3Puzzle.Level;
using Match3Puzzle.UI;

namespace Match3Puzzle.UI.Battle
{
    /// <summary>
    /// 몬스터 공격 처리. 4턴마다(설정 가능) 공격, 턴 소진 시 보스 체력으로 승/패 판정.
    /// </summary>
    public class MonsterAttackController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private PartyHealthUI partyHealthUI;
        [SerializeField] private MonsterHealthUI monsterHealthUI;
        [SerializeField] private StageDatabase stageDatabase;
        [SerializeField] private bool debugAttackFlow = true;

        private StageData currentStageData;
        private bool _hasFailedByPartyDown;

        private void Awake()
        {
            if (levelManager == null)
                levelManager = FindFirstObjectByType<Level.LevelManager>(FindObjectsInactive.Include);
            if (partyHealthUI == null)
                partyHealthUI = FindFirstObjectByType<PartyHealthUI>();
            if (monsterHealthUI == null)
                monsterHealthUI = FindFirstObjectByType<MonsterHealthUI>();
            if (stageDatabase == null)
                stageDatabase = Resources.Load<StageDatabase>("StageDatabase");
        }

        private void OnEnable()
        {
            if (levelManager != null)
            {
                levelManager.OnTurnChanged += HandleTurnChanged;
                levelManager.OnTurnsExhausted += HandleTurnsExhausted;
            }
            if (partyHealthUI != null)
                partyHealthUI.OnCharacterHpZero += HandleAnyPartyMemberDown;
            if (monsterHealthUI != null)
                monsterHealthUI.OnDied += HandleMonsterDied;
        }

        private void OnDisable()
        {
            if (levelManager != null)
            {
                levelManager.OnTurnChanged -= HandleTurnChanged;
                levelManager.OnTurnsExhausted -= HandleTurnsExhausted;
            }
            if (partyHealthUI != null)
                partyHealthUI.OnCharacterHpZero -= HandleAnyPartyMemberDown;
            if (monsterHealthUI != null)
                monsterHealthUI.OnDied -= HandleMonsterDied;
        }

        private void Start()
        {
            _hasFailedByPartyDown = false;
            int index = BattleStageHolder.CurrentStageIndex;
            currentStageData = stageDatabase != null ? stageDatabase.GetStage(index) : null;

            // OnEnable 시점에 monsterHealthUI가 null이었을 경우를 대비해 Start에서 재구독
            if (monsterHealthUI != null)
            {
                monsterHealthUI.OnDied -= HandleMonsterDied; // 중복 방지
                monsterHealthUI.OnDied += HandleMonsterDied;
            }

            Debug.Log($"[MonsterAttack] DB:{stageDatabase != null} / Data:{currentStageData?.stageName ?? "NULL"} / " +
                      $"LevelMgr:{levelManager != null} / PartyUI:{partyHealthUI != null} / MonsterUI:{monsterHealthUI != null}");

            TryApplyStartAttackRule();
        }

        private void HandleAnyPartyMemberDown(int characterIndex)
        {
            if (_hasFailedByPartyDown) return;
            _hasFailedByPartyDown = true;

            if (debugAttackFlow)
                Debug.Log($"[MonsterAttack] Party member down -> fail (idx={characterIndex})");

            GameManager.Instance?.ChangeState(GameState.GameOver);

            var uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
                uiManager.ShowPartyDefeatPanel();
        }

        private void HandleTurnChanged(int turn)
        {
            if (currentStageData == null || partyHealthUI == null) return;
            // 전투 시작 직후 UI 동기화용 turn=0 이벤트는 공격 트리거에서 제외
            if (turn <= 0) return;
            int interval = BattlePhaseRuntime.ActivePhaseIndex == BattlePhaseRuntime.Phase2
                ? currentStageData.attackIntervalTurnsPhase2
                : currentStageData.attackIntervalTurns;

            if (debugAttackFlow)
                Debug.Log($"[MonsterAttack] TurnChanged turn={turn}, interval={interval}, phase={BattlePhaseRuntime.ActivePhaseIndex}");

            if (interval <= 0) return;
            if (turn % interval != 0) return;

            if (debugAttackFlow)
                Debug.Log($"[MonsterAttack] Attack Triggered at turn={turn} (interval={interval})");
            ApplyMonsterAttack();
        }

        /// <summary>몬스터 HP가 0이 되는 순간 즉시 클리어 처리</summary>
        private void HandleMonsterDied()
        {
            Debug.Log("[MonsterAttack] HandleMonsterDied 호출 → LevelComplete");
            GameManager.Instance?.LevelComplete();
        }

        /// <summary>턴 소진 시: 몬스터가 살아있으면 패배, 이미 죽었으면 무시 (HandleMonsterDied에서 처리됨)</summary>
        private void HandleTurnsExhausted()
        {
            if (monsterHealthUI == null) return;

            if (monsterHealthUI.CurrentHp <= 0) return; // 이미 HandleMonsterDied에서 처리됨
            GameManager.Instance?.GameOver();
        }

        private void ApplyMonsterAttack()
        {
            int damage = BattlePhaseRuntime.ActivePhaseIndex == BattlePhaseRuntime.Phase2
                ? currentStageData.attackDamagePhase2
                : currentStageData.attackDamage;
            var targets = GetAttackTargets();

            if (debugAttackFlow)
                Debug.Log($"[MonsterAttack] ApplyAttack damage={damage}, targets={string.Join(",", targets)}");

            foreach (int idx in targets)
            {
                if (partyHealthUI.IsAlive(idx))
                    partyHealthUI.TakeDamage(idx, damage);
            }
        }

        private List<int> GetAttackTargets()
        {
            var alive = new List<int>();
            for (int i = 0; i < partyHealthUI.CharacterCount; i++)
            {
                if (partyHealthUI.IsAlive(i))
                    alive.Add(i);
            }

            if (alive.Count == 0) return alive;

            var targetType = BattlePhaseRuntime.ActivePhaseIndex == BattlePhaseRuntime.Phase2
                ? currentStageData.attackTargetTypePhase2
                : currentStageData.attackTargetType;

            switch (targetType)
            {
                case MonsterAttackTargetType.All:
                    return new List<int>(alive);
                case MonsterAttackTargetType.SingleRandom:
                    return new List<int> { alive[Random.Range(0, alive.Count)] };
                case MonsterAttackTargetType.Random2:
                    return PickRandom(alive, 2);
                case MonsterAttackTargetType.Random3:
                    return PickRandom(alive, 3);
                default:
                    return new List<int>(alive);
            }
        }

        private List<int> PickRandom(List<int> source, int count)
        {
            var shuffled = new List<int>(source);
            for (int i = 0; i < shuffled.Count; i++)
            {
                int j = Random.Range(i, shuffled.Count);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }
            count = Mathf.Min(count, shuffled.Count);
            return shuffled.GetRange(0, count);
        }

        private void TryApplyStartAttackRule()
        {
            if (currentStageData == null || partyHealthUI == null) return;

            switch (currentStageData.startAttackMode)
            {
                case BattleStartAttackMode.Disabled:
                    if (debugAttackFlow)
                        Debug.Log("[MonsterAttack] StartAttackMode=Disabled");
                    break;

                case BattleStartAttackMode.AttackOnly:
                    if (debugAttackFlow)
                        Debug.Log("[MonsterAttack] StartAttackMode=AttackOnly -> 즉시 공격");
                    ApplyMonsterAttack();
                    break;

                case BattleStartAttackMode.AttackAndCountTurn:
                    if (debugAttackFlow)
                        Debug.Log("[MonsterAttack] StartAttackMode=AttackAndCountTurn -> 즉시 공격 + 턴+1");
                    ApplyMonsterAttack();
                    if (levelManager != null)
                        levelManager.IncrementMoves();
                    break;
            }
        }
    }
}
