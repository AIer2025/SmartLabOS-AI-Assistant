// SmartLabOS 详细设计方案 — WORD 生成脚本
// 数据源：1.html（主方案）、50ml高速冷冻离心模块-URS.html、Summary-输出总结.md
// 模板结构：references/_templates/02-SmartLabOS提案标准-详细设计方案版.md
const fs = require("fs");
const path = require("path");
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
  Header, Footer, AlignmentType, LevelFormat, TabStopType, TabStopPosition,
  TableOfContents, HeadingLevel, BorderStyle, WidthType, ShadingType,
  VerticalAlign, PageNumber, PageBreak, SectionType
} = require("docx");

const OUT = "C:\\TestClaude\\SmartLabOS-AI-Assistant\\projects\\端到端验证-20260626\\端到端验证-20260626_SmartLabOS_Presales_提案_20260626185640.docx";

// ---------- 调色 ----------
const BRAND = "0C5C8F";   // 主蓝
const BRAND2 = "0A9396";  // 青
const DANGER = "C1121F";  // 红（新建模块）
const HEADFILL = "DCE9F3";// 表头填充
const SUBFILL = "EEF4FA"; // 副表头
const NEWFILL = "FBECEB"; // 新建模块行
const CONTENT_W = 9360;   // US Letter 1" margins

// ---------- 通用构件 ----------
const FONT = "Microsoft YaHei";

function tnr(text, opts = {}) { return new TextRun({ text, font: FONT, ...opts }); }

function P(text, opts = {}) {
  const { runs, ...pOpts } = opts;
  return new Paragraph({
    children: runs || [tnr(text, opts.run || {})],
    spacing: { after: 120, line: 300, ...(opts.spacing || {}) },
    ...pOpts,
  });
}

function H1(text) {
  return new Paragraph({ heading: HeadingLevel.HEADING_1, children: [tnr(text, { bold: true })] });
}
function H2(text) {
  return new Paragraph({ heading: HeadingLevel.HEADING_2, children: [tnr(text, { bold: true })] });
}
function H3(text) {
  return new Paragraph({ heading: HeadingLevel.HEADING_3, children: [tnr(text, { bold: true })] });
}

function bullet(text, level = 0, runs = null) {
  return new Paragraph({
    numbering: { reference: "bullets", level },
    spacing: { after: 60, line: 290 },
    children: runs || [tnr(text)],
  });
}
function numItem(text, runs = null) {
  return new Paragraph({
    numbering: { reference: "numbers", level: 0 },
    spacing: { after: 60, line: 290 },
    children: runs || [tnr(text)],
  });
}

const cellBorder = { style: BorderStyle.SINGLE, size: 1, color: "BFD2E2" };
const cellBorders = { top: cellBorder, bottom: cellBorder, left: cellBorder, right: cellBorder };

function cell(content, { width, fill, bold, align, size } = {}) {
  const paras = Array.isArray(content) ? content : [content];
  return new TableCell({
    borders: cellBorders,
    width: width ? { size: width, type: WidthType.DXA } : undefined,
    shading: fill ? { fill, type: ShadingType.CLEAR } : undefined,
    margins: { top: 50, bottom: 50, left: 110, right: 110 },
    verticalAlign: VerticalAlign.CENTER,
    children: paras.map((c) =>
      typeof c === "string"
        ? new Paragraph({
            alignment: align || AlignmentType.LEFT,
            spacing: { after: 0, line: 270 },
            children: [tnr(c, { bold: !!bold, size: size || 19 })],
          })
        : c
    ),
  });
}

// rows: array of arrays. First row = header. widths: column widths.
function table(widths, rows, { headerFill = HEADFILL, firstColFill = null } = {}) {
  const trs = rows.map((cells, ri) =>
    new TableRow({
      tableHeader: ri === 0,
      children: cells.map((c, ci) => {
        const isHeader = ri === 0;
        const txt = (c && typeof c === "object" && "t" in c) ? c.t : c;
        const opt = (c && typeof c === "object" && "t" in c) ? c : {};
        return cell(txt, {
          width: widths[ci],
          fill: isHeader ? headerFill : (opt.fill || (firstColFill && ci === 0 ? firstColFill : undefined)),
          bold: isHeader || opt.bold,
          align: opt.align || (isHeader ? AlignmentType.CENTER : undefined),
          size: 19,
        });
      }),
    })
  );
  return new Table({
    width: { size: CONTENT_W, type: WidthType.DXA },
    columnWidths: widths,
    rows: trs,
  });
}

// 信息提示框（用单元格底纹模拟 note）
function noteBox(title, lines, fill = "EAF2FB", barColor = BRAND) {
  const kids = [];
  if (title) kids.push(new Paragraph({ spacing: { after: 60, line: 280 }, children: [tnr(title, { bold: true, color: barColor })] }));
  lines.forEach((ln) => kids.push(new Paragraph({ spacing: { after: 40, line: 280 }, children: Array.isArray(ln) ? ln : [tnr(ln, { size: 19 })] })));
  return new Table({
    width: { size: CONTENT_W, type: WidthType.DXA },
    columnWidths: [CONTENT_W],
    rows: [new TableRow({ children: [new TableCell({
      borders: { left: { style: BorderStyle.SINGLE, size: 18, color: barColor }, top: { style: BorderStyle.SINGLE, size: 1, color: fill }, bottom: { style: BorderStyle.SINGLE, size: 1, color: fill }, right: { style: BorderStyle.SINGLE, size: 1, color: fill } },
      shading: { fill, type: ShadingType.CLEAR },
      margins: { top: 90, bottom: 90, left: 160, right: 140 },
      children: kids,
    })] })],
  });
}

function spacer(h = 80) { return new Paragraph({ spacing: { after: h }, children: [tnr("")] }); }

// ============================================================
// 文档正文
// ============================================================
const body = [];

// ---------- 封面 ----------
body.push(new Paragraph({ spacing: { before: 1600, after: 200 }, alignment: AlignmentType.CENTER, children: [tnr("百泉聚兴（北京）科技有限公司", { bold: true, size: 30, color: BRAND })] }));
body.push(new Paragraph({ spacing: { after: 800 }, alignment: AlignmentType.CENTER, children: [tnr("SmartLabOS 智能实验室系统", { size: 24, color: BRAND2 })] }));
body.push(new Paragraph({ spacing: { before: 600, after: 160 }, alignment: AlignmentType.CENTER, children: [tnr("动物性食品中四环素类、磺胺类和喹诺酮类药物残留量测定", { bold: true, size: 30 })] }));
body.push(new Paragraph({ spacing: { after: 160 }, alignment: AlignmentType.CENTER, children: [tnr("自动化样品前处理系统", { bold: true, size: 30 })] }));
body.push(new Paragraph({ spacing: { before: 200, after: 1000 }, alignment: AlignmentType.CENTER, children: [tnr("详细设计方案", { bold: true, size: 40, color: BRAND })] }));
body.push(new Paragraph({ spacing: { after: 80 }, alignment: AlignmentType.CENTER, children: [tnr("项目名称：端到端验证-20260626", { size: 22 })] }));
body.push(new Paragraph({ spacing: { after: 80 }, alignment: AlignmentType.CENTER, children: [tnr("设计依据：GB 31658.17—2021", { size: 22 })] }));
body.push(new Paragraph({ spacing: { after: 80 }, alignment: AlignmentType.CENTER, children: [tnr("文档版本：V1.0（详细设计方案）", { size: 22 })] }));
body.push(new Paragraph({ spacing: { after: 80 }, alignment: AlignmentType.CENTER, children: [tnr("文档状态：售前技术提案", { size: 22 })] }));
body.push(new Paragraph({ spacing: { after: 80 }, alignment: AlignmentType.CENTER, children: [tnr("编制单位：百泉聚兴（北京）科技有限公司", { size: 22 })] }));
body.push(new Paragraph({ spacing: { after: 80 }, alignment: AlignmentType.CENTER, children: [tnr("编制日期：2026 年 6 月", { size: 22 })] }));
body.push(new Paragraph({ children: [new PageBreak()] }));

