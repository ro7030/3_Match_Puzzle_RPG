using System.Collections;
using System.Collections.Generic;
using Match3Puzzle.Skill;
using UnityEngine;

namespace Match3Puzzle.UI.Battle
{
    /// <summary>
    /// 스킬 사용 시 이펙트 오브젝트를 잠시 켜고 Animator 1회 재생 + 효과음 재생 후 다시 끕니다.
    /// 배틀 씬에 빈 오브젝트로 두고 Inspector에서 타입별 VFX 루트를 연결합니다.
    /// </summary>
    public class BattleSkillFxPlayer : MonoBehaviour
    {
        [System.Serializable]
        public struct SkillBattleFxBinding
        {
            public SkillEffectType effectType;
            [Tooltip("배치해 둔 스킬 이펙트 루트. 평소 비활성.")]
            public GameObject vfxRoot;
            [Tooltip("비우면 Resources/Sound/Effect_Sound 타입별 기본 클립을 로드합니다.")]
            public AudioClip sfxClip;
            [Tooltip("Animator.Play에 넣을 상태 이름. 비우면 프로젝트 기본 이름(활=Wind_Effect 등).")]
            public string animatorStateOverride;
        }

        [SerializeField] private SkillBattleFxBinding[] bindings =
        {
            new SkillBattleFxBinding { effectType = SkillEffectType.Sword },
            new SkillBattleFxBinding { effectType = SkillEffectType.Bow },
            new SkillBattleFxBinding { effectType = SkillEffectType.Wand },
            new SkillBattleFxBinding { effectType = SkillEffectType.Heal }
        };
        [SerializeField] private AudioSource audioSource;

        private readonly Dictionary<GameObject, Coroutine> _runningByRoot = new Dictionary<GameObject, Coroutine>();

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        /// <summary>스킬 버튼이 실제로 사용 처리될 때 호출합니다.</summary>
        public void Play(SkillEffectType effectType)
        {
            var bindingNullable = FindBinding(effectType);
            if (!bindingNullable.HasValue)
                return;

            SkillBattleFxBinding binding = bindingNullable.Value;
            HaltRoot(binding.vfxRoot);
            _runningByRoot[binding.vfxRoot] = StartCoroutine(PlayRoutine(binding, effectType));
        }

        private SkillBattleFxBinding? FindBinding(SkillEffectType effectType)
        {
            if (bindings == null) return null;
            for (int i = 0; i < bindings.Length; i++)
            {
                var b = bindings[i];
                if (b.vfxRoot != null && b.effectType == effectType)
                    return b;
            }

            return null;
        }

        private void HaltRoot(GameObject root)
        {
            if (root == null) return;
            if (_runningByRoot.TryGetValue(root, out var c) && c != null)
            {
                StopCoroutine(c);
                _runningByRoot.Remove(root);
            }

            root.SetActive(false);
        }

        private IEnumerator PlayRoutine(SkillBattleFxBinding binding, SkillEffectType effectType)
        {
            GameObject root = binding.vfxRoot;
            root.SetActive(true);

            AudioClip clip = binding.sfxClip != null ? binding.sfxClip : LoadDefaultSfx(effectType);
            if (clip != null && audioSource != null)
                audioSource.PlayOneShot(clip);

            Animator animator = root.GetComponentInChildren<Animator>(true);
            float waitSeconds;
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                string stateName = !string.IsNullOrEmpty(binding.animatorStateOverride)
                    ? binding.animatorStateOverride
                    : DefaultAnimatorStateName(effectType);

                animator.enabled = true;
                animator.Rebind();
                animator.Update(0f);
                animator.Play(Animator.StringToHash(stateName), 0, 0f);

                yield return null;

                var info = animator.GetCurrentAnimatorStateInfo(0);
                if (info.length < 0.0001f)
                {
                    yield return null;
                    info = animator.GetCurrentAnimatorStateInfo(0);
                }

                float spd = Mathf.Abs(info.speed) < 0.0001f ? 1f : Mathf.Abs(info.speed);
                waitSeconds = info.length / spd;
                if (waitSeconds <= 0f)
                    waitSeconds = clip != null ? clip.length : 0.35f;
            }
            else
                waitSeconds = clip != null ? clip.length : 0.35f;

            yield return new WaitForSeconds(waitSeconds);

            root.SetActive(false);
            if (_runningByRoot.ContainsKey(root))
                _runningByRoot.Remove(root);
        }

        private static string DefaultAnimatorStateName(SkillEffectType t)
        {
            return t switch
            {
                SkillEffectType.Sword => "Sword_Effect",
                SkillEffectType.Bow => "Wind_Effect",
                SkillEffectType.Wand => "Magition_Effect",
                SkillEffectType.Heal => "Helth_Effect",
                _ => "Default"
            };
        }

        private static AudioClip LoadDefaultSfx(SkillEffectType t)
        {
            string path = t switch
            {
                SkillEffectType.Sword => "Sound/Effect_Sound/Sword_Effect_Sound",
                SkillEffectType.Bow => "Sound/Effect_Sound/Acher_Effect_Sound",
                SkillEffectType.Wand => "Sound/Effect_Sound/Magition_Effect_Sound",
                SkillEffectType.Heal => "Sound/Effect_Sound/Helth_Effect_Sound",
                _ => null
            };

            return string.IsNullOrEmpty(path) ? null : Resources.Load<AudioClip>(path);
        }

        private void OnDisable()
        {
            if (_runningByRoot.Count == 0) return;
            foreach (var kv in _runningByRoot)
            {
                if (kv.Value != null)
                    StopCoroutine(kv.Value);
                if (kv.Key != null)
                    kv.Key.SetActive(false);
            }

            _runningByRoot.Clear();
        }
    }
}
