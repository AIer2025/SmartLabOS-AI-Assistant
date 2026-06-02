---
identification_version:
  id: MOD-YJ-001
  module_part_no: HHM11B-250
  name: 250ml锥形瓶连续加盐模块
  category: 加盐
  version: 1.0
  status: active
  last_updated: 2025/11/7
  owner: 王志
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 250
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
  function_code: 225
  function_description: 加盐
  operation_target: 250ml锥形瓶
  operation_workflow: "盐罐放入浮动载台 - 齿盘啮合 - 250ml锥形瓶载台移至上下料位 - 称重传感器清零 - 250ml锥形瓶放入载台 - 载台移至加盐位 - 齿盘转动 - 快速加盐 - 加盐量至95% - 慢速加盐 - 加盐量100% - 停止 - 加盐重量数值稳定 - 补充加盐 - 250ml锥形瓶载台移至上下料位 - 取下250ml锥形瓶； / 加盐过程中，无盐落下10s，振动盐罐浮动部件； / 更换250ml锥形瓶，重复测试"
  absolute_accuracy_pct: "(1)0.1-1g  ±10% / (2)1-10g  ±3%"
  repeat_accuracy_pct: "(1)0.1-1g  ±10% / (2)1-10g  ±3%"
  cycle_time_sec: "(1)1g  30S / (2)5g  90S / (3)10g  120"
  consumables: 锥形瓶-250ml
  sub_functions: [225 加盐]
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
  wear_parts: 无
tags_meta:
  tags: [加盐]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.084
  power_kva_peak: 0.14
  voltage_v: 48
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 250ml锥形瓶连续加盐模块

## Purpose (用途说明)
本模块实现往250ml锥形瓶中加入指定重量的固体试剂，锥形瓶不需要拆装盖，装载入模组后加入固体执行称重操作，并且可以更换盐罐。

## Typical Scenarios (典型应用场景)
无

## Working Principle (工作原理)
模块由储料仓、定量送料机构、下料导向机构、瓶体定位工装组成，依靠电机驱动定量分度 / 螺旋送料结构，精准控制单次出料盐料重量；250ml 锥形瓶完成精准定位对位后，机构按程序连续定量投放固体盐类试剂，通过匀速送料、稳流下料方式将盐料平稳送入瓶内，可实现多瓶依次连续自动加盐，全程统一投料量，保障实验配比一致性。

## Limits & Cautions (限制与注意事项)
无

## Remarks (备注)

