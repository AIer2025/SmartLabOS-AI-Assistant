---
identification_version:
  id: MOD-YY-006
  module_part_no: HHM09L-250
  name: 250ml锥形瓶全量转移模块
  category: 移液
  version: 1.0
  status: active
  last_updated: 2025/8/29
  owner: 蔡勤超
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "单工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 15
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: 空压机
process_capability_1:
  function_code: 580
  function_description: 250ml锥形瓶液体全量转移模块是实现将皂化后的样品从一个容器全部抽入另一个容器，并加一定量的溶剂清洗原容器后再次抽入接收容器的操作。
  operation_target: 250ml锥形瓶，接收容器：250ml萃取瓶/100ml容量瓶
  operation_workflow: "上料：一个不带盖的空250ml萃取瓶或不带盖的100ml容量瓶，一个带盖带料的250ml锥形瓶； / 全量转移：通过管路将锥形瓶中的样品抽到接收容器中。注意：底部带磁吸，可将磁力搅拌子吸在锥形瓶底部的中间位置，同时倾斜锥形瓶，用管路将样品吸干净（就是磁力搅拌子不能影响管理针头吸样品）； / 清洗：向锥形瓶中喷加一定量的液体清洗瓶壁，（如30ml水或20ml乙醇-水溶液）加液通道2，加液量：20-100ml可设置，精度±2% / 下料：2个带盖带料的15ml离心管，两个带料不带盖的50ml离心管。 / 下料：一个不带盖的250ml萃取瓶或不带盖的100ml容量瓶，一个带盖的250ml锥形瓶；"
  absolute_accuracy_pct: 250ml
  repeat_accuracy_pct: NA
  cycle_time_sec: 507
  consumables: "250ml锥形瓶 / 250ml萃取瓶/100ml容量瓶"
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
    - 250ml锥形瓶恒温振荡模块
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [全量转移]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.1
  power_kva_peak: 0.167
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: true
---

# 250ml锥形瓶全量转移模块

## Purpose (用途说明)
用于实验室样品前处理，自动将 250ml 锥形瓶内液体全量转移至 250ml 萃取瓶，集成瓶体定位、注射泵抽排液、锥形瓶与管路自动清洗功能。适配食品、环境、医药等检测场景，实现全量移液自动化，减少人工操作误差，降低污染风险，提升样品处理效率与标准化水平

## Typical Scenarios (典型应用场景)
用于实验室前处理，适配 250ml 锥形瓶→萃取瓶全量转移。典型案例：食品农残 / 真菌毒素提取液转移、环境水样萃取分相、医药中间体 / 发酵液转瓶、固相萃取后洗脱液归集、化工样品净化液转移。自动化全量移液、自带清洗，防污染、降误差、提通量

## Working Principle (工作原理)
由 Y/Z 轴电机、注射泵、电磁阀、定位载台及清洗组件构成。载台定位瓶位，Z 轴带动取液针下移，注射泵抽液，电磁阀切换管路排至萃取瓶；重复抽排实现全量转移。转移后自动喷淋清洗瓶体与管路，排废回收，完成自动化移液与清洗流程

## Limits & Cautions (限制与注意事项)
仅适配标准 250ml 锥形瓶与萃取瓶，固定规格不兼容非标容器。仅适用于低粘度、无固体颗粒、不易结晶液体；禁强腐蚀、高粘度、易堵样品。依赖 DC24V 供电与稳定气源，全量转移存在微量残留；管路易堵，需定期清洗维护，安装需水平稳固防漏液。

## Remarks (备注)

