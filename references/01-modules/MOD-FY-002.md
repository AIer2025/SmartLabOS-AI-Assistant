---
identification_version:
  id: MOD-FY-002
  module_part_no: HHM01J-005
  name: 5m1离心管恒温振摇反应模块
  category: 加热
  version: 1.0
  status: active
  last_updated: 2025/9/26
  owner: 唐宋
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "二十四工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: "NA"
  volume_ml_max: "NA"
  temperature_c_min: 25
  temperature_c_max: 90
  speed_rpm_min: 0
  speed_rpm_max: 300
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: ""
process_capability_1:
  function_code: 647
  function_description: (上下料完成)启动平摇并关盖
  operation_target: 5ml离心管
  operation_workflow: 初始化-温度达到指定温度-加热底座移出 - 遮光盖打开 - 放入离心管 - 遮光盖关闭 -Y轴到平遥为止- 振摇 - 重复上料 -振摇时间到依此取出离心管-结束
  absolute_accuracy_pct: ""
  repeat_accuracy_pct: ""
  cycle_time_sec: 3600
  consumables: 离心管-5ml
  sub_functions: [646-开盖停止平摇, 648启动预热, 649加热关闭]
module_up_unload_time:
  up_unload_time: 60s
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
  wear_parts: 温度传感器线路
tags_meta:
  tags: [恒温, 振摇, 反应]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.564
  power_kva_peak: 0.94
  voltage_v: 220
  noise_db: 65
  heat_generating: "true"
  requires_ventilation: false
  requires_compressed_air: false
  requires_water: false
  requires_drain: false
---

# 5m1离心管恒温振摇反应模块

## Purpose (用途说明)
本模块为样品恒温混匀专用设备，适配 5ml 离心管，提供 24 通道并行处理，集成精准控温与平摇混匀功能，满足生物、化学样品恒温反应与混匀需求

## Typical Scenarios (典型应用场景)
该模块用于 5ml 离心管样品恒温振摇反应，提供 24 通道并行处理，控温室温至 60℃、精度 ±3℃，平摇转速 0–300rpm 可调。适配分子生物、临床检验、环境食品检测、生物制药等场景，满足核酸孵育、酶反应、样品混匀需求，自动化上下料，高效稳定、重复性好。

## Working Principle (工作原理)
加热控温 + 水平振摇，24 通道恒温混匀，实现样品自动孵育反应。

## Limits & Cautions (限制与注意事项)
本模块是面向实验室自动化的24 通道恒温混匀设备，专为 5ml 离心管设计，集成精准控温、水平振摇与自动上下料功能。控温范围室温至 60℃、精度 ±3℃，振摇转速 0–300rpm 可调，配合密闭遮光盖稳定反应环境。适配分子生物、临床检验、环境食品检测、生物制药等场景，满足核酸孵育、酶反应、样品混匀需求，自动化程度高、运行稳定、重复性好，助力实验室无人化高效作业。

## Remarks (备注)

