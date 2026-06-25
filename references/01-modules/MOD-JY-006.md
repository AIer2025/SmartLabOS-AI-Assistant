---
identification_version:
  id: MOD-JY-006
  module_part_no: HHM10B-025-UD
  name: 25ml注射泵加液模块
  category: 加液
  version: 1.0
  status: active
  last_updated: 2026/3/20
  owner: 徐翔宇
physical_specs:
  length_mm: 220
  width_mm: 180
  height_mm: 410
  weight_kg: 13.6
  module_slots: 0
  tray_slots: "六工位"
  mount_type: 平台外置
module_performance:
  volume_ml_min: 0
  volume_ml_max: 25
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: 650
  function_description: 吸吐液
  operation_target: 试剂瓶-500ml
  operation_workflow: 润管- 启动 -吸取溶剂1 - 阀体切换到注液1通道 - 排液 - 切换到吸液口 -吸取溶剂1 - 阀体切换到注液2通道 - 排液 - 溶剂2加液过程和溶剂1一样-实验结束-阀体切换到清洗4通道- 满量程吸取清洗液1 - 从5号通道排废-满量程吸取清洗液2- 从5号通道排废- - 满量程吸取清洗液1 - 从5号通道排废-结束
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 44
  consumables: "试剂瓶-500ml / "
  sub_functions: [651-清洗]
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
    []
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: ""
tags_meta:
  tags: [注射泵加液]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.0432
  power_kva_peak: 0.072
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: false
  requires_water: false
  requires_drain: true
---

# 25ml注射泵加液模块

## Purpose (用途说明)
将储液瓶内的液体精准加入到对应加液位置瓶内

## Typical Scenarios (典型应用场景)
实验室自动化前处理、体外诊断、食品安全检测、环境监测、生物制药、科研高通量实验

## Working Principle (工作原理)
依靠电机驱动带动注射器推杆匀速直线推进 / 后退，精准控制推杆位移，从而定量推送 / 抽取注射器内液体，实现高精度加液、排液

## Limits & Cautions (限制与注意事项)
注射泵针管内可能有气泡附着

## Remarks (备注)

