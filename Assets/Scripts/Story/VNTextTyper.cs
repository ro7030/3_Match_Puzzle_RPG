using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Story
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class VNTextTyper : MonoBehaviour, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private Button clickAreaButton;

        [Header("Typing")]
        [Tooltip("한 글자당 대기 시간(초).")]
        [SerializeField] private float charDelay = 0.05f; // 너무 빠르면 안 되므로 기본값을 적당히 설정
        [SerializeField] private bool clickAnywhere = true;

        private const float MinCharDelay = 0.02f;

        private string[] _lines;
        private int _currentLineIndex;
        private Coroutine _typingCoroutine;
        
        // 💡 [핵심] 상태 관리를 위한 3가지 변수
        private bool _isWaitingToStart; // 1. 타이핑 시작 전 빈 화면 대기 상태
        private bool _isTyping;         // 2. 한 글자씩 타이핑 중인 상태
        private bool _isComplete;       // 3. 타이핑 완료 후 대기 상태
        
        private Action _onLineComplete;
        private Action _onAllComplete;

        private float _lastClickTime = 0f;

        public bool IsTyping => _isTyping;
        public bool IsComplete => _isComplete;
        public int CurrentLineIndex => _currentLineIndex;
        public int TotalLines => _lines?.Length ?? 0;

        private void Awake()
        {
            if (targetText == null) targetText = GetComponent<TextMeshProUGUI>();
            if (clickAreaButton != null) clickAreaButton.onClick.AddListener(OnClick);
        }

        public void Play(string[] lines, Action onAllComplete = null)
        {
            if (lines == null || lines.Length == 0)
            {
                onAllComplete?.Invoke();
                return;
            }

            if (targetText != null) targetText.text = "";

            _lines = lines;
            _currentLineIndex = 0;
            _isComplete = false;
            _onAllComplete = onAllComplete;
            _onLineComplete = null;

            StopTyping();
            ShowNextLine();
        }

        public void Play(string fullText, Action onAllComplete = null)
        {
            if (string.IsNullOrEmpty(fullText))
            {
                onAllComplete?.Invoke();
                return;
            }
            string[] lines = fullText.Split(new[] { '\n' }, StringSplitOptions.None);
            Play(lines, onAllComplete);
        }

        public void Clear()
        {
            StopTyping();
            _lines = null;
            _currentLineIndex = 0;
            if (targetText != null) targetText.text = "";
            _isWaitingToStart = false;
            _isTyping = false;
        }

        private void ShowNextLine()
        {
            if (_lines == null || _currentLineIndex >= _lines.Length)
            {
                _isComplete = true;
                _onAllComplete?.Invoke();
                return;
            }

            string line = _lines[_currentLineIndex];
            if (string.IsNullOrEmpty(line))
            {
                _currentLineIndex++;
                ShowNextLine();
                return;
            }

            // 💡 [핵심 변경 1] 대사가 로드되면 바로 타이핑을 시작하지 않고 화면을 비운 채 대기합니다.
            if (targetText != null) targetText.text = "";
            _isWaitingToStart = true;
            _isTyping = false;
        }

        private IEnumerator TypeLine(string line, float delayPerChar)
        {
            if (targetText != null) targetText.text = ""; 
            
            float timer = 0f;
            int currentIndex = 0;

            // 딜레이 안전 장치: 값이 0에 가까워 전체가 한 번에 출력되는 버그 방지
            if (delayPerChar < MinCharDelay) delayPerChar = MinCharDelay;

            while (currentIndex < line.Length)
            {
                if (_typingCoroutine == null) yield break;

                timer += Time.deltaTime;
                
                while (timer >= delayPerChar && currentIndex < line.Length)
                {
                    currentIndex++;
                    timer -= delayPerChar;
                }

                if (targetText != null)
                {
                    targetText.text = line.Substring(0, currentIndex);
                }

                yield return null;
            }

            FinishLine();
        }

        private void FinishLine()
        {
            StopTyping();
            _isTyping = false;
        }

        private void StopTyping()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }
        }

        public void OnClick()
        {
            if (Time.unscaledTime - _lastClickTime < 0.05f) return;
            _lastClickTime = Time.unscaledTime;

            if (_lines == null || _currentLineIndex >= _lines.Length) return;

            // 💡 [핵심 변경 2] 클릭 시 상태에 따라 3단계로 정확히 나뉘어 작동합니다.
            if (_isWaitingToStart)
            {
                // [1단계] 빈 화면 대기 중 클릭 -> 타이핑 애니메이션 시작
                _isWaitingToStart = false;
                _isTyping = true;
                float delay = charDelay > 0f ? charDelay : MinCharDelay;
                _typingCoroutine = StartCoroutine(TypeLine(_lines[_currentLineIndex], delay));
            }
            else if (_isTyping)
            {
                // [2단계] 타이핑 중 클릭 -> 타이핑을 멈추고 전체 텍스트를 즉시 표시
                StopTyping();
                if (targetText != null) targetText.text = _lines[_currentLineIndex];
                _isTyping = false;
            }
            else
            {
                // [3단계] 전체 텍스트 표시 완료 상태에서 클릭 -> 다음 대사 로드 (다시 1단계로 진입)
                _onLineComplete?.Invoke();
                _currentLineIndex++;
                if (_currentLineIndex >= _lines.Length)
                {
                    _isComplete = true;
                    _onAllComplete?.Invoke();
                }
                else
                {
                    ShowNextLine();
                }
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (clickAnywhere) OnClick();
        }
    }
}