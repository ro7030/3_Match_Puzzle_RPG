using UnityEngine;
using UnityEngine.EventSystems;

namespace Match3Puzzle.Skill
{
    /// <summary>
    /// 초상화 Image에 붙여 캐릭터 영역의 포인터 이벤트를 CharacterSkillSlotUI로 전달합니다.
    /// </summary>
    public class SkillSlotPointerProxy : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private CharacterSkillSlotUI _owner;

        public void Init(CharacterSkillSlotUI owner)
        {
            _owner = owner;
        }

        public void OnPointerEnter(PointerEventData eventData) => _owner?.NotifyPointerEnter();

        public void OnPointerExit(PointerEventData eventData) => _owner?.NotifyPointerExit();

        public void OnPointerClick(PointerEventData eventData) => _owner?.NotifyPointerClick();
    }
}
