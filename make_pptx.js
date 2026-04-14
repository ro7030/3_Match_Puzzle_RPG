const pptxgen = require("C:/Users/EZ/AppData/Roaming/npm/node_modules/pptxgenjs");

let pres = new pptxgen();
pres.layout = 'LAYOUT_16x9';
pres.title = '메인 게임 루프 설계 - 3-Match Puzzle RPG';

const BG = "1a1a2e";
const GOLD = "f0a500";
const WHITE = "FFFFFF";
const DARK_CARD = "0f3460";
const GRAY = "8899aa";

// ============================================================
// SLIDE 1: TITLE
// ============================================================
let s1 = pres.addSlide();
s1.background = { color: BG };

s1.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 10, h: 0.08, fill: { color: GOLD }, line: { color: GOLD } });
s1.addShape(pres.shapes.RECTANGLE, { x: 0, y: 5.545, w: 10, h: 0.08, fill: { color: GOLD }, line: { color: GOLD } });

// Large decorative diamond/accent shape
s1.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0.08, w: 10, h: 5.465, fill: { color: BG }, line: { color: BG } });

// Centered title block
s1.addShape(pres.shapes.RECTANGLE, { x: 1.0, y: 1.5, w: 8.0, h: 2.8, fill: { color: "0f3460" }, line: { color: GOLD, width: 2 } });
s1.addShape(pres.shapes.RECTANGLE, { x: 1.0, y: 1.5, w: 8.0, h: 0.07, fill: { color: GOLD }, line: { color: GOLD } });
s1.addShape(pres.shapes.RECTANGLE, { x: 1.0, y: 4.23, w: 8.0, h: 0.07, fill: { color: GOLD }, line: { color: GOLD } });

s1.addText("메인 게임 루프 설계", {
  x: 1.0, y: 1.6, w: 8.0, h: 1.3,
  fontSize: 44, fontFace: "Arial Black", color: WHITE, bold: true, align: "center", valign: "middle"
});

s1.addText("3-Match Puzzle RPG", {
  x: 1.0, y: 2.9, w: 8.0, h: 0.7,
  fontSize: 24, fontFace: "Georgia", color: GOLD, align: "center", italic: true
});

s1.addText("GameManager  ·  TileSwapper  ·  MatchDetector  ·  TileClearer  ·  BattleSystem", {
  x: 1.0, y: 3.65, w: 8.0, h: 0.5,
  fontSize: 10, fontFace: "Consolas", color: GRAY, align: "center"
});

// ============================================================
// SLIDE 2: GAME LOOP OVERVIEW
// ============================================================
let s2 = pres.addSlide();
s2.background = { color: BG };
s2.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 10, h: 0.08, fill: { color: GOLD }, line: { color: GOLD } });

s2.addText("게임 루프 전체 흐름", {
  x: 0.3, y: 0.18, w: 9.4, h: 0.58,
  fontSize: 28, fontFace: "Arial Black", color: GOLD, bold: true, align: "left"
});

const steps = [
  { label: "플레이어\n입력", sub: "InputHandler", num: "01", color: "4488dd" },
  { label: "타일 스왑\n& 매칭 검사", sub: "TileSwapper\n+ MatchDetector", num: "02", color: "8855cc" },
  { label: "타일 제거\n& 전투 효과", sub: "TileClearer\n+ MatchEffectHandler", num: "03", color: "cc4444" },
  { label: "중력 &\n타일 생성", sub: "GravityController\n+ TileSpawner", num: "04", color: "448844" },
  { label: "연쇄 매칭\nor 턴 복귀", sub: "MatchDetector\n(재귀)", num: "05", color: GOLD },
];

const boxW = 1.62;
const boxH = 2.6;
const gap = 0.2;
const startX = 0.3;
const boxY = 0.95;

steps.forEach((step, i) => {
  const x = startX + i * (boxW + gap);
  s2.addShape(pres.shapes.RECTANGLE, { x, y: boxY, w: boxW, h: boxH, fill: { color: DARK_CARD }, line: { color: step.color, width: 2 } });
  s2.addShape(pres.shapes.RECTANGLE, { x, y: boxY, w: boxW, h: 0.4, fill: { color: step.color }, line: { color: step.color } });
  s2.addText(step.num, { x, y: boxY, w: boxW, h: 0.4, fontSize: 14, fontFace: "Arial Black", color: i === 4 ? BG : WHITE, bold: true, align: "center", valign: "middle" });
  s2.addText(step.label, { x: x + 0.05, y: boxY + 0.45, w: boxW - 0.1, h: 0.9, fontSize: 12, fontFace: "Arial", color: WHITE, bold: true, align: "center", valign: "middle" });
  s2.addText(step.sub, { x: x + 0.05, y: boxY + 1.45, w: boxW - 0.1, h: 0.8, fontSize: 9.5, fontFace: "Consolas", color: step.color, align: "center", valign: "top" });

  if (i < steps.length - 1) {
    s2.addText("▶", { x: x + boxW + 0.02, y: boxY + boxH / 2 - 0.2, w: gap + 0.06, h: 0.4, fontSize: 14, color: GOLD, align: "center", valign: "middle", bold: true });
  }
});

