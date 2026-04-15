using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Match3Puzzle.UI.Battle
{
    /// <summary>
    /// 데미지/힐 숫자를 0~9 스프라이트 이미지로 조합해 표시하고,
    /// 위로 떠오르며 서서히 사라지는 애니메이션을 수행합니다.
    /// DamageNumberSpawner.SpawnNumber()에서 동적으로 생성됩니다.
    /// </summary>
    public class DamageNumberDisplay : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;

        private void Awake()
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// 숫자 스프라이트를 조합해 데미지/힐 숫자를 빌드하고 애니메이션을 시작합니다.
        /// </summary>
        /// <param name="value">표시할 절대값 (Spawner에서 부호 처리 후 전달)</param>
        /// <param name="digitSprites">0~9 순서의 스프라이트 배열 (길이 10)</param>
        /// <param name="prefixSprite">앞에 붙일 기호 스프라이트 (데미지=-，힐=+，없으면 null)</param>
        /// <param name="isHeal">true면 초록색, false면 흰색으로 표시</param>
        /// <param name="digitWidth">각 숫자 이미지의 너비 (px)</param>
        /// <param name="digitHeight">각 숫자 이미지의 높이 (px)</param>
        /// <param name="floatDistance">위로 떠오를 거리 (px)</param>
        /// <param name="duration">애니메이션 지속 시간 (초)</param>
        public void Setup(
            int value,
            Sprite[] digitSprites,
            Sprite prefixSprite,
            bool isHeal,
            float digitWidth,
            float digitHeight,
            float floatDistance,
            float duration)
        {
            // 표시할 스프라이트 목록 구성
            var spriteList = new List<Sprite>();

            // 앞에 붙일 기호 (+/-). null이면 생략
            if (prefixSprite != null)
                spriteList.Add(prefixSprite);

            string numStr = Mathf.Abs(value).ToString(); // 부호는 prefixSprite로 처리하므로 절대값만 사용
            foreach (char c in numStr)
            {
                int d = c - '0';
                if (d >= 0 && d <= 9 && digitSprites != null && d < digitSprites.Length && digitSprites[d] != null)
                    spriteList.Add(digitSprites[d]);
            }

            if (spriteList.Count == 0)
            {
                Destroy(gameObject);
                return;
            }

            // 숫자 색상: 힐=초록, 데미지=흰색
            Color col = isHeal ? new Color(0.3f, 1f, 0.3f) : Color.white;

            // 전체 너비를 계산해 중앙 정렬
            float totalWidth = spriteList.Count * digitWidth;
            float startX = -totalWidth * 0.5f + digitWidth * 0.5f;

            for (int i = 0; i < spriteList.Count; i++)
            {
                var go = new GameObject("Digit_" + i, typeof(RectTransform));
                go.transform.SetParent(transform, false);

                var img = go.AddComponent<Image>();
                img.sprite = spriteList[i];
                img.preserveAspect = true;
                img.color = col;
                img.raycastTarget = false;

                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(digitWidth, digitHeight);
                rt.anchoredPosition = new Vector2(startX + i * digitWidth, 0f);
            }

            StartCoroutine(Animate(floatDistance, duration));
        }

        private IEnumerator Animate(float floatDistance, float duration)
        {
            float elapsed = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                // 위로 선형 이동, 후반부에 페이드 아웃
                rectTransform.anchoredPosition = startPos + Vector2.up * (floatDistance * t);
                canvasGroup.alpha = 1f - Mathf.Max(0f, (t - 0.4f) / 0.6f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
