---
identification_version:
  id: MOD-PY-001
  module_part_no: HHM01B-901
  name: 25-200ml容量瓶平摇模块
  category: 平摇
  version: 1.0
  status: active
  last_updated: 2025/8/8
  owner: 黄锐
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 25
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 25
  volume_ml_max: 200
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 100
  speed_rpm_max: 1000
  pressure_bar_min: 1
  pressure_bar_max: 6
  vacuum_kpa: -50
peripherals:
  required_peripherals: 空压机
process_capability_1:
  function_code: 510
  function_description: 兼容25ml-200ml容量瓶的上下料、平摇功能
  operation_target: 25ml容量瓶/50ml容量瓶/100ml容量瓶/200ml容量瓶
  operation_workflow: "上料：机械手把2个相同规格的容量瓶移动至上料位，光电感应器检测到容量瓶，确认上料成功；随后夹紧气缸夹紧容量瓶； / 平摇：平面摇匀开始，容量瓶零位开始圆周旋转，速度按照设定速度旋转；摇匀结束，回到零位。 / 下料：下料确认，机械手或人手取走容量，传感器检测不到目标，确认下料成功。"
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 260
  consumables: 25ml容量瓶/50ml容量瓶/100ml容量瓶/200ml容量瓶
  sub_functions: [NA]
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
    []
  conflicts:
    - 自动化称量模块
  typical_pairings:
    - NA
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [平摇、混匀、]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.08
  power_kva_peak: 0.147
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 25-200ml容量瓶平摇模块

## Purpose (用途说明)
用于容量瓶定容后的翻转摇匀操作

## Typical Scenarios (典型应用场景)
液体混匀、液体反应

## Working Principle (工作原理)
模块由步进电机、减速机、夹紧气缸、回转压紧气缸及电控组件组成。DC24V 供电，支持 25–200ml 标准容量瓶，2 通道并行。传感器检测到位后气缸夹紧，电机驱动 360° 匀速翻转摇匀，完成后回零位，等待机械手或人工下料，实现自动化摇匀

## Limits & Cautions (限制与注意事项)
仅适配 25–200ml 标准容量瓶，固定 2 通道并行；仅支持 360° 匀速翻转，角度 / 变速不可调。DC24V 供电、156W 功率，依赖机械手上下料；禁异形、破损或超规格瓶，夹紧力需适中防脱落，安装需水平，定期校准传感器与气缸。

## Remarks (备注)

