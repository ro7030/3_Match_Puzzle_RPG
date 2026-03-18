using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Story
{
    /// <summary>
    /// 이 컴포넌트가 붙은 UI 요소 위에서 마우스 휠을 굴리면
    /// 지정한 ScrollRect로 스크롤 이벤트를 전달합니다.
    /// ScriptPanel의 Background, 닫기 버튼 등 ScrollRect를 가리는
    /// 모든 UI 오브젝트에 부착하세요.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ScrollRectForwarder : MonoBehaviour, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private ScrollRect targetScrollRect;

        public void OnScroll(PointerEventData eventData)
        {
            if (targetScrollRect != null)
                targetScrollRect.OnScroll(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData) { }
        public void OnPointerExit(PointerEventData eventData) { }
    }
}
