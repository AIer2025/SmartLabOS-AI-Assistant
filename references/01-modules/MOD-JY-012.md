---
identification_version:
  id: MOD-JY-012
  module_part_no: HHM10A-001
  name: 8通道-正压加液模块
  category: 加液
  version: 2.0
  status: active
  last_updated: 2024/12/23
  owner: 徐翔宇
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 300
  weight_kg: 15
  module_slots: 0
  tray_slots: "八工位"
  mount_type: 平台外置
module_performance:
  volume_ml_min: "NA"
  volume_ml_max: "NA"
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: XXX
  function_description: 加液
  operation_target: 溶剂瓶
  operation_workflow: 通气-电磁阀打开-加液-电磁阀关闭
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 10
  consumables: 溶剂瓶
  sub_functions: []
platform_compatibility:
  compatible_platforms:
    - PLT-800
    - PLT-1200
    - PLT-1400
  incompatible_platforms:
    []
module_relationships:
  dependencies:
    - --前置/后置模块
  conflicts:
    - --相互影响模块
  typical_pairings:
    - --和模块配合使用
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: ""
tags_meta:
  tags: [正压加液，溶剂瓶]
documentation:
  urs_url: URS连接
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.252
  power_kva_peak: 0.42
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 8通道-正压加液模块

## Purpose (用途说明)
给模块提供气源，溶剂瓶，通过电磁阀控制加液量

## Typical Scenarios (典型应用场景)
1. 实验室标准液 / 缓冲液配制
2. 体外诊断试剂（IVD）生产
3. 制药行业药液 / 中间液定量加注
4. 食品 / 环境 / 水质检测前处理
5. 精细化工 / 化妆品微量配液
6. 易起泡 / 易结晶液体专用加注

## Working Principle (工作原理)
密闭加压 → 液体被稳定推出 → 流量由气压 / 阀门精准控制 → 无脉动、无气泡、高精度加液

## Limits & Cautions (限制与注意事项)
1.压力稳、密封好，压力不能超过瓶子、管路的极限
2.先排气、再运行
3.不喷液、不滴漏
4.开盖先泄压

## Remarks (备注)

