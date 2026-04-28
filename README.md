# SmartLabOS AI Assistant

> 百泉聚兴（北京）科技有限公司 · SmartLabOS 售前技术方案智能助手  
> 基于 Claude Code + Claude Max 的五阶段提案生成 Agent

## 项目简介

本仓库实现了一个面向**实验室全流程自动化样品前处理系统**的售前技术方案生成 Agent。
项目负责人通过自然语言对话，由 AI 助手按"五阶段工作流"逐步引导，完成从客户需求采集到正式技术方案输出的全过程。

所有技术参数、工艺流程、设备规格均来自 [references/](references/) 知识库，**严禁编造数据**；
工艺流程严格遵循对应国标（GB23200.113-2018、GB23200.121-2021、GB31658.26-2025 等）。

## 五阶段工作流

```text
阶段 1 创建提案项目  →  用户说"新建提案 {项目名称}"
阶段 2 提案信息收集  →  用户说"开始采集"
阶段 3 提案初稿生成  →  用户说"生成初稿"
阶段 4 审核及修改    →  用户说"审核方案"
阶段 5 正式发布      →  用户说"发布方案"

随时查看进度        →  用户说"查看进度"或"提案状态"
```

每个阶段对应一个独立的 Skill，由 `proposal-workflow` 主控编排器协调，状态写入项目目录下的 `workflow-status.md`。

## 快速开始

1. 在 VS Code 中打开 `C:\TestClaude\SmartLabOS-AI-Assistant`
2. 启动 Claude Code（终端运行 `claude`，或使用 VS Code 扩展）
3. 验证知识库：让 Claude 列出 [references/](references/) 下所有文件并简要说明
4. 创建提案：输入"新建提案 {客户名称}"，按提示逐步推进五个阶段
5. 输出位置：`projects/<客户或项目名称>/`

## 目录结构

| 路径 | 说明 |
|------|------|
| [CLAUDE.md](CLAUDE.md) | Agent 角色、规则与约束（每次会话自动加载到系统提示词） |
| [.claude/skills/](.claude/skills/) | 五阶段工作流的 Skill 定义 |
| [references/](references/) | 知识库：国标、设备参数、工艺流程、文档模板 |
| [projects/](projects/) | 每个客户/项目一个目录，存放六类工作产物 |
| [samples/](samples/) | 历史项目参考样例（如北京海关农残检测前处理系统） |
| [SetupTools/](SetupTools/) | 安装工具与演进路线说明 |
| [设计资料/](设计资料/) | SmartLabOS 设计方法、知识卡片体系、自动化资料 |
| [大连项目/](大连项目/) | 大连中药经典名方质量分析自动化项目原始资料 |
| [食检院项目/](食检院项目/) | 成都食检院项目国标 PDF 与设计稿 |
| [FromUbuntu/](FromUbuntu/) | 原 Ubuntu OpenClaw 环境导出归档（迁移完成后可删除） |

## Skills 一览

位于 [.claude/skills/](.claude/skills/)，由 Claude Code 自动加载：

| Skill | 触发关键词 | 职责 |
|-------|-----------|------|
| `proposal-workflow` | 新建提案 / 提案流程 | 主控编排器，管理五个阶段的状态机 |
| `proposal-collector` | 开始采集 / 信息收集 | 引导用户逐步采集六大类业务/技术信息 |
| `proposal-analyzer` | 分析需求 / 开始分析 | 匹配工艺流程、设备选型、系统配置 |
| `proposal-generator` | 生成方案 / 出方案 | 按模板生成完整的技术方案 Markdown |
| `proposal-reviewer` | 审查方案 / 检查方案 | 五维度审查：完整性、参数一致性、国标合规、通量合理性、需求匹配 |

## 知识库（references/）

| 文件 | 内容 |
|------|------|
| [product-specs.md](references/product-specs.md) | SmartLabOS 系统参数与产品规格 |
| [process-quechers.md](references/process-quechers.md) | QuEChERS 农残前处理工艺流程 |
| [process-inorganic.md](references/process-inorganic.md) | 无机元素前处理工艺流程 |
| [unit-catalog.md](references/unit-catalog.md) | 硬件功能单元目录（农残/无机两套） |
| [standards-library.md](references/standards-library.md) | 国标/行标关键要求摘要 |
| [proposal-template.md](references/proposal-template.md) | 技术方案文档模板规范 |

## 项目工作产物（projects/{name}/）

每个项目目录会按阶段产出以下文件：

```text
collected-info.md             # 阶段 2 采集到的客户信息
analysis-report.md            # 阶段 3 需求分析报告
technical-proposal-draft.md   # 阶段 3 生成的初稿（可有 v2/v3）
review-report.md              # 阶段 4 审查报告
technical-proposal.md         # 阶段 5 正式发布版本
project-summary.md            # 阶段 5 项目摘要
workflow-status.md            # 全程状态追踪
```

## 行为准则（节选自 CLAUDE.md）

- 所有技术参数必须来自 [references/](references/) 知识库，**严禁编造**
- 工艺流程必须严格遵循对应国标
- 不确定时诚实说明，并明确列出需补充的信息
- 涉及报价问题统一回复"请联系销售团队获取报价"
- 仅处理 SmartLabOS 相关方案，不回答无关问题
- 输出统一使用 Markdown 格式，沟通使用中文

## License

[MIT](LICENSE) © 2026 AIer2025
