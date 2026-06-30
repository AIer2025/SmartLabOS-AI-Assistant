// SmartLabOS 售前 WORD 提案生成器 — 项目：测试项目（成都市食品检验研究院 · 兽残专项）
// 数据源：projects/测试项目/{1.html,2.html,串接双柱SPE净化-模块-URS.html,模块选定-推荐.json}
//        references/01-modules/*.md（11 个已确认模块卡）
// 输出：测试项目_SmartLabOS_Presales_提案_20260630160022.docx
const fs = require("fs");
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
  Header, Footer, AlignmentType, LevelFormat, TabStopType, TabStopPosition,
  TableOfContents, HeadingLevel, BorderStyle, WidthType, ShadingType,
  VerticalAlign, PageNumber, PageBreak
} = require("docx");

// ---------- 颜色 / 常量 ----------
const BRAND = "0C5C8F", BRAND2 = "0A7A72", INK = "0F1B2D", HEADFILL = "0C5C8F";
const ZEBRA = "F1F6FB", WARNFILL = "FFF6E5", OKFILL = "E9F7EF", BADFILL = "FDECEE", INFOFILL = "EAF2FB";
const CW = 9360; // content width (US Letter, 1" margins)

const cellBorder = { style: BorderStyle.SINGLE, size: 4, color: "C9D6E4" };
const BORDERS = { top: cellBorder, bottom: cellBorder, left: cellBorder, right: cellBorder };

// ---------- 文本/段落 helper ----------
function P(text, opts = {}) {
  const runs = Array.isArray(text) ? text : [new TextRun({ text: String(text), ...opts.run })];
  return new Paragraph({
    children: runs,
    spacing: { after: opts.after ?? 120, before: opts.before ?? 0, line: 300 },
    alignment: opts.align,
    ...opts.p
  });
}
function H1(text) { return new Paragraph({ heading: HeadingLevel.HEADING_1, children: [new TextRun(text)], spacing: { before: 320, after: 160 } }); }
function H2(text) { return new Paragraph({ heading: HeadingLevel.HEADING_2, children: [new TextRun(text)], spacing: { before: 240, after: 120 } }); }
function H3(text) { return new Paragraph({ heading: HeadingLevel.HEADING_3, children: [new TextRun(text)], spacing: { before: 180, after: 100 } }); }
function bullet(text, level = 0) {
  const runs = Array.isArray(text) ? text : [new TextRun(String(text))];
  return new Paragraph({ numbering: { reference: "bullets", level }, children: runs, spacing: { after: 60, line: 290 } });
}
function num(text, level = 0) {
  const runs = Array.isArray(text) ? text : [new TextRun(String(text))];
  return new Paragraph({ numbering: { reference: "nums", level }, children: runs, spacing: { after: 60, line: 290 } });
}
function b(t) { return new TextRun({ text: t, bold: true }); }
function t(t2) { return new TextRun({ text: t2 }); }
function mono(t3) { return new TextRun({ text: t3, font: "Consolas", color: BRAND, bold: true }); }

// 提示框（单元格底色的单格表）
function callout(title, lines, fill) {
  const kids = [new Paragraph({ children: [new TextRun({ text: title, bold: true, color: INK })], spacing: { after: 60 } })];
  for (const ln of lines) {
    const runs = Array.isArray(ln) ? ln : [new TextRun(String(ln))];
    kids.push(new Paragraph({ numbering: { reference: "bullets", level: 0 }, children: runs, spacing: { after: 40, line: 280 } }));
  }
  return new Table({
    width: { size: CW, type: WidthType.DXA }, columnWidths: [CW],
    rows: [new TableRow({ children: [new TableCell({
      borders: { top: { style: BorderStyle.SINGLE, size: 18, color: BRAND }, bottom: cellBorder, left: cellBorder, right: cellBorder },
      width: { size: CW, type: WidthType.DXA }, shading: { fill, type: ShadingType.CLEAR },
      margins: { top: 100, bottom: 100, left: 160, right: 160 }, children: kids
    })] })]
  });
}

// 通用表格：headers=[], rows=[[...]], widths=[]
function table(headers, rows, widths, opts = {}) {
  const total = widths.reduce((a, c) => a + c, 0);
  const mkCell = (txt, w, isHead, rowIdx) => {
    const runs = Array.isArray(txt)
      ? txt
      : [new TextRun({ text: String(txt), bold: isHead, color: isHead ? "FFFFFF" : INK, size: isHead ? 19 : 18 })];
    return new TableCell({
      borders: BORDERS, width: { size: w, type: WidthType.DXA },
      shading: { fill: isHead ? HEADFILL : (rowIdx % 2 ? ZEBRA : "FFFFFF"), type: ShadingType.CLEAR },
      margins: { top: 54, bottom: 54, left: 100, right: 100 },
      verticalAlign: VerticalAlign.CENTER,
      children: runs.length && runs[0] instanceof Paragraph ? runs : [new Paragraph({ children: runs, spacing: { after: 0, line: 250 } })]
    });
  };
  const headRow = new TableRow({ tableHeader: true, children: headers.map((h, i) => mkCell(h, widths[i], true, 0)) });
  const bodyRows = rows.map((r, ri) => new TableRow({ children: r.map((c, ci) => mkCell(c, widths[ci], false, ri)) }));
  return new Table({ width: { size: total, type: WidthType.DXA }, columnWidths: widths, rows: [headRow, ...bodyRows] });
}
function spacer(h = 80) { return new Paragraph({ children: [], spacing: { after: h } }); }

// =================================================================
// 文档主体
// =================================================================
const children = [];

// ---------- 封面 ----------
children.push(
  new Paragraph({ spacing: { before: 1400, after: 0 }, alignment: AlignmentType.CENTER,
    children: [new TextRun({ text: "百泉聚兴（北京）科技有限公司", bold: true, size: 30, color: BRAND })] }),
  new Paragraph({ spacing: { before: 80, after: 700 }, alignment: AlignmentType.CENTER,
    children: [new TextRun({ text: "SmartLabOS 智能实验室自动化系统", size: 22, color: "5B6B7E" })] }),
  new Paragraph({ alignment: AlignmentType.CENTER, spacing: { before: 200, after: 60 },
    children: [new TextRun({ text: "成都市食品检验研究院", bold: true, size: 40, color: INK })] }),
  new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 60 },
    children: [new TextRun({ text: "兽药残留专项自动化检测系统", bold: true, size: 32, color: INK })] }),
  new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 500 },
    children: [new TextRun({ text: "详细设计方案", bold: true, size: 48, color: BRAND })] }),
  new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 40 },
    children: [new TextRun({ text: "硝基呋喃 · 氯霉素 · 磺胺类 · 沙星类（喹诺酮）四类兽残 全流程自动化前处理", size: 20, color: "5B6B7E" })] }),
  new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 700 },
    children: [new TextRun({ text: "依据 GB 31658.17—2021 / GB/T 21981—2008", size: 20, color: "5B6B7E" })] }),
);
// 封面信息小表
children.push(new Table({
  width: { size: 6200, type: WidthType.DXA }, columnWidths: [2400, 3800],
  alignment: AlignmentType.CENTER,
  rows: [
    ["编制单位", "百泉聚兴（北京）科技有限公司"],
    ["文档类型", "详细设计方案（中标后 / 合同前 · 技术评审版）"],
    ["文档版本", "V1.0"],
    ["文档状态", "送审稿"],
    ["编制日期", "2026 年 6 月 30 日"],
    ["编制人 / 联系人", "SmartLabOS 智能方案助手 · 售前技术组"],
  ].map((r, i) => new TableRow({ children: r.map((c, ci) => new TableCell({
    borders: BORDERS, width: { size: ci ? 3800 : 2400, type: WidthType.DXA },
    shading: { fill: ci ? "FFFFFF" : ZEBRA, type: ShadingType.CLEAR },
    margins: { top: 60, bottom: 60, left: 120, right: 120 },
    children: [new Paragraph({ children: [new TextRun({ text: c, bold: ci === 0, size: 19 })] })]
  })) }))
}));
children.push(new Paragraph({ children: [new PageBreak()] }));

// ---------- 目录 ----------
children.push(new Paragraph({ children: [new TextRun({ text: "目  录", bold: true, size: 30, color: INK })], spacing: { after: 200 }, alignment: AlignmentType.CENTER }));
children.push(new TableOfContents("目录", { hyperlink: true, headingStyleRange: "1-3" }));
children.push(new Paragraph({ children: [new PageBreak()] }));

