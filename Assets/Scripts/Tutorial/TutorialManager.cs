using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Match3Puzzle.UI.Battle;
using Match3Puzzle.Clear;
using Story;

namespace Match3Puzzle.Tutorial
{
    public enum TutorialStep
    {
        WaitingForMatch,
        WaitingForSkill,
        Completed
    }

    /// <summary>
    /// 튜토리얼 흐름 제어.
    /// 매칭 데미지는 <see cref="matchDamage"/>만 적용 (씬에 이 컴포넌트가 있으면 MatchEffectHandler가 스탯 데미지를 넣지 않음).
    /// TutorialManager 빈 오브젝트에 붙임.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [Header("UI 연결")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private SkillSlotUI skillSlotUI;

        [Header("몬스터 체력 (없으면 자동 탐색)")]
        [SerializeField] private MonsterHealthUI monsterHealthUI;

        [Header("튜토리얼 고정 데미지")]
        [Tooltip("3-match 성공 시 몬스터에게 입히는 고정 데미지")]
        [SerializeField] private int matchDamage = 20;
        [Tooltip("스킬 사용 시 몬스터에게 입히는 고정 데미지")]
        [SerializeField] private int skillDamage = 80;

        [Header("이벤트 연결 (직접 연결 권장)")]
        [Tooltip("GameManager 오브젝트의 TileClearer. 비워두면 자동 탐색.")]
        [SerializeField] private TileClearer tileClearer;

        [Header("대사 텍스트")]
        [SerializeField] private string step1Message = "타일을 클릭하여 상하좌우로 움직여 3개를 맞춰 보세요.";
        [SerializeField] private string step2Message = "하단에 있는 스킬 아이콘을 클릭해 보세요.";
        [SerializeField] private string step3Message = "튜토리얼이 끝났습니다.";

        [Header("씬 전환 설정")]
        [Tooltip("완료 메시지 표시 후 자동 씬 전환까지 대기 시간(초).")]
        [SerializeField] private float autoProceedDelay = 3f;
        [Tooltip("튜토리얼 완료 후 재생할 컷신 ID. 비워두면 nextSceneName으로 직행.")]
        [SerializeField] private string postCutsceneId = "PostTutorial";
        [Tooltip("컷신이 없을 때 이동할 씬 이름. 컷신이 있으면 CutsceneData의 Next Scene Name 사용.")]
        [SerializeField] private string nextSceneName = "MapScene";

        public TutorialStep CurrentStep { get; private set; } = TutorialStep.WaitingForMatch;

        private void Start()
        {
            // 자동 탐색
            if (monsterHealthUI == null)
                monsterHealthUI = FindFirstObjectByType<MonsterHealthUI>();

            if (tileClearer == null)
                tileClearer = FindFirstObjectByType<TileClearer>();

            if (skillSlotUI == null)
                skillSlotUI = FindFirstObjectByType<SkillSlotUI>();

            // 연결 확인 로그
            Debug.Log($"[TutorialManager] monsterHealthUI: {(monsterHealthUI != null ? "OK" : "NULL")}");
            Debug.Log($"[TutorialManager] tileClearer: {(tileClearer != null ? "OK" : "NULL")}");
            Debug.Log($"[TutorialManager] skillSlotUI: {(skillSlotUI != null ? skillSlotUI.gameObject.name : "NULL")}");

            if (tileClearer != null)
                tileClearer.OnMatchCleared += OnMatchOccurred;
            else
                Debug.LogError("[TutorialManager] TileClearer를 찾지 못했습니다.");

            if (skillSlotUI != null)
                skillSlotUI.OnSkillUsed += OnSkillUsed;
            else
                Debug.LogError("[TutorialManager] SkillSlotUI를 찾지 못했습니다.");

            SetSkillInteractable(false);
            ShowDialogue(step1Message);
        }

        private void OnDestroy()
        {
            if (tileClearer != null)
                tileClearer.OnMatchCleared -= OnMatchOccurred;

            if (skillSlotUI != null)
                skillSlotUI.OnSkillUsed -= OnSkillUsed;
        }

        /// <summary>
        /// TileClearer.OnMatchCleared 이벤트로 호출. 고정 데미지를 몬스터에게 적용.
        /// </summary>
        public void OnMatchOccurred()
        {
            Debug.Log($"[TutorialManager] OnMatchOccurred 호출. CurrentStep={CurrentStep}");

            if (CurrentStep != TutorialStep.WaitingForMatch) return;

            if (monsterHealthUI != null)
                monsterHealthUI.TakeDamage(matchDamage);

            CurrentStep = TutorialStep.WaitingForSkill;
            ShowDialogue(step2Message);
            SetSkillInteractable(true);

            Debug.Log($"[TutorialManager] 매칭 성공 → 몬스터 {matchDamage} 데미지 → 스킬 버튼 활성화");
        }

        private void OnSkillUsed()
        {
            Debug.Log($"[TutorialManager] OnSkillUsed 호출. CurrentStep={CurrentStep}");

            if (CurrentStep != TutorialStep.WaitingForSkill) return;

            if (monsterHealthUI != null)
                monsterHealthUI.TakeDamage(skillDamage);

            CurrentStep = TutorialStep.Completed;
            ShowDialogue(step3Message);
            SetSkillInteractable(false);

            Debug.Log($"[TutorialManager] 스킬 사용 → 몬스터 {skillDamage} 데미지 → 튜토리얼 완료");

            StartCoroutine(AutoProceed());
        }

        private IEnumerator AutoProceed()
        {
            yield return new WaitForSeconds(autoProceedDelay);

            if (!string.IsNullOrEmpty(postCutsceneId))
            {
                CutsceneContext.SetNext(postCutsceneId);
                SceneManager.LoadScene("StoryScene");
            }
            else if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }

        private void ShowDialogue(string message)
        {
            if (dialogueText != null)
                dialogueText.text = message;
        }

        private void SetSkillInteractable(bool interactable)
        {
            Debug.Log($"[TutorialManager] SetSkillInteractable({interactable}), skillSlotUI={skillSlotUI != null}");
            if (skillSlotUI != null)
                skillSlotUI.SetInteractable(interactable);
        }
    }
}
