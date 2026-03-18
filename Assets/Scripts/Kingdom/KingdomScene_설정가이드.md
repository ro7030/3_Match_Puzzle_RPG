# KingdomScene 설정 가이드

캐릭터를 배치하고 스탯·스킬·장비 화면으로 이동하는 **왕국 허브 씬**입니다.  
배틀 씬에 들어가기 전 플레이어가 파티를 정비하는 공간으로 사용합니다.

---

## 구조 한눈에 보기

```
[KingdomScene]
  Canvas
    Background              ← 왕국 배경 이미지
    CharacterGroup          ← 캐릭터 4명 (Sprite)
      CharacterSlot_0       ← 캐릭터 0 (검사)
      CharacterSlot_1       ← 캐릭터 1 (마법사)
      CharacterSlot_2       ← 캐릭터 2 (궁수)
      CharacterSlot_3       ← 캐릭터 3 (힐러)
    TopBar
      BackButton            ← 왼쪽 상단 ↩ 버튼 (MapScene으로)
      SettingsButton        ← 오른쪽 상단 ⚙ 버튼
    MenuPanel               ← 중앙 메뉴 버튼 3개
      StatButton            ← "스탯" → StatScene
      SkillButton           ← "스킬" → SkillScene
      EquipButton           ← "장비" → EquipScene
  EventSystem
  KingdomSceneController    ← 씬 전환 담당 (빈 GameObject)
```

### 씬 흐름

```
MapScene (스테이지 선택)
  └─ KingdomScene (왕국 허브)
        ├─ [스탯 버튼]  → StatScene  → 돌아오기 → KingdomScene
        ├─ [스킬 버튼]  → SkillScene → 돌아오기 → KingdomScene
        ├─ [장비 버튼]  → EquipScene → 돌아오기 → KingdomScene
        └─ [뒤로 버튼]  → MapScene
```

---

## STEP 1: 씬 생성

> **File > New Scene** → 이름을 `KingdomScene` 으로 저장  
> `Assets/Scenes/` 폴더에 저장 권장

---

## STEP 2: Canvas 기본 설정

| 항목 | 값 |
|------|----|
| Canvas Scaler / UI Scale Mode | **Scale With Screen Size** |
| Reference Resolution | **1080 × 1920** |
| Match | **0.5** (Width ↔ Height 중간) |

---

## STEP 3: Hierarchy 구성

### 3-1. Background

1. Canvas 아래 **UI > Image** 생성 → 이름: `Background`
2. Rect Transform: **Stretch 전체** (앵커 사방 끝으로)
3. Source Image: 왕국 배경 스프라이트 할당
4. Preserve Aspect: 배경 비율에 맞게 조정

---

### 3-2. CharacterGroup (캐릭터 4명)

1. Canvas 아래 **빈 GameObject** 생성 → 이름: `CharacterGroup`
2. 아래 4개의 자식 **UI > Image** 생성

| 이름 | 앵커 기준 위치 | 설명 |
|------|--------------|------|
| `CharacterSlot_0` | 왼쪽 1번째 | 검사 스프라이트 |
| `CharacterSlot_1` | 왼쪽 2번째 | 마법사 스프라이트 |
| `CharacterSlot_2` | 왼쪽 3번째 | 궁수 스프라이트 |
| `CharacterSlot_3` | 오른쪽 끝 | 힐러 스프라이트 |

> 각 Slot의 **Preserve Aspect** 체크 → 캐릭터 비율 유지  
> **Raycast Target** 은 `false` 로 설정 (버튼 클릭 방해 방지)

---

### 3-3. TopBar (뒤로가기 / 설정)

1. Canvas 아래 **빈 GameObject** → 이름: `TopBar`
2. Rect Transform: 화면 상단 전체 너비, 높이 약 120px

#### BackButton (왼쪽 상단 ↩)

| 항목 | 값 |
|------|----|
| 컴포넌트 | Button + Image |
| Anchor | 좌상단 |
| Pivot | (0, 1) |
| Position | X: 30, Y: -30 |
| Width / Height | 90 / 90 |
| Source Image | 뒤로가기 아이콘 스프라이트 |

#### SettingsButton (오른쪽 상단 ⚙)

| 항목 | 값 |
|------|----|
| 컴포넌트 | Button + Image |
| Anchor | 우상단 |
| Pivot | (1, 1) |
| Position | X: -30, Y: -30 |
| Width / Height | 90 / 90 |
| Source Image | 설정 아이콘 스프라이트 |

---

### 3-4. MenuPanel (스탯 / 스킬 / 장비)

1. Canvas 아래 **빈 GameObject** → 이름: `MenuPanel`
2. Rect Transform: 화면 중앙 상단 부근 배치 (이미지 참고)
3. **Vertical Layout Group** 컴포넌트 추가

| Layout Group 항목 | 값 |
|-------------------|----|
| Spacing | 10 |
| Child Alignment | Middle Center |
| Control Child Size | Width ✓, Height ✓ |
| Child Force Expand | Width ✗, Height ✗ |

