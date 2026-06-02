---
identification_version:
  id: MOD-YY-005
  module_part_no: HHM09H-015
  name: 15ml离心管液体全量转移模块V1.0
  category: 移液
  version: 1.0
  status: active
  last_updated: 2025/7/25
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
  volume_ml_min: 0
  volume_ml_max: 15
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
  function_code: 160
  function_description: 全量转移
  operation_target: 15ml带盖离心管，50ml不带盖离心管
  operation_workflow: 上料：两个不带盖的空50ml离心管，两个带盖带料的15ml离心管；拆盖：带盖带料15ml离心管拆盖；倒液：抓起两个带料离心管，移到空离心管上方，带料离心管倾斜，将上层清液倒入空离心管后立起；装盖：带料离心管装盖；下料：2个带盖带料的15ml离心管，两个带料不带盖的50ml离心管。
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: ""
  consumables: "离心管-15ml / 离心管-50ml"
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
  wear_parts: 手指胶皮
tags_meta:
  tags: [全量转移，15ml离心管]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.31
  power_kva_peak: 0.52
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 15ml离心管液体全量转移模块V1.0

## Purpose (用途说明)
15ml全量转移模块是实现上层清液从一个容器全部倒入另一个容器的操作。

## Typical Scenarios (典型应用场景)
1、分子生物学：核酸提取前处理
2、生化与蛋白：离心后上清转移
3、微生物：样本富集与除菌
4、药物研发：发酵液 / 中间体处理

## Working Principle (工作原理)
以正向置换式注射泵为核心，配合高精度 XYZ 运动 + 多传感器闭环，通过 “精准吸液→无残留排液 + 强吹” 流程，实现15 mL 液体 100% 转移、残留 < 5 μL、精度 ±0.5%，广泛用于生化、免疫、样品前处理等场景。

## Limits & Cautions (限制与注意事项)
本模块仅适配 15ml 带盖管上清全量转入 50ml 无盖管，固定 2 管并行；仅适用于分层、流动性好的上清，禁高粘、腐蚀、易燃液。单批约 50 秒、无计量精度、微量残留；尺寸固定，需 DC24V 供电与 6mm 气源，通信仅支持 MODBUS TCP，流程自定义受限

## Remarks (备注)

