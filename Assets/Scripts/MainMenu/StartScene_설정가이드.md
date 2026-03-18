# Start Scene 메인 메뉴 설정 가이드

## 스크립트 구성

| 스크립트 | 역할 |
|----------|------|
| `MainMenuController` | New Game / Continue / Option / End 버튼 연동 |
| `ProloguePanel` | 뉴 게임 시 프롤로그 창 표시, 시작 시 게임 씬 로드 |
| `OptionsPanel` | 옵션 팝업 (볼륨, 전체화면). 메인 메뉴·인게임 공용 |
| `SaveSystem` | 세이브 파일 저장/로드 (JSON, persistentDataPath) |
| `GameSaveData` | 저장 데이터 구조 (챕터, 골드, 스킬, 마지막 플레이 시간 등) |
| `GameSaveApplier` | 게임 씬에서 Continue 로드 데이터 적용 |

---

## 1. Hierarchy에서 만드는 것 구분

아래는 **만드는 방법** 기준으로 정리했습니다.
- **빈 오브젝트**: `GameObject > Create Empty`
- **Canvas**: `UI > Canvas` (Screen Space - Overlay)
- **Panel**: `UI > Panel` 또는 빈 오브젝트 + `UI > Image` (배경용)
- **Button**: `UI > Button - TextMeshPro` 또는 `UI > Button`
- **Text (TMP)**: `UI > Text - TextMeshPro`
- **Slider**: `UI > Slider`
- **Toggle**: `UI > Toggle`
- **스크립트**: 해당 오브젝트에 컴포넌트로 추가 (빈 오브젝트 또는 UI 오브젝트에 붙임)

---

## 2. Start Scene Hierarchy 구조

### 2-1. 씬 루트

| Hierarchy 이름 | 만드는 것 | 비고 |
|----------------|-----------|------|
| **Main Camera** | 씬 기본 카메라 | 보통 씬 생성 시 있음 |
| **EventSystem** | 씬 기본 이벤트 시스템 | UI 사용 시 필요, 없으면 `GameObject > UI > Event System` |
| **Canvas** | **UI** (`UI > Canvas`) | Screen Space - Overlay, 메인 메뉴 UI 전체의 부모 |

---

### 2-2. 메인 메뉴 컨트롤러 (루트)

| Hierarchy 이름 | 만드는 것 | 스크립트 |
|----------------|-----------|----------|
| **MainMenuController** | **빈 오브젝트** | MainMenuController (Script) 붙임. New Game / Continue / Option / End 버튼 4개 할당, ProloguePanel·OptionsPanel 참조 연결 |

---

### 2-3. Canvas 자식 (메인 메뉴 UI)

| Hierarchy 이름 | 만드는 것 | 스크립트 |
|----------------|-----------|----------|
| **MainMenuPanel** | **UI** (Panel 또는 빈 오브젝트 + Image 배경) | 없음. 메인 메뉴 버튼들의 부모 |
| **MainMenuPanel/NewGameButton** | **UI** (`UI > Button`, MainMenuPanel 자식) | 없음. MainMenuController의 **New Game Button**에 연결 |
| **MainMenuPanel/ContinueButton** | **UI** (Button) | 없음. MainMenuController의 **Continue Button**에 연결 |
| **MainMenuPanel/OptionButton** | **UI** (Button) | 없음. MainMenuController의 **Option Button**에 연결 |
| **MainMenuPanel/EndButton** | **UI** (Button) | 없음. MainMenuController의 **End Button**에 연결 |

---

### 2-4. 프롤로그 패널 (Canvas 자식)

| Hierarchy 이름 | 만드는 것 | 스크립트 |
|----------------|-----------|----------|
| **ProloguePanel** | **UI** (Panel, Canvas 자식) | ProloguePanel (Script) 붙임. 초기에는 비활성화(체크 해제) |
| **ProloguePanel/PrologueText** | **UI** (`UI > Text - TextMeshPro`) | ProloguePanel이 자동으로 VNTextTyper 추가. ProloguePanel의 **Prologue Text**에 연결 |
| **ProloguePanel/ClickArea** | **UI** (Button, 선택) | 대화 영역 전체 클릭용. Image Alpha 0, Raycast Target 체크. ProloguePanel의 **Click Area Button**에 연결 |
| **ProloguePanel/StartButton** | **UI** (Button) | 없음. ProloguePanel의 **Start Button**에 연결 |
| **ProloguePanel/SkipButton** | **UI** (Button, 선택) | 없음. ProloguePanel의 **Skip Button**에 연결 |

비주얼 노벨 스타일(글자 타이핑, 클릭으로 완성/다음) 설정은 `Assets/Scripts/Story/VisualNovel_설정가이드.md` 참고.

---

### 2-5. 옵션 팝업 (Canvas 자식)

| Hierarchy 이름 | 만드는 것 | 스크립트 |
|----------------|-----------|----------|
| **OptionsPanel** | **UI** (Panel, Canvas 자식) | OptionsPanel (Script) 붙임. 초기에는 비활성화 |
| **OptionsPanel/MasterSlider** | **UI** (`UI > Slider`) | 없음. 볼륨 슬라이더 연결 |
| **OptionsPanel/BGMSlider** | **UI** (Slider) | 없음 |
| **OptionsPanel/SFXSlider** | **UI** (Slider) | 없음 |
| **OptionsPanel/FullscreenToggle** | **UI** (`UI > Toggle`) | 없음. 전체화면 토글 |
| **OptionsPanel/CloseButton** | **UI** (Button) | 없음. OptionsPanel의 **닫기** 버튼 연결 |

---

