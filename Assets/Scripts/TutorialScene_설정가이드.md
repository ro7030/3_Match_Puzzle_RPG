# TutorialScene 설정 가이드

BattleScene을 복사한 뒤 아래 **제거 → 수정 → 추가** 순서대로 작업하세요.

---

## 0. 목표 동작 흐름

```
씬 시작
  └─ 보드 초기화 (칼 타일 6개, 특정 배치)
  └─ 대사 ① "타일을 클릭하여 상하좌우로 움직여 3개를 맞춰 보세요."
        ↓ (플레이어가 3-match 성공)
  └─ 대사 ② "하단에 있는 스킬 아이콘을 클릭해 보세요."
        ↓ (플레이어가 스킬 아이콘 클릭)
  └─ 스킬 사용 연출 (SkillSlotUI)
  └─ 대사 ③ "튜토리얼이 끝났습니다."
        ↓ (일정 시간 후 또는 버튼 클릭)
  └─ 씬 전환 (예: MapScene / MainMenu)
```

---

## 1. BattleScene에서 제거할 것

### 1-1. Hierarchy에서 삭제할 오브젝트

| 삭제 오브젝트 | 이유 |
|--------------|------|
| **MonsterPanel** (MonsterSprite, MonsterHealthBar, PercentText, HealthNumbersText 포함) | 몬스터 없음 |
| **MonsterAttackController** | 몬스터 공격 없음 |
| **TurnUI** (Icon, TurnText 포함) | 턴 카운터 불필요 |
| **BattleSceneConfigApplier** | 스테이지 데이터 불필요 |
| **TopRightButtons/BackButton** | BattleBackButton 스크립트 불필요 |
| **PartyPanel** (CharacterSlot0~3 포함) | 파티 체력 UI 불필요 |
| **SkillSlot1, SkillSlot2, SkillSlot3** | 스킬 1개만 사용 |

> **SkillSlot0 하나만 남기세요.** SkillBar 오브젝트 자체는 유지.

---

### 1-2. GameManager 오브젝트에서 떼어낼 스크립트

| 스크립트 | 처리 방법 |
|----------|-----------|
| **BattleSceneStarter** | **제거** (→ TutorialSceneStarter로 교체) |
| **MatchEffectHandler** | **제거** (몬스터/파티 데미지 없음) |
| **ScoreManager** | **제거** (점수 불필요) |
| **LevelManager** | **유지** 가능 (movesRemaining을 9999 등 무제한으로 설정하면 무방) 또는 제거 |

> 제거 시 TileSwapper에서 LevelManager 참조를 null로 두면 에러 없이 동작합니다 (TileSwapper는 levelManager == null일 때 턴 소모만 건너뜀).

---

### 1-3. TileSlots: 100개 → 6개로 교체

`PuzzleBoardFrame/TileSlots` 아래 **Image 100개를 모두 삭제**하고, 아래 설명대로 **Image 6개**만 새로 배치합니다.

---

## 2. Hierarchy 수정 사항

### 2-1. TileSlots: 6개 타일 (+자형) 배치

칼 3개를 세로로 맞추는 시나리오를 기준으로 아래 배치를 권장합니다.  
(칸 크기 예시: 120×120px, 간격 130px)

```
인덱스  anchoredPosition  설명
  0      (0, +130)        위 (Top)
  1      (-130, 0)        좌 (Left)
  2      (0, 0)           가운데 (Center) ← 플레이어가 이동할 타일
  3      (+130, 0)        우 (Right)
  4      (0, -130)        아래 1 (Bottom-1)
  5      (0, -260)        아래 2 (Bottom-2)
```

이렇게 배치하면 GameBoard가 자동으로 **width=3, height=2** (또는 비슷한 크기)로 잡지 못하므로,  
아래 2-2의 TutorialSceneStarter에서 수동으로 `InitializeBoard`를 호출해 타일을 직접 할당합니다.

> **만드는 방법:**  
> PuzzleBoardFrame/TileSlots 빈 오브젝트 선택 → `UI > Image` 6개 추가 →  
> 각 Image의 anchoredPosition을 위 표대로 조정 → 각 Image에 **Tile (Script)** 컴포넌트 추가.

