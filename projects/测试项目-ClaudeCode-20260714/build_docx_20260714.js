/* SmartLabOS 售前 WORD 详细设计方案提案 生成脚本
 * 项目：测试项目（成都市食品检验研究院 · 兽残专项 · 磺胺类 GB/T 21316—2007）
 * 数据来源：projects/测试项目/1.html、微波萃取-模块-URS.html、references/01-modules、02-platforms
 * 全部技术参数取自知识库模块/平台卡片与已生成 HTML，未编造。
 */
const fs = require("fs");
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
  Header, Footer, AlignmentType, LevelFormat, TableOfContents, HeadingLevel,
  BorderStyle, WidthType, ShadingType, VerticalAlign, PageNumber, PageBreak,
} = require("docx");

// ---------- 常量 ----------
const FONT = "Microsoft YaHei";
const BRAND = "0C5C8F";
const BRAND2 = "0A7A72";
const INK = "1F2A38";
const LIGHT = "EEF4FA";
const CW = 9740; // A4 内容宽度（margin 1080 两侧）

const B = { style: BorderStyle.SINGLE, size: 4, color: "BFC9D4" };
const BORDERS = { top: B, bottom: B, left: B, right: B,
  insideHorizontal: B, insideVertical: B };
const CELLM = { top: 60, bottom: 60, left: 110, right: 110 };

// ---------- 帮助函数 ----------
function t(text, opts = {}) {
  return new TextRun({ text: String(text), bold: !!opts.bold, italics: !!opts.italics,
    color: opts.color || INK, size: opts.size || 21, font: FONT });
}
function p(text, opts = {}) {
  const children = Array.isArray(text) ? text : [t(text, opts)];
  return new Paragraph({ children, alignment: opts.align,
    spacing: { before: opts.before ?? 20, after: opts.after ?? 80, line: opts.line ?? 276 },
    indent: opts.indent, pageBreakBefore: opts.pageBreakBefore });
}
function h1(text) {
  return new Paragraph({ heading: HeadingLevel.HEADING_1, pageBreakBefore: true,
    children: [t(text, { bold: true, size: 30, color: BRAND })] });
}
function h2(text) {
  return new Paragraph({ heading: HeadingLevel.HEADING_2,
    children: [t(text, { bold: true, size: 25, color: INK })] });
}
function h3(text) {
  return new Paragraph({ heading: HeadingLevel.HEADING_3,
    children: [t(text, { bold: true, size: 22, color: BRAND2 })] });
}
function bullet(text, opts = {}) {
  const children = Array.isArray(text) ? text : [t(text, opts)];
  return new Paragraph({ children, numbering: { reference: "bul", level: 0 },
    spacing: { before: 10, after: 40, line: 270 } });
}
function numItem(text, opts = {}) {
  const children = Array.isArray(text) ? text : [t(text, opts)];
  return new Paragraph({ children, numbering: { reference: opts.ref || "num", level: 0 },
    spacing: { before: 10, after: 40, line: 270 } });
}
function cellP(content, opts = {}) {
  if (Array.isArray(content) && content[0] instanceof Paragraph) return content;
  const runs = Array.isArray(content) ? content
    : [t(content, { bold: opts.bold, color: opts.color, size: opts.size || 19 })];
  return [new Paragraph({ children: runs, alignment: opts.align,
    spacing: { before: 10, after: 10, line: 250 } })];
}
function tc(content, w, opts = {}) {
  return new TableCell({
    width: { size: w, type: WidthType.DXA },
    borders: BORDERS, margins: CELLM, verticalAlign: VerticalAlign.CENTER,
    shading: opts.fill ? { fill: opts.fill, type: ShadingType.CLEAR, color: "auto" } : undefined,
    columnSpan: opts.span,
    children: cellP(content, opts),
  });
}
function headerRow(labels, widths, fill = BRAND) {
  return new TableRow({ tableHeader: true, children: labels.map((l, i) =>
    tc(l, widths[i], { bold: true, color: "FFFFFF", fill, size: 19, align: AlignmentType.CENTER })) });
}
function table(widths, headerLabels, rows, opts = {}) {
  const trs = [];
  if (headerLabels) trs.push(headerRow(headerLabels, widths, opts.headerFill));
  rows.forEach((r, ri) => {
    trs.push(new TableRow({ children: r.map((c, i) => {
      const co = (c && typeof c === "object" && !Array.isArray(c) && c.text !== undefined) ? c : { text: c };
      return tc(co.content || co.text, widths[i], {
        bold: co.bold, color: co.color, align: co.align, span: co.span, size: co.size || 19,
        fill: co.fill || (opts.zebra && ri % 2 ? "F5F8FC" : undefined),
      });
    }) }));
  });
  return new Table({ width: { size: widths.reduce((a, b) => a + b, 0), type: WidthType.DXA },
    columnWidths: widths, rows: trs, borders: BORDERS });
}
function spacer(h = 60) { return new Paragraph({ spacing: { after: h }, children: [] }); }
function note(label, text, color = BRAND) {
  return new Paragraph({
    border: { left: { style: BorderStyle.SINGLE, size: 18, color, space: 8 } },
    shading: { fill: "F5F8FC", type: ShadingType.CLEAR, color: "auto" },
    spacing: { before: 60, after: 100, line: 270 },
    indent: { left: 120 },
    children: [t(label + "  ", { bold: true, color }), ...(Array.isArray(text) ? text : [t(text)])] });
}

// 通讯口径统一说明（源自各平台机械臂 + URS FR-07：接入 SmartLabOS 智慧实验室软件标准总线）
const BUS = "工业以太网 / RS-485（Modbus）接入 SmartLabOS 智慧实验室软件标准总线";