s2.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 3.72, w: 9.4, h: 0.55, fill: { color: "16213e" }, line: { color: GOLD, width: 1 } });
s2.addText("↺  연쇄 매칭 발생 시 단계 03 → 04 → 05 반복 (재귀) · 매칭 없으면 플레이어 턴으로 복귀", {
  x: 0.5, y: 3.72, w: 9.1, h: 0.55, fontSize: 11, fontFace: "Arial", color: GOLD, align: "center", valign: "middle"
});

s2.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 4.35, w: 9.4, h: 0.95, fill: { color: "0a0a1a" }, line: { color: "334466", width: 1 } });
s2.addText([
  { text: "한 턴 = ", options: { bold: true, color: GOLD } },
  { text: "타일 스왑 → 매칭 성공 시", options: { color: WHITE } },
  { text: "  |  ", options: { color: GRAY } },
  { text: "매칭 실패 시 턴 소모 없음", options: { color: "ff9966", bold: true } },
], { x: 0.5, y: 4.35, w: 9.1, h: 0.95, fontSize: 13, fontFace: "Arial", align: "center", valign: "middle" });

// ============================================================
// SLIDE 3: STATE MACHINE
// ============================================================
let s3 = pres.addSlide();
s3.background = { color: BG };
s3.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 10, h: 0.08, fill: { color: GOLD }, line: { color: GOLD } });

s3.addText("GameState — 11가지 상태 관리", {
  x: 0.3, y: 0.18, w: 9.4, h: 0.58, fontSize: 28, fontFace: "Arial Black", color: GOLD, bold: true, align: "left"
});

// Main loop row
const mainStates = [
  { name: "Playing", color: GOLD, textColor: BG, highlight: true },
  { name: "Swapping", color: DARK_CARD, textColor: WHITE, highlight: false },
  { name: "Matching", color: DARK_CARD, textColor: WHITE, highlight: false },
  { name: "Clearing", color: DARK_CARD, textColor: WHITE, highlight: false },
  { name: "Falling", color: DARK_CARD, textColor: WHITE, highlight: false },
  { name: "Spawning", color: DARK_CARD, textColor: WHITE, highlight: false },
];

const sW = 1.3;
const sH = 0.5;
const sStartX = 0.3;
const sGap = 0.22;
const sY = 1.1;

mainStates.forEach((state, i) => {
  const x = sStartX + i * (sW + sGap);
  s3.addShape(pres.shapes.RECTANGLE, { x, y: sY, w: sW, h: sH, fill: { color: state.color }, line: { color: GOLD, width: state.highlight ? 2.5 : 1 } });
  s3.addText(state.name, { x, y: sY, w: sW, h: sH, fontSize: 11, fontFace: "Consolas", color: state.textColor, bold: true, align: "center", valign: "middle" });
  if (i < mainStates.length - 1) {
    s3.addText("→", { x: x + sW + 0.01, y: sY, w: sGap + 0.08, h: sH, fontSize: 13, color: GOLD, align: "center", valign: "middle", bold: true });
  }
});

// Loop back notation
s3.addText("└─────────────────────────────────────── 연쇄 루프 ───────────────────────────────────────┘", {
  x: 0.3, y: sY + sH, w: 9.4, h: 0.35, fontSize: 8.5, fontFace: "Consolas", color: GOLD, align: "center"
});
s3.addText("↑ (Playing으로 복귀)", { x: 0.3, y: sY + sH + 0.32, w: 1.8, h: 0.28, fontSize: 8, color: GOLD, align: "left" });

// Side states section
s3.addText("특수 상태", { x: 0.3, y: 2.25, w: 2.0, h: 0.35, fontSize: 11, fontFace: "Arial Black", color: GRAY, bold: true });

// Paused
s3.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 2.65, w: 1.4, h: 0.48, fill: { color: "1a3a5c" }, line: { color: "4488cc", width: 1.5 } });
s3.addText("Paused", { x: 0.3, y: 2.65, w: 1.4, h: 0.48, fontSize: 11, fontFace: "Consolas", color: "88bbee", align: "center", valign: "middle" });
s3.addText("Playing ⇆ Paused  (일시정지 토글)", { x: 1.85, y: 2.65, w: 4.5, h: 0.48, fontSize: 11, color: GRAY, align: "left", valign: "middle" });

