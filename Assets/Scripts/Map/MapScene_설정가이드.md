# MapScene 설정 가이드

챕터(지역)를 선택하는 **월드 맵 씬**입니다.  
처음에는 챕터1(Monster Forest)만 열려 있고, 스테이지를 클리어할수록 다음 챕터가 해금됩니다.  
잠긴 챕터는 흑백 이미지로 덮이고 클릭이 비활성화됩니다.  
챕터를 클릭하면 **별도 씬 전환 없이 MapScene 위에 패널이 열리며** 스테이지 3개를 선택할 수 있습니다.

---

## 챕터 해금 조건

| 챕터 | 지역 이름 | 챕터 해금 조건 |
|------|-----------|-----------|
| 챕터1 | Monster Forest | 항상 열림 |
| 챕터2 | Ancient Legendary Ruins | 챕터1 **스테이지3** 클리어 (`lastClearedStageIndex >= 2`) |
| 챕터3 | Dragon's Fortress | 챕터2 **스테이지3** 클리어 (`lastClearedStageIndex >= 5`) |

### 챕터 내 스테이지 해금 조건

| 스테이지 | 해금 조건 |
|----------|-----------|
| Stage 1 | 챕터가 해금되면 자동으로 열림 |
| Stage 2 | Stage 1 클리어 (`lastClearedStageIndex >= 챕터첫인덱스`) |
| Stage 3 | Stage 2 클리어 (`lastClearedStageIndex >= 챕터첫인덱스 + 1`) |

### 전역 스테이지 인덱스 매핑

```
챕터1  Stage1=0  Stage2=1  Stage3=2
챕터2  Stage1=3  Stage2=4  Stage3=5
챕터3  Stage1=6  Stage2=7  Stage3=8
```

---

## 구조 한눈에 보기

```
[MapScene]
  Canvas
    Background                ← World_Map_1 (전체 컬러 배경, 항상 표시)
    │
    ├─ Chapter1Node           ← ChapterNodeUI.cs 부착 (Monster Forest)
    │     ├─ ChapterImage     ← 해당 지역 스프라이트 + Button 컴포넌트
    │     ├─ GrayscaleOverlay ← Monster_Forest_2 흑백 스프라이트 (잠금 시 표시)
    │     ├─ LockIcon         ← 자물쇠 아이콘 (선택 사항)
    │     └─ LockLabel        ← "LOCKED" 텍스트 (선택 사항)
    │
    ├─ Chapter2Node           ← ChapterNodeUI.cs 부착 (Ancient Legendary Ruins)
    │     ├─ ChapterImage
    │     ├─ GrayscaleOverlay ← Ruins_2 흑백 스프라이트
    │     ├─ LockIcon
    │     └─ LockLabel
    │
    ├─ Chapter3Node           ← ChapterNodeUI.cs 부착 (Dragon's Fortress)
    │     ├─ ChapterImage
    │     ├─ GrayscaleOverlay ← Fortress_2 흑백 스프라이트
    │     ├─ LockIcon
    │     └─ LockLabel
    │
    ├─ StageSelectPanel       ← StageSelectPanel.cs 부착 (기본 비활성, 챕터 클릭 시 표시)
    │     ├─ PanelBackground  ← 반투명 어두운 배경 이미지
    │     ├─ ChapterTitle     ← 챕터 이름 텍스트
    │     ├─ Stage1Button     ← "스테이지 1" 버튼
    │     ├─ Stage2Button     ← "스테이지 2" 버튼
    │     ├─ Stage3Button     ← "스테이지 3" 버튼
    │     └─ CloseButton      ← 닫기 버튼 (선택 사항)
    │
    ├─ KingdomButton          ← KingdomScene 으로 이동 (← 아이콘)
    └─ SettingsButton         ← 설정 버튼 (선택 사항)
  EventSystem
  MapSceneController          ← MapSceneController.cs 부착 (빈 GameObject)
```

### 씬 흐름

```
KingdomScene
  └─ [맵 버튼] → MapScene
                    ├─ [챕터1 클릭] → StageSelectPanel 열림 (스테이지 1~3 선택)
                    │                    └─ [스테이지 클릭] → BattleScene
                    ├─ [챕터2 클릭] → StageSelectPanel 열림 (해금된 경우)
                    ├─ [챕터3 클릭] → StageSelectPanel 열림 (해금된 경우)
                    └─ [뒤로가기]   → KingdomScene
```

