---
identification_version:
  id: MOD-WX-004
  module_part_no: HHM03K-050
  name: 50ml离心管单涡旋模块
  category: 涡旋
  version: 1.0
  status: active
  last_updated: 2026/3/27
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
  speed_rpm_min: 1000
  speed_rpm_max: 3000
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: 223
  function_description: 涡旋
  operation_target: 50ml离心管
  operation_workflow: 载台移至上下料位-样品离心管放入卡槽-载台移至瓶盖检测位-确认上料成功--载台移至涡旋位-涡旋-载台移至上下料位-取走离心管
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 220
  consumables: 离心管-50ml
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
  wear_parts: 涡旋下胶皮，涡旋上胶皮
tags_meta:
  tags: [涡旋]
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

# 50ml离心管单涡旋模块

## Purpose (用途说明)
实现50ml离心管通过涡旋的方式使样品液充分混合/溶解

## Typical Scenarios (典型应用场景)
实验室前处理、生化 / 免疫检测、环境 / 食品检测、生物制药、科研实验

## Working Principle (工作原理)
模块依靠Y 轴定位 + 气动压紧 + 高速偏心涡旋 + 传感器闭环，实现 50ml 离心管自动对位、压紧、高速涡旋混匀、松压复位，核心是偏心旋转振动 + 气动稳定夹持 + 精准时序控制。

## Limits & Cautions (限制与注意事项)
仅适配标准 50ml 尖底离心管；适用低粘度水相 / 常规试剂，禁高粘、结晶、含颗粒、强腐蚀液体。最高 3000rpm、振幅 ±2mm，单次连续不超 10 分钟，单管液体≤45ml。环境 10–35℃、湿度 30%–70% RH，DC24V 供电、稳定气源。运行中禁干预；试管需卡紧；定期更换橡胶垫、校准转速；异响 / 漏液先断电处理

## Remarks (备注)

