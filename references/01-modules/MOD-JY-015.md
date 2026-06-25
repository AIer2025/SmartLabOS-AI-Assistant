---
identification_version:
  id: MOD-JY-015
  module_part_no: HHM02G-004
  name: 4ml西林瓶拆盖正压加液模块
  category: 加液
  version: 1.0
  status: active
  last_updated: 2025/4/10
  owner: 何向东
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 0.2
  volume_ml_max: 4
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
  function_code: 177
  function_description: 拆盖/加液/装盖
  operation_target: 4ml西林瓶
  operation_workflow: 载台移至上下料位 - 放入西林瓶- 移至拆盖位 - 电动夹爪下行 - 拆盖 -电动夹爪上行-移至加液位 - 加液- 移至拆盖位 - 电动夹爪下行 - 装盖 -电动夹爪上行-移至下料位
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 40
  consumables: 西林瓶-4ml
  sub_functions: [175-拆盖, 176-装盖]
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
    - 正压加液模块
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: 手指包胶
tags_meta:
  tags: [加液]
documentation:
  urs_url: ""
  urd_url: ""
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

# 4ml西林瓶拆盖正压加液模块

## Purpose (用途说明)
4ml西林瓶拆盖加液

## Typical Scenarios (典型应用场景)
1、水质 / 环境检测4ml 西林瓶自动拧盖、定量加液前处理
2、实验室试剂、标准液、缓冲液自动开盖 + 加注 + 封盖一体化作业
3、药品 / 生物样品微量试剂（0.5–2ml）精准加液场景

## Working Principle (工作原理)
模块通过 Y 轴、Z 轴联动定位，在拆盖位由电动旋转夹爪自动完成西林瓶拧盖与封盖；Y 轴移送至加液位后，6 路直通阀实现 0.5–2ml 试剂定量加注；全程由光电传感器检测瓶体与瓶盖，异常自动报警，完成拆盖、加液、装盖全自动作业

## Limits & Cautions (限制与注意事项)
1、仅限4ml 标准西林瓶使用，禁止混用其他规格容器。
2、拆盖 / 拧盖前必须夹紧瓶体，防止打滑、破损或旋盖失败。
3、加液时确保瓶口与加液头对准，避免液体溅出、腐蚀器件
4、运行时严禁伸手进入Y/Z 轴、电动夹爪、加液区域，防止夹伤

## Remarks (备注)