// ---------- 模块主数据（全部取自 references/01-modules 卡片）----------
const MODULES = {
  JZ: { id: "MOD-JZ-002", pn: "HHM06B-050", name: "50ml离心管均质模块", cat: "均质", slots: 1,
    dim: "450×180×600", kg: 25, ind: [
      ["处理量", "1–40 mL"], ["均质转速", "1000–30000 rpm"], ["气动压力", "1–4 bar"],
      ["功能码 / 单循环", "115 均质 / 60 s"], ["供电", "DC 24 V（0.28 kVA 额定 / 0.47 kVA 峰值）"],
      ["公用工程", "压缩空气 + 上水 + 下水（刀头清洗）"], ["噪声", "87 dB"], ["通讯", BUS]] },
  JY: { id: "MOD-JY-010", pn: "HHM02M-050", name: "50ml离心管拆装盖加液模块", cat: "加液", slots: 1,
    dim: "450×180×600", kg: 20, ind: [
      ["加液量程", "1–50 mL"], ["加液绝对精度", "±1%"], ["加液重复精度", "±0.5%"],
      ["运动 / 定位", "Y 100 mm、Z 50 mm，速度 50 mm/s，定位 ±0.1 mm"],
      ["功能码 / 单循环", "169 拆盖-加液-装盖 / 106 s（含子功能 165/166/167）"],
      ["供电", "DC 24 V（0.23 / 0.38 kVA）"], ["公用工程", "压缩空气"], ["通讯", BUS]] },
  WX: { id: "MOD-WX-004", pn: "HHM03K-050", name: "50ml离心管单涡旋模块", cat: "涡旋", slots: 1,
    dim: "450×180×600", kg: 15, ind: [
      ["涡旋转速", "1000–3000 rpm"], ["振幅", "±2 mm"], ["单次连续时长", "≤ 10 min（单管液量 ≤ 45 mL）"],
      ["功能码 / 单循环", "223 涡旋 / 220 s"], ["气动压力", "1–4 bar"],
      ["供电", "DC 24 V（0.23 / 0.38 kVA）"], ["公用工程", "压缩空气"], ["通讯", BUS]] },
  LX: { id: "MOD-LX-001", pn: "HHM15C-050", name: "50ml×4孔位高速离心机模块（带XYZ）", cat: "离心", slots: 3,
    dim: "800×850×2050", kg: 201, ind: [
      ["孔位", "50 mL × 4 孔位"], ["转速", "1–8000 rpm"], ["控温", "2–7 ℃"], ["真空", "−80 kPa"],
      ["功能码 / 循环", "188 离心 / 0–300 s（本标准取 300 s = 5 min）"],
      ["子功能", "185 定位 / 186 预冷 / 187 取盖 / 189 除水 / 190 停止"],
      ["供电 / 发热", "DC 24 V 控制（3.5 / 4.8 kVA）· 发热需通风"],
      ["配套", "真空泵 + 压缩机"], ["通讯", BUS]] },
  YY: { id: "MOD-YY-004", pn: "HHM09F-050", name: "50ml全量液体转移模块", cat: "移液", slots: 1,
    dim: "450×180×600", kg: 20, ind: [
      ["转移量程", "0–50 mL（全量转移）"], ["动作", "三轴联动 + 旋转倒液 + 气动夹紧 + 电动夹爪 + 闭环检测"],
      ["功能码 / 单循环", "155 拆盖-倒液-装盖 / 40 s"], ["气动压力", "1–4 bar"],
      ["供电", "DC 24 V（0.31 / 0.52 kVA）"], ["公用工程", "压缩空气"], ["通讯", BUS]] },
  NS: { id: "MOD-NS-004", pn: "HHM05J-050", name: "50ml西林瓶连续离心浓缩模块", cat: "浓缩", slots: 2,
    dim: "450×380×600", kg: 45, ind: [
      ["处理量", "1–30 mL"], ["温度范围", "25–55 ℃（离心+加热+真空三效蒸干）"], ["真空", "−95 kPa"],
      ["离心转速", "1000–8000 rpm"], ["浓缩/定容绝对精度", "±1%"], ["重复精度", "±0.5%"],
      ["功能码 / 单循环", "200 浓缩 / 1800 s（含 201 复溶、205 浓缩+复溶）"],
      ["供电 / 发热", "DC 48 V（0.56 / 1.0 kVA）· 发热需通风"],
      ["公用工程", "真空泵 + 压缩空气 + 上水 + 下水"], ["通讯", BUS]] },
  CS: { id: "MOD-CS-005", pn: "HHM14C-050", name: "50ml离心管超声模块", cat: "超声", slots: 1,
    dim: "450×180×600", kg: 20, ind: [
      ["工位", "四工位并行"], ["控温", "0–60 ℃（水槽 25±5 ℃ 闭环）"],
      ["功能码 / 本步耗时", "726 启动超声 / 动态可编程（本标准 §7.2 取 30 s 溶解残渣）"],
      ["供电", "AC 220 V（0.22 / 0.37 kVA）"], ["公用工程", "冷却循环水（上水）"], ["通讯", BUS]] },
  GL: { id: "MOD-GL-002", pn: "HHM04J-005", name: "5cc 针式过滤模块", cat: "过滤", slots: 1,
    dim: "450×180×600", kg: 20, ind: [
      ["过滤样品量", "1–5 mL"], ["滤器规格", "13 mm 针式过滤器（适配 0.22 µm 微孔滤膜）"],
      ["功能", "吸液-预过滤-过滤-洗针（3 mL 清洗剂+3 mL 纯水×3）-空吹全流程"],
      ["功能码 / 单循环", "310 启动针式过滤 / 520 s"],
      ["供电", "DC 24 V（0.13 / 0.22 kVA）"], ["公用工程", "压缩空气 + 下水（排废）"], ["通讯", BUS]] },
  WB: { id: "MOD-WB-001（拟建）", pn: "待定", name: "50ml离心管微波辅助萃取模块", cat: "微波（缺口·URS）", slots: 1,
    dim: "450×180×600", kg: "≤30（目标）", ind: [
      ["微波功率", "700 W 额定（300–800 W 可调）"], ["处理液量", "1–50 mL（典型 25 mL 乙腈-水）"],
      ["超温保护", "≤ 60 ℃（红外/光纤测温闭环，超温降功率/停机）"],
      ["功能码 / 处理时间", "启动微波辅助萃取 / 30 s×2 轮 = 60 s（按标准 §7.1）"],
      ["供电", "DC 24 V 控制 + AC 220 V 微波电源"],
      ["安全", "微波泄漏 ≤ 5 mW/cm²@5cm · 门/工位联锁 · 防爆排风"],
      ["公用工程", "排风 + 压缩空气"], ["通讯", BUS]] },
};

// 工作站定义
const WS = [
  { id: "WS-SA-Homog-CDFI-01", name: "制样·提取工作站", plt: "PLT-800", clause: "§7.1 匀浆·加液·涡旋",
    slots: "3 / 3", cyc: 566,
    mods: [["MOD-JZ-002", "C₁₈ 填料匀浆分散", 1], ["MOD-JY-010", "加乙腈-水 25 mL（拆/装盖）", 1],
      ["MOD-WX-004", "涡旋 1 min", 1]],
    keys: ["JZ", "JY", "WX"],
    fn: ["自动上料 50 mL 离心管（内含 2 g 试样 + C₁₈ 填料，人工上样前置）；",
      "MOD-JZ-002 高剪切均质（1000–30000 rpm）对 C₁₈ 填料匀浆分散，应对高脂高蛋白基质分层、痕量取样偏差（挑战-2）；",
      "MOD-JY-010 拆盖后加乙腈-水 25 mL（±1%）再装盖；",
      "MOD-WX-004 涡旋 1 min 使样品充分混合，规避乳化（挑战-3）；随后自动流转至 WS2。"],
    time: [["MOD-JZ-002", "C₁₈ 填料匀浆", 60, 60, 120], ["MOD-JY-010", "加乙腈-水 25 mL", 106, 60, 166],
      ["MOD-WX-004", "涡旋 1 min", 220, 60, 280]] },
  { id: "WS-SA-Extract-CDFI-02", name: "微波·离心分离工作站", plt: "PLT-1200", clause: "§7.1 微波·离心·取层",
    slots: "5 / 6", cyc: 580,
    mods: [["MOD-WB-001（拟建）", "微波光波辐照 30 s ×2 轮", 1], ["MOD-LX-001", "3000 r/min 离心 5 min", 3],
      ["MOD-YY-004", "取乙腈上清层 / 合并", 1]],
    keys: ["WB", "LX", "YY"],
    fn: ["MOD-WB-001（拟建微波萃取模块）按标准 §7.1 700 W 光波辐照 30 s，强化乙腈-水体系传质提取；",
      "MOD-LX-001 高速离心机（3000 r/min，5 min = 300 s）实现固液分离，转速 ≤8000 rpm 完全满足；",
      "MOD-YY-004 全量转移取乙腈上清层；沉淀残渣加乙腈重复微波辅助提取 1 次后合并提取液（共 2 轮）；",
      "本站含离心机（占 3 位），选用 6 模块位 PLT-1200，余 1 位备缓存/扩展。"],
    time: [["MOD-WB-001（拟建）", "微波辐照 30 s ×2 轮", 60, 60, 120], ["MOD-LX-001", "3000 r/min 离心 5 min", 300, 60, 360],
      ["MOD-YY-004", "取乙腈层 / 合并", 40, 60, 100]] },
  { id: "WS-SA-Cleanup-CDFI-03", name: "液液分配（LLE）净化工作站", plt: "PLT-800", clause: "§7.2 加正己烷·振摇·取底层",
    slots: "3 / 3", cyc: 546,
    mods: [["MOD-JY-010 #2", "加乙腈饱和正己烷 25 mL / 正丙醇 10 mL", 1], ["MOD-WX-004 #2", "振摇 5 min（LLE）", 1],
      ["MOD-YY-004 #2", "取底层乙腈 / 弃正己烷层", 1]],
    keys: ["JY", "WX", "YY"],
    fn: ["本标准 §7.2 净化采用「乙腈饱和正己烷液液分配（LLE）」而非 SPE，规避 SPE 柱活化/平衡/流速失控、柱干堵塞（挑战-4）；",
      "MOD-JY-010（第 2 套）加乙腈饱和正己烷 25 mL；MOD-WX-004（第 2 套）振摇 5 min 完成液液分配；",
      "回 WS2 的 MOD-LX-001 离心分层（再入 1 次），MOD-YY-004 取底层乙腈溶液、弃正己烷层；随后流转至 WS4。"],
    time: [["MOD-JY-010 #2", "加乙腈饱和正己烷 25 mL", 106, 60, 166], ["MOD-WX-004 #2", "振摇 5 min（LLE）", 220, 60, 280],
      ["MOD-YY-004 #2", "取底层乙腈 / 弃正己烷", 40, 60, 100]] },
  { id: "WS-SA-Concentrate-CDFI-04", name: "浓缩·复溶工作站（3 工位并行）", plt: "PLT-1400", clause: "§7.2 45 ℃ 浓缩近干 + 复溶 1 mL",
    slots: "6 / 6（发热 3/3）", cyc: 620,
    mods: [["MOD-NS-004 ×3（并行）", "45 ℃ 真空离心浓缩近干 + 复溶 1 mL（子功能 205）", "2×3=6"]],
    keys: ["NS"],
    fn: ["MOD-NS-004 以「离心（扩面积）+ 加热（25–55 ℃）+ 真空（−95 kPa）」三效合一，低温离心浓缩等效替代标准 45 ℃ 旋蒸近干 + 氮吹干，解决氮吹控温控速难、目标物易挥发、定容精度不足（挑战-5）；",
      "内置复溶功能加乙腈-水 1 mL，以 ±1% 精度精密定容；",
      "单台浓缩约 1800 s 为全线瓶颈，以 3 台并行（占满 PLT-1400 的 6 模块位与 3 发热上限）使节拍 = 1800÷3 = 600 s，与其余平台对齐；",
      "PLT-1400 的 1×8 线性托盘利于连续浓缩缓存。"],
    time: [["MOD-NS-004 ×3 并行", "45 ℃ 真空离心浓缩近干 + 复溶 1 mL", "1800（单台）→ 600（3 台并行节拍）", 60, 620]] },
  { id: "WS-SA-Filter-CDFI-05", name: "超声·过滤上机工作站", plt: "PLT-800", clause: "§7.2 超声溶解 + 0.22 µm 过滤",
    slots: "2 / 3", cyc: 670,
    mods: [["MOD-CS-005", "超声 30 s 溶解残渣（25±5 ℃ 控温）", 1], ["MOD-GL-002", "0.22 µm 针式过滤至进样瓶", 1]],
    keys: ["CS", "GL"],
    fn: ["MOD-CS-005 四工位超声，25±5 ℃ 控温下超声 30 s 溶解残渣，保障痕量磺胺回收率（挑战-6）；",
      "加乙腈饱和正己烷 0.5 mL 涡旋、离心分层复用前站模块；",
      "MOD-GL-002 以 13 mm 针式过滤器过 0.22 µm 微孔滤膜终净化，降低基质杂质与离子抑制；",
      "过滤后直入 2 mL 进样小瓶，对接后端 LC-MS/MS；本站余 1 模块位可备扩展并行过滤以提升瓶颈通量。"],
    time: [["MOD-CS-005", "超声 30 s 溶解残渣", 30, 60, 90], ["MOD-GL-002", "0.22 µm 针式过滤", 520, 60, 580]] },
];