// GameOver
s3.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 3.3, w: 1.7, h: 0.48, fill: { color: "3c1515" }, line: { color: "cc4444", width: 1.5 } });
s3.addText("GameOver", { x: 0.3, y: 3.3, w: 1.7, h: 0.48, fontSize: 11, fontFace: "Consolas", color: "ff6666", align: "center", valign: "middle" });
s3.addText("파티 전멸 / 턴 소진 → 종료 (복귀 불가)", { x: 2.15, y: 3.3, w: 5.0, h: 0.48, fontSize: 11, color: "ff9999", align: "left", valign: "middle" });

// LevelComplete
s3.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 3.88, w: 2.1, h: 0.48, fill: { color: "0f3c1a" }, line: { color: "44cc77", width: 1.5 } });
s3.addText("LevelComplete", { x: 0.3, y: 3.88, w: 2.1, h: 0.48, fontSize: 11, fontFace: "Consolas", color: "66ffaa", align: "center", valign: "middle" });
s3.addText("몬스터 처치 → 클리어 (씬 전환)", { x: 2.55, y: 3.88, w: 4.5, h: 0.48, fontSize: 11, color: "aaffcc", align: "left", valign: "middle" });

// None / Menu
s3.addShape(pres.shapes.RECTANGLE, { x: 6.5, y: 2.65, w: 1.0, h: 0.48, fill: { color: "222222" }, line: { color: "666666", width: 1 } });
s3.addText("None", { x: 6.5, y: 2.65, w: 1.0, h: 0.48, fontSize: 10, fontFace: "Consolas", color: GRAY, align: "center", valign: "middle" });
s3.addShape(pres.shapes.RECTANGLE, { x: 7.7, y: 2.65, w: 1.0, h: 0.48, fill: { color: "222222" }, line: { color: "666666", width: 1 } });
s3.addText("Menu", { x: 7.7, y: 2.65, w: 1.0, h: 0.48, fontSize: 10, fontFace: "Consolas", color: GRAY, align: "center", valign: "middle" });
s3.addText("초기·메뉴 상태", { x: 6.5, y: 3.25, w: 2.5, h: 0.3, fontSize: 9, color: GRAY, align: "center", italic: true });

// Key insight
s3.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 4.55, w: 9.4, h: 0.75, fill: { color: "16213e" }, line: { color: GOLD, width: 1.5 } });
s3.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 4.55, w: 0.08, h: 0.75, fill: { color: GOLD }, line: { color: GOLD } });
s3.addText([
  { text: "Playing ", options: { bold: true, color: GOLD, fontFace: "Consolas" } },
  { text: "상태일 때만 플레이어 입력 허용 ", options: { color: WHITE } },
  { text: "→ ", options: { color: GOLD } },
  { text: "각 상태에서 정해진 처리만 실행 → ", options: { color: WHITE } },
  { text: "동시성 버그 원천 차단", options: { bold: true, color: GOLD } },
], { x: 0.55, y: 4.55, w: 9.1, h: 0.75, fontSize: 13, fontFace: "Arial", align: "left", valign: "middle" });

// ============================================================
// SLIDE 4: TileSwapper + MatchDetector
// ============================================================
let s4 = pres.addSlide();
s4.background = { color: BG };
s4.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 10, h: 0.08, fill: { color: GOLD }, line: { color: GOLD } });
s4.addText("TileSwapper + MatchDetector", { x: 0.3, y: 0.18, w: 9.4, h: 0.58, fontSize: 28, fontFace: "Arial Black", color: GOLD, bold: true, align: "left" });

// Left card
s4.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 0.95, w: 4.5, h: 3.8, fill: { color: DARK_CARD }, line: { color: "4488dd", width: 2 } });
s4.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 0.95, w: 4.5, h: 0.42, fill: { color: "4488dd" }, line: { color: "4488dd" } });
s4.addText("TileSwapper 동작 순서", { x: 0.3, y: 0.95, w: 4.5, h: 0.42, fontSize: 13, fontFace: "Arial Black", color: WHITE, bold: true, align: "center", valign: "middle" });

const swapSteps = [
  { text: "① GameState → Swapping 전환", color: WHITE },
  { text: "② 두 타일의 스프라이트/타입 교환", color: WHITE },
  { text: "③ 0.3초 대기 (스왑 애니메이션)", color: GRAY },
  { text: "④ MatchDetector.FindAllMatches() 호출", color: GOLD },
  { text: "⑤ 매칭 없음 → 원위치 후 Playing 복귀", color: "ff9966" },
  { text: "⑥ 매칭 있음 → 턴 +1, TileClearer 호출", color: "66ffaa" },
];
swapSteps.forEach((s, i) => {
  s4.addText(s.text, { x: 0.5, y: 1.47 + i * 0.52, w: 4.1, h: 0.46, fontSize: 12, fontFace: "Arial", color: s.color, align: "left", valign: "middle" });
});

