using MainMenu;
using UnityEngine;
using UnityEngine.UI;

namespace Match3Puzzle.UI
{
    /// <summary>
    /// 씬마다 옵션 버튼에 붙여서 OptionsPanel(Open)을 호출한다.
    /// 사용자는 버튼만 인스펙터로 연결하면 된다(패널은 타입으로 자동 탐색).
    /// </summary>
    public class OptionPanelButtonOpener : MonoBehaviour
    {
        [SerializeField] private Button optionButton;

        private OptionsPanel _optionsPanel;

        private void Awake()
        {
            if (optionButton == null)
                optionButton = GetComponent<Button>();

            CacheOptionsPanel();

            if (optionButton != null)
            {
                optionButton.onClick.RemoveListener(OpenOptions);
                optionButton.onClick.AddListener(OpenOptions);
            }
        }

        private void CacheOptionsPanel()
        {
            // OptionsPanel은 씬에서 비활성 상태일 수 있으므로 includeInactive=true
            var panels = Object.FindObjectsOfType<OptionsPanel>(true);
            _optionsPanel = panels != null && panels.Length > 0 ? panels[0] : null;
        }

        private void OpenOptions()
        {
            if (_optionsPanel == null)
                CacheOptionsPanel();

            if (_optionsPanel != null)
                _optionsPanel.Open();
            else
                Debug.LogWarning("[OptionPanelButtonOpener] OptionsPanel을 찾지 못했습니다. (OptionsPanel을 씬에 배치했는지 확인)");
        }
    }
}