#### 초기 타일 타입 배치 (3-match가 즉시 발생하지 않도록)

| 위치 | 타일 타입 |
|------|-----------|
| Top (0, +130) | 칼 (0) |
| Left (-130, 0) | 칼 (0) |
| **Center (0, 0)** | **칼 (0) ← 이 타일을 아래로 드래그** |
| Right (+130, 0) | 칼 (0) |
| Bottom-1 (0, -130) | 칼 (0) |
| Bottom-2 (0, -260) | 칼 (0) |

> 초기에는 세로(Top-Center-Bottom1)가 3줄이면 바로 매칭됩니다.  
> **TutorialSceneStarter**가 board 초기화 후 Center 타일을 일시적으로 **다른 타입(예: 타입 1)**으로 설정하면  
> 초기 매칭을 막을 수 있습니다. 아래 스크립트 섹션 참고.

---

### 2-2. 추가할 Hierarchy 오브젝트

| Hierarchy 이름 | 만드는 것 | 스크립트 | 비고 |
|---------------|-----------|----------|------|
| **TutorialManager** | 빈 오브젝트 (Canvas 자식) | **TutorialManager (Script)** | 튜토리얼 흐름 제어 |
| **TutorialDialogue** | **Text - TextMeshPro** (Canvas 자식) | 없음 | 대사 텍스트 표시 |

> TutorialDialogue: 화면 하단 또는 중앙에 배치. 폰트 크기 28~36 권장.

---

## 3. 스크립트 연결 방법

### 3-1. GameBoard

- **Tile Slot Parent** ← `TileSlots` (6개 Image의 부모)
- **Tile Sprites** ← **칼 스프라이트만 8번 채워도 되고**, 실제로는 인덱스 0에만 칼 스프라이트 할당  
  (TutorialSceneStarter가 tileTypeCount=1로 초기화하므로 0번 스프라이트만 사용)
- Board Width / Height: GameBoard가 자동 계산하므로 그대로 둠

---

### 3-2. TutorialSceneStarter (신규 — 아래 작성 필요)

`GameManager` 오브젝트에 붙임. BattleSceneStarter 자리를 교체.

```
TutorialSceneStarter (Script)
  - Tile Type Count: 1 (칼만 나오게)
  - Center Tile Index: 2 (0번째부터 세어 가운데 타일의 Tile Slot 인덱스)
  - Initial Center Type Override: 1 (초기에 Center를 다른 타입으로 설정해 즉시 매칭 방지)
```

---

### 3-3. TutorialManager (신규 — 아래 작성 필요)

`TutorialManager` 오브젝트에 붙임.

```
TutorialManager (Script)
  - Dialogue Text ← [Component] TutorialDialogue의 TextMeshProUGUI
  - Skill Slot UI  ← [Component] SkillSlot0의 SkillSlotUI
  - Step1 Message : "타일을 클릭하여 상하좌우로 움직여 3개를 맞춰 보세요."
  - Step2 Message : "하단에 있는 스킬 아이콘을 클릭해 보세요."
  - Step3 Message : "튜토리얼이 끝났습니다."
  - Auto Proceed Delay : 3 (완료 메시지 후 씬 전환까지 대기 초)
  - Next Scene Name  : "MapScene" (또는 전환할 씬 이름)
```

---

### 3-4. SkillSlot0 (SkillSlotUI)

- **Button** ← SkillSlot0/Button
- **Icon Image** ← SkillSlot0/Icon (칼 캐릭터 스킬 아이콘 스프라이트 할당)
- **Cooldown Duration** ← 0 (튜토리얼이므로 쿨타임 없앰) 또는 원하는 값
- **OnSkillUsed** 이벤트 → TutorialManager.OnSkillUsed 연결 (코드로 처리, 아래 스크립트 참고)

---

### 3-5. InputHandler