// Right card
s4.addShape(pres.shapes.RECTANGLE, { x: 5.2, y: 0.95, w: 4.5, h: 3.8, fill: { color: DARK_CARD }, line: { color: "8855cc", width: 2 } });
s4.addShape(pres.shapes.RECTANGLE, { x: 5.2, y: 0.95, w: 4.5, h: 0.42, fill: { color: "8855cc" }, line: { color: "8855cc" } });
s4.addText("MatchDetector 핵심", { x: 5.2, y: 0.95, w: 4.5, h: 0.42, fontSize: 13, fontFace: "Arial Black", color: WHITE, bold: true, align: "center", valign: "middle" });

const matchItems = [
  { text: "가로/세로 3개 이상 동일 타입 검출", color: WHITE, mono: false },
  { text: "GetBaseType(t) = t % 4", color: GOLD, mono: true },
  { text: "강화 타일도 동일 기본 타입으로 처리", color: GRAY, mono: false },
  { text: "WouldCreateMatch()로 스왑 전 예측", color: WHITE, mono: false },
  { text: "FindHorizontalMatch + FindVerticalMatch", color: "8855cc", mono: true },
  { text: "중복 없는 MatchGroup 리스트 반환", color: WHITE, mono: false },
];
matchItems.forEach((m, i) => {
  s4.addText(m.text, { x: 5.4, y: 1.47 + i * 0.52, w: 4.1, h: 0.46, fontSize: 12, fontFace: m.mono ? "Consolas" : "Arial", color: m.color, align: "left", valign: "middle" });
});

// Bottom
s4.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 4.92, w: 9.4, h: 0.55, fill: { color: "16213e" }, line: { color: GOLD, width: 1.5 } });
s4.addText([
  { text: "매칭 실패 시 턴 소모 없음", options: { bold: true, color: GOLD } },
  { text: "  →  전략적 타일 배치와 콤보 노려보기 가능", options: { color: WHITE } },
], { x: 0.5, y: 4.92, w: 9.1, h: 0.55, fontSize: 13, fontFace: "Arial", align: "center", valign: "middle" });

// ============================================================
// SLIDE 5: TileClearer
// ============================================================
let s5 = pres.addSlide();
s5.background = { color: BG };
s5.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 10, h: 0.08, fill: { color: GOLD }, line: { color: GOLD } });
s5.addText("TileClearer — 클리어 & 연쇄 처리", { x: 0.3, y: 0.18, w: 9.4, h: 0.58, fontSize: 28, fontFace: "Arial Black", color: GOLD, bold: true, align: "left" });

const clearSteps = [
  { num: "1", text: "4매치 이상 감지 → 강화 타일 스폰 위치 예약", color: GOLD },
  { num: "2", text: "MatchEffectHandler → 전투 효과 즉시 적용", color: "aaddff" },
  { num: "3", text: "0.1초 간격으로 타일별 제거 애니메이션 실행", color: WHITE },
  { num: "4", text: "모든 타일 비우기 + 강화 타일 스폰", color: GOLD },
  { num: "5", text: "GravityController → 빈 칸 위의 타일 낙하", color: WHITE },
  { num: "6", text: "TileSpawner → 상단 빈 칸에 새 타일 채우기", color: WHITE },
  { num: "7", text: "MatchDetector 재검사 → 매칭 있으면 1번 반복 !", color: "66ffaa" },
  { num: "8", text: "매칭 없으면 → GameState.Playing 복귀", color: "ff9966" },
];

clearSteps.forEach((step, i) => {
  const col = i < 4 ? 0 : 1;
  const row = i % 4;
  const x = col === 0 ? 0.3 : 5.15;
  const y = 0.98 + row * 0.87;

  // Number badge
  s5.addShape(pres.shapes.OVAL, { x, y: y + 0.1, w: 0.4, h: 0.4, fill: { color: GOLD }, line: { color: GOLD } });
  s5.addText(step.num, { x, y: y + 0.1, w: 0.4, h: 0.4, fontSize: 13, fontFace: "Arial Black", color: BG, bold: true, align: "center", valign: "middle" });

  // Text
  s5.addText(step.text, { x: x + 0.52, y, w: 4.3, h: 0.62, fontSize: 12, fontFace: "Arial", color: step.color, align: "left", valign: "middle" });

  // Connector arrow (except last of each column)
  if (row < 3) {
    s5.addText("↓", { x: x + 0.1, y: y + 0.62, w: 0.3, h: 0.25, fontSize: 11, color: GOLD, align: "center" });
  }
});

