using UnityEngine;

namespace Story
{
    /// <summary>
    /// 캐릭터 데이터. 여러 대화/컷신에서 ID로 재사용.
    /// </summary>
    [CreateAssetMenu(fileName = "Chr_", menuName = "Story/Data Character", order = 1)]
    public class DataCharacter : ScriptableObject
    {
        public int chrID;
        public string displayName = "";
        public Sprite chrImage;
        [Tooltip("스킬 씬 등에서 미선택 시 표시할 흑백 초상화. 비어 있으면 컬러 스프라이트만 사용합니다.")]
        public Sprite chrImageGrayscale;
    }
}
