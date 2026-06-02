---
identification_version:
  id: MOD-HY-001
  module_part_no: HHM23H-010
  name: 10ml西林瓶加液混匀模块
  category: 混匀
  version: 1.0
  status: active
  last_updated: 2025/7/25
  owner: 汪健
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 10
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 0
  speed_rpm_max: 3000
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: 560
  function_description: 加液混匀
  operation_target: 10ml西林瓶
  operation_workflow: 西林瓶座移至上下料位 - 机械臂从托盘取西林瓶放入瓶座 - 瓶座移至加液位 - 气缸下压 - 加液 - 气缸抬起 - 西林瓶座移至混匀位 - 气缸下压压紧瓶口 - 混匀 - 停止 - 气缸抬起 - 西林瓶座移至上下料位 - 机械臂取下西林瓶放入托盘
  absolute_accuracy_pct: ±1
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 100
  consumables: 10ml西林瓶
  sub_functions: [NA]
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
    - 正压加液模块
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: 涡旋电机
tags_meta:
  tags: [加液，混匀]
documentation:
  urs_url: URS连接
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.13
  power_kva_peak: 0.22
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 10ml西林瓶加液混匀模块

## Purpose (用途说明)
这是一套专为10ml西林瓶设计的"精确加液 + 振荡混匀"自动化模块，核心解决的是小体积（10ml级别）药液/试剂/生物样品的"精确配制 + 快速混匀"问题

## Typical Scenarios (典型应用场景)
10ml西林瓶加液混匀模块的应用场景 = 静配中心（量最大）+ 药物研发（精度要求最高）+ CGT生物制药（合规要求最高）+ 微生物/疾控（换型最频繁）+ 分析质控（防污染最严）+ 小批量试产（灵活性最高），六大场景的共同核心需求是：手工加液不准、振荡不匀、交叉污染、产气反应危险——这四个痛点，模块一个不留全部解决

## Working Principle (工作原理)
加液原理：靠气体压力把液体压出，实现定量加注

混匀原理：西林瓶定位固定后，模块通过旋转混匀带动瓶内液体快速混合，使试剂充分溶解、反应、均一。

## Limits & Cautions (限制与注意事项)
10ml西林瓶加液混匀模块的限制 = 灌装≤8.5ml（起泡≤8.0ml）+ 混匀500~3000rpm + 精度±0.5% ；注意事项的核心 = 防炸瓶、防污染、防交叉污染（一次性+密封）、防失控。记住：这个模块最大的二个杀手是 ① 灌太满 → 灭菌炸瓶 ② 产气不排气

## Remarks (备注)