---

## 관련 스크립트 위치

| 스크립트 | 경로 |
|----------|------|
| `MapSceneController.cs` | `Assets/Scripts/Map/` |
| `ChapterNodeUI.cs` | `Assets/Scripts/Map/` |
| `StageSelectPanel.cs` | `Assets/Scripts/Map/` |
| `ChapterHolder.cs` | `Assets/Scripts/Map/` |
| `GameSaveData.cs` | `Assets/Scripts/MainMenu/` |

---

## STEP 1: 씬 생성

> **File > New Scene** → 이름을 `MapScene` 으로 저장  
> `Assets/Scenes/` 폴더에 저장 권장

---

## STEP 2: Canvas 기본 설정

| 항목 | 값 |
|------|----|
| Canvas Scaler / UI Scale Mode | **Scale With Screen Size** |
| Reference Resolution | **1080 × 1920** |
| Match | **0.5** (Width ↔ Height 중간) |

---

## STEP 3: 스프라이트 Import Settings

**이미지 모양 그대로 클릭 판정(알파 히트 테스트)** 을 위해 아래 설정이 필수입니다.

적용 대상 스프라이트: `Monster_Forest_2`, `Ruins_2`, `Fortress_2`

> 1. Project 창에서 해당 스프라이트 선택
> 2. Inspector → **Texture Type: Sprite (2D and UI)** 확인
> 3. **Read/Write Enabled 체크** ← 필수! 없으면 히트 테스트가 동작하지 않음
> 4. Format: `RGBA 32 bit` 권장
> 5. **Apply**

---

## STEP 4: Hierarchy 구성

### 4-1. Background

1. Canvas 아래 **UI > Image** 생성 → 이름: `Background`
2. Rect Transform: **Stretch 전체** (앵커 사방 끝으로)
3. Source Image: `World_Map_1` 스프라이트 할당
4. **Raycast Target: OFF**

---

### 4-2. Chapter1Node (Monster Forest)

1. Canvas 아래 **빈 GameObject** 생성 → 이름: `Chapter1Node`
2. `ChapterNodeUI.cs` 컴포넌트 추가
3. Rect Transform: 맵에서 Monster Forest 위치에 배치

#### 자식 오브젝트 구성

**① ChapterImage** (Button + Image)

| 항목 | 값 |
|------|----|
| 컴포넌트 | Image + Button |
| Source Image | Monster Forest 컬러 스프라이트 |
| Raycast Target | **ON** |

**② GrayscaleOverlay** (Image only)

| 항목 | 값 |
|------|----|
| 컴포넌트 | Image |
| Source Image | `Monster_Forest_2` (흑백 스프라이트) |
| Raycast Target | **OFF** |
| 기본 Active | **ON** (잠금 상태가 기본) |

> 챕터1은 항상 해금 상태이므로 실행 시 자동으로 비활성화됩니다.

**③ LockIcon / ④ LockLabel** (선택 사항)

| 항목 | 값 |
|------|----|
| LockIcon | 자물쇠 아이콘 Image, Raycast Target OFF |
| LockLabel | "LOCKED" Text, Raycast Target OFF |

---

### 4-3. Chapter2Node / Chapter3Node

Chapter1Node와 동일한 구조로 생성합니다.

| 오브젝트 | GrayscaleOverlay 스프라이트 | 위치 |
|----------|----------------------------|------|
| `Chapter2Node` | `Ruins_2` | 맵에서 Ruins 위치 |
| `Chapter3Node` | `Fortress_2` | 맵에서 Fortress 위치 |

---

### 4-4. StageSelectPanel

챕터 클릭 시 맵 위에 뜨는 스테이지 선택 패널입니다.  
**기본 Active: OFF** (숨겨진 상태로 시작, 스크립트가 Show/Hide 처리)

1. Canvas 아래 **빈 GameObject** 생성 → 이름: `StageSelectPanel`
2. `StageSelectPanel.cs` 컴포넌트 추가
3. **기본 Active를 OFF로 설정** (Inspector 체크박스 해제)