기존 그대로 사용 가능. 단, GameState.Playing 상태일 때만 동작하므로  
TutorialSceneStarter가 `GameManager.ChangeState(Playing)`을 호출해야 함.

---

## 4. 작성해야 할 신규 스크립트

### 4-1. TutorialSceneStarter.cs (필수)

`BattleSceneStarter`를 대체하는 스크립트. 아래 로직이 필요합니다.

```csharp
// Assets/Scripts/Tutorial/TutorialSceneStarter.cs
using UnityEngine;
using Match3Puzzle.Core;
using Match3Puzzle.Board;

namespace Match3Puzzle.Tutorial
{
    public class TutorialSceneStarter : MonoBehaviour
    {
        [SerializeField] private int tileTypeCount = 1;      // 칼만 나오게 (타입 0만 사용)
        [SerializeField] private int centerTileIndex = 2;    // TileSlots 자식 중 가운데 타일 인덱스
        [SerializeField] private int initialCenterTypeOverride = 1; // 초기 매칭 방지용 타입

        private void Start()
        {
            var board = FindFirstObjectByType<GameBoard>();
            if (board != null)
                board.InitializeBoard(board.Width, board.Height, tileTypeCount);

            // 가운데 타일만 다른 타입으로 덮어써서 초기 즉시 매칭 방지
            OverrideCenterTile(board);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Playing);
                // StartGame은 호출하지 않음 (board.InitializeBoard를 직접 호출했으므로)
            }
        }

        private void OverrideCenterTile(GameBoard board)
        {
            if (board == null) return;
            // TileSlots 자식에서 centerTileIndex번 타일의 타입을 임시 변경
            var tileSlotParent = board.transform.parent; // 필요 시 Inspector에서 직접 참조
            // TutorialManager가 플레이어에게 "Center를 아래로 드래그하세요"를
            // 보여준 뒤, 드래그 시 타입을 0으로 복원하면 됩니다.
            // (아래 TutorialManager.cs에서 처리)
        }
    }
}
```

> **핵심**: `tileTypeCount = 1`로 `InitializeBoard`를 호출하면 모든 타일이 칼(타입 0)로 초기화됩니다.  
> 단, 세로 3줄이 되는 경우 즉시 매칭이 발생할 수 있으므로, 가운데 타일(Center)의 초기 타입을  
> 잠깐 1(칼이 아닌 타입)로 덮어써 매칭을 막습니다.  
> 플레이어가 Center를 드래그하는 순간 타입을 0으로 복원하면 자연스럽게 3-match가 성공합니다.

---

### 4-2. TutorialManager.cs (필수)

튜토리얼 단계 제어의 핵심 스크립트.

```csharp
// Assets/Scripts/Tutorial/TutorialManager.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Match3Puzzle.UI.Battle;
using Match3Puzzle.Board;

namespace Match3Puzzle.Tutorial
{
    public enum TutorialStep
    {
        WaitingForMatch,    // 대사①: 타일을 맞춰 보세요
        WaitingForSkill,    // 대사②: 스킬 아이콘 클릭
        Completed           // 대사③: 튜토리얼 끝
    }

    public class TutorialManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private SkillSlotUI skillSlotUI;

        [Header("Messages")]
        [SerializeField] private string step1Message = "타일을 클릭하여 상하좌우로 움직여 3개를 맞춰 보세요.";
        [SerializeField] private string step2Message = "하단에 있는 스킬 아이콘을 클릭해 보세요.";
        [SerializeField] private string step3Message = "튜토리얼이 끝났습니다.";

        [Header("Settings")]
        [SerializeField] private float autoProceedDelay = 3f;
        [SerializeField] private string nextSceneName = "MapScene";

        public TutorialStep CurrentStep { get; private set; } = TutorialStep.WaitingForMatch;

        private void Start()
        {
            ShowDialogue(step1Message);

            if (skillSlotUI != null)
            {
                skillSlotUI.OnSkillUsed += OnSkillUsed;
                // 스킬 버튼은 대사② 전까지 비활성화
                SetSkillInteractable(false);
            }
        }

        private void OnDestroy()
        {
            if (skillSlotUI != null)
                skillSlotUI.OnSkillUsed -= OnSkillUsed;
        }

        /// <summary>
        /// TileClearer(또는 TileSwapper)에서 매칭 발생 시 호출.
        /// Inspector의 UnityEvent 또는 코드에서 직접 연결하세요.
        /// </summary>
        public void OnMatchOccurred()
        {
            if (CurrentStep != TutorialStep.WaitingForMatch) return;

            CurrentStep = TutorialStep.WaitingForSkill;
            ShowDialogue(step2Message);
            SetSkillInteractable(true);
        }

        private void OnSkillUsed()
        {
            if (CurrentStep != TutorialStep.WaitingForSkill) return;

            CurrentStep = TutorialStep.Completed;
            ShowDialogue(step3Message);
            StartCoroutine(AutoProceed());
        }

        private IEnumerator AutoProceed()
        {
            yield return new WaitForSeconds(autoProceedDelay);
            if (!string.IsNullOrEmpty(nextSceneName))
                SceneManager.LoadScene(nextSceneName);
        }

        private void ShowDialogue(string message)
        {
            if (dialogueText != null)
                dialogueText.text = message;
        }

        private void SetSkillInteractable(bool interactable)
        {
            if (skillSlotUI == null) return;
            var btn = skillSlotUI.GetComponentInChildren<UnityEngine.UI.Button>();
            if (btn != null)
                btn.interactable = interactable;
        }
    }
}
```

