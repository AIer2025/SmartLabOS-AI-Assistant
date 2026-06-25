---
identification_version:
  id: MOD-DR-001
  module_part_no: HHM08A-902
  name: 50-250ml容量瓶定容平遥模块
  category: 定容
  version: 1.0
  status: active
  last_updated: 2026/5/13
  owner: 唐宋
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 55
  module_slots: 1
  tray_slots: "单工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 50
  volume_ml_max: 250
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 5
  pressure_bar_max: 6
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: 709
  function_description: " 50ml-250ml容量瓶： 拆盖-加液-定容-平遥-装盖"
  operation_target: 50ml容量瓶,100ml容量瓶,200ml容量瓶,250ml容量瓶
  operation_workflow: 机械臂放入容量瓶 -横移气缸左移- 相机拍照确认刻度线高度偏移-上料载台上下移动-夹紧-上料载台下降 - 横移气缸移拆盖位 - 拆盖-X轴移动到吐液池排液-X轴移动到容量瓶处-加液Z轴下降到加液位加液-平遥-启动高压定容-中压-低压定容液面与刻度相切 - 加液停止-装盖子-横移气缸左移到上料接驳位-上料Z轴上抬-机械臂下料
  absolute_accuracy_pct: ±0.15mm
  repeat_accuracy_pct: ±0.1mm
  cycle_time_sec: 36S
  consumables: 容量瓶-50ml,100ml,200ml,250ml
  sub_functions: [706-定容高压纯水, 707-定容中压纯水, 708-定容低压纯水, 705-拆盖/加液/平摇/装盖]
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
    - 正压加液
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [拆盖, 加液, 平遥, 定容, 装盖]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.05
  power_kva_peak: 0.12
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: true
---

# 50-250ml容量瓶定容平遥模块

## Purpose (用途说明)
50ml-250ml容量瓶 拆盖,加液,平摇,装盖

## Typical Scenarios (典型应用场景)
全自动定容和过滤工站

## Working Principle (工作原理)
通过正压加液器控制，向容量瓶加入一定量的液体，视觉定容至刻度线，通过平摇进行混匀。

## Limits & Cautions (限制与注意事项)
1、正压加液模块：10 通道溶剂 + 1 通道纯水，与拆盖模块联动控压；定容分高 / 中 / 低压，不可超压。

2、拆盖加液定容模块：定容精度 ±0.03mm，依赖刻度清晰；三段定容不可省慢速段；防交叉污染、超程运动。
翻转摇匀模块：适配标准容量瓶；翻转＞180°、≥10 次 / 分钟；真空 + 夹爪双重固定，缺一不可。

3、12 通道超声模块：容量瓶对称摆放；注水控液位，水温≤45℃；散热风扇常转，结束排净废水。

## Remarks (备注)

