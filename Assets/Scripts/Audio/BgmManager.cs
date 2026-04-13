using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Match3Puzzle.Stage;
using Story;

namespace Match3Puzzle.Audio
{
    /// <summary>
    /// 배경음(BGM) 전역 매니저. DontDestroyOnLoad 싱글톤.
    ///
    /// 동작 요약
    /// ─────────────────────────────────────────────────────
    /// 1. 씬이 로드되면 <b>SO 데이터</b>를 먼저 확인 (Battle→StageData, Story→CutsceneData).
    /// 2. SO에 bgmClip이 없으면 Inspector의 <b>씬 매핑 테이블</b>에서 기본 클립을 가져옴.
    /// 3. 결정된 클립이 현재 재생 중인 클립과 동일하면 <b>끊지 않고 이어서 재생</b>.
    /// 4. 같은 <b>bgmGroup</b>에 속한 씬끼리는 클립이 다르더라도 현재 곡을 유지.
    /// 5. 곡이 바뀌면 <b>크로스페이드</b>로 부드럽게 전환.
    /// ─────────────────────────────────────────────────────
    /// </summary>
    public class BgmManager : MonoBehaviour
    {
        public static BgmManager Instance { get; private set; }

        [Serializable]
        public struct SceneBgmEntry
        {
            [Tooltip("씬 이름 (Build Settings 이름과 동일)")]
            public string sceneName;
            [Tooltip("이 씬의 기본 BGM 클립")]
            public AudioClip bgmClip;
            [Tooltip("같은 그룹 이름을 공유하는 씬끼리는 BGM이 끊기지 않고 유지됩니다 (예: Lobby)")]
            public string bgmGroup;
        }

        [Header("씬별 기본 BGM 매핑")]
        [SerializeField] private SceneBgmEntry[] sceneEntries = new SceneBgmEntry[]
        {
            new SceneBgmEntry { sceneName = "TitleScene",     bgmGroup = "Title" },
            new SceneBgmEntry { sceneName = "PrologueScene",  bgmGroup = "Title" },
            new SceneBgmEntry { sceneName = "TutorialScene",  bgmGroup = "" },
            new SceneBgmEntry { sceneName = "KingdomScene",   bgmGroup = "Lobby" },
            new SceneBgmEntry { sceneName = "MapScene",       bgmGroup = "Lobby" },
            new SceneBgmEntry { sceneName = "InventoryScene", bgmGroup = "Lobby" },
            new SceneBgmEntry { sceneName = "SkillScene",     bgmGroup = "Lobby" },
            new SceneBgmEntry { sceneName = "StatusScene",    bgmGroup = "Lobby" },
            new SceneBgmEntry { sceneName = "StoryScene",     bgmGroup = "" },
            new SceneBgmEntry { sceneName = "BattleScene",    bgmGroup = "" },
        };

        [Header("오디오")]
        [SerializeField] private AudioSource audioSource;
        [Range(0f, 1f)]
        [SerializeField] private float bgmVolume = 0.5f;
        [Tooltip("BGM 전환 시 페이드 총 시간(초). 절반은 페이드아웃, 절반은 페이드인.")]
        [SerializeField] private float fadeDuration = 1.0f;

        private string _currentGroup = "";
        private AudioClip _currentClip;
        private Coroutine _fadeCoroutine;

        // ──────────────────────────────────────────────────
        // 라이프사이클
        // ──────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = bgmVolume;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this) Instance = null;
        }

