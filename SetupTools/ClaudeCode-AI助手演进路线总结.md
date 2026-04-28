# SmartLabOS 智慧实验室 AI 售前提案助手 — 方案总结

> 目标：利用 Claude 大模型，辅助完成 SmartLabOS 智慧实验室的**自动提案生成**工作。
> 输入资料类型：行业规范、实验模块硬件规格、平台搭建规则、实验室前处理 SOP、用电/环境/平台尺寸等参数。

---

## 一、关键前置澄清：Claude Max ≠ API 额度

| 订阅/产品 | 覆盖范围 | 能做什么 |
|---|---|---|
| **Claude Max 订阅** | claude.ai 网页/桌面端 + Claude Code | 对话、Projects 知识库、Claude Code 本地脚本 |
| **Anthropic API**（单独计费） | 直接调用 API（token 计费） | 自动化管道、Web 服务、批量处理 |

**结论**：
- 只用 Max 订阅 → 走 **claude.ai Projects**（人工驱动）或 **Claude Code**（本地脚本，消耗 Max 额度）
- 想做自动化服务/定时任务/批量生成 → 必须额外购买 **Anthropic API** token

---

## 二、三种架构方案（按工程量递增）

### 方案 A：Claude Projects + 结构化知识库 ✅【当前选定】
- **工作量**：0 代码，1–2 天
- **做法**：把行业规范、硬件规格、SOP、尺寸/电力表整理成 Markdown/PDF，上传到一个 Claude Project，写好 System Prompt（扮演售前工程师、输出提案骨架）；每次手动贴入"客户需求"生成提案。
- **适合**：先跑通一两轮，搞清楚提案真实结构、信息缺口、文风。**强烈建议从这里起步**，否则过早自动化会白做。

### 方案 B：Claude Code + MCP Server
- **工作量**：中等，1–2 周
- **做法**：把硬件规格、平台规则等**结构化数据**做成 MCP Server（查询接口：按实验类型筛选模块、计算用电负载、校验尺寸合规等），Claude Code 作为前端调用；规范类**非结构化**文档继续走 Projects 知识库或 RAG。
- **适合**：数据频繁变更（硬件 SKU 更新、规范版本升级），且希望 Claude 真正"算"而不仅仅"查"。

### 方案 C：独立 Web 平台 + Claude API + RAG
- **工作量**：重，1–2 月
- **做法**：自建后端（C# ASP.NET Core 或 Python FastAPI）+ 向量库（Qdrant/PGVector）+ Claude API + Word/PDF 模板导出（DocX / OpenXML）;前端让售前填表单，一键出正式提案文件。
- **适合**：多人协作、版本沉淀、对接 CRM。

---

## 三、C# 要写吗？

**核心 AI 逻辑不需要 C#**，Claude Agent SDK 目前主力是 TypeScript 和 Python。

**但 C# 在本场景里有三个天然位置**：
1. SmartLabOS 本身是 Windows / .NET 栈（InstallShield、Consul、MySQL 等），对接现有实验室数据库用 C# 最顺
2. Word/PDF 提案文件生成 — `DocumentFormat.OpenXml` / `Aspose.Words` 在 .NET 生态很成熟
3. 若提案平台要装到实验室内网 Windows Server 上、作为 SmartLabOS 一个模块，C# 合理

**推荐分工**：
- **C# 层**：数据接入、文档导出、与 SmartLabOS 业务系统对接
- **Python / TS 层**：Agent 编排
- **Claude API / Max**：推理

---

## 四、推荐落地路径

| 阶段 | 时间窗 | 任务 |
|---|---|---|
| ① 本周 | 按**方案 A** 跑通 2–3 个真实案例 | 沉淀提案骨架、章节结构、公式/查表逻辑 |
| ② 下一步 | 观察 Claude 在哪些环节易出错 | 通常是硬件选型、负载计算、规范引用 — 这些由 MCP Server 兜底 |
| ③ 再下一步 | 决定终态形态 | 留在 Claude Code + MCP(个人/小团队) 或上独立 Web 平台(公司级) |

---

## 五、方案 A 实施清单（下一步即将开展）

- [ ] 整理并分类知识资料目录结构（规范 / 硬件库 / SOP / 参数表 / 历史提案）
- [ ] 提供一份典型**历史提案样例**作为文风与骨架参考
- [ ] 设计 Claude Project 的 **System Prompt**（角色、输出格式、引用规则、禁忌）
- [ ] 在 claude.ai 创建 Project 并上传知识库
- [ ] 第一轮试跑：给定一个客户需求输入,观察输出质量
- [ ] 迭代 System Prompt 与知识库组织,对齐输出规范

---

*文档生成时间：2026-04-22*
