# 3-Match Puzzle RPG

팀 프로젝트로 제작한 **3매치 퍼즐 + RPG** 하이브리드 게임입니다.  
타일을 맞춰 파티를 강화하고, 몬스터와 턴제 전투를 진행하며 스토리를 클리어하는 구성입니다.

> **프로젝트 상태:** 개발 종료 (최종 점검 및 엔딩 씬까지 반영)

---

## 게임 소개

플레이어는 4인 파티(검사 · 마법사 · 궁수 · 힐러)를 이끌며, 맵에서 스테이지를 선택하고 왕국에서 스탯·스킬·장비를 정비한 뒤 배틀에 진입합니다.

배틀에서는 **검 / 지팡이 / 활 / 십자가** 타일을 3개 이상 매칭해 캐릭터별 공격·회복을 발동하고, 스킬·장비를 활용해 몬스터를 격파합니다.

### 주요 특징

- **3매치 퍼즐 전투** — 스왑, 매칭, 제거, 중력, 리스폰 및 연쇄 매칭
- **강화 타일** — 4개 이상 매칭 시 특수(강화) 타일 생성
- **턴제 RPG 전투** — 파티 HP, 몬스터 HP, 턴 UI, 스킬 바
- **월드 맵 · 챕터 해금** — 3개 챕터 × 스테이지 3개 (총 9스테이지)
- **왕국 허브** — 스탯 / 스킬 / 장비 정비
- **스토리(VN)** — 타이핑·스킵 지원 컷신
- **튜토리얼** — 매칭·스킬 사용 안내
- **세이브/로드** — JSON 기반 이어하기
- **옵션** — 볼륨, 전체화면 등 (메뉴·인게임 공용)
- **엔딩 크레딧**

---

## 기술 스택