        // ──────────────────────────────────────────────────
        // 씬 로드 이벤트
        // ──────────────────────────────────────────────────

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive) return;

            string sceneName = scene.name;

            AudioClip soClip = ResolveSoClip(sceneName);
            SceneBgmEntry? entry = FindEntry(sceneName);

            string newGroup = entry.HasValue ? (entry.Value.bgmGroup ?? "") : "";
            AudioClip resolvedClip = soClip != null ? soClip : (entry.HasValue ? entry.Value.bgmClip : null);

            // SO에서도, 씬 매핑에서도 클립을 얻지 못한 경우 → 현재 BGM 유지
            if (resolvedClip == null)
            {
                if (!string.IsNullOrEmpty(newGroup))
                    _currentGroup = newGroup;
                return;
            }

            // 같은 그룹끼리는 곡이 달라도 현재 BGM 유지 (로비 씬 간 이동 시 끊김 방지)
            if (!string.IsNullOrEmpty(newGroup)
                && !string.IsNullOrEmpty(_currentGroup)
                && string.Equals(newGroup, _currentGroup, StringComparison.Ordinal)
                && audioSource.isPlaying)
            {
                return;
            }

            // 같은 클립이면 재시작하지 않음
            if (resolvedClip == _currentClip && audioSource.isPlaying)
            {
                _currentGroup = newGroup;
                return;
            }

            // 새 곡 크로스페이드
            TransitionTo(resolvedClip, newGroup);
        }

        // ──────────────────────────────────────────────────
        // SO 데이터 기반 BGM 탐색
        // ──────────────────────────────────────────────────

        private static AudioClip ResolveSoClip(string sceneName)
        {
            if (string.Equals(sceneName, "BattleScene", StringComparison.Ordinal))
                return ResolveBattleBgm();

            if (string.Equals(sceneName, "StoryScene", StringComparison.Ordinal))
                return ResolveStoryBgm();

            return null;
        }

        private static AudioClip ResolveBattleBgm()
        {
            var db = Resources.Load<StageDatabase>("StageDatabase");
            if (db == null) return null;
            var data = db.GetStage(BattleStageHolder.CurrentStageIndex);
            return data != null ? data.bgmClip : null;
        }

        private static AudioClip ResolveStoryBgm()
        {
            if (!CutsceneContext.HasNext) return null;
            var so = Resources.Load<CutsceneData>(
                $"{CutsceneContext.ScriptableObjectPathPrefix}{CutsceneContext.CutsceneId}");
            return so != null ? so.bgmClip : null;
        }

        // ──────────────────────────────────────────────────
        // 씬 매핑 조회
        // ──────────────────────────────────────────────────

        private SceneBgmEntry? FindEntry(string sceneName)
        {
            if (sceneEntries == null) return null;
            for (int i = 0; i < sceneEntries.Length; i++)
            {
                if (string.Equals(sceneEntries[i].sceneName, sceneName, StringComparison.Ordinal))
                    return sceneEntries[i];
            }
            return null;
        }

        // ──────────────────────────────────────────────────
        // 공개 API
        // ──────────────────────────────────────────────────

        /// <summary>즉시 또는 크로스페이드로 BGM을 교체합니다.</summary>
        public void PlayBgm(AudioClip clip, string group = "")
        {
            if (clip == null) { StopBgm(); return; }
            if (clip == _currentClip && audioSource.isPlaying) { _currentGroup = group; return; }
            TransitionTo(clip, group);
        }

        /// <summary>BGM을 페이드아웃 후 정지합니다.</summary>
        public void StopBgm()
        {
            _currentClip = null;
            _currentGroup = "";
            HaltFade();
            _fadeCoroutine = StartCoroutine(FadeOutRoutine());
        }

        /// <summary>BGM 볼륨을 런타임에 변경합니다 (설정 UI 연동용).</summary>
        public void SetVolume(float volume01)
        {
            bgmVolume = Mathf.Clamp01(volume01);
            if (_fadeCoroutine == null)
                audioSource.volume = bgmVolume;
        }

        public float Volume => bgmVolume;
        public bool IsPlaying => audioSource != null && audioSource.isPlaying;
        public AudioClip CurrentClip => _currentClip;

        // ──────────────────────────────────────────────────
        // 전환 / 페이드
        // ──────────────────────────────────────────────────

        private void TransitionTo(AudioClip clip, string group)
        {
            _currentClip = clip;
            _currentGroup = group;
            HaltFade();
            _fadeCoroutine = StartCoroutine(CrossFadeRoutine(clip));
        }

        private void HaltFade()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }

        private IEnumerator CrossFadeRoutine(AudioClip newClip)
        {
            float half = Mathf.Max(fadeDuration * 0.5f, 0.01f);

            if (audioSource.isPlaying && audioSource.volume > 0.001f)
            {
                float start = audioSource.volume;
                for (float t = 0f; t < half; t += Time.unscaledDeltaTime)
                {
                    audioSource.volume = Mathf.Lerp(start, 0f, t / half);
                    yield return null;
                }
            }

            audioSource.Stop();
            audioSource.clip = newClip;
            audioSource.volume = 0f;
            audioSource.Play();

            for (float t = 0f; t < half; t += Time.unscaledDeltaTime)
            {
                audioSource.volume = Mathf.Lerp(0f, bgmVolume, t / half);
                yield return null;
            }

            audioSource.volume = bgmVolume;
            _fadeCoroutine = null;
        }

        private IEnumerator FadeOutRoutine()
        {
            float start = audioSource.volume;
            float duration = Mathf.Max(fadeDuration, 0.01f);

            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                audioSource.volume = Mathf.Lerp(start, 0f, t / duration);
                yield return null;
            }

            audioSource.Stop();
            audioSource.clip = null;
            audioSource.volume = bgmVolume;
            _fadeCoroutine = null;
        }
    }
}
