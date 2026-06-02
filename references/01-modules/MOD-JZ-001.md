---
identification_version:
  id: MOD-JZ-001
  module_part_no: HHM06C-125
  name: "125mlPP瓶均质模块 "
  category: 均质
  version: 2.0
  status: active
  last_updated: 2025/4/29
  owner: 蔡勤超
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 125
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 1
  speed_rpm_max: 30000
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: ""
process_capability_1:
  function_code: 115
  function_description: 启动均质
  operation_target: 125mlPP瓶
  operation_workflow: 载台移至上下料位 - 放入PP瓶（无盖） - 载台移至均质位 - 均质刀头下降伸进PP瓶内 - 开始均质过程（时间）- 停止 - 均质刀头抬起 - 清洗槽移至均质刀头清洗位 - 均质刀头下降伸进清洗槽内 - 清洗槽排水泵开启 - 清洗液电磁阀开启- 冲洗 - 停止 - 排水泵关闭 - 均质刀头抬起 - 载台移至上下料位 - 取下PP瓶
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 90
  consumables: PP瓶-125ml
  sub_functions: [115 启动均质]
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
  wear_parts: 均质刀头
tags_meta:
  tags: [均质，125mlPP瓶]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.2826
  power_kva_peak: 0.471
  voltage_v: 24
  noise_db: 65
  heat_generating: "true"
  requires_ventilation: false
  requires_compressed_air: false
  requires_water: true
  requires_drain: true
---

# 125mlPP瓶均质模块 

## Purpose (用途说明)
125ml PP瓶内，可通过均质的方式，实现悬浮液中的分散物微粒化、均匀化，提高分悬浮液散物分布均匀性的功能。模块对溶液进行均质处理

## Typical Scenarios (典型应用场景)
食品 / 环境样品全自动均质分散前处理
针对125ml PP 瓶内的悬浮液、组织液、提取液进行高速均质、微粒化、均匀化，替代人工手动均质，自动完成均质 + 刀头自动清洗，满足样品前处理高效、稳定、无交叉污染要求。

## Working Principle (工作原理)
模块夹持固定 125ml PP 试样瓶，驱动均质刀头伸入瓶内高速旋转剪切，利用机械剪切、撞击、湍流作用，击碎固体颗粒，使物料与液体充分混合分散，快速完成样品均质、破碎、乳化处理。

## Limits & Cautions (限制与注意事项)
1.仅适配125ml PP 瓶，必须放正、盖紧、无破损，禁止敞口、空瓶均质。
2. 均质刀头必须完全伸入液面再启动，严禁空转、干磨、撞瓶。
3. 转速≤30000r/min，运行中严禁触碰刀头、运动轴、气缸。
4. 每次均质后必须自动清洗刀头，防止残留与交叉污染。
5. 确保清洗液、气源、排水正常，缺水、无气、堵液禁止运行。
6. 异常立即急停，维护需断电、断气、刀头复位后操作。

## Remarks (备注)