// ---------- 目录 ----------
body.push(new Paragraph({ spacing: { after: 200 }, alignment: AlignmentType.CENTER, children: [tnr("目  录", { bold: true, size: 28 })] }));
body.push(new TableOfContents("Table of Contents", { hyperlink: true, headingStyleRange: "1-3" }));
body.push(new Paragraph({ children: [new PageBreak()] }));

// ---------- 0 文档元信息 ----------
body.push(H1("文档元信息"));
body.push(table([2400, 6960], [
  ["项目", "内容"],
  ["客户/项目全称", "端到端验证-20260626 · 动物性食品兽药残留自动化前处理系统"],
  ["文档名称", "SmartLabOS 详细设计方案"],
  ["设计依据标准", "GB 31658.17—2021《食品安全国家标准 动物性食品中四环素类、磺胺类和喹诺酮类药物残留量的测定 液相色谱-串联质谱法》"],
  ["编制单位", "百泉聚兴（北京）科技有限公司"],
  ["版本 / 状态", "V1.0 / 售前技术提案（详细设计）"],
  ["编制日期", "2026-06-26"],
  ["联系人信息", "百泉聚兴销售与售前技术团队（价格、报价请联系销售团队获取）"],
]));
body.push(spacer());
body.push(H2("配套内部文档索引"));
body.push(table([3400, 5960], [
  ["文档", "说明"],
  ["1.html", "解决方案 HTML 提案（主方案，含工艺/平台/模组/工作站/耗时测算/公用工程）"],
  ["50ml高速冷冻离心模块-URS.html", "新建模块 URS（按生成指令 5.3，针对 §8.1 离心 −2 ℃/10000 r/min 能力缺口）"],
  ["Summary-输出总结.md", "输出总结（方案要点、对客户需求的回应、待确认事项）"],
  ["WORD生成指令-20260626-185640.txt", "本 WORD 提案生成指令文档"],
]));
body.push(noteBox("数据来源声明", [
  "本方案全部模组、平台、托盘、耗时参数均引用自 SmartLabOS 知识库 references/（01-modules、02-platforms、03-workstation、04-solutions、06-pallet）及本项目已生成的 1.html / URS 文件，并经模组卡逐一校验，未做杜撰；现有模组无法满足处按指令 5.3 输出新建模块 URS。",
], "EAF2FB", BRAND));
body.push(new Paragraph({ children: [new PageBreak()] }));

// ============================================================
// 1 项目概述
// ============================================================
body.push(H1("1  项目概述"));

body.push(H2("1.1 客户与业务背景"));
body.push(P("客户为食品安全检验检测实验室，承担动物性食品中兽药残留的检测任务。实验室目前以人工方式完成兽药残留前处理，存在效率低、重复性差的问题，计划引入 SmartLabOS 自动化样品制备系统，实现前处理全流程自动化，以提升通量与结果一致性。"));
body.push(P("本项目检测业务依据 GB 31658.17—2021，覆盖动物性食品（畜禽肉及脏器）中四环素类、磺胺类、喹诺酮类三大类共 41 种药物残留的液相色谱-串联质谱法（LC-MS/MS）测定，属典型的痕量、多目标物、多基质检测场景。"));

body.push(H2("1.2 检测对象与目标物"));
body.push(table([2600, 6760], [
  ["维度", "内容"],
  ["检验标准", "GB 31658.17—2021（2022-02-01 实施）"],
  ["适用基质", "牛、羊、猪、鸡的肌肉 / 肝脏 / 肾脏组织"],
  ["目标药物", "四环素类 4 种 + 磺胺类 21 种 + 喹诺酮类 16 种，共 41 种"],
  ["方法原理", "Mcllvaine-Na₂EDTA 缓冲液提取 → 亲水亲脂平衡型（HLB）固相萃取柱净化 → LC-MS/MS 外标法定量"],
  ["目标物理化特性", "痕量（µg/kg 级）、多组分共存、基质干扰强、部分组分热敏/易降解，对提取效率与净化一致性要求高"],
  ["检出 / 定量限", "检测限 2 µg/kg；定量限 10 µg/kg"],
  ["质控指标", "回收率 60%~110%；批内 RSD ≤ 15%、批间 RSD ≤ 20%"],
  ["关键耗材", "HLB 固相萃取柱 200 mg / 6 mL；微孔尼龙滤膜 0.22 µm"],
]));

body.push(H2("1.3 现状痛点深度分析"));
body.push(P("结合客户现状与 GB 31658.17—2021 前处理流程，逐环节剖析人工操作难点及其对结果有效性的影响机理："));
body.push(table([1900, 4060, 3400], [
  ["环节", "人工操作难点", "对回收率/精准度的影响机理"],
  ["提取（加液涡旋）", "缓冲液加注体积凭经验、涡旋时间不一致", "提取不充分或个体差异大，导致目标物回收率波动"],
  ["超声提取", "水浴温度与超声时长依赖人工计时", "提取效率不稳定，批内重复性差"],
  ["冷冻离心", "人工离心温度/转速难以稳定控制（标准要求 −2 ℃/10000 r/min）", "上清分离不彻底、基质残留，引入离子抑制"],
  [{ t: "SPE 净化（核心痛点）", bold: true }, { t: "活化/平衡/上样/淋洗/洗脱流速靠手动控制，柱易干、流速不均", fill: NEWFILL }, { t: "回收率波动大、批间一致性差 —— 即客户挑战-1", fill: NEWFILL }],
  ["氮吹浓缩", "吹干程度凭目视，易过吹损失或残留溶剂", "痕量组分损失、定容浓度偏差"],
  ["复溶定容", "复溶体积与溶解充分度人工差异", "定量偏差，影响 RSD"],
  ["过滤进样", "滤膜规格/过滤压力不统一", "进样液一致性差，污染色谱系统"],
]));
body.push(spacer());
body.push(noteBox("现状痛点量化与聚焦（对应客户挑战-1）", [
  [tnr("挑战-1：", { bold: true }), tnr("人工 SPE 净化流速不稳定，回收率波动大。", {})],
  "人工前处理对操作者熟练度依赖高，存在容错率低、个体差异大、重复性差、效率低等系统性问题；其中 SPE 净化环节的流速失控是回收率波动的主要来源，是本项目自动化改造需重点闭环的痛点。",
], "FFF6E5", "EE9B00"));

body.push(H2("1.4 项目场地概述"));
body.push(P("场地改造与详细尺寸需在「阶段一：调研细化与方案确认」中现场实测确认。基于设备外形与公用工程要求，本方案对场地提出以下基线要求（详见第 3.3 节与第 9 类公用工程）："));
body.push(bullet("设备外形高度 2055 mm，要求房间净层高 ≥ 3 m；"));
body.push(bullet("地面承重 ≥ 500 kg/㎡（基础 7 座工站合计约 1470 kg）；"));
body.push(bullet("基础 7 座线含通道占地约 18–21 ㎡（按平台净占地 2.5~3 倍预留通道）；达成 10 min 节拍的全线（约 13 座）占地按比例放大；"));
body.push(bullet("需具备排风/通风、给排水与危废收集、强弱电与网络、压缩空气与真空等配套条件（详见第 3.3、第 9 类）。"));
body.push(noteBox("○ 按需项裁剪说明", [
  "本节「改造前用途/楼层/房间、原始结构尺寸图、隔断打通范围」因售前阶段尚未取得现场实测图纸，暂以基线要求替代，相关图纸（原始结构尺寸图、改造范围图）将在阶段一现场勘测后补全。此为售前提案阶段的合理裁剪，不影响系统选型与技术指标的成立。",
], "FFF6E5", "EE9B00"));
body.push(new Paragraph({ children: [new PageBreak()] }));

