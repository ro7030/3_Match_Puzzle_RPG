using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Match3Puzzle.UI.Battle
{
    /// <summary>
    /// 배틀 씬에서 "이전 단계(스테이지 선택)"로 돌아가는 버튼.
    /// Target Scene Name에 맵/스테이지 선택 씬 이름을 넣으면 해당 씬으로 로드.
    /// </summary>
    public class BattleBackButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [Tooltip("스테이지 선택 씬 이름 (예: MapScene, StageSelectScene)")]
        [SerializeField] private string targetSceneName = "MapScene";

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(OnBackClicked);
        }

        private void OnBackClicked()
        {
            if (string.IsNullOrEmpty(targetSceneName))
                return;
            SceneManager.LoadScene(targetSceneName);
        }

        /// <summary>
        /// 돌아갈 씬 이름 설정 (코드에서 변경 시)
        /// </summary>
        public void SetTargetScene(string sceneName)
        {
            targetSceneName = sceneName;
        }
    }
}
