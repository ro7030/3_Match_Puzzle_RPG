# SkillScene 설정 가이드 (재구성 버전)

이 가이드는 **Hierarchy를 전부 지우고**, 참고 이미지 형태로 SkillScene을 다시 만드는 방식입니다.
핵심은 **오브젝트 구조는 고정**하고, 이후에는 **Image Sprite만 교체**해서 작업하는 것입니다.

---

## 1) 한 번에 씬 재구성

1. `Assets/Scenes/SkillScene.unity`를 연다.
2. 상단 메뉴에서 아래를 실행한다.

`MATCH3 > Skill Scene > 씬 전체 재구성 (이미지 교체형)`

실행하면 현재 씬 루트 오브젝트를 모두 삭제하고, 아래 구조를 자동 생성합니다.

- `Main Camera`
- `EventSystem`
- `Canvas`
  - `Background`
  - `TopBar`
  - `CharacterSkillGrid`
  - `SkillSelectionPanel`
  - `SkillInfo`
  - `SkillController`

---

## 2) 자동 생성되는 캐릭터 슬롯 구조

각 슬롯(`Slot_0~Slot_3`)은 아래 순서로 생성됩니다.

- `Highlight` (뒤, 기본 비활성)
- `HoverRoot`
  - `Portrait`
  - `BottomMask`
- `SkillIconButton`
  - `Icon`

즉, 참고 이미지 기준 레이어는 아래와 같습니다.

1. 뒤: `Highlight`
2. 중간: `Portrait` + `BottomMask`
3. 앞: `SkillIconButton/Icon` (장착 스킬 아이콘)

---

## 3) 캐릭터 순서

요청대로 슬롯 순서는 `CharacterDatabase`의 ID 순서와 동일합니다.

- `Slot_0` → `chrID 0`
- `Slot_1` → `chrID 1`
- `Slot_2` → `chrID 2`
- `Slot_3` → `chrID 3`

`SkillSceneController.characterIds`는 자동으로 `[0,1,2,3]`로 설정됩니다.
캐릭터 클릭 시 `SkillSlots0~3` 중 같은 번호 그룹만 활성화됩니다. (`Slot_0` 클릭 → `SkillSlots0`)

---

## 4) 이제 이미지 교체만 하면 되는 위치

### 배경
- `Canvas/Background`

### 캐릭터별(4개 슬롯 공통)
- `Canvas/CharacterSkillGrid/Slot_X/Highlight`
- `Canvas/CharacterSkillGrid/Slot_X/HoverRoot/Portrait`
- `Canvas/CharacterSkillGrid/Slot_X/HoverRoot/BottomMask`
- `Canvas/CharacterSkillGrid/Slot_X/SkillIconButton/Icon`

### 하단 스킬 아이콘 3개
- `Canvas/SkillSelectionPanel/SkillSlots0/Slot_0/Icon`
- `Canvas/SkillSelectionPanel/SkillSlots0/Slot_1/Icon`
- `Canvas/SkillSelectionPanel/SkillSlots0/Slot_2/Icon`
- `Canvas/SkillSelectionPanel/SkillSlots1/Slot_0/Icon`
- `Canvas/SkillSelectionPanel/SkillSlots1/Slot_1/Icon`
- `Canvas/SkillSelectionPanel/SkillSlots1/Slot_2/Icon`
- `Canvas/SkillSelectionPanel/SkillSlots2/Slot_0/Icon`
- `Canvas/SkillSelectionPanel/SkillSlots2/Slot_1/Icon`
- `Canvas/SkillSelectionPanel/SkillSlots2/Slot_2/Icon`
- `Canvas/SkillSelectionPanel/SkillSlots3/Slot_0/Icon`
- `Canvas/SkillSelectionPanel/SkillSlots3/Slot_1/Icon`
- `Canvas/SkillSelectionPanel/SkillSlots3/Slot_2/Icon`

---

## 5) 흑백 이미지 넣는 위치

흑백 초상화는 씬 Image에 직접 넣지 말고, 캐릭터 데이터에서 지정합니다.

- `Assets/Resources/ScriptableObjects/Story/Characters/Chr_*.asset`
  - `Chr Image` = 컬러
  - `Chr Image Grayscale` = 흑백

미선택 상태에서는 `Chr Image Grayscale`, 선택 시 `Chr Image`를 사용합니다.

---

## 6) 중요 체크

- 알파 히트 테스트를 쓰는 이미지(초상화/스킬 아이콘)는 스프라이트 Import에서 `Read/Write Enabled`를 켜야 합니다.
- 재구성 메뉴 실행 후에는 씬 저장(`Ctrl+S`)을 해주세요.

---

## 7) 관련 스크립트

- `Assets/Scripts/Skill/Editor/SkillSceneRebuildTool.cs`  
  씬 전체 재구성 메뉴 스크립트
- `Assets/Scripts/Skill/SkillSceneController.cs`  
  캐릭터 선택/스킬 장착/하단 정보 표시
- `Assets/Scripts/Skill/CharacterSkillSlotUI.cs`  
  캐릭터 슬롯(호버, 선택, 글로우)
- `Assets/Scripts/Skill/SkillSelectionSlotUI.cs`  
  하단 스킬 슬롯(호버 설명, 클릭 장착)
