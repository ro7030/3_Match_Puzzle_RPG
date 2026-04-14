const fs = require("fs");
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
  Header, Footer, AlignmentType, LevelFormat,
  HeadingLevel, BorderStyle, WidthType, ShadingType,
  PageNumber, PageBreak, VerticalAlign
} = require("docx");

// ── 색상 팔레트 ──────────────────────────────────────────────────
const C = {
  navy:    "1A3764", blue:   "2E5090", skyBlue: "3A6EA5",
  teal:    "1B6B72", green:  "1E6B3A", gold:    "B8860B",
  red:     "8B1A1A", purple: "5C2D91", gray:    "595959",
  light:   "F2F7FB", altRow: "EAF3FB", subHead: "D6E4F0",
  white:   "FFFFFF", border: "CCCCCC", accent:  "E8F4FD",
  tut:     "E8F5E9", ch1:    "E3F2FD", ch2:     "FFF3E0", ch3:     "FCE4EC",
  tutH:    "2E7D32", ch1H:   "1565C0", ch2H:    "E65100", ch3H:    "880E4F",
};

// ── 공통 보더/마진 ────────────────────────────────────────────────
const bdr  = c => ({ style: BorderStyle.SINGLE, size: 1, color: c || C.border });
const bdrT = c => ({ style: BorderStyle.SINGLE, size: 3, color: c || C.blue });
const mkBorders = c => ({ top: bdr(c), bottom: bdr(c), left: bdr(c), right: bdr(c) });
const mkBordersT = c => ({ top: bdrT(c), bottom: bdrT(c), left: bdrT(c), right: bdrT(c) });
const CM = { top: 70, bottom: 70, left: 110, right: 110 };
const CMw = { top: 70, bottom: 70, left: 140, right: 140 };

// ── 셀 헬퍼 ────────────────────────────────────────────────────
function hCell(text, w, color) {
  return new TableCell({
    borders: mkBordersT(color || C.blue),
    width: { size: w, type: WidthType.DXA },
    shading: { fill: color || C.blue, type: ShadingType.CLEAR },
    margins: CM, verticalAlign: VerticalAlign.CENTER,
    children: [new Paragraph({ alignment: AlignmentType.CENTER,
      children: [new TextRun({ text, bold: true, color: C.white, font: "맑은 고딕", size: 19 })] })]
  });
}
function dCell(text, w, opts = {}) {
  return new TableCell({
    borders: mkBorders(opts.borderColor || C.border),
    width: { size: w, type: WidthType.DXA },
    shading: opts.fill ? { fill: opts.fill, type: ShadingType.CLEAR } : undefined,
    margins: opts.wide ? CMw : CM, verticalAlign: VerticalAlign.CENTER,
    children: [new Paragraph({
      alignment: opts.center ? AlignmentType.CENTER : AlignmentType.LEFT,
      children: [new TextRun({ text: String(text), font: "맑은 고딕", size: 19,
        bold: opts.bold || false, color: opts.color || "000000" })] })]
  });
}
function shCell(text, w, color) {
  return new TableCell({
    borders: mkBorders(color || C.skyBlue),
    width: { size: w, type: WidthType.DXA },
    shading: { fill: C.subHead, type: ShadingType.CLEAR },
    margins: CM, verticalAlign: VerticalAlign.CENTER,
    children: [new Paragraph({ alignment: AlignmentType.CENTER,
      children: [new TextRun({ text, bold: true, font: "맑은 고딕", size: 19 })] })]
  });
}

// ── 텍스트 헬퍼 ───────────────────────────────────────────────
const H1 = t => new Paragraph({ heading: HeadingLevel.HEADING_1,
  spacing: { before: 360, after: 180 },
  children: [new TextRun({ text: t, font: "맑은 고딕" })] });
const H2 = t => new Paragraph({ heading: HeadingLevel.HEADING_2,
  spacing: { before: 280, after: 140 },
  children: [new TextRun({ text: t, font: "맑은 고딕" })] });
const H3 = t => new Paragraph({ heading: HeadingLevel.HEADING_3,
  spacing: { before: 200, after: 100 },
  children: [new TextRun({ text: t, font: "맑은 고딕" })] });
const P = (t, opts = {}) => new Paragraph({
  spacing: { after: opts.after || 100, line: 340 },
  indent: opts.indent ? { left: opts.indent } : undefined,
  children: [new TextRun({ text: t, font: "맑은 고딕", size: opts.size || 21,
    bold: opts.bold || false, italics: opts.italic || false, color: opts.color || "000000" })] });
const BL = (t, lvl = 0) => new Paragraph({
  numbering: { reference: "bullets", level: lvl },
  spacing: { after: 80, line: 330 },
  children: [new TextRun({ text: t, font: "맑은 고딕", size: 21 })] });
const NL = (t) => new Paragraph({
  numbering: { reference: "numbers", level: 0 },
  spacing: { after: 80, line: 330 },
  children: [new TextRun({ text: t, font: "맑은 고딕", size: 21 })] });
const SP = () => new Paragraph({ spacing: { after: 80 }, children: [] });

// ── 구분선 단락 ───────────────────────────────────────────────
const HR = (color) => new Paragraph({
  spacing: { before: 120, after: 120 },
  border: { bottom: { style: BorderStyle.SINGLE, size: 4, color: color || C.blue, space: 2 } },
  children: [] });

// ── 단순 2열 테이블 ───────────────────────────────────────────
function infoTable(rows, w1 = 3200, w2 = 5826) {
  return new Table({
    width: { size: w1 + w2, type: WidthType.DXA },
    columnWidths: [w1, w2],
    rows: rows.map(([k, v], i) => new TableRow({ children: [
      dCell(k, w1, { fill: i % 2 === 0 ? C.accent : C.white, bold: true, center: true }),
      dCell(v, w2, { fill: i % 2 === 0 ? C.white : C.light }),
    ]}))
  });
}