#### 자식 오브젝트 구성

**① PanelBackground**

| 항목 | 값 |
|------|----|
| 컴포넌트 | Image |
| Color | RGBA (40, 20, 0, 220) — 어두운 갈색 반투명 |
| Rect Transform | 화면 중앙, 예) Width: 600 / Height: 500 |
| Raycast Target | **ON** (패널 뒤의 맵 클릭 차단) |

**② ChapterTitle**

| 항목 | 값 |
|------|----|
| 컴포넌트 | Text (또는 TextMeshPro) |
| 내용 | (스크립트가 자동 설정) |
| Font Size | 36 |
| Alignment | Center |

**③ Stage1Button / Stage2Button / Stage3Button**

| 항목 | 값 |
|------|----|
| 컴포넌트 | Button + Image |
| Width / Height | 400 / 80 |
| 배치 | Vertical 정렬, 간격 20px |
| 자식 Text | "스테이지 1" / "스테이지 2" / "스테이지 3" |

> 잠긴 스테이지는 스크립트가 자동으로 `interactable = false` 처리하고 텍스트에 🔒 표시를 추가합니다.

**④ CloseButton** (선택 사항)

| 항목 | 값 |
|------|----|
| 컴포넌트 | Button + Image |
| 위치 | 패널 우상단 |
| Width / Height | 60 / 60 |
| Source Image | X 아이콘 스프라이트 |

---

### 4-5. KingdomButton (뒤로가기)

| 항목 | 값 |
|------|----|
| 컴포넌트 | Button + Image |
| Anchor | 좌상단 |
| Pivot | (0, 1) |
| Position | X: 30, Y: -30 |
| Width / Height | 90 / 90 |
| Source Image | 뒤로가기(←) 아이콘 스프라이트 |

---

## STEP 5: MapSceneController 인스펙터 연결

Hierarchy에 **빈 GameObject** 생성 → 이름: `MapSceneController`  
`MapSceneController.cs` 컴포넌트 추가 후 아래와 같이 연결합니다.

| 인스펙터 필드 | 연결 대상 | 설명 |
|---------------|-----------|------|
| Chapter 1 Node | `Chapter1Node` GameObject | Monster Forest 노드 |
| Chapter 2 Node | `Chapter2Node` GameObject | Ruins 노드 |
| Chapter 3 Node | `Chapter3Node` GameObject | Fortress 노드 |
| Stage Select Panel | `StageSelectPanel` GameObject | 챕터 클릭 시 열릴 패널 |
| Kingdom Scene Name | `"KingdomScene"` | KingdomButton 클릭 시 이동할 씬 |
| Kingdom Button | `KingdomButton` | 뒤로가기 버튼 |
| Back Button | (선택 사항) | 별도 Back 버튼이 있는 경우 연결 |

### 유니티 On Click() 직접 연결 방법 (선택)

코드 연결 대신 On Click()으로 직접 연결하고 싶을 때:

> 1. `KingdomButton` 선택 → Inspector → `On Click()` → `+`
> 2. `MapSceneController` GameObject 드래그
> 3. 함수 선택: `MapSceneController` → **`OnKingdomButton`**

---

## STEP 6: StageSelectPanel 인스펙터 연결

`StageSelectPanel` GameObject 선택 → `StageSelectPanel.cs` 컴포넌트 확인

| 인스펙터 필드 | 연결 대상 | 설명 |
|---------------|-----------|------|
| Panel Root | `StageSelectPanel` 자기 자신 또는 자식 패널 GameObject | Show/Hide 대상 |
| Chapter Title Text | `ChapterTitle` Text 컴포넌트 | 챕터 이름 자동 표시 |
| Stage 1 Button | `Stage1Button` | 스테이지 1 버튼 |
| Stage 2 Button | `Stage2Button` | 스테이지 2 버튼 |
| Stage 3 Button | `Stage3Button` | 스테이지 3 버튼 |
| Stage 1 Text | `Stage1Button` 자식 Text | 버튼 텍스트 (선택 사항) |
| Stage 2 Text | `Stage2Button` 자식 Text | 버튼 텍스트 (선택 사항) |
| Stage 3 Text | `Stage3Button` 자식 Text | 버튼 텍스트 (선택 사항) |
| Close Button | `CloseButton` | 패널 닫기 버튼 (선택 사항) |
| Battle Scene Name | `"BattleScene"` | 스테이지 클릭 시 이동할 씬 |

