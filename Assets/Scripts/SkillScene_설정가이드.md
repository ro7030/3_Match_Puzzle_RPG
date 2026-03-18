# SkillScene 설정 가이드

캐릭터 4명의 스킬을 확인하고 **장착**할 수 있는 씬입니다.

### 스킬 시스템 규칙
- 상단: 캐릭터 4명의 초상화 + 각 캐릭터별 **현재 장착 스킬** 아이콘
- **장착 스킬 아이콘 클릭** → 해당 캐릭터의 스킬 선택 화면(패널) 표시
- 하단: 선택 가능한 스킬 목록(3개 슬롯). **클릭 시** 해당 스킬을 현재 캐릭터에 장착
- 선택한 스킬 클릭 시 아래에 **작은 스킬 아이콘 + 스킬 이름(displayName) + 스킬 설명(description)** 표시
- `unlockedSkillIds`에 포함된 스킬만 장착 가능
- **두 번째 스킬**: 챕터1 클리어 이후 장착 가능 (`SkillData.requiredChapter = 2`)
- **세 번째 스킬**: 챕터2 클리어 이후 장착 가능 (`SkillData.requiredChapter = 3`)

---

## 0. 자주 묻는 질문 (설정 전 확인)

### Q1. 스킬 아이콘을 UI에서 직접 할당해야 하나요?
**아니요.** 아이콘은 **SkillData ScriptableObject**에만 설정하면 됩니다.  
`SkillSceneController`가 `SkillDatabase`에서 스킬 데이터를 불러올 때 `SkillData.icon`을 자동으로 사용합니다.  
→ **SkillDatabase에 등록된 각 SkillData 에셋**의 Inspector에서 **Icon** 필드에 스프라이트를 할당하세요.

### Q2. 검사/궁수/마법사/힐러 패널을 4개 따로 만들어야 하나요?
**아니요.** 패널은 **1개**만 있으면 됩니다.  
캐릭터 슬롯을 클릭하면, 그 캐릭터의 스킬만 `SkillDatabase.GetAllForCharacter(characterIndex)`로 가져와서 같은 패널에 동적으로 표시합니다.  
`SkillData.ownerCharacterIndex`(0=검사, 1=궁수, 2=마법사, 3=힐러)로 자동 필터링됩니다.

### Q3. 현재 장착 중인 스킬이 캐릭터 초상화 아래에 나오나요?
**네.** `CharacterSkillSlotUI`가 `Refresh(character, equippedSkill)` 호출 시 `skillIconImage`에 장착 스킬 아이콘을 표시합니다.  
**Hierarchy에서 Portrait를 위에, SkillIconButton을 아래에** 배치하면 초상화 아래에 장착 스킬이 나옵니다. (Vertical Layout Group 사용 권장)

---

## 1. 관련 스크립트 개요

| 스크립트 | 위치 | 역할 |
|----------|------|------|
| `SkillSceneController` | `Scripts/Skill/` | 씬 전체 초기화·갱신·스킬 장착 처리 |
| `CharacterSkillSlotUI` | `Scripts/Skill/` | 캐릭터 1개 슬롯 (초상화 + 장착 스킬 아이콘, 클릭 시 선택 패널 열기) |
| `SkillSelectionSlotUI` | `Scripts/Skill/` | 스킬 선택 슬롯 (클릭 시 장착, 아래에 이름·효과 표시) |
| `EquippedSkillsHolder` | `Scripts/Skill/` | 캐릭터별 장착 스킬 ID 런타임 보관 + 세이브 연동 |
| `SkillDatabase` | `Scripts/Skill/` | skillId → SkillData 조회 |
| `SkillData` | `Scripts/Skill/` | 스킬 아이콘·이름·효과 ScriptableObject |
| `CharacterDatabase` | `Scripts/Story/` | chrID → DataCharacter 조회 |
| `SaveSystem` | `Scripts/MainMenu/` | JSON 세이브·로드 |

---

## 2. 씬 진입 경로

```
KingdomScene → 스킬 버튼 → SkillScene
```

`KingdomSceneController`의 `skillSceneName` 필드가 `"SkillScene"`이면 자동 연결됩니다.  
SkillScene에서 뒤로가기 버튼을 누르면 `"KingdomScene"`으로 복귀합니다.

---

## 3. 스킬 장착 흐름

```
[SkillScene 진입]
        ↓
SkillSceneController.Start()
  ├── SaveSystem.Load()                    → GameSaveData 로드
  ├── EquippedSkillsHolder.LoadFromSave()  → 장착 스킬 복원
  └── RefreshAll()                         → 상단 4개 슬롯 갱신

[상단 장착 스킬 아이콘 클릭]
        ↓
OnCharacterSlotClicked(characterIndex)
  └── SkillSelectionPanel 활성화
  └── 해당 캐릭터의 선택 가능 스킬 목록 표시 (하단 3개 슬롯)

[하단 스킬 슬롯 클릭]
        ↓
OnSkillSelected(skillId)
  ├── EquippedSkillsHolder.EquipSkill(characterIndex, skillId)
  ├── GameSaveData.equippedSkillIds 갱신 → SaveSystem.Save()
  ├── SkillSelectionPanel 비활성화
  └── RefreshAll()                         → 상단 슬롯 갱신
```