### 2-6. 저장 없음 팝업 (선택, Canvas 자식)

| Hierarchy 이름 | 만드는 것 | 스크립트 |
|----------------|-----------|----------|
| **NoSaveDataPopup** | **UI** (작은 Panel, Canvas 자식) | 없음. 초기에는 비활성화. MainMenuController의 **No Save Data Popup**에 할당 |
| **NoSaveDataPopup/MessageText** | **UI** (Text - TextMeshPro) | 없음. "저장된 데이터가 없습니다." 문구 |
| **NoSaveDataPopup/CloseButton** | **UI** (Button) | 없음. MainMenuController의 **No Save Popup Close Button**에 할당 |

---

### 2-7. 요약

| 구분 | Hierarchy 오브젝트 |
|------|--------------------|
| **빈 오브젝트** | MainMenuController |
| **UI (Canvas)** | Canvas |
| **UI (Panel)** | MainMenuPanel, ProloguePanel, OptionsPanel, NoSaveDataPopup |
| **UI (Button)** | NewGameButton, ContinueButton, OptionButton, EndButton, StartButton, SkipButton, CloseButton(옵션·저장없음 팝업) |
| **UI (Text - TMP)** | PrologueText, MessageText |
| **UI (Slider)** | MasterSlider, BGMSlider, SFXSlider |
| **UI (Toggle)** | FullscreenToggle |
| **스크립트 붙임** | MainMenuController(MainMenuController), ProloguePanel(ProloguePanel), OptionsPanel(OptionsPanel) |

---

## 3. 스크립트 연결 방법

### 3-1. MainMenuController

- **MainMenuController (Script)** [Script 붙임] → `MainMenuController` **빈 오브젝트**에 붙임
- **New Game Button** ← `[Component]` Hierarchy의 NewGameButton
- **Continue Button** ← `[Component]` ContinueButton
- **Option Button** ← `[Component]` OptionButton
- **End Button** ← `[Component]` EndButton
- **Prologue Panel** ← `[Component]` ProloguePanel 오브젝트의 ProloguePanel 스크립트
- **Options Panel** ← `[Component]` OptionsPanel 오브젝트의 OptionsPanel 스크립트
- **Game Scene Name** ← 게임 플레이 씬 이름 (예: `GameScene`)
- (선택) **No Save Data Popup** ← 저장 없을 때 띄울 패널 GameObject
- (선택) **No Save Popup Close Button** ← 해당 패널의 닫기 버튼

### 3-2. ProloguePanel

- **ProloguePanel (Script)** [Script 붙임] → `ProloguePanel` **UI Panel**에 붙임
- **Panel Root** ← 이 패널 GameObject
- **Prologue Text** ← `[Component]` 문구 Text (TMP). ProloguePanel이 VNTextTyper를 자동 추가함
- **Click Area Button** ← (선택) 클릭으로 텍스트 진행할 영역. 전체 대화 상자를 덮는 투명 Button 권장
- **Start Button** ← `[Component]` 시작 버튼 (프롤로그 완료 후 표시)
- **Skip Button** ← `[Component]` (선택) 스킵 버튼
- **Prologue Content** ← 인스펙터에서 기본 문구 입력 (줄바꿈으로 여러 줄 구분)
- **Game Scene Name** ← 뉴 게임 시작 시 로드할 씬 이름

### 3-3. OptionsPanel

- **OptionsPanel (Script)** [Script 붙임] → `OptionsPanel` **UI Panel**에 붙임
- **Panel Root** ← 이 패널 GameObject
- 슬라이더·토글·닫기 버튼 연결
- 메인 메뉴 씬의 `MainMenuController`에서 **Options Panel**에 이 `OptionsPanel` 할당

---

## 4. 게임 씬에서 할 일

### Continue 로드 데이터 적용

- 게임 씬에 **빈 오브젝트**를 만들고 `GameSaveApplier` 추가
- `Apply On Start` 체크 시, 씬 로드 후 자동으로 `LoadedSaveDataHolder.Data` 적용
- `GameSaveApplier.ApplyLoadedSave()` 안에서 프로젝트에 맞게 다음을 구현:
  - 챕터: `data.lastClearedChapter`
  - 골드: `data.gold`
  - 스킬: `data.unlockedSkillIds`
  - 플레이 시간: `data.playTimeSeconds` 등

### 게임 진행 중 저장

- 챕터 클리어, 골드 변경, 스킬 해금 시 `SaveSystem.Save(data)` 호출
- `GameSaveData`를 채울 때:
  - `currentSceneName = SceneManager.GetActiveScene().name`
  - `lastPlayTimeUtc`는 `SaveSystem.Save()` 안에서 자동 갱신

### 인게임에서 옵션 열기

- **방식 1**: 게임 씬에도 옵션 패널 프리팹을 두고, 옵션 버튼에서 `OptionsPanel.Open()` 호출
- **방식 2**: 옵션만 있는 씬을 추가하고, 옵션 씬을 additive로 로드한 뒤 닫을 때 unload

같은 `OptionsPanel` 프리팹을 메인 메뉴와 게임 씬 양쪽에 두고, 버튼에서 `FindFirstObjectByType<OptionsPanel>()` 후 `Open()` 호출해도 됨.

---

## 5. End 버튼

- `MainMenuController`에서 End 버튼에만 연결하면 됨
- 에디터에서는 Play 모드 종료, 빌드에서는 `Application.Quit()` 호출

---

## 6. 씬 이름 확인

- **File > Build Settings**에 Start Scene과 Game Scene 모두 추가
- `MainMenuController`의 **Game Scene Name**과 실제 씬 이름이 일치하는지 확인
