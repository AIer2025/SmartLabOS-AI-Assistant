---
identification_version:
  id: MOD-WX-008
  module_part_no: HHM01E-010
  name: 10ml比色管拔塞摇匀模块
  category: 涡旋
  version: 1.0
  status: active
  last_updated: 2025/5/23
  owner: 何向东
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
  speed_rpm_min: 1
  speed_rpm_max: 3000
  pressure_bar_min: 1
  pressure_bar_max: 6
  vacuum_kpa: NA
peripherals:
  required_peripherals: 空压机
process_capability_1:
  function_code: 270
  function_description: 启动摇匀
  operation_target: 10ml比色管
  operation_workflow: 比色管放入载台 - 载台移至拆盖位 - 夹紧比色管 - 拆盖夹爪下降 - 拆盖 - 夹爪夹持管盖上升 - 松开比色管 - 载台移至上下料位 - 取走比色管 装什么液体 - 比色管放回载台 - 载台移至拆盖位 -  夹紧比色管 - 拆盖夹爪下降 - 装盖 - 夹爪上升 - 松开比色管 - 载台移至混匀位 - 涡旋气缸下压 - 混匀100s - 停止 - 涡旋气缸抬起 - 载台移至上下料位 - 取下比色管
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 100
  consumables: "比色管-10ml / "
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
  wear_parts: 手指胶皮、涡旋胶皮
tags_meta:
  tags: [涡旋，拔塞，装塞]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.228
  power_kva_peak: 0.38
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 10ml比色管拔塞摇匀模块

## Purpose (用途说明)
10ml比色管的上下料、涡旋、拔塞装塞功能

## Typical Scenarios (典型应用场景)
1、水质检测10ml 比色管自动拔塞、装塞、涡旋混匀一体化处理
2、水样前处理产线开盖取样 + 混匀 + 回盖全自动工位
3、COD、总磷、总氮等指标比色管样品快速涡旋摇匀前处理
4、实验室小体积试剂自动开盖 + 高速涡旋混匀场景
5、机械手联动10ml 比色管全自动上下料、拔塞、摇匀流水线

## Working Principle (工作原理)
通过 Y 轴、Z 轴步进模组配合电动旋转夹爪与夹紧、下压气缸，自动完成比色管上下料、拔塞、装塞及涡旋混匀的一体化流程

## Limits & Cautions (限制与注意事项)
1、定期清洁光电开关、夹爪、涡旋台面，保持无液体、无粉尘
2、检查电机线、传感器线、气缸线无松动、无破损。
3、导轨、滑动部件定期润滑，保证运动顺畅。
4、涡旋电机、电动夹爪不可长时间连续满载运行，避免过热
5、定位不准、丢步时，重新回零校准再启动。
6、气路需定期排水排污，防止损坏气缸与电磁阀。

## Remarks (备注)

