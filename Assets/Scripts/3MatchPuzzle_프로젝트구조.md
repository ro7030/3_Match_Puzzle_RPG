# 3매치 퍼즐 게임 프로젝트 구조 및 기능 명세서

## 📋 프로젝트 개요
Unity에서 구현할 3매치 퍼즐 게임의 전체 구조와 필요한 기능들을 정리한 문서입니다.

---

## 🎯 핵심 게임 시스템

### 1. 게임 보드 시스템 (GameBoard)
**목적**: 퍼즐 타일들이 배치되는 그리드 관리

**필요한 스크립트**:
- `GameBoard.cs` - 보드 생성, 타일 배치 관리
- `GridManager.cs` - 그리드 좌표 시스템 관리
- `Tile.cs` - 개별 타일 데이터 및 상태 관리

**주요 기능**:
- 보드 크기 설정 (예: 8x8, 9x9)
- 타일 타입 정의 (색상별, 모양별)
- 타일 스폰 및 배치
- 보드 초기화 (매칭 없는 상태로 시작)

---

### 2. 매칭 시스템 (Matching System)
**목적**: 3개 이상의 같은 타일이 연결되었는지 감지

**필요한 스크립트**:
- `MatchDetector.cs` - 매칭 감지 로직
- `MatchChecker.cs` - 특정 위치의 매칭 확인

**주요 기능**:
- 가로/세로 매칭 감지
- L자, T자 모양 매칭 감지
- 4개 이상 매칭 시 특수 타일 생성
- 매칭 가능 여부 사전 체크

---

### 3. 스왑 시스템 (Swap System)
**목적**: 플레이어가 두 타일을 교환하는 기능

**필요한 스크립트**:
- `TileSwapper.cs` - 타일 교환 로직
- `InputHandler.cs` - 터치/마우스 입력 처리
- `TileSelector.cs` - 타일 선택 관리

**주요 기능**:
- 드래그 앤 드롭으로 타일 교환
- 인접한 타일만 교환 가능
- 교환 후 매칭 여부 확인
- 매칭 없으면 원위치 복귀

---

### 4. 폭발/제거 시스템 (Clear System)
**목적**: 매칭된 타일들을 제거하고 효과 연출

**필요한 스크립트**:
- `TileClearer.cs` - 타일 제거 처리
- `ClearEffect.cs` - 폭발 이펙트 관리
- `ParticleManager.cs` - 파티클 효과 관리

**주요 기능**:
- 매칭된 타일 제거
- 폭발 애니메이션/파티클
- 사운드 효과 재생
- 제거 순서 관리 (연쇄 반응)

---

### 5. 중력 시스템 (Gravity System)
**목적**: 타일 제거 후 위의 타일들이 아래로 떨어지도록 처리

**필요한 스크립트**:
- `GravityController.cs` - 중력 적용 로직
- `TileMover.cs` - 타일 이동 애니메이션

**주요 기능**:
- 빈 공간 감지
- 타일 낙하 처리
- 낙하 애니메이션
- 낙하 후 추가 매칭 체크

---

### 6. 스폰 시스템 (Spawn System)
**목적**: 빈 공간에 새로운 타일 생성

**필요한 스크립트**:
- `TileSpawner.cs` - 새 타일 생성
- `TilePool.cs` - 타일 오브젝트 풀링

**주요 기능**:
- 빈 공간에 새 타일 생성
- 랜덤 타입 할당
- 생성 애니메이션
- 오브젝트 풀링으로 성능 최적화

---

### 7. 점수 시스템 (Score System)
**목적**: 플레이어 점수 계산 및 관리

**필요한 스크립트**:
- `ScoreManager.cs` - 점수 계산 및 저장
- `ComboSystem.cs` - 콤보 보너스 계산

**주요 기능**:
- 매칭별 점수 계산
- 콤보 보너스
- 특수 타일 보너스
- 최고 점수 저장

---

### 8. 레벨/스테이지 시스템 (Level System)
**목적**: 게임 레벨 관리 및 목표 설정

**필요한 스크립트**:
- `LevelManager.cs` - 레벨 데이터 관리
- `LevelData.cs` - 레벨 정보 (ScriptableObject)
- `GoalManager.cs` - 목표 달성 체크

**주요 기능**:
- 레벨별 목표 설정 (점수, 특정 타일 제거 등)
- 제한 시간/이동 횟수
- 레벨 클리어 조건
- 레벨 진행도 저장

---

### 9. UI 시스템 (UI System)
**목적**: 게임 인터페이스 관리

**필요한 스크립트**:
- `UIManager.cs` - 전체 UI 관리
- `ScoreUI.cs` - 점수 표시
- `GoalUI.cs` - 목표 표시
- `GameOverUI.cs` - 게임 오버 화면
- `PauseMenu.cs` - 일시정지 메뉴

**주요 기능**:
- 점수/목표 실시간 표시
- 게임 오버/클리어 화면
- 일시정지 기능
- 설정 메뉴

---

### 10. 사운드 시스템 (Audio System)
**목적**: 게임 사운드 및 음악 관리

**필요한 스크립트**:
- `AudioManager.cs` - 사운드 재생 관리
- `SoundEffect.cs` - 효과음 재생

**주요 기능**:
- 배경 음악 재생
- 효과음 재생 (매칭, 폭발 등)
- 볼륨 조절
- 사운드 온/오프