const PLT = {
  "PLT-800": { pn: "HHM40D-002", dim: "800×850×2055", kg: 175, area: "0.68 ㎡", mod: 3, tray: "4（1×4）", heat: 3 },
  "PLT-1200": { pn: "HHM40D-005", dim: "1200×850×2055", kg: 210, area: "1.02 ㎡", mod: 6, tray: "8（2×4）", heat: 6 },
  "PLT-1400": { pn: "HHM40D-001", dim: "1400×850×2055", kg: 210, area: "1.19 ㎡", mod: 6, tray: "8（1×8）", heat: 3 },
};

// ---------- 构建正文 ----------
const body = [];

/* ===== 封面 ===== */
body.push(new Paragraph({ spacing: { before: 1400, after: 0 }, alignment: AlignmentType.CENTER,
  children: [t("百泉聚兴（北京）科技有限公司", { bold: true, size: 26, color: BRAND })] }));
body.push(new Paragraph({ alignment: AlignmentType.CENTER, spacing: { before: 40, after: 600 },
  children: [t("SmartLabOS 智慧实验室系统", { size: 22, color: "6B7C93" })] }));
body.push(new Paragraph({ alignment: AlignmentType.CENTER, spacing: { before: 200, after: 20 },
  children: [t("成都市食品检验研究院", { bold: true, size: 34, color: INK })] }));
body.push(new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 20 },
  children: [t("兽药残留专项自动化检测（前处理）", { bold: true, size: 30, color: INK })] }));
body.push(new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 500 },
  children: [t("详细设计方案", { bold: true, size: 40, color: BRAND })] }));
body.push(new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 30 },
  children: [t("依据标准：GB/T 21316—2007《动物源性食品中磺胺类药物残留量的测定 液相色谱-质谱/质谱法》", { size: 20, color: "33455C" })] }));
body.push(new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 600 },
  children: [t("检测方式：LC-MS/MS · 上下料：自动上下料 · 软件：智慧实验室软件（SmartLabOS）", { size: 20, color: "33455C" })] }));
[["项目名称", "测试项目 · 兽残专项自动化前处理"], ["客户单位", "成都市食品检验研究院"],
 ["文档类型", "详细设计方案（售前技术提案）"], ["文档版本 / 状态", "V1.0 / 送审稿"],
 ["编制单位", "百泉聚兴（北京）科技有限公司 · SmartLabOS 智能方案助手"],
 ["编制日期", "2026 年 07 月 14 日"], ["联系人 / 报价", "涉及价格、报价信息，请联系销售团队获取报价"]].forEach(r => {
  body.push(new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 20 },
    children: [t(r[0] + "：", { bold: true, size: 20, color: BRAND }), t(r[1], { size: 20, color: INK })] }));
});

/* ===== 目录 ===== */
body.push(new Paragraph({ heading: HeadingLevel.HEADING_1, pageBreakBefore: true,
  children: [t("目　录", { bold: true, size: 30, color: BRAND })] }));
body.push(new TableOfContents("目录", { hyperlink: true, headingStyleRange: "1-3" }));

/* ===== 配套内部文档索引 ===== */
body.push(h1("0　文档元信息与配套文档索引"));
body.push(h2("0.1 文档元信息"));
body.push(table([2600, 7140], null, [
  [{ text: "客户全称", bold: true, fill: LIGHT }, "成都市食品检验研究院"],
  [{ text: "项目名称", bold: true, fill: LIGHT }, "测试项目 · 兽药残留专项自动化检测（前处理）"],
  [{ text: "文档名称", bold: true, fill: LIGHT }, "SmartLabOS 详细设计方案"],
  [{ text: "编制单位", bold: true, fill: LIGHT }, "百泉聚兴（北京）科技有限公司"],
  [{ text: "版本 / 状态", bold: true, fill: LIGHT }, "V1.0 / 送审稿"],
  [{ text: "编制 / 完成日期", bold: true, fill: LIGHT }, "2026-07-14"],
  [{ text: "适用标准", bold: true, fill: LIGHT }, "GB/T 21316—2007（2008-04-01 实施）"],
], { zebra: false }));
body.push(spacer());
body.push(h2("0.2 配套内部文档索引"));
body.push(p("本详细设计方案与前一阶段生成的以下文档配套使用，互为数据来源与溯源依据："));
body.push(table([900, 3600, 5240], ["#", "文档", "说明"], [
  ["1", "1.html（解决方案 HTML 提案）", "GB/T 21316—2007 §7.1 提取 + §7.2 净化 前处理全自动化解决方案主文档（11 章，含平台/模块/工作站/耗时测算）"],
  ["2", "微波萃取-模块-URS.html", "针对 §7.1「微波辅助提取」缺口新建模块（拟建 MOD-WB-001）用户需求说明书"],
  ["3", "模块选定-推荐.json", "已确认的 8 个模块清单与工序—模块映射说明"],
  ["4", "Summary-输出总结.md", "生成文件与方案要点汇总"],
  ["5", "references/01-modules、02-platforms、06-pallet", "模块卡 / 平台卡 / 托盘库（全部技术参数溯源来源）"],
], { zebra: true }));
body.push(note("数据来源声明",
  "本方案全部设备/软件指标均引用自 references/ 知识库模块卡、平台卡与已生成 HTML 提案；模块选型严格限定于已确认的 8 个模块，清单外模块一律不引入，唯一缺口（微波萃取）已出具 URS 并明确标注，未编造任何数据。"));