// Center divider
s5.addShape(pres.shapes.LINE, { x: 5.0, y: 1.0, w: 0, h: 3.55, line: { color: "334466", width: 1, dashType: "dash" } });

// Recursive loop indicator
s5.addText("↺  연쇄 콤보!", { x: 4.15, y: 4.35, w: 1.7, h: 0.4, fontSize: 13, color: "66ffaa", bold: true, align: "center" });

// Bottom
s5.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 4.92, w: 9.4, h: 0.55, fill: { color: "16213e" }, line: { color: "66ffaa", width: 1.5 } });
s5.addText("재귀 구조로 콤보 연쇄 자동 처리 — 플레이어 개입 없이 연속 매칭 진행", { x: 0.5, y: 4.92, w: 9.1, h: 0.55, fontSize: 13, fontFace: "Arial", color: "66ffaa", bold: true, align: "center", valign: "middle" });

// ============================================================
// SLIDE 6: MatchEffectHandler
// ============================================================
let s6 = pres.addSlide();
s6.background = { color: BG };
s6.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 10, h: 0.08, fill: { color: GOLD }, line: { color: GOLD } });
s6.addText("MatchEffectHandler — 퍼즐을 전투로", { x: 0.3, y: 0.18, w: 9.4, h: 0.58, fontSize: 28, fontFace: "Arial Black", color: GOLD, bold: true, align: "left" });

const tileTypes = [
  { icon: "⚔", name: "검 (Sword)", type: "0", effect: "물리 광역 데미지", target: "몬스터 전체", accentColor: "cc3333" },
  { icon: "✦", name: "지팡이 (Wand)", type: "1", effect: "마법 광역 데미지", target: "몬스터 전체", accentColor: "8855cc" },
  { icon: "→", name: "활 (Bow)", type: "2", effect: "강력 단일 데미지", target: "몬스터 1체", accentColor: "448844" },
  { icon: "+", name: "십자가 (Cross)", type: "3", effect: "파티 전체 힐", target: "살아있는 파티원", accentColor: "2288cc" },
];

tileTypes.forEach((tile, i) => {
  const x = 0.3 + i * 2.35;
  const y = 0.95;
  s6.addShape(pres.shapes.RECTANGLE, { x, y, w: 2.2, h: 2.6, fill: { color: DARK_CARD }, line: { color: tile.accentColor, width: 2 } });
  s6.addShape(pres.shapes.RECTANGLE, { x, y, w: 2.2, h: 0.55, fill: { color: tile.accentColor }, line: { color: tile.accentColor } });
  s6.addText(tile.icon, { x, y: y + 0.02, w: 2.2, h: 0.51, fontSize: 22, color: WHITE, bold: true, align: "center", valign: "middle" });
  s6.addText(tile.name, { x: x + 0.05, y: y + 0.6, w: 2.1, h: 0.45, fontSize: 12, fontFace: "Arial Black", color: WHITE, bold: true, align: "center" });
  s6.addText(`타입 ${tile.type}`, { x: x + 0.05, y: y + 1.05, w: 2.1, h: 0.3, fontSize: 9.5, fontFace: "Consolas", color: GOLD, align: "center" });
  s6.addText(tile.effect, { x: x + 0.05, y: y + 1.4, w: 2.1, h: 0.4, fontSize: 11.5, fontFace: "Arial", color: WHITE, align: "center" });
  s6.addText(tile.target, { x: x + 0.05, y: y + 1.88, w: 2.1, h: 0.42, fontSize: 10.5, fontFace: "Arial", color: GRAY, align: "center", italic: true });
});

// Damage formula
s6.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 3.72, w: 9.4, h: 1.05, fill: { color: "0a0a20" }, line: { color: GOLD, width: 1.5 } });
s6.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 3.72, w: 0.07, h: 1.05, fill: { color: GOLD }, line: { color: GOLD } });
s6.addText("데미지 공식", { x: 0.5, y: 3.72, w: 2.0, h: 0.42, fontSize: 11, fontFace: "Arial Black", color: GOLD, bold: true, align: "left", valign: "middle" });
s6.addText("최종 데미지  =  ( 기본 데미지  ×  매칭 수  +  강화 보너스 )  ×  ( 1  −  저항력 )", {
  x: 0.5, y: 4.1, w: 9.1, h: 0.42, fontSize: 13, fontFace: "Consolas", color: WHITE, align: "center", valign: "middle"
});

