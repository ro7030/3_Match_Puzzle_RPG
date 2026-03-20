using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
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

        private StageData _stageData;
        private bool _phase2Triggered;

        private void Start()
        {
            BattlePhaseRuntime.ResetForNewBattle();
            BattleClearStatsRuntime.ResetForNewBattle();
            _phase2Triggered = false;
            ApplyStageConfig();
            if (monsterHealthUI != null)
                monsterHealthUI.OnHPChanged += HandleMonsterHpChanged;
        }

        private void OnDestroy()
        {
            if (monsterHealthUI != null)
                monsterHealthUI.OnHPChanged -= HandleMonsterHpChanged;
        }

        private void ApplyStageConfig()
        {
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
        }

        private void HandleMonsterHpChanged(int currentHp, int maxHp)
        {
            if (_stageData == null) return;
            if (!_stageData.hasPhase2) return;
            if (_phase2Triggered) return;

            // HP=0인 순간은 HandleMonsterDied(클리어) 쪽이 우선 처리되도록 방어
            if (currentHp <= 0) return;

            if (currentHp <= _stageData.phase2TriggerHp)
            {
                _phase2Triggered = true;
                StartCoroutine(TriggerPhase2CutsceneCoroutine());
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
