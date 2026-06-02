---
identification_version:
  id: MOD-CS-004
  module_part_no: HHM14D-050
  name: 50ml离心管多通道超声模块
  category: 超声
  version: 2.0
  status: active
  last_updated: 2025/6/6
  owner: 蔡勤超/深圳
physical_specs:
  length_mm: 250
  width_mm: 180
  height_mm: 175
  weight_kg: 25
  module_slots: 0
  tray_slots: "十六工位"
  mount_type: 托盘位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 50
  temperature_c_min: 25
  temperature_c_max: 60
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: 726
  function_description: 1#超声启动
  operation_target: 50ml离心管
  operation_workflow: 冷却循环水开启 - 放入离心管 - 超声振子开启 - 超声 - 停止 - 取出离心管 - 置于蒸发槽内 - 离心管底部水珠吸入海绵并蒸干 - 取走离心管
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 300
  consumables: 50ml离心管
  sub_functions: [727-2#超声启动, 728-排废水]
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
    - 称量模块
  typical_pairings:
    - --和模块配合使用
    - 离心管拆盖模块
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [50ml离心管，超声]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.23
  power_kva_peak: 0.37
  voltage_v: 220
  noise_db: 72
  heat_generating: "true"
  requires_ventilation: false
  requires_compressed_air: false
  requires_water: true
  requires_drain: true
---

# 50ml离心管多通道超声模块

## Purpose (用途说明)
本模块用于50ml 离心管样品的多通道并行超声处理，适配样品分散、溶解、萃取、混匀、脱气等前处理场景，支持不停机续料 / 取料，提升样品处理效率。

## Typical Scenarios (典型应用场景)
1、药物样品超声溶解
2、生物样品提取 / 混匀
3、化学样品分散 / 脱气
4、香精 / 精油 / 微量样品萃取

## Working Principle (工作原理)
通过超声空化效应处理样品，配合TEC 恒温 + 液位自控保障条件稳定，实现 50ml离心管多通道并行、不停机上下料的自动化超声前处理。

## Limits & Cautions (限制与注意事项)
1、水位控制：必须在设定液位范围内运行；严禁无水 / 低水位超声，会烧毁振子。
2、放瓶要求：瓶子直立、平稳放置，避免倾倒、进水。
3、动态上下料：中途取放瓶需确认工位到位、动作平稳，避免碰撞、溅水。
4、温度监控：超声中实时关注水温，异常立即停机。
5、后处理：取出后擦拭瓶底残留水，避免带入后续模块。
6、维护：定期换水、清洁水槽；长期不用排空水、断电

## Remarks (备注)