/* ===== 第1章 项目概述 ===== */
body.push(h1("第 1 章　项目概述"));
body.push(h2("1.1 客户与业务背景"));
body.push(p("成都市食品检验研究院为成都市市场监督管理局正处级公益二类事业单位，承担食品、食用农产品等的监督检验、风险监测与技术研究，是国家食品复检机构，牵头食用农产品抽检监测，并开展多兽药残留检测技术攻关。"));
body.push(p("该院针对食品中硝基呋喃、氯霉素、磺胺类、沙星类四大重点兽残开展检测，其前处理实操难点突出且极具针对性。四类兽残均为痕量残留、基质干扰极强，前处理是核心控险环节。本项目以「多品类兽残高通量大筛查」为核心基础，深度整合自动化前处理与上机检测全流程；本详细设计方案落地其中的磺胺类残留（GB/T 21316—2007）前处理自动化部分。"));
body.push(note("资质要点", "复检机构 / 监督检验 / 风险监测 / 食用农产品抽检监测牵头单位——对方法合规性、结果可追溯性与重复性要求极高。"));

body.push(h2("1.2 检测对象与目标物"));
body.push(table([2600, 7140], null, [
  [{ text: "检测项目", bold: true, fill: LIGHT }, "动物源性食品中磺胺类药物残留量测定（本方案）；院内并行开展硝基呋喃、氯霉素、沙星类兽残检测"],
  [{ text: "目标物清单", bold: true, fill: LIGHT }, "磺胺脒、磺胺嘧啶、磺胺甲噁唑等 23 种磺胺类药物"],
  [{ text: "样品基质", bold: true, fill: LIGHT }, "肝、肾、肌肉、水产品（鱼/虾）、牛奶等动物源性食品基质"],
  [{ text: "目标物理化特性", bold: true, fill: LIGHT }, "痕量残留、基质干扰极强、易乳化损失、热敏感（浓缩易分解/挥发）"],
  [{ text: "定量限 / 回收率", bold: true, fill: LIGHT }, "肉·肝·肾·奶 50 µg/kg、水产 10 µg/kg；回收率约 70%~110%，RSD ≤ 15%"],
], { zebra: false }));
body.push(spacer());

body.push(h2("1.3 现状痛点深度分析"));
body.push(p("人工前处理全程手工、容错率极低、对人员熟练度依赖强。下表将客户 6 项前处理痛点逐项剖析，并映射本方案的自动化应对（引用已确认模块）："));
body.push(table([2600, 3900, 3240], ["客户挑战（人工难点）", "机理与影响", "本方案自动化应对（引用模块）"], [
  ["挑战-1 全程手工容错率极低、依赖人员熟练度", "个体差异大、步骤繁琐，直接影响回收率与结果有效性", "全流程自动上下料 + SmartLabOS 固化 SOP，8 模块闭环执行，去人因"],
  ["挑战-2 样品均质易不均、高脂高蛋白分层、痕量取样偏差大", "取样代表性差、重复性差", "MOD-JZ-002 高剪切均质 1000–30000 rpm + C₁₈ 填料匀浆分散"],
  ["挑战-3 萃取极易乳化、分层困难、目标物易损失", "乳化致相分离困难、回收率损失", "MOD-WX-004 涡旋（≤3000 rpm）+ MOD-LX-001 高速离心破乳分层 + MOD-YY-004 全量转移防损失"],
  ["挑战-4 SPE 柱活化/平衡/流速难控、易柱干堵塞、回收率波动", "柱效不稳、回收率波动大", "本标准净化采用乙腈饱和正己烷液液分配（LLE）而非 SPE，由加液+涡旋+离心+移液组合复用实现，规避柱干/流速不均"],
  ["挑战-5 氮吹浓缩控温控速难、热敏物分解、定容精度不足", "目标物降解/挥发、定容不准", "MOD-NS-004 真空+离心+45–50 ℃ 低温热风三效浓缩（±1%），内置复溶精密定容 1 mL"],
  ["挑战-6 基质杂质难除、离子抑制、个体差异大", "严重离子抑制、精准度下降", "MOD-CS-005 超声助溶残渣 + MOD-GL-002 针式过滤（0.22 µm）终净化，降低离子抑制"],
], { zebra: true }));
body.push(spacer());

body.push(h2("1.4 项目场地概述"));
body.push(p("本方案 5 座工作站占地及公用工程需求详见第 3 章与第 10 章效率/公用工程章节。以下场地基础信息需在阶段一现场勘测中补全，本方案先给出对场地的硬性要求作为改造依据："));
body.push(table([2600, 7140], null, [
  [{ text: "改造前用途 / 楼层 / 房间", bold: true, fill: LIGHT }, "待现场勘测确认（客户暂未提供，不作臆测）"],
  [{ text: "场地尺寸（长×宽/面积/层高）", bold: true, fill: LIGHT }, "待现场勘测确认；本方案设备高 2055 mm，硬性要求层高 ≥ 3 m"],
  [{ text: "地面承重", bold: true, fill: LIGHT }, "要求 ≥ 500 kg/㎡（引自平台卡安装要求）"],
  [{ text: "设备净占地", bold: true, fill: LIGHT }, "5 平台合计约 4.25 ㎡，含通道按 2.5–3× 预留约 11–13 ㎡"],
  [{ text: "隔断打通 / 改造范围", bold: true, fill: LIGHT }, "待现场勘测确认，随排风/气路/强弱电改造一并设计（见 3.3）"],
], { zebra: false }));
body.push(note("○ 按需裁剪说明", "「原始结构尺寸图 / 改造范围图」因客户本阶段未提供现场图纸与尺寸，暂以「待现场勘测确认」占位，于阶段一《调研细化与方案确认》补全，不作编造。", "C9760F"));

/* ===== 第2章 自动化需求与标准适配 ===== */
body.push(h1("第 2 章　自动化需求与标准适配"));
body.push(h2("2.1 设计依据标准"));
body.push(table([2200, 4600, 2940], ["标准号", "标准全称", "本方案适用范围"], [
  ["GB/T 21316—2007", "动物源性食品中磺胺类药物残留量的测定 液相色谱-质谱/质谱法", "§7.1 提取、§7.2 净化（含浓缩复溶、超声、过滤），至 LC 进样小瓶为止"],
  ["GB/T 6682", "分析实验室用水规格和试验方法", "上水一级纯水（供均质清洗、浓缩、超声冷却）"],
], { zebra: true }));
body.push(note("现行有效性", "GB/T 21316—2007 于 2008-04-01 实施，为本方案磺胺类残留检测的现行有效方法标准；工艺流程严格遵循其 §5 仪器与 §7.1/§7.2 原文。"));

body.push(h2("2.2 检测标准与设备适配表"));
body.push(table([2000, 2100, 3400, 2240], ["检测方法号", "检测项目", "前处理系统（本方案）", "检测仪器"], [
  ["GB/T 21316—2007", "23 种磺胺类残留", "SmartLabOS 磺胺前处理产线（5 工作站：制样提取→微波离心→LLE 净化→浓缩复溶→超声过滤）", "LC-MS/MS"],
], { zebra: false }));
body.push(p("方法原理：C₁₈ 填料研磨 → 乙腈-水微波辅助提取 → 乙腈饱和正己烷液液分配净化 → 低温离心浓缩复溶 → 0.22 µm 过滤 → LC-MS/MS 外标法定量。", { size: 20, color: "33455C" }));
body.push(spacer());

