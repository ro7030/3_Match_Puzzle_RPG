using System.Collections.Generic;
using UnityEngine;

namespace Story
{
    /// <summary>
    /// 캐릭터 ID → DataCharacter 조회. Inspector에 리스트 등록 후 런타임에 Dictionary로 변환.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Story/Character Database", order = 2)]
    public class CharacterDatabase : ScriptableObject
    {
        [SerializeField] private List<DataCharacter> characters = new List<DataCharacter>();

        private Dictionary<int, DataCharacter> _dict;

        /// <summary>
        /// ID로 캐릭터 조회. -1이면 null (나레이터 등).
        /// </summary>
        public DataCharacter Get(int id)
        {
            if (id < 0) return null;
            EnsureDict();
            return _dict.TryGetValue(id, out var c) ? c : null;
        }

        private void EnsureDict()
        {
            if (_dict != null) return;
            _dict = new Dictionary<int, DataCharacter>();
            foreach (var c in characters)
            {
                if (c != null && !_dict.ContainsKey(c.chrID))
                    _dict.Add(c.chrID, c);
            }
        }

        /// <summary>
        /// 리스트 반환 (에디터/확장용)
        /// </summary>
        public IReadOnlyList<DataCharacter> GetList() => characters;

#if UNITY_EDITOR
        private void OnValidate() => _dict = null;
#endif
    }
}