| 항목 | 내용 |
|------|------|
| 엔진 | Unity **6000.3.11f1** (Unity 6) |
| 렌더 파이프라인 | URP (Universal Render Pipeline) |
| UI | uGUI + TextMesh Pro |
| 입력 | Input System |
| 2D | Sprite, Animation, Tilemap 등 |
| 세이브 | `persistentDataPath` JSON |
| 저장소 | [github.com/ro7030/3_Match_Puzzle_RPG](https://github.com/ro7030/3_Match_Puzzle_RPG) |

---

## 플레이 흐름

```
TitleScene
  ├─ New Game → Prologue / Story → Tutorial → Map
  └─ Continue  → 세이브 로드 후 이어서 진행

MapScene (챕터·스테이지 선택)
  └─ KingdomScene (파티 정비)
        ├─ StatusScene   (스탯)
        ├─ SkillScene    (스킬)
        └─ InventoryScene (장비)
  └─ BattleScene (3매치 전투)
        └─ 클리어 시 스토리/진행도 갱신 → Map
             └─ 최종 클리어 시 EndingScene
```

### 챕터 구성

| 챕터 | 지역 | 해금 조건 |
|------|------|-----------|
| 1 | Monster Forest | 항상 해금 |
| 2 | Ancient Legendary Ruins | 챕터1 스테이지3 클리어 |
| 3 | Dragon's Fortress | 챕터2 스테이지3 클리어 |

---

## 씬 목록

| 씬 | 역할 |
|----|------|
| `TitleScene` | 타이틀 / 뉴게임·이어하기·옵션 |
| `PrologueScene` | 프롤로그 |
| `StoryScene` | 비주얼 노벨 컷신 |
| `TutorialScene` | 조작·스킬 튜토리얼 |
| `MapScene` | 월드 맵 · 스테이지 선택 |
| `KingdomScene` | 왕국 허브 |
| `StatusScene` | 캐릭터 스탯 |
| `SkillScene` | 스킬 장착·상점 |
| `InventoryScene` | 장비 |
| `BattleScene` | 3매치 배틀 |
| `EndingScene` | 엔딩 크레딧 |

---

## 프로젝트 구조

```
Assets/
├── Scenes/          # 게임 씬
├── Scripts/         # C# 스크립트 (기능별 폴더)
│   ├── Board/       # 보드 · 타일 · 그리드
│   ├── Matching/    # 매칭 감지 · 전투 효과 연동
│   ├── Swap/        # 타일 스왑
│   ├── Clear/       # 타일 제거
│   ├── Gravity/     # 낙하
│   ├── Spawn/       # 타일 생성
│   ├── Core/        # GameManager · 배틀 시작 등
│   ├── Level/       # 레벨·목표
│   ├── Score/       # 점수
│   ├── Stage/       # 스테이지 데이터
│   ├── Skill/       # 스킬 UI·데이터
│   ├── Stats/       # 캐릭터 스탯
│   ├── Status/      # 스테이터스 씬
│   ├── Inventory/   # 장비
│   ├── Map/         # 맵 · 챕터
│   ├── Kingdom/     # 왕국 허브
│   ├── MainMenu/    # 타이틀 · 세이브
│   ├── Story/       # 컷신 · VN
│   ├── Tutorial/    # 튜토리얼
│   ├── UI/          # 공통 UI
│   └── Audio/       # BGM
├── Data/            # Equipment · Skills · Stages 등
├── Prefab/          # 프리팹
├── Resources/       # 런타임 로드 리소스 (이미지·스토리 등)
└── Settings/        # URP 등 프로젝트 설정
```

스크립트는 약 80개 내외이며, 배틀·맵·인벤토리·스토리 등이 씬별로 분리되어 있습니다.

---

## 실행 방법

1. **Unity Hub**에서 이 프로젝트를 엽니다.  
   - 권장 버전: **Unity 6000.3.11f1** (다르면 마이그레이션 안내가 뜰 수 있음)
2. `File > Build Settings`에서 위 씬들이 포함되어 있는지 확인합니다.
3. `TitleScene`을 연 뒤 Play로 실행합니다.

> `Library/`, `Logs/`, `Temp/` 등은 `.gitignore` 대상입니다. 클론 후 Unity가 한 번 임포트합니다.

---

## 문서

씬·시스템별 설정 가이드는 `Assets/Scripts/` 아래에 있습니다.

| 문서 | 내용 |
|------|------|
| [3MatchPuzzle_프로젝트구조.md](Assets/Scripts/3MatchPuzzle_프로젝트구조.md) | 퍼즐 시스템 구조·기능 명세 |
| [3MatchPuzzle_설정가이드.md](Assets/Scripts/3MatchPuzzle_설정가이드.md) | 보드·타일·매칭 기본 설정 |
| [BattleScene_설정가이드.md](Assets/Scripts/BattleScene_설정가이드.md) | 배틀 씬 Hierarchy·연결 |
| [BattleData_설정가이드.md](Assets/Scripts/BattleData_설정가이드.md) | 배틀 데이터 |
| [MapScene_설정가이드.md](Assets/Scripts/Map/MapScene_설정가이드.md) | 맵·챕터·스테이지 해금 |
| [KingdomScene_설정가이드.md](Assets/Scripts/Kingdom/KingdomScene_설정가이드.md) | 왕국 허브 |
| [StatusScene_설정가이드.md](Assets/Scripts/StatusScene_설정가이드.md) | 스탯 씬 |
| [SkillScene_설정가이드.md](Assets/Scripts/SkillScene_설정가이드.md) | 스킬 씬 |
| [InventoryScene_설정가이드.md](Assets/Scripts/Inventory/InventoryScene_설정가이드.md) | 장비 씬 |
| [StartScene_설정가이드.md](Assets/Scripts/MainMenu/StartScene_설정가이드.md) | 타이틀·세이브 |
| [VisualNovel_설정가이드.md](Assets/Scripts/Story/VisualNovel_설정가이드.md) | 스토리 컷신 |
| [TutorialScene_설정가이드.md](Assets/Scripts/TutorialScene_설정가이드.md) | 튜토리얼 |

---

## 팀

Git 기여 기준으로 참여한 멤버입니다.

- ro7030
- MBC_Aca_Jun

---

## 라이선스 / 기타

팀 학습·포트폴리오용 프로젝트입니다.  
외부 에셋·폰트 라이선스는 각 리소스 폴더 내 안내 파일을 참고하세요.