// ============================================================
// 2 自动化需求与标准适配
// ============================================================
body.push(H1("2  自动化需求与标准适配"));

body.push(H2("2.1 设计依据标准"));
body.push(table([2400, 6960], [
  ["标准号", "标准全称（现行有效）"],
  ["GB 31658.17—2021", "《食品安全国家标准 动物性食品中四环素类、磺胺类和喹诺酮类药物残留量的测定 液相色谱-串联质谱法》（2022-02-01 实施）"],
  ["GB/T 6682", "《分析实验室用水规格和试验方法》（SPE 活化/淋洗、超声循环水及清洗用水水质依据）"],
]));
body.push(noteBox("标准要点（GB 31658.17—2021 §8.1 / §8.2）", [
  [tnr("§8.1 提取：", { bold: true }), tnr("称取试料 1 g → 加 Mcllvaine Na₂-EDTA 缓冲液 8 mL → 涡旋 1 min → 超声 20 min → −2 ℃ / 10000 r/min 离心 5 min → 收集上清；残渣加磷酸盐缓冲液 8 mL 重复提取 1 次 → 合并 2 次提取液。", {})],
  [tnr("§8.2 净化：", { bold: true }), tnr("HLB 柱甲醇 5 mL + 水 5 mL 活化 → 上样 → 水 5 mL + 20% 甲醇水 5 mL 淋洗 → 抽干 → 洗脱液 10 mL 洗脱 → 45 ℃ 氮吹干 → 复溶 1.0 mL 涡旋溶解 → 14000 r/min 离心 5 min → 0.22 µm 过滤。", {})],
], "EAF2FB", BRAND));

body.push(H2("2.2 检测标准与设备适配表"));
body.push(P("本项目为单标准前处理自动化，前处理系统输出进样小瓶后对接后端 LC-MS/MS 检测仪器："));
body.push(table([2200, 2400, 2660, 2100], [
  ["检测方法号", "检测项目", "前处理系统（本方案）", "检测仪器"],
  ["GB 31658.17—2021", "四环素类/磺胺类/喹诺酮类 41 种残留", "SmartLabOS 7 座工站自动化前处理产线（§8.1 提取 + §8.2 净化）", "LC-MS/MS"],
]));

body.push(H2("2.3 自动化需求统计与适配说明"));
body.push(table([3000, 6360], [
  ["需求维度", "目标 / 说明"],
  ["流程范围", "前处理（§8.1 提取、§8.2 净化），至 LC-MS/MS 进样小瓶为止"],
  ["上下料方式", "自动上下料（机械臂自动取放，上下料时间按自动方式 60 s/模组计，取自模组卡 module_up_unload_time）"],
  ["软件功能", "智慧实验室软件（SmartLabOS）：流程编排调度、参数配方管理、样品全程追溯、与 LIMS 对接"],
  ["节拍目标", "客户期望-1：单批次 10 分钟节拍；本方案稳态流水节拍 ≈ 9.7 min/样（详见第 6 章）"],
  ["拆分标准", "每平台处理时间尽量均衡、尽量接近 600 s（10 min），据此将传统三大工站细拆为 7 座单/小功能平台"],
]));
body.push(noteBox("差异化检项的系统适配", [
  "本项目为单一国标、单一前处理范式，无多检项分时调度需求；系统按 §8.1/§8.2 串联式产线设计。对于标准中超过 600 s 的硬工序（超声 20 min、浓缩 44 min），通过批处理（超声 16 工位）与并行扩容（浓缩 ×N）使每样/每批均摊压到 600 s 量级，由 SmartLabOS 统一调度（详见第 6 章）。",
], "EAF2FB", BRAND));
body.push(new Paragraph({ children: [new PageBreak()] }));

// ============================================================
// 3 总体设计
// ============================================================
body.push(H1("3  总体设计"));

body.push(H2("3.1 设计依据与原则"));
body.push(bullet("模块化：以标准离心管自动化岛平台（PLT-1200）为载体，单功能/小功能模组按需组合，便于扩容与维护；"));
body.push(bullet("智能物流联动：机械臂/转运实现工站间样品自动流转，全流程无人化衔接；"));
body.push(bullet("数字化管理：SmartLabOS 统一编排调度、配方参数版本管理、样品全程追溯并与 LIMS 对接；"));
body.push(bullet("可扩展：以「每平台处理时间尽量接近 600 s」为拆分标准，瓶颈工序按 N=⌈t/600⌉ 并行扩容，可分期实施。"));
body.push(spacer());
body.push(P("业务闭环范围界定（本方案覆盖灰底环节）：", { run: { bold: true } }));
body.push(table([CONTENT_W], [
  [{ t: "加缓冲液 → 涡旋 → 超声 → 冷冻离心 → 上清合并（残渣重复1次）→ SPE 活化/上样/淋洗/洗脱 → 45 ℃ 氮吹浓缩 → 复溶 → 高速离心 → 0.22 µm 过滤 → 进样小瓶 → 〔对接 LC-MS/MS〕", fill: "EAF2FB" }],
]));
body.push(P("注：称样（1 g 组织，感量 0.01 g 天平）SmartLabOS 模组库无称量模组，按行业惯例由人工称样后上样；自动化范围自「加缓冲液」起至「过滤进样」止。", { run: { size: 18, color: "6B7C93" } }));

body.push(H2("3.2 整体布局设计"));
body.push(P("按「每平台处理时间尽量均衡、尽量接近 600 s」标准，将前处理流程拆分为 7 座单/小功能 PLT-1200 工作站，按工艺顺序串联："));
body.push(table([900, 3000, 1500, 3960], [
  ["序", "工作站", "平台", "对应标准条款 / 输入→输出"],
  ["1", "WS-VET-AddVortex-01 加液涡旋", "PLT-1200", "§8.1 加缓冲液+涡旋｜1 g 组织 → 缓冲液样液（50 mL 离心管）"],
  ["2", "WS-VET-Ultrasonic-02 超声", "PLT-1200", "§8.1 超声 20 min｜样液 → 超声后样液（50 mL 离心管）"],
  ["3", "WS-VET-CryoCentri-03 冷冻离心+合并", "PLT-1200", "§8.1 离心+合并｜样液 → 合并提取液（50 mL 离心管）"],
  ["4", "WS-VET-SPE-04 SPE 净化", "PLT-1200", "§8.2 活化/上样/淋洗/洗脱｜提取液 → 洗脱液 10 mL（15 mL 西林瓶）"],
  ["5", "WS-VET-Concentrate-05 浓缩", "PLT-1200", "§8.2 45 ℃ 氮吹干｜洗脱液 → 浓缩残渣（15 mL 西林瓶）"],
  ["6", "WS-VET-ReconCentri-06 复溶+高速离心", "PLT-1200", "§8.2 复溶/离心｜残渣 → 复溶液（2 mL 离心管）"],
  ["7", "WS-VET-Filter-07 过滤进样", "PLT-1200", "§8.2 0.22 µm 过滤｜复溶液 → 进样液（2 mL 进样瓶）"],
]));
body.push(spacer());
body.push(noteBox("布局合理性论证", [
  "① 按工艺顺序线性排布，机械臂/转运在相邻工站间流转，流程合理、操作便捷；② 统一采用 PLT-1200（占地 1.02 ㎡/座，6 模组+8 托盘），空间利用率高且为公司主力验证机型；③ 各平台模组位需求 1~4 位，留有扩展余量用于瓶颈工序并行扩容；④ 平台标准化预装，现场拼接，缩短建设周期。",
], "EAF2FB", BRAND));
body.push(noteBox("○ 布局图占位说明（按需，待补）", [
  "「智能化实验室规划布局图」「平台俯视布局图」在 1.html 中以 SVG 示意图呈现；本 WORD 售前版以工作站串联表与平台俯视描述替代实图，正式详设阶段将补充 CAD 布局图（见第 8 章交付物）。",
], "FFF6E5", "EE9B00"));

