---
# ========== 标识与版本 ==========
id: PLT-XXXX                           # 平台唯一编号，建议 PLT- 前缀
Platform_Part_No: PRD-PLT-202604001    # 平台发货号，建议 PRD- 前缀
name: 平台显示名称
version: 1.0                          # 卡片版本号，便于追踪修订
status: draft                         # draft | active | deprecated
last_updated: 2026-04-25
owner: 负责人或团队                    # 可选

# ========== 物理规格 ==========
dimensions_mm:
  length: 0
  width: 0
  height: 0
weight_kg: 0                          # 可选
footprint_m2: 0                       # 可选,占地面积

# ========== 承载能力 ==========
max_modules: 0
max_trays: 0
tray_size_mm:
  length: 0
  width: 0
tray_layout: ""                       # 可选,如 "2x4" 描述布局

# ========== 功能与配置 ==========
supported_functions:                   # 平台能承担的工艺功能
  - 称量
  - 提取
  # ...
pipette_options_ml: [10, 5, 2, 1]     # 可选移液规格
gripper_options:                       # 抓手选项
  - 试管-15ml
  - 试管-50ml
module_compatibility:                  # 可选,兼容的模块类型清单
  - SPE
  - 浓缩
  - 加液振荡

# ========== 电气与环境 ==========
power_kva_nominal: 0.0                # 额定功率
power_kva_peak: 0.0                   # 峰值功率
voltage_v: 220                        # 可选
requires_ventilation: false           # 是否强制通风
requires_compressed_air: false        # 是否需要压缩空气
ambient_temp_c: { min: 15, max: 30 }  # 可选

# ========== 限制（结构化部分） ==========
constraints:
  max_heating_modules: 4              # 超过该数量需通风
  power_check_combos:                 # 需要电力复核的模块组合
    - [SPE, 浓缩]
  forbidden_combos: []                # 禁止同台的模块组合

# ========== 价格与交付（可选） ==========
price_rmb: null
lead_time_weeks: null

# ========== 标签 ==========
tags: [主力机型, 中通量]
---

# 平台显示名称

## 用途说明
一段话描述这个平台的定位、目标场景、和其他平台的区别。

## 典型配置示例
- 配置 A（用途）：模块组合描述
- 配置 B（用途）：模块组合描述

## 限制
- 用人话补充 frontmatter 里 `constraints` 字段的背景和注意事项
- 其他不易结构化但需要工程师注意的要点

## 备注
其他自由说明：维护周期、已知坑、订货注意事项等。
