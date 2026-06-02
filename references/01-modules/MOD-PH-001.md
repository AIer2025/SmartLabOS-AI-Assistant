---
identification_version:
  id: MOD-PH-001
  module_part_no: HHM31A-050-V2.0
  name: 50ml离心管pH调测模块V2.0（无触摸平版）
  category: pH
  version: 2.0
  status: active
  last_updated: 2026/4/11
  owner: 涂高祥
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 17
  module_slots: 1
  tray_slots: "单工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 0
  volume_ml_max: 50
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 1
  speed_rpm_max: 3000
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: 71
  function_description: PH值调测
  operation_target: 50ml离心管
  operation_workflow: "1.样品瓶放入样品载台 / 2.电极移到检测位,深入使样品液没过电极头 / 3. 测量样品pH值 / 4.电极移出至冲洗位,冲洗吹干  / 5.电极移至高位等待,加液通道加酸/碱（加液量参考混匀效果测试结果) / 6.混匀（时间参考混匀效果测试结果） / 7.电极移到检测位,深入使样品液没过电极头,测量样品pH值 / 8.电极移出至冲洗位,冲洗吹干 / 9.电极移至高位等待,取下样品瓶 / 10.电极保存液放入校正液载台,电极移至保存位电极浸泡到保存液中"
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 1603
  consumables: "pp瓶-30ml / 离心管-50ml"
  sub_functions: [72-校准液CP1清洗校准清洗，73-校准液CP2 校准清洗]
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
    - 正压加液系统
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [pH调测]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.258
  power_kva_peak: 0.43
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: true
---

# 50ml离心管pH调测模块V2.0（无触摸平版）

## Purpose (用途说明)
实现向50ml离心管里通过加酸/碱混匀，调节样品液pH值

## Typical Scenarios (典型应用场景)
1、公安法医毒物检测
2、环境样品前处理
3、食品 / 理化分析

## Working Principle (工作原理)
通过三轴运动实现 pH 电极精准定位并测定样品pH值，配合离心管混匀、电极液泵清洗、气动吹干，完成 50mL 离心管样品酸碱度自动化调节

## Limits & Cautions (限制与注意事项)
仅适配50ml 无盖离心管；适用水相样品，禁高粘度、高盐、含颗粒、强腐蚀、易结晶液体。适配浓盐酸、高氯酸、NaOH 溶液调 pH；电极怕污染、怕干放、怕硬物磕碰。环境 10–35℃、湿度 30%–70% RH，DC24V 供电、稳定气源。运行中禁伸手；定期校准电极、更换易损件；电极用完必清洗吹干；异常先断电排查

## Remarks (备注)

