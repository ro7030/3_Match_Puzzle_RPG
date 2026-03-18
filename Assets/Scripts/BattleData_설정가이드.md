# 배틀 데이터 설정 가이드

스토리 진행 순서에 따라 **스테이지를 하나씩 추가할 때** 이 가이드를 참고하세요.  
배틀 씬은 1개만 두고, 데이터(ScriptableObject)만 채워서 스테이지를 늘립니다.

---

## 구조 한눈에 보기

```
[데이터 에셋]
  CharacterStats      ← 플레이어 기본 데미지/힐 + 업그레이드 레벨당 증가량
  StageDatabase       ← 스테이지 0~8 배열 (최대 9개)
    └ StageData_S1    ← 스테이지 1 데이터 (몬스터HP, 저항력, 공격 패턴, 배경 등)
    └ StageData_S2    ← 스테이지 2 데이터
    └ ...
  SkillDatabase       ← 스킬 목록
    └ SkillData_A     ← 캐릭터 A 스킬

[배틀 씬 오브젝트]
  BattleSceneConfigApplier  ← StageDatabase 읽어서 배경/몬스터/턴 자동 적용
  MonsterAttackController   ← StageDatabase 읽어서 몬스터 공격 패턴 적용
  MatchEffectHandler        ← CharacterStats(플레이어 데미지) + StageData(저항력) 적용
  BattleSkillBarInitializer ← 스킬 버튼 클릭 시 CharacterStats × 배율로 즉시 효과
  PartyHealthUI             ← 파티 4명 체력 (Inspector에서 Max HP 설정)

[런타임 정적 보관]
  CharacterUpgradeHolder    ← 현재 업그레이드 레벨 (세이브 데이터와 연동)

[최종 데미지 계산]
  CharacterStatsResolver    ← 기본값 + 레벨 × 레벨당증가량
```

### 데이터 책임 분리

| 데이터 | 담당 | 변경 시점 |
|--------|------|-----------|
| 플레이어 기본 데미지/힐 | `CharacterStats` | 게임 밸런싱 시 |
| 업그레이드 레벨당 증가량 | `CharacterStats` | 게임 밸런싱 시 |
| 현재 업그레이드 레벨 | `CharacterUpgradeHolder` (세이브) | 플레이어 업그레이드 시 |
| 몬스터 저항력/HP/공격 | `StageData` | 스테이지 추가 시 |

---

## STEP 1: CharacterStats 에셋 생성 (한 번만)

> **어디서**: Project 창 → `Assets/Resources/` 폴더 우클릭 → **Create > Match3 > Character Stats**  
> 에셋 이름을 반드시 **`CharacterStats`** 로 저장 (코드에서 자동 로드)

| 헤더 | 항목 | 설명 | 예시 |
|------|------|------|------|
| **기본 데미지/힐** | Base Sword Damage | 검 타일 매칭 1개당 기본 데미지 | `30` |
| **기본 데미지/힐** | Base Bow Damage | 활 타일 매칭 단일 기본 데미지 | `80` |
| **기본 데미지/힐** | Base Wand Damage | 지팡이 매칭 1개당 기본 마법 데미지 | `35` |
| **기본 데미지/힐** | Base Heal Amount | 십자가 매칭 1개당 회복량 (전원) | `25` |
| **기본 데미지/힐** | Base Enhanced Bonus | 강화 타일 1개당 추가 데미지 | `50` |
| **레벨당 증가량** | Sword Bonus Per Level | 검 업그레이드 1레벨당 데미지 증가 | `10` |
| **레벨당 증가량** | Bow Bonus Per Level | 활 업그레이드 1레벨당 증가 | `20` |
| **레벨당 증가량** | Wand Bonus Per Level | 마법 업그레이드 1레벨당 증가 | `12` |
| **레벨당 증가량** | Heal Bonus Per Level | 힐 업그레이드 1레벨당 증가 | `8` |
| **레벨당 증가량** | Enhanced Bonus Per Level | 강화 타일 업그레이드 1레벨당 증가 | `15` |

#### 최종 데미지 계산 공식

```
검  최종 데미지 = (baseSwordDamage + SwordLevel × swordBonusPerLevel) × 매칭 수 × (1 - swordResistance)
활  최종 데미지 = (baseBowDamage   + BowLevel   × bowBonusPerLevel)              × (1 - bowResistance)
마법 최종 데미지 = (baseWandDamage  + WandLevel  × wandBonusPerLevel) × 매칭 수 × (1 - wandResistance)
힐  최종 회복  = (baseHealAmount  + HealLevel  × healBonusPerLevel) × 매칭 수
```

> 업그레이드 전(레벨 0)에는 base 수치만 사용됩니다.

---

## STEP 2: 파티 캐릭터 체력 설정 (한 번만, 구 STEP 1)

> **어디서**: 배틀 씬 Hierarchy → `PartyPanel` 오브젝트 선택 → Inspector

