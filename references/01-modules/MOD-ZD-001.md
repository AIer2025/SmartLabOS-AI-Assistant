---
identification_version:
  id: MOD-ZD-001
  module_part_no: HHM01A-050
  name: 50ml垂直震荡模块
  category: 震荡
  version: 2.0
  status: active
  last_updated: 2026/1/30
  owner: 宾俊
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 15
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 50
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 1
  speed_rpm_max: 500
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: "140 / "
  function_description: 垂直震荡
  operation_target: 50ml离心管
  operation_workflow: 载台移动到最高点-电推杆向前推动离心管抱夹-抱夹打开-放入离心管-电推杆向后运动-抱夹夹紧-振荡-载台移动到最高点-电推杆向前推动离心管抱夹-抱夹打开-取下离心管；更换样品，重复测试
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 220
  consumables: 50ml离心管
  sub_functions: [140 震荡]
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
  wear_parts: 工位硅胶条
tags_meta:
  tags: [垂直震荡]
documentation:
  urs_url: 未找到
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.2
  power_kva_peak: 0.2
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 50ml垂直震荡模块

## Purpose (用途说明)
50ml离心管垂直震荡

## Typical Scenarios (典型应用场景)
北京海关项目

## Working Principle (工作原理)
50ml 离心管垂直震荡模块（垂直振荡混匀仪）的核心工作原理，是通过电机驱动机械传动机构，将旋转运动转化为高频、垂直方向的直线往复振动，带动离心管内液体因惯性产生剧烈翻滚与冲击，从而实现快速、均匀混合。

## Limits & Cautions (限制与注意事项)
最高转速不要超过500

## Remarks (备注)

