# StatusScene 설정 가이드

캐릭터 4명의 스탯을 확인하고 **업그레이드 포인트**를 소모해 **Lv UP**할 수 있는 씬입니다.

### 레벨 시스템 규칙
- 신규 스테이지를 클리어할 때마다 **플레이어 레벨 +1**, **업그레이드 포인트 +1**
- StatusScene에서 Lv UP 버튼을 누르면 **업그레이드 포인트 -1**, 해당 캐릭터 스탯 레벨 +1
- 포인트가 0이면 모든 Lv UP 버튼이 비활성화됨
- 골드는 아이템 구매 전용으로 별도 관리

---

## 1. 관련 스크립트 개요

| 스크립트 | 위치 | 역할 |
|----------|------|------|
| `StatusSceneController` | `Scripts/Status/` | 씬 전체 초기화·갱신·업그레이드 처리 |
| `CharacterStatusSlotUI` | `Scripts/Status/` | 캐릭터 1개 슬롯 UI (초상화·수치·Lv UP 버튼) |
| `CharacterUpgradeHolder` | `Scripts/Stats/` | 스탯 업그레이드 레벨 런타임 정적 보관 |
| `CharacterStatsResolver` | `Scripts/Stats/` | `base + level × bonus` 최종 수치 계산 |
| `CharacterStatsData` | `Scripts/Stats/` | 기본 수치·레벨당 증가량 ScriptableObject |
| `CharacterDatabase` | `Scripts/Story/` | chrID → DataCharacter 조회 |
| `UIManager` | `Scripts/UI/` | 스테이지 클리어 시 레벨·포인트 지급 |
| `GameManager` | `Scripts/Core/` | UIManager 없을 때 동일 처리 (fallback) |
| `SaveSystem` | `Scripts/MainMenu/` | JSON 세이브·로드 |

---

## 2. 씬 진입 경로

```
KingdomScene → Status 버튼 → StatusScene
```

`KingdomSceneController`의 `statusSceneName` 필드가 `"StatusScene"`이면 자동 연결됩니다.  
StatusScene에서 뒤로가기 버튼을 누르면 `"KingdomScene"`으로 복귀합니다.

---

## 3. 레벨 & 업그레이드 포인트 흐름

```
[배틀씬: 스테이지 클리어]
        ↓
UIManager.SaveClearedStage()
  └── 신규 클리어인 경우만
        ├── saveData.playerLevel++
        └── saveData.upgradePoints++
        → SaveSystem.Save() 즉시 기록

[StatusScene 진입]
        ↓
StatusSceneController.Start()
  ├── SaveSystem.Load()               → GameSaveData 로드
  ├── CharacterUpgradeHolder.LoadFromSave()  → 스탯 레벨 복원
  └── RefreshAll()                    → 슬롯 4개 갱신

        upgradePoints > 0  →  Lv UP 버튼 활성화
        upgradePoints == 0 →  Lv UP 버튼 전부 비활성화

[Lv UP 버튼 클릭]
        ↓
OnLevelUp(statIndex)
  ├── 포인트 0 → 로그 출력 후 종료
  ├── saveData.upgradePoints--
  ├── CharacterUpgradeHolder.SwordLevel++ (또는 Bow/Wand/Heal)
  ├── CharacterUpgradeHolder.ApplyToSave()
  ├── SaveSystem.Save()               → 즉시 저장
  └── RefreshAll()                    → UI 재갱신
```

---

## 4. 슬롯 인덱스 매핑

| 슬롯 인덱스 | 스탯 | 퍼즐 타일 | CharacterUpgradeHolder |
|:-----------:|------|-----------|------------------------|
| 0 | 검 데미지 | 검 타일 매칭 | `SwordLevel` |
| 1 | 활 데미지 | 활 타일 매칭 | `BowLevel` |
| 2 | 마법 데미지 | 지팡이 타일 매칭 | `WandLevel` |
| 3 | 회복량 | 십자가 타일 매칭 | `HealLevel` |

---

## 5. Hierarchy 구조

