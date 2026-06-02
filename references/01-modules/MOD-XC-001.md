---
identification_version:
  id: MOD-XC-001
  module_part_no: HHM02Q-002
  name: 2ml离心管/西林瓶拆盖吸磁模块
  category: 吸磁
  version: 1.0
  status: active
  last_updated: 2026/4/3
  owner: 蔡勤超
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 15
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: "NA"
  volume_ml_max: "NA"
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
  function_code: 165,166
  function_description: 拆装盖
  operation_target: 2ml离心管，2ml西林瓶
  operation_workflow: "1.2ml离心管或2ml西林瓶进行拆盖 / 2.2ml离心管或2ml西林瓶进行装盖"
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 97
  consumables: "离心管-2ml / 西林瓶-2ml"
  sub_functions: []
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
  wear_parts: 夹爪胶条
tags_meta:
  tags: [拆装盖吸磁]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.228
  power_kva_peak: 0.38
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 2ml离心管/西林瓶拆盖吸磁模块

## Purpose (用途说明)
用于2ml西林瓶或2ml离心管进行拆装盖操作，同时瓶壁两侧带磁吸可将样品中磁性物质吸附在壁上

## Typical Scenarios (典型应用场景)
血样

## Working Principle (工作原理)
通过电动夹爪下降至拆/拧盖位，夹紧瓶子，旋转电爪逆时针或顺时针旋转约3圈后，瓶盖拧开或盖上，另外在瓶壁两侧加装磁铁，将瓶内样品中磁性物质吸附至两侧

## Limits & Cautions (限制与注意事项)
仅适配标准 2ml 离心管 / 西林瓶及配套螺纹盖；高紧固 / 锈蚀瓶盖慎用。工作环境 10–35℃、湿度 30%–70% RH，DC24V 供电、0.5MPa 洁净气源。运行中禁干预；瓶管需直立卡紧；定期更换夹爪胶条、校准定位与力矩；打滑 / 异响先断电排查

## Remarks (备注)

