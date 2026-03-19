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
        
        // 상태: 타이핑 중 / 타이핑 완료 후 다음 줄 대기
        private bool _isTyping;
        private bool _isComplete;
        
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
            _isTyping = false;
            _isComplete = false;
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

            _isComplete = false;
            _isTyping = true;
            float delay = charDelay > 0f ? charDelay : MinCharDelay;
            _typingCoroutine = StartCoroutine(TypeLine(line, delay));
        }

        private IEnumerator TypeLine(string line, float delayPerChar)
        {
            if (targetText != null) targetText.text = ""; 

            // 딜레이 안전 장치: 값이 0에 가까워 전체가 한 번에 출력되는 버그 방지
            if (delayPerChar < MinCharDelay) delayPerChar = MinCharDelay;
            var wait = new WaitForSecondsRealtime(delayPerChar);
            int currentIndex = 0;

            while (currentIndex < line.Length)
            {
                if (_typingCoroutine == null) yield break;

                currentIndex++;

                if (targetText != null)
                {
                    targetText.text = line.Substring(0, currentIndex);
                }

                if (currentIndex < line.Length)
                    yield return wait;
            }

            FinishLine();
        }

        private void FinishLine()
        {
            StopTyping();
            _isTyping = false;
            _isComplete = true;
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

            if (_isTyping)
            {
                // 타이핑 중 클릭 -> 타이핑을 멈추고 전체 텍스트를 즉시 표시
                StopTyping();
                if (targetText != null) targetText.text = _lines[_currentLineIndex];
                _isTyping = false;
                _isComplete = true;
            }
            else
            {
                // 전체 텍스트 표시 완료 상태에서 클릭 -> 다음 대사 즉시 시작
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