# 3매치 퍼즐 게임 Unity 설정 가이드

## 📋 프로젝트 설정 단계

### 1단계: 씬 설정

1. **새 씬 생성** 또는 기존 씬 사용
   - `File > New Scene` 또는 `Scenes/GameScene.unity` 사용

2. **메인 게임 오브젝트 생성**
   ```
   GameManager (빈 GameObject)
   ├── GameBoard (빈 GameObject)
   │   ├── GridManager (Component)
   │   └── Tiles (빈 GameObject - 자동 생성됨)
   ├── InputHandler (Component)
   ├── MatchDetector (Component)
   ├── TileSwapper (Component)
   ├── TileClearer (Component)
   ├── GravityController (Component)
   ├── TileSpawner (Component)
   ├── ScoreManager (Component)
   ├── LevelManager (Component)
   └── UIManager (Component)
   ```

### 2단계: 컴포넌트 설정

#### GameManager 설정
- `GameManager.cs` 스크립트 추가
- **Board Width**: 8
- **Board Height**: 8
- **Tile Types**: 4 (검, 지팡이, 활, 십자가)
- GameBoard, UIManager, ScoreManager, LevelManager 참조 연결

#### GameBoard 설정
- `GameBoard.cs` 스크립트 추가
- **Grid Manager**: GridManager 컴포넌트 참조
- **Tile Prefab**: 타일 프리팹 할당 (아래 참조)
- **Tile Sprites**: 8개 (0~3: 기본 타일, 4~7: 강화 타일 스프라이트. 4개 이상 매칭 시 강화 타일 생성)

#### GridManager 설정
- `GridManager.cs` 스크립트 추가
- **Cell Size**: 1.0 (타일 간격)
- **Grid Offset**: (0, 0) 또는 보드 중앙 정렬

#### InputHandler 설정
- `InputHandler.cs` 스크립트 추가
- **Event System** 추가 (Unity UI 필요)
- **Main Camera** 참조
- **Tile Layer** 설정 (타일이 있는 레이어)

### 3단계: 타일 프리팹 생성

1. **타일 프리팹 만들기**
   ```
   Tile (GameObject)
   ├── SpriteRenderer (Component)
   ├── BoxCollider2D (Component) - Input 감지용
   ├── Tile.cs (Component)
   └── Animator (Component) - 선택적
   ```

2. **타일 설정**
   - SpriteRenderer: 타일 스프라이트 할당
   - BoxCollider2D: Is Trigger 체크
   - Tile.cs: 스크립트 추가
   - Animator: 애니메이션 컨트롤러 할당 (선택적)

3. **프리팹으로 저장**
   - `Assets/Prefabs/Tile.prefab`로 저장

### 4단계: 스프라이트 준비

1. **타일 스프라이트 준비**
   - 기본 4종: 검, 지팡이, 활, 십자가 (인덱스 0~3)
   - 강화 4종: 위와 같은 순서로 강화 이미지 (인덱스 4~7). 4개 이상 매칭 시 해당 타입의 강화 타일이 한 칸에 생성됨
   - GameBoard의 **Tile Sprites**에는 위 8개를 순서대로 할당
   - 크기: 64x64 또는 128x128 권장

2. **스프라이트 임포트 설정**
   - Texture Type: Sprite (2D and UI)
   - Pixels Per Unit: 100
   - Filter Mode: Bilinear

3. **GameBoard에 할당**
   - GameBoard의 Tile Sprites 배열에 순서대로 할당

### 5단계: UI 설정

#### Canvas 생성
1. `GameObject > UI > Canvas` 생성
2. Canvas Scaler 설정:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080

#### UI 요소 생성

**Score UI**
```
Canvas
├── ScorePanel
│   ├── ScoreText (TextMeshProUGUI)
│   ├── BestScoreText (TextMeshProUGUI)
│   └── ComboText (TextMeshProUGUI)
```

**Level UI**
```
Canvas
├── LevelPanel
│   ├── LevelText (TextMeshProUGUI)
│   ├── MovesText (TextMeshProUGUI)
│   ├── TimeText (TextMeshProUGUI)
│   └── TargetScoreText (TextMeshProUGUI)
```