아래는 **만드는 방법** 기준입니다.

- **빈 오브젝트**: `GameObject > Create Empty`
- **Image**: `UI > Image` (Canvas 자식)
- **Button**: `UI > Button - TextMeshPro`
- **Text (TMP)**: `UI > Text - TextMeshPro`

### 5-1. 씬 루트

| Hierarchy 이름 | 만드는 것 | 비고 |
|----------------|-----------|------|
| **Main Camera** | 씬 기본 카메라 | 씬 생성 시 자동 포함 |
| **EventSystem** | 씬 기본 이벤트 시스템 | UI 사용 시 필요 |
| **Canvas** | `UI > Canvas` | Screen Space - Overlay 권장 |

---

### 5-2. Canvas 자식

#### ① StatusController (로직 오브젝트)

| Hierarchy 이름 | 만드는 것 | 스크립트 |
|----------------|-----------|----------|
| **StatusController** | **빈 오브젝트** (Canvas 자식) | `StatusSceneController` 붙임 |

`StatusSceneController` Inspector 설정:

| 필드 | 연결 대상 | 기본값 |
|------|-----------|--------|
| Character Database | `CharacterDatabase` ScriptableObject | — |
| Stats Data | `CharacterStats` ScriptableObject | 비워 두면 Resources 자동 로드 |
| Slots (크기 4) | 각 `CharacterStatusSlotUI` 컴포넌트 | 순서 중요 (0=검, 1=활, 2=마법, 3=힐) |
| Character Ids | 각 슬롯에 대응하는 chrID 배열 | `[0, 1, 2, 3]` |
| Player Level Text | 플레이어 레벨 표시 TMP | 선택 |
| Upgrade Points Text | 남은 포인트 표시 TMP | 선택 |
| Back Button | 뒤로가기 Button | — |
| Back Scene Name | 복귀 씬 이름 | `"KingdomScene"` |

---

#### ② 배경

| Hierarchy 이름 | 만드는 것 | 비고 |
|----------------|-----------|------|
| **Background** | **Image** (Canvas 자식) | 배경 스프라이트 할당. Stretch로 전체 화면 |

---

#### ③ 상단 바 (플레이어 정보)

| Hierarchy 이름 | 만드는 것 | 비고 |
|----------------|-----------|------|
| **TopBar** | **빈 오브젝트** (Canvas 자식) | |
| **TopBar/TitleText** | **Text - TMP** (TopBar 자식) | `"Status"` 등 타이틀 |
| **TopBar/PlayerLevelText** | **Text - TMP** (TopBar 자식) | `StatusSceneController`의 **Player Level Text**에 연결 → `Lv.5` 형태로 자동 표시 |
| **TopBar/UpgradePointsText** | **Text - TMP** (TopBar 자식) | **Upgrade Points Text**에 연결 → `업그레이드 포인트: 2` 형태로 자동 표시 |
| **TopBar/BackButton** | **Button** (TopBar 자식) | **Back Button**에 연결 |

---

#### ④ CharacterGrid (캐릭터 슬롯 4개)

캐릭터 4명을 2×2 또는 원하는 배치로 배열합니다.  
**각 슬롯 오브젝트에 `CharacterStatusSlotUI` 스크립트를 붙이고**  
`StatusSceneController`의 `Slots` 배열에 순서대로 연결합니다.

| Hierarchy 이름 | 만드는 것 | 스크립트 |
|----------------|-----------|----------|
| **CharacterGrid** | **빈 오브젝트** (Canvas 자식) | Grid Layout Group 권장 |
| **CharacterGrid/Slot_0** | **빈 오브젝트** | `CharacterStatusSlotUI` 붙임 (검) |
| **CharacterGrid/Slot_1** | **빈 오브젝트** | `CharacterStatusSlotUI` 붙임 (활) |
| **CharacterGrid/Slot_2** | **빈 오브젝트** | `CharacterStatusSlotUI` 붙임 (마법) |
| **CharacterGrid/Slot_3** | **빈 오브젝트** | `CharacterStatusSlotUI` 붙임 (힐) |

