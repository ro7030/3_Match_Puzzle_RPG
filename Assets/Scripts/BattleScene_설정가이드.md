# 배틀 씬(Battle Scene) 전용 설정 가이드

사진과 같은 배틀 씬 구성을 위한 Hierarchy 구조와 스크립트 연결 방법입니다.

---

## 1. 배틀 씬 Hierarchy 구조

아래는 **만드는 방법** 기준으로 정리했습니다.  
- **빈 오브젝트**: `GameObject > Create Empty`  
- **Image**: `UI > Image` (Canvas 자식으로 만들면 RectTransform 자동)  
- **Button**: `UI > Button - TextMeshPro` 또는 `UI > Button`  
- **Text (TMP)**: `UI > Text - TextMeshPro`  
- **Slider**: `UI > Slider`  
- **스크립트만 붙임**: 해당 오브젝트에 컴포넌트로 스크립트 추가 (빈 오브젝트 또는 UI 오브젝트에 붙임)

---

### 1-1. 씬 루트

| Hierarchy 이름 | 만드는 것 | 비고 |
|----------------|-----------|------|
| **Main Camera** | 씬 기본 카메라 | 보통 씬 생성 시 있음 |
| **EventSystem** | 씬 기본 이벤트 시스템 | UI 사용 시 필요, 없으면 `GameObject > UI > Event System` |
| **Canvas** | `UI > Canvas` | Screen Space - Overlay, 배틀 UI 전체의 부모 |

---

### 1-2. GameManager (퍼즐 로직 루트)

| Hierarchy 이름 | 만드는 것 | 스크립트 |
|----------------|-----------|----------|
| **GameManager** | **빈 오브젝트** | Game Manager (Script), Battle Scene Starter (Script), Input Handler (Script), Match Detector (Script), Tile Swapper (Script), Tile Clearer (Script), Gravity Controller (Script), Tile Spawner (Script), **Match Effect Handler (Script)** (매칭 시 전투 효과), Score Manager (Script), Level Manager (Script) **전부 이 오브젝트에 붙임** |
| **GameBoard** | **빈 오브젝트** (GameManager 자식) | GameBoard (Script) 붙임. **Tile Slot Parent**에 100개 Tile이 있는 부모 연결 |

---

### 1-3. Canvas 자식 (배틀 UI)

| Hierarchy 이름 | 만드는 것 | 스크립트 |
|----------------|-----------|----------|
| **BattleSceneConfigApplier** | **빈 오브젝트** (Canvas 자식) | BattleSceneConfigApplier (Script) 붙임. 배경/몬스터/턴 적용용 |
| **Background** | **빈 오브젝트** (Canvas 자식) | 없음 |
| **Background/Image** | **Image** (`UI > Image`, Background 자식) | 없음. 풀스크린으로 키우고, 스프라이트는 ConfigApplier가 스테이지별로 할당 |

| **TurnUI** | **빈 오브젝트** (Canvas 자식) | BattleTurnUI (Script) 붙임 |
| **TurnUI/Icon** | **Image** (TurnUI 자식) | 없음. 모래시계 아이콘 스프라이트 할당 |
| **TurnUI/TurnText** | **Text - TextMeshPro** (TurnUI 자식) | 없음. BattleTurnUI의 Turn Text 참조로 연결 |

| **TopRightButtons** | **빈 오브젝트** (Canvas 자식) | 없음 |
| **TopRightButtons/BackButton** | **Button** (`UI > Button`, TopRightButtons 자식) | BattleBackButton (Script) 붙임. Target Scene Name에 스테이지 선택 씬 이름 지정 |
| **TopRightButtons/SettingsButton** | **Button** (TopRightButtons 자식) | 없음. 클릭 시 설정 팝업 등 연결 |