// =================================================================
// 0. 文档元信息
// =================================================================
children.push(H1("0  文档元信息"));
children.push(P([b("客户全称："), t("成都市食品检验研究院（成都市市场监督管理局直属正处级公益二类事业单位 · 国家食品复检机构）")]));
children.push(P([b("项目名称："), t("兽药残留专项自动化检测系统 —— SmartLabOS 全流程自动化前处理详细设计")]));
children.push(P([b("文档定位："), t("中标后 / 合同前的详细设计、技术评审与工程交底文件，颗粒度细化至参数、表格、交付物级别。")]));
children.push(H3("0.1 版本与状态"));
children.push(table(
  ["版本号", "状态", "编制日期", "完成日期", "编制 / 联系人"],
  [["V1.0", "送审稿", "2026-06-30", "2026-06-30", "百泉聚兴 SmartLabOS 售前技术组"]],
  [1500, 1700, 1900, 1900, 2360]
));
children.push(H3("0.2 配套内部文档索引"));
children.push(table(
  ["序", "文档", "对应内容", "说明"],
  [
    ["1", "1.html", "GB 31658.17—2021 解决方案", "四环素 / 磺胺 / 喹诺酮类兽残 提取 & 净化 前处理自动化方案（含逐平台耗时测算）"],
    ["2", "2.html", "GB/T 21981—2008 解决方案", "动物源食品激素多残留 提取（酶解）& 净化 前处理自动化方案"],
    ["3", "串接双柱SPE净化-模块-URS.html", "新建模组 URS", "ENVI-Carb 串接氨基柱双柱净化（MOD-SPE-D01），按指令 §5.3 输出"],
    ["4", "模块选定-推荐.json", "模块选定 → 确认记录", "已确认 11 模块清单与逐工序选型依据"],
    ["5", "Summary-输出总结.md", "阶段输出总结", "方案要点、模块映射、偏差与待确认事项汇总"],
    ["6", "WORD生成指令-20260630-160022.txt", "本文件生成指令", "提纲、数据来源、强制模块范围、输出路径约束"],
  ],
  [600, 3000, 2300, 3460]
));

// =================================================================
// 1. 项目概述
// =================================================================
children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(H1("1  项目概述"));

children.push(H2("1.1 客户与业务背景"));
children.push(P("成都市食品检验研究院系成都市市场监督管理局直属正处级公益二类事业单位，承担食品、食用农产品等的监督检验、风险监测与技术研究工作，是国家食品复检机构，牵头本地食用农产品抽检监测，并开展多兽药残留检测技术攻关。"));
children.push(P([b("机构资质与职能定位："), t("国家食品复检机构 · 监督检验 · 风险监测 · 食用农产品抽检监测牵头单位 · 兽残检测技术攻关。其检测结果具备法律效力与复检权威性，对前处理的回收率、重复性与可追溯性提出极高要求。")]));

children.push(H2("1.2 检测对象与目标物"));
children.push(P("本项目以多品类兽残高通量大筛查为核心，深度整合自动化前处理与上机检测全流程，可同步实现以下四大重点兽残的全自动化检测："));
children.push(table(
  ["目标物类别", "代表化合物 / 范围", "对应主标准", "检测仪器"],
  [
    ["硝基呋喃类", "AOZ、AMOZ、AHD、SEM 等代谢物（痕量、需衍生）", "GB 31658.17 体系 / 专项衍生法", "LC-MS/MS"],
    ["氯霉素类", "氯霉素、氟苯尼考、甲砜霉素", "GB 31658 系列", "LC-MS/MS"],
    ["磺胺类", "磺胺类 21 种", "GB 31658.17—2021", "LC-MS/MS"],
    ["沙星类（喹诺酮）", "喹诺酮类 16 种", "GB 31658.17—2021", "LC-MS/MS"],
    ["（同源激素扩展）", "雄 / 孕 / 皮质醇 / 雌激素 50 种", "GB/T 21981—2008", "LC-MS/MS（内标法）"],
  ],
  [1900, 3300, 2360, 1800]
));
children.push(P([b("样品基质类型："), t("以动物性食品 / 食用农产品为主 —— 牛、羊、猪、鸡的肌肉、肝脏、肾脏组织（GB 31658.17）；并兼容猪肉 / 肝 / 蛋 / 奶 / 牛肉 / 鸡 / 虾等基质（GB/T 21981 激素扩展）。")]));
children.push(P([b("目标物理化特性（决定前处理难度）："), t("四类兽残均为痕量残留，基质干扰极强；硝基呋喃代谢物需衍生、热敏感、易挥发降解；高脂高蛋白基质易分层、乳化；目标物在 SPE / 氮吹环节易损失。前处理是核心控险环节。")]));

children.push(H2("1.3 现状痛点深度分析"));
children.push(P("人工前处理实操难点突出且极具针对性，逐环节剖析如下（编号对应客户提出的挑战 1–6）："));
children.push(table(
  ["环节", "人工难点剖析", "对结果的影响机理", "对应挑战"],
  [
    ["全流程", "全程手工容错率极低，高度依赖人员熟练度，个体差异大", "操作偏差直接传导至回收率与重复性，结果有效性不可控", "挑战-1"],
    ["均质 / 取样", "样品均质易不均，高脂高蛋白基质分层，痕量取样偏差大", "取样不代表性 → 定量偏差大、重复性差（RSD 超限）", "挑战-2"],
    ["萃取", "萃取极易乳化，分层困难，目标物易损失", "相分离不清 → 目标物随乳化层损失，回收率偏低", "挑战-3"],
    ["SPE 净化", "柱活化 / 平衡 / 流速人工把控难，易柱干、堵塞、填料吸附", "流速失控 → 穿透或保留异常，回收率波动大", "挑战-4"],
    ["氮吹浓缩 / 定容", "控温控速难，热敏物易分解、目标物易挥发，定容精度不足", "蒸干过度 / 温度过高 → 目标物降解、损失；定容不准", "挑战-5"],
    ["基质净化整体", "基质杂质难除引发严重离子抑制，步骤繁琐", "离子抑制 → 灵敏度下降、定量失准、结果有效性受损", "挑战-6"],
  ],
  [1300, 3700, 3000, 1360]
));
children.push(P([b("人工操作问题量化："), t("容错率极低（任一步失误即需重做）、个体差异大（不同人员回收率离散）、重复性差（批内 / 批间 RSD 不稳定）、效率低（串行手工、人力占用高）、强熟练度依赖（培养周期长）。")]));

children.push(H2("1.4 项目场地概述"));
children.push(callout("说明（占位需现场补全）", [
  "改造前用途 / 楼层 / 房间：待现场踏勘确认（建议提供原始建筑平面图）。",
  "场地尺寸（长×宽、面积、层高）+ 原始结构尺寸图：待甲方提供 CAD 底图后补全。",
  "隔断打通 / 改造范围：依设备布局（见 §3.2）与公用工程改造（见 §3.3）现场核定。",
  [b("空间基线要求（来自设备包络）："), t("设备高 2055 mm → 层高 ≥ 3 m；地面承重 ≥ 500 kg/㎡；环境温度 15–30 ℃。方案一净占地约 4.76 ㎡、方案二约 5.10 ㎡，含通道按 2.5~3× 预留约 12~15 ㎡。")],
], WARNFILL));

// =================================================================
// 2. 自动化需求与标准适配
// =================================================================
children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(H1("2  自动化需求与标准适配"));
children.push(H2("2.1 设计依据国标 / 行标清单"));
children.push(table(
  ["标准号", "标准全称", "状态", "本项目用途"],
  [
    ["GB 31658.17—2021", "食品安全国家标准 动物性食品中四环素类、磺胺类和喹诺酮类药物残留量的测定 液相色谱-串联质谱法", "现行（2022-02-01 实施）", "磺胺 / 喹诺酮 / 四环素 兽残主标准（外标法），方案一选型主干"],
    ["GB/T 21981—2008", "动物源食品中激素多残留检测方法 液相色谱-质谱/质谱法", "现行", "激素多残留（内标法、含酶解、双柱串接净化）方案二依据"],
    ["GB/T 6682—2008", "分析实验室用水规格和试验方法", "现行", "纯水（一级水）供水规格依据"],
  ],
  [2100, 4400, 1500, 1360]
));
children.push(callout("数据来源与编造红线声明", [
  [b("全部技术参数均引用自 "), mono("references/01-modules"), t("（11 个已确认模块卡）、"), mono("02-platforms"), t("、"), mono("06-pallet"), t(" 及本项目已生成 HTML 提案；凡卡片字段缺失需工程估算之处，均明确标注「工程估算」，最终以现场 URS 实测为准，绝不编造。")],
  [b("模块选型严格限定于「模块选定 → 模块确认」后的 11 个已确认模块"), t("，未引入清单之外任何模块；确有工艺缺口者在 §4.4 / 附录 B 明确说明，不自行替换。")],
], OKFILL));

