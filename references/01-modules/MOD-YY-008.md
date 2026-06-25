---
identification_version:
  id: MOD-YY-008
  module_part_no: HHM12A-250
  name: 250ml锥形瓶加内标模块
  category: 移液
  version: 1.0
  status: active
  last_updated: 2025/9/12
  owner: 宾俊
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 0.1
  volume_ml_max: 0.4
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
  function_code: 250
  function_description: 加内标
  operation_target: 250ml锥形瓶
  operation_workflow: 定位治具移至上下料位 - 放入锥形瓶/内标瓶 - 定位治具移至加内标品位 - 加标针对准内标瓶 - 加标针降至吸内标位 - 吸取内标准品 - 加标针上升 - 加标针对准锥形瓶 - 加标针降至加内标位 - 加内标准品 - 加标针上升 - 加标针对准清洗槽 - 加标针下降至清洗位 - 清洗槽内加有机溶剂清洗加标针 - 排废液 - 注射泵排空（两次）- 清洗槽内加纯水清洗加标针 - 排废液 - 注射泵排空（两次）- 清洗槽内加有机溶剂清洗加标针 - 排废液 - 注射泵排空（两次）- 加标针上升 - 定位治具移至上下料位 - 取下锥形瓶/内标瓶
  absolute_accuracy_pct: ±2%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 97
  consumables: 锥形瓶-250ml
  sub_functions: [251-清洗标针]
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
  wear_parts: 加液电磁阀
tags_meta:
  tags: [加内标]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.27
  power_kva_peak: 0.45
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: false
  requires_water: true
  requires_drain: true
---

# 250ml锥形瓶加内标模块

## Purpose (用途说明)
广泛用于环境监测、食品检测、化工、医药、科研实验室，为水质、土壤、食品、化工原料等样品的色谱、质谱分析，提供精准内标添加，保障定量分析准确性

## Typical Scenarios (典型应用场景)
该模块是实验室样品前处理自动化设备，双通道适配 250ml 锥形瓶，精准添加 100μL/400μL 内标液，精度 ±2%。自动完成吸标、加标、有机 - 水 - 有机三步洗针，杜绝交叉污染。广泛用于环境、食品、化工、医药等领域，为色谱、质谱分析提供精准内标添加，高效稳定、数据可靠。

## Working Principle (工作原理)
由 PLC 控制 Y/Z 轴定位、加标泵与洗针组件，双通道自动作业。精准吸取 100μL/400μL 内标液并注入锥形瓶，完成后经有机 - 水 - 有机三步洗针、气吹防交叉污染，全流程自动化、精度高

## Limits & Cautions (限制与注意事项)
100ul/400ul，通过更换注射器实现

## Remarks (备注)

