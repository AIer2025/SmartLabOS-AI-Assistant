---
identification_version:
  id: MOD-CS-001
  module_part_no: HHM14E-010
  name: 10ML西林瓶多通道超声模块
  category: 超声
  version: 1.0
  status: active
  last_updated: 2025/10/17
  owner: 王志
physical_specs:
  length_mm: 400
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "四工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 10
  temperature_c_min: 20
  temperature_c_max: 30
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: 冷水机
process_capability_1:
  function_code: 726
  function_description: 1#超声启动
  operation_target: 10ml西林瓶
  operation_workflow: 冷却循环水开启 - 超声水槽水温达标（25±5℃）- 放入西林瓶 - 超声振子开启 - 超声（60min）- 停止 - 取出西林瓶 - 置于蒸发槽内 - 西林瓶底部水珠吸入海绵并蒸干 - 取走西林瓶
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: NA
  consumables: 西林瓶-10ml
  sub_functions: [726 1#超声启动 727 2#超声启动 728 排废水]
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
  wear_parts: 无
tags_meta:
  tags: [超声，10ml西林瓶]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.3444
  power_kva_peak: 0.574
  voltage_v: 24
  noise_db: 65
  heat_generating: "true"
  requires_ventilation: false
  requires_compressed_air: false
  requires_water: true
  requires_drain: true
---

# 10ML西林瓶多通道超声模块

## Purpose (用途说明)
按设定超声时间计时，取出已经达到超声时间的西林瓶，不需要停止其他超声中的西林瓶

## Typical Scenarios (典型应用场景)
1. 样品前处理消解加速
2. 药物溶出/均匀化
3. 放射性核素均匀分散
4. 标准品/质控样制备

## Working Principle (工作原理)
模块搭载多组独立超声振子，精准对应 10ml 标准西林瓶工位，设备通电后超声换能器将电能转化为高频机械超声波振动；振动能量平稳传导至瓶内液体，形成空化效应、湍流效应与微射流作用，快速实现样品破壁、萃取、混匀、溶解、乳化等处理；多通道同步独立工作，可批量同步处理样品，能量均匀输出，保证各组样品处理效果一致，整体结构贴合瓶体，超声传导损耗低，处理效率高。

## Limits & Cautions (限制与注意事项)
核心限制 = 仅限10mL瓶 + 4瓶/批 + 噪音/电磁干扰大 + 无自动终点判断
凡是"瓶型不对、要大批量、要自动判断终点、不做防辐射处理"的场景，都需重新评估是否适用本模块。

## Remarks (备注)

