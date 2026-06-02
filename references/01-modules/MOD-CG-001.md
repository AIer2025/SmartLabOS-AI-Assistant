---
identification_version:
  id: MOD-CG-001
  module_part_no: HHM02F-901
  name: 30ml/125mlPP瓶拆装盖模块
  category: 拆盖
  version: 1.0
  status: active
  last_updated: 2025/3/14
  owner: 黄文兴
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 15
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 30
  volume_ml_max: 125
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
  function_code: 91
  function_description: 完成125mlPP瓶和30mlPP瓶的拆装盖，可以拆同一个规格瓶，也可以左右两边拆装的不是同一个规格瓶
  operation_target: 125mlPP瓶，30mlPP瓶
  operation_workflow: 上料：2个带盖的125mlPP瓶/1个125mlPP瓶和1个30mlPP瓶；拆盖：2个带盖PP瓶拆盖；装盖：2个带盖PP瓶装盖；下料：2个带盖的125mlPP瓶/1个125mlPP瓶和1个30mlPP瓶
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 80
  consumables: "125mlPP瓶 / 30mlPP瓶 / "
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
    []
  conflicts:
    []
  typical_pairings:
    []
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [拆盖]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.1344
  power_kva_peak: 0.224
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 30ml/125mlPP瓶拆装盖模块

## Purpose (用途说明)
用于实现有机样品净化，通过SPE色谱柱分离除去样品中杂质

## Typical Scenarios (典型应用场景)
无机金属离子检测

## Working Principle (工作原理)
模块主要由Y轴、Z轴滚珠丝杆电机、钧舵电爪、Y轴载台、光电传感器、手指气缸、电磁阀等组成。 其中手指气缸夹紧瓶身后，Y轴电机带动载台移动，Z轴电机带动电爪进行直线运动，下降到合适高度进行拆盖

## Limits & Cautions (限制与注意事项)
核心原则：速度不超50mm/s、精度守住±0.1mm、气压稳在0.5MPa、夹紧力必须反馈确认、单独测试必须手动按钮——任何一条红线都不能碰

## Remarks (备注)

