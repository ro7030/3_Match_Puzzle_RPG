# InventoryScene 설정 가이드

이미지 기준의 인벤토리/상점 씬입니다.  
좌측은 장비 슬롯 2개, 중앙은 구매 가능한 장비 카드 4개, 우측은 구매/장비 버튼 안내 및 실행 버튼으로 구성됩니다.

---

## 1. 씬 진입 경로
`KingdomScene`에서 인벤토리 버튼 클릭 시 `InventoryScene`으로 이동합니다.

---

## 2. 핵심 동작 규칙
- 좌측 슬롯은 2개만 사용 (`EquipmentSlot_0`, `EquipmentSlot_1`)
- 중앙 카드의 장착 대상 슬롯은 고정 매핑
  - `ShopItemCard_0`, `ShopItemCard_2` → `EquipmentSlot_0`
  - `ShopItemCard_1`, `ShopItemCard_3` → `EquipmentSlot_1`
- 중앙 카드 클릭 → 우측 버튼의 대상 스킬 선택
- 우측 버튼
  - 대상 스킬이 `unlockedSkillIds`에 있으면 `장비` → `EquippedSkillsHolder`에 장착
  - 없으면 `구매` → 골드 차감 후 `unlockedSkillIds`에 추가
- 장착 스킬 아이콘은 `SkillDatabase`에서 `EquippedSkillsHolder`의 id를 찾아 표시

### 장비 데이터 구조
- `EquipmentData`를 사용합니다.
- `equipmentId`, `displayName`, `description`, `icon`, `goldCost`, `requiredChapter`
- 효과 배율:
  - `partyMaxHpPercent` (파티 전체 최대 체력 증가 %)
  - `attackPercent` (검/활/마법 공격 증가 %)
  - `healPercent` (수녀 힐량 증가 %)

---

## 3. 관련 스크립트 개요
- `InventorySceneController` (`Assets/Scripts/Inventory/InventorySceneController.cs`)
  - 씬 전체 초기화/갱신/버튼 동작 담당
- `EquipmentSlotUI` (`Assets/Scripts/Inventory/EquipmentSlotUI.cs`)
  - 좌측 장비 슬롯 1개 UI 담당
- `SkillShopItemCardUI` (`Assets/Scripts/Inventory/SkillShopItemCardUI.cs`)
  - 중앙 장비 카드 1개 UI 담당 (클래스 이름은 유지했지만 내부는 장비 전용)
- `EquipmentData` / `EquipmentDatabase` / `EquippedEquipmentHolder` / `EquipmentStatModifier`
  - 장비 저장/장착/배틀 배율 반영

---

## 4. Hierarchy 구조(자동 탐색 기반)
`InventorySceneController`는 하위 오브젝트 이름을 기준으로 자동 연결합니다(Inspector 세팅 없이 동작).

### 4-1. 씬 루트
- `Main Camera` (기본)
- `EventSystem` (UI 버튼 입력 처리용)
- `Canvas` (UGUI 화면 구성)

### 4-2. Canvas 자식(권장)
- `InventorySceneController` (빈 오브젝트 + `InventorySceneController` 컴포넌트)
- `Background` (Image)

#### 좌측: Equipment Slots
- `EquipmentPanel`
  - `EquipmentSlot_0` (Button + Image + `EquipmentSlotUI`)
    - `SlotFrameImage` (Image)
    - `EquippedSkillIconImage` (Image)
  - `EquipmentSlot_1` (동일 구조)
  - `EquipmentSlot_2`, `EquipmentSlot_3`는 비활성(사용 안 함)

#### 중앙: Shop Cards
- `ShopPanel`
  - `ShopItemCard_0` ~ `ShopItemCard_3`
    - `ItemIconImage` (Image) 또는 `SkillIconImage` (레거시 이름)
    - `ItemNameText` (TextMeshProUGUI) 또는 `SkillNameText`
    - `ItemCostText` (TextMeshProUGUI) 또는 `SkillCostText`
    - `ItemDescriptionText` (TextMeshProUGUI, 선택)
    - root에 Button + `SkillShopItemCardUI`

#### 우측: Purchase/Equip
- `InfoPanel`
  - `PurchaseHelpText` (TextMeshProUGUI) : 이미지의 안내 문구 + 선택 스킬 정보
  - `ActionButton` (Button)
    - `ActionButtonText` (TextMeshProUGUI)
  - `GoldText` (TextMeshProUGUI) : 현재 골드 표시

---

## 5. 다음 단계(사용자 수행 필요할 수 있는 부분)
- 이미지와 100% 동일한 UI 스타일(배경 스프라이트/색/폰트/크기)은 수동 미세 조정이 필요합니다.
- 카드 개수를 늘리려면 `EquipmentDatabase`에 `EquipmentData` 에셋을 추가 등록해야 합니다.