body.push(H2("3.3 配套基础设施改造设计"));
body.push(P("基于各工站 electrical_environment 及工艺溶剂特性，配套改造设计要点如下（详细参数见第 9 类公用工程，施工图于阶段一/三补全）："));
body.push(table([2400, 6960], [
  ["系统", "改造设计要点"],
  ["管道排风系统", "提取涉乙腈，净化涉甲醇/乙酸乙酯/浓氨水/甲酸 + 45 ℃ 氮吹挥发；各工站上方设万向抽气罩/防爆排风，罩口面风速 ≥ 0.5 m/s；冷冻离心压缩机散热需通风；排风经活性炭/喷淋处理达标后高空排放。"],
  ["气路分布系统", "洁净干燥压缩空气 0.4–0.7 MPa（正压加液、SPE、针式过滤、转移模组）；真空泵供浓缩（−95 kPa）与 2 mL 高速离心配套；压缩空气需除水除油。"],
  ["强弱电与网络系统", "主电 220 V 单相 + 控制电 24 V；浓缩模组（MOD-NS-005）另需 48 V；独立配电箱 + 漏电/接地保护、可靠接地；建议在线式 UPS；网络接入 SmartLabOS 调度与 LIMS。"],
  ["给排水/纯水管路", "上水：一级纯水（GB/T 6682），供 SPE 活化/淋洗、超声循环水及清洗；下水：平台排废口接独立排废管路，废液不得直排市政下水；有机废液与氨性废液分类收集至危废中转罐。"],
]));
body.push(new Paragraph({ children: [new PageBreak()] }));

// ============================================================
// 4 功能平台逐一详细设计
// ============================================================
body.push(H1("4  功能平台逐一详细设计"));
body.push(P("对每一个已配置平台，逐一展开【平台组成 + 平台功能说明 + 关键技术指标表】三要素。各模组参数全部引用自 references/01-modules，关键技术指标表含具体数值、单位、精度与通讯协议。", { run: { size: 19 } }));
body.push(spacer());

// 模组库总表（4.0）
body.push(H2("4.0 选用模组总览（11 型，含 1 新建）"));
body.push(table([1900, 1600, 3300, 1660, 900], [
  ["工艺功能", "模组 ID", "模组名称 / 关键参数", "单循环耗时", "模块位"],
  ["加液涡旋", "MOD-WX-006", "50ml 离心管正压加液涡旋模块；1–50 mL，1–3000 rpm，±1%，含拆/合盖", "300 s", "1"],
  ["超声", "MOD-CS-004", "50ml 离心管多通道超声模块；25–60 ℃，十六工位批处理（标准取 1200 s）", "300 s", "0"],
  [{ t: "冷冻离心", fill: NEWFILL }, { t: "MOD-LX-006", fill: NEWFILL, bold: true }, { t: "50ml 高速冷冻离心模块（新建·URS）；−5~+10 ℃ / ≤10000 rpm，50ml×4", fill: NEWFILL }, { t: "300 s", fill: NEWFILL }, { t: "3", fill: NEWFILL }],
  ["转移", "MOD-YY-004", "50ml 全量液体转移模块；0–50 mL，拆盖·倒液·装盖", "40 s", "1"],
  ["上样移取", "MOD-YY-003", "10ml 移液模块；1–10 mL，±1%", "275 s", "1"],
  ["SPE", "MOD-SPE-004", "6cc 自动上样 SPE 模块；6 cc HLB 柱，2 路并行，流速 ±1% 闭环", "360 s", "1"],
  ["浓缩", "MOD-NS-005", "15ml 西林瓶离心浓缩模块；1–10 mL，25–55 ℃，−95 kPa，48 V", "2640 s", "1"],
  ["复溶加液", "MOD-YY-002", "1ml 移液模块；0.5–1 mL，±1%", "270 s", "1"],
  ["高速离心", "MOD-LX-004", "2ml-6 孔位高速离心模块 V4.0；0.5–2 mL，1000–14000 rpm，1–10 ℃", "420 s", "3"],
  ["过滤", "MOD-GL-002", "5cc 针式过滤模块；1–5 mL，13 mm 滤器", "520 s", "1"],
  ["多路供液", "MOD-JY-012", "8 通道正压加液模块；8 通道，1–4 bar，±1%，平台外置", "10 s", "0"],
]));
body.push(P("注：MOD-CS-004（十六工位、托盘位安装）与 MOD-JY-012（平台外置共享供液）经模组卡校验 module_slots=0，不占标准模组位。上下料时间统一取 60 s/模组（自动上下料）。", { run: { size: 18, color: "6B7C93" } }));
body.push(spacer());

// ---- 平台四要素生成器 ----
function platform(title, sub, compRows, funcLines, idxRows, flowText) {
  body.push(H2(title));
  body.push(P(sub, { run: { size: 19, color: BRAND, bold: true } }));
  body.push(P("【平台组成】", { run: { bold: true, color: BRAND2 } }));
  body.push(table([2200, 4860, 1100, 1200], [
    ["模组", "功能", "模块位", "处理(s)"],
    ...compRows,
  ]));
  body.push(P("【平台功能说明】", { run: { bold: true, color: BRAND2 } }));
  funcLines.forEach((l) => body.push(bullet(l)));
  body.push(P("【关键技术指标表】", { run: { bold: true, color: BRAND2 } }));
  body.push(table([2000, 5560, 1800], [
    ["设备 / 模组", "关键技术指标（数值 + 单位 + 精度）", "通讯协议"],
    ...idxRows,
  ]));
  if (flowText) body.push(noteBox("自动化逻辑 / 流转", [flowText], "EAF2FB", BRAND));
  body.push(spacer(120));
}

// WS1
platform(
  "4.1 提取·加液涡旋平台（WS-VET-AddVortex-01）",
  "PLT-1200 · 覆盖 §8.1（加缓冲液+涡旋）· 模块位 1/6 · 处理时间 430 s",
  [
    ["MOD-WX-006", "加 Mcllvaine 缓冲液 8 mL + 涡旋 1 min", "1", "360"],
    ["MOD-JY-012", "Mcllvaine/磷酸盐缓冲液多路供给", "0", "70"],
    [{ t: "合计", bold: true }, { t: "—", bold: true }, { t: "1/6", bold: true }, { t: "430", bold: true }],
  ],
  [
    "人工称样 1 g 上样后，由 MOD-WX-006 自动加注 8 mL Mcllvaine Na₂-EDTA 缓冲液并涡旋 1 min；",
    "适配 50 mL 离心管，含拆/合盖动作；MOD-JY-012 提供 Mcllvaine 缓冲液与磷酸盐缓冲液多路供给，支持残渣重复提取 1 次。",
  ],
  [
    ["MOD-WX-006 加液涡旋", "加液体积 1–50 mL；涡旋转速 1–3000 rpm；加液/吸排液重复精度 ±1%；含拆盖/合盖", "MODBUS TCP"],
    ["MOD-JY-012 八通道供液", "8 通道；供液压力 1–4 bar；计量精度 ±1%；平台外置共享单元", "MODBUS TCP"],
  ],
  "人工称样 1 g 上样 → WX-006 加缓冲液涡旋 → 转超声平台；残渣回流由 JY-012 补磷酸盐缓冲液重复一次。"
);

// WS2
platform(
  "4.2 提取·超声平台（WS-VET-Ultrasonic-02）",
  "PLT-1200 · 覆盖 §8.1（超声 20 min）· 模块位 0/6（十六工位批处理）· 处理时间 1260 s",
  [
    ["MOD-CS-004", "多通道超声 20 min（十六工位批处理）", "0", "1260"],
    [{ t: "合计", bold: true }, { t: "—", bold: true }, { t: "0/6", bold: true }, { t: "1260", bold: true }],
  ],
  [
    "超声 20 min 为标准硬性时长；MOD-CS-004 十六工位批处理，单管均摊 ≈ 79 s，不构成 600 s 节拍瓶颈；",
    "托盘位安装，0 模块位，配冷却循环水与排废。",
  ],
  [
    ["MOD-CS-004 多通道超声", "控温 25–60 ℃；十六工位（16 管/循环）并行批处理；超声时长按标准 20 min（1200 s）设定", "MODBUS TCP"],
  ],
  "超声 20 min 为标准硬性时长；16 工位批处理使单样均摊耗时极低，对稳态节拍贡献小。"
);