| 항목 | 위치 | 설명 |
|------|------|------|
| **Max Hp Per Character** | `PartyHealthUI (Script)` | 파티 4명 공통 최대 HP. 예: `100` |
| **Slots [0~3]** | `PartyHealthUI (Script)` | 각 캐릭터 슬롯. Portrait Image / Health Bar Fill / Health Text 연결 |

> 캐릭터마다 HP가 다르게 하려면 `PartyHealthUI.cs`의 `SetHP(index, current, max)` 를 코드에서 호출해서 초기화하면 됩니다. 현재는 4명 동일 HP로 동작합니다.

---

## STEP 3: StageData 에셋 생성 (스테이지 추가할 때마다, 구 STEP 2)

> **어디서**: Project 창 → 원하는 폴더(예: `Assets/Data/Stages`) 우클릭 → **Create > Match3 > Stage Data**

에셋 이름 예시: `StageData_S1`, `StageData_S2`, ...

### Inspector에서 채울 항목

| 헤더 | 항목 | 설명 | 예시 |
|------|------|------|------|
| **표시** | Stage Name | 스테이지 이름 (표시용) | `"Stage 1 - 어둠의 숲"` |
| **표시** | Description | 설명 (선택) | `"첫 번째 전투"` |
| **배경** | Background Sprite | 배경 이미지 스프라이트 | 배경 이미지 드래그 |
| **보스/몬스터** | Boss Sprite | 몬스터 이미지 스프라이트 | 몬스터 이미지 드래그 |
| **보스/몬스터** | Monster Max Hp | 몬스터 최대 체력 | `500` |
| **몬스터 공격** | Attack Interval Turns | 공격 주기 (N턴마다 1회) | `4` |
| **몬스터 공격** | Attack Damage | 1인당 공격 데미지 | `20` |
| **몬스터 공격** | Attack Target Type | 공격 대상 | `All` / `SingleRandom` / `Random2` / `Random3` |
| **난이도** | Max Turns | 제한 턴 수 (초과 시 패배) | `40` |
| **저항력** | Sword Resistance | 검 공격 저항 (0~1). 0=저항 없음, 1=완전 면역 | `0` |
| **저항력** | Bow Resistance | 활 공격 저항 (0~1) | `0` |
| **저항력** | Wand Resistance | 마법(지팡이) 공격 저항 (0~1) | `0` |
> **StageData는 몬스터 정보만 관리합니다.** 플레이어 데미지/힐은 `CharacterStats` 에셋에서 설정합니다.

---

## STEP 4: StageDatabase에 등록 (스테이지 추가할 때마다, 구 STEP 3)

> **어디서**: Project 창에서 `StageDatabase` 에셋 선택 → Inspector

- `Stages` 배열의 **Element 0** = 스테이지 1, **Element 1** = 스테이지 2, ...
- 새 스테이지 추가 시 해당 인덱스 칸에 방금 만든 `StageData_Sn` 에셋을 드래그

```
StageDatabase Inspector:
  Stages (Size: 9)
    Element 0  ← StageData_S1 드래그
    Element 1  ← StageData_S2 드래그
    Element 2  ← (아직 미완성이면 비워둠)
    ...
```

> StageDatabase 에셋이 없으면:  
> Project 창 우클릭 → **Create > Match3 > Stage Database**  
> 이름을 `StageDatabase` 로 두고 `Assets/Resources/` 폴더에 넣으면 코드에서 자동 로드됩니다.

---

## STEP 5: SkillData 에셋 생성 (캐릭터 스킬 추가 시, 구 STEP 4)

> **어디서**: Project 창 우클릭 → **Create > Match3 > Skill Data**

| 항목 | 설명 | 예시 |
|------|------|------|
| Skill Id | 내부 식별 코드 (영문/숫자) | `"skill_heal_a"` |
| Display Name | 표시 이름 | `"치유의 빛"` |
| Description | 설명 | `"전원 HP 50 회복"` |
| Icon | 스킬 아이콘 스프라이트 | 아이콘 이미지 드래그 |
| Cooldown Seconds | 쿨타임 (초) | `8` |
| **Effect Type** | 스킬 효과 종류 선택 | `Sword` / `Bow` / `Wand` / `Heal` |
| **Effect Multiplier** | StageData 기본 수치 대비 배율 | `1.5` = 150% |
| Owner Character Index | 이 스킬을 가진 캐릭터 번호 (0~3) | `0` |

### Effect Type 설명

| Effect Type | 효과 | 적용 수치 | 저항 적용 |
|-------------|------|-----------|-----------|
| `Sword` | 몬스터에게 검 데미지 | `matchDamageSword × effectMultiplier` | `swordResistance` |
| `Bow` | 몬스터에게 활 데미지 | `matchDamageBow × effectMultiplier` | `bowResistance` |
| `Wand` | 몬스터에게 마법 데미지 | `matchDamageWand × effectMultiplier` | `wandResistance` |
| `Heal` | 파티 전원 HP 회복 | `matchHealCross × effectMultiplier` | 없음 |