| **PartyPanel** | **빈 오브젝트** (Canvas 자식) | PartyHealthUI (Script) 붙임 |
| **PartyPanel/CharacterSlot0** | **빈 오브젝트** (PartyPanel 자식) | 없음. PartyHealthUI의 Slots[0]에 아래 참조 연결 |
| **CharacterSlot0/Portrait** | **Image** (CharacterSlot0 자식) | 없음. 캐릭터 초상화 스프라이트 |
| **CharacterSlot0/HealthBar** | **Image** (CharacterSlot0 자식) | 없음. Image Type을 Filled로 두고 Fill Amount로 체력 표시 |
| **CharacterSlot0/HealthText** | **Text - TextMeshPro** (CharacterSlot0 자식, 체력 바 위에 배치) | 없음. PartyHealthUI의 해당 Slot **Health Text**에 연결 시 "80/100" 형태로 표시 |
| **PartyPanel/CharacterSlot1** | **빈 오브젝트** | 동일 구조 (Portrait + HealthBar) |
| **PartyPanel/CharacterSlot2** | **빈 오브젝트** | 동일 구조 |
| **PartyPanel/CharacterSlot3** | **빈 오브젝트** | 동일 구조 |

| **PuzzleBoardFrame** | **빈 오브젝트** (Canvas 자식) | 없음. 퍼즐 타일이 놓일 **보드 배경**의 부모. 화면에서 보드가 보일 위치에 배치 |
| **PuzzleBoardFrame/Image** | **Image** (PuzzleBoardFrame 자식) | 없음. 보드 배경(테두리+체커보드) 스프라이트 할당 |
| **PuzzleBoardFrame/TileSlots** | **빈 오브젝트** (PuzzleBoardFrame 자식) | 없음. **10×10 = 100개 Image**를 여기 자식으로 배치. 각 Image에 **Tile (Script)** 붙임. GameBoard의 **Tile Slot Parent**에 연결 |

| **MonsterPanel** | **빈 오브젝트** (Canvas 자식) | MonsterHealthUI (Script) 붙임. **MonsterAttackController**는 Canvas 자식 빈 오브젝트에 하나만 붙임 |
| **MonsterPanel/MonsterSprite** | **Image** (MonsterPanel 자식) | 없음. ConfigApplier가 스테이지별 보스 스프라이트 할당 |
| **MonsterPanel/MonsterHealthBar** | **Image** (MonsterPanel 자식) | 없음. Image Type Filled, Fill Amount로 체력. MonsterHealthUI의 Health Bar Fill에 연결 |
| **MonsterPanel/PercentText** | **Text - TextMeshPro** (MonsterPanel 자식, 선택) | 없음. "63%" 등. MonsterHealthUI의 Percent Text에 연결 |
| **MonsterPanel/HealthNumbersText** | **Text - TextMeshPro** (MonsterPanel 자식, 체력 바 위 권장) | 없음. MonsterHealthUI의 **Health Numbers Text**에 연결 시 "315/500" 형태로 현재·최대 표시 |

| **SkillBar** | **빈 오브젝트** (Canvas 자식) | BattleSkillBarInitializer (Script)를 여기에 붙이고 Slots 배열에 아래 4개 SkillSlot 연결 권장 |
| **SkillBar/SkillSlot0** | **빈 오브젝트** (SkillBar 자식) | SkillSlotUI (Script) 붙임. 캐릭터 0번(맨 위 초상화)의 장착 스킬 |
| **SkillSlot0/Button** | **Button** (SkillSlot0 자식) | 없음. SkillSlotUI의 Button 참조 |
| **SkillSlot0/Icon** | **Image** (SkillSlot0 자식) | 없음. 스킬 아이콘 스프라이트 |
| **SkillSlot0/CooldownOverlay** | **Image** (SkillSlot0 자식) | 없음. Fill Method: Radial 360, Origin: Top, Clockwise. SkillSlotUI의 Cooldown Overlay 연결 |
| **SkillSlot0/GrayOverlay** | **Image** (SkillSlot0 자식) | 없음. 사용 중 회색 처리. SkillSlotUI의 Gray Overlay 연결 |
| **SkillBar/SkillSlot1** | **빈 오브젝트** | SkillSlotUI, 자식으로 Button / Icon / CooldownOverlay / GrayOverlay 동일. 캐릭터 1번 스킬 |
| **SkillBar/SkillSlot2** | **빈 오브젝트** | 동일. 캐릭터 2번 스킬 |
| **SkillBar/SkillSlot3** | **빈 오브젝트** | 동일. 캐릭터 3번 스킬 (총 4칸) |

