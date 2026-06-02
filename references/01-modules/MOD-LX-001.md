---
identification_version:
  id: MOD-LX-001
  module_part_no: HHM15C-050
  name: 50ml×4孔位高速离心机模块（带XYZ）
  category: 离心
  version: 1.0
  status: active
  last_updated: 2025/8/14
  owner: 宾俊
physical_specs:
  length_mm: 800
  width_mm: 850
  height_mm: 2050
  weight_kg: 201
  module_slots: 3
  tray_slots: "四工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: "NA"
  volume_ml_max: "NA"
  temperature_c_min: 2
  temperature_c_max: 7
  speed_rpm_min: 1
  speed_rpm_max: 8000
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: -80
peripherals:
  required_peripherals: 真空泵，压缩机
process_capability_1:
  function_code: 188
  function_description: 离心机  启动：
  operation_target: 50ml离心管
  operation_workflow: "1.离心机预冷 / 2.取盖 / 3.定位 / 4.上料 / 5.盖盖 / 6.离心启动 / 7.离心停止"
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 0-300
  consumables: 离心管-50ml
  sub_functions: [185-离心机定位, 186-预冷, 187-取盖，189-除水，190-停止]
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
  wear_parts: 手指包胶
tags_meta:
  tags: [离心]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 3.5
  power_kva_peak: 4.8
  voltage_v: 24
  noise_db: 67
  heat_generating: "true"
  requires_ventilation: true
  requires_compressed_air: false
  requires_water: false
  requires_drain: false
---

# 50ml×4孔位高速离心机模块（带XYZ）

## Purpose (用途说明)
固液分离，液液分离，浓缩提纯，沉降分层，分级筛选

## Typical Scenarios (典型应用场景)
血样；团泊三号；青海盐湖所

## Working Principle (工作原理)
利用高速旋转产生的离心力，替代重力，让密度不同的物质快速沉降、分层，实现分离。

## Limits & Cautions (限制与注意事项)
离心速度不要高于 8000Rpm；上料重量一定要均衡

## Remarks (备注)

