const fs = require("fs");
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
  Header, Footer, AlignmentType, LevelFormat,
  HeadingLevel, BorderStyle, WidthType, ShadingType,
  PageNumber, PageBreak
} = require("docx");

// ── helpers ──────────────────────────────────────────────────────
const border = { style: BorderStyle.SINGLE, size: 1, color: "CCCCCC" };
const borders = { top: border, bottom: border, left: border, right: border };
const thickBorder = { style: BorderStyle.SINGLE, size: 2, color: "2E5090" };
const thickBorders = { top: thickBorder, bottom: thickBorder, left: thickBorder, right: thickBorder };

const cellMargins = { top: 60, bottom: 60, left: 100, right: 100 };
const headerShading = { fill: "2E5090", type: ShadingType.CLEAR };
const subHeaderShading = { fill: "D6E4F0", type: ShadingType.CLEAR };
const altRowShading = { fill: "F2F7FB", type: ShadingType.CLEAR };

function headerCell(text, width) {
  return new TableCell({
    borders: thickBorders, width: { size: width, type: WidthType.DXA },
    shading: headerShading, margins: cellMargins,
    verticalAlign: "center",
    children: [new Paragraph({ alignment: AlignmentType.CENTER, children: [new TextRun({ text, bold: true, color: "FFFFFF", font: "Malgun Gothic", size: 20 })] })]
  });
}

function cell(text, width, opts = {}) {
  const shading = opts.shading || undefined;
  return new TableCell({
    borders, width: { size: width, type: WidthType.DXA },
    shading, margins: cellMargins,
    verticalAlign: "center",
    children: [new Paragraph({
      alignment: opts.center ? AlignmentType.CENTER : AlignmentType.LEFT,
      children: [new TextRun({ text, font: "Malgun Gothic", size: 20, bold: opts.bold || false })]
    })]
  });
}

function subHeaderCell(text, width) {
  return new TableCell({
    borders, width: { size: width, type: WidthType.DXA },
    shading: subHeaderShading, margins: cellMargins,
    verticalAlign: "center",
    children: [new Paragraph({ alignment: AlignmentType.CENTER, children: [new TextRun({ text, bold: true, font: "Malgun Gothic", size: 20 })] })]
  });
}

function heading1(text) {
  return new Paragraph({
    heading: HeadingLevel.HEADING_1,
    spacing: { before: 360, after: 200 },
    children: [new TextRun({ text, font: "Malgun Gothic" })]
  });
}

function heading2(text) {
  return new Paragraph({
    heading: HeadingLevel.HEADING_2,
    spacing: { before: 280, after: 160 },
    children: [new TextRun({ text, font: "Malgun Gothic" })]
  });
}

function heading3(text) {
  return new Paragraph({
    heading: HeadingLevel.HEADING_3,
    spacing: { before: 200, after: 120 },
    children: [new TextRun({ text, font: "Malgun Gothic" })]
  });
}

function para(text, opts = {}) {
  return new Paragraph({
    spacing: { after: opts.afterSpacing || 120, line: 360 },
    indent: opts.indent ? { left: opts.indent } : undefined,
    children: [new TextRun({ text, font: "Malgun Gothic", size: 22, bold: opts.bold || false, italics: opts.italics || false })]
  });
}

function bullet(text, level = 0) {
  return new Paragraph({
    numbering: { reference: "bullets", level },
    spacing: { after: 80, line: 340 },
    children: [new TextRun({ text, font: "Malgun Gothic", size: 22 })]
  });
}

function emptyLine() {
  return new Paragraph({ spacing: { after: 80 }, children: [] });
}

// ── Table builders ───────────────────────────────────────────────
function makeTable(headers, rows, colWidths) {
  const totalWidth = colWidths.reduce((a, b) => a + b, 0);
  return new Table({
    width: { size: totalWidth, type: WidthType.DXA },
    columnWidths: colWidths,
    rows: [
      new TableRow({ children: headers.map((h, i) => headerCell(h, colWidths[i])) }),
      ...rows.map((row, ri) =>
        new TableRow({
          children: row.map((c, ci) =>
            cell(c, colWidths[ci], { shading: ri % 2 === 1 ? altRowShading : undefined, center: ci === 0 })
          )
        })
      )
    ]
  });
}