body.push(h2("2.3 自动化需求统计"));
body.push(p("客户流程范围：制样 → 分样 → 前处理 → 检测 → 出具报告；上下料方式：自动上下料；软件：智慧实验室软件（SmartLabOS）。本方案自动化范围界定与节拍目标如下："));
body.push(table([3400, 6340], null, [
  [{ text: "自动化覆盖范围", bold: true, fill: LIGHT }, "自「加乙腈-水」提取起，至「0.22 µm 过滤进样」止的前处理全链路（均质→提取→净化→浓缩→过滤）"],
  [{ text: "平台节拍目标", bold: true, fill: LIGHT }, "各平台处理节拍尽量相等并接近 600 s（5 平台均值 ≈ 596 s）"],
  [{ text: "解决方案总节拍", bold: true, fill: LIGHT }, "单样 5 平台串行叠加 ≈ 2982 s ≈ 49.7 min/样（稳态流水通量远高于串行值）"],
  [{ text: "通量目标（待客户确认）", bold: true, fill: LIGHT }, "客户本阶段未给出「X 天 Y 样 / 年上机针数」量化指标，需于阶段一确认后据以配置并行度（详见第 6 章）"],
], { zebra: false }));
body.push(note("○ 差异化检项适配", "本方案落地磺胺类（LC-MS/MS）；院内并行的硝基呋喃（含恒温衍生）、氯霉素、沙星类检项，可在同一 SmartLabOS 平台体系下经调度分时复用与增配对应模块扩展适配，具体于后续专项方案细化。", "C9760F"));

/* ===== 第3章 总体设计 ===== */
body.push(h1("第 3 章　总体设计"));
body.push(h2("3.1 设计依据与原则"));
body.push(bullet([t("模块化：", { bold: true }), t("以 50 mL 尖底离心管为标准化容器，全部工序由标准模块位（450×180×600 mm）模块承载，即插即用、可裁剪可扩展。")]));
body.push(bullet([t("智能物流联动：", { bold: true }), t("机械臂 + 自动上下料实现工站间样品流转，5 座工作站串联为完整样品制备产线。")]));
body.push(bullet([t("数字化管理：", { bold: true }), t("SmartLabOS 智慧实验室软件固化 SOP、调度设备、采集数据、全生命周期追溯。")]));
body.push(bullet([t("可扩展：", { bold: true }), t("平台预留模块位/托盘位，瓶颈工序可增配并行模块提升通量。")]));
body.push(p("业务闭环范围：", { bold: true }));
body.push(p("制样（人工称样研磨）→ 分样 → 提取（均质·加液·涡旋·微波·离心·取层）→ 净化（LLE 加正己烷·振摇·离心·取底层）→ 浓缩·定容（低温离心浓缩+复溶）→ 过滤进样 → 检测（LC-MS/MS）→ 报告。本方案自动化覆盖「提取→过滤进样」；称样/研磨/分样为人工前置工位（见 3.1 缺口说明）。"));
body.push(note("上游人工工序（诚实标注）", "精密称样（2 g，感量 0.01 g）、加 C₁₈ 填料玻璃研钵研磨、固体四分法分样——已确认清单与知识库中无对应称量/研磨/固体分样模块，由人工/上游完成，自动化范围自「加乙腈-水」起。", "C1121F"));

body.push(h2("3.2 整体布局设计"));
body.push(p("本方案按功能与节拍均衡划分为 5 座工作站（区域），采用 3 类标准平台承载："));
body.push(table([1500, 3300, 1600, 3340], ["工作站", "功能", "平台", "承接标准条款"],
  WS.map(w => [w.id.replace("WS-SA-", "").replace("-CDFI-0", " #"), w.name, w.plt, w.clause]), { zebra: true }));
body.push(spacer());
body.push(p("布局合理性论证：", { bold: true }));
body.push(bullet("按前处理工序顺序线性排布，样品单向流转，避免交叉往复，操作便捷、流程合理；"));
body.push(bullet("制样提取、LLE 净化、超声过滤三站模组位 ≤ 3，选紧凑 PLT-800（0.68 ㎡）省地；含离心机（占 3 位）的微波·离心站选 6 位 PLT-1200；浓缩瓶颈以 3 台并行选 PLT-1400（1×8 线性托盘利于连续缓存），空间利用率高；"));
body.push(bullet("三类平台均被采用，标准化预装、现场拼接，缩短建设周期。"));
body.push(note("○ 布局图裁剪说明", "「智能化实验室规划布局 CAD 图」需结合 1.4 现场尺寸于阶段一出具；本方案已在第 4 章为每座工作站提供平台俯视布局（模块位占用）示意描述，规划总图待现场勘测后补全，不作臆造。", "C9760F"));

body.push(h2("3.3 配套基础设施改造设计"));
body.push(p("依据各模块 electrical_environment 与平台公用工程要求，配套改造设计如下（详细管线图于阶段一现场勘测后出具）："));
body.push(table([2200, 7540], ["改造系统", "设计要求（引自模块/平台卡）"], [
  ["管道排风系统", "提取涉乙腈、净化涉正己烷/正丙醇 + 45 ℃ 浓缩挥发，离心机（MOD-LX-001）发热——各工站上方设万向抽气罩/防爆排风，罩口面风速 ≥ 0.5 m/s，经活性炭处理达标排放；微波模块（拟建）需防爆排风。"],
  ["气路分布系统", "洁净干燥压缩空气 0.4–0.7 MPa（供加液/涡旋/移液/过滤/均质模块气动）；真空泵：离心机 −80 kPa、浓缩 −95 kPa 配套。"],
  ["强弱电与网络系统", "主电 AC 220 V 单相 + 控制电 DC 24 V；浓缩 MOD-NS-004 需 DC 48 V、超声 MOD-CS-005 为 AC 220 V；独立配电箱 + 漏电/接地保护；建议配 3 kVA 在线式 UPS；网络接入 SmartLabOS 标准总线（工业以太网）。"],
  ["给排水 / 纯水管路", "上水：一级纯水（GB/T 6682），供均质清洗、浓缩、超声冷却；下水：平台排废口接独立排废管路，有机废液不得直排市政下水，分类收集至危废中转罐。"],
], { zebra: true }));
body.push(note("★ 施工衔接", "以上改造要求为工程可施工依据；管道排风/气路/强弱电/给排水的具体走向图纸，需结合 1.4 现场结构尺寸于阶段一《调研细化与方案确认》完成，并经甲方评审。"));

/* ===== 第4章 功能平台逐一详细设计 ===== */
body.push(h1("第 4 章　功能平台逐一详细设计"));
body.push(p("本章对 5 座已配置工作站逐一展开【平台组成 + 平台功能说明 + 关键技术指标表】三要素。全部模块严格取自已确认的 8 个模块清单（+1 拟建微波模块 URS）；指标均量化并含供电与通讯口径。", { color: "33455C" }));
body.push(note("模块位口径", "MOD-LX-001（离心机）占 3 位；MOD-NS-004（浓缩）占 2 位；其余每模块占 1 位。通讯口径：各模块经平台机械臂与控制单元统一接入 SmartLabOS 智慧实验室软件标准总线（工业以太网 / RS-485 Modbus），DC24V/48V 或 AC220V 供电如各表所列。"));

WS.forEach((w, wi) => {
  const plt = PLT[w.plt];
  body.push(h2(`4.${wi + 1} ${w.name}（${w.id}）`));
  body.push(p([t("承载平台：", { bold: true }), t(`${w.plt}（${plt.dim} mm，${plt.area}，${plt.kg} kg，${plt.mod} 模块位 / ${plt.tray} 托盘，发热模组上限 ${plt.heat}）`),
    t("　｜　覆盖标准：", { bold: true }), t(w.clause), t("　｜　模块位：", { bold: true }), t(w.slots),
    t("　｜　节拍：", { bold: true }), t(`${w.cyc} s`)]));
  // 平台组成
  body.push(h3("① 平台组成"));
  body.push(table([3000, 5540, 1200], ["模块", "功能", "位"],
    w.mods.map(m => [{ text: m[0], bold: true }, m[1], { text: String(m[2]), align: AlignmentType.CENTER }]), { zebra: true }));
  // 功能说明
  body.push(h3("② 平台功能说明"));
  w.fn.forEach(f => body.push(bullet(f)));
  // 关键技术指标表
  body.push(h3("③ 关键技术指标表"));
  const rows = [];
  w.keys.forEach((k, idx) => {
    const m = MODULES[k];
    m.ind.forEach((pair, j) => {
      const cells = [];
      if (j === 0) cells.push({ content: [new Paragraph({ children: [t(m.id, { bold: true, size: 18 })],
        spacing: { after: 8 } }), new Paragraph({ children: [t(m.name, { size: 16, color: "6B7C93" })] }),
        new Paragraph({ children: [t(`${m.dim} mm · ${m.kg} kg · ${m.slots} 位`, { size: 16, color: "6B7C93" })] })],
        rowSpanFlag: m.ind.length });
      else cells.push(null);
      cells.push({ text: pair[0], bold: true, size: 18 });
      cells.push({ text: pair[1], size: 18 });
      rows.push({ cells, first: j === 0, span: m.ind.length });
    });
  });
  // 手工构造带 rowSpan 的表
  body.push(indicatorTable(w.keys.map(k => MODULES[k])));
  if (w.id.includes("Extract")) body.push(note("模块缺口（微波）", "本站 MOD-WB-001 为拟建微波辅助萃取模块（已确认清单及 97 模块库均无匹配，不得以清单外模块替换）；已按售前指令 5.3 出具《微波萃取-模块-URS.html》，处理时间 30 s×2 + 上下料 60 s = 120 s 已计入本站 580 s 节拍。落地前以人工微波炉工位衔接 WS1↔WS2。", "C1121F"));
});