s6.addText("업그레이드 레벨 (CharacterStatsResolver) · 강화 타일 보너스 · 스테이지 저항력 (StageData) 반영", {
  x: 0.3, y: 4.9, w: 9.4, h: 0.4, fontSize: 9.5, fontFace: "Arial", color: GRAY, align: "center", italic: true
});

// ============================================================
// SLIDE 7: Turn System & Monster Attack
// ============================================================
let s7 = pres.addSlide();
s7.background = { color: BG };
s7.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 10, h: 0.08, fill: { color: GOLD }, line: { color: GOLD } });
s7.addText("LevelManager + MonsterAttackController", { x: 0.3, y: 0.18, w: 9.4, h: 0.58, fontSize: 24, fontFace: "Arial Black", color: GOLD, bold: true, align: "left" });

// Left - Turn
s7.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 0.95, w: 4.4, h: 4.0, fill: { color: DARK_CARD }, line: { color: GOLD, width: 2 } });
s7.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 0.95, w: 4.4, h: 0.44, fill: { color: GOLD }, line: { color: GOLD } });
s7.addText("턴 시스템 (LevelManager)", { x: 0.3, y: 0.95, w: 4.4, h: 0.44, fontSize: 13, fontFace: "Arial Black", color: BG, bold: true, align: "center", valign: "middle" });

const turnItems = [
  { text: "매칭 성공 시에만 턴 +1 (LevelManager.IncrementMoves)", color: "66ffaa" },
  { text: "매칭 실패 → 턴 소모 없음", color: WHITE },
  { text: "최대 20턴 제한 (BattleScene 기준)", color: WHITE },
  { text: "OnTurnChanged 이벤트 발생", color: GOLD },
  { text: "→ UI / 몬스터공격 / 체력감소 모두 연동", color: GRAY },
  { text: "OnTurnsExhausted → 패배 조건 체크", color: "ff6666" },
];
turnItems.forEach((item, i) => {
  s7.addText(item.text, { x: 0.5, y: 1.52 + i * 0.52, w: 4.1, h: 0.46, fontSize: 11.5, fontFace: "Arial", color: item.color, align: "left", valign: "middle" });
});

// Right - Monster Attack
s7.addShape(pres.shapes.RECTANGLE, { x: 5.3, y: 0.95, w: 4.4, h: 4.0, fill: { color: DARK_CARD }, line: { color: "cc4444", width: 2 } });
s7.addShape(pres.shapes.RECTANGLE, { x: 5.3, y: 0.95, w: 4.4, h: 0.44, fill: { color: "cc4444" }, line: { color: "cc4444" } });
s7.addText("몬스터 공격 (MonsterAttackController)", { x: 5.3, y: 0.95, w: 4.4, h: 0.44, fontSize: 12, fontFace: "Arial Black", color: WHITE, bold: true, align: "center", valign: "middle" });

const monsterItems = [
  { text: "매 턴 파티 체력 지속 감소 (partyHpLossPerTurn)", color: "ff9966" },
  { text: "턴 수 % 공격간격 == 0 → 집중 공격 발동", color: WHITE },
  { text: "공격 대상: All / Single / Random2 / Random3", color: WHITE },
  { text: "페이즈 2 돌입 시 공격 패턴 변경 가능", color: GOLD },
  { text: "전투 시작 선공 옵션 (StartAttackRule)", color: GRAY },
  { text: "파티원 HP 0 → OnCharacterHpZero 발생", color: "ff6666" },
];
monsterItems.forEach((item, i) => {
  s7.addText(item.text, { x: 5.5, y: 1.52 + i * 0.52, w: 4.1, h: 0.46, fontSize: 11, fontFace: "Arial", color: item.color, align: "left", valign: "middle" });
});

// Bottom
s7.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 5.1, w: 9.4, h: 0.35, fill: { color: "16213e" }, line: { color: GOLD, width: 1 } });
s7.addText("플레이어는 빠른 매칭으로 몬스터를 공격, 몬스터는 일정 턴마다 반격 → 전략적 긴장감", { x: 0.5, y: 5.1, w: 9.1, h: 0.35, fontSize: 10.5, fontFace: "Arial", color: GOLD, align: "center", valign: "middle" });

// ============================================================
// SLIDE 8: WIN/LOSS CONDITIONS
// ============================================================
let s8 = pres.addSlide();
s8.background = { color: BG };
s8.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 10, h: 0.08, fill: { color: GOLD }, line: { color: GOLD } });
s8.addText("승패 판정 시스템", { x: 0.3, y: 0.18, w: 9.4, h: 0.58, fontSize: 28, fontFace: "Arial Black", color: GOLD, bold: true, align: "left" });

