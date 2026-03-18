using UnityEngine;
using UnityEngine.SceneManagement;

namespace Story
{
    /// <summary>
    /// StoryScene 로드 시 어떤 컷신을 재생할지 전달하는 정적 컨텍스트.
    /// 맵/전투 씬에서 StoryScene 로드 전에 SetNext() 호출 후 씬 전환.
    /// </summary>
    public static class CutsceneContext
    {
        public static string CutsceneId { get; private set; }

        /// <summary>
        /// Resources 하위 경로 prefix: "Story/Cutscenes/"
        /// </summary>
        public const string ScriptableObjectPathPrefix = "Story/Cutscenes/";

        /// <summary>
        /// 다음 재생할 컷신을 ScriptableObject로 설정. StoryScene 로드 전 호출.
        /// </summary>
        /// <param name="cutsceneId">Resources/Story/Cutscenes/{cutsceneId} 에서 로드</param>
        public static void SetNext(string cutsceneId)
        {
            CutsceneId = cutsceneId ?? "";
        }

        /// <summary>
        /// Context 초기화 (StoryScene 진입 후 내부적으로 호출)
        /// </summary>
        public static void Clear()
        {
            CutsceneId = "";
        }

        /// <summary>
        /// Context에 설정된 컷신이 있는지
        /// </summary>
        public static bool HasNext => !string.IsNullOrEmpty(CutsceneId);
    }
}