> **Panel Root 설정 팁:**  
> `StageSelectPanel` GameObject 자체를 Panel Root에 연결하면 `Show()`/`Hide()` 시  
> 이 오브젝트 전체가 활성/비활성됩니다.  
> 또는 자식 Panel 오브젝트를 따로 만들어 연결해도 됩니다.

### CloseButton On Click() 연결

`CloseButton`의 On Click() → `StageSelectPanel` 드래그 → **`Hide`** 함수 선택  
(스크립트 내부에서 `Awake()`에 이미 자동 등록되므로, 필드 연결만 해도 됩니다.)

---

## STEP 7: ChapterNodeUI 인스펙터 연결

**Chapter1Node** 선택 → `ChapterNodeUI` 컴포넌트 확인

| 인스펙터 필드 | 연결 대상 | 설명 |
|---------------|-----------|------|
| Chapter Button | `ChapterImage`의 Button 컴포넌트 | 클릭 이벤트 처리 |
| Hit Area Image | `ChapterImage`의 Image 컴포넌트 | 알파 히트 테스트 기준 이미지 |
| Alpha Hit Threshold | `0.1` | 알파 0.1 미만 픽셀 클릭 무시 |
| Grayscale Image | `GrayscaleOverlay`의 Image 컴포넌트 | 잠금 시 표시될 흑백 이미지 |
| Hover Target | `ChapterImage`의 RectTransform | 호버 시 이동할 오브젝트 |
| Hover Offset Y | `12` | 올라갈 픽셀 거리 |
| Hover Duration | `0.15` | 애니메이션 시간(초) |
| Lock Icon | `LockIcon` GameObject | (없으면 비워둠) |
| Lock Label | `LockLabel` GameObject | (없으면 비워둠) |
| Chapter Display Name | `"Monster Forest"` | 표시될 챕터 이름 |

> Chapter2Node: `"Ancient Legendary Ruins"` / Chapter3Node: `"Dragon's Fortress"`

---

## STEP 8: 스테이지 클리어 시 세이브 연동

배틀 씬에서 스테이지를 클리어했을 때 아래 코드를 호출해야 챕터/스테이지 해금이 정상 동작합니다.  
기존 클리어 판정 스크립트(LevelManager 등)의 클리어 처리 부분에 추가합니다.

```csharp
using MainMenu;
using Match3Puzzle.Stage;

// 스테이지 클리어 처리 부분에 추가
var saveData = SaveSystem.Load() ?? new GameSaveData();
int clearedIndex = BattleStageHolder.CurrentStageIndex; // 0~8

if (clearedIndex > saveData.lastClearedStageIndex)
{
    saveData.lastClearedStageIndex = clearedIndex;
    saveData.lastClearedChapter = (clearedIndex / 3) + 1;
    SaveSystem.Save(saveData);
}
```

### 해금 판정 흐름

```
챕터1 Stage3 클리어 (index=2) → lastClearedStageIndex=2 저장
                               → MapScene 로드 시 Ch2 챕터 해금 (2 >= 2)
                               → 패널에서 Ch2 Stage1 표시 (i=0, 항상 열림)

챕터2 Stage3 클리어 (index=5) → lastClearedStageIndex=5 저장
                               → MapScene 로드 시 Ch3 챕터 해금 (5 >= 5)
```

---

## STEP 9: Build Settings에 씬 추가

**File > Build Settings** 에서 아래 씬들을 등록합니다.

| 씬 이름 | 비고 |
|---------|------|
| `MapScene` | 이번 가이드 |
| `BattleScene` | 스테이지 버튼 클릭 시 이동 |
| `KingdomScene` | 뒤로가기 시 이동 |

> `StageSelectScene`은 더 이상 필요하지 않습니다. 패널로 처리합니다.

---

## 씬 전체 Hierarchy 최종 예시