// ── 스테이지 상세 카드 ──────────────────────────────────────────
function stageCard(data, accentColor) {
  const TW = 9026;
  const c1 = 2200, c2 = 1606, c3 = 1606, c4 = 1606, c5 = 2008;

  const rows = [];
  // 헤더행
  rows.push(new TableRow({ children: [
    hCell("항목", c1, accentColor),
    hCell("Phase 1", c2 + c3, accentColor),
    hCell("Phase 2", c4 + c5, accentColor),
  ]}));
  // 몬스터 HP
  rows.push(new TableRow({ children: [
    dCell("몬스터 최대 HP", c1, { fill: C.accent, bold: true }),
    new TableCell({ borders: mkBorders(), width: { size: c2 + c3, type: WidthType.DXA },
      shading: { fill: C.white, type: ShadingType.CLEAR }, margins: CM, verticalAlign: VerticalAlign.CENTER,
      columnSpan: 2,
      children: [new Paragraph({ alignment: AlignmentType.CENTER,
        children: [new TextRun({ text: data.hp.toLocaleString(), font: "맑은 고딕", size: 21, bold: true, color: C.red })] })] }),
    new TableCell({ borders: mkBorders(), width: { size: c4 + c5, type: WidthType.DXA },
      shading: { fill: C.white, type: ShadingType.CLEAR }, margins: CM, verticalAlign: VerticalAlign.CENTER,
      columnSpan: 2,
      children: [new Paragraph({ alignment: AlignmentType.CENTER,
        children: [new TextRun({ text: data.hp2 ? data.hp2.toLocaleString() : "없음", font: "맑은 고딕", size: 21,
          bold: !!data.hp2, color: data.hp2 ? C.red : C.gray })] })] }),
  ]}));
  // 공격 간격 / 데미지
  rows.push(new TableRow({ children: [
    dCell("공격 간격 (턴)", c1, { fill: C.light, bold: true }),
    dCell(`${data.atkInterval}턴마다`, c2 + c3, { fill: C.white, center: true }),
    dCell(data.atkInterval2 ? `${data.atkInterval2}턴마다` : "없음", c4 + c5, { fill: C.white, center: true, color: data.atkInterval2 ? "000000" : C.gray }),
  ]}));
  rows.push(new TableRow({ children: [
    dCell("공격 데미지", c1, { fill: C.accent, bold: true }),
    dCell(`${data.atkDmg} (${data.atkType})`, c2 + c3, { fill: C.white, center: true }),
    dCell(data.atkDmg2 ? `${data.atkDmg2} (${data.atkType2})` : "없음", c4 + c5, { fill: C.white, center: true, color: data.atkDmg2 ? "000000" : C.gray }),
  ]}));
  // 저항값
  rows.push(new TableRow({ children: [
    shCell("저항값", c1),
    shCell("검", c2, accentColor), shCell("활", c3, accentColor),
    shCell("검", c4, accentColor), shCell("활", c5, accentColor),
  ]}));
  rows.push(new TableRow({ children: [
    dCell("검 / 활 / 완드", c1, { fill: C.light, bold: true }),
    dCell(`${data.rSword}%`, c2, { center: true, fill: data.rSword > 0 ? "#FFF0F0" : C.white, color: data.rSword > 0 ? C.red : C.green }),
    dCell(`${data.rBow}%`, c3, { center: true, fill: data.rBow > 0 ? "#FFF0F0" : C.white, color: data.rBow > 0 ? C.red : C.green }),
    dCell(data.rSword2 !== undefined ? `${data.rSword2}%` : "-", c4, { center: true, color: data.rSword2 > 0 ? C.red : C.green }),
    dCell(data.rBow2 !== undefined ? `${data.rBow2}%` : "-", c5, { center: true, color: data.rBow2 > 0 ? C.red : C.green }),
  ]}));
  rows.push(new TableRow({ children: [
    dCell("완드 저항", c1, { fill: C.accent, bold: true }),
    new TableCell({ borders: mkBorders(), width: { size: c2 + c3, type: WidthType.DXA },
      shading: { fill: data.rWand > 0 ? "#FFF0F0" : C.white, type: ShadingType.CLEAR },
      margins: CM, verticalAlign: VerticalAlign.CENTER, columnSpan: 2,
      children: [new Paragraph({ alignment: AlignmentType.CENTER,
        children: [new TextRun({ text: `${data.rWand}%`, font: "맑은 고딕", size: 21, color: data.rWand > 0 ? C.red : C.green })] })] }),
    new TableCell({ borders: mkBorders(), width: { size: c4 + c5, type: WidthType.DXA },
      shading: { fill: data.rWand2 > 0 ? "#FFF0F0" : C.white, type: ShadingType.CLEAR },
      margins: CM, verticalAlign: VerticalAlign.CENTER, columnSpan: 2,
      children: [new Paragraph({ alignment: AlignmentType.CENTER,
        children: [new TextRun({ text: data.rWand2 !== undefined ? `${data.rWand2}%` : "-", font: "맑은 고딕", size: 21, color: data.rWand2 > 0 ? C.red : C.green })] })] }),
  ]}));
  // 기타
  rows.push(new TableRow({ children: [
    dCell("최대 턴 수", c1, { fill: C.light, bold: true }),
    new TableCell({ borders: mkBorders(), width: { size: c2+c3+c4+c5, type: WidthType.DXA },
      shading: { fill: C.white, type: ShadingType.CLEAR }, margins: CM, verticalAlign: VerticalAlign.CENTER, columnSpan: 4,
      children: [new Paragraph({ alignment: AlignmentType.CENTER,
        children: [new TextRun({ text: `${data.maxTurns}턴`, font: "맑은 고딕", size: 21 })] })] }),
  ]}));
  rows.push(new TableRow({ children: [
    dCell("클리어 보상 골드", c1, { fill: C.accent, bold: true }),
    new TableCell({ borders: mkBorders(), width: { size: c2+c3+c4+c5, type: WidthType.DXA },
      shading: { fill: C.white, type: ShadingType.CLEAR }, margins: CM, verticalAlign: VerticalAlign.CENTER, columnSpan: 4,
      children: [new Paragraph({ alignment: AlignmentType.CENTER,
        children: [new TextRun({ text: `${data.gold.toLocaleString()}G`, font: "맑은 고딕", size: 21, bold: true, color: C.gold })] })] }),
  ]}));
  if (data.startHpDeduct || data.hpLossPerTurn) {
    rows.push(new TableRow({ children: [
      dCell("파티 패널티", c1, { fill: C.light, bold: true }),
      new TableCell({ borders: mkBorders(), width: { size: c2+c3+c4+c5, type: WidthType.DXA },
        shading: { fill: "#FFF8E1", type: ShadingType.CLEAR }, margins: CM, verticalAlign: VerticalAlign.CENTER, columnSpan: 4,
        children: [new Paragraph({
          children: [new TextRun({ text: [
            data.startHpDeduct ? `시작 HP -${data.startHpDeduct}` : "",
            data.hpLossPerTurn ? `  /  턴당 HP -${data.hpLossPerTurn}` : ""
          ].filter(Boolean).join(""), font: "맑은 고딕", size: 21, color: C.red })] })] }),
    ]}));
  }
  if (data.phase2Trigger) {
    rows.push(new TableRow({ children: [
      dCell("페이즈 2 트리거", c1, { fill: C.accent, bold: true }),
      new TableCell({ borders: mkBorders(), width: { size: c2+c3+c4+c5, type: WidthType.DXA },
        shading: { fill: "#FFF0F0", type: ShadingType.CLEAR }, margins: CM, verticalAlign: VerticalAlign.CENTER, columnSpan: 4,
        children: [new Paragraph({
          children: [new TextRun({ text: `몬스터 HP ≤ ${data.phase2Trigger.toLocaleString()} 이하 시 전환`, font: "맑은 고딕", size: 21, bold: true, color: C.red })] })] }),
    ]}));
  }
  if (data.notes) {
    rows.push(new TableRow({ children: [
      dCell("비고", c1, { fill: C.light, bold: true }),
      new TableCell({ borders: mkBorders(), width: { size: c2+c3+c4+c5, type: WidthType.DXA },
        shading: { fill: C.white, type: ShadingType.CLEAR }, margins: CM, verticalAlign: VerticalAlign.CENTER, columnSpan: 4,
        children: [new Paragraph({ children: [new TextRun({ text: data.notes, font: "맑은 고딕", size: 19, italics: true, color: C.gray })] })] }),
    ]}));
  }

  return new Table({
    width: { size: TW, type: WidthType.DXA },
    columnWidths: [c1, c2, c3, c4, c5],
    rows
  });
}

// ── 페이지 속성 ───────────────────────────────────────────────
const pageProps = {
  size: { width: 11906, height: 16838 },
  margin: { top: 1200, right: 1200, bottom: 1200, left: 1200 }
};
const makeHeader = () => new Header({ children: [new Paragraph({
  alignment: AlignmentType.RIGHT,
  children: [new TextRun({ text: "3 Match Puzzle RPG  |  레벨 기획서", font: "맑은 고딕", size: 17, color: "AAAAAA", italics: true })] })] });
const makeFooter = () => new Footer({ children: [new Paragraph({
  alignment: AlignmentType.CENTER,
  children: [
    new TextRun({ text: "- ", font: "맑은 고딕", size: 17, color: "AAAAAA" }),
    new TextRun({ children: [PageNumber.CURRENT], font: "맑은 고딕", size: 17, color: "AAAAAA" }),
    new TextRun({ text: " -", font: "맑은 고딕", size: 17, color: "AAAAAA" }),
  ] })] });

