---
identification_version:
  id: MOD-JY-002
  module_part_no: HHM23K-250
  name: 250ml锥形瓶加磁力搅拌子模块
  category: 搅拌
  version: 1.0
  status: active
  last_updated: 2025/10/17
  owner: 汪健
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 25
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 0
  volume_ml_max: 250
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 100
  speed_rpm_max: 1000
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: 空压机
process_capability_1:
  function_code: 610
  function_description: 拆盖/加液/加磁力子/混匀/装盖
  operation_target: 250ml锥形瓶
  operation_workflow: 载台移至上下料位 - 放入锥形瓶/ 搅拌子弹夹 - Y轴移动至取搅拌子位  -X轴移动到取搅拌子位置- 夹爪夹紧取搅拌子 - 前Z轴回至最高点零位-Y轴移动至拆盖位 - 拆盖 - Y轴移动至放搅拌子位 -前Z轴X轴移动放搅拌子伟- 前Z轴夹爪松开放搅拌子 - Y轴移动至装盖位 - 装盖 -Y轴 移动至下料位 - 机械臂取下锥形瓶/弹夹
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 582
  consumables: 锥形瓶-250ml
  sub_functions: [611-拆盖/加液/装盖, 612-磁力子弹夹更换完成]
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
    - 正压加液模块
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: U型光电
tags_meta:
  tags: [加液, 加磁力子, 搅拌]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.42
  power_kva_peak: 0.7
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: false
  requires_water: false
  requires_drain: false
---

# 250ml锥形瓶加磁力搅拌子模块

## Purpose (用途说明)
完成锥形瓶拆盖，加液，自动加磁力子搅拌，将样品与试剂混匀

## Typical Scenarios (典型应用场景)
食品5009

## Working Principle (工作原理)
三轴联动 + 拆盖投子 + 多通道加液 + 磁力搅拌，实现锥形瓶全自动前处理。

## Limits & Cautions (限制与注意事项)
本模块为实验室自动化前处理设备，适配 250ml 锥形瓶，双工位并行作业，自动完成拆盖、投放磁力子、6 通道精准加液与磁力混匀。加液范围 10–100ml、精度 ±2%，搅拌转速 300–8000rpm 可调。广泛应用于环境、食品、化工、医药等领域，实现样品前处理全流程自动化，高效稳定、减少人工误差，助力实验室智能化升级

## Remarks (备注)

