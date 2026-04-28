---
name: 模块类别词表
description: SmartLabOS 模块卡片 category 字段受控词表
status: draft
last_updated: 2026-04-28
---

# 模块类别受控词表

> 模块卡片的 `category` 字段**只能**从下表选取。
> 类别用于 `_index.md` 自动分组，所以一旦定义就尽量稳定。

| 类别 | 含义 | 典型模块前缀 |
|------|------|------------|
| 称量 | 固体/液体精准称量 | `MOD-WEIGH-*` |
| 加液振荡 | 加液 + 涡旋/振荡复合 | `MOD-DISP-SHAKE-*` |
| 移液 | 精确移液转移 | `MOD-PIPETTE-*` |
| SPE | 固相萃取净化 | `MOD-SPE-*` |
| 浓缩 | 溶剂浓缩（氮吹/旋蒸） | `MOD-CONC-*` |
| 离心 | 固液分离 | `MOD-CENT-*` |
| 加热 | 恒温/加热消解 | `MOD-HEAT-*` |
| 超声 | 超声辅助 | `MOD-ULTRA-*` |
| 过滤 | 滤膜过滤 | `MOD-FILTER-*` |
| 消解 | 微波/加热消解 | `MOD-DIGEST-*` |
| 取液 | 定量取液 | `MOD-LIQ-*` |
| 留样 | 样品留存 | `MOD-STORE-*` |
| 其他 | 上述未涵盖 | — |

## 维护规则

- 新增类别需同步修改本表与对应模块卡片
- 类别废弃时改记 `~~已废弃~~` 而非直接删除
- 类别变更后跑 `python tools/build_indexes.py` 重建索引
