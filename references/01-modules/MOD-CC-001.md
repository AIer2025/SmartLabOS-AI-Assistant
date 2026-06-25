---
identification_version:
  id: MOD-CC-001
  module_part_no: HHM43E-002
  name: 800样品存储模块
  category: 存储
  version: 1.0
  status: active
  last_updated: 2025/11/21
  owner: 杨宇
physical_specs:
  length_mm: 850
  width_mm: 800
  height_mm: 600
  weight_kg: 210
  module_slots: 3
  tray_slots: "九十工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: "NA"
  volume_ml_max: "NA"
  temperature_c_min: 0
  temperature_c_max: 10
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: 空压机
process_capability_1:
  function_code: D210
  function_description: 该模块实现样品冷藏存储功能，并能够通过XYZ机械臂自动取放样品，可通过温度设置，控温0℃或10℃
  operation_target: 2ml西林瓶
  operation_workflow: "初始化-预冷- 上样准备-带料托盘放至平台-传感器反馈-气缸动作定位销伸出-机械手从托盘夹取西林瓶 -移至冷藏口- 气缸动作冷藏门打开 -放料到转盘-机械手取下一个料-盖子盖好- 重复上料到上料完成 /  / 样品出库-下料准备-空托盘放至平台-传感器反馈-气缸动作定位销伸出-机械手移至冷藏口-气缸动作冷藏门打开 -机械手从转盘取料-放料到托盘-气缸动作推杆门关闭-重复下料动作到下料完成"
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 31
  consumables: 西林瓶-2ml
  sub_functions: [D210 存储]
module_up_unload_time:
  up_unload_time: 60s
platform_compatibility:
  compatible_platforms:
    - PLT-800
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
  tags: [样品存储，2ml西林瓶]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 1.44
  power_kva_peak: 2.4
  voltage_v: 220
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: true
---

# 800样品存储模块

## Purpose (用途说明)
该模块实现样品冷藏存储功能，并能够通过XYZ机械臂自动取放样品。控温需求0~10℃

## Typical Scenarios (典型应用场景)
它解决的核心痛点是：在自动化流程中，让温度敏感的试剂/样本全程不离开冷链、不被人工触碰、不被温度波动影响——从临床免疫到分子诊断，从CGT到药物研发，凡是"液体+低温+自动化"的场景，都是它的主场

## Working Principle (工作原理)
800 样品存储模块（通常指可容纳约 800 支标准样本管 / 冻存管的自动化高密度存储单元），核心工作原理是：矩阵式高密度仓位存储 + 机械 / 气动自动存取 + 精确定位与环境控管 + 智能信息绑定，实现样本批量、稳定、可追溯、自动化存储与快速调取。

## Limits & Cautions (限制与注意事项)
2mL西林瓶90位0-10°C存储模块 = "冷链不断裂"的自动化中间站
核心记住三个字：温（度精准）、洁（闭管防污）、序（FIFO追溯），做到这三点，90%的问题都不会发生

## Remarks (备注)

