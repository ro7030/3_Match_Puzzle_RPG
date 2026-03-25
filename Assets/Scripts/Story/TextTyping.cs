using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Story
{
    /// 공용 텍스트 타이핑 컴포넌트.
    /// - 문자열 직접형: Play(string[] lines), Play(string fullText)
    /// - 구조화 데이터형: 컨트롤러가 라인 단위 문자열을 넘겨 Play(string line)
    /// 클릭 동작:
    /// 1) 타이핑 중 클릭 -> 현재 줄 즉시 완성
    /// 2) 완성 상태 클릭 -> 다음 줄/완료 콜백
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextTyping : MonoBehaviour
    {
        [Header("Typing")]
        [Tooltip("한 글자당 대기 시간(초). 값이 클수록 타이핑이 느려집니다.")]
        [SerializeField] private float charDelay = 0.05f; //의도한 타이핑 속도(디자이너 조절값)
        // 안전 하한값: charDelay가 0 또는 너무 작게 설정되어도
        // 텍스트가 한 프레임에 전부 출력되지 않도록 최소 딜레이를 보장합니다.
        [SerializeField] private float minCharDelay = 0.01f; //비정상 설정 방지용 가드레일(최소값)
        [Tooltip("대사 시작 직후 이전 UI 클릭 입력이 흘러들어오는 것을 막기 위한 클릭 잠금 시간(초)")]
        [SerializeField] private float clickBlockDurationOnLineStart = 0.3f;

        private TextMeshProUGUI _targetText;
        private Coroutine _typingRoutine;

        private string[] _lines;
        private int _lineIndex;
        private string _currentLine = string.Empty;
        private int _currentCharCount;

        private bool _isTyping;
        private bool _isLineCompleted;
        private Action _onAllComplete;
        private float _lastClickTime;
        private float _clickBlockedUntil;

        public bool IsTyping => _isTyping;
        public bool IsLineCompleted => _isLineCompleted;
        public int CurrentLineIndex => _lineIndex;
        public int TotalLines => _lines?.Length ?? 0;

        private void Awake()
        {
            // 같은 GameObject의 TMP 텍스트를 자동으로 타겟으로 사용
            _targetText = GetComponent<TextMeshProUGUI>();
        }

        public void Play(string[] lines, Action onAllComplete = null)
        {
            if (lines == null || lines.Length == 0)
            {
                onAllComplete?.Invoke();
                return;
            }

            // 새로운 대화 시작 시 화면 문자열부터 초기화
            ClearInternalTextOnly();
            _lines = lines;
            _lineIndex = 0;
            _onAllComplete = onAllComplete;
            ShowCurrentLine();
        }

        public void Play(string fullText, Action onAllComplete = null)
        {
            if (string.IsNullOrEmpty(fullText))
            {
                onAllComplete?.Invoke();
                return;
            }

            // single-line도 동일 파이프라인으로 처리
            Play(new[] { fullText }, onAllComplete);
        }

        public void Clear()
        {
            // 외부에서 강제로 대화 상태를 초기화할 때 사용
            StopTyping();
            _lines = null;
            _lineIndex = 0;
            _currentLine = string.Empty;
            _currentCharCount = 0;
            _isTyping = false;
            _isLineCompleted = false;
            _onAllComplete = null;
            ClearInternalTextOnly();
        }

        public void OnClick()
        {
            // 라인 시작 직후 잠금 구간: 씬 전환 직후 입력 잔여 이벤트를 무시
            if (Time.unscaledTime < _clickBlockedUntil) return;

            // 더블클릭/중복 입력 방지용 최소 간격
            if (Time.unscaledTime - _lastClickTime < 0.05f) return;
            _lastClickTime = Time.unscaledTime;

            if (_lines == null || _lineIndex >= _lines.Length) return;

            if (_isTyping)
            {
                // 타이핑 중 클릭: 현재 줄 즉시 완성
                CompleteCurrentLineInstantly();
                return;
            }

            // 타이핑도 아니고 줄 완성 상태도 아니면 진행 불가
            if (!_isLineCompleted) return;

            // 완성 상태 클릭: 다음 줄로 이동
            _lineIndex++;
            if (_lineIndex >= _lines.Length)
            {
                // 마지막 줄까지 끝났으면 완료 콜백
                _onAllComplete?.Invoke();
                return;
            }

            ShowCurrentLine();
        }

        private void ShowCurrentLine()
        {
            // 이전 코루틴이 남아 있을 수 있으므로 먼저 정리
            StopTyping();

            _currentLine = _lines[_lineIndex] ?? string.Empty;
            _currentCharCount = 0;
            _isLineCompleted = false;
            _isTyping = true;

            // 새 줄 시작 시 클릭 잠금 (이전 버튼 클릭 이벤트가 흘러 들어오는 문제 방지)
            _clickBlockedUntil = Time.unscaledTime + Mathf.Max(0f, clickBlockDurationOnLineStart);

            if (_currentLine.Length == 0)
            {
                // 빈 줄은 타이핑 없이 즉시 "완성된 줄" 상태로 둔다.
                // 다음 클릭에서 다음 줄로 넘어간다.
                _isTyping = false;
                _isLineCompleted = true;
                if (_targetText != null) _targetText.text = string.Empty;
                return;
            }

            _typingRoutine = StartCoroutine(TypeRoutine());
        }

        private IEnumerator TypeRoutine()
        {
            // 딜레이 하한을 두어 0 근처 설정값으로 인한 즉시 출력 방지
            float delay = Mathf.Max(minCharDelay, charDelay);
            var wait = new WaitForSecondsRealtime(delay);

            while (_currentCharCount < _currentLine.Length)
            {
                _currentCharCount++;
                if (_targetText != null)
                    _targetText.text = _currentLine.Substring(0, _currentCharCount);

                if (_currentCharCount < _currentLine.Length)
                    yield return wait;
            }

            _isTyping = false;
            _isLineCompleted = true;
            _typingRoutine = null;
        }

        private void CompleteCurrentLineInstantly()
        {
            // 현재 줄 코루틴을 중단하고 전체 문자열을 한 번에 출력
            StopTyping();
            _currentCharCount = _currentLine.Length;
            if (_targetText != null)
                _targetText.text = _currentLine;
            _isTyping = false;
            _isLineCompleted = true;
        }

        private void StopTyping()
        {
            if (_typingRoutine != null)
            {
                StopCoroutine(_typingRoutine);
                _typingRoutine = null;
            }
        }

        private void ClearInternalTextOnly()
        {
            if (_targetText != null)
                _targetText.text = string.Empty;
        }
    }
}
