using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button closeButton;

        private const string PrefMasterVolume = "Options_MasterVolume";
        private const string PrefBgmVolume = "Options_BgmVolume";
        private const string PrefSfxVolume = "Options_SfxVolume";
        private const string PrefFullscreen = "Options_Fullscreen";

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
            if (fullscreenToggle != null)
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
            if (fullscreenToggle != null) fullscreenToggle.isOn = fullscreen == 1;

            ApplyMasterVolume(master);
            ApplyBgmVolume(bgm);
            ApplySfxVolume(sfx);
            Screen.fullScreenMode = fullscreen == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

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
            PlayerPrefs.SetInt(PrefFullscreen, isOn ? 1 : 0);
            PlayerPrefs.Save();
            Screen.fullScreenMode = isOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
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
