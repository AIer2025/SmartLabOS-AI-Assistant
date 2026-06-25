---
identification_version:
  id: MOD-ZD-003
  module_part_no: HHM01A-015
  name: 15ml离心管垂直振荡模块
  category: 振荡
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
  speed_rpm_max: 500
  pressure_bar_min: 1
  pressure_bar_max: 6
  amplitude_mm_min: 1
  amplitude_mm_max: 53
  vacuum_kpa: NA
peripherals:
  required_peripherals: 空压机
process_capability_1:
  function_code: 140
  function_description: 振荡
  operation_target: 15ml离心管
  operation_workflow: "载台移至最高点 - 电推杆推动抱夹打开 - 机械臂放入离心管 - 夹紧 - 振荡（500rpm/5min）- 停止 - 载台移至最高点 - 机械臂夹持离心管（两个同时） - 电推杆推动抱夹打开 - 取下离心管 - 电推杆返回； / 停顿1分钟，重复工作流程"
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 30
  consumables: "离心管-15ml / "
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
  wear_parts: NA
tags_meta:
  tags: [垂直振荡]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.216
  power_kva_peak: 0.36
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 15ml离心管垂直振荡模块

## Purpose (用途说明)
实现15ml尖底离心管的上下振荡

## Typical Scenarios (典型应用场景)
1、水质检测15ml 离心管样品垂直振荡混匀前处理
2、环境水样、土壤浸提液、固废提取液垂直振荡萃取
3、食品 / 农产品样品15ml 离心管振荡均匀化处理
4、生化试剂、标准溶液、缓冲液垂直振荡充分溶解
5、实验室小体积样品高速垂直振荡、充分反应场景

## Working Principle (工作原理)
通过伺服电机驱动、电动推杆夹紧试管，并由光电开关检测定位，自动完成放瓶、夹紧、上下振荡、取瓶的全流程作业

## Limits & Cautions (限制与注意事项)
1、仅适用于15ml 标准离心管，不得使用其他规格。
2、仅适配带盖密封的 15ml 离心管，无盖、裂盖、变形管禁止使用。
3、振荡幅度、转速、时间按工艺参数设定，禁止超范围运行
4、仅用于常规水样 / 提取液 / 非危险样品，严禁用于强酸、强碱、易燃易爆、易挥发、剧毒液体。

## Remarks (备注)