| **MonsterAttackController** | **빈 오브젝트** (Canvas 자식) | MonsterAttackController (Script) 붙임. LevelManager, PartyHealthUI, MonsterHealthUI, StageDatabase 참조 연결 |
| **ScorePanel** 등 | (기존 UIManager 사용 시) **빈 오브젝트** 또는 **UI 요소** | UIManager (Script)는 별도 빈 오브젝트에 붙이거나 Canvas 자식에 붙임 |

---

### 1-4. 요약

- **빈 오브젝트**: GameManager, GameBoard, BattleSceneConfigApplier, Background, TurnUI, TopRightButtons, PartyPanel, CharacterSlot0~3, MonsterPanel, SkillBar, SkillSlot0~3, PuzzleBoardFrame
- **Image**: Background/Image, TurnUI/Icon, CharacterSlot의 Portrait·HealthBar, PuzzleBoardFrame/Image, MonsterSprite, MonsterHealthBar, SkillSlot의 Icon·CooldownOverlay·GrayOverlay
- **Button**: BackButton, SettingsButton, SkillSlot0~3의 Button
- **Text - TextMeshPro**: TurnText, PercentText, CharacterSlot별 HealthText(체력 숫자), MonsterPanel HealthNumbersText(현재/최대)
- **스크립트만 붙이는 오브젝트**: GameManager(여러 스크립트), GameBoard(GameBoard), BattleSceneConfigApplier, TurnUI(BattleTurnUI), BackButton(BattleBackButton), PartyPanel(PartyHealthUI), MonsterPanel(MonsterHealthUI), MonsterAttackController, SkillBar(BattleSkillBarInitializer), SkillSlot0~3(SkillSlotUI)

**참고:** 퍼즐 타일은 **10×10 Image 슬롯**을 사용자가 직접 배치합니다. 각 Image에 Tile 스크립트를 붙이고, GameBoard가 런타임에 0~3 타입을 랜덤 할당합니다.

---

### 1-5. 퍼즐 보드 배경 이미지 (타일이 놓일 보드)

| 항목 | 내용 |
|------|------|
| **Hierarchy 위치** | **Canvas** 자식 → **PuzzleBoardFrame** (빈 오브젝트) → 그 자식으로 **Image** 하나 |
| **오브젝트 이름** | 부모: **PuzzleBoardFrame**, 자식 Image: **Image** 또는 **PuzzleBoardImage** 등 (이름만 구분되면 됨) |
| **이미지 넣는 곳** | 자식 **Image** 오브젝트 선택 → Inspector **Image (Script)** → **Source Image**에 사용할 퍼즐 보드 스프라이트 할당 (테두리+체커보드 등 한 장) |
| **붙일 스크립트** | **없음**. Image 컴포넌트만으로 표시. 별도 스크립트 불필요 |

타일은 GameBoard가 런타임에 생성하므로, 보드 배경은 **보이기만 하면** 되고 로직은 없습니다. RectTransform으로 위치·크기만 조정해 두면 됩니다.

---

### 1-6. 타일 슬롯 (10×10 UI Image)

**사용자가 10×10 그리드로 100개 Image를 직접 배치**합니다. 각 Image에 Tile 스크립트를 붙입니다.

| 컴포넌트 | 용도 |
|----------|------|
| **Image** | 타일 표시. Source Image는 비워두어도 됨 (런타임에 GameBoard가 0~3 스프라이트 할당) |
| **Tile (Script)** | 타일 데이터·위치 관리. **반드시 붙임** |

**만드는 순서**: PuzzleBoardFrame 아래에 TileSlots 빈 오브젝트 생성 → `UI > Image` 100개 추가, 체커보드 칸에 맞게 10×10 배치 → 각 Image에 **Tile (Script)** 추가. GameBoard의 **Tile Slot Parent**에 TileSlots 오브젝트 연결. **Tile Sprites**에 8개 스프라이트 (0~3 기본, 4~7 강화) 할당.