// rowSpan 指标表构造器
function indicatorTable(mods) {
  const widths = [2600, 2600, 4540];
  const trs = [headerRow(["模块（设备）", "指标项", "指标值 / 精度 / 供电 / 通讯"], widths)];
  mods.forEach(m => {
    m.ind.forEach((pair, j) => {
      const cells = [];
      if (j === 0) {
        cells.push(new TableCell({ width: { size: widths[0], type: WidthType.DXA }, borders: BORDERS,
          margins: CELLM, verticalAlign: VerticalAlign.CENTER, rowSpan: m.ind.length, shading: { fill: "F0F6FC", type: ShadingType.CLEAR, color: "auto" },
          children: [new Paragraph({ children: [t(m.id, { bold: true, size: 18, color: BRAND })], spacing: { after: 6 } }),
            new Paragraph({ children: [t(m.name, { size: 16, color: "33455C" })], spacing: { after: 6 } }),
            new Paragraph({ children: [t(`${m.dim} mm`, { size: 15, color: "6B7C93" })] }),
            new Paragraph({ children: [t(`${m.kg} kg · ${m.slots} 模块位`, { size: 15, color: "6B7C93" })] })] }));
      }
      cells.push(new TableCell({ width: { size: widths[1], type: WidthType.DXA }, borders: BORDERS, margins: CELLM,
        verticalAlign: VerticalAlign.CENTER, children: [new Paragraph({ children: [t(pair[0], { bold: true, size: 18 })], spacing: { before: 6, after: 6 } })] }));
      cells.push(new TableCell({ width: { size: widths[2], type: WidthType.DXA }, borders: BORDERS, margins: CELLM,
        verticalAlign: VerticalAlign.CENTER, children: [new Paragraph({ children: [t(pair[1], { size: 18 })], spacing: { before: 6, after: 6 } })] }));
      trs.push(new TableRow({ children: cells }));
    });
  });
  return new Table({ width: { size: widths.reduce((a, b) => a + b, 0), type: WidthType.DXA }, columnWidths: widths, rows: trs, borders: BORDERS });
}

body.push(h2("4.6 平台选型汇总"));
body.push(table([1900, 2400, 1600, 1900, 1940], ["平台", "尺寸(mm)", "模组/托盘", "发热上限", "本方案采用"], [
  ["PLT-800", "800×850×2055", "3 / 4（1×4）", "3", "WS1 / WS3 / WS5"],
  ["PLT-1200", "1200×850×2055", "6 / 8（2×4）", "6", "WS2（含离心机 3 位）"],
  ["PLT-1400", "1400×850×2055", "6 / 8（1×8）", "3", "WS4（3×浓缩并行）"],
], { zebra: true }));

body.push(h2("4.7 辅助 / 可选设备（○ 按需）"));
body.push(table([2400, 7340], ["设备", "说明"], [
  ["纯水制备系统", "一级纯水（GB/T 6682）供均质清洗/浓缩/超声冷却；型号与产水能力按现场用水量核定。报价请联系销售团队。"],
  ["UPS", "推荐 3 kVA 在线式 UPS（平台标配接口），续航按关键工序保护时长核定。报价请联系销售团队。"],
  ["增强型 SPE / 特殊净化模块", "本磺胺标准净化采用 LLE，不需 SPE；若院内其他兽残检项（如需 SPE 净化）扩展时按需增配。"],
], { zebra: true }));
body.push(note("○ 裁剪理由", "本项目磺胺标准净化以液液分配（LLE）实现，故不配置 SPE 模块；纯水/UPS 为通用配套，型号容量待现场用量核定，价格请联系销售团队获取报价。", "C9760F"));

/* ===== 第5章 软件系统详细设计 ===== */
body.push(h1("第 5 章　软件系统详细设计"));
body.push(p("本项目软件为「智慧实验室软件（SmartLabOS）」，负责设备调度、流程执行、数据管理与全生命周期追溯。以下为 SmartLabOS 标准能力框架；涉及与院内 LIMS 的具体对接字段与 SLA 数值，于阶段一接口设计中确认。", { color: "33455C" }));
body.push(h2("5.1 中央控制系统"));
body.push(h3("技术架构"));
body.push(bullet("B/S 架构、分布式部署；采用「设备执行层—单机控制层—调度控制层—管理应用层」分层架构，集中调度 + 分散执行；"));
body.push(bullet("前后端分离，标准总线（工业以太网 / RS-485 Modbus）连接各模块与机械臂；支持本地部署。"));
body.push(h3("功能模块清单"));
body.push(table([2600, 7140], ["功能模块", "说明"], [
  ["基础数据管理", "检测标准库、实验流程库、模块/平台/托盘台账、试剂耗材库"],
  ["样品管理", "样品全生命周期（采样前/中/后）+ 申请-审批-确认-记录闭环，条码标识与追溯"],
  ["仓库管理", "试剂耗材物料调度、库存监测、补料提醒"],
  ["实验流程管理", "SOP 固化、按 GB/T 21316—2007 §7.1/§7.2 编排前处理流程"],
  ["实验调度", "多工作站/多模块并行调度，节拍均衡（≈600 s），瓶颈平衡"],
  ["设备管理与自动化执行", "单机设备执行、设备台账与运维（保养提醒、故障报警、正反向追溯）"],
  ["数字化大屏", "实时数据展示与运行监控"],
], { zebra: true }));
body.push(h3("自动化功能"));
["实验室调度控制：多工作站流水调度、节拍匹配、异常重试；",
 "单机设备执行：按功能码驱动各模块动作（如 115 均质、169 加液、188 离心、200 浓缩、310 过滤）；",
 "仓库管理与物料调度：托盘/试剂/耗材调度与补料提醒；",
 "检测标准库与实验流程库：标准方法与 SOP 版本化管理；",
 "样品全生命周期管理：申请-审批-确认-记录闭环，条码追溯；",
 "设备台账与运维：保养提醒、故障报警、正反向追溯。"].forEach(x => body.push(bullet(x)));
body.push(h3("数据管理功能"));
["实验数据抓取与传输、多通道实时采集与校验；",
 "与院内 LIMS 双向对接（对接字段于阶段一确认）；",
 "通风/有害气体监测联动；数据清洗、审查与质控校验；",
 "实时存储与多维查询；智能监测分析与预警。"].forEach(x => body.push(bullet(x)));
body.push(h3("报表 / 报告 / 权限 / 安全"));
["报表与统计：设备使用、检测次数、试剂用量、异常判定、原始记录/报告导出；",
 "报告定制与一键生成：模板定制 + 多级审核；数据导出 Word/Excel/PDF；",
 "角色与权限管理：分级 + 自定义角色；",
 "报警管理：实时/历史、规则配置、多通道通知；",
 "数据安全：冗余容错、传输存储加密、权限隔离。"].forEach(x => body.push(bullet(x)));

body.push(h2("5.2 数字孪生 / 可视化系统（○ 按需）"));
body.push(p("本项目为前处理产线自动化，数字孪生为可选高端配置。若客户选配，SmartLabOS 可提供中央控制可视化、试剂耗材管理、物流与设备运行监控三大组成，及实时数据展示与监控、实验过程可视化（视频流/多视角/录制）、数据分析与决策支持、预警与应急响应、成果展示与参观交互五大功能。"));
body.push(note("○ 裁剪理由", "数字孪生非本前处理项目必备项，是否选配由客户按参观展示/管理需求决定；本方案默认以数字化大屏满足实时监控，数字孪生留作扩展。", "C9760F"));