MenuPanel 아래 Button 3개 생성:

| 이름 | Button Text | 이동 씬 |
|------|------------|---------|
| `StatButton` | 스탯 (Stat) | `StatScene` |
| `SkillButton` | 스킬 (Skill) | `SkillScene` |
| `EquipButton` | 장비 (Inventory) | `EquipScene` |

각 버튼 권장 크기: **Width 200 / Height 60**

---

## STEP 4: KingdomSceneController 스크립트

`Assets/Scripts/Kingdom/` 폴더를 만들고 아래 스크립트를 생성합니다.

```csharp
// Assets/Scripts/Kingdom/KingdomSceneController.cs
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Match3Puzzle.Kingdom
{
    public class KingdomSceneController : MonoBehaviour
    {
        [Header("이동할 씬 이름")]
        [SerializeField] private string statSceneName  = "StatScene";
        [SerializeField] private string skillSceneName = "SkillScene";
        [SerializeField] private string equipSceneName = "EquipScene";
        [SerializeField] private string backSceneName  = "MapScene";

        public void OnStatButton()  => SceneManager.LoadScene(statSceneName);
        public void OnSkillButton() => SceneManager.LoadScene(skillSceneName);
        public void OnEquipButton() => SceneManager.LoadScene(equipSceneName);
        public void OnBackButton()  => SceneManager.LoadScene(backSceneName);
        public void OnSettingsButton()
        {
            // TODO: OptionsPanel 활성화 또는 SettingsScene 전환
            Debug.Log("[KingdomScene] 설정 버튼 클릭");
        }
    }
}
```

---

## STEP 5: 버튼 이벤트 연결

Hierarchy에서 `KingdomSceneController` 빈 GameObject를 만들고  
위 스크립트를 부착한 뒤 각 버튼 On Click() 에 연결합니다.

| 버튼 | On Click() 함수 |
|------|----------------|
| `StatButton` | `KingdomSceneController.OnStatButton` |
| `SkillButton` | `KingdomSceneController.OnSkillButton` |
| `EquipButton` | `KingdomSceneController.OnEquipButton` |
| `BackButton` | `KingdomSceneController.OnBackButton` |
| `SettingsButton` | `KingdomSceneController.OnSettingsButton` |

---

## STEP 6: Build Settings에 씬 추가

**File > Build Settings** 에서 아래 씬들을 등록합니다.

| 씬 이름 | 비고 |
|---------|------|
| `KingdomScene` | 이번 가이드 |
| `StatScene` | 스탯 업그레이드 씬 (별도 제작) |
| `SkillScene` | 스킬 장착 씬 (별도 제작) |
| `EquipScene` | 장비 씬 (별도 제작) |

> 씬 이름이 스크립트의 `*SceneName` 문자열과 **정확히 일치**해야 합니다.

---

## STEP 7: 하위 씬(StatScene / SkillScene / EquipScene) 공통 규칙

각 씬에서 KingdomScene으로 돌아올 때 공통으로 사용합니다.

```csharp
// 뒤로가기 버튼 공통 패턴
SceneManager.LoadScene("KingdomScene");
```

| 씬 | 읽어야 할 데이터 | 저장해야 할 데이터 |
|----|----------------|------------------|
| `StatScene` | `SaveSystem.Load().statUpgradeLevels` | `statUpgradeLevels` (업그레이드 후 Save) |
| `SkillScene` | `SaveSystem.Load().unlockedSkillIds` | `EquippedSkillsHolder.EquipSkill()` |
| `EquipScene` | 장비 데이터 (미구현 시 placeholder) | 장비 장착 결과 Save |

---

## 씬 전체 Hierarchy 최종 예시

```
KingdomScene (씬)
  Main Camera
  EventSystem
  Canvas
    Background
    CharacterGroup
      CharacterSlot_0
      CharacterSlot_1
      CharacterSlot_2
      CharacterSlot_3
    TopBar
      BackButton
      SettingsButton
    MenuPanel
      StatButton
      SkillButton
      EquipButton
  KingdomSceneController   ← KingdomSceneController.cs 부착
```

---

## 체크리스트

- [ ] Canvas Scaler → Scale With Screen Size (1080×1920)
- [ ] Background 이미지 할당 및 Stretch 설정
- [ ] CharacterSlot 0~3 스프라이트 할당, Raycast Target OFF
- [ ] BackButton / SettingsButton 앵커·피벗 정확히 설정
- [ ] MenuPanel에 Vertical Layout Group 적용
- [ ] KingdomSceneController.cs 생성 및 씬 이름 입력
- [ ] 버튼 5개 On Click() 이벤트 전부 연결
- [ ] Build Settings에 KingdomScene / StatScene / SkillScene / EquipScene 등록
- [ ] 각 하위 씬에서 "KingdomScene" 으로 돌아오는 뒤로가기 버튼 구현