---

## 2. 스크립트 연결 방법

**참조 표기 의미**
- `[GameObject]` = Hierarchy에 있는 오브젝트 (이름으로 찾을 수 있는 것)
- `[Component]` = 해당 오브젝트에 붙은 스크립트/컴포넌트
- `[Prefab]` = Project 창의 프리팹 에셋
- `[Layer]` = Edit > Project Settings > Tags and Layers 에서 설정한 레이어
- `[Script 붙임]` = 해당 오브젝트에 이 스크립트를 컴포넌트로 추가

---

### 2-1. GameManager (루트)

- **Game Manager (Script)** [Script 붙임] → `GameManager` 오브젝트에 붙임  
  - Board Width: 8, Board Height: 8, Tile Types: 4  
  - **Game Board** ← `[GameObject]` Hierarchy의 `GameBoard` 오브젝트  
  - **UIManager** ← `[Component]` UIManager 스크립트가 붙은 오브젝트  
  - **Score Manager** ← `[Component]` ScoreManager 스크립트가 붙은 오브젝트  
  - **Level Manager** ← `[Component]` LevelManager 스크립트가 붙은 오브젝트  

### 2-2. GameBoard

- **GameBoard (Script)** [Script 붙임] → `GameBoard` 오브젝트에 붙임  
- **Tile Slot Parent** ← `[GameObject]` 100개 Tile이 자식으로 있는 부모 (예: PuzzleBoardFrame/TileSlots)  
- **Tile Sprites** ← 스프라이트 8개 (0~3 기본: 검/지팡이/활/십자가, 4~7 강화)

### 2-3. Input Handler / Tile Swapper / 기타 퍼즐

- **Input Handler (Script)** [Script 붙임] → `GameManager` 오브젝트에 붙임  
  - **꾹 누른 상태에서** 위/아래/좌/우로 드래그 시 인접 1칸 스왑  
  - 스왑 후 3매칭 이상이면 제거·낙하·스폰. 3매칭 미만이면 스왑 원복  
  - **강화 타일**: 4개 이상 매칭 시 그 자리에 1개만 생성 (시작 시에는 생성되지 않음)  
- **Match Detector (Script)** [Script 붙임] → `GameManager` 오브젝트에 붙임  
- **Tile Swapper (Script)** [Script 붙임] → `GameManager` 오브젝트에 붙임  
  - **Game Board**, **Match Detector**, **Tile Clearer**, **Level Manager** ← 각각 `[Component]`  
- **Tile Clearer**, **Gravity Controller**, **Tile Spawner** [Script 붙임] → `GameManager` 오브젝트에 붙임

### 2-4. Level Manager (턴 제한)

- **Level Manager (Script)** [Script 붙임] → 별도 오브젝트 또는 GameManager에 붙임  
  - **Moves Remaining (movesLimit)**: 20  
  - 턴 UI는 **CurrentTurnNumber / MaxTurns** 사용

### 2-5. 턴 UI (왼쪽 상단)

- **BattleTurnUI (Script)** [Script 붙임] → `TurnUI` [GameObject]에 붙임  
  - **Turn Text** ← `[Component]` "1/20" 표시하는 TextMeshProUGUI  
  - LevelManager는 씬에 하나만 있으면 자동 탐색

### 2-6. 파티 체력 (왼쪽 4명)

- **PartyHealthUI (Script)** [Script 붙임] → `PartyPanel` [GameObject]에 붙임  
  - **Slots** 크기 4  
  - 각 Slot: **Portrait Image** ← `[Component]` Image, **Health Bar Fill** ← `[Component]` Image (Filled)  
  - **Health Text** (선택) ← `[Component]` TextMeshProUGUI ("80/100" 표시)

### 2-7. 몬스터 체력 (오른쪽)