---

### 11. 특수 타일 시스템 (Special Tiles)
**목적**: 특수한 효과를 가진 타일 구현

**필요한 스크립트**:
- `SpecialTile.cs` - 특수 타일 베이스 클래스
- `BombTile.cs` - 폭탄 타일 (주변 폭발)
- `RainbowTile.cs` - 무지개 타일 (같은 색 모두 제거)
- `RocketTile.cs` - 로켓 타일 (한 줄 제거)

**주요 기능**:
- 4개 매칭 시 폭탄 생성
- 5개 매칭 시 무지개 생성
- 특수 타일 조합 효과
- 특수 타일 활성화 애니메이션

---

### 12. 게임 상태 관리 (Game State)
**목적**: 게임의 전체 상태 관리

**필요한 스크립트**:
- `GameManager.cs` - 게임 전체 관리
- `GameState.cs` - 게임 상태 열거형
- `StateMachine.cs` - 상태 머신

**주요 기능**:
- 게임 시작/일시정지/종료 상태 관리
- 상태 전환 처리
- 게임 루프 관리

---

## 📁 권장 폴더 구조

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs
│   │   ├── GameState.cs
│   │   └── StateMachine.cs
│   ├── Board/
│   │   ├── GameBoard.cs
│   │   ├── GridManager.cs
│   │   └── Tile.cs
│   ├── Matching/
│   │   ├── MatchDetector.cs
│   │   └── MatchChecker.cs
│   ├── Swap/
│   │   ├── TileSwapper.cs
│   │   ├── InputHandler.cs
│   │   └── TileSelector.cs
│   ├── Clear/
│   │   ├── TileClearer.cs
│   │   └── ClearEffect.cs
│   ├── Gravity/
│   │   ├── GravityController.cs
│   │   └── TileMover.cs
│   ├── Spawn/
│   │   ├── TileSpawner.cs
│   │   └── TilePool.cs
│   ├── Special/
│   │   ├── SpecialTile.cs
│   │   ├── BombTile.cs
│   │   ├── RainbowTile.cs
│   │   └── RocketTile.cs
│   ├── Score/
│   │   ├── ScoreManager.cs
│   │   └── ComboSystem.cs
│   ├── Level/
│   │   ├── LevelManager.cs
│   │   ├── LevelData.cs
│   │   └── GoalManager.cs
│   ├── UI/
│   │   ├── UIManager.cs
│   │   ├── ScoreUI.cs
│   │   ├── GoalUI.cs
│   │   ├── GameOverUI.cs
│   │   └── PauseMenu.cs
│   └── Audio/
│       ├── AudioManager.cs
│       └── SoundEffect.cs
├── Prefabs/
│   ├── Tile.prefab
│   ├── BombTile.prefab
│   ├── RainbowTile.prefab
│   └── RocketTile.prefab
├── Sprites/
│   ├── Tiles/
│   │   ├── Tile_Red.png
│   │   ├── Tile_Blue.png
│   │   ├── Tile_Green.png
│   │   ├── Tile_Yellow.png
│   │   └── ...
│   └── UI/
│       ├── Button.png
│       └── ...
├── ScriptableObjects/
│   └── LevelData/
│       ├── Level_01.asset
│       ├── Level_02.asset
│       └── ...
├── Scenes/
│   ├── MainMenu.unity
│   ├── GameScene.unity
│   └── LevelSelect.unity
├── Audio/
│   ├── Music/
│   │   └── BGM.mp3
│   └── SFX/
│       ├── Match.wav
│       ├── Explode.wav
│       └── ...
└── Materials/
    └── TileMaterial.mat
```

---

## 🎮 게임 플로우

1. **게임 시작**
   - 레벨 데이터 로드
   - 보드 생성 및 타일 배치
   - UI 초기화

2. **플레이어 입력**
   - 타일 선택
   - 인접 타일과 교환

3. **매칭 체크**
   - 교환 후 매칭 확인
   - 매칭 없으면 원위치

4. **타일 제거**
   - 매칭된 타일 제거
   - 효과 연출

5. **중력 적용**
   - 타일 낙하
   - 새 타일 스폰

6. **연쇄 반응**
   - 낙하 후 추가 매칭 체크
   - 반복

7. **목표 달성 체크**
   - 레벨 클리어 여부 확인
   - 게임 오버 조건 확인

---

## 🔧 구현 우선순위

### Phase 1: 기본 시스템
1. 게임 보드 생성
2. 타일 배치
3. 기본 매칭 감지
4. 타일 교환

### Phase 2: 핵심 게임플레이
5. 타일 제거 및 중력
6. 새 타일 스폰
7. 연쇄 반응

### Phase 3: 게임 완성
8. 점수 시스템
9. 레벨 시스템
10. UI 구현

### Phase 4: 폴리싱
11. 특수 타일
12. 사운드/이펙트
13. 애니메이션

---

## 📝 추가 고려사항

- **성능 최적화**: 오브젝트 풀링 사용
- **확장성**: ScriptableObject로 레벨 데이터 관리
- **재사용성**: 모듈화된 구조로 설계
- **디버깅**: 에디터 도구 추가 고려
- **저장**: PlayerPrefs 또는 JSON으로 진행도 저장
