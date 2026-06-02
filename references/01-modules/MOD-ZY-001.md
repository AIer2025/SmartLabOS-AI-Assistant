---
identification_version:
  id: MOD-ZY-001
  module_part_no: HHM01K-150
  name: 150ml面包瓶锥形瓶振摇模块
  category: 振摇
  version: 1.0
  status: active
  last_updated: 2026/4/17
  owner: 唐宋
physical_specs:
  length_mm: 469
  width_mm: 224
  height_mm: 165
  weight_kg: 40
  module_slots: 3
  tray_slots: "四工位"
  mount_type: 托盘位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 150
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 100
  speed_rpm_max: 3000
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: ""
process_capability_1:
  function_code: 590
  function_description: 面包瓶/锥形瓶振摇
  operation_target: 150ml面包瓶/锥形瓶
  operation_workflow: 初始化 -放第一批2个面包瓶 - 振摇- 停止 - 放第二批2个面包瓶-振摇-第一批振摇时间到-下料第一批面包瓶-振摇-第二批振摇时间到-下料第二批面包瓶
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 1200
  consumables: 150ml面包瓶,150ml锥形瓶
  sub_functions: [591-搅拌中途上料]
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
  tags: [振摇, 混匀, 面包瓶]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.13
  power_kva_peak: 0.22
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: false
  requires_water: false
  requires_drain: false
---

# 150ml面包瓶锥形瓶振摇模块

## Purpose (用途说明)
实现150ml面包瓶和锥形瓶的振摇/水平振摇，使样品充分混匀

## Typical Scenarios (典型应用场景)
1、微生物 / 菌种培养振荡
2、样品萃取与固液混匀
3、试剂反应加速混匀


## Working Principle (工作原理)
依靠传感器识别物料，通过旋转轴运转带动瓶体晃动，配合机械手上下料，分时控时完成多批次物料交替振摇

## Limits & Cautions (限制与注意事项)
1、放瓶要求：瓶子底部放平、直立放稳，紧贴定位槽，防止振摇滑动、倾倒。
2、参数设置：根据样品粘度与性质选择转速，低粘度用中高速、高粘度用低速。
3、中途上下料：仅在平台回待机位时放取瓶子，避免碰撞、夹手。
4、运行观察：振摇中留意瓶子是否移位、液体是否溢出，异常立即停机。
5、日常维护：保持平台清洁干燥；定期检查电机、传感器；长期不用断电。

## Remarks (备注)

