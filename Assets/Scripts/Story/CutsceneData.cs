using System;
using System.Collections.Generic;
using UnityEngine;

namespace Story
{
    /// <summary>
    /// 컷신 대사 데이터. 캐릭터는 CharacterDatabase ID로 참조.
    /// Resources/Story/Cutscenes/ 경로에 두면 CutsceneContext로 불러올 수 있음.
    /// </summary>
    [CreateAssetMenu(fileName = "Cutscene_New", menuName = "Story/Cutscene Data", order = 0)]
    public class CutsceneData : ScriptableObject
    {
        [Tooltip("컷신 ID. CutsceneContext.SetNext(id) 시 사용. 파일명과 일치 권장")]
        public string cutsceneId;

        [Tooltip("끝난 뒤 이동할 씬 이름. 비워두면 GameScene")]
        public string nextSceneName = "GameScene";

        [Tooltip("true: 끝나면 nextSceneName으로 이동. false: onComplete 콜백만 호출")]
        public bool isPrologueMode = true;

        [Tooltip("true: 완료/스킵 시 세이브 삭제 (New Game 프롤로그용). false: 저장 유지")]
        public bool clearSaveOnComplete = true;

        public List<DialogueLineData> dialogueLines = new List<DialogueLineData>();

        [Serializable]
        public class DialogueLineData
        {
            [Tooltip("CharacterDatabase ID. -1이면 나레이터(캐릭터 없음)")]
            public int characterId = -1;
            [Tooltip("characterId가 -1일 때 사용, 또는 표시 이름 오버라이드")]
            public string speakerName = "";
            [TextArea(2, 6)]
            public string text = "";
            [Tooltip("(선택) 이 대사에서 배경 전환")]
            public Sprite backgroundSprite;
        }

        /// <summary>
        /// CharacterDatabase로 해석하여 StoryDialogueController용 리스트로 변환
        /// </summary>
        public List<StoryDialogueController.DialogueLine> ToDialogueLines(CharacterDatabase characterDb)
        {
            var list = new List<StoryDialogueController.DialogueLine>();
            foreach (var d in dialogueLines)
            {
                var chr = characterDb != null && d.characterId >= 0 ? characterDb.Get(d.characterId) : null;
                string name = !string.IsNullOrEmpty(d.speakerName) ? d.speakerName : (chr?.displayName ?? "");
                Sprite portrait = chr != null ? chr.chrImage : null;

                list.Add(new StoryDialogueController.DialogueLine
                {
                    speakerName = name ?? "",
                    text = d.text ?? "",
                    characterSprite = portrait,
                    backgroundSprite = d.backgroundSprite
                });
            }
            return list;
        }

        /// <summary>
        /// characterIds[i], texts[i] 병렬 리스트에서 DialogueLine 생성 (DataDialogue 연동용)
        /// </summary>
        public static List<StoryDialogueController.DialogueLine> FromParallelLists(
            List<int> characterIds, List<string> texts, CharacterDatabase characterDb)
        {
            var list = new List<StoryDialogueController.DialogueLine>();
            if (characterIds == null || texts == null) return list;

            int count = Mathf.Min(characterIds.Count, texts.Count);
            for (int i = 0; i < count; i++)
            {
                var chr = characterDb != null && characterIds[i] >= 0 ? characterDb.Get(characterIds[i]) : null;
                list.Add(new StoryDialogueController.DialogueLine
                {
                    speakerName = chr?.displayName ?? "",
                    text = texts[i] ?? "",
                    characterSprite = chr?.chrImage,
                    backgroundSprite = null
                });
            }
            return list;
        }
    }
}
