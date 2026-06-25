---
identification_version:
  id: MOD-HC-002
  module_part_no: HHM17A-050
  name: 50ml合成反应制冷反应器模块
  category: 合成
  version: 1.0
  status: active
  last_updated: 2025/9/12
  owner: 唐宋
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 35
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 50
  temperature_c_min: -20
  temperature_c_max: 25
  speed_rpm_min: 100
  speed_rpm_max: 2000
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: 冷水机,空压机，氮气瓶
process_capability_1:
  function_code: 301
  function_description: 制冷合成反应
  operation_target: 50ml两口反应瓶
  operation_workflow: 冷却循环水开启 - 反应器治具达到设定温度 - 反应瓶放入治具 - 治具移至反应位 - 定位气缸夹紧反应瓶口 - 主口密封气缸下压 - 侧口密封气缸下压 - 抽真空至设定压力 - 通入氮气 - 重复3次 -冷却循环水开启 - 反应器治具达到设定温度 开始合成反应 - 添加反应试剂 - 反应完毕 - 停止吹氮气 - 停止加热 - 开启冷却吹气 - 反应器降至室温 - 冲洗冷凝管 - 密封气缸抬起 - 定位气缸松开瓶口 - 反应器治具移至上下料位 - 取下反应瓶
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 36000
  consumables: 两口反应瓶-50ml
  sub_functions: []
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
  wear_parts: 半导体制冷片
tags_meta:
  tags: [制冷合成反应, 磁力搅拌]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.639
  power_kva_peak: 1.065
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: false
---

# 50ml合成反应制冷反应器模块

## Purpose (用途说明)
本模块是有机合成与精细化工自动化前处理设备，专为50ml 两口反应瓶设计，集成低温制冷、真空氮气置换、磁力搅拌、密闭反应功能，实现无氧低温合成反应自动化，保障反应可控、安全、高效。

## Typical Scenarios (典型应用场景)
广泛用于有机合成实验室、医药中间体研发、精细化工、新材料合成、高校科研院所，适配低温加成、缩合、氧化还原、格氏反应等需无氧低温环境的合成工艺。

## Working Principle (工作原理)
利用制冷（低温）把反应产物冷凝 / 液化分离，同时用制冷循环的余热预热原料，让合成反应在 “反应 — 制冷分离 — 预热循环” 下连续、低能耗进行。

## Limits & Cautions (限制与注意事项)
1.低温操作需佩戴防冻手套，防止冻伤；严禁触碰制冷台面。
2.密封前确认瓶口干净无破损，避免漏气、漏液。
3.外接氮气、真空、冷却水需符合规格，定期检查管路密封性
4.运行中严禁开盖、触碰运动部件，防止夹伤、喷溅。
5.定期清洁密封件、校准温度与压力，避免老化失效。
6.强腐蚀、易结晶、高粘度液体慎用，易损坏制冷与密封组件

## Remarks (备注)