// ═══════════════════════════════════════════════════════════════
const doc = new Document({
  styles: {
    default: { document: { run: { font: "맑은 고딕", size: 21 } } },
    paragraphStyles: [
      { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 36, bold: true, font: "맑은 고딕", color: C.navy },
        paragraph: { spacing: { before: 360, after: 180 }, outlineLevel: 0,
          border: { bottom: { style: BorderStyle.SINGLE, size: 6, color: C.blue, space: 4 } } } },
      { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 29, bold: true, font: "맑은 고딕", color: C.blue },
        paragraph: { spacing: { before: 260, after: 130 }, outlineLevel: 1 } },
      { id: "Heading3", name: "Heading 3", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 24, bold: true, font: "맑은 고딕", color: C.skyBlue },
        paragraph: { spacing: { before: 180, after: 100 }, outlineLevel: 2 } },
    ]
  },
  numbering: {
    config: [
      { reference: "bullets", levels: [
        { level: 0, format: LevelFormat.BULLET, text: "\u2022", alignment: AlignmentType.LEFT,
          style: { paragraph: { indent: { left: 720, hanging: 360 } } } },
        { level: 1, format: LevelFormat.BULLET, text: "\u25E6", alignment: AlignmentType.LEFT,
          style: { paragraph: { indent: { left: 1440, hanging: 360 } } } },
      ]},
      { reference: "numbers", levels: [
        { level: 0, format: LevelFormat.DECIMAL, text: "%1.", alignment: AlignmentType.LEFT,
          style: { paragraph: { indent: { left: 720, hanging: 360 } } } },
      ]},
    ]
  },
  sections: [
    // ══════════════════════════════════════════════
    // [표지]
    // ══════════════════════════════════════════════
    {
      properties: { page: pageProps },
      children: [
        SP(), SP(), SP(), SP(), SP(), SP(), SP(), SP(),
        new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 160 },
          children: [new TextRun({ text: "3 Match Puzzle RPG", font: "맑은 고딕", size: 60, bold: true, color: C.navy })] }),
        new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 600 },
          border: { bottom: { style: BorderStyle.SINGLE, size: 10, color: C.blue, space: 10 } },
          children: [new TextRun({ text: "레 벨  기 획 서", font: "맑은 고딕", size: 46, bold: true, color: C.blue })] }),
        SP(), SP(),
        new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 100 },
          children: [new TextRun({ text: "버전: v1.0     작성일: 2026년 4월 14일", font: "맑은 고딕", size: 23, color: "777777" })] }),
        new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 100 },
          children: [new TextRun({ text: "총 스테이지: 10개 (Tutorial + Chapter 1~3)", font: "맑은 고딕", size: 23, color: "777777" })] }),
        new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 100 },
          children: [new TextRun({ text: "플랫폼: Unity Mobile / PC", font: "맑은 고딕", size: 23, color: "777777" })] }),
        SP(), SP(), SP(),
        // 챕터 요약 박스
        new Table({
          width: { size: 7026, type: WidthType.DXA }, columnWidths: [1756, 1756, 1757, 1757],
          rows: [
            new TableRow({ children: [
              hCell("Tutorial", 1756, C.teal), hCell("Chapter 1", 1756, C.ch1H),
              hCell("Chapter 2", 1757, C.ch2H), hCell("Chapter 3", 1757, C.ch3H),
            ]}),
            new TableRow({ children: [
              dCell("Stage 0", 1756, { center: true, fill: C.tut }),
              dCell("Stage 1~3", 1756, { center: true, fill: C.ch1 }),
              dCell("Stage 4~6", 1757, { center: true, fill: C.ch2 }),
              dCell("Stage 7~9", 1757, { center: true, fill: C.ch3 }),
            ]}),
            new TableRow({ children: [
              dCell("HP 100", 1756, { center: true, fill: C.tut }),
              dCell("HP 1,000~3,000", 1756, { center: true, fill: C.ch1 }),
              dCell("HP 2,500~5,000", 1757, { center: true, fill: C.ch2 }),
              dCell("HP 6,000~10,000", 1757, { center: true, fill: C.ch3 }),
            ]}),
          ]
        }),
      ]
    },
    // ══════════════════════════════════════════════
    // [목차 + 개요]
    // ══════════════════════════════════════════════
    {
      properties: { page: pageProps },
      headers: { default: makeHeader() }, footers: { default: makeFooter() },
      children: [
        H1("목 차"),
        ...[
          "1. 레벨 기획서 개요",
          "2. 전체 스테이지 구성 한눈에 보기",
          "3. Tutorial - Stage 0",
          "4. Chapter 1 - Stage 1 ~ 3",
          "5. Chapter 2 - Stage 4 ~ 6",
          "6. Chapter 3 - Stage 7 ~ 9",
          "7. 캐릭터 기본 스탯 & 업그레이드 시스템",
          "8. 스킬 데이터",
          "9. 장비 데이터",
          "10. 밸런스 설계 가이드",
          "11. 챕터 잠금해제 & 보상 흐름",
        ].map(t => P(t, { after: 120 })),
        SP(),
        HR(),
        SP(),
        H1("1. 레벨 기획서 개요"),
        P("본 문서는 3 Match Puzzle RPG의 레벨 설계 전반을 기술한다. 게임은 총 10개의 스테이지로 구성되며, Tutorial(0) + Chapter 1(1~3) + Chapter 2(4~6) + Chapter 3(7~9)으로 분류된다."),
        SP(),
        H2("1.1 레벨 설계 목표"),
        BL("초반 진입 장벽을 낮춰 퍼즐 & 전투 시스템을 자연스럽게 학습"),
        BL("챕터가 올라갈수록 몬스터 HP·저항·공격 강도를 점진적으로 상승"),
        BL("Chapter 2·3에서 특정 타입 저항을 부여해 전략적 타일 선택 유도"),
        BL("Chapter 3에서 파티 HP 패널티(턴당 감소)를 추가해 긴장감 극대화"),
        BL("최종 보스(Stage 9)에서 페이즈 2 전환으로 클라이막스 연출"),
        SP(),
        H2("1.2 진행 단계별 난이도 방향"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [1600, 1600, 1800, 2013, 2013],
          rows: [
            new TableRow({ children: [
              hCell("단계", 1600), hCell("스테이지", 1600), hCell("몬스터 HP 범위", 1800),
              hCell("저항 특징", 2013), hCell("파티 패널티", 2013),
            ]}),
            new TableRow({ children: [
              dCell("Tutorial", 1600, { fill: C.tut, bold: true, center: true, color: C.teal }),
              dCell("Stage 0", 1600, { center: true }), dCell("100", 1800, { center: true }),
              dCell("없음 (0%)", 2013, { center: true, color: C.green }),
              dCell("없음", 2013, { center: true, color: C.green }),
            ]}),
            new TableRow({ children: [
              dCell("Chapter 1", 1600, { fill: C.ch1, bold: true, center: true, color: C.ch1H }),
              dCell("Stage 1~3", 1600, { center: true, fill: C.light }), dCell("1,000 ~ 3,000", 1800, { center: true, fill: C.light }),
              dCell("없음 (0%)", 2013, { center: true, color: C.green, fill: C.light }),
              dCell("없음", 2013, { center: true, color: C.green, fill: C.light }),
            ]}),
            new TableRow({ children: [
              dCell("Chapter 2", 1600, { fill: C.ch2, bold: true, center: true, color: C.ch2H }),
              dCell("Stage 4~6", 1600, { center: true }), dCell("2,500 ~ 5,000", 1800, { center: true }),
              dCell("검/완드 최대 60%", 2013, { center: true, color: C.red }),
              dCell("없음 (Stage 6에 페이즈 2)", 2013, { center: true }),
            ]}),
            new TableRow({ children: [
              dCell("Chapter 3", 1600, { fill: C.ch3, bold: true, center: true, color: C.ch3H }),
              dCell("Stage 7~9", 1600, { center: true, fill: C.light }), dCell("6,000 ~ 10,000", 1800, { center: true, fill: C.light }),
              dCell("전 타입 최대 70%", 2013, { center: true, color: C.red, fill: C.light }),
              dCell("턴당 HP -10~-15", 2013, { center: true, color: C.red, fill: C.light }),
            ]}),
          ]
        }),
      ]
    },
    // ══════════════════════════════════════════════
    // [전체 스테이지 한눈에 보기]
    // ══════════════════════════════════════════════
    {
      properties: { page: pageProps },
      headers: { default: makeHeader() }, footers: { default: makeFooter() },
      children: [
        H1("2. 전체 스테이지 구성 한눈에 보기"),
        new Table({
          width: { size: 9026, type: WidthType.DXA },
          columnWidths: [600, 900, 1000, 1300, 600, 600, 600, 600, 700, 700, 826],
          rows: [
            new TableRow({ children: [
              hCell("인덱스", 600), hCell("챕터", 900), hCell("이름", 1000),
              hCell("몬스터 HP", 1300), hCell("최대턴", 600),
              hCell("검%", 600), hCell("활%", 600), hCell("완드%", 600),
              hCell("공격피해", 700), hCell("보상골드", 700), hCell("특이사항", 826),
            ]}),
            // Stage 0
            new TableRow({ children: [
              dCell("0", 600, { center: true, fill: C.tut }),
              dCell("Tutorial", 900, { fill: C.tut, bold: true }),
              dCell("Tutorial", 1000, { fill: C.tut }),
              dCell("100", 1300, { center: true, fill: C.tut }),
              dCell("40", 600, { center: true }), dCell("0", 600, { center: true }), dCell("0", 600, { center: true }), dCell("0", 600, { center: true }),
              dCell("20", 700, { center: true }), dCell("300G", 700, { center: true, color: C.gold }),
              dCell("시작 공격 O", 826, { center: true }),
            ]}),
            // Stage 1
            new TableRow({ children: [
              dCell("1", 600, { center: true, fill: C.ch1 }),
              dCell("Chapter 1", 900, { fill: C.ch1, bold: true }),
              dCell("Stage 1", 1000, { fill: C.ch1 }),
              dCell("1,000", 1300, { center: true, fill: C.ch1 }),
              dCell("40", 600, { center: true, fill: C.light }), dCell("0", 600, { center: true, fill: C.light }), dCell("0", 600, { center: true, fill: C.light }), dCell("0", 600, { center: true, fill: C.light }),
              dCell("10", 700, { center: true, fill: C.light }), dCell("100G", 700, { center: true, color: C.gold, fill: C.light }),
              dCell("-", 826, { center: true, fill: C.light }),
            ]}),
            // Stage 2
            new TableRow({ children: [
              dCell("2", 600, { center: true, fill: C.ch1 }),
              dCell("Chapter 1", 900, { fill: C.ch1, bold: true }),
              dCell("Stage 2", 1000, { fill: C.ch1 }),
              dCell("1,500", 1300, { center: true, fill: C.ch1 }),
              dCell("40", 600, { center: true }), dCell("0", 600, { center: true }), dCell("0", 600, { center: true }), dCell("0", 600, { center: true }),
              dCell("20", 700, { center: true }), dCell("100G", 700, { center: true, color: C.gold }),
              dCell("2명 공격", 826, { center: true }),
            ]}),
            // Stage 3
            new TableRow({ children: [
              dCell("3", 600, { center: true, fill: C.ch1 }),
              dCell("Chapter 1", 900, { fill: C.ch1, bold: true }),
              dCell("Stage 3", 1000, { fill: C.ch1 }),
              dCell("3,000", 1300, { center: true, fill: C.ch1, bold: true }),
              dCell("40", 600, { center: true, fill: C.light }), dCell("0", 600, { center: true, fill: C.light }), dCell("0", 600, { center: true, fill: C.light }), dCell("0", 600, { center: true, fill: C.light }),
              dCell("40", 700, { center: true, fill: C.light }), dCell("200G", 700, { center: true, color: C.gold, fill: C.light }),
              dCell("Ch.2 해금", 826, { center: true, fill: C.light }),
            ]}),
            // Stage 4
            new TableRow({ children: [
              dCell("4", 600, { center: true, fill: C.ch2 }),
              dCell("Chapter 2", 900, { fill: C.ch2, bold: true, color: C.ch2H }),
              dCell("Stage 4", 1000, { fill: C.ch2 }),
              dCell("2,500", 1300, { center: true, fill: C.ch2 }),
              dCell("40", 600, { center: true }), dCell("20", 600, { center: true, color: C.red }), dCell("20", 600, { center: true, color: C.red }), dCell("0", 600, { center: true }),
              dCell("20", 700, { center: true }), dCell("200G", 700, { center: true, color: C.gold }),
              dCell("3명 공격", 826, { center: true }),
            ]}),
            // Stage 5
            new TableRow({ children: [
              dCell("5", 600, { center: true, fill: C.ch2 }),
              dCell("Chapter 2", 900, { fill: C.ch2, bold: true, color: C.ch2H }),
              dCell("Stage 5", 1000, { fill: C.ch2 }),
              dCell("4,000", 1300, { center: true, fill: C.ch2 }),
              dCell("40", 600, { center: true, fill: C.light }), dCell("30", 600, { center: true, color: C.red, fill: C.light }), dCell("0", 600, { center: true, fill: C.light }), dCell("30", 600, { center: true, color: C.red, fill: C.light }),
              dCell("40", 700, { center: true, fill: C.light }), dCell("200G", 700, { center: true, color: C.gold, fill: C.light }),
              dCell("시작 공격 O", 826, { center: true, fill: C.light }),
            ]}),
            // Stage 6
            new TableRow({ children: [
              dCell("6", 600, { center: true, fill: C.ch2 }),
              dCell("Chapter 2", 900, { fill: C.ch2, bold: true, color: C.ch2H }),
              dCell("Stage 6", 1000, { fill: C.ch2 }),
              dCell("5,000", 1300, { center: true, fill: C.ch2, bold: true }),
              dCell("40", 600, { center: true }), dCell("60", 600, { center: true, color: C.red, bold: true }), dCell("0", 600, { center: true }), dCell("60", 600, { center: true, color: C.red, bold: true }),
              dCell("55 (3명)", 700, { center: true }), dCell("300G", 700, { center: true, color: C.gold }),
              dCell("페이즈 2 O / Ch.3 해금", 826, { center: true, color: C.red, bold: true }),
            ]}),
            // Stage 7
            new TableRow({ children: [
              dCell("7", 600, { center: true, fill: C.ch3 }),
              dCell("Chapter 3", 900, { fill: C.ch3, bold: true, color: C.ch3H }),
              dCell("Stage 7", 1000, { fill: C.ch3 }),
              dCell("6,000", 1300, { center: true, fill: C.ch3 }),
              dCell("40", 600, { center: true, fill: C.light }), dCell("40", 600, { center: true, color: C.red, fill: C.light }), dCell("40", 600, { center: true, color: C.red, fill: C.light }), dCell("40", 600, { center: true, color: C.red, fill: C.light }),
              dCell("60 (2명)", 700, { center: true, fill: C.light }), dCell("600G", 700, { center: true, color: C.gold, fill: C.light }),
              dCell("턴당 HP -10", 826, { center: true, color: C.red, fill: C.light }),
            ]}),
            // Stage 8
            new TableRow({ children: [
              dCell("8", 600, { center: true, fill: C.ch3 }),
              dCell("Chapter 3", 900, { fill: C.ch3, bold: true, color: C.ch3H }),
              dCell("Stage 8", 1000, { fill: C.ch3 }),
              dCell("7,500", 1300, { center: true, fill: C.ch3 }),
              dCell("40", 600, { center: true }), dCell("70", 600, { center: true, color: C.red, bold: true }), dCell("70", 600, { center: true, color: C.red, bold: true }), dCell("30", 600, { center: true, color: C.red }),
              dCell("70 (전체)", 700, { center: true }), dCell("800G", 700, { center: true, color: C.gold }),
              dCell("시작HP-30 / 턴당-10", 826, { center: true, color: C.red }),
            ]}),
            // Stage 9
            new TableRow({ children: [
              dCell("9", 600, { center: true, fill: C.ch3 }),
              dCell("Chapter 3", 900, { fill: C.ch3, bold: true, color: C.ch3H }),
              dCell("Stage 9", 1000, { fill: C.ch3 }),
              dCell("10,000", 1300, { center: true, fill: C.ch3, bold: true, color: C.red }),
              dCell("40", 600, { center: true, fill: C.light }), dCell("70", 600, { center: true, color: C.red, bold: true, fill: C.light }), dCell("70", 600, { center: true, color: C.red, bold: true, fill: C.light }), dCell("70", 600, { center: true, color: C.red, bold: true, fill: C.light }),
              dCell("75 (전체)", 700, { center: true, fill: C.light }), dCell("1,000G", 700, { center: true, color: C.gold, bold: true, fill: C.light }),
              dCell("페이즈 2 O / 최종 보스", 826, { center: true, color: C.red, bold: true, fill: C.light }),
            ]}),
          ]
        }),
        SP(),
        P("※ 저항값은 해당 타입 대미지를 감소시키는 비율(%)이며, 빨간색은 주의 필요 수치", { italic: true, color: C.gray }),
      ]
    },
    // ══════════════════════════════════════════════
    // [Tutorial]
    // ══════════════════════════════════════════════
    {
      properties: { page: pageProps },
      headers: { default: makeHeader() }, footers: { default: makeFooter() },
      children: [
        H1("3. Tutorial - Stage 0"),
        P("게임의 핵심 조작(타일 매칭, 스왑, 전투)을 플레이어에게 자연스럽게 익히도록 설계된 입문 스테이지.", { bold: true }),
        SP(),
        H2("3.1 스테이지 상세 데이터"),
        stageCard({
          hp: 100, maxTurns: 40, gold: 300,
          atkInterval: 4, atkDmg: 20, atkType: "단일 랜덤",
          rSword: 0, rBow: 0, rWand: 0,
          notes: "시작 즉시 몬스터 공격 발생 (AttackAndCountTurn). 보스 클리어 후 이미지 변경 연출."
        }, C.teal),
        SP(),
        H2("3.2 기획 의도"),
        BL("몬스터 HP를 100으로 낮게 설정 → 짧은 시간 안에 클리어 가능"),
        BL("저항값 0% → 모든 타일 타입이 효과적, 전략 강요 없이 자유롭게 플레이"),
        BL("파티 HP 패널티 없음 → 힐 타일 메커닉을 부담 없이 학습"),
        BL("시작 즉시 몬스터 공격 → 전투 개념 즉각 인지"),
        BL("보상 골드 300G (타 스테이지 대비 고보상) → 초반 장비/스킬 구매 유도"),
        SP(),
        H2("3.3 튜토리얼 스테이지 특수 처리"),
        BL("skipGravityAndSpawn 플래그 활성화 → 보드에서 타일 소환/낙하 없이 고정 배치 가능"),
        BL("TutorialManager가 SwapCompleted 이벤트 구독 → 스왑 완료 시 다음 안내 단계 진행"),
        BL("하이라이트/가이드 화살표로 조작 안내"),
      ]
    },
    // ══════════════════════════════════════════════
    // [Chapter 1]
    // ══════════════════════════════════════════════
    {
      properties: { page: pageProps },
      headers: { default: makeHeader() }, footers: { default: makeFooter() },
      children: [
        H1("4. Chapter 1 - Stage 1 ~ 3  (괴물의 숲)"),
        P("본격적인 전투 시스템에 입문하는 챕터. 저항 없음, 패널티 없음으로 순수 퍼즐 실력으로 승리 가능. Stage 3 클리어 시 Chapter 2 해금.", { bold: true }),
        SP(),
        H2("4.1 Stage 1 (인덱스: 1)"),
        P("가장 기초적인 전투 스테이지. 낮은 데미지, 낮은 HP로 전투 흐름 체험.", { italic: true, color: C.gray }),
        stageCard({
          hp: 1000, maxTurns: 40, gold: 100,
          atkInterval: 4, atkDmg: 10, atkType: "단일 랜덤",
          rSword: 0, rBow: 0, rWand: 0,
          notes: "기본 전투 흐름 체험용 스테이지. 저항 없음."
        }, C.ch1H),
        SP(),
        H2("4.2 Stage 2 (인덱스: 2)"),
        P("몬스터가 2명을 동시에 공격하기 시작. 힐 타일의 중요성을 처음 느끼는 시점.", { italic: true, color: C.gray }),
        stageCard({
          hp: 1500, maxTurns: 40, gold: 100,
          atkInterval: 4, atkDmg: 20, atkType: "랜덤 2명",
          rSword: 0, rBow: 0, rWand: 0,
          notes: "공격 대상이 2명으로 증가. Cross(힐) 타일 활용 중요성 인지."
        }, C.ch1H),
        SP(),
        H2("4.3 Stage 3 (인덱스: 3) — Chapter 1 보스"),
        P("Chapter 1의 최종 보스. HP가 3,000으로 크게 증가하고, 공격 피해도 40으로 상승. 클리어 컷신 연출.", { italic: true, color: C.gray }),
        stageCard({
          hp: 3000, maxTurns: 40, gold: 200,
          atkInterval: 4, atkDmg: 40, atkType: "단일 랜덤",
          rSword: 0, rBow: 0, rWand: 0,
          notes: "클리어 컷신 연출. 이 스테이지 클리어 시 Chapter 2 해금."
        }, C.ch1H),
        SP(),
        H2("4.4 Chapter 1 기획 의도"),
        BL("저항값 0% 유지 → 타일 전략 없이 매칭 수에 집중"),
        BL("Stage 1→2→3: 공격 데미지 10→20→40으로 단계 상승"),
        BL("HP 1,000→1,500→3,000: 전투 지속 시간 점진적 증가"),
        BL("Stage 3에서 클리어 컷신으로 챕터 완료 보상감 제공"),
      ]
    },
    // ══════════════════════════════════════════════
    // [Chapter 2]
    // ══════════════════════════════════════════════
    {
      properties: { page: pageProps },
      headers: { default: makeHeader() }, footers: { default: makeFooter() },
      children: [
        H1("5. Chapter 2 - Stage 4 ~ 6  (고대 유적)"),
        P("저항 시스템이 본격 도입되는 챕터. 스테이지별로 취약한 타일 타입이 다르므로 전략적 판단 필요. Stage 6 클리어 시 Chapter 3 해금 + 페이즈 2 첫 등장.", { bold: true }),
        SP(),
        H2("5.1 Stage 4 (인덱스: 4)"),
        P("검·활 저항 20% 도입. 완드 타일이 유효한 전략이 됨.", { italic: true, color: C.gray }),
        stageCard({
          hp: 2500, maxTurns: 40, gold: 200,
          atkInterval: 4, atkDmg: 20, atkType: "랜덤 3명",
          rSword: 20, rBow: 20, rWand: 0,
          rSword2: undefined, rBow2: undefined, rWand2: undefined,
          notes: "공격 대상 3명으로 확대. 검·활 저항 20% 도입으로 완드 타일 활용 유도."
        }, C.ch2H),
        SP(),
        H2("5.2 Stage 5 (인덱스: 5)"),
        P("검·완드 저항 30%. 활 타일 집중 전략이 핵심. 전투 시작 즉시 몬스터 공격 발생.", { italic: true, color: C.gray }),
        stageCard({
          hp: 4000, maxTurns: 40, gold: 200,
          atkInterval: 4, atkDmg: 40, atkType: "단일 랜덤",
          rSword: 30, rBow: 0, rWand: 30,
          rSword2: undefined, rBow2: undefined, rWand2: undefined,
          notes: "시작 즉시 공격 발동. 검·완드 저항으로 활 타일 집중 유도."
        }, C.ch2H),
        SP(),
        H2("5.3 Stage 6 (인덱스: 6) — Chapter 2 보스 & 첫 페이즈 2"),
        P("검·완드 저항 60%의 강력한 Phase 1. HP 1,000 이하에서 페이즈 2로 전환, 저항이 0%로 초기화되어 반전 연출.", { italic: true, color: C.gray }),
        stageCard({
          hp: 5000, maxTurns: 40, gold: 300,
          atkInterval: 4, atkDmg: 55, atkType: "랜덤 3명",
          rSword: 60, rBow: 0, rWand: 60,
          hp2: 1000, atkInterval2: 4, atkDmg2: 20, atkType2: "전체 공격",
          rSword2: 0, rBow2: 0, rWand2: 0,
          phase2Trigger: 1000,
          notes: "Phase 1: 검/완드 저항 60% → 활 필수. Phase 2 전환 시 저항 해제 & 전체 공격으로 반전 긴장감. Ch.3 해금."
        }, C.ch2H),
        SP(),
        H2("5.4 Chapter 2 기획 의도"),
        BL("저항 시스템으로 단일 타입 반복 매칭 전략 차단 → 다양한 타일 활용 유도"),
        BL("Stage 4: 완드 강조 / Stage 5: 활 강조 / Stage 6: 활 강조 → 후 반전"),
        BL("페이즈 2 첫 등장(Stage 6): 저항이 오히려 0%로 낮아지며 '막판 총공세' 느낌"),
        BL("3명 공격(Stage 4·6)으로 힐 타일 운영 압박 증가"),
      ]
    },
    // ══════════════════════════════════════════════
    // [Chapter 3]
    // ══════════════════════════════════════════════
    {
      properties: { page: pageProps },
      headers: { default: makeHeader() }, footers: { default: makeFooter() },
      children: [
        H1("6. Chapter 3 - Stage 7 ~ 9  (용의 요새)"),
        P("최고 난이도 챕터. 전 타입 고저항 + 파티 HP 턴당 감소 패널티 + 시작 HP 차감이 복합적으로 적용. Stage 9는 최종 보스로 페이즈 2 전환 연출.", { bold: true }),
        SP(),
        H2("6.1 Stage 7 (인덱스: 7)"),
        P("전 타입 저항 40% 도입. 턴당 HP -10으로 시간 싸움 시작.", { italic: true, color: C.gray }),
        stageCard({
          hp: 6000, maxTurns: 40, gold: 600,
          atkInterval: 4, atkDmg: 60, atkType: "랜덤 2명",
          rSword: 40, rBow: 40, rWand: 40,
          hpLossPerTurn: 10,
          notes: "전 타입 저항 40%. 턴당 HP -10으로 장기전 불가능. 효율적인 타일 선택 필수."
        }, C.ch3H),
        SP(),
        H2("6.2 Stage 8 (인덱스: 8)"),
        P("검·활 저항 70%로 대폭 상승. 완드가 유일한 효과적 공격 타입. 전체 공격으로 힐 운영 압박 극대화.", { italic: true, color: C.gray }),
        stageCard({
          hp: 7500, maxTurns: 40, gold: 800,
          atkInterval: 4, atkDmg: 70, atkType: "전체 공격",
          rSword: 70, rBow: 70, rWand: 30,
          startHpDeduct: 30, hpLossPerTurn: 10,
          notes: "시작 HP -30. 전체 공격 70 피해. 검/활 저항 70%로 완드 타일 집중 필수. 가장 높은 스킬 의존도."
        }, C.ch3H),
        SP(),
        H2("6.3 Stage 9 (인덱스: 9) — 최종 보스"),
        P("전 타입 저항 70% + 시작 HP -50 + 턴당 HP -15. HP 2,000 이하에서 페이즈 2 전환. 게임의 클라이막스.", { italic: true, color: C.gray }),
        stageCard({
          hp: 10000, maxTurns: 40, gold: 1000,
          atkInterval: 4, atkDmg: 75, atkType: "전체 공격",
          rSword: 70, rBow: 70, rWand: 70,
          hp2: 2000, atkInterval2: 4, atkDmg2: 70, atkType2: "전체 공격",
          rSword2: 30, rBow2: 30, rWand2: 30,
          startHpDeduct: 50, hpLossPerTurn: 15,
          phase2Trigger: 2000,
          notes: "최종 보스. Phase 1: 모든 저항 70%. Phase 2: 저항 30%로 완화, 전체 70피해. 페이즈 2 컷신 + 클리어 컷신."
        }, C.ch3H),
        SP(),
        H2("6.4 Chapter 3 기획 의도"),
        BL("파티 HP 턴당 감소: 매 턴마다 위기감 → 힐과 공격 타일 균형 운영 강요"),
        BL("시작 HP 차감(Stage 8: -30, Stage 9: -50): 전투 시작부터 긴장감"),
        BL("Stage 9 페이즈 2: Phase 1 저항 70% → Phase 2 저항 30%로 완화, '마지막 발악' 연출"),
        BL("보상 600~1,000G로 대폭 상승: 최종 장비/스킬 구매 유도"),
        BL("시작 즉시 공격(Stage 9): 방심 금지 메시지"),
      ]
    },
    // ══════════════════════════════════════════════
    // [캐릭터 스탯 & 업그레이드]
    // ══════════════════════════════════════════════
    {
      properties: { page: pageProps },
      headers: { default: makeHeader() }, footers: { default: makeFooter() },
      children: [
        H1("7. 캐릭터 기본 스탯 & 업그레이드 시스템"),
        H2("7.1 4인 파티 구성"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [800, 1600, 2000, 2313, 2313],
          rows: [
            new TableRow({ children: [hCell("인덱스",800), hCell("캐릭터 역할",1600), hCell("주력 타일",2000), hCell("기본 스킬 타입",2313), hCell("특징",2313)] }),
            new TableRow({ children: [dCell("0",800,{center:true}), dCell("검사 (Warrior)",1600,{fill:C.light}), dCell("Sword 타일",2000,{fill:C.light}), dCell("Sword 계열",2313,{fill:C.light}), dCell("단일 집중 타격",2313,{fill:C.light})] }),
            new TableRow({ children: [dCell("1",800,{center:true}), dCell("마법사 (Mage)",1600), dCell("Wand 타일",2000), dCell("Wand 계열",2313), dCell("마법 광역 피해",2313)] }),
            new TableRow({ children: [dCell("2",800,{center:true}), dCell("궁수 (Archer)",1600,{fill:C.light}), dCell("Bow 타일",2000,{fill:C.light}), dCell("Bow 계열",2313,{fill:C.light}), dCell("원거리 저격",2313,{fill:C.light})] }),
            new TableRow({ children: [dCell("3",800,{center:true}), dCell("성직자 (Clergy)",1600), dCell("Cross 타일",2000), dCell("Heal 계열",2313), dCell("파티 전원 회복",2313)] }),
          ]
        }),
        SP(),
        H2("7.2 기본 스탯 (업그레이드 0레벨 기준)"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [1800, 1445, 1445, 1445, 1445, 1446],
          rows: [
            new TableRow({ children: [hCell("스탯 항목",1800), hCell("기본값",1445), hCell("레벨당 보너스",1445), hCell("10레벨 최종",1445), hCell("20레벨 최종",1445), hCell("비고",1446)] }),
            new TableRow({ children: [dCell("검(Sword) 대미지",1800,{fill:C.light,bold:true}), dCell("20",1445,{center:true,fill:C.light}), dCell("+10",1445,{center:true,fill:C.light,color:C.green}), dCell("120",1445,{center:true,fill:C.light}), dCell("220",1445,{center:true,fill:C.light}), dCell("타일 1개 기준",1446,{fill:C.light})] }),
            new TableRow({ children: [dCell("활(Bow) 대미지",1800,{bold:true}), dCell("20",1445,{center:true}), dCell("+10",1445,{center:true,color:C.green}), dCell("120",1445,{center:true}), dCell("220",1445,{center:true}), dCell("타일 1개 기준",1446)] }),
            new TableRow({ children: [dCell("완드(Wand) 대미지",1800,{fill:C.light,bold:true}), dCell("20",1445,{center:true,fill:C.light}), dCell("+10",1445,{center:true,fill:C.light,color:C.green}), dCell("120",1445,{center:true,fill:C.light}), dCell("220",1445,{center:true,fill:C.light}), dCell("타일 1개 기준",1446,{fill:C.light})] }),
            new TableRow({ children: [dCell("회복(Heal) 량",1800,{bold:true}), dCell("5",1445,{center:true}), dCell("+3",1445,{center:true,color:C.green}), dCell("35",1445,{center:true}), dCell("65",1445,{center:true}), dCell("Cross 타일 1개 기준",1446)] }),
            new TableRow({ children: [dCell("강화(Enhanced) 보너스",1800,{fill:C.light,bold:true}), dCell("40",1445,{center:true,fill:C.light}), dCell("+15",1445,{center:true,fill:C.light,color:C.green}), dCell("190",1445,{center:true,fill:C.light}), dCell("340",1445,{center:true,fill:C.light}), dCell("강화 타일 1개당 추가",1446,{fill:C.light})] }),
          ]
        }),
        SP(),
        P("최종 스탯 공식:  최종값 = 기본값 + (업그레이드 레벨 × 레벨당 보너스)", { bold: true, color: C.navy }),
        SP(),
        H2("7.3 업그레이드 슬롯 (5종)"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [600, 1800, 1500, 2563, 2563],
          rows: [
            new TableRow({ children: [hCell("슬롯",600), hCell("이름",1800), hCell("대상 스탯",1500), hCell("효과",2563), hCell("추천 우선순위",2563)] }),
            new TableRow({ children: [dCell("0",600,{center:true}), dCell("검 강화",1800,{fill:C.light}), dCell("swordDamage",1500,{fill:C.light}), dCell("검 타일 대미지 +10/레벨",2563,{fill:C.light}), dCell("Ch.2 검 저항 낮은 스테이지",2563,{fill:C.light})] }),
            new TableRow({ children: [dCell("1",600,{center:true}), dCell("활 강화",1800), dCell("bowDamage",1500), dCell("활 타일 대미지 +10/레벨",2563), dCell("Ch.2 스테이지 5~6 필수",2563)] }),
            new TableRow({ children: [dCell("2",600,{center:true}), dCell("완드 강화",1800,{fill:C.light}), dCell("wandDamage",1500,{fill:C.light}), dCell("완드 타일 대미지 +10/레벨",2563,{fill:C.light}), dCell("Ch.2 스테이지 4, Ch.3 스테이지 8",2563,{fill:C.light})] }),
            new TableRow({ children: [dCell("3",600,{center:true}), dCell("회복 강화",1800), dCell("healAmount",1500), dCell("Cross 타일 회복량 +3/레벨",2563), dCell("Ch.3 턴당 HP 감소 대응",2563)] }),
            new TableRow({ children: [dCell("4",600,{center:true}), dCell("강화 타일 강화",1800,{fill:C.light}), dCell("enhancedBonus",1500,{fill:C.light}), dCell("강화 타일 보너스 +15/레벨",2563,{fill:C.light}), dCell("4매칭+ 빈번 시 우선",2563,{fill:C.light})] }),
          ]
        }),
        SP(),
        H2("7.4 대미지 계산 예시 (Stage 6 vs 활 타일 매칭 3개)"),
        P("조건: 활 대미지 업그레이드 5레벨, 장비 공격 배율 1.25, Stage 6 활 저항 0%"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [3000, 3013, 3013],
          rows: [
            new TableRow({ children: [hCell("계산 단계",3000), hCell("수식",3013), hCell("결과",3013)] }),
            new TableRow({ children: [dCell("활 최종 스탯",3000,{fill:C.light}), dCell("20 + (5 × 10)",3013,{fill:C.light}), dCell("70",3013,{fill:C.light,bold:true})] }),
            new TableRow({ children: [dCell("기본 대미지 (3매칭)",3000), dCell("70 × 3",3013), dCell("210",3013,{bold:true})] }),
            new TableRow({ children: [dCell("장비 배율 적용",3000,{fill:C.light}), dCell("210 × 1.25",3013,{fill:C.light}), dCell("262.5",3013,{fill:C.light,bold:true})] }),
            new TableRow({ children: [dCell("저항 적용 (0%)",3000), dCell("262.5 × (1 - 0)",3013), dCell("262 (최종)",3013,{bold:true,color:C.red})] }),
          ]
        }),
      ]
    },
    // ══════════════════════════════════════════════
    // [스킬]
    // ══════════════════════════════════════════════
    {
      properties: { page: pageProps },
      headers: { default: makeHeader() }, footers: { default: makeFooter() },
      children: [
        H1("8. 스킬 데이터"),
        P("총 12개 스킬, 캐릭터 1명당 3개. 모든 스킬의 쿨다운은 3턴이며, 챕터 진행에 따라 순차 해금."),
        SP(),
        H2("8.1 캐릭터 0 — 검사 스킬 (Sword 계열)"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [400, 1500, 1000, 1000, 600, 600, 1000, 2926],
          rows: [
            new TableRow({ children: [hCell("ID",400), hCell("스킬명",1500), hCell("효과 타입",1000), hCell("배율",1000), hCell("쿨다운",600), hCell("해금",600), hCell("해금 조건",1000), hCell("설명",2926)] }),
            new TableRow({ children: [dCell("0",400,{center:true}), dCell("강타",1500,{fill:C.light}), dCell("Sword",1000,{center:true,fill:C.light}), dCell("×5.0",1000,{center:true,fill:C.light,bold:true}), dCell("3턴",600,{center:true,fill:C.light}), dCell("항상",600,{center:true,fill:C.light,color:C.green}), dCell("-",1000,{center:true,fill:C.light}), dCell("검 기본 공격력의 500% 대미지",2926,{fill:C.light})] }),
            new TableRow({ children: [dCell("1",400,{center:true}), dCell("빛의 심판",1500), dCell("Sword",1000,{center:true}), dCell("×10.0",1000,{center:true,bold:true,color:C.blue}), dCell("3턴",600,{center:true}), dCell("Chapter 2",600,{center:true,color:C.ch2H}), dCell("Ch.2 해금",1000,{center:true}), dCell("검 기본 공격력의 1000% 대미지",2926)] }),
            new TableRow({ children: [dCell("2",400,{center:true}), dCell("엑스칼리버",1500,{fill:C.light}), dCell("Sword",1000,{center:true,fill:C.light}), dCell("×15.0",1000,{center:true,bold:true,color:C.red,fill:C.light}), dCell("3턴",600,{center:true,fill:C.light}), dCell("Chapter 3",600,{center:true,color:C.ch3H,fill:C.light}), dCell("Ch.3 해금",1000,{center:true,fill:C.light}), dCell("검 기본 공격력의 1500% 대미지",2926,{fill:C.light})] }),
          ]
        }),
        SP(),
        H2("8.2 캐릭터 1 — 마법사 스킬 (Wand 계열)"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [400, 1500, 1000, 1000, 600, 600, 1000, 2926],
          rows: [
            new TableRow({ children: [hCell("ID",400), hCell("스킬명",1500), hCell("효과 타입",1000), hCell("배율",1000), hCell("쿨다운",600), hCell("해금",600), hCell("해금 조건",1000), hCell("설명",2926)] }),
            new TableRow({ children: [dCell("3",400,{center:true}), dCell("파이어 볼",1500,{fill:C.ch2}), dCell("Wand",1000,{center:true,fill:C.ch2}), dCell("×5.0",1000,{center:true,fill:C.ch2,bold:true}), dCell("3턴",600,{center:true,fill:C.ch2}), dCell("항상",600,{center:true,fill:C.ch2,color:C.green}), dCell("-",1000,{center:true,fill:C.ch2}), dCell("완드 기본 공격력의 500%",2926,{fill:C.ch2})] }),
            new TableRow({ children: [dCell("4",400,{center:true}), dCell("헬 파이어",1500), dCell("Wand",1000,{center:true}), dCell("×5.0",1000,{center:true,bold:true}), dCell("3턴",600,{center:true}), dCell("Chapter 2",600,{center:true,color:C.ch2H}), dCell("Ch.2 해금",1000,{center:true}), dCell("완드 기본 공격력의 500% (강화판)",2926)] }),
            new TableRow({ children: [dCell("5",400,{center:true}), dCell("익스플로전",1500,{fill:C.ch2}), dCell("Wand",1000,{center:true,fill:C.ch2}), dCell("×5.0",1000,{center:true,bold:true,fill:C.ch2}), dCell("3턴",600,{center:true,fill:C.ch2}), dCell("Chapter 3",600,{center:true,color:C.ch3H,fill:C.ch2}), dCell("Ch.3 해금",1000,{center:true,fill:C.ch2}), dCell("완드 기본 공격력의 500% (최강판)",2926,{fill:C.ch2})] }),
          ]
        }),
        SP(),
        H2("8.3 캐릭터 2 — 궁수 스킬 (Bow 계열)"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [400, 1500, 1000, 1000, 600, 600, 1000, 2926],
          rows: [
            new TableRow({ children: [hCell("ID",400), hCell("스킬명",1500), hCell("효과 타입",1000), hCell("배율",1000), hCell("쿨다운",600), hCell("해금",600), hCell("해금 조건",1000), hCell("설명",2926)] }),
            new TableRow({ children: [dCell("6",400,{center:true}), dCell("바람의 화살",1500,{fill:C.ch1}), dCell("Bow",1000,{center:true,fill:C.ch1}), dCell("×5.0",1000,{center:true,fill:C.ch1,bold:true}), dCell("3턴",600,{center:true,fill:C.ch1}), dCell("항상",600,{center:true,fill:C.ch1,color:C.green}), dCell("-",1000,{center:true,fill:C.ch1}), dCell("활 기본 공격력의 500%",2926,{fill:C.ch1})] }),
            new TableRow({ children: [dCell("7",400,{center:true}), dCell("정령의 활시위",1500), dCell("Bow",1000,{center:true}), dCell("×10.0",1000,{center:true,bold:true,color:C.blue}), dCell("3턴",600,{center:true}), dCell("Chapter 2",600,{center:true,color:C.ch2H}), dCell("Ch.2 해금",1000,{center:true}), dCell("활 기본 공격력의 1000%",2926)] }),
            new TableRow({ children: [dCell("8",400,{center:true}), dCell("토네이도",1500,{fill:C.ch1}), dCell("Bow",1000,{center:true,fill:C.ch1}), dCell("×15.0",1000,{center:true,bold:true,color:C.red,fill:C.ch1}), dCell("3턴",600,{center:true,fill:C.ch1}), dCell("Chapter 3",600,{center:true,color:C.ch3H,fill:C.ch1}), dCell("Ch.3 해금",1000,{center:true,fill:C.ch1}), dCell("활 기본 공격력의 1500%",2926,{fill:C.ch1})] }),
          ]
        }),
        SP(),
        H2("8.4 캐릭터 3 — 성직자 스킬 (Heal 계열)"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [400, 1500, 1000, 1000, 600, 600, 1000, 2926],
          rows: [
            new TableRow({ children: [hCell("ID",400), hCell("스킬명",1500), hCell("효과 타입",1000), hCell("배율",1000), hCell("쿨다운",600), hCell("해금",600), hCell("해금 조건",1000), hCell("설명",2926)] }),
            new TableRow({ children: [dCell("9",400,{center:true}), dCell("회복",1500,{fill:C.tut}), dCell("Heal",1000,{center:true,fill:C.tut}), dCell("×1.5",1000,{center:true,fill:C.tut,bold:true}), dCell("3턴",600,{center:true,fill:C.tut}), dCell("항상",600,{center:true,fill:C.tut,color:C.green}), dCell("-",1000,{center:true,fill:C.tut}), dCell("기본 회복량의 150% 회복",2926,{fill:C.tut})] }),
            new TableRow({ children: [dCell("10",400,{center:true}), dCell("정화",1500), dCell("Heal",1000,{center:true}), dCell("×2.0",1000,{center:true,bold:true,color:C.blue}), dCell("3턴",600,{center:true}), dCell("Chapter 2",600,{center:true,color:C.ch2H}), dCell("Ch.2 해금",1000,{center:true}), dCell("기본 회복량의 200% 회복",2926)] }),
            new TableRow({ children: [dCell("11",400,{center:true}), dCell("축복",1500,{fill:C.tut}), dCell("Heal",1000,{center:true,fill:C.tut}), dCell("×3.0",1000,{center:true,bold:true,color:C.green,fill:C.tut}), dCell("3턴",600,{center:true,fill:C.tut}), dCell("Chapter 3",600,{center:true,color:C.ch3H,fill:C.tut}), dCell("Ch.3 해금",1000,{center:true,fill:C.tut}), dCell("기본 회복량의 300% 회복 (파티 전체)",2926,{fill:C.tut})] }),
          ]
        }),
        SP(),
        H2("8.5 스킬 시스템 특징"),
        BL("모든 스킬 쿨다운 3턴 동일 → 매 3턴마다 스킬 1회 사용 가능"),
        BL("Chapter 진행으로만 해금 → 스테이지 클리어 = 더 강한 스킬 획득"),
        BL("검/활/완드 스킬은 타일 타입 대미지 × 배율 → 해당 타입 업그레이드와 시너지"),
        BL("힐 스킬 배율이 상대적으로 낮아 보이나, Ch.3 턴당 HP 감소 대응에 핵심 역할"),
      ]
    },
    // ══════════════════════════════════════════════
    // [장비]
    // ══════════════════════════════════════════════
    {
      properties: { page: pageProps },
      headers: { default: makeHeader() }, footers: { default: makeFooter() },
      children: [
        H1("9. 장비 데이터"),
        P("총 4종 장비, 2개 슬롯(Slot 0: HP/방어 계열, Slot 1: 공격/회복 계열). 각 슬롯에 1개씩 장착 가능."),
        SP(),
        H2("9.1 Slot 0 — HP·방어 계열"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [1800, 1200, 1200, 1200, 1200, 2426],
          rows: [
            new TableRow({ children: [hCell("장비명",1800), hCell("구매 비용",1200), hCell("HP 증가",1200), hCell("공격 증가",1200), hCell("회복 증가",1200), hCell("해금 / 효과 설명",2426)] }),
            new TableRow({ children: [
              dCell("기사단장의 갑옷",1800,{fill:C.light,bold:true}),
              dCell("500G",1200,{center:true,fill:C.light,color:C.gold}),
              dCell("+25%",1200,{center:true,fill:C.light,color:C.green,bold:true}),
              dCell("0%",1200,{center:true,fill:C.light}), dCell("0%",1200,{center:true,fill:C.light}),
              dCell("Ch.1 클리어 시 해금. 파티 최대 HP 25% 증가.",2426,{fill:C.light}),
            ]}),
            new TableRow({ children: [
              dCell("모험가의 망토",1800,{bold:true}),
              dCell("1,000G",1200,{center:true,color:C.gold}),
              dCell("+50%",1200,{center:true,color:C.green,bold:true}),
              dCell("0%",1200,{center:true}), dCell("0%",1200,{center:true}),
              dCell("Ch.2 클리어 시 해금. 파티 최대 HP 50% 증가.",2426),
            ]}),
          ]
        }),
        SP(),
        H2("9.2 Slot 1 — 공격·회복 계열"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [1800, 1200, 1200, 1200, 1200, 2426],
          rows: [
            new TableRow({ children: [hCell("장비명",1800), hCell("구매 비용",1200), hCell("HP 증가",1200), hCell("공격 증가",1200), hCell("회복 증가",1200), hCell("해금 / 효과 설명",2426)] }),
            new TableRow({ children: [
              dCell("마법사의 반지",1800,{fill:C.ch2,bold:true}),
              dCell("500G",1200,{center:true,fill:C.ch2,color:C.gold}),
              dCell("0%",1200,{center:true,fill:C.ch2}),
              dCell("+25%",1200,{center:true,fill:C.ch2,color:C.red,bold:true}),
              dCell("+10%",1200,{center:true,fill:C.ch2,color:C.green}),
              dCell("Ch.1 클리어 시 해금. 공격력 25%, 회복 10% 증가.",2426,{fill:C.ch2}),
            ]}),
            new TableRow({ children: [
              dCell("성녀의 목걸이",1800,{bold:true}),
              dCell("1,000G",1200,{center:true,color:C.gold}),
              dCell("0%",1200,{center:true}),
              dCell("+50%",1200,{center:true,color:C.red,bold:true}),
              dCell("+20%",1200,{center:true,color:C.green}),
              dCell("Ch.2 클리어 시 해금. 공격력 50%, 회복 20% 증가.",2426),
            ]}),
          ]
        }),
        SP(),
        H2("9.3 장비 조합 전략"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [2200, 2200, 2313, 2313],
          rows: [
            new TableRow({ children: [hCell("Slot 0",2200), hCell("Slot 1",2200), hCell("효과 합산",2313), hCell("추천 상황",2313)] }),
            new TableRow({ children: [
              dCell("기사단장의 갑옷",2200,{fill:C.light}), dCell("마법사의 반지",2200,{fill:C.light}),
              dCell("HP+25%, 공격+25%, 회복+10%",2313,{fill:C.light}), dCell("Ch.1~2 초반 (균형형)",2313,{fill:C.light}),
            ]}),
            new TableRow({ children: [
              dCell("기사단장의 갑옷",2200), dCell("성녀의 목걸이",2200),
              dCell("HP+25%, 공격+50%, 회복+20%",2313), dCell("Ch.2~3 (공격 집중형)",2313),
            ]}),
            new TableRow({ children: [
              dCell("모험가의 망토",2200,{fill:C.light}), dCell("마법사의 반지",2200,{fill:C.light}),
              dCell("HP+50%, 공격+25%, 회복+10%",2313,{fill:C.light}), dCell("Ch.3 Stage 7 (생존 우선형)",2313,{fill:C.light}),
            ]}),
            new TableRow({ children: [
              dCell("모험가의 망토",2200), dCell("성녀의 목걸이",2200),
              dCell("HP+50%, 공격+50%, 회복+20%",2313), dCell("Stage 9 최종 보스 (풀세트)",2313,{bold:true,color:C.red}),
            ]}),
          ]
        }),
        SP(),
        H2("9.4 장비 투자 우선순위"),
        NL("기사단장의 갑옷 (500G): Ch.1 클리어 직후 구매 → HP 25% 증가로 생존력 확보"),
        NL("마법사의 반지 (500G): 동시 구매 → 공격력 25% 증가로 DPS 향상"),
        NL("모험가의 망토 (1,000G): Ch.2 진입 전 필수 → Ch.3 턴당 HP 감소 대응"),
        NL("성녀의 목걸이 (1,000G): Ch.3 진입 전 → 공격력 50%로 Stage 9 저항 돌파"),
      ]
    },
    // ══════════════════════════════════════════════
    // [밸런스 & 잠금해제]
    // ══════════════════════════════════════════════
    {
      properties: { page: pageProps },
      headers: { default: makeHeader() }, footers: { default: makeFooter() },
      children: [
        H1("10. 밸런스 설계 가이드"),
        H2("10.1 파티 HP vs 몬스터 공격력 밸런스"),
        P("기본 파티 HP를 기준으로 각 스테이지 공격량과 수용 가능 턴 수를 설계:"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [700, 1000, 1300, 1300, 1300, 1300, 2126],
          rows: [
            new TableRow({ children: [hCell("Stage",700), hCell("공격 대상",1000), hCell("공격 피해",1300), hCell("턴당 HP 감소",1300), hCell("시작 HP 차감",1300), hCell("4턴당 피해",1300), hCell("설계 의도",2126)] }),
            new TableRow({ children: [dCell("0",700,{center:true,fill:C.tut}), dCell("1명",1000,{center:true}), dCell("20",1300,{center:true}), dCell("0",1300,{center:true,color:C.green}), dCell("0",1300,{center:true,color:C.green}), dCell("20",1300,{center:true}), dCell("입문 학습",2126)] }),
            new TableRow({ children: [dCell("3",700,{center:true,fill:C.ch1}), dCell("1명",1000,{center:true,fill:C.light}), dCell("40",1300,{center:true,fill:C.light}), dCell("0",1300,{center:true,fill:C.light,color:C.green}), dCell("0",1300,{center:true,fill:C.light,color:C.green}), dCell("40",1300,{center:true,fill:C.light}), dCell("Ch.1 최고 난이도",2126,{fill:C.light})] }),
            new TableRow({ children: [dCell("6",700,{center:true,fill:C.ch2}), dCell("3명",1000,{center:true}), dCell("55",1300,{center:true}), dCell("0",1300,{center:true,color:C.green}), dCell("0",1300,{center:true,color:C.green}), dCell("165(3명)",1300,{center:true,color:C.red}), dCell("Ch.2 보스 압박",2126)] }),
            new TableRow({ children: [dCell("7",700,{center:true,fill:C.ch3}), dCell("2명",1000,{center:true,fill:C.light}), dCell("60",1300,{center:true,fill:C.light}), dCell("10/턴",1300,{center:true,fill:C.light,color:C.red,bold:true}), dCell("0",1300,{center:true,fill:C.light,color:C.green}), dCell("120+α",1300,{center:true,fill:C.light,color:C.red}), dCell("턴당 감소 도입",2126,{fill:C.light})] }),
            new TableRow({ children: [dCell("9",700,{center:true,fill:C.ch3}), dCell("전체",1000,{center:true}), dCell("75",1300,{center:true}), dCell("15/턴",1300,{center:true,color:C.red,bold:true}), dCell("50",1300,{center:true,color:C.red,bold:true}), dCell("300+β",1300,{center:true,color:C.red,bold:true}), dCell("최대 난이도",2126)] }),
          ]
        }),
        SP(),
        H2("10.2 저항값 밸런스 설계 원칙"),
        BL("한 타입 저항이 높을 때 반드시 다른 타입의 저항은 낮게 설정"),
        BL("Stage 6: 검/완드 60% 저항 → 활이 유일한 유효 타입 (집중 전략 강요)"),
        BL("Stage 8: 검/활 70% 저항 → 완드가 유일한 유효 타입"),
        BL("Stage 9: 전 타입 70% → 스킬과 강화 타일 보너스로만 돌파 (최고 압박)"),
        BL("페이즈 2는 항상 Phase 1보다 저항을 낮게 설정 → 클라이막스 반전 설계"),
        SP(),
        H2("10.3 골드 보상 설계"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [2000, 1500, 1500, 4026],
          rows: [
            new TableRow({ children: [hCell("구간",2000), hCell("스테이지당 보상",1500), hCell("최대 수령(2회)",1500), hCell("누적 목표",4026)] }),
            new TableRow({ children: [dCell("Tutorial",2000,{fill:C.tut}), dCell("300G",1500,{center:true,fill:C.tut}), dCell("600G",1500,{center:true,fill:C.tut}), dCell("Slot 0 기본 장비(500G) 구매 가능",4026,{fill:C.tut})] }),
            new TableRow({ children: [dCell("Chapter 1 (3스테이지)",2000,{fill:C.ch1}), dCell("100~200G",1500,{center:true,fill:C.ch1}), dCell("최대 1,200G",1500,{center:true,fill:C.ch1}), dCell("양쪽 500G 장비 2개 구매 가능",4026,{fill:C.ch1})] }),
            new TableRow({ children: [dCell("Chapter 2 (3스테이지)",2000,{fill:C.ch2}), dCell("200~300G",1500,{center:true,fill:C.ch2}), dCell("최대 2,400G",1500,{center:true,fill:C.ch2}), dCell("1,000G 장비 1~2개 업그레이드",4026,{fill:C.ch2})] }),
            new TableRow({ children: [dCell("Chapter 3 (3스테이지)",2000,{fill:C.ch3}), dCell("600~1,000G",1500,{center:true,fill:C.ch3}), dCell("최대 4,800G",1500,{center:true,fill:C.ch3}), dCell("최고 장비 풀세트 완성 가능",4026,{fill:C.ch3})] }),
          ]
        }),
        SP(),
        H2("10.4 스킬 해금과 스테이지 진행 매핑"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [2000, 1513, 1513, 4000],
          rows: [
            new TableRow({ children: [hCell("해금 시점",2000), hCell("해금 스킬 수",1513), hCell("누적 스킬",1513), hCell("첫 사용 적합 스테이지",4000)] }),
            new TableRow({ children: [dCell("게임 시작 (항상)",2000,{fill:C.tut}), dCell("4개",1513,{center:true,fill:C.tut}), dCell("4개",1513,{center:true,fill:C.tut}), dCell("Tutorial ~ Stage 3",4000,{fill:C.tut})] }),
            new TableRow({ children: [dCell("Chapter 2 해금 (Stage 3 클리어)",2000,{fill:C.ch1}), dCell("4개",1513,{center:true,fill:C.ch1}), dCell("8개",1513,{center:true,fill:C.ch1}), dCell("Stage 4~6 (저항 대응 강화 스킬)",4000,{fill:C.ch1})] }),
            new TableRow({ children: [dCell("Chapter 3 해금 (Stage 6 클리어)",2000,{fill:C.ch2}), dCell("4개",1513,{center:true,fill:C.ch2}), dCell("12개",1513,{center:true,fill:C.ch2}), dCell("Stage 7~9 (최강 스킬 필수)",4000,{fill:C.ch2})] }),
          ]
        }),
        SP(),
        H1("11. 챕터 잠금해제 & 보상 흐름"),
        H2("11.1 스테이지 잠금해제 조건"),
        new Table({
          width: { size: 9026, type: WidthType.DXA }, columnWidths: [1800, 2500, 4726],
          rows: [
            new TableRow({ children: [hCell("챕터",1800), hCell("해금 조건",2500), hCell("저장 데이터",4726)] }),
            new TableRow({ children: [dCell("Tutorial",1800,{fill:C.tut}), dCell("항상 해금",2500,{fill:C.tut,color:C.green}), dCell("lastClearedStageIndex >= -1",4726,{fill:C.tut})] }),
            new TableRow({ children: [dCell("Chapter 1",1800,{fill:C.ch1}), dCell("항상 해금",2500,{fill:C.ch1,color:C.green}), dCell("lastClearedStageIndex >= 0",4726,{fill:C.ch1})] }),
            new TableRow({ children: [dCell("Chapter 2",1800,{fill:C.ch2}), dCell("Stage 3 (Ch.1 마지막) 클리어",2500,{fill:C.ch2}), dCell("lastClearedStageIndex >= 3",4726,{fill:C.ch2})] }),
            new TableRow({ children: [dCell("Chapter 3",1800,{fill:C.ch3}), dCell("Stage 6 (Ch.2 마지막) 클리어",2500,{fill:C.ch3}), dCell("lastClearedStageIndex >= 6",4726,{fill:C.ch3})] }),
          ]
        }),
        SP(),
        H2("11.2 스테이지 보상 수령 규칙"),
        BL("스테이지당 최대 2회 보상 수령 가능 (MaxRewardClaimPerStage = 2)"),
        BL("재도전으로 추가 골드 수급 가능 → 장비 구매 유도"),
        BL("stageRewardClaimCounts[10] 배열로 수령 횟수 추적 및 저장"),
        SP(),
        H2("11.3 전체 진행 흐름 요약"),
        NL("게임 시작 → (프롤로그 컷신) → 맵 화면"),
        NL("Tutorial(Stage 0) 클리어 → Chapter 1 해금"),
        NL("Stage 1 → 2 → 3 순차 클리어 (컷신 연출) → Chapter 2 해금 + Ch.2 스킬 해금"),
        NL("Stage 4 → 5 → 6 클리어 (페이즈 2 첫 등장) → Chapter 3 해금 + Ch.3 스킬 해금"),
        NL("Stage 7 → 8 → 9 클리어 (최종 보스 페이즈 2 + 엔딩 컷신)"),
        SP(),
        SP(),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          border: { top: { style: BorderStyle.SINGLE, size: 4, color: C.blue, space: 8 } },
          spacing: { before: 400 },
          children: [new TextRun({ text: "--- End of Document  |  3 Match Puzzle RPG Level Design v1.0 ---",
            font: "맑은 고딕", size: 19, color: "AAAAAA", italics: true })]
        }),
      ]
    },
  ]
});

const OUT = "C:/UnityProject/3_Match_Puzzle_RPG/3_Match_Puzzle_RPG_Level_Design.docx";
Packer.toBuffer(doc).then(buf => {
  fs.writeFileSync(OUT, buf);
  console.log("Level Design Doc created:", OUT);
});