children.push(H2("2.2 检测标准与设备适配表"));
children.push(table(
  ["检测方法号", "检测项目", "前处理系统（SmartLabOS 产线）", "检测仪器"],
  [
    ["GB 31658.17—2021", "四环素 4 + 磺胺 21 + 喹诺酮 16 = 41 种", "提取 → SPE（HLB）净化 → 浓缩复溶 → 离心过滤（6 工作站）", "LC-MS/MS"],
    ["硝基呋喃 / 氯霉素专项", "AOZ/AMOZ/AHD/SEM、氯霉素类", "提取 →（衍生 / 恒温反应）→ 净化 → 浓缩 → 过滤", "LC-MS/MS"],
    ["GB/T 21981—2008", "激素多残留 50 种", "提取（酶解 37 ℃ 12 h）→ ENVI-Carb 串接氨基柱净化 → 浓缩 → 过滤（7 工作站）", "LC-MS/MS（内标法）"],
  ],
  [2000, 2500, 3500, 1360]
));

children.push(H2("2.3 自动化需求与通量目标"));
children.push(P("方案以「全自动化替代人工繁琐操作」为目标，覆盖样品自动均质、萃取、净化、浓缩、定容、进样等关键环节，全程自动上下料，兼顾高通量筛查与靶向定量。"));
children.push(table(
  ["检项 / 方案", "全流程串行单样节拍", "瓶颈环节", "高通量保障"],
  [
    ["方案一（GB 31658.17 兽残）", "≈ 96.8 min/样（5810 s，6 平台串行）", "WS5 浓缩蒸干（1860 s）", "超声 16 工位、SPE 2 路、离心 4 孔位并行，稳态通量远高于串行节拍"],
    ["方案二（GB/T 21981 激素）", "≈ 152 min/样（9120 s 在线，不含离线酶解）", "WS5 串接双柱 SPE（3060 s）", "酶解 24 孔离线过夜批处理，不占在线节拍"],
  ],
  [2600, 2900, 2100, 1760]
));
children.push(callout("流程范围与配置基线（来自客户需求 3.4–3.6）", [
  [b("流程范围："), t("采样 → 制样 → 分样 → 前处理 → 检测 → 出具报告（本方案自动化覆盖制样均质至进样过滤；称量与上机检测见 §4.4 范围说明）。")],
  [b("上下料方式："), t("自动上下料 —— 由平台 XYZ 机械臂在各模块间统一实现，耗时按模块卡标称 60 s/循环计入。")],
  [b("软件功能："), t("智慧实验室软件（SmartLabOS 中央控制 + 数字孪生），详见第 5 章。")],
], INFOFILL));

// =================================================================
// 3. 总体设计
// =================================================================
children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(H1("3  总体设计"));
children.push(H2("3.1 设计依据与原则"));
children.push(bullet([b("模块化："), t("以标准模块位（450×180×600 包络）+ 托盘位为最小单元，模块即插即用、按工艺自由组合，缩短建设与扩展周期。")]));
children.push(bullet([b("智能物流联动："), t("平台 XYZ 机械臂 + 工站间转运实现样品在容器链（50 mL 离心管 → 10 mL 西林瓶 → 2 mL 进样瓶）上的全自动流转。")]));
children.push(bullet([b("数字化管理："), t("中央控制系统统一调度、采集、追溯，对接 LIMS，实现样品全生命周期闭环。")]));
children.push(bullet([b("可扩展："), t("平台预留模组位 / 托盘位，可在线扩容并行通道或追加新工艺模组。")]));
children.push(P([b("业务闭环范围界定："), t("分样 → 制样均质 → 加缓冲液 →（加内标 / 衍生 / 酶解）→ 提取（涡旋 + 超声）→ 离心取上清 → SPE 净化 → 浓缩蒸干 → 复溶定容 → 离心过滤 → 进样 → 检测 → 报告。")]));

children.push(H2("3.2 整体布局设计"));
children.push(P("依据「各平台处理时间尽量相等、尽量接近 600 s」的口径划分功能平台（工作站），按前处理顺序线性串联，机械臂 / 转运衔接。平台 / 区域划分如下："));
children.push(table(
  ["区域 / 平台", "承载工艺", "采用平台型号", "占地"],
  [
    ["制样均质 · 加液涡旋", "组织均质 + 加缓冲液涡旋（含拆 / 合盖）", "PLT-800", "0.68 ㎡"],
    ["超声萃取", "16 工位多通道超声（TEC 恒温）", "PLT-800", "0.68 ㎡"],
    ["冷冻离心 · 上清合并", "高速冷冻离心 + 上清精密转移合并", "PLT-1200", "1.02 ㎡"],
    ["SPE 净化", "6cc HLB 自动上样 SPE（2 路并行）", "PLT-800", "0.68 ㎡"],
    ["浓缩蒸干", "真空 + 离心 + 45 ℃ 热风连续浓缩", "PLT-800", "0.68 ㎡"],
    ["复溶 · 离心 · 过滤", "复溶混匀 + 高速离心 + 0.22 µm 过滤", "PLT-1200", "1.02 ㎡"],
    ["（方案二）恒温酶解", "24 孔位恒温振摇酶解（离线批处理）", "PLT-800", "0.68 ㎡"],
    ["（方案二）串接双柱 SPE", "ENVI-Carb 串接氨基柱（新建 MOD-SPE-D01）", "PLT-800", "0.68 ㎡"],
    ["中央控制 / 数字孪生", "调度服务器 + 可视化大屏", "机柜 / 工作站", "按现场"],
    ["智能线边仓库 / 物流", "试剂耗材暂存 + 机械臂转运（可选扩展）", "PLT-1400 等", "按现场"],
  ],
  [2500, 3300, 2200, 1360]
));
children.push(P([b("平台选型逻辑（来自 02-platforms）："), t("一个平台最多搭载 6 模组 + 8 托盘。含 50 mL 离心模组 "), mono("MOD-LX-001"), t("（占 3 模组位）的工站须用 PLT-1200（6 位）；其余 ≤3 位工站用占地最省的 PLT-800；如需延长无人运行可将浓缩 / 过滤工站升级 PLT-1400 扩展托盘缓存。")]));
children.push(table(
  ["平台", "尺寸 (mm)", "模组位", "托盘位", "布局", "占地", "本方案采用"],
  [
    ["PLT-800", "800×850×2055", "3", "4", "1×4", "0.68 ㎡", "WS1/WS2/WS4/WS5"],
    ["PLT-1200", "1200×850×2055", "6", "8", "2×4", "1.02 ㎡", "WS3/WS6（含离心 3 位）"],
    ["PLT-1400", "1400×850×2055", "6", "8", "1×8", "1.19 ㎡", "○ 可选（扩展托盘缓存）"],
  ],
  [1100, 1900, 900, 900, 800, 1100, 2660]
));
children.push(callout("布局合理性与标准化预装", [
  "操作便捷：工站按工艺顺序线性排列，样品单向流转、无回流交叉，机械臂行程最短。",
  "空间利用率：以 PLT-800 为主力，仅离心 / 大托盘缓存工站用 PLT-1200/1400，净占地最省。",
  [b("标准化预装与现场拼接："), t("各平台在工厂完成模组装配与联调（FAT），现场仅做平台拼接与系统联调，大幅缩短建设周期（平台标准交付约 8 周、工站集成约 12 周，供参考）。")],
  [b("布局图占位："), t("「智能化实验室规划布局图」须在现场 CAD 底图确定后出图（占位 → 实图，见附录 A）。")],
], OKFILL));

children.push(H2("3.3 配套基础设施改造设计"));
children.push(P("依据各模块卡 electrical_environment 字段，配套改造需求如下（详图待现场底图确定后出施工图，占位见附录 A）："));
children.push(table(
  ["系统", "改造设计要点", "依据（模块字段）"],
  [
    ["管道排风系统", "WS1/WS4/WS5 上方设万向抽气罩 / 防爆排风，罩口面风速 ≥ 0.5 m/s；排风经活性炭 / 喷淋处理达标后高空排放；方案二二氯甲烷工站强制防爆排风", "提取涉乙腈，净化涉甲醇 / 乙酸乙酯 / 浓氨水 / 甲酸 + 45 ℃ 浓缩挥发；LX-001 requires_ventilation"],
    ["气路分布系统", "洁净干燥压缩空气 0.4–0.7 MPa（WX-006/SPE-004/NS-001/HY-001/GL-002 正压）；真空泵 −80~−95 kPa（LX-001/NS-001）；压缩空气需除水除油", "requires_compressed_air / vacuum_kpa"],
    ["强弱电与网络", "主电 220 V（CS-004/LX-001 外设/NS-001 加热/FY-002 恒温）+ 控制电 24 V；NS-001 另需 48 V；独立配电箱 + 漏电 / 接地保护；建议 3 kVA 在线式 UPS；工业以太网至各平台与中央控制", "voltage_v / power_kva"],
    ["给排水 / 纯水管路", "上水：一级纯水（GB/T 6682）供 SPE 活化 / 淋洗、均质清洗、复溶；下水：JZ-002/CS-004/DR-003/SPE-004/NS-001/GL-002 均接独立排废管路；有机废液与氨性废液分类收集至危废中转罐；地面设地漏 / 防渗托盘", "requires_water / requires_drain"],
  ],
  [1700, 5200, 2460]
));

