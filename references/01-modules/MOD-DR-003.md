---
identification_version:
  id: MOD-DR-003
  module_part_no: HHM08C-901
  name: 注射器定容模块（250ul/1000ul）
  category: 定容
  version: 1.0
  status: active
  last_updated: 2026/3/19
  owner: 徐翔宇
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 21
  module_slots: 1
  tray_slots: "单工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 0.25
  volume_ml_max: 1
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: ""
process_capability_1:
  function_code: 660，661
  function_description: 加标
  operation_target: 50ml离心管
  operation_workflow: 载台移至上下料位 - 放入西林瓶/离心管/样品瓶 - 载台移至吸样品位 - 样品针下降 - 润针-吸样品 - 样品针上升 - 载台旋转至加样品位 - 样品针下降 - 吐液 -移液针上升 -重复移液动作- 载台移至清洗位 - 清洗槽内加有机溶剂 - 移液针下降 - 清洗 -吸入吐出 -- 蠕动泵开启 - 清洗槽排空 - 蠕动泵关闭 - 移液针排空2次 - 移液针上升 - 清洗槽内加纯水 - 移液针下降 - 清洗-吸入吐出 -  - 蠕动泵开启 - 清洗槽排空 - 蠕动泵关闭 - 移液针排空2次 - 移液针上升 - 清洗槽内加有机溶剂 - 移液针下降 - 清洗-吸入吐出 - - 蠕动泵开启 - 清洗槽排空 - 蠕动泵关闭 - 移液针排空2次 - 移液针上升 - 载体移至上下料位 - 取下西林瓶/离心管/样品瓶
  absolute_accuracy_pct: ±0.6%
  repeat_accuracy_pct: ≤0.2%
  cycle_time_sec: 270
  consumables: "西林瓶-10ml / 西林瓶-2ml / 离心管-15ml"
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
    []
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: 注射器
tags_meta:
  tags: [定容]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.138
  power_kva_peak: 0.23
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: false
  requires_water: true
  requires_drain: true
---

# 注射器定容模块（250ul/1000ul）

## Purpose (用途说明)
往样品管定量注入试剂，支持试剂管的自动上料及位置旋转切换，自动洗针等功能

## Typical Scenarios (典型应用场景)
1、理化 / 化学分析标准溶液配制
2、环境监测水样 / 土壤样品定量稀释
3、食品检测微量样品定量分装
4、生物医药试剂 / 培养基微量配制
5、科研高通量微量反应体系构建

## Working Principle (工作原理)
通过进出旋转接料平台实现管路位置切换，搭配双组独立升降、抽液轴完成注射器吸液、定量注液动作，并由双工位洗针机构自动清洗进样器，以此实现精准、高效的样品与试剂移液定容作业。

## Limits & Cautions (限制与注意事项)
仅适配 25/100/250/1000μL 进样器与标准微量管；适用低粘度水相 / 常规试剂，禁高粘、结晶、强腐蚀、含颗粒液体。环境要求 18–28℃、湿度 40%–60% RH，DC24V 稳定供电。运行中禁止干预，定期校准进样器、更换针头；换试剂需充分洗针防交叉污染；堵针 / 漏液 / 卡顿先断电处理。

## Remarks (备注)