##### 각 Slot 내부 구조 (Slot_0 기준, 나머지 동일)

| Hierarchy 이름 | 만드는 것 | CharacterStatusSlotUI 필드 |
|----------------|-----------|---------------------------|
| **Slot_0/Portrait** | **Image** | **Portrait Image** |
| **Slot_0/NameText** | **Text - TMP** | **Name Text** |
| **Slot_0/StatLabel** | **Text - TMP** | **Stat Label Text** (`검 데미지` 등 자동 입력) |
| **Slot_0/LevelText** | **Text - TMP** | **Level Text** (`Lv.0` 형태로 자동 갱신) |
| **Slot_0/CurrentValue** | **Text - TMP** | **Current Value Text** (현재 수치) |
| **Slot_0/NextValue** | **Text - TMP** (선택) | **Next Value Text** (`→ 40` 형태 미리보기) |
| **Slot_0/LvUpButton** | **Button** | **Level Up Button** (포인트 없으면 자동 비활성화) |

> **선택** 표시 필드는 연결하지 않아도 동작합니다.

---

## 6. 최종 수치 계산 공식

```
최종 수치 = base + level × bonusPerLevel
```

`CharacterStats` ScriptableObject (에셋 이름: `CharacterStats`, Resources 폴더 저장):

| 스탯 | 기본값 | 레벨당 증가 | 레벨 3 예시 |
|------|--------|------------|------------|
| 검 데미지 | `baseSwordDamage = 30` | `swordBonusPerLevel = 10` | 30 + 3×10 = **60** |
| 활 데미지 | `baseBowDamage = 80` | `bowBonusPerLevel = 20` | 80 + 3×20 = **140** |
| 마법 데미지 | `baseWandDamage = 35` | `wandBonusPerLevel = 12` | 35 + 3×12 = **71** |
| 회복량 | `baseHealAmount = 25` | `healBonusPerLevel = 8` | 25 + 3×8 = **49** |

---

## 7. GameSaveData 신규 필드

| 필드 | 타입 | 설명 |
|------|------|------|
| `playerLevel` | `int` | 클리어한 신규 스테이지 수 (= 총 획득 레벨) |
| `upgradePoints` | `int` | 현재 사용 가능한 업그레이드 포인트 |

기존 `statUpgradeLevels[]` 배열과 `gold` 필드는 그대로 유지됩니다.

---

## 8. 자주 묻는 설정 문제

| 증상 | 원인 | 해결 방법 |
|------|------|-----------|
| 초상화가 표시되지 않음 | `CharacterDatabase` 미연결 또는 `characterIds` 값이 없는 ID | Database에 해당 ID 캐릭터 등록 확인 |
| Lv UP을 눌러도 수치가 안 바뀜 | `CharacterStats` ScriptableObject 연결 누락 | Inspector에 연결하거나 `Resources/CharacterStats`에 에셋 저장 |
| Lv UP 버튼이 항상 비활성화 | `upgradePoints == 0` (포인트 없음) | 배틀씬에서 스테이지를 클리어하면 포인트 지급 |
| 스테이지 클리어해도 포인트 안 늘어남 | 같은 스테이지 재클리어 (신규만 지급) | 아직 클리어하지 않은 스테이지 진행 |
| 씬 전환 후 스탯 레벨이 초기화됨 | `CharacterUpgradeHolder.LoadFromSave()` 미호출 | `StatusSceneController.Start()`에 이미 포함. 다른 씬에서 스탯 참조 시 동일 호출 필요 |
| 배틀에서 업그레이드 수치가 반영 안 됨 | `MatchEffectHandler`에서 `CharacterStatsResolver` 미사용 | `CharacterStatsResolver.GetSwordDamage(statsData)` 등으로 참조 |

---

## 9. Build Settings 확인

`File > Build Settings > Scenes In Build`에 `StatusScene`을 추가해야  
`SceneManager.LoadScene("StatusScene")`이 정상 작동합니다.