// =================================================================
// 4. 功能平台逐一详细设计
// =================================================================
children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(H1("4  功能平台逐一详细设计"));
children.push(P([t("本章对每一个已配置功能平台逐一展开"), b("【平台组成 + 平台功能说明 + 关键技术指标表】"), t("三要素。指标均量化并含通讯协议；模组参数逐条引用自 "), mono("references/01-modules"), t(" 卡片，单循环耗时取自 "), mono("process_capability.cycle_time_sec"), t(" 与 "), mono("module_up_unload_time"), t("（自动上下料 60 s）。")]));
children.push(callout("通讯协议统一基线", [
  [t("各模组 / 平台均经平台 PLC 汇聚后以 "), b("MODBUS TCP"), t(" 接入 SmartLabOS 中央调度（参见 "), mono("MOD-SPE-004"), t(" 卡片明示「仅支持 MODBUS TCP」，全线统一）；中央控制对上层提供 RESTful API / HTTPS 与 LIMS 双向对接（见 §5.3）。")],
], INFOFILL));

// 平台模板函数
function platform(title, sub, composition, funcLines, indHeaders, indRows, indWidths, extra = []) {
  children.push(H2(title));
  children.push(P([new TextRun({ text: sub, italics: true, color: "5B6B7E", size: 18 })]));
  children.push(H3("平台组成"));
  for (const c of composition) children.push(bullet(c));
  children.push(H3("平台功能说明"));
  for (const f of funcLines) children.push(P(f));
  children.push(H3("关键技术指标表"));
  children.push(table(indHeaders, indRows, indWidths));
  for (const e of extra) children.push(e);
}

const IND_H = ["模组 ID", "设备名称", "关键系统指标（数值 + 单位 + 精度）", "通讯"];
const IND_W = [1300, 2200, 5160, 700];

// ---- WS1 ----
platform(
  "4.1 制样均质 · 加液涡旋平台（WS-VETDRUG-Homog-CDSJY-01）",
  "平台 PLT-800（800×850×2055 mm，3 模组位 / 4 托盘位，0.68 ㎡）· 覆盖 §7 制样均质 + §8.1 加缓冲液涡旋 · 模块位 2/3 · 平台处理耗时 480 s",
  [
    [mono("MOD-JZ-002"), t(" 50 ml 离心管均质模块 —— 组织高剪切均质（1 模组位）")],
    [mono("MOD-WX-006"), t(" 50 ml 离心管正压加液涡旋模块 —— 拆盖 + 加 Mcllvaine Na₂-EDTA 缓冲液 8 mL + 涡旋 1 min + 合盖（1 模组位）")],
    [t("配套托盘：50 ml 离心管托盘（供试样，18 位/盘）、缓冲液 / 废液托盘")],
  ],
  [
    [t("流程：上样 → "), mono("JZ-002"), t(" 高剪切均质（标准化打碎匀浆，规避高脂高蛋白基质分层、痕量取样不均，回应挑战-2）→ "), mono("WX-006"), t(" 拆盖、正压加缓冲液 8 mL、涡旋 1 min、合盖（一体化规避萃取乳化与人工加液偏差，回应挑战-3）→ 转 WS2 超声。")],
  ],
  IND_H,
  [
    [mono("MOD-JZ-002"), "50 ml 离心管均质模块", "容量 1–40 mL；转速 1000–30000 rpm；单循环 60 s；适配 50 mL 尖底管；正压 1–4 bar；24 V / 0.2826 kVA（峰值 0.471）；噪声 87 dB", "PLC/\nMODBUS TCP"],
    [mono("MOD-WX-006"), "50 ml 离心管正压加液涡旋模块", "容量 1–50 mL；涡旋 1–3000 rpm；加液绝对精度 ±1%、重复 ±0.5%；含拆 / 合盖；单循环 300 s；正压 1–4 bar；24 V / 0.228 kVA", "PLC/\nMODBUS TCP"],
  ],
  IND_W
);

// ---- WS2 ----
platform(
  "4.2 超声萃取平台（WS-VETDRUG-Ultrasound-CDSJY-02）",
  "平台 PLT-800 · 覆盖 §8.1 超声 20 min · 16 工位并行（托盘位）· 平台处理耗时 1260 s（单样均摊 ≈ 79 s）",
  [
    [mono("MOD-CS-004"), t(" 50 ml 离心管多通道超声模块 —— 16 工位并行、TEC 恒温（占 0 模组位，托盘位）")],
    [t("配套托盘：50 ml 离心管托盘（16 工位超声槽 + 缓存）")],
  ],
  [
    [t("流程：WS1 离心管放入 16 工位超声槽 → 超声 20 min（TEC 恒温 25–60 ℃ + 冷却循环水）→ 吸干瓶底残水 → 转 WS3 离心。超声为标准强制 20 min 长时操作，单批不可压缩；")],
    [mono("MOD-CS-004"), t(" 16 工位并行将单样均摊降至 ≈ 79 s，节拍由并行批量摊薄。")],
  ],
  IND_H,
  [
    [mono("MOD-CS-004"), "50 ml 离心管多通道超声模块", "容量 1–50 mL；16 工位并行；TEC 控温 25–60 ℃；卡片标称单循环 300 s（本标准按 20 min = 1200 s 取值）；220 V / 0.23 kVA；噪声 72 dB；冷却循环水", "PLC/\nMODBUS TCP"],
  ],
  IND_W
);

// ---- WS3 ----
platform(
  "4.3 冷冻离心 · 上清合并平台（WS-VETDRUG-Centrifuge-CDSJY-03）",
  "平台 PLT-1200（1200×850×2055 mm，6 模组位 / 8 托盘位，1.02 ㎡）· 覆盖 §8.1 离心 + 上清合并 / 残渣重复提取 · 模块位 4/6 · 平台处理耗时 690 s",
  [
    [mono("MOD-LX-001"), t(" 50 ml×4 孔位高速离心机模块（带 XYZ）—— 冷冻离心固液分离（占 3 模组位）")],
    [mono("MOD-DR-003"), t(" 注射器定容模块 —— 上清精密转移 / 两次提取液合并（占 1 模组位）")],
    [t("配套托盘：50 ml 离心管托盘（离心 18 位/盘）、50 ml 离心管托盘（合并接收）")],
  ],
  [
    [mono("LX-001"), t(" 冷冻离心 5 min → "), mono("DR-003"), t(" 取上清；残渣回 WS1 加磷酸盐缓冲液重复提取 1 次后再离心 → 两次上清合并 → 转 WS4。")],
    [new TextRun({ text: "偏差提示：标准 §8.1 要求 −2 ℃ / 10000 r/min，MOD-LX-001 为 2–7 ℃ / ≤8000 rpm，详见附录 B 偏差说明。", bold: true, color: "9A5A08" })],
  ],
  IND_H,
  [
    [mono("MOD-LX-001"), "50 ml×4 孔位高速离心机（带 XYZ）", "4 孔位；转速 1–8000 rpm；控温 2–7 ℃；真空 −80 kPa；单循环 0–300 s；外形 800×850×2050；24 V / 3.5 kVA（峰值 4.8）；需真空泵 + 压缩机 + 通风", "PLC/\nMODBUS TCP"],
    [mono("MOD-DR-003"), "注射器定容模块（250 / 1000 µL）", "量程 0.25–1 mL；绝对精度 ±0.6%、重复 ≤0.2%；适配 25/100/250/1000 µL 注射器；自动洗针；单循环 270 s；24 V / 0.138 kVA", "PLC/\nMODBUS TCP"],
  ],
  IND_W
);