// WS3
platform(
  "4.3 提取·冷冻离心+合并平台（WS-VET-CryoCentri-03）",
  "PLT-1200 · 覆盖 §8.1（−2 ℃/10000 rpm 离心 + 上清合并）· 模块位 4/6 · 处理时间 460 s",
  [
    [{ t: "MOD-LX-006", bold: true, fill: NEWFILL }, { t: "−2 ℃/10000 rpm 冷冻离心 5 min（新建）", fill: NEWFILL }, { t: "3", fill: NEWFILL }, { t: "360", fill: NEWFILL }],
    ["MOD-YY-004", "上清全量转移合并", "1", "100"],
    [{ t: "合计", bold: true }, { t: "—", bold: true }, { t: "4/6", bold: true }, { t: "460", bold: true }],
  ],
  [
    "MOD-LX-006（新建模块）执行 −2 ℃ / 10000 r/min 离心 5 min，满足 §8.1 硬性离心条件；",
    "MOD-YY-004 取上清并合并 2 次提取液（含残渣磷酸盐缓冲液重复提取 1 次的合并）。",
  ],
  [
    [{ t: "MOD-LX-006 高速冷冻离心（新建）", bold: true }, { t: "控温 −5~+10 ℃，精度 ±1 ℃；转速 ≤10000 r/min，精度 ±50 r/min；50 mL×4 对称装载；离心 1–10 min 可设（本流程 5 min）；门联锁+振动报警", }, { t: "MODBUS TCP" }],
    ["MOD-YY-004 全量转移", "转移体积 0–50 mL；拆盖·倒液·装盖全自动", "MODBUS TCP"],
  ],
  "LX-006 −2 ℃/10000 rpm 离心 → YY-004 取上清并合并 2 次提取液 → 转 SPE 平台。MOD-LX-006 为新建模块，详见第 4.5 节 URS。"
);

// WS4
platform(
  "4.4 SPE 净化平台（WS-VET-SPE-04）— 应对挑战-1 核心平台",
  "PLT-1200 · 覆盖 §8.2（活化→上样→淋洗→洗脱）· 模块位 2/6 · 处理时间 825 s",
  [
    ["MOD-YY-003", "提取液上样移取", "1", "335"],
    ["MOD-SPE-004", "6cc HLB SPE（2 路并行，流速闭环）", "1", "420"],
    ["MOD-JY-012", "活化/淋洗/洗脱溶剂供给", "0", "70"],
    [{ t: "合计", bold: true }, { t: "—", bold: true }, { t: "2/6", bold: true }, { t: "825", bold: true }],
  ],
  [
    "MOD-SPE-004 对 HLB 柱活化·上样·淋洗·洗脱全程微升级流速闭环控制（精度 ±1%），规避人工流速不均与柱干失效，从根本上稳定回收率 —— 直接回应客户挑战-1；",
    "MOD-YY-003 完成提取液上样移取，洗脱液 10 mL 收入 15 mL 西林瓶转浓缩平台；MOD-JY-012 供给甲醇/水/20% 甲醇水等溶剂。",
  ],
  [
    [{ t: "MOD-SPE-004 自动上样 SPE", bold: true }, { t: "适配 6 cc HLB 柱（200 mg/6 mL）；2 路并行；活化/上样/淋洗/洗脱流速闭环控制，精度 ±1%；八步操作可编程", }, { t: "MODBUS TCP" }],
    ["MOD-YY-003 移液模块", "移液量程 1–10 mL；重复精度 ±1%；过程压力/液位安全检测", "MODBUS TCP"],
    ["MOD-JY-012 八通道供液", "8 通道；压力 1–4 bar；精度 ±1%", "MODBUS TCP"],
  ],
  "应对挑战-1：SPE-004 微升级流速闭环控制（±1%）稳定回收率；YY-003 上样，洗脱液 10 mL 收入 15 mL 西林瓶转浓缩平台。"
);

// WS5
platform(
  "4.5 浓缩平台（WS-VET-Concentrate-05）",
  "PLT-1200 · 覆盖 §8.2（45 ℃ 氮吹干）· 模块位 1/6 · 处理时间 2700 s（节拍瓶颈，需并行扩容）",
  [
    ["MOD-NS-005", "45 ℃ 离心浓缩至干（真空+加热+离心）", "1", "2700"],
    [{ t: "合计", bold: true }, { t: "—", bold: true }, { t: "1/6", bold: true }, { t: "2700", bold: true }],
  ],
  [
    "MOD-NS-005 以 45 ℃ + 真空 −95 kPa + 离心方式将洗脱液浓缩至干，覆盖标准 45 ℃ 氮吹干要求；",
    "单台浓缩循环 2640 s 为全线最长工序，是 10 min 节拍的瓶颈，通过并行 5 台 NS-005 使每批均摊 ≈ 540 s（详见第 6 章），亦可升级 PLT-1400 增配 1×8 托盘缓存。",
  ],
  [
    ["MOD-NS-005 离心浓缩", "处理体积 1–10 mL；控温 25–55 ℃（覆盖 45 ℃）；真空度 −95 kPa；驱动 48 V；适配 15 mL 西林瓶", "MODBUS TCP"],
  ],
  "单台浓缩循环 2640 s 为全线最长，是 10 min 节拍的唯一硬瓶颈；通过并行 5 台 NS-005 使每批均摊 ≈ 540 s（第 6 章）。"
);

// WS6
platform(
  "4.6 复溶·高速离心平台（WS-VET-ReconCentri-06）",
  "PLT-1200 · 覆盖 §8.2（复溶 1 mL + 14000 rpm 离心）· 模块位 4/6 · 处理时间 810 s",
  [
    ["MOD-YY-002", "复溶液 1.0 mL 加注/转移 + 涡旋溶解", "1", "330"],
    ["MOD-LX-004", "14000 r/min 离心 5 min", "3", "480"],
    [{ t: "合计", bold: true }, { t: "—", bold: true }, { t: "4/6", bold: true }, { t: "810", bold: true }],
  ],
  [
    "MOD-YY-002 加注 1.0 mL 复溶液并转入 2 mL 离心管溶解；",
    "MOD-LX-004 以 14000 r/min 离心 5 min，完全满足标准 §8.2 复溶离心条件后转过滤平台。",
  ],
  [
    ["MOD-YY-002 移液模块", "移液量程 0.5–1 mL；重复精度 ±1%", "MODBUS TCP"],
    ["MOD-LX-004 高速离心 V4.0", "处理体积 0.5–2 mL；转速 1000–14000 r/min（满足 14000 r/min）；控温 1–10 ℃；6 孔位", "MODBUS TCP"],
  ],
  "YY-002 加 1.0 mL 复溶液并转入 2 mL 管溶解 → LX-004 14000 rpm 离心 → 转过滤平台。"
);

// WS7
platform(
  "4.7 过滤·进样平台（WS-VET-Filter-07）",
  "PLT-1200 · 覆盖 §8.2（0.22 µm 过滤）· 模块位 1/6 · 处理时间 580 s",
  [
    ["MOD-GL-002", "0.22 µm 针式过滤至进样小瓶", "1", "580"],
    [{ t: "合计", bold: true }, { t: "—", bold: true }, { t: "1/6", bold: true }, { t: "580", bold: true }],
  ],
  [
    "MOD-GL-002 以 13 mm/0.22 µm 尼龙针式过滤器过滤复溶液至 2 mL 进样小瓶；",
    "输出进样液交付后端 LC-MS/MS 上机检测，处理时间 580 s（0.97× 600 s），与节拍标准吻合。",
  ],
  [
    ["MOD-GL-002 针式过滤", "处理体积 1–5 mL；适配 13 mm 滤器（0.22 µm 尼龙滤膜）", "MODBUS TCP"],
  ],
  "GL-002 针式过滤（13 mm/0.22 µm 尼龙）至 2 mL 进样小瓶 → 交付 LC-MS/MS。"
);

