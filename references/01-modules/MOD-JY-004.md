---
identification_version:
  id: MOD-JY-004
  module_part_no: HHM10G-002
  name: 萃取瓶加内标模块
  category: 加液
  version: 1.0
  status: active
  last_updated: 2025/11/17
  owner: 宾俊
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "单工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 0.01
  volume_ml_max: 0.1
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
  function_code: 250
  function_description: 加内标
  operation_target: 250ml萃取瓶，500ml萃取瓶，1250ml萃取瓶，2ml西林瓶，10ml西林瓶
  operation_workflow: 定位治具移至上下料位 - 放入萃取瓶/内标瓶 - 定位治具移至加内标品位 - 加标针对准内标瓶 - 加标针降至吸内标位 - 吸取内标准品 - 加标针上升 - 加标针对准萃取瓶 - 加标针降至加内标位 - 加内标准品 - 加标针上升 - 加标针对准清洗槽 - 加标针下降至清洗位 - 清洗槽内加有机溶剂清洗加标针 - 排废液 - 注射泵排空（两次）- 清洗槽内加纯水清洗加标针 - 排废液 - 注射泵排空（两次）-加标针对准清洗槽 - 加标针下降至清洗位 - 清洗槽内加有机溶剂清洗加标针- 排废液 - 注射泵排空（两次）-  加标针上升 - 定位治具移至上下料位 - 取下萃取瓶/内标瓶
  absolute_accuracy_pct: ±2%
  repeat_accuracy_pct: NA
  cycle_time_sec: 135
  consumables: "萃取瓶-250ml / 萃取瓶-500ml / 萃取瓶-1250ml / 西林瓶-2ml / 西林瓶-10ml"
  sub_functions: [250 加内标]
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
  wear_parts: 注射器
tags_meta:
  tags: [加内标，西林瓶，萃取瓶]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.2544
  power_kva_peak: 0.424
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: false
  requires_water: false
  requires_drain: true
---

# 萃取瓶加内标模块

## Purpose (用途说明)
URD萃取瓶加内标模块 = 自动化内标定量添加站
核心价值：精准（±2%）+ 闭管（防污染）+ 自清洗（三次清洗防交叉污染），专为250/500/1250mL萃取瓶的标准化内标添加而设计，是URD前处理流程中确保定量准确性与批间一致性的关键执行单元

## Typical Scenarios (典型应用场景)
北京海关

## Working Principle (工作原理)
通过精密定量泵送机构抽取固定体积内标溶液，配合精准瓶口定位机构，将内标液精准定量注入萃取瓶内；依靠伺服驱动控制吸排液行程与流速，保证每瓶加注量均匀一致，加注完成后自动吹扫清理管路，减少残留误差，实现自动化批量完成萃取瓶内标添加工序。

## Limits & Cautions (限制与注意事项)
无

## Remarks (备注)