---

### 4-3. TutorialManager에 매칭 이벤트 연결 방법

TutorialManager.OnMatchOccurred()를 호출할 수 있는 방법은 두 가지입니다.

#### 방법 A — TileClearer에 이벤트 추가 (권장)

`TileClearer.cs`를 열어 아래 한 줄만 추가합니다.

```csharp
// TileClearer.cs의 ClearMatches 코루틴 안, matchEffectHandler 호출 직후에 추가:
public event System.Action OnMatchCleared;  // 클래스 상단에 선언

// ClearMatches 코루틴 안:
if (matchEffectHandler != null)
    matchEffectHandler.ApplyMatchEffects(matches);

OnMatchCleared?.Invoke();  // ← 이 한 줄 추가
```

그런 다음 TutorialManager의 Start()에서:
```csharp
var clearer = FindFirstObjectByType<TileClearer>();
if (clearer != null)
    clearer.OnMatchCleared += OnMatchOccurred;
```

#### 방법 B — Inspector UnityEvent 사용

TileClearer에 `[SerializeField] UnityEvent onMatchCleared`를 추가하고  
Inspector에서 TutorialManager.OnMatchOccurred를 연결합니다.

---

## 5. 최종 Hierarchy 구조 (TutorialScene)

```
Main Camera
EventSystem
Canvas
  ├─ Background/Image         (배경 이미지, 선택)
  ├─ GameManager              (빈 오브젝트)
  │    └─ GameBoard           (빈 오브젝트)
  ├─ PuzzleBoardFrame         (빈 오브젝트)
  │    ├─ Image               (보드 배경 스프라이트)
  │    └─ TileSlots           (빈 오브젝트, 6개 Image+Tile 자식)
  │         ├─ Tile(0)        Image + Tile Script  (Top)
  │         ├─ Tile(1)        Image + Tile Script  (Left)
  │         ├─ Tile(2)        Image + Tile Script  (Center ★)
  │         ├─ Tile(3)        Image + Tile Script  (Right)
  │         ├─ Tile(4)        Image + Tile Script  (Bottom-1)
  │         └─ Tile(5)        Image + Tile Script  (Bottom-2)
  ├─ SkillBar                 (빈 오브젝트)
  │    └─ SkillSlot0          (SkillSlotUI Script)
  │         ├─ Button
  │         ├─ Icon
  │         ├─ CooldownOverlay
  │         └─ GrayOverlay
  ├─ TutorialManager          (빈 오브젝트, TutorialManager Script)
  └─ TutorialDialogue         (Text - TextMeshPro)
```

---

## 6. GameManager 오브젝트에 붙을 스크립트 목록