// ---- WS4 ----
platform(
  "4.4 SPE 净化平台（WS-VETDRUG-SPE-CDSJY-04）",
  "平台 PLT-800 · 覆盖 §8.2 活化 → 上样 → 淋洗 → 抽干 → 洗脱 · 模块位 1/3（余位可扩并行）· 平台处理耗时 420 s",
  [
    [mono("MOD-SPE-004"), t(" 6cc 自动上样 SPE 模块 —— 2 路并行、注射泵流速闭环、6cc HLB 柱（占 1 模组位）")],
    [t("配套托盘：6CC SPE 柱托盘（24 位/盘）、50 ml 离心管托盘（上样提取液）、10 ml 西林瓶托盘（洗脱接收）")],
  ],
  [
    [t("甲醇 5 mL + 水 5 mL 活化 → 上样 → 水 + 20% 甲醇水淋洗 → 抽干 → 洗脱液 10 mL 洗脱入 10 mL 西林瓶 → 转 WS5。单柱循环 360 s，2 路并行；注射泵微升级流速闭环（0.1–15 mL/min）直接回应客户 SPE 柱干 / 堵塞 / 流速失控痛点（挑战-4）。")],
    [new TextRun({ text: "完全满足项：标准 HLB 200 mg / 6 mL ↔ MOD-SPE-004 适配 6cc HLB 柱、流速闭环，柱型与流速控制一致。", bold: true, color: "1F7A4A" })],
  ],
  IND_H,
  [
    [mono("MOD-SPE-004"), "6cc 自动上样 SPE 模块", "适配 6cc HLB 柱（换载具兼容 3cc）；移液 1–50 mL；流速 0.1–15 mL/min；计量精度 ±1%、重复 ±1%；2 路并行；正压 1–4 bar；8 步可编程；单循环 360 s；24 V / 0.168 kVA；需空压机", "MODBUS\nTCP"],
  ],
  IND_W
);

// ---- WS5 ----
platform(
  "4.5 浓缩蒸干平台（WS-VETDRUG-Concentrate-CDSJY-05）",
  "平台 PLT-800 · 覆盖 §8.2 45 ℃ 氮吹干 · 双工位连续 · 平台处理耗时 1860 s（含工程估算）",
  [
    [mono("MOD-NS-001"), t(" 10 ml 大容量连续浓缩模块 —— 真空 + 离心 + 45–50 ℃ 热风浓缩蒸干至 10 mL 西林瓶（占 2 模组位）")],
    [t("配套托盘：10 ml 西林瓶托盘（浓缩接收，28 位/盘）")],
  ],
  [
    [t("洗脱液 10 mL → "), mono("NS-001"), t(" 真空 −95 kPa + 离心贴壁 + 45–50 ℃ 热风浓缩蒸干至近干，红外测温 + 流量 / 温度 / 真空度曲线判断终点，控温防热敏物分解、规避目标物挥发损失（回应挑战-5）→ 转 WS6 复溶。")],
    [new TextRun({ text: "耗时取值：MOD-NS-001 卡片 cycle_time_sec 为空，按标准「45 ℃ 氮气吹干」工程估算 1800 s（30 min），非卡片实测值，最终以现场 URS 测试为准（见附录 B）。", bold: true, color: "9A5A08" })],
  ],
  IND_H,
  [
    [mono("MOD-NS-001"), "10 ml 大容量连续浓缩模块", "蒸干至 10 mL 西林瓶（兼容 50/100/200 mL 尖 / 圆底瓶）；真空 −95 kPa；离心 5000–8000 rpm；控温 45–50 ℃（红外测温 + 曲线判终点）；正压 1–4 bar；单循环工程估算 1800 s；48 V / 0.38 kVA；需真空泵 + 空压机", "PLC/\nMODBUS TCP"],
  ],
  IND_W
);

// ---- WS6 ----
platform(
  "4.6 复溶 · 离心 · 过滤平台（WS-VETDRUG-Reconst-CDSJY-06）",
  "平台 PLT-1200 · 覆盖 §8.2 复溶 → 离心 → 过滤 · 模块位 5/6 · 平台处理耗时 1100 s",
  [
    [mono("MOD-HY-001"), t(" 10 ml 西林瓶加液混匀模块 —— 复溶液 1.0 mL 精密加注 + 涡旋溶解（占 1 模组位）")],
    [mono("MOD-LX-001"), t(" 50 ml×4 孔位高速离心机模块 —— 复溶液澄清高速离心 5 min（占 3 模组位）")],
    [mono("MOD-GL-002"), t(" 5cc 针式过滤模块 —— 0.22 µm 针式过滤至进样小瓶（占 1 模组位）")],
    [t("配套托盘：10 ml 西林瓶托盘（复溶）、15 ml 离心管托盘（离心 32 位/盘）、13 mm 过滤器托盘（40 位/盘）+ 2 ml 进样小瓶托盘")],
  ],
  [
    [mono("HY-001"), t(" 加复溶液 1.0 mL 涡旋溶解 → "), mono("LX-001"), t(" 高速离心 → "), mono("GL-002"), t(" 0.22 µm 过滤至进样小瓶 → LC-MS/MS。")],
    [new TextRun({ text: "偏差提示：标准复溶离心 14000 r/min，MOD-LX-001 ≤8000 rpm，详见附录 B。", bold: true, color: "9A5A08" })],
  ],
  IND_H,
  [
    [mono("MOD-HY-001"), "10 ml 西林瓶加液混匀模块", "容量 1–10 mL（灌装 ≤8.5 mL）；加液绝对 ±1%、重复 ±0.5%；混匀 0–3000 rpm；正压 1–4 bar；单循环 100 s；24 V / 0.13 kVA", "PLC/\nMODBUS TCP"],
    [mono("MOD-LX-001"), "50 ml×4 孔位高速离心机（带 XYZ）", "转速 1–8000 rpm；控温 2–7 ℃；真空 −80 kPa；单循环 0–300 s（同 §4.3）", "PLC/\nMODBUS TCP"],
    [mono("MOD-GL-002"), "5cc 针式过滤模块", "样品 1–5 mL；13 mm 针式过滤器（适配 0.22 µm 尼龙膜）；含预过滤 / 洗针 / 空吹；单循环 520 s；24 V / 0.132 kVA；需压缩空气", "PLC/\nMODBUS TCP"],
  ],
  IND_W
);

// ---- WS（方案二附加）----
children.push(H2("4.7 方案二附加平台（GB/T 21981 激素 · 差异工序）"));
children.push(P([t("方案二在共用上述提取 / 浓缩 / 过滤模组基础上，增加恒温酶解与双柱串接净化两类差异工序，所用模组仍取自已确认清单（恒温酶解 "), mono("MOD-FY-002"), t("、加水混匀 "), mono("MOD-JY-017"), t("、加内标 "), mono("MOD-DR-003"), t("）；双柱串接 SPE 构成模组缺口，按指令 §5.3 新建 URS（见附录 C），"), b("不以清单外模组替代"), t("。")]));
children.push(H3("关键技术指标表（方案二附加模组）"));
children.push(table(IND_H, [
  [mono("MOD-FY-002"), "5 ml 离心管恒温振摇反应模块", "24 孔位并行；控温 25–90 ℃（卡片正文标注精度 ±3 ℃）；平摇 0–300 rpm；遮光盖避光；单循环 3600 s（酶解按标准 12 h 离线批处理）；220 V / 0.564 kVA；发热", "PLC/\nMODBUS TCP"],
  [mono("MOD-JY-017"), "125 ml PP 瓶拆盖正压加液混匀模块", "容量 1–100 mL（单次加液 ≤50 mL）；混匀 0–1000 rpm；加液绝对 ±5%、重复 ±0.5%；含拆 / 装盖、15° 倾斜移液；单循环 70 s；24 V / 0.24 kVA", "PLC/\nMODBUS TCP"],
  [mono("MOD-SPE-D01"), "串接双柱 SPE 净化模块（新建 · URS）", "ENVI-Carb 500 mg/6 mL 串接氨基柱 500 mg/6 mL；上样流速 2–3 mL/min（闭环 ±5%）；上样体积 1–150 mL；洗脱计量 ≤±2%；抽干 ≤−80 kPa；单循环估算 3000 s + 上下料 60 s", "MODBUS\nTCP"],
], IND_W));