```
MapScene (씬)
  Main Camera
  EventSystem
  Canvas
    Background                       ← World_Map_1, Raycast Target OFF
    │
    Chapter1Node                     ← ChapterNodeUI.cs
      ChapterImage                   ← Image + Button, Read/Write ON
      GrayscaleOverlay               ← Monster_Forest_2, 기본 Active ON
      LockIcon                       ← (선택 사항)
      LockLabel                      ← (선택 사항)
    │
    Chapter2Node                     ← ChapterNodeUI.cs
      ChapterImage
      GrayscaleOverlay               ← Ruins_2, 기본 Active ON
      LockIcon
      LockLabel
    │
    Chapter3Node                     ← ChapterNodeUI.cs
      ChapterImage
      GrayscaleOverlay               ← Fortress_2, 기본 Active ON
      LockIcon
      LockLabel
    │
    StageSelectPanel                 ← StageSelectPanel.cs, 기본 Active OFF ★
      PanelBackground                ← 반투명 어두운 Image, Raycast Target ON
      ChapterTitle                   ← Text
      Stage1Button                   ← Button + Image
        Stage1Text                   ← Text "스테이지 1"
      Stage2Button
        Stage2Text
      Stage3Button
        Stage3Text
      CloseButton                    ← (선택 사항)
    │
    KingdomButton                    ← Button + Image (On Click → OnKingdomButton)
  │
  MapSceneController                 ← MapSceneController.cs 부착
```

---

## 자주 발생하는 문제

| 증상 | 원인 | 해결 |
|------|------|------|
| 투명 영역도 클릭됨 | Read/Write Enabled 미체크 | 스프라이트 Import Settings에서 체크 후 Apply |
| 패널이 열리지 않음 | Stage Select Panel 필드 미연결 | MapSceneController의 Stage Select Panel 필드 확인 |
| 챕터를 클리어해도 해금 안 됨 | 클리어 시 `SaveSystem.Save()` 미호출 | STEP 8의 코드를 클리어 판정 부분에 추가 |
| 호버 효과가 안 됨 | EventSystem 없음 | Hierarchy에 EventSystem 오브젝트 확인 |
| 패널 뒤의 챕터가 클릭됨 | PanelBackground Raycast Target OFF | PanelBackground의 Raycast Target을 **ON**으로 설정 |
| 흑백 이미지가 안 사라짐 | GrayscaleOverlay가 ChapterNodeUI에 미연결 | Grayscale Image 필드에 컴포넌트 연결 확인 |
| 씬 전환이 안 됨 | Build Settings 미등록 | File > Build Settings에 씬 추가 |

---

## 체크리스트

- [ ] Canvas Scaler → Scale With Screen Size (1080×1920)
- [ ] Background 이미지(`World_Map_1`) 할당, Raycast Target **OFF**
- [ ] 스프라이트 Import Settings → **Read/Write Enabled** 체크
- [ ] Chapter1~3Node 생성 및 맵 위치 배치
- [ ] 각 ChapterNode 아래 ChapterImage / GrayscaleOverlay / LockIcon / LockLabel 구성
- [ ] GrayscaleOverlay 기본 **Active ON** (잠금 상태가 기본)
- [ ] ChapterNodeUI 인스펙터 필드 전부 연결
- [ ] Chapter Display Name 각각 입력
- [ ] **StageSelectPanel 기본 Active OFF** ← 중요!
- [ ] StageSelectPanel 인스펙터 필드 전부 연결 (버튼 3개 + PanelRoot + CloseButton)
- [ ] PanelBackground Raycast Target **ON** (패널 뒤 클릭 차단)
- [ ] MapSceneController 빈 GameObject 생성 및 필드 연결 (3개 노드 + StageSelectPanel + KingdomButton)
- [ ] KingdomButton On Click() → `MapSceneController.OnKingdomButton` 연결
- [ ] Kingdom Scene Name = `"KingdomScene"`, Battle Scene Name = `"BattleScene"` 입력
- [ ] 배틀 클리어 처리 코드에 `lastClearedStageIndex` 저장 로직 추가 (STEP 8)
- [ ] Build Settings에 `MapScene` / `BattleScene` / `KingdomScene` 등록
