---
identification_version:
  id: MOD-CG-002
  module_part_no: HHM02L-100
  name: 100ml试管拔塞模块
  category: 拔塞
  version: 1.0
  status: active
  last_updated: 2025/5/19
  owner: 朱园林
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 15
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: "NA"
  volume_ml_max: "NA"
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: 空压机
process_capability_1:
  function_code: 550
  function_description: 启动拔塞装塞
  operation_target: 100ml试管
  operation_workflow: "载台移至上下料位 - 放入试管 - 载台移至拆盖位 - 夹紧试管 - 气缸夹爪下降至试管塞处 - 夹爪夹住管塞 - 提起 - 载台移至上下料位 - 取走试管操作 - 试管返回 - 载台移至拆盖位 - 气缸夹爪下降 - 管塞塞进试管 - 气缸夹爪上升 - 载台移至上下料位 - 取下试管； / 停顿1分钟，重复工作流程"
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 90
  consumables: 试管-100ml
  sub_functions: [551-启动拔塞
552-启动装塞]
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
    - NA
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [拔塞，盖塞，拆盖，装盖]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.12
  power_kva_peak: 0.224
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 100ml试管拔塞模块

## Purpose (用途说明)
100ml试管的拆盖和装盖功能

## Typical Scenarios (典型应用场景)
1、全自动水样前处理产线中100ml 试管自动拔塞工位
2、检测前试管开盖取样与取样后自动回塞封口一体化场景
3、多工位联动流水线：试管拔塞→样品处理→试管装盖闭环作业
4、机械手自动上下料的无人化试管拆盖 / 装盖场景
5、水质 COD、总磷、总氮等项目标准 100ml 试管开盖前处理

## Working Principle (工作原理)
依靠 Y 轴、Z 轴步进模组配合气缸夹爪与夹紧模组，通过光电及传感器检测定位，自动完成试管移位、夹紧、拔塞、等待处理、自动回装瓶盖并送回下料位的全自动拆盖装盖流程

## Limits & Cautions (限制与注意事项)
1、定期清洁光电开关、传感器表面，保证检测准确。
2、检查气缸感应线、电机线无松动、无破损，连接牢固。
3、导轨与滑动部件定期润滑，保持运动顺畅无异响。
4、电磁阀、气缸漏气、响应慢时立即更换，避免拔塞失败。
5、步进电机丢步、定位不准时，重新回零校准再运行。
6、气路保持排水、过滤，防止水分损坏气缸与电磁阀。

## Remarks (备注)