// ── Document ─────────────────────────────────────────────────────
const doc = new Document({
  styles: {
    default: { document: { run: { font: "Malgun Gothic", size: 22 } } },
    paragraphStyles: [
      { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 36, bold: true, font: "Malgun Gothic", color: "1A3764" },
        paragraph: { spacing: { before: 360, after: 200 }, outlineLevel: 0,
          border: { bottom: { style: BorderStyle.SINGLE, size: 6, color: "2E5090", space: 4 } }
        }
      },
      { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 30, bold: true, font: "Malgun Gothic", color: "2E5090" },
        paragraph: { spacing: { before: 280, after: 160 }, outlineLevel: 1 }
      },
      { id: "Heading3", name: "Heading 3", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 26, bold: true, font: "Malgun Gothic", color: "3A6EA5" },
        paragraph: { spacing: { before: 200, after: 120 }, outlineLevel: 2 }
      },
    ]
  },
  numbering: {
    config: [
      { reference: "bullets",
        levels: [
          { level: 0, format: LevelFormat.BULLET, text: "\u2022", alignment: AlignmentType.LEFT,
            style: { paragraph: { indent: { left: 720, hanging: 360 } } } },
          { level: 1, format: LevelFormat.BULLET, text: "\u25E6", alignment: AlignmentType.LEFT,
            style: { paragraph: { indent: { left: 1440, hanging: 360 } } } },
        ]
      },
      { reference: "numbers",
        levels: [
          { level: 0, format: LevelFormat.DECIMAL, text: "%1.", alignment: AlignmentType.LEFT,
            style: { paragraph: { indent: { left: 720, hanging: 360 } } } },
        ]
      },
    ]
  },
  sections: [
    // ════════════════════════════════════════════════
    // COVER PAGE
    // ════════════════════════════════════════════════
    {
      properties: {
        page: {
          size: { width: 11906, height: 16838 },
          margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 }
        }
      },
      children: [
        emptyLine(), emptyLine(), emptyLine(), emptyLine(), emptyLine(),
        emptyLine(), emptyLine(), emptyLine(),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          spacing: { after: 200 },
          children: [new TextRun({ text: "3 Match Puzzle RPG", font: "Malgun Gothic", size: 56, bold: true, color: "1A3764" })]
        }),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          spacing: { after: 600 },
          border: { bottom: { style: BorderStyle.SINGLE, size: 8, color: "2E5090", space: 8 } },
          children: [new TextRun({ text: "\uC2DC\uC2A4\uD15C \uAE30\uD68D\uC11C", font: "Malgun Gothic", size: 44, bold: true, color: "2E5090" })]
        }),
        emptyLine(), emptyLine(),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          spacing: { after: 120 },
          children: [new TextRun({ text: "\uBB38\uC11C \uBC84\uC804: v1.0", font: "Malgun Gothic", size: 24, color: "666666" })]
        }),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          spacing: { after: 120 },
          children: [new TextRun({ text: "\uC791\uC131\uC77C: 2026\uB144 4\uC6D4 14\uC77C", font: "Malgun Gothic", size: 24, color: "666666" })]
        }),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          spacing: { after: 120 },
          children: [new TextRun({ text: "\uC7A5\uB974: \uD37C\uC990 RPG (\uB9E4\uCE58-3 + \uD134\uC81C RPG)", font: "Malgun Gothic", size: 24, color: "666666" })]
        }),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          spacing: { after: 120 },
          children: [new TextRun({ text: "\uD50C\uB7AB\uD3FC: Unity (Mobile/PC)", font: "Malgun Gothic", size: 24, color: "666666" })]
        }),
      ]
    },
    // ════════════════════════════════════════════════
    // TABLE OF CONTENTS (page)
    // ════════════════════════════════════════════════
    {
      properties: {
        page: {
          size: { width: 11906, height: 16838 },
          margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 }
        }
      },
      headers: {
        default: new Header({
          children: [new Paragraph({
            alignment: AlignmentType.RIGHT,
            children: [new TextRun({ text: "3 Match Puzzle RPG \uC2DC\uC2A4\uD15C \uAE30\uD68D\uC11C", font: "Malgun Gothic", size: 18, color: "999999", italics: true })]
          })]
        })
      },
      footers: {
        default: new Footer({
          children: [new Paragraph({
            alignment: AlignmentType.CENTER,
            children: [new TextRun({ text: "- ", font: "Malgun Gothic", size: 18, color: "999999" }), new TextRun({ children: [PageNumber.CURRENT], font: "Malgun Gothic", size: 18, color: "999999" }), new TextRun({ text: " -", font: "Malgun Gothic", size: 18, color: "999999" })]
          })]
        })
      },
      children: [
        new Paragraph({
          spacing: { after: 400 },
          border: { bottom: { style: BorderStyle.SINGLE, size: 6, color: "2E5090", space: 4 } },
          children: [new TextRun({ text: "\uBAA9\uCC28 (Table of Contents)", font: "Malgun Gothic", size: 36, bold: true, color: "1A3764" })]
        }),
        emptyLine(),
        para("1. \uAC8C\uC784 \uAC1C\uC694 (Game Overview)"),
        para("2. \uAC8C\uC784 \uD50C\uB85C\uC6B0 (Game Flow)"),
        para("3. \uD37C\uC990 \uBCF4\uB4DC \uC2DC\uC2A4\uD15C (Puzzle Board System)"),
        para("4. \uC804\uD22C \uC2DC\uC2A4\uD15C (Battle System)"),
        para("5. \uCE90\uB9AD\uD130 \uC2DC\uC2A4\uD15C (Character System)"),
        para("6. \uC2A4\uD0AC \uC2DC\uC2A4\uD15C (Skill System)"),
        para("7. \uC7A5\uBE44 \uC2DC\uC2A4\uD15C (Equipment System)"),
        para("8. \uC2A4\uD14C\uC774\uC9C0/\uB808\uBCA8 \uC2DC\uC2A4\uD15C (Stage/Level System)"),
        para("9. \uC800\uC7A5/\uC9C4\uD589 \uC2DC\uC2A4\uD15C (Save/Progression System)"),
        para("10. UI \uC2DC\uC2A4\uD15C (UI System)"),
        para("11. \uC2A4\uCF54\uC5B4 \uC2DC\uC2A4\uD15C (Score System)"),
        para("12. \uC2A4\uD1A0\uB9AC/\uCEF7\uC2E0 \uC2DC\uC2A4\uD15C (Story/Cutscene System)"),
        para("13. \uD29C\uD1A0\uB9AC\uC5BC \uC2DC\uC2A4\uD15C (Tutorial System)"),
        para("14. \uB370\uC774\uD130 \uC124\uACC4 (Data Architecture)"),
        para("15. \uC528 \uAD6C\uC870 \uBC0F \uB9E4\uB2C8\uC800 \uC124\uACC4 (Scene & Manager Design)"),
      ]
    },
    // ════════════════════════════════════════════════
    // MAIN CONTENT
    // ════════════════════════════════════════════════
    {
      properties: {
        page: {
          size: { width: 11906, height: 16838 },
          margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 }
        }
      },
      headers: {
        default: new Header({
          children: [new Paragraph({
            alignment: AlignmentType.RIGHT,
            children: [new TextRun({ text: "3 Match Puzzle RPG \uC2DC\uC2A4\uD15C \uAE30\uD68D\uC11C", font: "Malgun Gothic", size: 18, color: "999999", italics: true })]
          })]
        })
      },
      footers: {
        default: new Footer({
          children: [new Paragraph({
            alignment: AlignmentType.CENTER,
            children: [new TextRun({ text: "- ", font: "Malgun Gothic", size: 18, color: "999999" }), new TextRun({ children: [PageNumber.CURRENT], font: "Malgun Gothic", size: 18, color: "999999" }), new TextRun({ text: " -", font: "Malgun Gothic", size: 18, color: "999999" })]
          })]
        })
      },
      children: [
        // ──────────────────────────────────────────
        // 1. 게임 개요
        // ──────────────────────────────────────────
        heading1("1. \uAC8C\uC784 \uAC1C\uC694 (Game Overview)"),

        heading2("1.1 \uAC8C\uC784 \uCEE8\uC149"),
        para("3 Match Puzzle RPG\uB294 \uD074\uB798\uC2DD \uB9E4\uCE58-3 \uD37C\uC990 \uBA54\uCE74\uB2C9\uACFC \uD134\uC81C RPG \uC804\uD22C \uC2DC\uC2A4\uD15C\uC744 \uACB0\uD569\uD55C \uD558\uC774\uBE0C\uB9AC\uB4DC \uC7A5\uB974 \uAC8C\uC784\uC774\uB2E4. \uD50C\uB808\uC774\uC5B4\uB294 10x10 \uD37C\uC990 \uBCF4\uB4DC\uC5D0\uC11C \uD0C0\uC77C\uC744 \uB9E4\uCE6D\uD558\uC5EC \uBAAC\uC2A4\uD130\uC5D0\uAC8C \uB370\uBBF8\uC9C0\uB97C \uC8FC\uACE0, \uD30C\uD2F0 \uBA64\uBC84\uB97C \uD68C\uBCF5\uC2DC\uD0A4\uBA70, \uC2A4\uD14C\uC774\uC9C0\uB97C \uD074\uB9AC\uC5B4\uD574 \uB098\uAC04\uB2E4."),

        heading2("1.2 \uD575\uC2EC \uD2B9\uC9D5"),
        bullet("\uB9E4\uCE58-3 \uD37C\uC990: 4\uAC00\uC9C0 \uD0C0\uC77C \uD0C0\uC785(Sword, Wand, Bow, Cross)\uC744 \uD65C\uC6A9\uD55C \uC804\uB7B5\uC801 \uD37C\uC990"),
        bullet("\uD134\uC81C RPG \uC804\uD22C: \uD37C\uC990 \uB9E4\uCE6D \uACB0\uACFC\uAC00 \uC804\uD22C\uC5D0 \uC9C1\uC811 \uBC18\uC601\uB418\uB294 \uC804\uD22C \uC2DC\uC2A4\uD15C"),
        bullet("4\uC778 \uD30C\uD2F0 \uC2DC\uC2A4\uD15C: \uAC01\uAC01 \uACE0\uC720 \uC2A4\uD0AC\uACFC \uC7A5\uBE44\uB97C \uAC00\uC9C4 4\uBA85\uC758 \uD30C\uD2F0 \uBA64\uBC84"),
        bullet("\uC2A4\uD14C\uC774\uC9C0 \uC9C4\uD589: 10\uAC1C \uC2A4\uD14C\uC774\uC9C0 (\uD29C\uD1A0\uB9AC\uC5BC + 3\uAC1C \uCC55\uD130, \uCC55\uD130\uB2F9 3\uAC1C \uC2A4\uD14C\uC774\uC9C0)"),
        bullet("\uC131\uC7A5 \uC2DC\uC2A4\uD15C: \uCE90\uB9AD\uD130 \uC2A4\uD0EF \uC5C5\uADF8\uB808\uC774\uB4DC, \uC2A4\uD0AC \uD574\uAE08, \uC7A5\uBE44 \uAD6C\uB9E4"),
        bullet("\uC2A4\uD1A0\uB9AC: \uCEF7\uC2E0/\uB300\uD654 \uC2DC\uC2A4\uD15C\uC744 \uD1B5\uD55C \uC2A4\uD1A0\uB9AC \uC804\uAC1C"),

        heading2("1.3 \uD0C0\uAC9F \uD50C\uB7AB\uD3FC"),
        para("Unity \uAE30\uBC18 \uBAA8\uBC14\uC77C/PC \uD06C\uB85C\uC2A4 \uD50C\uB7AB\uD3FC"),

        heading2("1.4 \uAC8C\uC784 \uBAA8\uB4DC"),
        makeTable(
          ["\uBAA8\uB4DC", "\uC124\uBA85"],
          [
            ["\uBA54\uC778 \uBA54\uB274", "\uC0C8 \uAC8C\uC784, \uC774\uC5B4\uD558\uAE30, \uC635\uC158 \uAD00\uB9AC"],
            ["\uB9F5 \uD654\uBA74", "\uCC55\uD130 \uC120\uD0DD, \uC2A4\uD14C\uC774\uC9C0 \uC120\uD0DD \uD328\uB110"],
            ["\uC804\uD22C \uC528", "\uD37C\uC990 \uBCF4\uB4DC + \uBAAC\uC2A4\uD130 \uC804\uD22C"],
            ["\uC2A4\uD1A0\uB9AC \uC528", "\uCEF7\uC2E0/\uB300\uD654 \uC5F0\uCD9C"],
            ["\uD0B9\uB364 \uC528", "\uC7A5\uBE44/\uC2A4\uD0AC/\uC2A4\uD14C\uC774\uD130\uC2A4 \uAD00\uB9AC"],
          ],
          [3000, 6026]
        ),

        // ──────────────────────────────────────────
        // 2. 게임 플로우
        // ──────────────────────────────────────────
        new Paragraph({ children: [new PageBreak()] }),
        heading1("2. \uAC8C\uC784 \uD50C\uB85C\uC6B0 (Game Flow)"),

        heading2("2.1 \uC804\uCCB4 \uAC8C\uC784 \uD50C\uB85C\uC6B0"),
        para("\uD50C\uB808\uC774\uC5B4\uAC00 \uAC8C\uC784\uC744 \uC2DC\uC791\uBD80\uD130 \uC2A4\uD14C\uC774\uC9C0 \uD074\uB9AC\uC5B4\uAE4C\uC9C0\uC758 \uC804\uCCB4 \uD50C\uB85C\uC6B0\uB294 \uB2E4\uC74C\uACFC \uAC19\uB2E4:"),
        emptyLine(),
        para("\uBA54\uC778 \uBA54\uB274 \u2192 [\uC0C8 \uAC8C\uC784/\uC774\uC5B4\uD558\uAE30] \u2192 (\uD504\uB864\uB85C\uADF8 \uCEF7\uC2E0) \u2192 \uB9F5 \uD654\uBA74 \u2192 \uCC55\uD130/\uC2A4\uD14C\uC774\uC9C0 \uC120\uD0DD \u2192 (\uC778\uD2B8\uB85C \uCEF7\uC2E0) \u2192 \uC804\uD22C \u2192 (\uD398\uC774\uC988 2 \uCEF7\uC2E0) \u2192 \uC2B9\uB9AC/\uD328\uBC30 \u2192 (\uD074\uB9AC\uC5B4 \uCEF7\uC2E0) \u2192 \uBCF4\uC0C1 \u2192 \uB9F5 \uBCF5\uADC0", { bold: true }),

        heading2("2.2 \uC804\uD22C \uB8E8\uD504 \uD50C\uB85C\uC6B0"),
        bullet("\uC2A4\uD14C\uC774\uC9C0 \uC9C4\uC785 \u2192 BattleSceneConfigApplier\uAC00 \uC2A4\uD14C\uC774\uC9C0 \uB370\uC774\uD130 \uB85C\uB4DC"),
        bullet("\uD50C\uB808\uC774\uC5B4 \uD0C0\uC77C \uC120\uD0DD \u2192 \uC2A4\uC654 \u2192 \uB9E4\uCE6D \uAC10\uC9C0"),
        bullet("\uB9E4\uCE6D \uC131\uACF5 \u2192 \uB370\uBBF8\uC9C0/\uD790 \uC801\uC6A9 \u2192 \uD0C0\uC77C \uC81C\uAC70 \u2192 \uC911\uB825 \uC801\uC6A9 \u2192 \uC0C8 \uD0C0\uC77C \uC0DD\uC131 \u2192 \uCE90\uC2A4\uCF00\uC774\uB4DC \uCCB4\uD06C"),
        bullet("\uD134 \uC99D\uAC00 \u2192 \uBAAC\uC2A4\uD130 \uACF5\uACA9 \uCCB4\uD06C (attackIntervalTurns\uB9C8\uB2E4)"),
        bullet("\uC2B9\uB9AC \uC870\uAC74: \uBAAC\uC2A4\uD130 HP 0 / \uD328\uBC30 \uC870\uAC74: \uD30C\uD2F0 \uC804\uBA38 \uB610\uB294 \uD134 \uC18C\uC9C4"),

        heading2("2.3 \uC2DC\uC2A4\uD15C \uC0C1\uD0DC \uC804\uC774 (GameState)"),
        makeTable(
          ["\uC0C1\uD0DC", "\uC124\uBA85", "\uC804\uC774 \uC870\uAC74"],
          [
            ["None", "\uCD08\uAE30 \uC0C1\uD0DC", "\uC528 \uB85C\uB4DC \uC2DC"],
            ["Menu", "\uBA54\uB274 \uD654\uBA74", "\uBA54\uB274 \uC9C4\uC785"],
            ["Playing", "\uD50C\uB808\uC774\uC5B4 \uC785\uB825 \uB300\uAE30", "\uBCF4\uB4DC \uCD08\uAE30\uD654 \uC644\uB8CC"],
            ["Swapping", "\uD0C0\uC77C \uAD50\uD658 \uC911", "\uD50C\uB808\uC774\uC5B4 \uC785\uB825"],
            ["Matching", "\uB9E4\uCE6D \uAC10\uC9C0 \uC911", "\uC2A4\uC654 \uC644\uB8CC"],
            ["Clearing", "\uD0C0\uC77C \uC81C\uAC70 \uC911", "\uB9E4\uCE6D \uBC1C\uACAC"],
            ["Falling", "\uC911\uB825 \uC801\uC6A9 \uC911", "\uD0C0\uC77C \uC81C\uAC70 \uC644\uB8CC"],
            ["Spawning", "\uC0C8 \uD0C0\uC77C \uC0DD\uC131", "\uC911\uB825 \uC801\uC6A9 \uC644\uB8CC"],
            ["GameOver", "\uAC8C\uC784 \uC885\uB8CC (\uD328\uBC30)", "\uD30C\uD2F0 \uC804\uBA78/\uD134 \uC18C\uC9C4"],
            ["LevelComplete", "\uC2A4\uD14C\uC774\uC9C0 \uD074\uB9AC\uC5B4", "\uBAAC\uC2A4\uD130 HP 0"],
          ],
          [2000, 3500, 3526]
        ),
        para("\u203B GameOver\uC640 LevelComplete\uB294 \uD130\uBBF8\uB110 \uC0C1\uD0DC\uB85C, \uD55C\uBC88 \uC9C4\uC785\uD558\uBA74 \uB2E4\uB978 \uC0C1\uD0DC\uB85C \uB36E\uC5B4\uC501\uC218 \uC5C6\uB2E4.", { italics: true }),

        // ──────────────────────────────────────────
        // 3. 퍼즐 보드 시스템
        // ──────────────────────────────────────────
        new Paragraph({ children: [new PageBreak()] }),
        heading1("3. \uD37C\uC990 \uBCF4\uB4DC \uC2DC\uC2A4\uD15C (Puzzle Board System)"),

        heading2("3.1 \uBCF4\uB4DC \uAD6C\uC131"),
        bullet("10x10 \uADF8\uB9AC\uB4DC \uAE30\uBC18 \uBCF4\uB4DC (\uCD1D 100\uAC1C \uC2AC\uB86F)"),
        bullet("UI Image \uCEF4\uD3EC\uB10C\uD2B8 \uAE30\uBC18 \uC2AC\uB86F \uBC30\uCE58 (\uC5D0\uB514\uD130\uC5D0\uC11C \uC0AC\uC804 \uBC30\uCE58)"),
        bullet("\uC2AC\uB86F \uAE30\uBC18 \uCC98\uB9AC (Slot-based): \uC560\uB2C8\uBA54\uC774\uC158 \uC5C6\uC774 \uB370\uC774\uD130 \uC989\uC2DC \uBC18\uC601"),

        heading2("3.2 \uD0C0\uC77C \uD0C0\uC785"),
        makeTable(
          ["\uD0C0\uC785 ID", "\uC774\uB984", "\uC124\uBA85", "\uC804\uD22C \uD6A8\uACFC"],
          [
            ["0", "Sword (\uAC80)", "\uADFC\uC811 \uACF5\uACA9 \uD0C0\uC77C", "\uBAAC\uC2A4\uD130\uC5D0\uAC8C \uAC80 \uB370\uBBF8\uC9C0"],
            ["1", "Wand (\uC644\uB4DC)", "\uB9C8\uBC95 \uACF5\uACA9 \uD0C0\uC77C", "\uBAAC\uC2A4\uD130\uC5D0\uAC8C \uC644\uB4DC \uB370\uBBF8\uC9C0"],
            ["2", "Bow (\uD65C)", "\uC6D0\uAC70\uB9AC \uACF5\uACA9 \uD0C0\uC77C", "\uBAAC\uC2A4\uD130\uC5D0\uAC8C \uD65C \uB370\uBBF8\uC9C0"],
            ["3", "Cross (\uC2ED\uC790\uAC00)", "\uD68C\uBCF5 \uD0C0\uC77C", "\uD30C\uD2F0 \uC804\uC6D0 HP \uD68C\uBCF5"],
            ["4~7", "Enhanced (\uAC15\uD654)", "\uAC01 \uAE30\uBCF8 \uD0C0\uC785\uC758 \uAC15\uD654 \uBC84\uC804", "\uCD94\uAC00 \uBCF4\uB108\uC2A4 \uB370\uBBF8\uC9C0/\uD68C\uBCF5"],
          ],
          [1200, 1800, 3000, 3026]
        ),

        heading2("3.3 \uB9E4\uCE6D \uC2DC\uC2A4\uD15C (MatchDetector)"),
        heading3("3.3.1 \uAE30\uBCF8 \uB9E4\uCE6D \uADDC\uCE59"),
        bullet("\uCD5C\uC18C 3\uAC1C \uC774\uC0C1\uC758 \uB3D9\uC77C \uD0C0\uC785 \uD0C0\uC77C\uC774 \uAC00\uB85C \uB610\uB294 \uC138\uB85C\uB85C \uC5F0\uC18D \uBC30\uCE58"),
        bullet("\uC778\uC811\uD55C \uD0C0\uC77C\uB9CC \uAD50\uD658(Swap) \uAC00\uB2A5"),
        bullet("\uB9E4\uCE6D\uC774 \uBC1C\uC0DD\uD558\uC9C0 \uC54A\uB294 \uC2A4\uC654\uC740 \uC790\uB3D9 \uBCF5\uADC0 (Revert)"),

        heading3("3.3.2 \uB9E4\uCE6D \uD328\uD134 \uBD84\uB958 (MatchGroup)"),
        makeTable(
          ["\uD328\uD134", "\uC870\uAC74", "\uD2B9\uC218 \uD6A8\uACFC"],
          [
            ["Horizontal", "3\uAC1C+ \uAC00\uB85C \uC5F0\uC18D", "\uAE30\uBCF8 \uD6A8\uACFC"],
            ["Vertical", "3\uAC1C+ \uC138\uB85C \uC5F0\uC18D", "\uAE30\uBCF8 \uD6A8\uACFC"],
            ["L-Shape", "L\uC790 \uD615\uD0DC \uB9E4\uCE6D", "\uBCF5\uD569 \uD6A8\uACFC"],
            ["T-Shape", "T\uC790 \uD615\uD0DC \uB9E4\uCE6D", "\uBCF5\uD569 \uD6A8\uACFC"],
            ["Cross", "\uC2ED\uC790 \uD615\uD0DC \uB9E4\uCE6D", "\uBCF5\uD569 \uD6A8\uACFC"],
          ],
          [2000, 3500, 3526]
        ),

        heading3("3.3.3 \uAC15\uD654 \uD0C0\uC77C \uC0DD\uC131"),
        bullet("4\uAC1C \uC774\uC0C1 \uB9E4\uCE6D \uC2DC \uAC15\uD654 \uD0C0\uC77C(Enhanced Tile) \uC790\uB3D9 \uC0DD\uC131"),
        bullet("\uAC15\uD654 \uD0C0\uC77C\uC740 \uAE30\uBCF8 \uD0C0\uC785\uC758 \uC0C1\uC704 \uBC84\uC804 (Type ID 4~7)"),
        bullet("\uAC15\uD654 \uD0C0\uC77C \uD3EC\uD568 \uB9E4\uCE6D \uC2DC \uCD94\uAC00 \uBCF4\uB108\uC2A4 \uB370\uBBF8\uC9C0 (baseEnhancedBonus: 50)"),

        heading2("3.4 \uC785\uB825 \uCC98\uB9AC (InputHandler)"),
        makeTable(
          ["\uC785\uB825 \uBC29\uC2DD", "\uC124\uBA85"],
          [
            ["\uD074\uB9AD-\uB4DC\uB798\uADF8", "\uD0C0\uC77C \uD074\uB9AD \uD6C4 \uC778\uC811 \uBC29\uD5A5\uC73C\uB85C \uB4DC\uB798\uADF8"],
            ["\uC778\uC811 \uD0C0\uC77C \uD3EC\uC778\uD130", "\uD130\uCE58\uD328\uB4DC/\uD130\uCE58 \uC2A4\uD06C\uB9B0 \uB300\uC751"],
            ["2-\uD074\uB9AD \uC2A4\uC654", "\uD0C0\uC77C A \uD074\uB9AD \u2192 \uC778\uC811 \uD0C0\uC77C B \uD074\uB9AD"],
          ],
          [3000, 6026]
        ),

        heading2("3.5 \uCE90\uC2A4\uCF00\uC774\uB4DC \uCC98\uB9AC (TileClearer)"),
        para("\uB9E4\uCE6D \uD6C4 \uCC98\uB9AC \uD50C\uB85C\uC6B0:"),
        bullet("\uB9E4\uCE6D\uB41C \uD0C0\uC77C \uB9C8\uD0B9 \u2192 \uAC15\uD654 \uD0C0\uC77C \uC0DD\uC131 \uD310\uC815"),
        bullet("\uB9E4\uCE6D \uD6A8\uACFC \uC801\uC6A9 (\uB370\uBBF8\uC9C0/\uD790)"),
        bullet("\uD0C0\uC77C \uC81C\uAC70 \uC560\uB2C8\uBA54\uC774\uC158 (\uD0C0\uC77C\uB2F9 0.1\uCD08 \uB51C\uB808\uC774)"),
        bullet("\uC911\uB825 \uC801\uC6A9 (GravityController) \u2192 \uBE48 \uC2AC\uB86F\uC744 \uC704\uC5D0\uC11C \uCC44\uC6C0"),
        bullet("\uC0C8 \uD0C0\uC77C \uC0DD\uC131 (TileSpawner) \u2192 \uC778\uC811 \uC911\uBCF5 \uD68C\uD53C"),
        bullet("\uC7AC\uADC0\uC801 \uB9E4\uCE6D \uCCB4\uD06C \u2192 \uCD94\uAC00 \uB9E4\uCE6D \uBC1C\uACAC \uC2DC \uBC18\uBCF5"),

        // ──────────────────────────────────────────
        // 4. 전투 시스템
        // ──────────────────────────────────────────
        new Paragraph({ children: [new PageBreak()] }),
        heading1("4. \uC804\uD22C \uC2DC\uC2A4\uD15C (Battle System)"),

        heading2("4.1 \uC804\uD22C \uAC1C\uC694"),
        para("\uD50C\uB808\uC774\uC5B4\uC758 4\uC778 \uD30C\uD2F0\uAC00 \uBAAC\uC2A4\uD130(\uBCF4\uC2A4)\uC640 \uC804\uD22C\uD55C\uB2E4. \uD37C\uC990 \uBCF4\uB4DC\uC5D0\uC11C \uD0C0\uC77C\uC744 \uB9E4\uCE6D\uD558\uC5EC \uBAAC\uC2A4\uD130\uC5D0\uAC8C \uB370\uBBF8\uC9C0\uB97C \uC8FC\uACE0, Cross \uD0C0\uC77C\uC744 \uB9E4\uCE6D\uD558\uC5EC \uD30C\uD2F0\uB97C \uD68C\uBCF5\uC2DC\uD0A8\uB2E4."),

        heading2("4.2 \uB370\uBBF8\uC9C0 \uACF5\uC2DD"),
        para("\uAE30\uBCF8 \uB370\uBBF8\uC9C0 = (baseDmg \u00D7 matchCount + enhancedBonus \u00D7 enhancedCount) \u00D7 (1 - resistance) \u00D7 equipmentMultiplier", { bold: true }),
        emptyLine(),
        makeTable(
          ["\uBCC0\uC218", "\uC124\uBA85", "\uAE30\uBCF8\uAC12"],
          [
            ["baseDmg", "\uD0C0\uC785\uBCC4 \uAE30\uBCF8 \uB370\uBBF8\uC9C0", "\uAC80:30, \uD65C:80, \uC644\uB4DC:35"],
            ["matchCount", "\uB9E4\uCE6D\uB41C \uD0C0\uC77C \uC218", "3 \uC774\uC0C1"],
            ["enhancedBonus", "\uAC15\uD654 \uD0C0\uC77C 1\uAC1C\uB2F9 \uBCF4\uB108\uC2A4", "50"],
            ["enhancedCount", "\uB9E4\uCE6D\uC5D0 \uD3EC\uD568\uB41C \uAC15\uD654 \uD0C0\uC77C \uC218", "0 \uC774\uC0C1"],
            ["resistance", "\uBAAC\uC2A4\uD130 \uC18D\uC131 \uC800\uD56D (0~1)", "\uC2A4\uD14C\uC774\uC9C0\uBCC4 \uC124\uC815"],
            ["equipmentMultiplier", "\uC7A5\uBE44 \uACF5\uACA9\uB825 \uBC30\uC728", "\uC7A5\uBE44\uC5D0 \uB530\uB77C \uBCC0\uB3D9"],
          ],
          [2400, 4000, 2626]
        ),

        heading2("4.3 \uD790 \uACF5\uC2DD"),
        para("\uD790\uB7C9 = (healAmount + bonusPerLevel \u00D7 upgradeLevel) \u00D7 matchCount \u00D7 equipmentHealMultiplier", { bold: true }),
        bullet("\uAE30\uBCF8 healAmount: 25"),
        bullet("Cross \uD0C0\uC77C \uB9E4\uCE6D \uC2DC \uC0DD\uC874 \uD30C\uD2F0 \uC804\uC6D0 \uD68C\uBCF5"),

        heading2("4.4 \uBAAC\uC2A4\uD130 \uACF5\uACA9 \uC2DC\uC2A4\uD15C"),
        bullet("attackIntervalTurns: N\uD134\uB9C8\uB2E4 \uBAAC\uC2A4\uD130\uAC00 \uD30C\uD2F0\uB97C \uACF5\uACA9"),
        bullet("attackDamage: \uBAAC\uC2A4\uD130 \uACF5\uACA9 \uB370\uBBF8\uC9C0"),
        bullet("attackTargetType: \uACF5\uACA9 \uB300\uC0C1 \uC120\uD0DD \uBC29\uC2DD"),
        bullet("partyHpLossPerTurn: \uB9E4 \uD134\uB9C8\uB2E4 \uD30C\uD2F0 \uC804\uCCB4 HP \uAC10\uC18C (\uC120\uD0DD\uC801)"),

        heading2("4.5 \uD398\uC774\uC988 2 \uC2DC\uC2A4\uD15C"),
        para("\uC77C\uBD80 \uC2A4\uD14C\uC774\uC9C0\uB294 2\uD398\uC774\uC988 \uC804\uD22C \uAD6C\uC870\uB97C \uAC00\uC9C4\uB2E4:"),
        makeTable(
          ["\uD56D\uBAA9", "\uC124\uBA85"],
          [
            ["\uD2B8\uB9AC\uAC70 \uC870\uAC74", "MonsterHp: \uBAAC\uC2A4\uD130 HP\uAC00 \uC784\uACC4\uAC12 \uC774\uD558 / PartyAnyMemberHp: \uD30C\uD2F0 \uBA64\uBC84 HP \uC784\uACC4"],
            ["\uC804\uD658 \uC5F0\uCD9C", "StoryScene \uC560\uB514\uD2F0\uBE0C \uB85C\uB4DC \u2192 \uCEF7\uC2E0 \uC7AC\uC0DD \u2192 \uC5B8\uB85C\uB4DC"],
            ["\uBCC0\uACBD \uC694\uC18C", "\uBCF4\uC2A4 \uC2A4\uD504\uB77C\uC774\uD2B8 \uBCC0\uACBD, HP \uCD5C\uB300\uCE58 \uBCC0\uACBD, \uC0C8\uB85C\uC6B4 \uC800\uD56D\uAC12"],
            ["\uC0C1\uD0DC \uBCF4\uD638", "BattlePhaseRuntime.IsBattleCutsceneActive\uB85C \uCEF7\uC2E0 \uC911 \uC0C1\uD0DC \uBCC0\uACBD \uBC29\uC9C0"],
          ],
          [2400, 6626]
        ),

        heading2("4.6 \uC2B9\uB9AC/\uD328\uBC30 \uC870\uAC74"),
        makeTable(
          ["\uACB0\uACFC", "\uC870\uAC74"],
          [
            ["\uC2B9\uB9AC (LevelComplete)", "\uBAAC\uC2A4\uD130 HP\uAC00 0 \uC774\uD558"],
            ["\uD328\uBC30 (GameOver)", "\uD30C\uD2F0 \uC804\uC6D0 HP 0 \uB610\uB294 \uCD5C\uB300 \uD134 \uC218 \uCD08\uACFC"],
          ],
          [3000, 6026]
        ),
        bullet("\uD328\uBC30 \uC2DC: \uB9AC\uD50C\uB808\uC774 \uB610\uB294 \uC2A4\uD14C\uC774\uC9C0 \uC120\uD0DD\uC73C\uB85C \uBCF5\uADC0 \uC120\uD0DD \uAC00\uB2A5"),

        // ──────────────────────────────────────────
        // 5. 캐릭터 시스템
        // ──────────────────────────────────────────
        new Paragraph({ children: [new PageBreak()] }),
        heading1("5. \uCE90\uB9AD\uD130 \uC2DC\uC2A4\uD15C (Character System)"),

        heading2("5.1 \uD30C\uD2F0 \uAD6C\uC131"),
        bullet("\uACE0\uC815 4\uC778 \uD30C\uD2F0 \uC2DC\uC2A4\uD15C"),
        bullet("\uAC01 \uCE90\uB9AD\uD130: ID, \uD45C\uC2DC\uBA85, \uCEEC\uB7EC \uCD08\uC0C1\uD654, \uADF8\uB808\uC774\uC2A4\uCF00\uC77C \uCD08\uC0C1\uD654"),
        bullet("CharacterDatabase\uC5D0\uC11C ScriptableObject\uB85C \uAD00\uB9AC"),

        heading2("5.2 \uCE90\uB9AD\uD130 \uC2A4\uD0EF (CharacterStatsData)"),
        makeTable(
          ["\uC2A4\uD0EF \uD56D\uBAA9", "\uAE30\uBCF8\uAC12", "\uB808\uBCA8\uB2F9 \uBCF4\uB108\uC2A4", "\uC124\uBA85"],
          [
            ["swordDamage", "30", "\uC124\uC815\uAC12", "\uAC80 \uD0C0\uC77C \uAE30\uBCF8 \uB370\uBBF8\uC9C0"],
            ["bowDamage", "80", "\uC124\uC815\uAC12", "\uD65C \uD0C0\uC77C \uAE30\uBCF8 \uB370\uBBF8\uC9C0"],
            ["wandDamage", "35", "\uC124\uC815\uAC12", "\uC644\uB4DC \uD0C0\uC77C \uAE30\uBCF8 \uB370\uBBF8\uC9C0"],
            ["healAmount", "25", "\uC124\uC815\uAC12", "Cross \uD0C0\uC77C \uAE30\uBCF8 \uD68C\uBCF5\uB7C9"],
            ["enhancedBonus", "50", "\uC124\uC815\uAC12", "\uAC15\uD654 \uD0C0\uC77C \uCD94\uAC00 \uBCF4\uB108\uC2A4"],
          ],
          [2000, 1500, 1800, 3726]
        ),

        heading2("5.3 \uC2A4\uD0EF \uC5C5\uADF8\uB808\uC774\uB4DC \uC2DC\uC2A4\uD15C"),
        para("\uCD5C\uC885 \uC2A4\uD0EF = \uAE30\uBCF8\uAC12 + (\uC5C5\uADF8\uB808\uC774\uB4DC \uB808\uBCA8 \u00D7 \uB808\uBCA8\uB2F9 \uBCF4\uB108\uC2A4)", { bold: true }),
        emptyLine(),
        makeTable(
          ["\uC5C5\uADF8\uB808\uC774\uB4DC \uC2AC\uB86F", "\uB300\uC0C1 \uC2A4\uD0EF", "\uD6A8\uACFC"],
          [
            ["\uC2AC\uB86F 0", "Sword \uB370\uBBF8\uC9C0", "\uAC80 \uACF5\uACA9\uB825 \uC99D\uAC00"],
            ["\uC2AC\uB86F 1", "Bow \uB370\uBBF8\uC9C0", "\uD65C \uACF5\uACA9\uB825 \uC99D\uAC00"],
            ["\uC2AC\uB86F 2", "Wand \uB370\uBBF8\uC9C0", "\uC644\uB4DC \uACF5\uACA9\uB825 \uC99D\uAC00"],
            ["\uC2AC\uB86F 3", "Heal \uB7C9", "\uD68C\uBCF5\uB7C9 \uC99D\uAC00"],
            ["\uC2AC\uB86F 4", "Enhanced \uBCF4\uB108\uC2A4", "\uAC15\uD654 \uD0C0\uC77C \uBCF4\uB108\uC2A4 \uC99D\uAC00"],
          ],
          [2000, 3500, 3526]
        ),
        bullet("CharacterUpgradeHolder (Static): \uB7F0\uD0C0\uC784 \uC5C5\uADF8\uB808\uC774\uB4DC \uB808\uBCA8 \uBCF4\uAD00"),
        bullet("CharacterStatsResolver: \uCD5C\uC885 \uC2A4\uD0EF \uACC4\uC0B0 \uCC98\uB9AC"),

        heading2("5.4 \uCE90\uB9AD\uD130 HP \uC0C1\uD0DC \uC2DC\uAC01\uD654"),
        bullet("HP \uBE44\uC728\uC5D0 \uB530\uB77C \uCE90\uB9AD\uD130 \uC774\uBBF8\uC9C0 \uBCC0\uACBD"),
        bullet("HP\uAC00 \uB0AE\uC544\uC9C0\uBA74 \uB2E4\uCE5C \uC0C1\uD0DC \uC774\uBBF8\uC9C0\uB85C \uC804\uD658"),
        bullet("\uC0AC\uB9DD \uC2DC \uADF8\uB808\uC774\uC2A4\uCF00\uC77C \uCD08\uC0C1\uD654\uB85C \uC804\uD658"),

        // ──────────────────────────────────────────
        // 6. 스킬 시스템
        // ──────────────────────────────────────────
        new Paragraph({ children: [new PageBreak()] }),
        heading1("6. \uC2A4\uD0AC \uC2DC\uC2A4\uD15C (Skill System)"),

        heading2("6.1 \uC2A4\uD0AC \uB370\uC774\uD130 \uAD6C\uC870 (SkillData)"),
        makeTable(
          ["\uD544\uB4DC", "\uD0C0\uC785", "\uC124\uBA85"],
          [
            ["skillId", "int", "\uACE0\uC720 \uC2A4\uD0AC ID"],
            ["displayName", "string", "\uD45C\uC2DC \uC774\uB984"],
            ["description", "string", "\uC2A4\uD0AC \uC124\uBA85 \uD14D\uC2A4\uD2B8"],
            ["icon", "Sprite", "\uCEEC\uB7EC \uC544\uC774\uCF58"],
            ["iconGrayscale", "Sprite", "\uBE44\uD65C\uC131\uD654 \uC0C1\uD0DC \uC544\uC774\uCF58"],
            ["effectType", "enum", "Sword / Bow / Wand / Heal"],
            ["effectMultiplier", "float", "\uAE30\uBCF8 \uB370\uBBF8\uC9C0 \uB300\uBE44 \uBC30\uC728 (1.5 = 150%)"],
            ["cooldownTurns", "int", "\uCFE8\uB2E4\uC6B4 \uD134 \uC218 (4 = 4\uD134\uB9C8\uB2E4 \uC0AC\uC6A9 \uAC00\uB2A5)"],
            ["ownerCharacterIndex", "int", "\uC18C\uC720 \uCE90\uB9AD\uD130 \uC778\uB371\uC2A4 (0~3)"],
            ["requiredChapter", "int", "\uD574\uAE08 \uC870\uAC74 (\uCC55\uD130)"],
          ],
          [2800, 1200, 5026]
        ),

        heading2("6.2 \uC2A4\uD0AC \uC7A5\uCC29 \uADDC\uCE59"),
        bullet("4\uAC1C \uC2AC\uB86F (\uCE90\uB9AD\uD130 1\uBA85\uB2F9 1\uAC1C \uC2A4\uD0AC)"),
        bullet("\uAC01 \uCE90\uB9AD\uD130\uB294 \uC790\uC2E0\uC758 \uC2A4\uD0AC\uB9CC \uC7A5\uCC29 \uAC00\uB2A5 (ownerCharacterIndex)"),
        bullet("\uC7A5\uCC29 \uC815\uBCF4\uB294 GameSaveData\uC5D0 \uC800\uC7A5"),

        heading2("6.3 \uC2A4\uD0AC \uD574\uAE08 \uC870\uAC74"),
        bullet("requiredChapter \uAC12\uC5D0 \uB530\uB77C \uCC55\uD130 \uD074\uB9AC\uC5B4 \uC2DC \uC790\uB3D9 \uD574\uAE08"),
        bullet("\uD574\uAE08\uB41C \uC2A4\uD0AC ID\uB294 unlockedSkillIds\uC5D0 \uC800\uC7A5"),

        heading2("6.4 \uC2A4\uD0AC \uC0AC\uC6A9 \uD50C\uB85C\uC6B0"),
        bullet("\uC804\uD22C \uC911 \uCFE8\uB2E4\uC6B4 \uCDA9\uC871 \uC2DC \uC2A4\uD0AC \uBC84\uD2BC \uD65C\uC131\uD654"),
        bullet("\uC2A4\uD0AC \uC0AC\uC6A9 \u2192 effectType\uC5D0 \uB530\uB978 \uB370\uBBF8\uC9C0/\uD68C\uBCF5 \uBC1C\uB3D9"),
        bullet("\uD6A8\uACFC\uB7C9 = \uAE30\uBCF8 \uC2A4\uD0EF \u00D7 effectMultiplier"),
        bullet("\uCFE8\uB2E4\uC6B4 \uC2DC\uC791 \u2192 cooldownTurns \uD6C4 \uC7AC\uC0AC\uC6A9 \uAC00\uB2A5"),

        // ──────────────────────────────────────────
        // 7. 장비 시스템
        // ──────────────────────────────────────────
        new Paragraph({ children: [new PageBreak()] }),
        heading1("7. \uC7A5\uBE44 \uC2DC\uC2A4\uD15C (Equipment System)"),

        heading2("7.1 \uC7A5\uBE44 \uB370\uC774\uD130 \uAD6C\uC870 (EquipmentData)"),
        makeTable(
          ["\uD544\uB4DC", "\uD0C0\uC785", "\uC124\uBA85"],
          [
            ["equipmentId", "int", "\uACE0\uC720 \uC7A5\uBE44 ID"],
            ["displayName", "string", "\uD45C\uC2DC \uC774\uB984"],
            ["description", "string", "\uC7A5\uBE44 \uC124\uBA85"],
            ["TMI", "string", "\uCD94\uAC00 \uC815\uBCF4 \uD14D\uC2A4\uD2B8"],
            ["icon", "Sprite", "\uC7A5\uBE44 \uC544\uC774\uCF58"],
            ["slotType", "enum", "Slot0 \uB610\uB294 Slot1"],
            ["goldCost", "int", "\uAD6C\uB9E4 \uBE44\uC6A9 (\uACE8\uB4DC)"],
            ["requiredChapter", "int", "\uD574\uAE08 \uC870\uAC74 (\uCC55\uD130)"],
            ["partyMaxHpPercent", "float", "\uD30C\uD2F0 \uCD5C\uB300 HP \uC99D\uAC00\uC728"],
            ["attackPercent", "float", "\uACF5\uACA9\uB825 \uC99D\uAC00\uC728"],
            ["healPercent", "float", "\uD68C\uBCF5\uB825 \uC99D\uAC00\uC728"],
          ],
          [2800, 1200, 5026]
        ),

        heading2("7.2 \uC7A5\uBE44 \uC2AC\uB86F \uADDC\uCE59"),
        bullet("2\uAC1C \uC7A5\uBE44 \uC2AC\uB86F (Slot0, Slot1)"),
        bullet("\uAC01 \uC2AC\uB86F\uC5D0\uB294 \uD574\uB2F9 slotType\uC758 \uC7A5\uBE44\uB9CC \uC7A5\uCC29 \uAC00\uB2A5"),
        bullet("\uC7A5\uCC29 \uC815\uBCF4\uB294 EquippedEquipmentHolder(Static)\uC5D0\uC11C \uAD00\uB9AC"),

        heading2("7.3 \uC7A5\uBE44 \uD6A8\uACFC \uC801\uC6A9 (EquipmentStatModifier)"),
        para("\uC7A5\uCC29\uB41C \uBAA8\uB4E0 \uC7A5\uBE44\uC758 \uD6A8\uACFC\uB97C \uD569\uC0B0\uD558\uC5EC \uC804\uD22C\uC5D0 \uC801\uC6A9:"),
        bullet("HP \uBCF0\uD2F0\uD30C\uC774\uC5B4: partyMaxHpPercent \uD569\uC0B0"),
        bullet("\uACF5\uACA9 \uBCF0\uD2F0\uD30C\uC774\uC5B4: attackPercent \uD569\uC0B0"),
        bullet("\uD68C\uBCF5 \uBCF0\uD2F0\uD30C\uC774\uC5B4: healPercent \uD569\uC0B0"),
        bullet("MatchEffectHandler\uC5D0\uC11C \uB370\uBBF8\uC9C0/\uD68C\uBCF5 \uACC4\uC0B0 \uC2DC \uBC30\uC728 \uC801\uC6A9"),

        heading2("7.4 \uC7A5\uBE44 \uAD6C\uB9E4"),
        bullet("\uACE8\uB4DC\uB85C \uAD6C\uB9E4 (goldCost)"),
        bullet("\uCC55\uD130 \uD074\uB9AC\uC5B4 \uC2DC \uD574\uAE08 (requiredChapter)"),
        bullet("\uAD6C\uB9E4\uD55C \uC7A5\uBE44 ID\uB294 unlockedEquipmentIds\uC5D0 \uC800\uC7A5"),

        // ──────────────────────────────────────────
        // 8. 스테이지/레벨 시스템
        // ──────────────────────────────────────────
        new Paragraph({ children: [new PageBreak()] }),
        heading1("8. \uC2A4\uD14C\uC774\uC9C0/\uB808\uBCA8 \uC2DC\uC2A4\uD15C (Stage/Level System)"),

        heading2("8.1 \uC2A4\uD14C\uC774\uC9C0 \uAD6C\uC131"),
        makeTable(
          ["\uC2A4\uD14C\uC774\uC9C0 \uC778\uB371\uC2A4", "\uCC55\uD130", "\uC124\uBA85"],
          [
            ["0", "\uD29C\uD1A0\uB9AC\uC5BC", "\uD29C\uD1A0\uB9AC\uC5BC \uC2A4\uD14C\uC774\uC9C0"],
            ["1~3", "Chapter 1", "\uCC55\uD130 1 \uC2A4\uD14C\uC774\uC9C0 3\uAC1C"],
            ["4~6", "Chapter 2", "\uCC55\uD130 2 \uC2A4\uD14C\uC774\uC9C0 3\uAC1C"],
            ["7~9", "Chapter 3", "\uCC55\uD130 3 \uC2A4\uD14C\uC774\uC9C0 3\uAC1C"],
          ],
          [2400, 2400, 4226]
        ),

        heading2("8.2 \uC2A4\uD14C\uC774\uC9C0 \uB370\uC774\uD130 (StageData)"),
        heading3("8.2.1 \uAE30\uBCF8 \uC124\uC815"),
        makeTable(
          ["\uD544\uB4DC", "\uC124\uBA85"],
          [
            ["stageName", "\uC2A4\uD14C\uC774\uC9C0 \uC774\uB984"],
            ["description", "\uC2A4\uD14C\uC774\uC9C0 \uC124\uBA85"],
            ["backgroundSprite", "\uBC30\uACBD \uC774\uBBF8\uC9C0"],
            ["bossSprite", "\uBCF4\uC2A4 \uC774\uBBF8\uC9C0"],
            ["bossSpriteAfterClear", "\uD074\uB9AC\uC5B4 \uD6C4 \uBCF4\uC2A4 \uC774\uBBF8\uC9C0"],
            ["maxTurns", "\uCD5C\uB300 \uD134 \uC218"],
            ["clearGoldReward", "\uD074\uB9AC\uC5B4 \uBCF4\uC0C1 \uACE8\uB4DC"],
          ],
          [3000, 6026]
        ),

        heading3("8.2.2 \uBAAC\uC2A4\uD130 \uC124\uC815"),
        makeTable(
          ["\uD544\uB4DC", "\uC124\uBA85"],
          [
            ["monsterMaxHp", "\uBAAC\uC2A4\uD130 \uCD5C\uB300 HP"],
            ["attackIntervalTurns", "\uBAAC\uC2A4\uD130 \uACF5\uACA9 \uC8FC\uAE30 (N\uD134\uB9C8\uB2E4)"],
            ["attackDamage", "\uBAAC\uC2A4\uD130 \uACF5\uACA9 \uB370\uBBF8\uC9C0"],
            ["attackTargetType", "\uACF5\uACA9 \uB300\uC0C1 \uC120\uD0DD \uBC29\uC2DD"],
            ["swordResistance", "\uAC80 \uC800\uD56D\uAC12 (0~1)"],
            ["bowResistance", "\uD65C \uC800\uD56D\uAC12 (0~1)"],
            ["wandResistance", "\uC644\uB4DC \uC800\uD56D\uAC12 (0~1)"],
            ["partyStartHpDeduction", "\uC804\uD22C \uC2DC\uC791 \uC2DC \uD30C\uD2F0 HP \uCC28\uAC10"],
            ["partyHpLossPerTurn", "\uD134\uB2F9 \uD30C\uD2F0 HP \uAC10\uC18C"],
          ],
          [3000, 6026]
        ),

        heading3("8.2.3 \uD398\uC774\uC988 2 \uC124\uC815"),
        makeTable(
          ["\uD544\uB4DC", "\uC124\uBA85"],
          [
            ["hasPhase2", "\uD398\uC774\uC988 2 \uC874\uC7AC \uC5EC\uBD80"],
            ["phase2TriggerTarget", "\uD2B8\uB9AC\uAC70 \uB300\uC0C1 (MonsterHp / PartyAnyMemberHp)"],
            ["phase2TriggerHp", "\uD2B8\uB9AC\uAC70 HP \uC784\uACC4\uAC12"],
            ["bossSpritePhase2", "\uD398\uC774\uC988 2 \uBCF4\uC2A4 \uC774\uBBF8\uC9C0"],
            ["monsterMaxHpPhase2", "\uD398\uC774\uC988 2 \uBAAC\uC2A4\uD130 \uCD5C\uB300 HP"],
            ["phase2Cutscene", "\uD398\uC774\uC988 2 \uC804\uD658 \uCEF7\uC2E0"],
          ],
          [3000, 6026]
        ),

        heading3("8.2.4 \uBAAC\uC2A4\uD130 HP \uC2A4\uD504\uB77C\uC774\uD2B8"),
        para("\uBAAC\uC2A4\uD130 HP\uAC00 \uD2B9\uC815 \uBE44\uC728 \uC774\uD558\uB85C \uB5A8\uC5B4\uC9C0\uBA74 \uBCF4\uC2A4 \uC774\uBBF8\uC9C0\uAC00 \uB370\uBBF8\uC9C0 \uC0C1\uD0DC\uB85C \uBCC0\uACBD\uB41C\uB2E4:"),
        bullet("MonsterHpSpriteEntry[] \uBC30\uC5F4\uB85C HP \uBE44\uC728\uBCC4 \uC2A4\uD504\uB77C\uC774\uD2B8 \uC124\uC815"),
        bullet("\uD398\uC774\uC988 1, \uD398\uC774\uC988 2 \uAC01\uAC01 \uBCC4\uB3C4 \uC2A4\uD504\uB77C\uC774\uD2B8 \uC124\uC815 \uAC00\uB2A5"),

        heading2("8.3 \uD074\uB9AC\uC5B4 \uBCF4\uC0C1 \uC2DC\uC2A4\uD15C"),
        bullet("\uC2A4\uD14C\uC774\uC9C0 \uD074\uB9AC\uC5B4 \uC2DC clearGoldReward \uACE8\uB4DC \uC9C0\uAE09"),
        bullet("\uC2A4\uD14C\uC774\uC9C0\uB2F9 \uCD5C\uB300 2\uD68C \uBCF4\uC0C1 \uC218\uB839 \uAC00\uB2A5 (MaxRewardClaimPerStage = 2)"),
        bullet("stageRewardClaimCounts[] \uBC30\uC5F4\uB85C \uBCF4\uC0C1 \uC218\uB839 \uD69F\uC218 \uCD94\uC801"),

        heading2("8.4 \uB808\uBCA8 \uAD00\uB9AC (LevelManager)"),
        bullet("currentMoves: \uD604\uC7AC \uC0AC\uC6A9 \uD134 \uC218 \uCD94\uC801"),
        bullet("maxTurns: StageData\uC5D0\uC11C \uAC00\uC838\uC628 \uCD5C\uB300 \uD134 \uC218"),
        bullet("\uC774\uBCA4\uD2B8: OnTurnChanged, OnTurnsExhausted"),

        // ──────────────────────────────────────────
        // 9. 저장/진행 시스템
        // ──────────────────────────────────────────
        new Paragraph({ children: [new PageBreak()] }),
        heading1("9. \uC800\uC7A5/\uC9C4\uD589 \uC2DC\uC2A4\uD15C (Save/Progression System)"),

        heading2("9.1 \uC800\uC7A5 \uB370\uC774\uD130 \uAD6C\uC870 (GameSaveData)"),
        makeTable(
          ["\uBD84\uB958", "\uD544\uB4DC", "\uC124\uBA85"],
          [
            ["\uC9C4\uD589", "lastClearedChapter", "\uB9C8\uC9C0\uB9C9 \uD074\uB9AC\uC5B4 \uCC55\uD130"],
            ["\uC9C4\uD589", "lastClearedStageIndex", "\uB9C8\uC9C0\uB9C9 \uD074\uB9AC\uC5B4 \uC2A4\uD14C\uC774\uC9C0 \uC778\uB371\uC2A4"],
            ["\uC7AC\uD654", "gold", "\uBCF4\uC720 \uACE8\uB4DC"],
            ["\uC131\uC7A5", "playerLevel", "\uD50C\uB808\uC774\uC5B4 \uB808\uBCA8"],
            ["\uC131\uC7A5", "upgradePoints", "\uC5C5\uADF8\uB808\uC774\uB4DC \uD3EC\uC778\uD2B8"],
            ["\uD574\uAE08", "unlockedSkillIds[]", "\uD574\uAE08\uB41C \uC2A4\uD0AC ID \uBAA9\uB85D"],
            ["\uD574\uAE08", "unlockedEquipmentIds[]", "\uD574\uAE08\uB41C \uC7A5\uBE44 ID \uBAA9\uB85D"],
            ["\uC7A5\uCC29", "equippedSkillIds[4]", "\uC7A5\uCC29 \uC2A4\uD0AC (4\uC2AC\uB86F)"],
            ["\uC7A5\uCC29", "equippedEquipmentIds[2]", "\uC7A5\uCC29 \uC7A5\uBE44 (2\uC2AC\uB86F)"],
            ["\uBCF4\uC0C1", "stageRewardClaimCounts[10]", "\uC2A4\uD14C\uC774\uC9C0\uBCC4 \uBCF4\uC0C1 \uC218\uB839 \uD69F\uC218"],
            ["\uC131\uC7A5", "statUpgradeLevels[5]", "\uC2A4\uD0EF \uC5C5\uADF8\uB808\uC774\uB4DC \uB808\uBCA8 (5\uC885)"],
            ["\uD1B5\uACC4", "playTimeSeconds", "\uCD1D \uD50C\uB808\uC774 \uC2DC\uAC04"],
            ["\uD1B5\uACC4", "currentSceneName", "\uD604\uC7AC \uC528 \uC774\uB984"],
            ["\uD1B5\uACC4", "lastPlayTimeUtc", "\uB9C8\uC9C0\uB9C9 \uD50C\uB808\uC774 \uC2DC\uAC04 (UTC)"],
          ],
          [1200, 3200, 4626]
        ),

        heading2("9.2 \uC800\uC7A5 \uBC29\uC2DD"),
        bullet("\uC800\uC7A5 \uD615\uC2DD: JSON \uD30C\uC77C"),
        bullet("\uC800\uC7A5 \uACBD\uB85C: Application.persistentDataPath"),
        bullet("\uC790\uB3D9 \uC800\uC7A5: \uC2A4\uD14C\uC774\uC9C0 \uD074\uB9AC\uC5B4 \uC2DC \uC790\uB3D9 \uC800\uC7A5 (GameManager.SaveClearedStageAndLoadMap)"),

        heading2("9.3 \uC9C4\uD589 \uD574\uAE08 \uADDC\uCE59"),
        bullet("\uC2A4\uD14C\uC774\uC9C0 \uC21C\uCC28 \uD574\uAE08: lastClearedStageIndex \uAE30\uC900"),
        bullet("\uCC55\uD130 \uD574\uAE08: \uCC55\uD130 \uB0B4 \uBAA8\uB4E0 \uC2A4\uD14C\uC774\uC9C0 \uD074\uB9AC\uC5B4 \uC2DC"),
        bullet("\uC2A4\uD0AC/\uC7A5\uBE44 \uD574\uAE08: requiredChapter \uAE30\uC900\uC73C\uB85C \uC790\uB3D9 \uD574\uAE08"),

        heading2("9.4 \uB370\uC774\uD130 \uB85C\uB4DC \uD750\uB984"),
        bullet("GameSaveApplier: \uAC8C\uC784 \uC2DC\uC791 \uC2DC \uC800\uC7A5 \uB370\uC774\uD130 \uB85C\uB4DC"),
        bullet("Static Holder\uB4E4\uC5D0 \uB370\uC774\uD130 \uB3D9\uAE30\uD654 (CharacterUpgradeHolder, EquippedSkillsHolder, EquippedEquipmentHolder)"),
        bullet("\uB7F0\uD0C0\uC784 \uBCC0\uACBD \uC0AC\uD56D\uC740 Holder\uC5D0 \uBC18\uC601 \u2192 \uC800\uC7A5 \uC2DC GameSaveData\uC5D0 \uB3D9\uAE30\uD654"),

        // ──────────────────────────────────────────
        // 10. UI 시스템
        // ──────────────────────────────────────────
        new Paragraph({ children: [new PageBreak()] }),
        heading1("10. UI \uC2DC\uC2A4\uD15C (UI System)"),

        heading2("10.1 UIManager \uC8FC\uC694 \uAE30\uB2A5"),
        makeTable(
          ["UI \uC694\uC18C", "\uAE30\uB2A5"],
          [
            ["\uC2A4\uCF54\uC5B4/\uCF64\uBCF4 \uD45C\uC2DC", "\uD604\uC7AC \uC2A4\uCF54\uC5B4, \uCF64\uBCF4 \uC218, \uBCA0\uC2A4\uD2B8 \uC2A4\uCF54\uC5B4 \uD45C\uC2DC"],
            ["\uD134/\uC774\uB3D9 \uD45C\uC2DC", "\uD604\uC7AC \uD134 / \uCD5C\uB300 \uD134 \uD45C\uC2DC"],
            ["\uBAAC\uC2A4\uD130 HP UI", "\uBCF4\uC2A4 \uCCB4\uB825 \uBC14, \uBCF4\uC2A4 \uCD08\uC0C1\uD654 \uD45C\uC2DC"],
            ["\uD30C\uD2F0 HP UI", "4\uC778 \uD30C\uD2F0 \uAC1C\uBCC4 \uCCB4\uB825 \uBC14 \uD45C\uC2DC"],
            ["\uC2A4\uD14C\uC774\uC9C0 \uD074\uB9AC\uC5B4 \uD328\uB110", "\uD074\uB9AC\uC5B4 \uD1B5\uACC4, \uBCF4\uC0C1, \uB2E4\uC74C \uC2A4\uD14C\uC774\uC9C0/\uB9F5 \uBCF5\uADC0 \uBC84\uD2BC"],
            ["\uD328\uBC30 \uD328\uB110", "\uB9AC\uD50C\uB808\uC774/\uC2A4\uD14C\uC774\uC9C0 \uC120\uD0DD \uBC84\uD2BC"],
            ["\uACE8\uB4DC \uD45C\uC2DC", "GoldTextUI: \uD604\uC7AC \uBCF4\uC720 \uACE8\uB4DC \uD45C\uC2DC"],
            ["\uC635\uC158 \uD328\uB110", "\uAC8C\uC784 \uC124\uC815 \uBC0F \uC635\uC158"],
          ],
          [3000, 6026]
        ),

        heading2("10.2 \uD074\uB9AC\uC5B4 \uD1B5\uACC4 (BattleClearStatsRuntime)"),
        para("\uC804\uD22C \uC885\uB8CC \uC2DC \uB204\uC801\uB41C \uC804\uD22C \uD1B5\uACC4\uB97C \uD45C\uC2DC:"),
        bullet("\uD0C0\uC785\uBCC4 \uB370\uBBF8\uC9C0 (\uAC80/\uD65C/\uC644\uB4DC \uAC01\uAC01)"),
        bullet("\uCD1D \uD68C\uBCF5\uB7C9"),
        bullet("\uAC15\uD654 \uD0C0\uC77C \uC18C\uD658 \uC218"),

        heading2("10.3 \uC528 \uC804\uD658 \uD750\uB984"),
        bullet("\uC2A4\uD14C\uC774\uC9C0 \uD074\uB9AC\uC5B4 \u2192 \uD074\uB9AC\uC5B4 \uCEF7\uC2E0 \u2192 \uB2E4\uC74C \uC778\uD2B8\uB85C \uCEF7\uC2E0 \u2192 \uB2E4\uC74C \uC804\uD22C (\u201C\uB2E4\uC74C \uC2A4\uD14C\uC774\uC9C0\u201D)"),
        bullet("\uC2A4\uD14C\uC774\uC9C0 \uD074\uB9AC\uC5B4 \u2192 \uD074\uB9AC\uC5B4 \uCEF7\uC2E0 \u2192 \uB9F5 \uBCF5\uADC0 (\u201C\uB9F5\uC73C\uB85C \uB3CC\uC544\uAC00\uAE30\u201D)"),
        bullet("\uD328\uBC30 \u2192 \uB9AC\uD50C\uB808\uC774 \uB610\uB294 \uC2A4\uD14C\uC774\uC9C0 \uC120\uD0DD \uD654\uBA74"),
        bullet("AutoOpenStageSelectOnMap: \uB9F5 \uBCF5\uADC0 \uC2DC \uC2A4\uD14C\uC774\uC9C0 \uC120\uD0DD \uD328\uB110 \uC790\uB3D9 \uC624\uD508"),

        // ──────────────────────────────────────────
        // 11. 스코어 시스템
        // ──────────────────────────────────────────
        new Paragraph({ children: [new PageBreak()] }),
        heading1("11. \uC2A4\uCF54\uC5B4 \uC2DC\uC2A4\uD15C (Score System)"),

        heading2("11.1 \uC2A4\uCF54\uC5B4 \uACC4\uC0B0 (ScoreManager)"),
        makeTable(
          ["\uD56D\uBAA9", "\uC124\uBA85"],
          [
            ["baseMatchScore", "\uD0C0\uC77C 1\uAC1C\uB2F9 \uAE30\uBCF8 \uC810\uC218"],
            ["comboMultiplier", "\uCF64\uBCF4 \uBC30\uC728"],
            ["specialTileBonus", "\uD2B9\uC218 \uD0C0\uC77C \uBCF4\uB108\uC2A4 \uC810\uC218"],
            ["enhancedTileBonus", "\uAC15\uD654 \uD0C0\uC77C \uBCF4\uB108\uC2A4 \uC810\uC218"],
          ],
          [3000, 6026]
        ),

        heading2("11.2 \uC2A4\uCF54\uC5B4 \uCD94\uC801"),
        bullet("currentScore: \uD604\uC7AC \uD310 \uC2A4\uCF54\uC5B4"),
        bullet("comboCount: \uC5F0\uC18D \uB9E4\uCE6D \uCF64\uBCF4 \uC218"),
        bullet("bestScore: \uC5ED\uB300 \uCD5C\uACE0 \uC810\uC218 (PlayerPrefs \uC800\uC7A5)"),
        bullet("\uC774\uBCA4\uD2B8: OnScoreChanged, OnComboChanged, OnBestScoreChanged"),

        // ──────────────────────────────────────────
        // 12. 스토리/컷신 시스템
        // ──────────────────────────────────────────
        heading1("12. \uC2A4\uD1A0\uB9AC/\uCEF7\uC2E0 \uC2DC\uC2A4\uD15C (Story/Cutscene System)"),

        heading2("12.1 \uCEF7\uC2E0 \uB370\uC774\uD130 (CutsceneData)"),
        para("ScriptableObject \uAE30\uBC18 \uCEF7\uC2E0 \uB370\uC774\uD130 \uAD6C\uC870:"),
        makeTable(
          ["\uD544\uB4DC", "\uC124\uBA85"],
          [
            ["characterId", "\uB300\uD654 \uCE90\uB9AD\uD130 ID (-1 = \uB0B4\uB808\uC774\uD130)"],
            ["characterSpriteOverride", "\uCD5C\uB300 3\uCE90\uB9AD\uD130 \uB3D9\uC2DC \uD45C\uC2DC \uAC00\uB2A5"],
            ["speakerName", "\uD654\uC790 \uC774\uB984"],
            ["text", "\uB300\uD654 \uB0B4\uC6A9"],
            ["backgroundSprite", "\uB77C\uC778\uBCC4 \uBC30\uACBD \uBCC0\uACBD \uAC00\uB2A5"],
          ],
          [3000, 6026]
        ),

        heading2("12.2 \uCEF7\uC2E0 \uC0AC\uC6A9 \uC2DC\uC810"),
        makeTable(
          ["\uC2DC\uC810", "\uC124\uBA85", "\uB370\uC774\uD130 \uD544\uB4DC"],
          [
            ["\uC2A4\uD14C\uC774\uC9C0 \uC9C4\uC785 \uC804", "\uC2A4\uD1A0\uB9AC \uBC30\uACBD \uC124\uBA85", "introCutscene"],
            ["\uD398\uC774\uC988 2 \uC804\uD658", "\uBCF4\uC2A4 \uBCC0\uC2E0 \uC5F0\uCD9C", "phase2Cutscene"],
            ["\uC2A4\uD14C\uC774\uC9C0 \uD074\uB9AC\uC5B4", "\uC2B9\uB9AC \uD6C4 \uC2A4\uD1A0\uB9AC", "clearCutscene"],
            ["\uC0C8 \uAC8C\uC784 \uC2DC\uC791", "\uD504\uB864\uB85C\uADF8 \uC5F0\uCD9C", "prologuePanel"],
          ],
          [2400, 3000, 3626]
        ),

        heading2("12.3 \uB300\uD654 \uC5F0\uCD9C"),
        bullet("StoryDialogueController: \uCEF7\uC2E0 \uC7AC\uC0DD \uCEE8\uD2B8\uB864\uB7EC"),
        bullet("TextTyping / VNTextTyper: \uD0C0\uC774\uD551 \uD6A8\uACFC\uB85C \uD14D\uC2A4\uD2B8 \uCD9C\uB825"),
        bullet("\uD130\uCE58/\uD074\uB9AD\uC73C\uB85C \uB2E4\uC74C \uB300\uD654\uB85C \uC9C4\uD589"),

        // ──────────────────────────────────────────
        // 13. 튜토리얼 시스템
        // ──────────────────────────────────────────
        new Paragraph({ children: [new PageBreak()] }),
        heading1("13. \uD29C\uD1A0\uB9AC\uC5BC \uC2DC\uC2A4\uD15C (Tutorial System)"),

        heading2("13.1 \uD29C\uD1A0\uB9AC\uC5BC \uAD6C\uC131"),
        bullet("TutorialManager: \uD29C\uD1A0\uB9AC\uC5BC \uC9C4\uD589 \uAD00\uB9AC"),
        bullet("TutorialSceneStarter: \uD29C\uD1A0\uB9AC\uC5BC \uC528 \uCD08\uAE30\uD654"),
        bullet("\uC2A4\uD14C\uC774\uC9C0 \uC778\uB371\uC2A4 0\uC774 \uD29C\uD1A0\uB9AC\uC5BC \uC804\uC6A9 \uC2A4\uD14C\uC774\uC9C0"),

        heading2("13.2 \uD29C\uD1A0\uB9AC\uC5BC \uD2B9\uC218 \uCC98\uB9AC"),
        bullet("skipGravityAndSpawn \uD50C\uB798\uADF8: \uD29C\uD1A0\uB9AC\uC5BC \uBCF4\uB4DC\uC5D0\uC11C \uC911\uB825/\uC0DD\uC131 \uC2A4\uD0B5"),
        bullet("SwapCompleted \uC774\uBCA4\uD2B8: \uC2A4\uC654 \uC644\uB8CC \uC2DC \uD29C\uD1A0\uB9AC\uC5BC \uB2E4\uC74C \uB2E8\uACC4 \uC9C4\uD589"),
        bullet("\uAC00\uC774\uB4DC \uD654\uC0B4\uD45C/\uD558\uC774\uB77C\uC774\uD2B8\uB85C \uC870\uC791 \uC548\uB0B4"),

        // ──────────────────────────────────────────
        // 14. 데이터 설계
        // ──────────────────────────────────────────
        heading1("14. \uB370\uC774\uD130 \uC124\uACC4 (Data Architecture)"),

        heading2("14.1 ScriptableObject \uB370\uC774\uD130\uBCA0\uC774\uC2A4"),
        makeTable(
          ["\uB370\uC774\uD130\uBCA0\uC774\uC2A4", "\uACBD\uB85C", "\uC124\uBA85"],
          [
            ["CharacterStatsData", "Resources/CharacterStats", "\uCE90\uB9AD\uD130 \uAE30\uBCF8 \uC2A4\uD0EF \uBC0F \uC5C5\uADF8\uB808\uC774\uB4DC \uC124\uC815"],
            ["StageDatabase", "Resources/StageDatabase", "10\uAC1C \uC2A4\uD14C\uC774\uC9C0 \uC124\uC815 \uB370\uC774\uD130"],
            ["SkillDatabase", "Resources/SkillDatabase", "\uC2A4\uD0AC \uBAA9\uB85D \uBC0F \uC124\uC815"],
            ["EquipmentDatabase", "Resources/EquipmentDatabase", "\uC7A5\uBE44 \uBAA9\uB85D \uBC0F \uC124\uC815"],
            ["CharacterDatabase", "Resources/", "\uCE90\uB9AD\uD130 \uAE30\uBCF8 \uC815\uBCF4 (ID, \uC774\uB984, \uCD08\uC0C1\uD654)"],
            ["CutsceneData", "Resources/Story/", "\uCEF7\uC2E0 \uB300\uD654 \uB370\uC774\uD130"],
          ],
          [2400, 2600, 4026]
        ),

        heading2("14.2 \uB7F0\uD0C0\uC784 \uB370\uC774\uD130 \uD640\uB354 (Static Holders)"),
        makeTable(
          ["\uD640\uB354", "\uC5ED\uD560", "\uC800\uC7A5 \uB3D9\uAE30\uD654"],
          [
            ["CharacterUpgradeHolder", "\uC5C5\uADF8\uB808\uC774\uB4DC \uB808\uBCA8 \uBCF4\uAD00", "GameSaveData.statUpgradeLevels"],
            ["EquippedSkillsHolder", "\uC7A5\uCC29 \uC2A4\uD0AC \uBCF4\uAD00 (4\uC2AC\uB86F)", "GameSaveData.equippedSkillIds"],
            ["EquippedEquipmentHolder", "\uC7A5\uCC29 \uC7A5\uBE44 \uBCF4\uAD00 (2\uC2AC\uB86F)", "GameSaveData.equippedEquipmentIds"],
            ["BattleStageHolder", "\uD604\uC7AC \uC120\uD0DD \uC2A4\uD14C\uC774\uC9C0 \uC778\uB371\uC2A4", "\uC2A4\uD14C\uC774\uC9C0 \uC120\uD0DD \uC2DC \uC124\uC815"],
            ["BattlePhaseRuntime", "\uD398\uC774\uC988 1/2 \uC0C1\uD0DC \uCD94\uC801", "\uC804\uD22C \uC911 \uB7F0\uD0C0\uC784"],
            ["BattleClearStatsRuntime", "\uC804\uD22C \uD1B5\uACC4 \uB204\uC801", "\uC804\uD22C \uC911 \uB7F0\uD0C0\uC784"],
          ],
          [2800, 3200, 3026]
        ),

        // ──────────────────────────────────────────
        // 15. 씬 구조 및 매니저 설계
        // ──────────────────────────────────────────
        new Paragraph({ children: [new PageBreak()] }),
        heading1("15. \uC528 \uAD6C\uC870 \uBC0F \uB9E4\uB2C8\uC800 \uC124\uACC4 (Scene & Manager Design)"),

        heading2("15.1 \uC528 \uAD6C\uC131"),
        makeTable(
          ["\uC528 \uC774\uB984", "\uC5ED\uD560", "\uC8FC\uC694 \uCEF4\uD3EC\uB10C\uD2B8"],
          [
            ["MainMenu", "\uBA54\uC778 \uBA54\uB274", "MainMenuController, SaveSystem"],
            ["MapScene", "\uCC55\uD130/\uC2A4\uD14C\uC774\uC9C0 \uC120\uD0DD", "MapSceneController, StageSelectPanel"],
            ["BattleScene", "\uD37C\uC990 + \uC804\uD22C", "GameBoard, GameManager, UIManager"],
            ["StoryScene", "\uCEF7\uC2E0 \uC7AC\uC0DD", "StoryDialogueController (Additive \uB85C\uB4DC)"],
            ["KingdomScene", "\uC7A5\uBE44/\uC2A4\uD0AC/\uC2A4\uD14C\uC774\uD130\uC2A4", "KingdomSceneController"],
            ["StatusScene", "\uCE90\uB9AD\uD130 \uC0C1\uD0DC \uD655\uC778", "StatusSceneController"],
            ["TutorialScene", "\uD29C\uD1A0\uB9AC\uC5BC", "TutorialManager"],
          ],
          [2000, 3000, 4026]
        ),

        heading2("15.2 \uD575\uC2EC \uB9E4\uB2C8\uC800"),
        makeTable(
          ["\uB9E4\uB2C8\uC800", "\uD328\uD134", "\uC5ED\uD560"],
          [
            ["GameManager", "Singleton (DontDestroyOnLoad)", "\uAC8C\uC784 \uC0C1\uD0DC \uAD00\uB9AC, \uC528 \uC804\uD658 \uC870\uC728"],
            ["UIManager", "Scene-local", "UI \uC694\uC18C \uAD00\uB9AC \uBC0F \uC5C5\uB370\uC774\uD2B8"],
            ["LevelManager", "Scene-local", "\uD134 \uAD00\uB9AC, \uD134 \uC18C\uC9C4 \uCCB4\uD06C"],
            ["ScoreManager", "Scene-local", "\uC2A4\uCF54\uC5B4 \uACC4\uC0B0 \uBC0F \uCD94\uC801"],
            ["SaveSystem", "Static", "JSON \uC800\uC7A5/\uB85C\uB4DC"],
          ],
          [2800, 2800, 3426]
        ),

        heading2("15.3 \uC544\uD0A4\uD14D\uCC98 \uD328\uD134"),
        bullet("Singleton \uD328\uD134: GameManager (DontDestroyOnLoad, \uC528 \uC7AC\uC9C4\uC785 \uC2DC \uC7AC\uC0DD\uC131)"),
        bullet("Static Holder \uD328\uD134: \uB7F0\uD0C0\uC784 \uB370\uC774\uD130 \uBCF4\uAD00 (GameSaveData\uC640 \uB3D9\uAE30\uD654)"),
        bullet("ScriptableObject Database: \uAC8C\uC784 \uC124\uC815 \uB370\uC774\uD130 \uAD00\uB9AC"),
        bullet("Event/Callback \uD328\uD134: \uC0C1\uD0DC \uBCC0\uACBD \uC54C\uB9BC (OnStateEnter/Exit, OnTurnChanged \uB4F1)"),
        bullet("Coroutine \uAE30\uBC18 \uC2DC\uD000\uC2A4: Swap \u2192 Match \u2192 Clear \u2192 Gravity \u2192 Spawn \u2192 Check \uB8E8\uD504"),

        emptyLine(), emptyLine(),
        new Paragraph({
          alignment: AlignmentType.CENTER,
          border: { top: { style: BorderStyle.SINGLE, size: 4, color: "2E5090", space: 8 } },
          spacing: { before: 400 },
          children: [new TextRun({ text: "--- End of Document ---", font: "Malgun Gothic", size: 22, color: "999999", italics: true })]
        }),
      ]
    }
  ]
});

Packer.toBuffer(doc).then(buffer => {
  fs.writeFileSync("C:/UnityProject/3_Match_Puzzle_RPG/3_Match_Puzzle_RPG_System_Design.docx", buffer);
  console.log("Document created successfully!");
});