children.push(H2("4.8 各模块参数核对清单（指令 §4.2 对照）"));
children.push(P("下表逐项核对模板 §4.2 要求覆盖的参数项是否已量化给出，并标注本项目已确认清单的覆盖与缺口情况："));
children.push(table(
  ["参数项类别", "本方案覆盖情况"],
  [
    ["移液模块（精度 / 量程 / 安全检测）", "✅ MOD-DR-003：±0.6%/≤0.2%、0.25–1 mL、自动洗针防交叉污染"],
    ["加液涡旋模块（体积 / 精度 / 转速 / 时长）", "✅ MOD-WX-006：1–50 mL、±1%/±0.5%、1–3000 rpm、含拆合盖"],
    ["均质模块（频率 / 时长）", "✅ MOD-JZ-002：1000–30000 rpm、60 s"],
    ["振荡 / 恒温反应模块（温度 / 频率 / 时长）", "✅ MOD-FY-002：25–90 ℃、0–300 rpm、24 孔位"],
    ["离心模块（转速 / 温控 / 兼容管型 / 不平衡）", "✅ MOD-LX-001：≤8000 rpm、2–7 ℃、50 mL×4、上料需均衡（防不平衡）"],
    ["SPE 模块（流速 / 体积 / 柱兼容 / 八步可编程）", "✅ MOD-SPE-004：0.1–15 mL/min、1–50 mL、6cc HLB、可编程"],
    ["浓缩模块（转速 / 温度 / 控温精度）", "✅ MOD-NS-001：5000–8000 rpm、45–50 ℃、真空 −95 kPa（耗时工程估算）"],
    ["过滤模块（孔径 / 体积 / 压力）", "✅ MOD-GL-002：13 mm / 0.22 µm、1–5 mL"],
    ["拆 / 封盖模块（容器 / 时间）", "✅ 集成于 MOD-WX-006 / MOD-JY-017（拆合盖一体）"],
    ["搬运 / 机械臂、电动夹爪、扫码识别、智能仓库、恒温衍生、低温暂存", "○ 按需：平台 XYZ 机械臂自带搬运 / 夹持；扫码识别、智能仓库、低温暂存为可选扩展（11 模块清单内无独立卡片，见 §4.9 裁剪说明）"],
    ["称量模块", "✘ 缺口：清单内无称量模组，1 g 试样按行业惯例人工称样（见附录 B）"],
  ],
  [3400, 5960]
));

children.push(H2("4.9 辅助 / 可选设备与裁剪说明（○ 按需）"));
children.push(bullet([b("纯水制备系统："), t("○ 按需 —— 供一级纯水（GB/T 6682），产水能力按峰值用水核定；报价请联系销售团队。")]));
children.push(bullet([b("UPS："), t("○ 按需 —— 建议 3 kVA 在线式，续航覆盖安全停机；报价请联系销售团队。")]));
children.push(bullet([b("智能仓库 / 扫码识别 / 低温暂存 / 恒温衍生："), t("○ 按需裁剪 —— 本期 11 个已确认模块清单内无对应独立模块卡，故本版不展开三要素；如客户确认纳入，将另行补充选型与 URS。硝基呋喃衍生工序可由 MOD-FY-002 恒温反应承接。")]));
children.push(bullet([b("称量单元："), t("○ 按需 —— 如需全自动称量须外接称量模块；本期人工称样。")]));

// =================================================================
// 5. 软件系统详细设计
// =================================================================
children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(H1("5  软件系统详细设计"));
children.push(P("软件层为 SmartLabOS 智慧实验室软件，由中央控制系统与数字孪生 / 可视化系统两大子系统构成，对应客户需求 3.6「智慧实验室软件」。"));

children.push(H2("5.1 中央控制系统"));
children.push(H3("5.1.1 技术架构"));
children.push(bullet([b("分层架构："), t("设备层（PLC / 模组）→ 控制层（平台调度）→ 业务层（流程 / 样品 / 仓储）→ 展示层（B/S 大屏 / 客户端）四层。")]));
children.push(bullet([b("部署方式："), t("B/S 架构，服务端集中部署（本地服务器 / 私有云），浏览器免安装访问；设备侧经工业以太网 + MODBUS TCP 接入。")]));
children.push(bullet([b("系统架构图："), t("占位 → 实图（见附录 A）。")]));
children.push(H3("5.1.2 功能模块清单"));
children.push(table(
  ["功能模块", "核心职责"],
  [
    ["基础数据", "检测标准库、实验流程库、模组 / 平台台账、容器 / 托盘字典"],
    ["销售 / 项目管理", "项目立项、需求登记（可选）"],
    ["样品管理", "样品全生命周期：采样前 / 中 / 后，申请-审批-确认-记录闭环"],
    ["仓库管理", "试剂耗材库存、物料调度、补料提醒"],
    ["实验流程管理", "标准流程模板编排、参数下发"],
    ["实验调度", "多工站节拍调度、并行 / 分时复用、瓶颈均衡"],
    ["设备管理与自动化执行", "单机执行、保养提醒、故障报警、正反向追溯"],
    ["数字化大屏", "产能 / 状态 / 预警实时可视化"],
  ],
  [2600, 6760]
));
children.push(H3("5.1.3 自动化功能"));
children.push(bullet("实验室调度控制：按检测标准库 / 流程库自动编排工站节拍，多模块并行与分时复用，瓶颈（浓缩 / SPE）均衡。"));
children.push(bullet("单机设备执行：逐模组功能码下发与状态回采，自动上下料由平台 XYZ 机械臂统一执行。"));
children.push(bullet("仓库管理与物料调度：库存监测、补料提醒、试剂耗材与样品的物流路径规划。"));
children.push(bullet("检测标准库与实验流程库：内置 GB 31658.17 / GB/T 21981 等方法流程，参数化复用。"));
children.push(bullet("样品全生命周期管理：采样前 / 中 / 后全程，申请-审批-确认-记录闭环，条码贯穿。"));
children.push(bullet("设备台账与运维：保养提醒、故障报警、正向（样品→结果）与反向（结果→样品）追溯。"));
children.push(H3("5.1.4 数据管理功能"));
children.push(bullet("实验数据抓取与传输：多通道实时采集、过程数据与结果数据分层存储。"));
children.push(bullet([b("LIMS 双向对接："), t("通过 RESTful API / HTTPS 与院内 LIMS 双向交换样品信息与检测结果（接口与数据流向见 §5.3）。")]));
children.push(bullet("通风 / 有害气体监测联动：异常触发排风 / 停机联锁。"));
children.push(bullet("数据清洗、审查与质控校验：多通道采集校验、异常判定、标曲 R² 校验。"));
children.push(bullet("实时存储与多维查询：按样品 / 批次 / 设备 / 时间多维检索。"));
children.push(H3("5.1.5 其他功能"));
children.push(bullet("智能监测分析与预警；报表与统计（设备使用、检测次数、试剂用量、异常判定、原始记录 / 报告导出）。"));
children.push(bullet("报告定制与一键生成（模板定制、多级审核）；数据导出 Word / Excel / PDF。"));
children.push(bullet("角色与权限管理（分级 + 自定义角色）；报警管理（实时 / 历史、规则配置、多通道通知）。"));
children.push(bullet("数据安全：冗余容错、传输 / 存储加密、权限隔离。"));

children.push(H2("5.2 数字孪生 / 可视化系统（○ 按需，高端项目建议配置）"));
children.push(bullet("系统组成：中央控制可视化、试剂耗材管理、物流与设备运行监控（示例图占位，见附录 A）。"));
children.push(bullet("实时数据展示与监控；实验过程可视化（视频流 / 多视角 / 录制）。"));
children.push(bullet("数据分析与决策支持；预警与应急响应；成果展示与参观交互。"));

children.push(H2("5.3 软件系统技术指标"));
children.push(table(
  ["指标类别", "具体指标"],
  [
    ["开放与集成", "RESTful API + HTTPS；设备侧 MODBUS TCP；与 LIMS / 工厂信息化双向对接（样品信息下行、检测结果上行）"],
    ["安全性", "容错冗余、可扩充；操作日志完备；关键功能冗余切换"],
    ["可靠性", "成熟案例支撑；7×24 连续运行；集中-分散控制结合；年故障累计时间设上限（以合同 SLA 约定）"],
    ["可扩展性", "硬件在线扩容；软件免定制配置适配新流程；升级简便"],
    ["日志记录", "操作日志含人 / 时 / 动作 / 对象要素；保存时长可设；到期自动清理"],
  ],
  [1900, 7460]
));

// =================================================================
// 6. 自动化产能效率
// =================================================================
children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(H1("6  自动化产能效率"));
children.push(H2("6.1 效率设计逻辑"));
children.push(P("效率口径（§5.2）：每平台处理耗时 = Σ 该平台所载模组（功能码单循环耗时 + 上下料 60 s）；解决方案总耗时 = Σ 各平台耗时。多模块并行（超声 16 工位、SPE 2 路、离心 4 孔位）、全流程联动与节拍匹配使稳态通量远高于串行单样节拍。"));

