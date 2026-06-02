---
identification_version:
  id: MOD-JY-008
  module_part_no: HHM10L-005
  name: 5ml注射泵加液模块
  category: 加液
  version: 1.0
  status: active
  last_updated: 2025/10/17
  owner: 徐翔宇
physical_specs:
  length_mm: 250
  width_mm: 180
  height_mm: 300
  weight_kg: 15
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 5
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
  function_code: 650
  function_description: 启动左注射泵
  operation_target: 500ml溶剂瓶
  operation_workflow: 润管- 启动 -吸取溶剂1 - 阀体切换到注液1通道 - 排液 - 切换到吸液口 -吸取溶剂1 - 阀体切换到注液2通道 - 排液 - 溶剂2加液过程和溶剂1一样-实验结束-阀体切换到清洗4通道- 满量程吸取清洗液1 - 从5号通道排废-满量程吸取清洗液2- 从5号通道排废- - 满量程吸取清洗液1 - 从5号通道排废-结束
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 30
  consumables: ""
  sub_functions: [651-启动右注射泵, 652-清洗左注射泵，653-清洗右注射泵]
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
  tags: [加液，注射泵]
documentation:
  urs_url: ""
  urd_url: ""
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
  requires_drain: false
---

# 5ml注射泵加液模块

## Purpose (用途说明)
用注射泵从试剂瓶吸取一定量的液体，加入到样品管里

## Typical Scenarios (典型应用场景)
广泛用于实验室自动化、环境食品检测、医药生物、精细化工及分析仪器配套，实现试剂精准定量加注。

## Working Principle (工作原理)
由两套一体式注射泵 + 6 通切换阀 + PLC组成，实现两路液体精准定量输送、自动润管与清洗

## Limits & Cautions (限制与注意事项)
注射泵加液模块适用于洁净、低粘度液体的高精度加液，精度≤1%、重复性≤0.5%、单次≤20 秒；不可用于强腐蚀、高粘度、含颗粒液体及超量程工况。使用前需检查密封与气泡、先润管再加液、定期校准与清洗，避免空转和误接管路，保障稳定、洁净、精准供液

## Remarks (备注)