- **MonsterHealthUI (Script)** [Script 붙임] → `MonsterPanel` [GameObject]에 붙임  
  - **Health Bar Fill** ← `[Component]` Image (Filled, 아래 "체력 바 Image 설정" 참고)  
  - **Percent Text** (선택) ← `[Component]` TextMeshProUGUI  
  - **Health Numbers Text** (선택) ← `[Component]` TextMeshProUGUI ("315/500")

### 2-7-1. 체력 바 Image 설정 (데미지/힐 시 채워졌다 빠졌다)

체력 바는 **그냥 Image** 하나로 두고, **스크립트에서 fillAmount(0~1)** 만 바꾸면 됩니다. Unity Image의 **Filled** 타입을 쓰면 됩니다.

1. 체력 바용 **Image** 오브젝트 생성 (`UI > Image`).
2. **Image** 컴포넌트에서:
   - **Image Type**: **Filled**
   - **Fill Method**: **Horizontal** (가로로 차는 바. 세로는 Vertical)
   - **Fill Origin**: **Left** (왼쪽부터 채움 → 데미지 시 오른쪽부터 빠짐)
   - **Fill Amount**: 1이면 풀, 0이면 빈 상태. 스크립트가 자동으로 바꿈.
3. 이 `[Component]` Image를 **PartyHealthUI**의 각 Slot **Health Bar Fill** / **MonsterHealthUI**의 **Health Bar Fill** 필드에 드래그로 연결.

스크립트에서 `TakeDamage`, `Heal`, `SetHP` 등을 호출하면 내부에서 `healthBarFill.fillAmount = (현재 HP / 최대 HP)` 로 갱신하므로, **채워졌다 빠졌다** 연출이 그대로 나옵니다. 별도 애니메이션 없이도 즉시 반영됩니다.

### 2-8. 스킬 아이콘 (하단 3개)

- **SkillSlotUI (Script)** [Script 붙임] → 각 스킬 슬롯 [GameObject]에 붙임  
  - **Button** ← `[Component]` UnityEngine.UI.Button  
  - **Icon Image** ← `[Component]` 스킬 아이콘 Image  
  - **Cooldown Overlay** ← `[Component]` Image (Fill Method: Radial 360, Origin: Top, Clockwise)  
  - **Gray Overlay** ← `[Component]` Image (사용 시 회색 처리)  
  - **Cooldown Duration** ← 쿨 시간(초) 숫자  
  - 스킬 사용 시 `OnSkillUsed` 이벤트로 연결

### 2-9. 오른쪽 상단 버튼

- **BattleBackButton (Script)** [Script 붙임] → BackButton [GameObject]에 붙임  
  - **Target Scene Name** ← 스테이지 선택 씬 이름 문자열 (예: `MapScene`, `StageSelectScene`)  
  - **Button** ← `[Component]` UnityEngine.UI.Button  
- **SettingsButton** [GameObject]: 설정 팝업용 (옵션 패널 등)

---

## 3. 배틀 씬과 사진 기능 매칭

| 사진 요소 | 구현 |
|-----------|------|
| 퍼즐 보드 (클릭 꾹 누른 채 드래그) | 기존 **InputHandler** (드래그로 스왑) |
| 턴 UI (1/20, 맞출 때마다 1 증가) | **LevelManager** (CurrentTurnNumber/MaxTurns) + **BattleTurnUI** |
| 파티 4명 체력·피격 판정 | **PartyHealthUI** (TakeDamage 등) |
| 몬스터 체력 바 (63%) | **MonsterHealthUI** (SetHP / SetPercent) |
| 스킬 아이콘 (클릭 사용, 회색+시계 방향 쿨) | **SkillSlotUI** (회색 오버레이 + Radial Fill) |
| 이전 단계/설정 아이콘 | BackButton → 스테이지 선택 씬 로드 / SettingsButton → 설정 |

---

## 4. 배틀 씬 시작 시 바로 플레이

- **BattleSceneStarter (Script)** [Script 붙임] → `GameManager` [GameObject]에 붙임  
  - 씬 로드 시 `ChangeState(Playing)` 후 `StartGame()` 호출 → 보드 생성 후 바로 플레이