children.push(H2("6.2 方案一（GB 31658.17 兽残）逐平台耗时"));
children.push(P("样品：牛 / 羊 / 猪 / 鸡 肌肉·肝·肾，41 种目标物，自动上下料。"));
children.push(table(
  ["平台 / 工作站", "主模组", "功能码耗时 (s)", "上下料 (s)", "平台耗时 (s)", "≈ min"],
  [
    ["WS1 制样均质·加液", "JZ-002 + WX-006", "60 / 300", "60 / 60", "480", "8.0"],
    ["WS2 超声萃取", "CS-004（16 工位）", "1200", "60", "1260", "21.0"],
    ["WS3 冷冻离心·合并", "LX-001 + DR-003", "300 / 270", "60 / 60", "690", "11.5"],
    ["WS4 SPE 净化", "SPE-004", "360", "60", "420", "7.0"],
    ["WS5 浓缩蒸干 ★", "NS-001（估算）", "1800★", "60", "1860", "31.0"],
    ["WS6 复溶·离心·过滤", "HY-001+LX-001+GL-002", "100/300/520", "60×3", "1100", "18.3"],
    [b("解决方案总节拍"), b("6 平台串行"), b("—"), b("—"), b("5810"), b("≈ 96.8")],
  ],
  [2300, 2300, 1500, 1300, 1160, 800]
));
children.push(callout("取值与口径说明（★ / ※）", [
  [b("※ 超声："), t("卡片标称 300 s，标准强制超声 20 min，按 1200 s 取值；不可分长时操作，16 工位并行单样均摊 ≈ 79 s。")],
  [b("★ 浓缩："), t("MOD-NS-001 卡片 cycle_time_sec 为空，按 45 ℃ 氮气吹干工程估算 1800 s，非实测值，以现场 URS 为准。")],
  [t("上表为串行单样节拍；实际生产各工站流水重叠、多工位并行，稳态通量远高于此（瓶颈为 WS5 浓缩）。残渣重复提取 1 次使提取段实际可达约 1.7×，此处按单轮计供横向比较。")],
], WARNFILL));

children.push(H2("6.3 方案二（GB/T 21981 激素）逐平台耗时"));
children.push(P("样品：猪肉 / 肝 / 蛋 / 奶 / 牛肉 / 鸡 / 虾，50 种激素，内标法。"));
children.push(table(
  ["工作站", "主模组", "平台耗时 (s)", "≈ min", "备注"],
  [
    ["WS1 制样·加内标", "JZ-002+WX-006+DR-003", "810", "13.5", ""],
    ["WS2 恒温酶解 ◆", "FY-002（24 孔）", "43260◆", "—", "12 h 离线过夜批处理，不计入在线节拍"],
    ["WS3 甲醇超声", "WX-006+CS-004", "1860", "31.0", ""],
    ["WS4 离心·加水混匀", "LX-001+JY-017", "790", "13.2", ""],
    ["WS5 串接双柱 SPE ★", "MOD-SPE-D01（新建）", "3060★", "51.0", "上样 ~135 mL @ 2–3 mL/min 主导"],
    ["WS6 浓缩蒸干 ★", "NS-001（估算）", "1860★", "31.0", ""],
    ["WS7 复溶·过滤", "HY-001+GL-002", "740", "12.3", ""],
    [b("在线总节拍（不含离线酶解）"), b("—"), b("9120"), b("≈ 152"), b("")],
  ],
  [2500, 2300, 1300, 900, 2360]
));

children.push(H2("6.4 专项 / 复合检测与提升对比"));
children.push(bullet([b("专项 / 痕量（硝基呋喃含衍生）："), t("在方案一提取-净化主线基础上增加恒温衍生 / 反应环节（MOD-FY-002 承接），衍生为恒温长时操作，按离线 / 分时复用编排，避免占用在线瓶颈。")]));
children.push(bullet([b("多检项复合："), t("磺胺 / 喹诺酮 / 四环素同主线并行；激素经酶解 + 双柱净化差异分支处理，模块分时复用，整体以中央调度均衡节拍。")]));
children.push(bullet([b("效率提升对比（vs 人工，定性结论 + 可量化方向）："), t("全程自动上下料替代人工繁琐操作，规避均质不均 / 萃取乳化 / SPE 偏差 / 氮吹损失等痛点；人力由多人手工串行降为以系统为主、人工辅助上下样；错误率与个体差异显著降低，回收率与重复性稳定性提升。具体提升幅度与 N→M 人力数须经现场基线测试量化（建议作为验收 KPI）。")]));

// =================================================================
// 7. 项目实施与管理方案
// =================================================================
children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(H1("7  项目实施与管理方案"));
children.push(H2("7.1 周期与组织"));
children.push(P([b("总建设周期："), t("约 12 周（平台标准交付约 8 周、工站集成约 12 周，引自平台 / 模组卡 lead_time_weeks，供参考；最终以合同为准）。")]));
children.push(table(
  ["角色", "职责"],
  [
    ["乙方项目经理", "总协调、进度 / 质量 / 沟通 / 变更总控，对甲方单一接口"],
    ["乙方技术团队", "工艺与软件实现、FAT / SAT 调试、URS / 偏差处理"],
    ["乙方质控团队", "全流程质控节点把关、归档、PQ 验证"],
    ["乙方售后工程师", "前置参与调试，承接 7×24 远程 + 现场响应"],
    ["甲方对接小组", "需求确认、现场配合、验收、数据 / LIMS 对接配合"],
  ],
  [2300, 7060]
));

children.push(H2("7.2 实施阶段"));
children.push(table(
  ["阶段", "工期", "核心任务", "交付物", "验收标准 / 管控要点"],
  [
    ["一 调研细化与方案确认", "1–2 周", "现场踏勘、URS 确认、布局 / 偏差对齐", "确认版详细设计、URS、布局图", "甲方书面确认；偏差项达成一致"],
    ["二 模块生产与工厂预装调试", "4–6 周", "模组生产、平台预装、工厂联调 FAT", "FAT 报告、设备合格证", "FAT 通过；指标达标"],
    ["三 现场基础施工与复核", "并行 1–2 周", "排风 / 气路 / 强弱电 / 给排水改造", "施工图、隐蔽工程记录", "公用工程复核合格"],
    ["四 现场安装与模块拼接", "1 周", "平台就位、拼接、管线对接", "安装 / 拼接验收单", "就位精度、接口连通"],
    ["五 系统调试与人员培训", "1–2 周", "SAT 联调、流程跑通、培训", "SAT 报告、培训记录", "全流程跑通；空白无污染；标曲 R² 达标"],
    ["六 项目验收与交付归档", "1 周", "PQ 验证、终验、归档", "验收报告、移交清单、全套资料", "PQ 回收率 / RSD 达标；文档齐全"],
  ],
  [1900, 1100, 2200, 2000, 2160]
));
children.push(P([new TextRun({ text: "总工期甘特图：占位 → 实图（见附录 A）。", italics: true, color: "5B6B7E", size: 18 })]));

children.push(H2("7.3 项目管理措施"));
children.push(bullet([b("进度管理："), t("三级管控（项目经理 / 周例会 / 节点验收）+ 赶工预案。")]));
children.push(bullet([b("质量管理："), t("全流程质控追溯、关键质控节点设卡、过程与结果归档。")]));
children.push(bullet([b("沟通管理："), t("固定例会机制 + 专属沟通群 + 纪要留痕。")]));
children.push(bullet([b("安全管理："), t("用电 / 动火 / 接地 / 应急停机规程，权限分级，二氯甲烷等危化流路密封 + 排风联锁。")]));
children.push(bullet([b("变更管理："), t("书面申请 → 影响评估 → 签字确认，闭环可追溯。")]));

children.push(H2("7.4 保障措施"));
children.push(bullet([b("资源保障："), t("资深工程师 + 专用调试设备 + 原厂部件；甲方配合现场条件与样品。")]));
children.push(bullet([b("技术保障："), t("攻坚小组 + 7×24 远程支持 + 现场响应时限（以 SLA 约定）。")]));
children.push(bullet([b("售后前置："), t("售后工程师参与调试，提前掌握系统状态。")]));
children.push(bullet([b("文档保障："), t("全流程归档，纸质 + 电子双备份，可追溯。")]));

// =================================================================
// 8. 交付物清单
// =================================================================
children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(H1("8  交付物清单"));
children.push(table(
  ["类别", "交付物"],
  [
    ["正式技术文档", "详细设计方案、各功能平台设计图、系统 / 功能架构图、布局 CAD 图"],
    ["工程交付物", "工厂 / 现场调试报告（FAT/SAT）、设备合格证、安装 / 拼接验收单、验收报告、设备移交清单"],
    ["资料交付", "操作手册、维护手册、技术资料（纸质 + 电子）、售后服务协议"],
    ["软件交付", "SmartLabOS 软件部署 + 配置文档、LIMS 接口文档、账号 / 权限清单"],
  ],
  [2200, 7160]
));

