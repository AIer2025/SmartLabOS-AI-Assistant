---
identification_version:
  id: MOD-HC-001
  module_part_no: HHM17A-050
  name: 50ml合成反应器模块-制热
  category: 合成
  version: 1.0
  status: active
  last_updated: 2025/7/17
  owner: 唐宋
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 40
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 50
  temperature_c_min: 26
  temperature_c_max: 200
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: 冷水机
process_capability_1:
  function_code: 300
  function_description: 启动制热反应
  operation_target: 50ml斜二口烧瓶
  operation_workflow: " 反应器治具达到设定温度 - 反应瓶放入治具 - 治具移至反应位 - 定位气缸夹紧反应瓶口 - 主口密封气缸下压 - 侧口密封气缸下压 - 抽真空至设定压力 - 通入氮气 - 重复3次 - 开始合成反应 - 添加反应试剂 - 反应完毕 - 停止吹氮气 - 停止加热 - 开启冷却吹气 - 反应器降至室温 - 冲洗冷凝管 - 密封气缸抬起 - 定位气缸松开瓶口 - 反应器治具移至上下料位 - 取下反应瓶；"
  absolute_accuracy_pct: ±1
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 36000
  consumables: 50ml斜二口烧瓶
  sub_functions: [NA]
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
  wear_parts: NA
tags_meta:
  tags: [合成反应]
documentation:
  urs_url: URS连接
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.36
  power_kva_peak: 0.65
  voltage_v: 220
  noise_db: 65
  heat_generating: "true"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: false
---

# 50ml合成反应器模块-制热

## Purpose (用途说明)
50ml斜二口玻璃瓶加热至200℃的合成反应器，本质上是一套"微型有机合成工作站"——主要用于小剂量有机合成反应、催化评价、反应条件优化，斜二口设计使其能同时实现控温+回流+通气，是制药、精细化工、新材料领域实验室最常用的小试反应装备之一

## Typical Scenarios (典型应用场景)
无

## Working Principle (工作原理)
通过电加热控温模块对斜二口玻璃反应瓶进行均匀加热，配合温度传感器实时采集瓶内 / 瓶壁温度，由控制系统自动调节加热功率，实现精准恒温、程序升温，为瓶内物料提供稳定高温反应环境，完成合成、催化、回流等化学反应

## Limits & Cautions (限制与注意事项)
这个模块最大的风险就两个：① 玻璃炸瓶（温差+暗伤），② 溶剂着火（没通N₂+没接冷凝管就加热）。只要记住"干瓶慢升温、先通气再加热、先冷却水再开火、全程有人看"，就能安全使用

## Remarks (备注)

