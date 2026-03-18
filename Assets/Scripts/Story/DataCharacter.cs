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
    }
}