#### 계산 예시

> StageData: `matchDamageSword = 30`, `swordResistance = 0.2`  
> SkillData: `effectType = Sword`, `effectMultiplier = 2.0`  
> → 스킬 발동 데미지 = `30 × 2.0 × (1 - 0.2)` = **48**

---

## STEP 6: SkillDatabase에 등록 (스킬 추가 시, 구 STEP 5)

> **어디서**: `SkillDatabase` 에셋 선택 → Inspector → `Skills` 배열에 추가

> SkillDatabase 에셋이 없으면:  
> Project 창 우클릭 → **Create > Match3 > Skill Database**

---

## STEP 7: 배틀 씬에서 스테이지 인덱스 넘기기 (구 STEP 6)

스테이지 선택 화면(맵 씬 등)에서 버튼 클릭 시:

```csharp
// 스테이지 1 진입 (인덱스 0)
BattleStageHolder.SetStage(0);
SceneManager.LoadScene("BattleScene");

// 스테이지 2 진입 (인덱스 1)
BattleStageHolder.SetStage(1);
SceneManager.LoadScene("BattleScene");
```

배틀 씬이 로드되면 **BattleSceneConfigApplier**가 자동으로 해당 스테이지 데이터를 읽어 배경/몬스터/턴 등을 적용합니다.

---

## 업그레이드 시스템 연동 방법 (나중에 업그레이드 UI 만들 때)

```csharp
// 업그레이드 구매 시 (예: 검 업그레이드)
CharacterUpgradeHolder.SwordLevel++;
CharacterUpgradeHolder.ApplyToSave(currentSaveData);
SaveSystem.Save(currentSaveData);

// 게임 시작/로드 시
GameSaveData save = SaveSystem.Load();
CharacterUpgradeHolder.LoadFromSave(save);
```

인덱스 상수 (`CharacterUpgradeHolder.Index*`):

| 상수 | 값 | 대상 |
|------|----|------|
| `IndexSword` | 0 | 검 |
| `IndexBow` | 1 | 활 |
| `IndexWand` | 2 | 마법 |
| `IndexHeal` | 3 | 힐 |
| `IndexEnhanced` | 4 | 강화 타일 |

---

## 스테이지 추가 체크리스트 (매번 반복)

스토리상 새 배틀 스테이지가 필요할 때마다:

- [ ] `StageData_Sn` 에셋 생성 (Create > Match3 > Stage Data)
- [ ] 배경 스프라이트 할당
- [ ] 몬스터 스프라이트 할당
- [ ] 몬스터 최대 HP 입력
- [ ] 공격 주기 / 데미지 / 공격 대상 입력
- [ ] 최대 턴 수 입력
- [ ] 저항력 설정 (0이면 저항 없음)
- [ ] 타일별 데미지/힐량 입력
- [ ] `StageDatabase` 에셋 열어서 해당 Element에 등록

---

## 데이터 보관 위치 권장

```
Assets/
  Data/
    Stages/
      StageData_S1.asset
      StageData_S2.asset
      ...
    Skills/
      SkillData_Heal.asset
      SkillData_Attack.asset
      ...
  Resources/
    CharacterStats.asset    ← 플레이어 기본 수치 (이름 CharacterStats 고정)
    StageDatabase.asset     ← 스테이지 DB (이름 StageDatabase 고정)
    SkillDatabase.asset
```

---

## 자주 묻는 것

### Q. 스테이지 1만 먼저 만들고 나머지는 나중에 추가해도 되나요?
네. StageDatabase의 Element 0에만 StageData_S1을 넣고, 나머지는 비워두면 됩니다.  
비어있는 인덱스로 진입하면 배경/몬스터가 바뀌지 않으므로 진입하지 않도록 관리만 해주세요.

### Q. 파티 캐릭터 4명의 초상화는 어디서 설정하나요?
배틀 씬 `PartyPanel` → `CharacterSlot0~3` → 각 `Portrait` Image 오브젝트의 **Source Image**에  
캐릭터 초상화 스프라이트를 직접 할당하면 됩니다. (런타임에 자동 할당되지 않음, Inspector에서 고정)

### Q. StageData의 데미지를 바꾸면 바로 반영되나요?
네. 런타임에 StageData 에셋의 값을 읽으므로, 에디터에서 값만 바꾸고 플레이하면 즉시 반영됩니다.  
(빌드 후에는 재빌드 필요)

### Q. 스테이지 9개를 다 채워야 하나요?
아니요. StageDatabase는 최대 9개를 지원하지만, 필요한 스테이지만 채우면 됩니다.  
현재 코드(`StageDatabase.StageCount = 9`)를 늘리면 9개 초과도 가능합니다.
