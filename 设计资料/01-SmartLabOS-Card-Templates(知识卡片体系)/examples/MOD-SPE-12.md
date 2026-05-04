---
id: MOD-SPE-12
Module_Part_No: PRD-MOD-202604001
name: 12 通道 SPE 净化模块
category: SPE
version: 1.2
status: active
last_updated: 2026-04-25
owner: SmartLabOS 模块组

dimensions_mm:
  length: 280
  width: 180
  height: 380
weight_kg: 18
module_slots: 1
tray_slots: 1
mount_type: 标准模块位

function: 净化
sub_functions: [上样, 淋洗, 洗脱, 抽真空]
throughput:
  capacity: 12
  unit: 管
  cycle_time_min: 25
parameters:
  volume_ml: { min: 0.5, max: 20 }
  pressure_bar: { min: -0.9, max: 0 }

consumables:
  - SPE 柱-3ml
  - SPE 柱-6ml
  - 收集管-15ml
sample_container:
  - 试管-15ml
gripper_required:
  - 试管-15ml

power_kva_nominal: 0.4
power_kva_peak: 0.6
voltage_v: 220
heat_generating: false
requires_ventilation: false
requires_compressed_air: true
requires_water: false
requires_drain: true
noise_db: 55

compatible_platforms:
  - PLT-1200
  - PLT-1400
  - PLT-1800
incompatible_platforms:
  - PLT-800

dependencies:
  - MOD-PIPETTE-8           # 必须配合移液模块上样
conflicts: []
typical_pairings:
  - 与 MOD-CONC-6 组成净化-浓缩工艺链
  - 与 MOD-PIPETTE-8 配合完成 SPE 全流程

price_rmb: null
lead_time_weeks: 6
maintenance_cycle_months: 6

tags: [核心模块, 净化, 高频]
---

# 12 通道 SPE 净化模块

## 用途说明
12 通道并行 SPE(固相萃取)净化模块,适用于农残、兽残、霉菌毒素等多种基质的前处理净化环节。相比 6 通道版本通量翻倍,适合中通量实验室。

## 典型应用场景
- 农残检测:配合 QuEChERS 流程,做 PSA/C18 净化
- 兽残检测:多种 SPE 柱(HLB/MCX)净化
- 中药复杂基质:多步净化

## 工作原理
通过真空泵在收集管下方提供负压,样品依次经过活化、上样、淋洗、洗脱四个步骤完成净化。每通道独立控制流速,避免通道间干扰。

## 限制与注意事项
- 必须配合移液模块完成上样,单独使用无意义
- 需要排液接口处理废液,安装时务必预留
- 真空压力上限 -0.9 bar,过强会导致柱床塌陷
- SPE 柱规格切换需手动更换适配器,约 5 分钟
- PLT-800 平台空间不足以容纳 12 通道版本,需选 6 通道版

## 备注
- 订货时需确认客户使用的 SPE 柱品牌,部分品牌需定制适配器
- 真空泵为内置免维护型,但每 6 个月需检查管路密封
- 客户常问:能否扩展到 24 通道? 答:目前不支持,需选购两台