**Game Over Panel**
```
Canvas
├── GameOverPanel (GameObject - 초기 비활성화)
│   ├── GameOverScoreText (TextMeshProUGUI)
│   └── RestartButton (Button)
```

**Level Complete Panel**
```
Canvas
├── LevelCompletePanel (GameObject - 초기 비활성화)
│   ├── LevelCompleteScoreText (TextMeshProUGUI)
│   └── NextLevelButton (Button)
```

**Pause Panel**
```
Canvas
├── PausePanel (GameObject - 초기 비활성화)
│   ├── ResumeButton (Button)
│   └── QuitButton (Button)
```

#### UIManager 설정
- 모든 UI 요소를 UIManager에 연결
- TextMeshProUGUI 컴포넌트가 있는 오브젝트들 참조

### 6단계: 레이어 설정

1. **레이어 생성**
   - `Edit > Project Settings > Tags and Layers`
   - Layer 8: "Tile" 생성

2. **타일 레이어 설정**
   - 모든 타일 프리팹의 레이어를 "Tile"로 설정
   - InputHandler의 Tile Layer를 "Tile"로 설정

### 7단계: 카메라 설정

1. **카메라 위치 조정**
   - 보드가 화면 중앙에 오도록 위치 조정
   - Orthographic Size 조정

2. **카메라 Culling Mask**
   - Tile 레이어가 보이도록 설정

### 8단계: Physics2D 설정

1. **Physics2D 설정 확인**
   - `Edit > Project Settings > Physics2D`
   - Raycasts가 작동하는지 확인

### 9단계: 네임스페이스 확인

모든 스크립트가 올바른 네임스페이스를 사용하는지 확인:
- `Match3Puzzle.Core`
- `Match3Puzzle.Board`
- `Match3Puzzle.Matching`
- `Match3Puzzle.Swap`
- `Match3Puzzle.Clear`
- `Match3Puzzle.Gravity`
- `Match3Puzzle.Spawn`
- `Match3Puzzle.Score`
- `Match3Puzzle.Level`
- `Match3Puzzle.UI`

---

## 🎮 게임 실행 방법

1. **씬 설정 완료 후**
   - Play 버튼 클릭
   - GameManager의 StartGame() 메서드가 자동 호출되거나
   - UI에서 시작 버튼 추가 필요

2. **게임 시작**
   - GameManager.StartGame() 호출
   - 또는 메뉴 씬에서 시작 버튼 클릭

---

## 🔧 문제 해결

### 타일이 보이지 않을 때
- 타일 프리팹의 SpriteRenderer 확인
- 카메라 Culling Mask 확인
- 타일 위치 확인 (GridManager의 GridToWorld 확인)

### 입력이 작동하지 않을 때
- EventSystem이 씬에 있는지 확인
- InputHandler의 Main Camera 참조 확인
- 타일의 Collider2D 확인
- Tile Layer 설정 확인

### 매칭이 감지되지 않을 때
- MatchDetector 컴포넌트 확인
- 타일의 TileType이 올바르게 설정되었는지 확인
- GameBoard의 tiles 배열이 올바르게 초기화되었는지 확인

### 타일이 떨어지지 않을 때
- GravityController 컴포넌트 확인
- TileSpawner 컴포넌트 확인
- GameBoard의 SwapTiles 메서드 확인

---

## 📝 추가 개선 사항

1. **애니메이션 추가**
   - 타일 선택 애니메이션
   - 타일 제거 애니메이션
   - 타일 낙하 애니메이션

2. **사운드 추가**
   - AudioManager 구현
   - 매칭 사운드
   - 폭발 사운드
   - 배경 음악

3. **특수 타일 구현**
   - BombTile
   - RainbowTile
   - RocketTile

4. **파티클 효과**
   - 제거 시 파티클
   - 특수 타일 활성화 파티클

5. **레벨 에디터**
   - ScriptableObject 기반 레벨 데이터
   - 레벨 에디터 툴

---

## 🎯 다음 단계

1. 기본 시스템 테스트
2. UI 완성
3. 사운드 추가
4. 특수 타일 구현
5. 레벨 디자인
6. 폴리싱 및 최적화
