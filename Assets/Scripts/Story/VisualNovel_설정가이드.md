# 비주얼 노벨 스타일 스토리 설정 가이드

## 목차

1. [개요](#1-개요)
2. [StoryScene 구성](#2-storyscene-구성)
3. [StoryDialogueController 연결](#3-storydialoguecontroller-연결)
4. [데이터베이스 관리](#4-데이터베이스-관리)
5. [컷신 재생 방식](#5-컷신-재생-방식)
6. [VNTextTyper](#6-vntexttyper)
7. [타이틀씬 연동](#7-타이틀씬-연동)
8. [Build Settings](#8-build-settings)
9. [스크립트 참조](#9-스크립트-참조)

---

## 1. 개요

### 1-1. 지원 기능

| 기능 | 설명 |
|------|------|
| **타이핑 효과** | 글자 한 글자씩 출력, 클릭 시 즉시 완성 |
| **스킵** | 컷신 전체 건너뛰기 |
| **씬 전환** | 컷신 종료 후 지정 씬으로 이동 |
| **캐릭터 재사용** | CharacterDatabase로 여러 컷신에서 ID 참조 |
| **배경 전환** | 대사별 배경 스프라이트 변경 |
| **DataDialogue 연동** | 병렬 리스트(characterIds, texts)로 재생 |

### 1-2. 데이터 흐름

```
[씬 A] CutsceneContext.SetNext("Prologue")
    → SceneManager.LoadScene("StoryScene")

[StoryScene]
    → CutsceneContext 확인
    → Resources/Story/Cutscenes/Prologue.asset 로드
    → CharacterDatabase에서 캐릭터 조회
    → 대화 재생 (타이핑, 스킵 가능)
    → 완료 시 GameScene 등으로 전환
```

---

## 2. StoryScene 구성

### 2-1. Hierarchy 구조

```
StoryScene
├── Main Camera
├── EventSystem
├── Canvas (Screen Space - Overlay)
│   ├── Background (Image)              ← 배경
│   ├── CharacterImage (Image)          ← 캐릭터 초상화
│   ├── DialogueBox (Panel/Image)
│   │   ├── SpeakerName (TextMeshPro)   ← 화자 이름
│   │   ├── DialogueText (TextMeshPro)  ← 대사
│   │   └── ClickArea (Button)          ← 클릭 영역 (투명)
│   └── TopUI (빈 오브젝트, 우상단)
│       ├── UIToggleButton (Button)
│       ├── ScriptButton (Button)
│       ├── SkipButton (Button)
│       └── SettingsButton (Button)
└── StoryDialogueController (빈 오브젝트)
```

### 2-2. 오브젝트 생성 방법

| Hierarchy 이름 | 생성 방법 | 비고 |
|----------------|-----------|------|
| Background | `UI > Image` | Canvas 자식, Anchor stretch |
| CharacterImage | `UI > Image` | Canvas 자식 |
| DialogueBox | `UI > Panel` 또는 `Image` | 반투명 권장 |
| SpeakerName | `UI > Text - TextMeshPro` | DialogueBox 자식 |
| DialogueText | `UI > Text - TextMeshPro` | DialogueBox 자식 |
| ClickArea | `UI > Button` | DialogueBox 자식, stretch. Image Alpha 0, Raycast Target 체크 |
| TopUI | 빈 GameObject | Canvas 자식, Anchor 우상단 |
| UIToggleButton | `UI > Button` | TopUI 자식 |
| ScriptButton | `UI > Button` | TopUI 자식 |
| SkipButton | `UI > Button` | TopUI 자식 |
| SettingsButton | `UI > Button` | TopUI 자식 |
| StoryDialogueController | 빈 GameObject | 씬 루트 |

---

## 3. StoryDialogueController 연결

StoryDialogueController 오브젝트에 스크립트를 붙인 뒤 Inspector에서 다음을 연결:

| 필드 | 연결 대상 |
|------|-----------|
| Speaker Name Text | SpeakerName (TMP) |
| Dialogue Text | DialogueText (TMP) |
| Dialogue Box | DialogueBox |
| Click Area Button | ClickArea (Button) |
| Background Image | Background (Image) |
| Character Image | CharacterImage (Image) |
| **Character Database** | CharacterDatabase 에셋 **(CutsceneData 사용 시 필수)** |
| Text Typer | 비워두면 자동 추가 |
| Skip Button | SkipButton |
| Script Button | ScriptButton |
| Settings Button | SettingsButton |
| UI Toggle Button | UIToggleButton |
| Next Scene Name | `"GameScene"` (Inspector 모드 기본값) |
| Is Prologue Mode | true (Inspector 모드 기본값) |
| Dialogue Lines | Inspector 모드 시 사용 (선택) |

---

## 4. 데이터베이스 관리

### 4-1. 폴더 구조 권장

```
Assets/
├── Resources/
│   └── Story/
│       └── Cutscenes/          ← CutsceneContext.SetNext("Prologue") 로 불러올 에셋
│           ├── Prologue.asset
│           ├── PostTutorial.asset
│           └── PostStage1.asset
├── ScriptableObjects/
│   └── Story/
│       ├── CharacterDatabase.asset
│       ├── Characters/         ← DataCharacter 에셋
│       │   ├── Chr_Elia.asset
│       │   └── ...
│       └── Cutscenes/          ← (선택) Inspector에서 직접 드래그할 CutsceneData
│           └── ...
```

**두 Cutscenes 폴더의 차이**

| 위치 | 용도 | 이유 |
|------|------|------|
| **Resources/Story/Cutscenes/** | `CutsceneContext.SetNext("ID")` 후 StoryScene 로드 시 **문자열 ID로 로드**하는 CutsceneData | Unity는 `Resources.Load()`로만 런타임에 경로로 에셋을 불러올 수 있음. 반드시 `Resources` 폴더 안에 있어야 함. |
| **ScriptableObjects/Story/Cutscenes/** | **Inspector에서** 어떤 컴포넌트의 `CutsceneData` 필드에 **직접 드래그해서 할당**할 때 쓰는 에셋 | 경로 상관없음. 에디터에서 참조만 하므로 `Resources` 밖이어도 됨. 정리용 폴더. |

- **CutsceneContext로 불러올 CutsceneData** → 반드시 `Resources/Story/Cutscenes/`에 저장
- **CharacterDatabase, DataCharacter** → 원하는 위치 (예: `ScriptableObjects/Story/`)

### 4-2. CharacterDatabase 관리

#### 생성

1. Project 창에서 우클릭
2. `Create > Story > Character Database`
3. 파일명: `CharacterDatabase` 등
4. 저장 위치: `Assets/ScriptableObjects/Story/` 등

#### 캐릭터 등록

1. CharacterDatabase 에셋 선택
2. Inspector에서 **Characters** 리스트 확장
3. `+`로 슬롯 추가
4. 각 슬롯에 DataCharacter 에셋 드래그

| 항목 | 설명 |
|------|------|
| Size | 등록할 캐릭터 수 |
| Element 0, 1, ... | DataCharacter 에셋 |

- **chrID 중복 금지**: 같은 chrID가 두 개 있으면 첫 번째만 사용됨
- **순서 무관**: Dictionary로 변환되므로 리스트 순서는 상관없음

#### CharacterDatabase 사용처

- StoryDialogueController의 **Character Database** 필드
- CutsceneData 재생 시, PlayFromParallelLists 호출 시 사용

### 4-3. DataCharacter 관리

#### 생성

1. Project 창에서 우클릭
2. `Create > Story > Data Character`
3. 파일명: `Chr_Elia`, `Chr_Sophia` 등

#### 필드 설정

| 필드 | 설명 |
|------|------|
| Chr ID | 고유 번호. CutsceneData·DataDialogue에서 참조할 ID |
| Display Name | 화면에 표시할 이름 (예: "엘리야") |
| Chr Image | 초상화 스프라이트 |

#### ID 배정 예시

| Chr ID | Display Name | 용도 |
|--------|--------------|------|
| 0 | 엘리야 | 주인공 |
| 1 | 소피아 | 파티원 |
| 2 | 다이나 | 파티원 |
| 3 | 세실리아 | 파티원 |
| -1 | (비움) | 나레이터(캐릭터 없음) |

- **chrID -1**: 나레이터, 시스템 메시지 등. DataCharacter 에셋 불필요
- **일관성 유지**: CutsceneData, DataDialogue 등에서 사용하는 ID가 CharacterDatabase에 등록된 것과 일치해야 함

### 4-4. CutsceneData 관리

#### 생성

1. Project 창에서 우클릭
2. `Create > Story > Cutscene Data`
3. **CutsceneContext로 불러올 경우**  
   - 저장 경로: `Assets/Resources/Story/Cutscenes/`  
   - 파일명이 컷신 ID (예: `Prologue.asset` → ID `"Prologue"`)

#### 상단 메타 필드

| 필드 | 설명 | 예시 |
|------|------|------|
| Cutscene Id | 참고용. 파일명과 맞추면 관리 편함 | `Prologue` |
| Next Scene Name | 완료 후 이동할 씬 | `GameScene`, `MapScene` |
| Is Prologue Mode | true: 완료 시 씬 전환 / false: 콜백만 | true |
| Clear Save On Complete | true: 세이브 삭제 (New Game용) / false: 저장 유지 | 프롤로그: true, 스테이지 후: false |

#### Dialogue Lines (대사 블록)

| 필드 | 설명 |
|------|------|
| Character Id | CharacterDatabase의 chrID. -1이면 나레이터 |
| Speaker Name | characterId -1일 때 사용. 또는 표시 이름 오버라이드 |
| Text | 대사 내용. `\n`으로 줄바꿈 |
| Background Sprite | (선택) 이 대사에서 배경 변경 |

#### 대사 추가 절차

1. CutsceneData 선택
2. **Dialogue Lines** 리스트에서 `+` 클릭
3. Character Id, Speaker Name, Text 등 입력
4. 배경 변경이 필요하면 Background Sprite에 스프라이트 할당

#### 컷신별 CutsceneData 예시

| CutsceneData | Next Scene | Clear Save |
|--------------|------------|------------|
| Prologue | GameScene | true |
| PostTutorial | MapScene | false |
| PostStage1 | MapScene | false |
| Chapter2Intro | BattleScene | false |

### 4-5. 데이터베이스 연동 체크리스트

- [ ] DataCharacter 에셋 생성 (chrID, displayName, chrImage)
- [ ] CharacterDatabase에 DataCharacter 등록
- [ ] CutsceneData Dialogue Lines에서 characterId에 맞는 ID 사용
- [ ] CutsceneContext로 불러올 CutsceneData는 `Resources/Story/Cutscenes/`에 저장
- [ ] StoryDialogueController에 CharacterDatabase 연결

---

## 5. 컷신 재생 방식

### 5-1. CutsceneContext (씬 전환 시)

다른 씬에서 StoryScene으로 넘기며 컷신 ID를 전달할 때 사용.

**코드:**

```csharp
using Story;

// 프롤로그
CutsceneContext.SetNext("Prologue");
SceneManager.LoadScene("StoryScene");

// 스테이지 1 클리어 후
CutsceneContext.SetNext("PostStage1");
SceneManager.LoadScene("StoryScene");
```

**동작:**

1. `CutsceneContext.SetNext("Prologue")` 호출
2. `SceneManager.LoadScene("StoryScene")` 실행
3. StoryScene 로드 후 StoryDialogueController가 `Resources/Story/Cutscenes/Prologue` 로드
4. CharacterDatabase로 캐릭터 조회 후 재생

### 5-2. Inspector Dialogue Lines

CutsceneContext 없이 StoryScene에 직접 진입했을 때, Inspector에 넣은 Dialogue Lines를 사용.

- **Dialogue Lines** 리스트에 대사 블록 추가
- Speaker Name, Text, Character Sprite, Background Sprite 직접 지정
- CharacterDatabase 없이도 동작

### 5-3. PlayCutscene (코드에서 직접)

이미 CutsceneData 참조가 있을 때:

```csharp
var controller = FindObjectOfType<StoryDialogueController>();
controller.PlayCutscene(myCutsceneData, onComplete: () => { });
```

### 5-4. PlayFromParallelLists (DataDialogue 연동)

`DataDialogue`(dlgCharacterIDs, dlgTextValues) 형식과 연동할 때:

```csharp
using Story;

var dataDialogue = ManagerData.Instance.GetDataDialogue(id);
var controller = FindObjectOfType<StoryDialogueController>();

controller.PlayFromParallelLists(
    dataDialogue.dlgCharacterIDs,
    dataDialogue.dlgTextValues,
    onComplete: () => { /* 완료 후 처리 */ },
    nextScene: "GameScene",
    isPrologue: true
);
```

- CharacterDatabase에 `dlgCharacterIDs`에 사용된 chrID가 등록되어 있어야 함
- `displayName`은 DataCharacter에서 가져옴

### 5-5. 우선순위

1. **CutsceneContext에 ID가 있으면** → 해당 CutsceneData 로드 후 재생
2. **없고 Dialogue Lines에 요소가 있으면** → Inspector Dialogue Lines 재생
3. **둘 다 없으면** → 아무 동작 없음

---

## 6. VNTextTyper

- `DialogueText`에 **VNTextTyper**가 없으면 StoryDialogueController가 자동 추가
- **동작:**
  - 타이핑 중 클릭 → 현재 줄 즉시 완성
  - 완료 상태에서 클릭 → 다음 대사로 진행
- 별도 설정 없이 Click Area Button과 연동됨

---

## 7. 타이틀씬 연동

### 7-1. MainMenuController에서 New Game 시 프롤로그 호출

```csharp
using Story;

private void OnNewGameClicked()
{
    SaveSystem.DeleteSave();
    CutsceneContext.SetNext("Prologue");
    SceneManager.LoadScene("StoryScene");
}
```

### 7-2. ProloguePanel 제거

- TitleScene에 ProloguePanel이 있으면 제거하거나 비활성화
- New Game 시 StoryScene으로 바로 전환하도록 변경

---

## 8. Build Settings

**File > Build Settings**에 다음 씬 등록:

- StartScene (또는 TitleScene)
- **StoryScene**
- GameScene
- (기타 맵, 전투 씬 등)

---

## 9. 스크립트 참조

| 스크립트 | 위치 | 역할 |
|----------|------|------|
| **StoryDialogueController** | `Assets/Scripts/Story/` | 대화·UI 제어, CutsceneData·병렬리스트 재생 |
| **VNTextTyper** | `Assets/Scripts/Story/` | 타이핑 효과, 클릭 진행 |
| **CutsceneData** | `Assets/Scripts/Story/` | 컷신 데이터 (characterId 기반) |
| **CutsceneContext** | `Assets/Scripts/Story/` | 씬 간 컷신 ID 전달 |
| **DataCharacter** | `Assets/Scripts/Story/` | 캐릭터 정의 (chrID, displayName, chrImage) |
| **CharacterDatabase** | `Assets/Scripts/Story/` | chrID → DataCharacter 조회 |

---

## 부록: 문제 해결

### 캐릭터가 안 나올 때

- CharacterDatabase에 해당 chrID의 DataCharacter가 등록되어 있는지 확인
- CutsceneData의 characterId가 CharacterDatabase의 chrID와 일치하는지 확인

### CutsceneContext로 로드가 안 될 때

- CutsceneData가 `Resources/Story/Cutscenes/` 안에 있는지 확인
- 파일명(확장자 제외)이 SetNext에 넘긴 ID와 같은지 확인

### 스킵 후 씬 전환이 안 될 때

- CutsceneData의 Is Prologue Mode가 true인지 확인
- Next Scene Name이 빈 문자열이 아닌지 확인
