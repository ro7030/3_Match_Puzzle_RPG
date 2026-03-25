using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using Match3Puzzle.Stage;
using Match3Puzzle.Level;
using Match3Puzzle.UI.Battle;
using Match3Puzzle.Core;
using Story;

namespace Match3Puzzle.Stage
{
    /// <summary>
    /// 배틀 씬 로드 시, 선택된 스테이지에 맞게 배경·보스·턴·몬스터 체력 등을 적용.
    /// 배틀 씬에 하나만 두고, BattleStageHolder.CurrentStageIndex 또는 StageDatabase 참조로 설정.
    /// </summary>
    public class BattleSceneConfigApplier : MonoBehaviour
    {
        [Header("페이즈2 컷씬")]
        [SerializeField] private string storySceneName = "StoryScene";

        [SerializeField] private StageDatabase stageDatabase;
        [Header("배경")]
        [SerializeField] private Image backgroundImage;
        [Header("몬스터")]
        [SerializeField] private Image monsterImage;
        [SerializeField] private MonsterHealthUI monsterHealthUI;
        [Header("턴/난이도 (선택)")]
        [SerializeField] private LevelManager levelManager;
        [Header("파티 체력 (선택)")]
        [SerializeField] private PartyHealthUI partyHealthUI;

        private StageData _stageData;
        private bool _phase2Triggered;

        private void Start()
        {
            BattlePhaseRuntime.ResetForNewBattle();
            BattleClearStatsRuntime.ResetForNewBattle();
            _phase2Triggered = false;
            ApplyStageConfig();
            DisableNonInteractiveUIRaycasts();

            // 배틀씬 재진입/다음 스테이지 진입 시에도 보드/점수/상태 초기화가
            // 항상 보장되도록 여기서 한 번 더 강제 시작한다.
            // (BattleSceneStarter 실행 누락, DontDestroyOnLoad 경합 대비)
            StartCoroutine(EnsureBattleStartedNextFrame());

            if (monsterHealthUI != null)
                monsterHealthUI.OnHPChanged += HandleMonsterHpChanged;

            if (partyHealthUI != null)
                partyHealthUI.OnCharacterHpChanged += HandlePartyCharacterHpChanged;

            // 시작 시점(파티 체력 깎기 등)에도 페이즈2 발동 조건을 한 번 체크
            CheckPhase2TriggerAtStart();
        }

        private IEnumerator EnsureBattleStartedNextFrame()
        {
            // 씬 로드/오브젝트 활성 순서 차이로 참조가 비어있는 프레임을 피하기 위해 1프레임 지연
            yield return null;

            var gm = GameManager.Instance != null
                ? GameManager.Instance
                : FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.StartGame();

                // 다음 스테이지 진입 시 턴 값이 이전 전투에서 남지 않도록
                // StageData 기준 턴 설정을 한 번 더 강제 적용
                if (levelManager == null)
                    levelManager = FindFirstObjectByType<LevelManager>();
                if (_stageData == null && stageDatabase != null)
                    _stageData = stageDatabase.GetStage(BattleStageHolder.CurrentStageIndex);
                if (levelManager != null && _stageData != null)
                    levelManager.SetBattleConfig(_stageData.maxTurns);
            }
            else
            {
                Debug.LogWarning("[BattleConfig] GameManager를 찾지 못해 StartGame을 호출하지 못했습니다.");
            }
        }

        /// <summary>
        /// 배틀 보드 입력이 UI 장식 요소(Text/Image)에 가로채이지 않도록,
        /// 비상호작용 그래픽의 raycastTarget을 비활성화한다.
        /// </summary>
        private void DisableNonInteractiveUIRaycasts()
        {
            int changed = 0;
            var graphics = FindObjectsByType<Graphic>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < graphics.Length; i++)
            {
                var g = graphics[i];
                if (g == null || !g.raycastTarget) continue;

                // 타일 이미지는 반드시 입력을 받아야 하므로 유지
                if (g.GetComponent<Match3Puzzle.Board.Tile>() != null) continue;

                // 클리어 패널 계층은 UIManager에서 별도 입력 제어를 하므로 제외
                if (IsUnderClearPanelHierarchy(g.transform)) continue;

                // 버튼/슬라이더/입력필드 등 Selectable 계열 UI는 유지
                if (g.GetComponentInParent<Selectable>() != null) continue;

                // 상호작용 없는 텍스트/이미지는 레이캐스트 비활성화
                if (g is Image || g is TMP_Text)
                {
                    g.raycastTarget = false;
                    changed++;
                }
            }

