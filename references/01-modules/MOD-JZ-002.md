---
identification_version:
  id: MOD-JZ-002
  module_part_no: HHM06B-050
  name: 50ml离心管均质模块
  category: 均质
  version: 1.0
  status: active
  last_updated: 2025/4/29
  owner: 蔡勤超
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 25
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 40
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 1000
  speed_rpm_max: 30000
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: ""
process_capability_1:
  function_code: 115
  function_description: 均质
  operation_target: 50ml离心管
  operation_workflow: 载台移至上下料位 - 放入离心管（无盖） - 载台移至均质位 - 均质刀头下降伸进离心管内 - 开始均质过程（时间）- 停止 - 均质刀头抬起 - 清洗槽移至均质刀头清洗位 - 均质刀头下降伸进清洗槽内 - 清洗槽排水泵开启 - 清洗液电磁阀开启- 冲洗 - 停止 - 排水泵关闭 - 均质刀头抬起 - 载台移至上下料位 - 取下离心管
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 60
  consumables: 离心管-50ml
  sub_functions: []
module_up_unload_time:
  up_unload_time: 60s
platform_compatibility:
  compatible_platforms:
    []
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
  wear_parts: ""
tags_meta:
  tags: [均质，50ml离心管]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.2826
  power_kva_peak: 0.471
  voltage_v: 24
  noise_db: 87
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: true
---

# 50ml离心管均质模块

## Purpose (用途说明)
50ml尖底瓶内，可通过均质的方式，实现悬浮液中的分散物微粒化、均匀化，提高分悬浮液散物分布均匀性的功能。模块对溶液进行均质处理。模块里包含均质，清洗均质刀头等功能。

## Typical Scenarios (典型应用场景)
1、环境水样悬浮颗粒物高速均质分散前处理
2、食品 / 土壤样品50ml 尖底瓶均质乳化、打碎匀浆
3、生物样本细胞破碎、悬浊液均匀化处理

## Working Principle (工作原理)
利用高压 / 高剪切 + 空化 + 撞击的组合效应，把乳浊液 / 悬浮液中的分散相（如脂肪球、固体颗粒）超细粉碎并均匀分散，长期稳定不分层。

## Limits & Cautions (限制与注意事项)
1、仅限50ml 尖底瓶使用，严禁使用其他规格容器。
2、均质转速不得超过 30000r/min，防止飞溅、破损或过载。
3、均质与清洗时，严禁伸手触碰高速刀头，避免割伤。
4、刀头必须完全进入液面才可启动均质，防止空转甩液。
5、清洗槽必须保持液位，排水泵正常工作，避免溢液、短路

## Remarks (备注)