- **StageData** [ScriptableObject 에셋]의 **Max Turns**를 40으로 두면 턴 UI가 "1/40" 형태로 동작. 40턴 소진 시 보스 체력으로 승/패 판정

---

## 5. 정리

### 5-1. 턴 (LevelManager)
- **한 턴** = 퍼즐 한 번 스왑 후 **매칭이 발생하면** 1턴 소모. 턴당 시간 제한 없음.
- **최대 40턴** (StageData.maxTurns). 매칭 성공 시 TileSwapper가 `IncrementMoves()` 호출.
- **MonsterAttackController**가 4턴마다 몬스터 공격, 40턴 소진 시 보스 체력으로 승/패 처리.

### 5-2. 점수 (ScoreManager)
- **역할**: 매칭 시 획득한 점수를 누적·표시 (UI 피드백용). **승패와 무관**.
- baseMatchScore × 매칭 개수, 콤보 보너스, 강화/특수 타일 보너스로 계산.
- 승패는 **몬스터 체력이 0 이하**인지로만 판정. 목표 점수 도달 클리어는 사용하지 않음.

### 5-3. 그 외
- **퍼즐 로직**: GameManager, GameBoard(슬롯 기반), InputHandler, MatchDetector, TileSwapper, TileClearer, GravityController, TileSpawner, ScoreManager, LevelManager 사용.
- **배틀 전용 UI**: BattleTurnUI, PartyHealthUI, MonsterHealthUI, SkillSlotUI를 추가로 연결.

---

## 6. 스테이지 9개: 배틀 씬 1개 + 데이터로 처리 (권장)

**배틀 씬을 9개 만들 필요 없습니다.** 배틀 씬은 **1개만** 두고, 스테이지 선택 시 **진입할 스테이지 번호만 넘긴 뒤** 같은 배틀 씬을 로드하면 됩니다. 씬 로드 후 스테이지에 맞게 **배경·보스 이미지·턴 수·몬스터 체력** 등이 바뀌게 할 수 있습니다.

### 6-1. 흐름

1. **스테이지 선택 화면** (맵/킹덤 씬 등)에서 스테이지 1~9 중 하나 클릭.
2. **진입할 스테이지 번호 저장** → `BattleStageHolder.SetStage(0);` (0 = 1스테이지, 8 = 9스테이지).
3. **배틀 씬 로드** → `SceneManager.LoadScene("BattleScene");`
4. 배틀 씬의 **BattleSceneConfigApplier**가 `BattleStageHolder.CurrentStageIndex`를 읽고, 해당 스테이지 데이터로 배경·보스·턴·몬스터 체력 적용.

### 6-2. 스테이지 데이터 설정

- **StageData** (ScriptableObject): 스테이지 하나당 하나.  
  `Assets > Create > Match3 > Stage Data`  
  - 스테이지 이름, 배경 스프라이트, 보스 스프라이트, 몬스터 최대 HP, 최대 턴 수 등. (목표 점수 없음, 승패는 몬스터 체력으로만 판정)
  - **매칭 시 전투 효과**: 검(0)=광역 공격, 활(1)=단일 강력 공격, 십자가(2)=파티 회복, 지팡이(3)=마법 광역 공격. **Match Effect Handler** 스크립트로 처리. StageData에서 데미지/힐량·저항력 조절  
  - **몬스터 저항력**: `Sword Resistance` / `Bow Resistance` / `Wand Resistance` 각각 0~1로 독립 설정  
  - **몬스터 공격 (인스펙터에서 조절)**:
    - **Attack Interval Turns**: 몬스터가 공격하는 턴 간격 (예: 4 = 4턴마다 공격)
    - **Attack Damage**: 공격 데미지 (1인당)
    - **Attack Target Type**: All(4명 전체) / SingleRandom(랜덤 1명) / Random2(랜덤 2명) / Random3(랜덤 3명)
- **StageDatabase** (ScriptableObject): StageData 9개를 배열로 보관.  
  `Assets > Create > Match3 > Stage Database`  
  - Element 0~8에 스테이지 1~9용 StageData 할당.