            Debug.Log($"[BattleConfig] 비상호작용 UI raycast 비활성화: {changed}개");
        }

        private static bool IsUnderClearPanelHierarchy(Transform t)
        {
            while (t != null)
            {
                string n = t.name;
                if (!string.IsNullOrEmpty(n) &&
                    (n.Contains("LevelCompletePanel") || n.Contains("ClearPanel")))
                    return true;
                t = t.parent;
            }
            return false;
        }

        private void OnDestroy()
        {
            if (monsterHealthUI != null)
                monsterHealthUI.OnHPChanged -= HandleMonsterHpChanged;

            if (partyHealthUI != null)
                partyHealthUI.OnCharacterHpChanged -= HandlePartyCharacterHpChanged;
        }

        private void ApplyStageConfig()
        {
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelManager>();

            if (stageDatabase == null)
                stageDatabase = Resources.Load<StageDatabase>("StageDatabase");

            int index = BattleStageHolder.CurrentStageIndex;
            StageData data = stageDatabase != null ? stageDatabase.GetStage(index) : null;
            _stageData = data;

            // StageData 직렬화 불일치로 인한 MissingReferenceException이 발생할 수 있어,
            // 배경 스프라이트 접근은 로그에서 제외합니다.
            Debug.Log($"[BattleConfig] 인덱스:{index} / DB:{stageDatabase != null} / Data:{data?.stageName ?? "NULL"} / Boss:{data?.bossSprite?.name ?? "NULL"}");

            if (data == null)
            {
                Debug.LogWarning($"[BattleConfig] StageData가 null입니다. StageDatabase의 Element {index}를 확인하세요.");
                return;
            }

            try
            {
                if (backgroundImage != null && data.backgroundSprite != null)
                    backgroundImage.sprite = data.backgroundSprite;
            }
            catch (MissingReferenceException ex)
            {
                Debug.LogWarning($"[BattleConfig] backgroundSprite 접근 실패(직렬화 불일치 가능): {ex.Message}");
            }

            try
            {
                if (monsterImage != null && data.bossSprite != null)
                    monsterImage.sprite = data.bossSprite;
            }
            catch (MissingReferenceException ex)
            {
                Debug.LogWarning($"[BattleConfig] bossSprite 접근 실패(직렬화 불일치 가능): {ex.Message}");
            }

            try
            {
                if (monsterHealthUI != null)
                    monsterHealthUI.SetHP(data.monsterMaxHp, data.monsterMaxHp);
            }
            catch (MissingReferenceException ex)
            {
                Debug.LogWarning($"[BattleConfig] monsterMaxHp 접근 실패(직렬화 불일치 가능): {ex.Message}");
            }

            try
            {
                if (levelManager != null)
                    levelManager.SetBattleConfig(data.maxTurns);
            }
            catch (MissingReferenceException ex)
            {
                Debug.LogWarning($"[BattleConfig] maxTurns 접근 실패(직렬화 불일치 가능): {ex.Message}");
            }

            try
            {
                if (partyHealthUI == null)
                    partyHealthUI = FindFirstObjectByType<PartyHealthUI>();
                if (partyHealthUI != null && data.partyStartHpDeduction > 0)
                    partyHealthUI.ApplyBattleStartingHpDeduction(data.partyStartHpDeduction);
            }
            catch (MissingReferenceException ex)
            {
                Debug.LogWarning($"[BattleConfig] partyStartHpDeduction 적용 실패(직렬화 불일치 가능): {ex.Message}");
            }
        }

        private void HandleMonsterHpChanged(int currentHp, int maxHp)
        {
            if (_stageData == null) return;
            if (!_stageData.hasPhase2) return;
            if (_phase2Triggered) return;
            if (_stageData.phase2TriggerTarget != Phase2TriggerTargetType.MonsterHp) return;

            // HP=0인 순간은 HandleMonsterDied(클리어) 쪽이 우선 처리되도록 방어
            if (currentHp <= 0) return;

            if (currentHp <= _stageData.phase2TriggerHp)
            {
                _phase2Triggered = true;
                StartCoroutine(TriggerPhase2CutsceneCoroutine());
            }
        }

        private void HandlePartyCharacterHpChanged(int characterIndex, int currentHp, int maxHp)
        {
            if (_stageData == null) return;
            if (!_stageData.hasPhase2) return;
            if (_phase2Triggered) return;
            if (_stageData.phase2TriggerTarget != Phase2TriggerTargetType.PartyAnyMemberHp) return;

            // HP=0인 순간은 패배 처리(파티원 down) 쪽이 우선 처리되도록 방어
            if (currentHp <= 0) return;

            if (currentHp <= _stageData.phase2TriggerHp)
            {
                _phase2Triggered = true;
                StartCoroutine(TriggerPhase2CutsceneCoroutine());
            }
        }

        private void CheckPhase2TriggerAtStart()
        {
            if (_stageData == null) return;
            if (!_stageData.hasPhase2) return;
            if (_phase2Triggered) return;
            if (_stageData.phase2TriggerHp <= 0) return; // 기본 0은 트리거되지 않는 용도

            if (_stageData.phase2TriggerTarget == Phase2TriggerTargetType.MonsterHp)
            {
                if (monsterHealthUI == null) return;
                int currentHp = monsterHealthUI.CurrentHp;
                if (currentHp > 0 && currentHp <= _stageData.phase2TriggerHp)
                {
                    _phase2Triggered = true;
                    StartCoroutine(TriggerPhase2CutsceneCoroutine());
                }
            }
            else if (_stageData.phase2TriggerTarget == Phase2TriggerTargetType.PartyAnyMemberHp)
            {
                if (partyHealthUI == null) return;
                for (int i = 0; i < partyHealthUI.CharacterCount; i++)
                {
                    int hp = partyHealthUI.GetCurrentHP(i);
                    if (hp > 0 && hp <= _stageData.phase2TriggerHp)
                    {
                        _phase2Triggered = true;
                        StartCoroutine(TriggerPhase2CutsceneCoroutine());
                        return;
                    }
                }
            }
        }

        private IEnumerator TriggerPhase2CutsceneCoroutine()
        {
            // 1) 입력/턴 진행을 멈춤
            BattlePhaseRuntime.SetCutsceneActive(true);
            GameManager.Instance?.ChangeState(GameState.Paused);

            // 2) 컷씬이 설정된 경우 StoryScene을 잠깐 Additive 로드
            if (_stageData != null && _stageData.phase2Cutscene != null && !string.IsNullOrEmpty(_stageData.phase2Cutscene.cutsceneId))
            {
                CutsceneContext.SetNextForBattle(_stageData.phase2Cutscene.cutsceneId, OnPhase2CutsceneFinished);

                // 이미 로드돼있는 경우를 방지(비정상 케이스)
                var existing = SceneManager.GetSceneByName(storySceneName);
                if (existing.IsValid() && existing.isLoaded)
                    yield return SceneManager.UnloadSceneAsync(storySceneName);

                var op = SceneManager.LoadSceneAsync(storySceneName, LoadSceneMode.Additive);
                while (op != null && !op.isDone)
                    yield return null;
            }
            else
            {
                // 컷씬이 없으면 바로 페이즈2 적용
                OnPhase2CutsceneFinished();
            }
        }

        private void OnPhase2CutsceneFinished()
        {
            StartCoroutine(FinishPhase2Coroutine());
        }

        private IEnumerator FinishPhase2Coroutine()
        {
            // 페이즈2 상태 활성화(데미지/공격/저항 계산 로직이 이 값 기반으로 동작)
            BattlePhaseRuntime.SetPhase(BattlePhaseRuntime.Phase2);

            // 시각/HP 최대치 갱신(현재 HP는 유지)
            ApplyPhase2MonsterVisuals();

            // StoryScene 언로드
            var storyScene = SceneManager.GetSceneByName(storySceneName);
            if (storyScene.IsValid() && storyScene.isLoaded)
            {
                var op = SceneManager.UnloadSceneAsync(storySceneName);
                while (op != null && !op.isDone)
                    yield return null;
            }

            // 전투 재개
            BattlePhaseRuntime.SetCutsceneActive(false);
            GameManager.Instance?.ChangeState(GameState.Playing);
        }

        private void ApplyPhase2MonsterVisuals()
        {
            if (_stageData == null || monsterHealthUI == null) return;

            if (monsterImage != null && _stageData.bossSpritePhase2 != null)
                monsterImage.sprite = _stageData.bossSpritePhase2;

            // 현재 HP는 유지하고, 최대치만 페이즈2 설정으로 변경
            if (_stageData.monsterMaxHpPhase2 > 0)
            {
                int currentHp = monsterHealthUI.CurrentHp;
                monsterHealthUI.SetHP(currentHp, _stageData.monsterMaxHpPhase2);
            }
        }
    }
}
