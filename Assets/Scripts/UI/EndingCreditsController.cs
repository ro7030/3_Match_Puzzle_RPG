using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Match3Puzzle.UI
{
    /// <summary>
    /// 엔딩 크레딧: 등록된 이미지들을 순서대로 페이드인(투명→불투명)해서 보여줍니다.
    /// 마지막 이미지까지 모두 표시되면 OnCreditsComplete 이벤트가 발생합니다.
    /// </summary>
    public class EndingCreditsController : MonoBehaviour
    {
        [System.Serializable]
        public class CreditEntry
        {
            [Tooltip("표시할 이미지")]
            public Image image;
            [Tooltip("이전 이미지 페이드인 완료 후 이 이미지가 시작하기까지 대기 시간 (초)")]
            public float delayBefore = 0.5f;
            [Tooltip("페이드인에 걸리는 시간 (초)")]
            public float fadeDuration = 1.5f;
        }

        [Header("크레딧 이미지 목록 (위에서부터 순서대로 페이드인)")]
        [SerializeField] private CreditEntry[] entries;

        [Header("설정")]
        [Tooltip("씬 시작 시 자동 재생")]
        [SerializeField] private bool playOnStart = true;
        [Tooltip("재생 시작 전 대기 시간 (초)")]
        [SerializeField] private float initialDelay = 0.5f;

        [Header("완료 이벤트")]
        [Tooltip("모든 이미지가 표시된 후 호출됩니다. 씬 전환 등을 연결하세요.")]
        [SerializeField] private UnityEvent onCreditsComplete;

        [Header("완료 후 입력")]
        [Tooltip("크레딧이 모두 끝난 뒤 좌클릭 시 이동할 씬 이름")]
        [SerializeField] private string titleSceneName = "TitleScene";

        private bool _isCreditsCompleted;

        private void Start()
        {
            _isCreditsCompleted = false;

            // 모든 이미지를 투명(alpha=0)으로 초기화
            if (entries == null) return;
            foreach (var entry in entries)
            {
                if (entry.image == null) continue;
                SetAlpha(entry.image, 0f);
            }

            if (playOnStart)
                StartCoroutine(PlayCredits());
        }

        private void Update()
        {
            if (!_isCreditsCompleted) return;
            if (string.IsNullOrWhiteSpace(titleSceneName)) return;

            if (Input.GetMouseButtonDown(0))
                SceneManager.LoadScene(titleSceneName);
        }

        /// <summary>외부(버튼 등)에서 크레딧 재생을 시작할 때 호출합니다.</summary>
        public void Play()
        {
            StopAllCoroutines();

            if (entries != null)
                foreach (var entry in entries)
                    if (entry.image != null) SetAlpha(entry.image, 0f);

            StartCoroutine(PlayCredits());
        }

        private IEnumerator PlayCredits()
        {
            if (initialDelay > 0f)
                yield return new WaitForSeconds(initialDelay);

            if (entries == null) yield break;

            foreach (var entry in entries)
            {
                if (entry.image == null) continue;

                // 이전 이미지 완료 후 대기
                if (entry.delayBefore > 0f)
                    yield return new WaitForSeconds(entry.delayBefore);

                // 페이드인
                yield return StartCoroutine(FadeIn(entry.image, entry.fadeDuration));
            }

            // 모든 이미지 표시 완료
            _isCreditsCompleted = true;
            onCreditsComplete?.Invoke();
        }

        private IEnumerator FadeIn(Image image, float duration)
        {
            float elapsed = 0f;
            SetAlpha(image, 0f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetAlpha(image, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            SetAlpha(image, 1f);
        }

        private void SetAlpha(Image image, float alpha)
        {
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }
    }
}