// 4.8 新建模块 URS
body.push(H2("4.8 新建模块 URS：50 mL 高速冷冻离心模块（MOD-LX-006）"));
body.push(noteBox("新建依据（指令 5.3）", [
  "GB 31658.17—2021 §8.1 规定提取离心条件为 −2 ℃、10000 r/min、5 min。检索 SmartLabOS 模组库 97 个模块，50 mL 规格离心仅有 MOD-LX-005（控温 1–10 ℃、最高转速 8000 r/min），既无法达到 −2 ℃ 的负温，也无法达到 10000 r/min；2 mL 高速离心模块 MOD-LX-004（14000 r/min）仅适配 2 mL 离心管。故 50 mL 负温高速离心为真实缺口，按指令 5.3 输出新建模块 URS。",
], "FBECEB", DANGER));
body.push(P("能力缺口比对：", { run: { bold: true } }));
body.push(table([2400, 2400, 2880, 1680], [
  ["条款", "标准要求", "现有 MOD-LX-005", "判定"],
  ["温度", "−2 ℃", "1 ~ 10 ℃", "❌ 不达标"],
  ["转速", "10000 r/min", "500 ~ 8000 r/min", "❌ 不达标"],
  ["容器", "50 mL 离心管", "50 mL 离心管 × 4", "✅ 一致"],
  ["时间", "离心 5 min", "cycle 420 s（含取放）", "△ 可达"],
]));
body.push(P("URS 功能与性能需求条目：", { run: { bold: true } }));
body.push(table([1500, 2200, 4060, 1600], [
  ["编号", "需求项", "规格 / 验收标准", "来源"],
  ["URS-01", "控温范围", "−5 ℃ ~ +10 ℃，覆盖标准 −2 ℃；控温精度 ±1 ℃", "GB 31658.17 §8.1"],
  ["URS-02", "最高转速", "≥ 10000 r/min，转速精度 ±50 r/min", "GB 31658.17 §8.1"],
  ["URS-03", "容器与装载", "50 mL 离心管 × 4，对称装载，带盖密封运行", "派生自 MOD-LX-005"],
  ["URS-04", "离心时间", "1 ~ 10 min 可设；本流程取 5 min", "GB 31658.17 §8.1"],
  ["URS-05", "自动上下料", "机械臂自动开/关门、取放管，上下料时间 ≤ 60 s", "项目「自动上下料」"],
  ["URS-06", "安全联锁", "门未关禁止启动；振动 > 300 µm 降速报警；运行中禁开门", "派生自 MOD-LX-005"],
  ["URS-07", "预冷", "支持运行前预冷至设定负温，缩短到温等待", "工艺需要"],
  ["URS-08", "通讯/集成", "MODBUS TCP，纳入 SmartLabOS 调度与追溯", "智慧实验室软件"],
]));
body.push(noteBox("处理时间与过渡方案", [
  [tnr("处理时间：", { bold: true }), tnr("按指令 5.3，新建模块处理时间 = 规范规定时间（离心 5 min = 300 s）+ 上下料 60 s = 360 s，已计入 WS-03 平台总耗时。", {})],
  [tnr("过渡方案：", { bold: true }), tnr("新模块到位前可暂用 MOD-LX-005 以 ≤4 ℃ / 8000 r/min 经方法学等效性验证后运行，作为临时措施；正式交付以满足标准 −2 ℃ / 10000 r/min 为准。", {})],
], "FFF6E5", "EE9B00"));

body.push(H2("4.9 配套托盘与辅助设备"));
body.push(table([3000, 1800, 4560], [
  ["托盘", "容量", "用途"],
  ["50 ml 离心管托盘", "18 个/盘", "供试样、提取、上样、合并接收"],
  ["6CC SPE 柱托盘", "24 个/盘", "HLB 固相萃取柱（MOD-SPE-004）"],
  ["15 ml 西林瓶托盘", "28 个/盘", "洗脱液接收 / 浓缩（MOD-NS-005）"],
  ["2 ml 离心管托盘", "60 个/盘", "复溶 / 14000 rpm 离心"],
  ["13 mm 过滤器托盘", "40 个/盘", "0.22 µm 针式过滤器（MOD-GL-002）"],
  ["2 ml 进样小瓶 / 枪头托盘", "60 / 128 / 60 个/盘", "进样小瓶、1ml·5ml·10ml 移液枪头"],
]));
body.push(noteBox("○ 辅助/可选设备（按需）", [
  "纯水制备系统（一级纯水 GB/T 6682）、在线式 UPS、增强型 SPE/特殊净化模块等为可选配套，型号/产水能力/容量及预算按现场需求与目标通量在阶段一确定；价格请联系销售团队获取报价。",
], "FFF6E5", "EE9B00"));
body.push(new Paragraph({ children: [new PageBreak()] }));

// ============================================================
// 5 软件系统详细设计
// ============================================================
body.push(H1("5  软件系统详细设计"));
body.push(P("本项目采用智慧实验室软件 SmartLabOS 统一编排 7 座工站调度、配方/参数版本管理、样品全程追溯，并与 LIMS 对接。", { run: { size: 19 } }));

body.push(H2("5.1 中央控制系统"));
body.push(P("技术架构：", { run: { bold: true } }));
body.push(bullet("采用分层架构（设备执行层 / 调度控制层 / 业务管理层 / 数据展示层），B/S 架构部署，支持集中-分散控制；"));
body.push(bullet("设备层通过 MODBUS TCP 等通用通讯协议接入各模组，统一纳管调度与追溯。"));
body.push(P("功能模块清单：", { run: { bold: true } }));
body.push(table([2600, 6760], [
  ["模块", "功能说明"],
  ["基础数据", "检测标准库、实验流程库、配方/参数版本管理"],
  ["样品管理", "样品全生命周期管理（采样前/中/后），申请-审批-确认-记录闭环"],
  ["仓库管理", "试剂耗材库存、物料调度、补料提醒"],
  ["实验流程管理", "§8.1/§8.2 流程编排，工序参数下发"],
  ["实验调度", "7 座工站调度控制、瓶颈并行台分配、节拍管理"],
  ["设备管理与自动化执行", "单机设备执行、设备台账与运维（保养提醒、故障报警、正反向追溯）"],
  ["数字化大屏", "运行状态、通量、异常的实时可视化展示"],
]));
body.push(P("自动化功能（逐项）：", { run: { bold: true } }));
body.push(bullet("实验室调度控制、单机设备执行、仓库管理与物料调度；"));
body.push(bullet("检测标准库与实验流程库（GB 31658.17—2021 §8.1/§8.2 工艺固化为可复用流程模板）；"));
body.push(bullet("样品全生命周期管理（采样前/中/后 + 申请-审批-确认-记录闭环）；"));
body.push(bullet("设备台账与运维（保养提醒、故障报警、正反向追溯）。"));
body.push(P("数据管理功能（逐项）：", { run: { bold: true } }));
body.push(bullet("实验数据抓取与传输、与 LIMS 双向对接；"));
body.push(bullet("通风/有害气体监测联动、多通道实时采集与校验、数据清洗；"));
body.push(bullet("数据审查与质控校验、实时存储与多维查询。"));
body.push(P("其他功能：", { run: { bold: true } }));
body.push(bullet("智能监测分析与预警；报表与统计（设备使用、检测次数、试剂用量、异常判定、原始记录/报告导出）；"));
body.push(bullet("报告定制与一键生成（模板定制、多级审核）；数据导出 Word/Excel/PDF；"));
body.push(bullet("角色与权限管理（分级、自定义角色）；报警管理（实时/历史、规则配置、多通道通知）；"));
body.push(bullet("数据安全（冗余容错、传输存储加密、权限隔离）。"));