---

## 4. 슬롯 인덱스 매핑

| 캐릭터 인덱스 | 설명 | CharacterDatabase chrID |
|:-------------:|------|-------------------------|
| 0 | 첫 번째 캐릭터 (검사) | `characterIds[0]` = 0 |
| 1 | 두 번째 캐릭터 (마법사) | `characterIds[1]` = 2 |
| 2 | 세 번째 캐릭터 (궁수) | `characterIds[2]` = 1 |
| 3 | 네 번째 캐릭터 (힐러) | `characterIds[3]` = 3 |

---

## 5. Hierarchy 구조

### 5-1. 씬 루트

| Hierarchy 이름 | 만드는 것 | 비고 |
|----------------|-----------|------|
| **Main Camera** | 씬 기본 카메라 | 씬 생성 시 자동 포함 |
| **EventSystem** | 씬 기본 이벤트 시스템 | UI 사용 시 필요 |
| **Canvas** | `UI > Canvas` | Screen Space - Overlay 권장 |

---

### 5-2. Canvas 자식

#### ① SkillController (로직 오브젝트)

| Hierarchy 이름 | 만드는 것 | 스크립트 |
|----------------|-----------|----------|
| **SkillController** | **빈 오브젝트** (Canvas 자식) | `SkillSceneController` 붙임 |

`SkillSceneController` Inspector 설정:

| 필드 | 연결 대상 | 기본값 |
|------|-----------|--------|
| Character Database | `CharacterDatabase` ScriptableObject | — |
| Skill Database | `SkillDatabase` ScriptableObject | — |
| Character Slots (크기 4) | 각 `CharacterSkillSlotUI` 컴포넌트 | 순서: 0~3 |
| Character Ids | 각 슬롯에 대응하는 chrID 배열 | `[0, 1, 2, 3]` |
| Skill Selection Panel | 스킬 선택 패널 GameObject | 클릭 시 표시/숨김 |
| Skill Selection Slots | `SkillSelectionSlotUI` 배열 (3개) | 선택 가능 스킬 (해당 캐릭터 스킬) |
| Skill Info Icon Image | 스킬 설명 왼쪽 작은 아이콘 Image | 클릭한 스킬의 아이콘 자동 표시 |
| Skill Name Text | 선택한 스킬 이름 TMP | "빛의 심판" 등 |
| Skill Effect Text | 선택한 스킬 설명 TMP | `SkillData.description` 내용 (비어 있으면 자동 효과 문구) |
| Back Button | 뒤로가기 Button | — |
| Back Scene Name | 복귀 씬 이름 | `"KingdomScene"` |

---

#### ② 배경

| Hierarchy 이름 | 만드는 것 | 비고 |
|----------------|-----------|------|
| **Background** | **Image** (Canvas 자식) | 배경 스프라이트. Stretch로 전체 화면 |

---

#### ③ 상단 바

| Hierarchy 이름 | 만드는 것 | 비고 |
|----------------|-----------|------|
| **TopBar** | **빈 오브젝트** (Canvas 자식) | |
| **TopBar/RefreshButton** | **Button** (선택) | 새로고침 아이콘 |
| **TopBar/SettingsButton** | **Button** (선택) | 설정 아이콘 |
| **TopBar/BackButton** | **Button** | **Back Button**에 연결 |

---

#### ④ CharacterSkillGrid (캐릭터 + 스킬 슬롯 4개)

상단에 캐릭터 4명 + 각 캐릭터의 장착 스킬 아이콘을 가로로 배치합니다.

| Hierarchy 이름 | 만드는 것 | 스크립트 |
|----------------|-----------|----------|
| **CharacterSkillGrid** | **빈 오브젝트** (Canvas 자식) | Horizontal Layout Group 권장 |
| **CharacterSkillGrid/Slot_0** | **빈 오브젝트** | `CharacterSkillSlotUI` 붙임 |
| **CharacterSkillGrid/Slot_1** | **빈 오브젝트** | `CharacterSkillSlotUI` 붙임 |
| **CharacterSkillGrid/Slot_2** | **빈 오브젝트** | `CharacterSkillSlotUI` 붙임 |
| **CharacterSkillGrid/Slot_3** | **빈 오브젝트** | `CharacterSkillSlotUI` 붙임 |

##### 각 Slot 내부 구조 (Slot_0 기준)

| Hierarchy 이름 | 만드는 것 | CharacterSkillSlotUI 필드 |
|----------------|-----------|---------------------------|
| **Slot_0/Portrait** | **Image** | **Portrait Image** |
| **Slot_0/SkillIconButton** | **Button** (자식에 Image) | **Skill Icon Button** |
| **Slot_0/SkillIconButton/Icon** | **Image** (Button 자식) | 스킬 아이콘 스프라이트 |

> 클릭 시 스킬 선택 패널이 열리도록 `SkillSceneController`에서 콜백 연결

---

#### ⑤ SkillSelectionPanel (스킬 선택 패널)

