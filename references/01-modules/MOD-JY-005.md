---
identification_version:
  id: MOD-JY-005
  module_part_no: HHM10L-010
  name: 10ml注射泵加液模块
  category: 加液
  version: 1.0
  status: active
  last_updated: 2025/10/17
  owner: 徐翔宇
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 15
  module_slots: 1
  tray_slots: "四工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 10
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
  operation_workflow: 放入容量瓶/PP瓶 - 夹紧 - 上样针移至吸液位 - 针头伸入瓶内液面下 - 吸入样品液 - 上样针移至放液位 - 针头伸入瓶内（瓶颈） - 放液 - 排出管路体积 - 上样针移至清洗位 - 针头内外同时冲洗 - 上样针移至吹干位 - 吹干 - 取下容量瓶/PP瓶
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 44
  consumables: 500ml溶剂瓶
  sub_functions: [651 启动右注射泵，652 清洗左注射泵，653 清洗右注射泵]
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
  wear_parts: 10ml注射泵
tags_meta:
  tags: [10ml注射泵, 加液]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.045
  power_kva_peak: 0.075
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: false
  requires_water: true
  requires_drain: false
---

# 10ml注射泵加液模块

## Purpose (用途说明)
依靠丝杆推动注射器柱塞匀速行进，实现高精度、小体积至大体积精准加注，满足实验严格配比要求，误差小、重复性好。

## Typical Scenarios (典型应用场景)
成都食检院

## Working Principle (工作原理)
由步进电机、丝杆传动机构、10ml 标准注射器及管路组件组成，电机驱动丝杆精准推动注射器推杆做直线往复运动；后退行程形成负压自动抽取试剂，前进行程匀速推送液体，依靠精准行程控制固定出液体积，实现10ml 量程内精准定量、恒速平稳加液，可精准控制加液流量与启停节奏，完成自动化定量加注。

## Limits & Cautions (限制与注意事项)
无

## Remarks (备注)