body.push(H2("5.2 数字孪生 / 可视化系统（○按需）"));
body.push(noteBox("按需裁剪说明", [
  "本项目客户软件需求为「智慧实验室软件」，数字孪生/可视化系统为高端项目按需项。本方案以数字化大屏（中央控制可视化、物流与设备运行监控、试剂耗材管理）满足实时数据展示与监控、实验过程可视化、预警与应急响应等核心诉求；完整三维数字孪生（视频流多视角录制、成果展示参观交互）可作为后续升级选项，按预算在详设阶段确定。",
], "FFF6E5", "EE9B00"));

body.push(H2("5.3 软件系统技术指标"));
body.push(table([2600, 6760], [
  ["指标类别", "技术指标"],
  ["开放与集成", "提供 RESTful API；HTTPS 传输；支持 MODBUS TCP 等通用通讯协议；与 LIMS/工厂信息化双向对接（样品信息下发、结果回传）"],
  ["安全性", "容错运行、可扩充；完整操作日志；关键功能冗余切换"],
  ["可靠性", "成熟案例支撑；7×24 运行；支持集中-分散控制；年故障累计时间设上限（详设阶段以 SLA 约定）"],
  ["可扩展性", "支持硬件扩容（瓶颈并行台即插即用）；软件免定制配置适配新流程；升级简便"],
  ["日志记录", "记录操作人/时间/动作/参数等要素；保存时长可设；支持自动清理"],
]));
body.push(new Paragraph({ children: [new PageBreak()] }));

// ============================================================
// 6 自动化产能效率
// ============================================================
body.push(H1("6  自动化产能效率"));
body.push(P("测算口径：每平台处理时间 = Σ 该平台所载模组（功能码单循环耗时 + 上下料时间）。数值取自模组卡 process_capability.cycle_time_sec 与 module_up_unload_time（自动上下料 60 s/模组），与 1.html 提案计算保持一致。", { run: { size: 19 } }));

body.push(H2("6.1 逐平台处理时间（按 600 s 标准拆分）"));
body.push(table([2400, 3760, 1100, 1100, 1000], [
  ["平台 / 工作站", "所载模组（功能码耗时 + 上下料）", "模块位", "处理(s)", "≈ min"],
  ["WS-01 加液涡旋", "WX-006(300+60) + JY-012(10+60)", "1/6", "430", "7.2"],
  ["WS-02 超声 ※", "CS-004(1200+60)", "0/6", "1260", "21.0"],
  ["WS-03 冷冻离心+合并", "MOD-LX-006(300+60) + YY-004(40+60)", "4/6", "460", "7.7"],
  ["WS-04 SPE 净化", "YY-003(275+60) + SPE-004(360+60) + JY-012(10+60)", "2/6", "825", "13.8"],
  [{ t: "WS-05 浓缩 ★" }, "NS-005(2640+60)", "1/6", "2700", "45.0"],
  ["WS-06 复溶+高速离心", "YY-002(270+60) + LX-004(420+60)", "4/6", "810", "13.5"],
  ["WS-07 过滤", "GL-002(520+60)", "1/6", "580", "9.7"],
  [{ t: "总处理时间（串行单批叠加）", bold: true }, { t: "—", bold: true }, { t: "—", bold: true }, { t: "7065", bold: true }, { t: "≈117.8", bold: true }],
]));
body.push(P("※ 超声 20 min 为标准硬性时长（模组卡标称 300 s，按标准取 1200 s），十六工位批处理单样均摊 ≈ 79 s。★ 浓缩单台 2640 s 为全线最长工序，是 10 min 节拍的唯一硬瓶颈。", { run: { size: 18, color: "6B7C93" } }));

body.push(H2("6.2 趋近 600 s 的均衡性说明"));
body.push(noteBox("为何不能让每个平台都恰好 600 s", [
  "单个模组的功能码耗时不可再拆分：超声（标准 1200 s）、浓缩（2640 s）两道工序的单模组耗时本身已超 600 s。本方案拆分策略：① 将原「提取大工站」拆为加液涡旋(430)/超声(1260)/冷冻离心+合并(460) 三座，使可拆工序尽量落在 600 s 附近；② 对两道「单模组即超 600 s」的工序，用批处理（超声 16 工位）与并行扩容（浓缩 ×N）把每样/每批均摊压到 600 s 量级。除两道硬瓶颈外，其余 5 座平台处理时间落在 430~825 s，围绕 600 s 较为均衡。",
], "EAF2FB", BRAND));

body.push(H2("6.3 达成「单批次 10 分钟节拍」的并行配置（对应期望-1）"));
body.push(P("流水线稳态节拍 = 各平台「每样有效处理时间」的最大值。对超过 600 s 的平台按 N=⌈处理时间÷600⌉ 并行扩容：", { run: { size: 19 } }));
body.push(table([2400, 1900, 2400, 1660, 1000], [
  ["平台", "单元处理(s)", "并行/批处理", "每样有效(s)", "≈ min"],
  ["WS-01 加液涡旋", "430", "×1", "430", "7.2"],
  ["WS-02 超声", "1260", "16 工位批 ×1", "≈ 79", "1.3"],
  ["WS-03 冷冻离心+合并", "460", "×1（4 管/循环）", "460", "7.7"],
  ["WS-04 SPE 净化", "825", "×2（SPE 2 路并行）", "≈ 413", "6.9"],
  ["WS-05 浓缩", "2700", "×5", "≈ 540", "9.0"],
  ["WS-06 复溶+离心", "810", "×2", "≈ 405", "6.8"],
  ["WS-07 过滤", "580", "×1", "580", "9.7"],
  [{ t: "稳态流水节拍 = 最大每样有效时间（WS-07）", bold: true }, { t: "—", bold: true }, { t: "—", bold: true }, { t: "≈ 580", bold: true }, { t: "≈9.7 ✓", bold: true }],
]));
body.push(noteBox("效率结论与对比（vs 人工）", [
  [tnr("单批次（首样）串行总耗时 ≈ 117.8 min", { bold: true }), tnr("（7 平台叠加，含 −2 ℃ 冷冻离心新建模块 360 s）。", {})],
  [tnr("稳态流水节拍 ≈ 9.7 min/样", { bold: true }), tnr("（瓶颈平台并行扩容后，过滤平台 580 s 成为新节拍上限），满足客户「单批次 10 分钟节拍」期望-1。", {})],
  "达成 10 min 节拍的全线配置：基础 7 座 + 浓缩补 4 座 + SPE/复溶各补 1 座，合计约 13 座 PLT-1200；可按预算分期实施（先 7 座基础线，再按通量补瓶颈并行台）。",
  "相比人工：前处理由人工逐管操作转为全自动流水，人力需求大幅下降、SPE 流速闭环消除回收率波动来源、结果重复性显著提升（具体提升幅度以现场方法学验证数据为准）。",
], "E9F7EF", "1F9D6B"));
body.push(new Paragraph({ children: [new PageBreak()] }));

// ============================================================
// 7 项目实施与管理方案
// ============================================================
body.push(H1("7  项目实施与管理方案"));

body.push(H2("7.1 周期与组织"));
body.push(P("总建设周期：平台标准交付约 8 周、工站集成约 12 周；新建 MOD-LX-006 模块含 URS→详设→样机验证周期另议（引自平台/工站卡，供参考，最终以合同为准）。", { run: { size: 19 } }));
body.push(table([3000, 6360], [
  ["角色", "职责"],
  ["乙方项目经理", "总体计划、进度/资源协调、对甲方接口、变更管控"],
  ["乙方技术团队", "详细设计、模块生产与工厂预装调试、现场安装拼接与系统调试"],
  ["乙方质控团队", "全流程质控追溯、方法学验证、验收测试"],
  ["甲方对接小组", "需求确认、现场条件保障、配合验收与人员受训"],
]));