// =================================================================
// 附录
// =================================================================
children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(H1("附录 A  图纸占位清单"));
children.push(P("以下「图 X-X」占位须在现场 CAD 底图与工厂出图后替换为实图，作为详细设计正式交付的组成部分："));
children.push(bullet("图 3-1 智能化实验室规划布局图（含设备定位、通道、机械臂行程）"));
children.push(bullet("图 3-2 管道排风系统改造图；图 3-3 气路分布系统改造图"));
children.push(bullet("图 3-4 强弱电与网络系统改造图；图 3-5 给排水 / 纯水管路改造图"));
children.push(bullet("图 4-1～4-8 各功能平台俯视布局示意图（模组位 + 托盘位）"));
children.push(bullet("图 5-1 软件系统架构图；图 5-2 功能模块架构图；图 5-3 数字孪生可视化示例图"));
children.push(bullet("图 7-1 总工期甘特图"));
children.push(P([new TextRun({ text: "注：上述布局 / 流程示意已在配套 HTML 提案（1.html / 2.html / 串接双柱SPE净化-模块-URS.html）中以 SVG / Mermaid 形式给出，可作为出图依据。", italics: true, color: "5B6B7E", size: 18 })]));

children.push(H1("附录 B  工艺偏差与范围说明（诚实标注，需与客户确认）"));
children.push(table(
  ["事项", "标准要求", "已确认模块能力", "建议处置"],
  [
    ["§8.1 提取离心", "−2 ℃ / 10000 r/min", "MOD-LX-001：2–7 ℃ / ≤8000 rpm", "① 选配外置高速冷冻离心机集成；或 ② 方法学验证后以 ≤4 ℃·8000 r/min 等效"],
    ["§8.2 复溶离心", "14000 r/min", "MOD-LX-001：≤8000 rpm", "复溶澄清对转速要求较低，可验证后延长时间等效；严格达标须外置高速离心"],
    ["浓缩耗时", "—", "MOD-NS-001：cycle_time 字段空白", "工程估算 1800 s，须现场 URS 实测替换"],
    ["称量（1 g 试样）", "感量 0.01 g 天平", "清单内无称量模组", "按行业惯例人工称样后上样，不属本期自动化范围"],
    ["方案二酶解容器", "50 mL 体系 / 37 ℃ 12 h", "MOD-FY-002 为 5 mL 管、控温精度 ±3 ℃", "缩量验证或定制同系列 50 mL 恒温振荡变体"],
    ["方案二微量加注", "内标 / 酶各 100 µL", "MOD-DR-003 标称 250–1000 µL", "选配小量程注射器（25/100 µL）"],
    ["方案二加水稀释", "加水 ~100 mL", "MOD-JY-017 单次 ≤50 mL", "分 2 次加注"],
  ],
  [1700, 1900, 2400, 3360]
));

children.push(H1("附录 C  新建模组 URS 摘要（MOD-SPE-D01 串接双柱 SPE）"));
children.push(P([t("依据指令 §5.3，GB/T 21981—2008 §7.2「ENVI-Carb 柱串接氨基柱」双柱串接净化在 11 个已确认模组中无对应能力（"), mono("MOD-SPE-004"), t(" 为单柱结构），构成模组缺口，新建 URS（详见 "), mono("串接双柱SPE净化-模块-URS.html"), t("）。")]));
children.push(bullet([b("核心功能需求："), t("双柱垂直串接夹持（上柱可独立取放、密封连通）、双柱独立活化、~135 mL 提取液 2–3 mL/min 上样、抽干 + 串接切换、二氯甲烷-甲醇 6 mL 串联洗脱 + 2 mL 二次洗下柱、自动上下料与洗针。")]));
children.push(bullet([b("关键参数："), t("上样流速 2–3 mL/min（闭环 ±5%）、上样体积 1–150 mL、洗脱计量 ≤±2%、抽干真空 ≤−80 kPa、≥2 串接对并行、≤2 模组位；通讯 MODBUS TCP。")]));
children.push(bullet([b("处理时间："), t("功能码 ≈3000 s（上样为主导项）+ 上下料 60 s = 3060 s，已计入方案二 WS5。")]));
children.push(bullet([b("验收（PQ）："), t("以 50 种激素基质加标验证，回收率 75.2%~121.8%、RSD 2.4%~20.8% 区间内为通过。")]));
children.push(bullet([b("公用工程："), t("二氯甲烷有机废液单独危废收集；工站强制防爆排风 + 抽气罩。")]));

children.push(H1("附录 D  后续待确认事项"));
children.push(num("高速冷冻离心达标方案（外置离心机集成 vs 8000 rpm 方法学等效验证）。"));
children.push(num("MOD-NS-001 浓缩实际耗时现场实测（替换本方案工程估算值）。"));
children.push(num("方案二酶解容器（5 mL 缩量 vs 定制 50 mL 恒温模块）与微量加注（小量程注射器）确认。"));
children.push(num("新建模组 MOD-SPE-D01 的 URS 评审、样机开发与 PQ 验证。"));
children.push(num("二氯甲烷 / 乙腈 / 氨性废液的危废分类收集与排风达标设计。"));
children.push(num("场地原始尺寸图、楼层 / 房间用途与隔断改造范围（出布局施工图前置条件）。"));

children.push(spacer(160));
children.push(callout("报价说明", [
  [t("本方案为售前技术配置建议。"), b("涉及价格、报价信息，请联系销售团队获取报价。"), t("交付周期与质保以最终合同为准。")],
  [b("数据来源："), t("全部技术参数引用自国家标准原文、本项目已生成 HTML 提案与 references/ 知识库（01-modules / 02-platforms / 06-pallet），模组严格限于 11 个已确认型号；技术参数以国标原文及现场 URS 确认为准。")],
], OKFILL));

// =================================================================
// 文档组装
// =================================================================
const doc = new Document({
  creator: "百泉聚兴 SmartLabOS 智能方案助手",
  title: "成都市食品检验研究院 兽药残留专项自动化检测系统 详细设计方案",
  description: "SmartLabOS 售前 WORD 提案 · 项目：测试项目",
  styles: {
    default: { document: { run: { font: "Microsoft YaHei", size: 21, color: INK } } },
    paragraphStyles: [
      { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 30, bold: true, font: "Microsoft YaHei", color: BRAND },
        paragraph: { spacing: { before: 320, after: 160 }, outlineLevel: 0 } },
      { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 25, bold: true, font: "Microsoft YaHei", color: INK },
        paragraph: { spacing: { before: 240, after: 120 }, outlineLevel: 1 } },
      { id: "Heading3", name: "Heading 3", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 22, bold: true, font: "Microsoft YaHei", color: BRAND2 },
        paragraph: { spacing: { before: 160, after: 80 }, outlineLevel: 2 } },
    ]
  },
  numbering: {
    config: [
      { reference: "bullets", levels: [
        { level: 0, format: LevelFormat.BULLET, text: "•", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 460, hanging: 260 } } } },
        { level: 1, format: LevelFormat.BULLET, text: "–", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 920, hanging: 260 } } } },
      ] },
      { reference: "nums", levels: [
        { level: 0, format: LevelFormat.DECIMAL, text: "%1.", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 460, hanging: 260 } } } },
      ] },
    ]
  },
  sections: [{
    properties: { page: { size: { width: 12240, height: 15840 }, margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 } } },
    headers: { default: new Header({ children: [new Paragraph({
      alignment: AlignmentType.RIGHT, border: { bottom: { style: BorderStyle.SINGLE, size: 6, color: BRAND, space: 2 } },
      children: [new TextRun({ text: "百泉聚兴 SmartLabOS · 成都市食品检验研究院 兽残专项详细设计方案", size: 15, color: "8090A0" })]
    })] }) },
    footers: { default: new Footer({ children: [new Paragraph({
      alignment: AlignmentType.CENTER,
      children: [new TextRun({ text: "第 ", size: 16, color: "8090A0" }), new TextRun({ children: [PageNumber.CURRENT], size: 16, color: "8090A0" }),
        new TextRun({ text: " 页 / 共 ", size: 16, color: "8090A0" }), new TextRun({ children: [PageNumber.TOTAL_PAGES], size: 16, color: "8090A0" }),
        new TextRun({ text: " 页　|　V1.0 · 2026-06-30　|　涉及报价请联系销售团队", size: 16, color: "8090A0" })]
    })] }) },
    children
  }]
});

const outPath = "C:\\TestClaude\\SmartLabOS-AI-Assistant\\projects\\测试项目\\测试项目_SmartLabOS_Presales_提案_20260630160022.docx";
Packer.toBuffer(doc).then(buf => { fs.writeFileSync(outPath, buf); console.log("WROTE " + outPath + " (" + buf.length + " bytes)"); });