- **StageDatabase** 에셋을 **Resources** 폴더에 넣고 이름을 `StageDatabase`로 두면, 참조 없이도 로드됩니다.  
  또는 배틀 씬의 **BattleSceneConfigApplier** 인스펙터에서 **Stage Database** 필드에 직접 할당해도 됩니다.

### 6-3. 턴·승패 규칙

- **최대 턴**: StageData의 **Max Turns** (기본 40). 40턴 안에 보스 처치 실패 시 패배.
- **턴 소진 시**: 보스 HP ≤ 0 → 승리, 보스 HP > 0 → 패배. **MonsterAttackController**가 판정.
- **몬스터 공격**: 4턴마다(StageData에서 조절) 파티에 데미지. 대상은 StageData의 **Attack Target Type**에 따라 전체/단일/2명/3명.

### 6-4. 배틀 씬에 연결

- **BattleSceneConfigApplier (Script)** [Script 붙임] → Canvas 자식 [GameObject]에 붙임  
  - **Stage Database** ← `[Prefab/에셋]` StageDatabase ScriptableObject 에셋  
  - **Background Image** ← `[Component]` Canvas의 배경 Image  
  - **Monster Image** ← `[Component]` 몬스터 스프라이트용 Image  
  - **Monster Health UI** ← `[Component]` MonsterHealthUI 스크립트  
  - **Level Manager** ← `[Component]` LevelManager 스크립트  
- **실행 순서**: Edit > Project Settings > Script Execution Order에서 **BattleSceneConfigApplier**를 **BattleSceneStarter**보다 작은 값으로 설정 (먼저 실행)

- **MonsterAttackController (Script)** [Script 붙임] → Canvas 자식 빈 [GameObject]에 붙임  
  - **Level Manager** ← `[Component]` LevelManager  
  - **Party Health UI** ← `[Component]` PartyHealthUI  
  - **Monster Health UI** ← `[Component]` MonsterHealthUI  
  - **Stage Database** ← `[에셋]` StageDatabase ScriptableObject

### 6-5. 스테이지 선택 시 코드 예시

```csharp
// 스테이지 3 클릭 시 (인덱스 2)
BattleStageHolder.SetStage(2);
SceneManager.LoadScene("BattleScene");
```

이렇게 하면 **배틀 씬은 1개**만 두고, 스테이지 1~9는 **이미지와 보스·난이도만 바꿔서** 모두 처리할 수 있습니다.

---

## 7. 타일이 보이지 않을 때 점검 사항

타일이 보이지 않는다면 아래를 순서대로 확인하세요.

| # | 원인 | 확인 방법 | 해결 |
|---|------|-----------|------|
| **1** | **Tile Slot Parent 미지정** | GameBoard 인스펙터의 **Tile Slot Parent**가 비어 있음 | 100개 Tile이 자식으로 있는 부모 오브젝트(PuzzleBoardFrame/TileSlots 등)를 할당 |
| **2** | **Tile Sprites 미지정** | GameBoard 인스펙터의 **Tile Sprites**가 비어 있거나 8개 미만 | 검/지팡이/활/십자가 스프라이트 8개(0~3 기본, 4~7 강화)를 **Tile Sprites** 배열에 할당 |
| **3** | **타일 슬롯 부족** | Tile Slot Parent 자식에 Tile이 100개 미만 | 10×10으로 Image 100개를 배치하고 각각에 **Tile (Script)** 붙임 |
| **4** | **이벤트 미동작** | 클릭/드래그가 안 됨 | Canvas에 **GraphicRaycaster**, 씬에 **EventSystem** 확인. 각 Tile Image의 **Raycast Target** 체크 |
| **5** | **StartGame 미호출** | 보드가 초기화되지 않음 | GameManager에 **BattleSceneStarter** 스크립트가 붙어 있는지 확인 |

---

이 가이드대로 Hierarchy를 만들고 참조만 채우면, 사진과 같은 배틀 씬 구성과 9스테이지 구분까지 맞출 수 있습니다.
