---
identification_version:
  id: MOD-PY-002
  module_part_no: HHM30E-050
  name: 50ml离心管金属浴模块
  category: 平遥
  version: 1.0
  status: active
  last_updated: 2025/5/16
  owner: 唐宋
physical_specs:
  length_mm: 280
  width_mm: 250
  height_mm: 198
  weight_kg: 25
  module_slots: 1
  tray_slots: "十六工位"
  mount_type: 托盘位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 50
  temperature_c_min: 25
  temperature_c_max: 70
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: ""
process_capability_1:
  function_code: 240
  function_description: 启动周期平遥
  operation_target: 50ml离心管
  operation_workflow: 加热开启 - 温度到达设定范围 - 治具平摇至外侧 - 放入离心管 - 平摇 - 停顿 - 间隔平摇至要求次数 - 停止平摇 - 依次放入离心管重复前述过程 - 金属浴至30分钟 - 依次取出离心管
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 220
  consumables: 50ml离心管
  sub_functions: [241-机构定位到上下料位置]
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
  tags: [超声]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.534
  power_kva_peak: 0.89
  voltage_v: 220
  noise_db: 65
  heat_generating: "true"
  requires_ventilation: false
  requires_compressed_air: false
  requires_water: false
  requires_drain: false
---

# 50ml离心管金属浴模块

## Purpose (用途说明)
16通道50ml离心管金属浴加热定时平摇等功能

## Typical Scenarios (典型应用场景)
生物样本恒温孵育与振荡混匀自动化处理
适用于核酸纯化、酶反应、抗原抗体反应、样品孵育等场景，对50ml 离心管进行恒温加热 + 定时平摇，16 通道批量处理，实现温控精准、混匀充分、无人值守

## Working Principle (工作原理)
50mL 离心管金属浴模块的核心原理是高导热金属块直接接触传热 + PID 闭环精准控温，把电能转为热能并稳定传递给离心管样品，实现快速、均匀、无液体污染的干式恒温。

## Limits & Cautions (限制与注意事项)
1、仅适配50ml 离心管，必须放正、放稳、盖紧，禁止歪放、空管加热。
2、加热温度 **≤70℃，控温异常、热电偶故障禁止加热 **。
3、摇匀转速150~600rpm，运行中禁止触碰平台与运动部件。
4、加热时确保管内无渗漏，防止液体损坏加热棒与电机。
5、异常立即急停，冷却后再处理；维护需断电，禁止湿手操作。

## Remarks (备注)