const conditions = [
  { title: "패배 1", subtitle: "파티원 전멸", accentColor: "cc3333", items: ["PartyHealthUI.OnCharacterHpZero", "GameState → GameOver", "DefeatPanel 표시"], mark: "X" },
  { title: "패배 2", subtitle: "턴 소진 (몬스터 생존)", accentColor: "cc7733", items: ["LevelManager.OnTurnsExhausted", "몬스터 HP > 0 → GameOver", "DefeatPanel 표시"], mark: "X" },
  { title: "승리!", subtitle: "몬스터 체력 0", accentColor: "33aa55", items: ["MonsterHealthUI.OnDied", "GameState → LevelComplete", "클리어 통계 & 보상 표시"], mark: "O" },
];

conditions.forEach((cond, i) => {
  const x = 0.3 + i * 3.22;
  s8.addShape(pres.shapes.RECTANGLE, { x, y: 1.0, w: 3.05, h: 3.4, fill: { color: DARK_CARD }, line: { color: cond.accentColor, width: 2.5 } });
  s8.addShape(pres.shapes.RECTANGLE, { x, y: 1.0, w: 3.05, h: 0.56, fill: { color: cond.accentColor }, line: { color: cond.accentColor } });
  s8.addText(`${cond.mark}  ${cond.title}`, { x, y: 1.0, w: 3.05, h: 0.56, fontSize: 17, fontFace: "Arial Black", color: WHITE, bold: true, align: "center", valign: "middle" });
  s8.addText(cond.subtitle, { x: x + 0.1, y: 1.62, w: 2.85, h: 0.48, fontSize: 13.5, fontFace: "Arial", color: WHITE, bold: true, align: "center" });
  cond.items.forEach((item, j) => {
    s8.addText(item, { x: x + 0.15, y: 2.2 + j * 0.58, w: 2.75, h: 0.52, fontSize: 11, fontFace: j === 0 ? "Consolas" : "Arial", color: j === 0 ? GOLD : WHITE, align: "left", valign: "middle" });
  });
});

// Bottom
s8.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 4.55, w: 9.4, h: 0.75, fill: { color: "16213e" }, line: { color: GOLD, width: 1.5 } });
s8.addText([
  { text: "클리어 후 흐름:  ", options: { bold: true, color: GOLD } },
  { text: "컷씬 재생 (선택)  →  MapScene 이동  →  골드 보상 지급  →  레벨 & 업그레이드 포인트 획득", options: { color: WHITE } },
], { x: 0.5, y: 4.55, w: 9.1, h: 0.75, fontSize: 12, fontFace: "Arial", align: "center", valign: "middle" });

// ============================================================
// SLIDE 9: ARCHITECTURE LAYERS
// ============================================================
let s9 = pres.addSlide();
s9.background = { color: BG };
s9.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 10, h: 0.08, fill: { color: GOLD }, line: { color: GOLD } });
s9.addText("시스템 아키텍처 — 5계층 구조", { x: 0.3, y: 0.18, w: 9.4, h: 0.58, fontSize: 28, fontFace: "Arial Black", color: GOLD, bold: true, align: "left" });

const layers = [
  { name: "입력층", classes: "InputHandler", labelColor: "4488dd", bgColor: "4488dd" },
  { name: "퍼즐 처리층", classes: "TileSwapper  ·  MatchDetector  ·  TileClearer  ·  GravityController  ·  TileSpawner", labelColor: "8855cc", bgColor: "8855cc" },
  { name: "전투층", classes: "MatchEffectHandler  ·  MonsterAttackController  ·  MonsterHealthUI  ·  PartyHealthUI", labelColor: "cc4444", bgColor: "cc4444" },
  { name: "게임 관리층", classes: "GameManager  ·  LevelManager  ·  ScoreManager  ·  UIManager  ·  BattlePhaseRuntime", labelColor: "448844", bgColor: "448844" },
  { name: "데이터층", classes: "StageData  ·  CharacterStatsResolver  ·  BattleClearStatsRuntime  ·  EquipmentStatModifier", labelColor: GOLD, bgColor: GOLD },
];

layers.forEach((layer, i) => {
  const y = 0.98 + i * 0.79;
  s9.addShape(pres.shapes.RECTANGLE, { x: 0.3, y, w: 1.7, h: 0.6, fill: { color: layer.bgColor }, line: { color: layer.bgColor } });
  s9.addText(layer.name, { x: 0.3, y, w: 1.7, h: 0.6, fontSize: 11, fontFace: "Arial Black", color: layer.bgColor === GOLD ? BG : WHITE, bold: true, align: "center", valign: "middle" });
  s9.addShape(pres.shapes.RECTANGLE, { x: 2.1, y, w: 7.6, h: 0.6, fill: { color: DARK_CARD }, line: { color: layer.bgColor, width: 1 } });
  s9.addText(layer.classes, { x: 2.2, y, w: 7.4, h: 0.6, fontSize: 11, fontFace: "Consolas", color: layer.bgColor === GOLD ? GOLD : WHITE, align: "left", valign: "middle" });
  if (i < layers.length - 1) {
    s9.addText("↕", { x: 0.75, y: y + 0.6, w: 0.7, h: 0.19, fontSize: 9, color: GRAY, align: "center" });
  }
});

