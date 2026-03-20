using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MainMenu
{
    /// <summary>
    /// 옵션 팝업. 메인 메뉴 또는 인게임에서 동일한 패널을 띄워 사용.
    /// 볼륨, 전체화면 등 설정을 PlayerPrefs에 저장.
    /// </summary>
    public class OptionsPanel : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;

        [Header("Volume")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeText;
        [SerializeField] private TextMeshProUGUI bgmVolumeText;
        [SerializeField] private TextMeshProUGUI sfxVolumeText;

        [Header("Other")]
        [Tooltip("Fullscreen 모드 드롭다운 (TextMeshPro TMP_Dropdown). (0=창모드, 1=전체화면)")]
        [SerializeField] private TMP_Dropdown fullscreenDropdown;
        [SerializeField] private Toggle fullscreenToggle;

        [Header("Debug (선택)")]
        [Tooltip("전체/창모드 적용 결과를 화면에 표시(비워두면 콘솔 로그만 남김)")]
        [SerializeField] private TextMeshProUGUI fullscreenDebugText;
        [Tooltip("ApplyFullscreenMode 호출 시에도 디버그 텍스트/로그를 남김")]
        [SerializeField] private bool verboseDebug = true;

        [SerializeField] private Button closeButton;

        private const string PrefMasterVolume = "Options_MasterVolume";
        private const string PrefBgmVolume = "Options_BgmVolume";
        private const string PrefSfxVolume = "Options_SfxVolume";
        private const string PrefFullscreen = "Options_Fullscreen";

        // PlayerPrefs 값: 0=Windowed, 1=FullscreenWindow
        private const int FullscreenWindowedValue = 0;
        private const int FullscreenFullscreenValue = 1;

        // 드롭다운 값(인덱스): 0=Windowed, 1=FullscreenWindow 가정
        private const int DropdownWindowedIndex = 0;
        private const int DropdownFullscreenIndex = 1;

        private void Awake()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);

            LoadAndApplySavedOptions();

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }
            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }
            if (fullscreenDropdown != null)
            {
                fullscreenDropdown.onValueChanged.AddListener(OnFullscreenDropdownChanged);
            }
            else if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
        }

        /// <summary>
        /// 옵션 패널 열기 (메인 메뉴 또는 인게임에서 호출)
        /// </summary>
        public void Open()
        {
            LoadAndApplySavedOptions();
            if (panelRoot != null)
                panelRoot.SetActive(true);
        }

        public void Close()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        /// <summary>
        /// 저장된 옵션 로드 후 UI와 실제 설정에 반영
        /// </summary>
        private void LoadAndApplySavedOptions()
        {
            float master = PlayerPrefs.GetFloat(PrefMasterVolume, 1f);
            float bgm = PlayerPrefs.GetFloat(PrefBgmVolume, 1f);
            float sfx = PlayerPrefs.GetFloat(PrefSfxVolume, 1f);
            int fullscreen = PlayerPrefs.GetInt(PrefFullscreen, 1);

            if (masterVolumeSlider != null) masterVolumeSlider.value = master;
            if (bgmVolumeSlider != null) bgmVolumeSlider.value = bgm;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfx;
            if (fullscreenToggle != null) fullscreenToggle.isOn = fullscreen == FullscreenFullscreenValue;

            if (fullscreenDropdown != null)
            {
                // dropdown은 0=창모드, 1=전체화면 가정
                int dropdownValue = fullscreen == FullscreenFullscreenValue
                    ? DropdownFullscreenIndex
                    : DropdownWindowedIndex;
                fullscreenDropdown.SetValueWithoutNotify(dropdownValue);
            }

            ApplyMasterVolume(master);
            ApplyBgmVolume(bgm);
            ApplySfxVolume(sfx);
            ApplyFullscreenMode(fullscreen);

            UpdateVolumeLabels();
        }

        private void OnMasterVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(PrefMasterVolume, value);
            PlayerPrefs.Save();
            ApplyMasterVolume(value);
            UpdateVolumeLabels();
        }

        private void OnBgmVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(PrefBgmVolume, value);
            PlayerPrefs.Save();
            ApplyBgmVolume(value);
            UpdateVolumeLabels();
        }

        private void OnSfxVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(PrefSfxVolume, value);
            PlayerPrefs.Save();
            ApplySfxVolume(value);
            UpdateVolumeLabels();
        }

        private void OnFullscreenChanged(bool isOn)
        {
            int fullscreenValue = isOn ? FullscreenFullscreenValue : FullscreenWindowedValue;
            PlayerPrefs.SetInt(PrefFullscreen, fullscreenValue);
            PlayerPrefs.Save();
            ApplyFullscreenMode(fullscreenValue);
        }

        private void OnFullscreenDropdownChanged(int dropdownIndex)
        {
            // dropdownIndex: 0=Windowed, 1=Fullscreen
            int fullscreenValue = dropdownIndex == DropdownFullscreenIndex
                ? FullscreenFullscreenValue
                : FullscreenWindowedValue;

            PlayerPrefs.SetInt(PrefFullscreen, fullscreenValue);
            PlayerPrefs.Save();
            ApplyFullscreenMode(fullscreenValue);
        }

        private void ApplyFullscreenMode(int fullscreenValue)
        {
            var mode = fullscreenValue == FullscreenFullscreenValue
                ? FullScreenMode.FullScreenWindow
                : FullScreenMode.Windowed;

            if (fullscreenToggle != null)
                fullscreenToggle.isOn = fullscreenValue == FullscreenFullscreenValue;

#if UNITY_EDITOR
            // 에디터 Play 모드에서 즉시 반영이 꼬일 때가 있어 즉시 + 한 프레임 딜레이로 재적용
            Screen.fullScreenMode = mode;
            if (verboseDebug)
                Debug.Log($"[OptionsPanel] ApplyFullscreenMode requested={fullscreenValue} -> mode={mode} currentMode={Screen.fullScreenMode} fullScreen={Screen.fullScreen} Screen={Screen.width}x{Screen.height} currentRes={(Screen.currentResolution.width)}x{(Screen.currentResolution.height)}");
            EditorApplication.delayCall += () =>
            {
                if (Screen.fullScreenMode != mode)
                    Screen.fullScreenMode = mode;
            };
#else
            Screen.fullScreenMode = mode;
#endif

            if (fullscreenDebugText != null)
            {
                fullscreenDebugText.text = BuildFullscreenDebugText(mode, fullscreenValue);
            }
        }

        private string BuildFullscreenDebugText(FullScreenMode requestedMode, int requestedValue)
        {
            int prefValue = PlayerPrefs.GetInt(PrefFullscreen, -999);
            var currentMode = Screen.fullScreenMode;
            int curW = Screen.width;
            int curH = Screen.height;
            int resW = Screen.currentResolution.width;
            int resH = Screen.currentResolution.height;
            return
                $"[Fullscreen]\n" +
                $"Requested(Pref)={requestedValue} / PrefNow={prefValue}\n" +
                $"RequestedMode={requestedMode}\n" +
                $"CurrentMode={currentMode}\n" +
                $"Screen={curW}x{curH}\n" +
                $"CurrentRes={resW}x{resH}";
        }

        private void ApplyMasterVolume(float value)
        {
            AudioListener.volume = value;
        }

        private void ApplyBgmVolume(float value)
        {
            // BGM 전용 오디오 소스가 있으면 여기서 볼륨 설정
            // 예: BgmManager.Instance?.SetVolume(value);
        }

        private void ApplySfxVolume(float value)
        {
            // SFX 전용 설정이 있으면 여기서 적용
        }

        private void UpdateVolumeLabels()
        {
            if (masterVolumeText != null && masterVolumeSlider != null)
                masterVolumeText.text = Mathf.RoundToInt(masterVolumeSlider.value * 100f) + "%";
            if (bgmVolumeText != null && bgmVolumeSlider != null)
                bgmVolumeText.text = Mathf.RoundToInt(bgmVolumeSlider.value * 100f) + "%";
            if (sfxVolumeText != null && sfxVolumeSlider != null)
                sfxVolumeText.text = Mathf.RoundToInt(sfxVolumeSlider.value * 100f) + "%";
        }
    }
}