상단 슬롯 클릭 시 나타나는 패널. 하단에 선택 가능 스킬 3개 + 스킬 정보 표시.

| Hierarchy 이름 | 만드는 것 | 비고 |
|----------------|-----------|------|
| **SkillSelectionPanel** | **빈 오브젝트** 또는 **Image** (패널 배경) | 기본 비활성화, 클릭 시 활성화 |
| **SkillSelectionPanel/SkillSlots** | **빈 오브젝트** | Horizontal Layout Group |
| **SkillSlots/Slot_0** | **Button** + **Image** | `SkillSelectionSlotUI` 붙임 |
| **SkillSlots/Slot_1** | **Button** + **Image** | `SkillSelectionSlotUI` 붙임 |
| **SkillSlots/Slot_2** | **Button** + **Image** | `SkillSelectionSlotUI` 붙임 |
| **SkillSelectionPanel/SkillInfo** | **빈 오브젝트** | Horizontal Layout Group 권장 (아이콘 왼쪽, 텍스트 오른쪽) |
| **SkillInfo/SkillIconImage** | **Image** | 클릭한 스킬 아이콘 (작게 표시). **Skill Info Icon Image**에 연결 |
| **SkillInfo/SkillNameText** | **Text - TMP** | 스킬 이름 (`displayName`) |
| **SkillInfo/SkillEffectText** | **Text - TMP** | 스킬 설명 (`description`) |
| **SkillSelectionPanel/CloseButton** | **Button** (선택) | 클릭 시 `SkillSceneController.CloseSkillSelectionPanel()` 호출 |

> **SkillInfo 레이아웃**: SkillIconImage를 왼쪽에 작게 (예: 48×48), Name·Effect 텍스트를 오른쪽에 배치.  
> Horizontal Layout Group 사용 시 Child Alignment = Middle Left, Spacing = 8 권장.

---

## 6. 스킬 해금 조건 (requiredChapter)

| requiredChapter | 해금 시점 |
|-----------------|-----------|
| 0 | 항상 장착 가능 (첫 번째 스킬) |
| 2 | 챕터1 클리어 이후 (두 번째 스킬) |
| 3 | 챕터2 클리어 이후 (세 번째 스킬) |

`lastClearedChapter`가 `requiredChapter` 이상일 때만 해당 스킬이 선택 목록에 표시됩니다.

---

## 7. 스킬 이름·설명 표시

| 표시 항목 | 출처 |
|-----------|------|
| 스킬 이름 | `SkillData.displayName` |
| 스킬 설명 | `SkillData.description` (비어 있으면 effectType·effectMultiplier 기반 자동 문구) |

`SkillData` Inspector에서 **Description** 필드에 스킬 설명을 입력하세요 (예: "500%의 광역 피해").

---

## 8. 기본 장착 스킬 (미장착 시)

스킬을 장착하지 않았을 때 자동으로 적용되는 기본 스킬 (SkillDatabase 배열 인덱스 기준):

| 캐릭터 인덱스 | 기본 스킬 인덱스 |
|:-------------:|:----------------:|
| 0 (검사) | 0 |
| 1 (마법사) | 3 |
| 2 (궁수) | 6 |
| 3 (힐러) | 9 |

→ SkillDatabase의 `skills` 배열에서 **0, 3, 6, 9번째** 스킬이 각 캐릭터의 기본 스킬입니다.

---

## 9. GameSaveData 신규 필드

| 필드 | 타입 | 설명 |
|------|------|------|
| `equippedSkillIds` | `string[]` (크기 4) | 캐릭터 0~3별 장착 스킬 ID. 빈 문자열 = 미장착 |

기존 `unlockedSkillIds`는 해금된 스킬 목록으로 유지됩니다.

---

## 10. 자주 묻는 설정 문제

| 증상 | 원인 | 해결 방법 |
|------|------|-----------|
| 초상화가 표시되지 않음 | `CharacterDatabase` 미연결 또는 `characterIds` 잘못됨 | Database에 해당 ID 캐릭터 등록 확인 |
| 스킬 아이콘 안 나옴 | `SkillDatabase` 미연결 또는 skillId 불일치 | SkillDatabase에 SkillData 등록, skillId 일치 확인 |
| 스킬 선택 패널이 안 열림 | `SkillSelectionPanel` 미연결 | Inspector에서 Panel 연결 |
| 장착해도 배틀에서 안 나옴 | `EquippedSkillsHolder` 미저장 | `SaveSystem.Save()` 호출 및 `equippedSkillIds` 저장 확인 |
| 해금되지 않은 스킬 장착됨 | `unlockedSkillIds` 체크 누락 | `SkillSceneController`에서 unlockedSkillIds 포함 여부 확인 |
| 두/세 번째 스킬이 안 보임 | `requiredChapter` 또는 `lastClearedChapter` | SkillData에 requiredChapter 설정, 챕터 클리어 후 확인 |

---

## 11. Build Settings 확인

`File > Build Settings > Scenes In Build`에 `SkillScene`을 추가해야  
`SceneManager.LoadScene("SkillScene")`이 정상 작동합니다.
