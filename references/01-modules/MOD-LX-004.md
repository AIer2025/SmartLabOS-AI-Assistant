---
identification_version:
  id: MOD-LX-004
  module_part_no: HHM15B-602
  name: 2ml-6孔位高速离心模块V4.0（带XYZ）
  category: 离心
  version: 1.0
  status: active
  last_updated: 2025/6/6
  owner: 杨宇/深圳
physical_specs:
  length_mm: 800
  width_mm: 850
  height_mm: 2055
  weight_kg: 45
  module_slots: 3
  tray_slots: "六工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 0.5
  volume_ml_max: 2
  temperature_c_min: 1
  temperature_c_max: 10
  speed_rpm_min: 1000
  speed_rpm_max: 14000
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: -75
peripherals:
  required_peripherals: 真空泵
process_capability_1:
  function_code: 188
  function_description: 离心机启动
  operation_target: 2ml离心管
  operation_workflow: 放入离心管1-点击按钮-放入离心管2-点击按钮-放入离心管3-点击按钮-放入离心管4-点击按钮-放入离心管5-点击按钮-放入离心管6-盖塞盖-气缸推出-卡紧塞盖-真空泵开启-合开开启(提制)-离心5min-停止-气缸退回-打开塞盖-依次取出离心管
  absolute_accuracy_pct: ""
  repeat_accuracy_pct: ""
  cycle_time_sec: 420
  consumables: 离心管-2ml
  sub_functions: [185-离心机定位, 186-离心机预冷, 187-离心机取盖, 188-离心机启动, 189-离心机除水, 190-离心机停止]
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
  wear_parts: ""
tags_meta:
  tags: [2ml离心管，离心]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 1
  power_kva_peak: 1.5
  voltage_v: 220
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 2ml-6孔位高速离心模块V4.0（带XYZ）

## Purpose (用途说明)
实现多个2ml离心管带盖溶液离心，离心转速和时长可设定、调节。

## Typical Scenarios (典型应用场景)
血样

## Working Principle (工作原理)
设备先完成电机轴回零、提篮对位下料口、上下料门开启的初始化；通过机械手交替向离心机提篮投放待处理 离心管，满负荷后关门启动离心，电机按设定转速（14000r/min）和时长（5min）运行，振动传感器实时监测三轴位移，超阈值（300um）则降速停机报警；离心结束后，提篮归位、开门，机械手取出成品，完成自动化离心作业循环。

## Limits & Cautions (限制与注意事项)
1、仅用/2ml 离心管，4 孔对称装样，禁止偏载、破损管、无盖管。
2、转速≤14000r/min，振动＞300μm、真空未达标禁止离心。
3、必须塞盖密封 + 抽真空后启动，运行中严禁开门、撬盖。
4、停机必做 2000 转 / 30 分钟除水，不可省略。
5、异常立即急停，断电破真空再检修，禁止强掰、硬撬、改参数。

## Remarks (备注)

