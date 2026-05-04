---
# ========== 标识与版本 ==========
id: MOD-XXXX                          # 模块唯一编号,建议 MOD- 前缀
Module_Part_No: PRD-MOD-202604001     # 模块发货号，建议 PRD- 前缀
name: 模块显示名称
category: 加液振荡                     # 大类:加液振荡 | SPE | 浓缩 | 离心 | 移液 | 称量 | 加热 | 其他
version: 1.0
status: draft                         # draft | active | deprecated
last_updated: 2026-04-25
owner: 负责人或团队

# ========== 物理规格 ==========
dimensions_mm:
  length: 0
  width: 0
  height: 0
weight_kg: 0
module_slots: 1                       # 占用几个标准模块位
tray_slots: 0                         # 占用几个托盘位(若占用)
mount_type: 标准模块位                 # 标准模块位 | 托盘位 | 平台外置 | 顶部安装

# ========== 工艺能力 ==========
function: 加液振荡                     # 主功能,与平台 supported_functions 对齐
sub_functions: []                     # 可选,如 [加液, 振荡, 加热]
throughput:                            # 通量描述
  capacity: 0                         # 单批处理数量
  unit: 样品                          # 单位:样品 | 板 | 管
  cycle_time_min: 0                   # 单循环耗时(分钟)
parameters:                            # 工艺参数范围
  volume_ml: { min: 0, max: 0 }       # 处理体积范围
  temperature_c: { min: null, max: null }
  speed_rpm: { min: null, max: null }
  pressure_bar: { min: null, max: null }
  # 按模块类型按需保留/删除

# ========== 耗材与样品适配 ==========
consumables:                           # 耗材类型清单
  - SPE 柱-3ml
  - 试管-15ml
sample_container:                      # 兼容的样品容器
  - 试管-15ml
  - 离心管-2ml
gripper_required:                      # 需要哪种抓手配合
  - 试管-15ml

# ========== 电气与环境 ==========
power_kva_nominal: 0.0
power_kva_peak: 0.0
voltage_v: 220
heat_generating: false                # 是否属于发热类模块(影响通风计数)
requires_ventilation: false
requires_compressed_air: false
requires_water: false                 # 是否需要水路
requires_drain: false                 # 是否需要排液
noise_db: null                        # 可选,噪音水平

# ========== 平台兼容性 ==========
compatible_platforms:                  # 兼容的平台 ID 列表
  - PLT-1400
  - PLT-1800
incompatible_platforms: []            # 明确不兼容的平台

# ========== 模块间关系 ==========
dependencies: []                       # 必须配合使用的模块,如移液模块通常依赖加液
conflicts: []                         # 不能同台的模块 ID
typical_pairings:                      # 典型搭配场景
  - 与 MOD-XXXX 组成提取工艺链
  - 与 MOD-XXXX 配合用于净化

# ========== 价格与交付 ==========
price_rmb: null
lead_time_weeks: null
maintenance_cycle_months: null        # 维护周期

# ========== 标签 ==========
tags: [核心模块, 高通量]
---

# 模块显示名称

## 用途说明
一段话描述这个模块在工艺链中的角色、解决什么问题、相比同类模块的差异。

## 典型应用场景
- 场景 A：在某类工艺中的角色和参数
- 场景 B：另一类工艺中的角色和参数

## 工作原理
简要描述模块的核心机制(可选,便于工程师和销售理解)。

## 限制与注意事项
- 用人话补充 frontmatter 里 parameters、consumables 的背景
- 已知坑、易混淆点、维护要点
- 与其他模块协作时的注意事项

## 备注
订货前需确认的项、客户常问问题、其他自由说明。
