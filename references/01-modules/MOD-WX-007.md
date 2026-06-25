---
identification_version:
  id: MOD-WX-007
  module_part_no: HHM03H-002
  name: 2ml离心管正压加液涡旋模块V3.0
  category: 涡旋
  version: 3.0
  status: active
  last_updated: 2025/3/14
  owner: 何向东
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 0
  volume_ml_max: 2
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 1000
  speed_rpm_max: 3000
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: ""
process_capability_1:
  function_code: 222
  function_description: 本模块预期实现：2ml离心管正压加液涡旋功能。
  operation_target: 2ml离心管
  operation_workflow: 上料：两个带盖的2ml离心管；拆盖：带盖2ml离心管拆盖；加液：加液；装盖：离心管装盖；涡旋：按照转速和时间进行涡旋；下料：2个带盖带料的2ml离心管。
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 290
  consumables: "离心管-2ml / "
  sub_functions: [加液，涡旋]
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
    []
  conflicts:
    []
  typical_pairings:
    - 正压加液模块
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [加液，涡旋，2ml离心管]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.31
  power_kva_peak: 0.52
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: false
---

# 2ml离心管正压加液涡旋模块V3.0

## Purpose (用途说明)


## Typical Scenarios (典型应用场景)
1.PCR / 荧光定量 PCR 体系自动配制
3.IVD 试剂：样本保存液、裂解液、缓冲液自动分装
4.抗原抗体反应、免疫检测加液 + 混匀
5.血清 / 血浆 / 尿液样本加液与涡旋混匀
6.食品安全、农残、重金属检测前处理
7.疾控 / 医院检验自动化样本处理系统

## Working Principle (工作原理)
通过Y 轴工位移动 + 电动夹爪旋盖 + 正压定量加液 + 涡旋混匀，全自动完成开盖 — 加液 — 混匀 — 关盖一体化流程。

## Limits & Cautions (限制与注意事项)
1.仅限2ml 离心管，加液0.5–2ml
2.正压压力稳定、密封完好、先泄压再操作
3.必须先夹紧，再旋盖 / 加液 / 涡旋
4.无瓶不运行、异常立即停机
5.运动区禁伸手，管路用完及时清洗

## Remarks (备注)