body.push(h2("5.3 软件系统技术指标"));
body.push(table([2600, 7140], ["指标类别", "指标要求"], [
  ["开放与集成", "提供 RESTful API、HTTPS 传输；标准通用通讯协议（工业以太网 / RS-485 Modbus）；支持与院内 LIMS / 工厂信息化双向对接"],
  ["安全性", "冗余容错、可扩充、操作日志留痕、关键功能冗余切换"],
  ["可靠性", "支持 7×24 运行、集中-分散控制；年故障累计时间上限于合同 SLA 约定（待确认）"],
  ["可扩展性", "支持硬件扩容与软件免定制配置适配，模块即插即用、升级简便"],
  ["日志记录", "操作日志含操作人/时间/动作/对象要素，保存时长可设、自动清理"],
], { zebra: true }));
body.push(note("★ LIMS 集成", "软件可集成关键：SmartLabOS 经 RESTful API / HTTPS 与院内 LIMS 双向对接，检测结果与原始记录回传 LIMS；具体接口字段与数据流向于阶段一接口设计确认。"));

/* ===== 第6章 自动化产能效率 ===== */
body.push(h1("第 6 章　自动化产能效率"));
body.push(p("测算口径：每平台处理节拍 = Σ 该平台所载模块（功能码单循环耗时 + 上下料时间）；本项目自动上下料，上下料按各模块卡 module_up_unload_time = 60 s 计；解决方案总节拍 = Σ 各平台节拍。数值取自模块卡 cycle_time_sec，微波缺口模块取标准规定时长。以下数据与 1.html 提案计算完全一致。", { color: "33455C" }));

body.push(h2("6.1 自动化检测效率设计逻辑"));
body.push(bullet("多模块并行：浓缩瓶颈以 3 台 MOD-NS-004 并行破解，离心 4 孔位、超声四工位并行；"));
body.push(bullet("全流程联动：5 工作站机械臂自动上下料串联，工站间流水重叠；"));
body.push(bullet("节拍匹配：将各平台节拍均衡至接近 600 s（均值 ≈ 596 s），最大偏差 +70 s / −54 s。"));

body.push(h2("6.2 逐平台处理耗时测算"));
WS.forEach((w, wi) => {
  body.push(h3(`平台 ${wi + 1} · ${w.name}（${w.plt}）`));
  const rows = w.time.map(r => [{ text: r[0], bold: true }, r[1], { text: String(r[2]), align: AlignmentType.CENTER },
    { text: String(r[3]), align: AlignmentType.CENTER }, { text: String(r[4]), align: AlignmentType.CENTER }]);
  rows.push([{ content: cellP([t(`平台 ${wi + 1} 节拍（与 600 s 目标偏差 ${w.cyc - 600 >= 0 ? "+" : ""}${w.cyc - 600} s）`, { bold: true })]), span: 4, fill: "EAF2FB" },
    { text: `${w.cyc} s`, bold: true, align: AlignmentType.CENTER, fill: "EAF2FB" }]);
  body.push(table([2700, 3540, 1300, 1100, 1100], ["模块", "功能（标准步骤）", "功能码(s)", "上下料(s)", "小计(s)"], rows));
  body.push(spacer(40));
});
body.push(note("并行说明", "MOD-NS-004 单台浓缩约 1800 s 为全线瓶颈，以 3 台并行使有效节拍 = 1800÷3 = 600 s，叠加上下料 ≈ 620 s，与其余平台对齐。"));

body.push(h2("6.3 解决方案总节拍（5 平台叠加）"));
const totRows = WS.map((w, i) => [`P${i + 1} · ${w.name}`, w.plt, { text: String(w.cyc), align: AlignmentType.CENTER },
  { text: (w.cyc / 60).toFixed(1), align: AlignmentType.CENTER }]);
totRows.push([{ text: "解决方案总节拍（串联叠加）", bold: true, fill: "EAF2FB" }, { text: "5 平台", bold: true, fill: "EAF2FB" },
  { text: "2982", bold: true, align: AlignmentType.CENTER, fill: "EAF2FB" }, { text: "≈ 49.7", bold: true, align: AlignmentType.CENTER, fill: "EAF2FB" }]);
totRows.push([{ content: cellP([t("平台节拍均值", { bold: true })]), span: 2, fill: "F5F8FC" },
  { content: cellP([t("596 s ≈ 9.9 min（≈ 目标 600 s，最大偏差 +70 s / −54 s）", { bold: true })]), span: 2, fill: "F5F8FC" }]);
body.push(table([3900, 2340, 1750, 1750], ["平台 / 工作站", "平台", "节拍(s)", "≈ min"], totRows));
body.push(spacer());

body.push(h2("6.4 专项 / 痕量与瓶颈说明"));
body.push(bullet("专项/痕量检测（磺胺 23 种）：浓缩（1800 s 单台）与过滤（520 s）为耗时最长环节，已分别以 3 台浓缩并行、针式过滤终净化保障回收率（70%~110%、RSD ≤15%）；"));
body.push(bullet("瓶颈平台为 WS5 超声过滤（670 s），提升整体稳态通量可在 WS5 增配并行过滤或将超声前移；"));
body.push(bullet("再入工序（提取残渣重复 1 次、LLE 对离心/加液/涡旋/移液的复用）按单轮计入横向比较，实际 WS2/WS3 residence 可达约 1.5~2×。"));
body.push(note("效率提升对比（vs 人工）", "自动化以固化 SOP + 自动上下料替代人工繁琐操作，规避均质不均、萃取乳化、SPE/氮吹损失等痛点，降低个体差异、提升重复性与回收率精准度。具体「人力 N→M、错误率 X%→Y%、X 天 Y 样」量化对比需结合客户现行人工基线与确认的通量目标测算，于阶段一补全，不作臆造数值。", "C9760F"));

/* ===== 第7章 项目实施与管理方案 ===== */
body.push(h1("第 7 章　项目实施与管理方案"));
body.push(h2("7.1 周期与组织"));
body.push(p("总建设周期：平台/模块标准交付约 8 周、工站集成约 12 周（引自平台/模块卡 lead_time_weeks，供参考，最终以合同为准）；拟建微波模块 MOD-WB-001 需另行 URS 评审与开发排期。"));
body.push(p("组织架构与职责：", { bold: true }));
body.push(table([2600, 7140], ["角色", "职责"], [
  ["乙方项目经理", "总体进度、资源协调、对甲方接口，三级进度管控"],
  ["乙方技术团队", "方案深化、模块生产/预装调试、现场安装拼接、系统调试"],
  ["乙方质控团队", "全流程质控追溯、质控节点验收、归档"],
  ["甲方对接小组", "需求确认、现场条件保障、阶段验收、变更审批"],
], { zebra: true }));

body.push(h2("7.2 实施阶段"));
body.push(table([1700, 1200, 2600, 2340, 1900], ["阶段", "工期", "核心任务 / 交付物", "验收标准", "管控要点"], [
  ["一 调研细化与方案确认", "1–2 周", "现场勘测、尺寸/图纸补全、URS 确认、方案定稿", "方案与接口经甲方评审签字", "需求冻结、变更基线"],
  ["二 模块生产与工厂预装调试", "约 8 周", "模块生产、平台预装、工厂 FAT 调试报告", "工厂调试报告合格、设备合格证", "质控节点、原厂部件"],
  ["三 现场基础施工与复核", "并行", "排风/气路/强弱电/给排水改造、复核", "现场条件复核单通过", "施工安全、隐蔽验收"],
  ["四 现场安装与模块拼接", "1–2 周", "平台就位、模块拼接、安装验收单", "安装/拼接验收单签署", "定位精度、接地"],
  ["五 系统调试与人员培训", "2–3 周", "联调、SmartLabOS 联通、方法学验证、培训", "SAT 联调通过、操作培训完成", "回收率/RSD 达标"],
  ["六 项目验收与交付归档", "1 周", "验收报告、移交清单、文档归档", "验收报告签署、资料齐全", "全流程可追溯"],
], { zebra: true }));
body.push(note("总工期甘特图", "总工期甘特图（各阶段起止与并行关系）于阶段一方案定稿时随排期表出具；上表工期为参考值，最终以合同排期为准。", "C9760F"));