body.push(H2("7.2 实施阶段"));
body.push(table([2200, 1200, 2200, 1880, 1880], [
  ["阶段", "工期", "核心任务", "交付物", "验收标准 / 管控"],
  ["一、调研细化与方案确认", "现场实测", "现场勘测、尺寸/公用工程核定、方案与新建模块需求确认", "现场勘测报告、确认版方案、布局图", "甲乙双方签字确认；管控：需求冻结"],
  ["二、模块生产与工厂预装调试", "并行生产", "模组/平台生产、新建 MOD-LX-006 样机研制、工厂预装与联调", "工厂预装调试报告、设备合格证", "工厂出厂测试通过；管控：质控节点"],
  ["三、现场基础施工与复核", "—", "排风/气路/强弱电/给排水改造施工与复核", "施工记录、隐蔽工程复核单", "公用工程符合设计；管控：施工质量"],
  ["四、现场安装与模块拼接", "—", "平台进场、就位、拼接、接驳公用工程", "安装/拼接验收单", "拼接到位、接口正确；管控：安全"],
  ["五、系统调试与人员培训", "—", "联机调试、节拍验证、方法学验证、操作/维护培训", "调试报告、方法学验证报告、培训记录", "回收率 60%~110%、节拍达标；管控：质控"],
  ["六、项目验收与交付归档", "—", "整体验收、资料归档、移交", "验收报告、设备移交清单、全套文档", "验收通过、文档齐全；管控：归档"],
]));
body.push(P("总工期甘特图：各阶段时序与里程碑（工厂生产与现场施工可并行）将在阶段一形成正式甘特图（○按需，详设阶段补全实图）。", { run: { size: 18, color: "6B7C93" } }));

body.push(H2("7.3 项目管理措施"));
body.push(bullet("进度管理：三级管控（项目经理-技术负责人-工序负责人）、定期例会、设赶工预案；"));
body.push(bullet("质量管理：全流程质控追溯、设关键质控节点、过程记录归档；"));
body.push(bullet("沟通管理：固定沟通机制、专属项目群、定期例会与纪要；"));
body.push(bullet("安全管理：用电/动火/接地/应急停机规范，操作权限分级；"));
body.push(bullet("变更管理：书面申请 → 影响评估 → 签字确认，闭环管控。"));

body.push(H2("7.4 保障措施"));
body.push(bullet("资源保障：资深工程师 + 专用设备 + 原厂部件，甲方现场配合；"));
body.push(bullet("技术保障：攻坚小组、7×24 远程支持、现场响应时限（以 SLA 约定）；"));
body.push(bullet("售后前置：售后工程师参与调试，提前熟悉系统；"));
body.push(bullet("文档保障：全流程归档可追溯。"));
body.push(new Paragraph({ children: [new PageBreak()] }));

// ============================================================
// 8 交付物清单
// ============================================================
body.push(H1("8  交付物清单"));
body.push(table([3000, 6360], [
  ["类别", "交付内容"],
  ["正式技术文档", "详细设计方案、各类设计图（系统架构图、布局图、流程图）"],
  ["工程交付物", "布局 CAD 图、工厂/现场调试报告、设备合格证、安装/拼接验收单、验收报告、设备移交清单"],
  ["资料交付", "操作手册、维护手册、技术资料（纸质 + 电子）、售后服务协议"],
  ["新建模块资料", "MOD-LX-006 URS、详设规格书、样机验证报告（含 −2 ℃/10000 r/min 回收率稳定性验证）"],
]));
body.push(spacer());

// 评审快速判定 / 合规声明
body.push(H2("8.1 合规与数据来源声明"));
body.push(bullet("全部技术参数引用自 references/ 模组/平台/托盘库及本项目 1.html、URS，无编造数据；模组卡 module_slots 实测值已更正（MOD-CS-004=0、MOD-JY-012=0）；"));
body.push(bullet("工艺流程严格遵循 GB 31658.17—2021 §8.1/§8.2；唯一硬件缺口（−2 ℃/10000 r/min 50 mL 离心）已通过新建模块 MOD-LX-006 URS 闭环；"));
body.push(bullet("各功能平台均满足【平台组成 + 功能说明 + 关键技术指标表】三要素，指标含数值/单位/精度/通讯协议（MODBUS TCP）；"));
body.push(bullet("效率数据均绑定「样品数 × 基质 × 全流程耗时」，与 1.html 计算一致。"));
body.push(noteBox("商务与报价", [
  "本方案为售前技术配置建议。涉及价格、报价信息，请联系销售团队获取报价。交付周期与质保以最终合同为准。",
], "FBECEB", DANGER));
body.push(spacer());

body.push(H2("8.2 待客户确认事项"));
body.push(numItem("新建模块 MOD-LX-006 的交付与方法学验证安排（或先采用 MOD-LX-005 过渡方案）；"));
body.push(numItem("目标日通量 → 据此确定浓缩/SPE/复溶并行台数（基础 7 座 vs 全线约 13 座的分期方案）；"));
body.push(numItem("现场公用工程：220V/24V/48V 供电、通风排风、一级纯水、压缩空气 0.4–0.7 MPa、真空、危废分类收集；"));
body.push(numItem("场地：层高 ≥ 3 m、地面承重 ≥ 500 kg/㎡、含通道占地 ≈ 18–21 ㎡（基础 7 座线）。"));

// ============================================================
// 文档组装
// ============================================================
const doc = new Document({
  creator: "百泉聚兴（北京）科技有限公司 · SmartLabOS",
  title: "端到端验证-20260626 SmartLabOS 详细设计方案",
  styles: {
    default: { document: { run: { font: FONT, size: 21 } } },
    paragraphStyles: [
      { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 30, bold: true, font: FONT, color: BRAND },
        paragraph: { spacing: { before: 280, after: 160 }, outlineLevel: 0 } },
      { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 25, bold: true, font: FONT, color: "13456B" },
        paragraph: { spacing: { before: 200, after: 120 }, outlineLevel: 1 } },
      { id: "Heading3", name: "Heading 3", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 22, bold: true, font: FONT, color: BRAND2 },
        paragraph: { spacing: { before: 140, after: 80 }, outlineLevel: 2 } },
    ],
  },
  numbering: {
    config: [
      { reference: "bullets", levels: [{ level: 0, format: LevelFormat.BULLET, text: "•", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 560, hanging: 280 } } } }] },
      { reference: "numbers", levels: [{ level: 0, format: LevelFormat.DECIMAL, text: "%1.", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 560, hanging: 280 } } } }] },
    ],
  },
  sections: [{
    properties: {
      page: { size: { width: 12240, height: 15840 }, margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 } },
    },
    headers: {
      default: new Header({ children: [new Paragraph({
        alignment: AlignmentType.RIGHT,
        border: { bottom: { style: BorderStyle.SINGLE, size: 4, color: BRAND, space: 2 } },
        children: [tnr("SmartLabOS 详细设计方案 · 端到端验证-20260626", { size: 16, color: "6B7C93" })],
      })] }),
    },
    footers: {
      default: new Footer({ children: [new Paragraph({
        alignment: AlignmentType.CENTER,
        children: [tnr("百泉聚兴（北京）科技有限公司　|　第 ", { size: 16, color: "6B7C93" }),
          new TextRun({ children: [PageNumber.CURRENT], font: FONT, size: 16, color: "6B7C93" }),
          tnr(" 页 / 共 ", { size: 16, color: "6B7C93" }),
          new TextRun({ children: [PageNumber.TOTAL_PAGES], font: FONT, size: 16, color: "6B7C93" }),
          tnr(" 页", { size: 16, color: "6B7C93" })],
      })] }),
    },
    children: body,
  }],
});

Packer.toBuffer(doc).then((buf) => {
  fs.writeFileSync(OUT, buf);
  console.log("WROTE:", OUT, "(" + buf.length + " bytes)");
});