// Event annotation
s9.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 5.0, w: 9.4, h: 0.4, fill: { color: "16213e" }, line: { color: GOLD, width: 1 } });
s9.addText("이벤트(OnTurnChanged, OnDied, OnCharacterHpZero 등)로 계층 간 직접 참조 없이 통신 → 결합도 최소화", {
  x: 0.5, y: 5.0, w: 9.1, h: 0.4, fontSize: 10, fontFace: "Arial", color: GRAY, align: "center", valign: "middle", italic: true
});

// ============================================================
// SLIDE 10: KEY DESIGN POINTS
// ============================================================
let s10 = pres.addSlide();
s10.background = { color: BG };
s10.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 10, h: 0.08, fill: { color: GOLD }, line: { color: GOLD } });
s10.addShape(pres.shapes.RECTANGLE, { x: 0, y: 5.545, w: 10, h: 0.08, fill: { color: GOLD }, line: { color: GOLD } });
s10.addText("설계 핵심 포인트", { x: 0.3, y: 0.18, w: 9.4, h: 0.58, fontSize: 28, fontFace: "Arial Black", color: GOLD, bold: true, align: "left" });

const points = [
  { num: "01", title: "상태 머신 (State Machine)", desc: "11가지 GameState로 단계별 처리 분리\n→ 동시 입력/처리 충돌 원천 방지", accentColor: "4488dd" },
  { num: "02", title: "이벤트 기반 연동", desc: "OnTurnChanged, OnDied 등 C# 이벤트\n→ 계층 간 느슨한 결합 유지", accentColor: "8855cc" },
  { num: "03", title: "재귀 연쇄 처리", desc: "TileClearer.ClearMatches()가 자신 호출\n→ 콤보 연쇄 완전 자동화", accentColor: "448844" },
  { num: "04", title: "데이터 분리 (ScriptableObject)", desc: "StageData로 스테이지별 밸런싱 외부화\n→ 코드 수정 없이 기획 변경 가능", accentColor: GOLD },
];

points.forEach((pt, i) => {
  const row = Math.floor(i / 2);
  const col = i % 2;
  const x = 0.3 + col * 4.85;
  const y = 1.1 + row * 1.9;
  s10.addShape(pres.shapes.RECTANGLE, { x, y, w: 4.6, h: 1.72, fill: { color: DARK_CARD }, line: { color: pt.accentColor, width: 2 } });
  s10.addShape(pres.shapes.RECTANGLE, { x, y, w: 0.08, h: 1.72, fill: { color: pt.accentColor }, line: { color: pt.accentColor } });
  s10.addText(pt.num, { x: x + 0.18, y: y + 0.08, w: 0.65, h: 0.48, fontSize: 22, fontFace: "Arial Black", color: pt.accentColor, bold: true, align: "left", valign: "middle" });
  s10.addText(pt.title, { x: x + 0.18, y: y + 0.52, w: 4.3, h: 0.44, fontSize: 13, fontFace: "Arial Black", color: WHITE, bold: true, align: "left" });
  s10.addText(pt.desc, { x: x + 0.22, y: y + 0.98, w: 4.25, h: 0.65, fontSize: 11, fontFace: "Arial", color: GRAY, align: "left", valign: "top" });
});

// Final tagline
s10.addShape(pres.shapes.RECTANGLE, { x: 0.3, y: 5.0, w: 9.4, h: 0.38, fill: { color: "16213e" }, line: { color: GOLD, width: 1.5 } });
s10.addText("퍼즐의 재미  +  RPG 전략성을  메인 게임 루프 설계로 하나로 연결", {
  x: 0.5, y: 5.0, w: 9.1, h: 0.38, fontSize: 13, fontFace: "Georgia", color: GOLD, bold: true, align: "center", valign: "middle", italic: true
});

// ============================================================
// SAVE
// ============================================================
pres.writeFile({ fileName: "C:\\UnityProject\\3_Match_Puzzle_RPG\\MainGameLoop_Presentation.pptx" })
  .then(() => console.log("Done! Saved to C:\\UnityProject\\3_Match_Puzzle_RPG\\MainGameLoop_Presentation.pptx"))
  .catch(e => { console.error("Error:", e); process.exit(1); });
