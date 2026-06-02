---
identification_version:
  id: MOD-CC-002
  module_part_no: HHM43C-001
  name: 落地式仓库模块
  category: 存储
  version: 1.0
  status: active
  last_updated: 2025/9/19
  owner: 汪健
physical_specs:
  length_mm: 1400
  width_mm: 850
  height_mm: 1800
  weight_kg: 200
  module_slots: 0
  tray_slots: "六十一工位"
  mount_type: 平台外置
module_performance:
  volume_ml_min: "NA"
  volume_ml_max: "NA"
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: ""
process_capability_1:
  function_code: 627
  function_description: 入仓
  operation_target: 标准托盘
  operation_workflow: "从接驳位取托盘，入到空闲库位，完成入库； / 从库位取托盘，出到接驳位，完成出库；"
  absolute_accuracy_pct: ""
  repeat_accuracy_pct: ""
  cycle_time_sec: 30
  consumables: ""
  sub_functions: [628-预出仓, 629-出仓, 631-取料, 632-放料, 633-取放料]
platform_compatibility:
  compatible_platforms:
    []
  incompatible_platforms:
    []
module_relationships:
  dependencies:
    - --前置/后置模块
  conflicts:
    - --相互影响模块
  typical_pairings:
    - --和模块配合使用
    - 叉车机器人
    - 人工操作平台
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: ""
tags_meta:
  tags: [落地式仓库]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.6084
  power_kva_peak: 1.014
  voltage_v: 220
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 落地式仓库模块

## Purpose (用途说明)
样品耗材存储

## Typical Scenarios (典型应用场景)
落地式仓库模块主要用于实验室自动化、样品前处理、检测流水线、小型智能制造，实现样品托盘自动入库、暂存、出库、调度，对接轨道机器人，提高通量、稳定性与无人化水平

## Working Principle (工作原理)
XYZ 机械手 + 伸缩货叉，自动存取托盘，对接轨道机器人，实现样品托盘自动化仓储流转

## Limits & Cautions (限制与注意事项)
1.运行前检查货叉、电磁锁、传感器状态，托盘摆放平稳无突出
2.运行中禁止进入作业区、触碰运动部件，防止夹伤、碰撞。
定期校准X/Z 轴零点、货叉伸缩精度，保证 ±0.1mm 定位。
3.保持库位与传感器清洁，避免粉尘遮挡导致误检。
4.异常（卡滞、异响、传感器报错）立即停机断电，排查后再启动。
5.长期停用需将货叉回零、断电防尘，防止受潮老化。

## Remarks (备注)