| 스크립트 | 유지/제거/신규 |
|----------|---------------|
| Game Manager | ✅ 유지 |
| **TutorialSceneStarter** | 🆕 신규 (BattleSceneStarter 교체) |
| Input Handler | ✅ 유지 |
| Match Detector | ✅ 유지 |
| Tile Swapper | ✅ 유지 |
| Tile Clearer | ✅ 유지 (OnMatchCleared 이벤트 추가) |
| Gravity Controller | ✅ 유지 |
| Tile Spawner | ✅ 유지 |
| ~~BattleSceneStarter~~ | ❌ 제거 |
| ~~MatchEffectHandler~~ | ❌ 제거 |
| ~~ScoreManager~~ | ❌ 제거 (선택) |
| ~~LevelManager~~ | ❌ 제거 또는 무제한 턴으로 유지 |

---

## 7. 작업 체크리스트

### Step 1 — Hierarchy 정리
- [ ] MonsterPanel 삭제
- [ ] MonsterAttackController 삭제
- [ ] TurnUI 삭제
- [ ] BattleSceneConfigApplier 삭제
- [ ] PartyPanel 삭제
- [ ] SkillSlot1, 2, 3 삭제
- [ ] TopRightButtons/BackButton의 BattleBackButton 제거 (버튼 자체는 유지해도 무방)

### Step 2 — TileSlots 교체
- [ ] 기존 TileSlots 자식 100개 Image 전부 삭제
- [ ] Image 6개 새로 생성, anchoredPosition 설정 (위 표 참고)
- [ ] 각 Image에 Tile (Script) 추가
- [ ] GameBoard의 Tile Slot Parent에 TileSlots 재연결
- [ ] GameBoard의 Tile Sprites[0]에 칼 스프라이트 할당

### Step 3 — 스크립트 교체 및 추가
- [ ] `Assets/Scripts/Tutorial/` 폴더 생성
- [ ] `TutorialSceneStarter.cs` 작성 및 GameManager에 추가, BattleSceneStarter 제거
- [ ] `TutorialManager.cs` 작성 및 TutorialManager 오브젝트에 추가
- [ ] TileClearer.cs에 `OnMatchCleared` 이벤트 추가

### Step 4 — Inspector 연결
- [ ] TutorialManager → Dialogue Text: TutorialDialogue 연결
- [ ] TutorialManager → Skill Slot UI: SkillSlot0 연결
- [ ] TutorialManager → Next Scene Name: 전환할 씬 이름 입력
- [ ] TutorialSceneStarter → Tile Type Count: 1

### Step 5 — 동작 확인
- [ ] 플레이 시 칼 타일 6개만 보이는지 확인
- [ ] 초기 매칭 없이 대사①이 뜨는지 확인
- [ ] 드래그로 3개를 맞추면 대사②로 전환되는지 확인
- [ ] 스킬 아이콘 클릭 → 스킬 연출 → 대사③ 확인
- [ ] 대사③ 후 씬 전환 확인

---

## 8. 자주 묻는 문제

| 증상 | 원인 | 해결 |
|------|------|------|
| 씬 시작 시 타일이 즉시 모두 사라짐 | tileTypeCount=1 시 모든 칼 타일이 3-match 즉시 발생 | TutorialSceneStarter에서 Center 타일을 타입 1로 임시 설정 |
| 스킬 버튼이 처음부터 눌림 | TutorialManager의 SetSkillInteractable(false)가 안 됨 | SkillSlotUI 참조 연결 확인 |
| 매칭 후 대사②가 안 뜸 | TileClearer.OnMatchCleared 이벤트 연결 누락 | TutorialManager.Start()에서 clearer 구독 확인 |
| Tile Spawner가 칼 아닌 타일을 스폰 | tileTypeCount가 1이 아님 | TutorialSceneStarter.tileTypeCount = 1 확인 |
| 보드가 초기화되지 않음 | TutorialSceneStarter에서 InitializeBoard 미호출 | Start() 안에서 `board.InitializeBoard(...)` 확인 |