body.push(h2("7.3 项目管理措施"));
body.push(bullet([t("进度管理：", { bold: true }), t("三级管控 + 固定例会 + 赶工预案；")]));
body.push(bullet([t("质量管理：", { bold: true }), t("全流程质控追溯、关键质控节点验收、归档；")]));
body.push(bullet([t("沟通管理：", { bold: true }), t("固定沟通机制、专属项目群、定期例会；")]));
body.push(bullet([t("安全管理：", { bold: true }), t("用电/动火/接地/应急停机规程，权限分级；")]));
body.push(bullet([t("变更管理：", { bold: true }), t("书面申请 + 影响评估 + 签字确认。")]));

body.push(h2("7.4 保障措施"));
body.push(bullet([t("资源保障：", { bold: true }), t("资深工程师 + 专用设备 + 原厂部件，甲方配合到位；")]));
body.push(bullet([t("技术保障：", { bold: true }), t("攻坚小组 + 7×24 远程支持 + 现场响应时限（合同约定）；")]));
body.push(bullet([t("售后前置：", { bold: true }), t("售后工程师参与调试；")]));
body.push(bullet([t("文档保障：", { bold: true }), t("全流程归档、可追溯。")]));

/* ===== 第8章 交付物清单 ===== */
body.push(h1("第 8 章　交付物清单"));
body.push(table([3000, 6740], ["类别", "交付物"], [
  ["正式技术文档", "详细设计方案、各功能平台设计图、系统架构图"],
  ["工程交付物", "布局 CAD 图、工厂/现场调试报告、设备合格证、安装/拼接验收单、验收报告、设备移交清单"],
  ["资料交付", "操作手册、维护手册、技术资料（纸质 + 电子）、售后服务协议"],
  ["软件交付", "SmartLabOS 部署、账号与权限配置、LIMS 对接联通、报表/报告模板"],
  ["缺口文档", "MOD-WB-001 微波萃取模块 URS（微波萃取-模块-URS.html）"],
], { zebra: true }));

/* ===== 附录 数据来源与合规 ===== */
body.push(h1("附录　数据来源、合规与待确认事项"));
body.push(h2("A.1 合规与数据来源声明"));
body.push(bullet("全部模块/平台/托盘参数均引用自知识库 references/01-modules、02-platforms、06-pallet，逐一取卡校验，未编造数据；"));
body.push(bullet("模块选型严格限定于已确认的 8 个模块，清单外模块一律不引入；唯一缺口（微波萃取）已明确标注并出具 URS；"));
body.push(bullet("工艺流程严格遵循 GB/T 21316—2007 §5 / §7.1 / §7.2 原文；"));
body.push(bullet([t("涉及价格、报价信息：", { bold: true }), t("请联系销售团队获取报价。", { bold: true, color: "C1121F" })]));
body.push(h2("A.2 已确认模块清单（选型范围）"));
body.push(table([2400, 4340, 1500, 1500], ["模块 ID", "名称", "工序", "占位"], [
  ["MOD-JZ-002", "50ml离心管均质模块", "均质", "1"],
  ["MOD-JY-010", "50ml离心管拆装盖加液模块", "加液", "1"],
  ["MOD-WX-004", "50ml离心管单涡旋模块", "涡旋", "1"],
  ["MOD-LX-001", "50ml×4孔位高速离心机模块（带XYZ）", "离心", "3"],
  ["MOD-YY-004", "50ml全量液体转移模块", "移液", "1"],
  ["MOD-NS-004", "50ml西林瓶连续离心浓缩", "浓缩", "2"],
  ["MOD-CS-005", "50ml离心管超声模块", "超声", "1"],
  ["MOD-GL-002", "5cc 针式过滤模块", "过滤", "1"],
  [{ text: "MOD-WB-001（拟建·URS）", color: "C1121F" }, { text: "50ml离心管微波辅助萃取模块", color: "C1121F" }, { text: "微波", color: "C1121F" }, { text: "1", color: "C1121F" }],
], { zebra: true }));
body.push(h2("A.3 待客户确认事项"));
["微波萃取模块（MOD-WB-001）立项开发排期，或落地前以人工微波炉工位衔接 WS1↔WS2；",
 "称样/研磨/分样人工前置工位的现场衔接方式；",
 "浓缩「低温离心浓缩」等效替代旋蒸/氮吹的方法学验证（回收率 70%~110%、RSD ≤15%）；",
 "通量目标（X 天 Y 样 / 年上机针数）与瓶颈平台（WS5 670 s）是否需增配并行以提升稳态产能；",
 "现场场地尺寸/结构图、LIMS 对接接口字段与 SLA 数值。"].forEach((x, i) => body.push(numItem(x)));

/* ===== 文档组装 ===== */
const doc = new Document({
  creator: "百泉聚兴 SmartLabOS 智能方案助手",
  title: "测试项目 SmartLabOS 详细设计方案",
  description: "成都市食品检验研究院 · 磺胺类残留（GB/T 21316—2007）前处理自动化详细设计方案",
  styles: {
    default: { document: { run: { font: FONT, size: 21, color: INK } } },
    paragraphStyles: [
      { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 30, bold: true, font: FONT, color: BRAND },
        paragraph: { spacing: { before: 320, after: 200 }, outlineLevel: 0,
          border: { bottom: { style: BorderStyle.SINGLE, size: 12, color: BRAND, space: 6 } } } },
      { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 25, bold: true, font: FONT, color: INK },
        paragraph: { spacing: { before: 240, after: 120 }, outlineLevel: 1 } },
      { id: "Heading3", name: "Heading 3", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 22, bold: true, font: FONT, color: BRAND2 },
        paragraph: { spacing: { before: 160, after: 80 }, outlineLevel: 2 } },
    ],
  },
  numbering: {
    config: [
      { reference: "bul", levels: [{ level: 0, format: LevelFormat.BULLET, text: "•", alignment: AlignmentType.LEFT,
        style: { paragraph: { indent: { left: 460, hanging: 280 } } } }] },
      { reference: "num", levels: [{ level: 0, format: LevelFormat.DECIMAL, text: "%1.", alignment: AlignmentType.LEFT,
        style: { paragraph: { indent: { left: 460, hanging: 280 } } } }] },
    ],
  },
  sections: [{
    properties: { page: { size: { width: 11906, height: 16838 },
      margin: { top: 1400, right: 1080, bottom: 1300, left: 1080 } } },
    headers: { default: new Header({ children: [new Paragraph({
      alignment: AlignmentType.RIGHT, border: { bottom: { style: BorderStyle.SINGLE, size: 4, color: "BFC9D4", space: 4 } },
      children: [t("测试项目 · SmartLabOS 详细设计方案　|　GB/T 21316—2007 磺胺类残留前处理自动化", { size: 15, color: "6B7C93" })] })] }) },
    footers: { default: new Footer({ children: [new Paragraph({ alignment: AlignmentType.CENTER,
      border: { top: { style: BorderStyle.SINGLE, size: 4, color: "BFC9D4", space: 4 } },
      children: [t("百泉聚兴（北京）科技有限公司　·　第 ", { size: 15, color: "6B7C93" }),
        new TextRun({ children: [PageNumber.CURRENT], size: 15, font: FONT, color: "6B7C93" }),
        t(" 页 / 共 ", { size: 15, color: "6B7C93" }),
        new TextRun({ children: [PageNumber.TOTAL_PAGES], size: 15, font: FONT, color: "6B7C93" }),
        t(" 页", { size: 15, color: "6B7C93" })] })] }) },
    children: body,
  }],
});

const OUT = "C:\\TestClaude\\SmartLabOS-AI-Assistant\\projects\\测试项目\\测试项目_SmartLabOS_Presales_提案_20260714182642.docx";
Packer.toBuffer(doc).then(buf => { fs.writeFileSync(OUT, buf); console.log("SAVED:", OUT, "bytes=", buf.length); });
