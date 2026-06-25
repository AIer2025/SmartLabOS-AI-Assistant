---
identification_version:
  id: MOD-JY-013
  module_part_no: ""
  name: 2ml离心管带磁拆装盖加液模块V3.0
  category: 加液
  version: 3.0
  status: active
  last_updated: 2025/3/14
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
  volume_ml_min: 0
  volume_ml_max: 2
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: ""
process_capability_1:
  function_code: 177
  function_description: 本模块预期实现：2ml离心管的上下料、开合盖和加液功能。
  operation_target: 2ml离心管
  operation_workflow: 上料：两个带盖的2ml离心管；拆盖：带盖2ml离心管拆盖；加液：加液；装盖：离心管装盖；下料：2个带盖带料的2ml离心管。
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 105
  consumables: "离心管-2ml / "
  sub_functions: [175 拆盖，176 装盖，178 加液]
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
  tags: [带磁，加液，离心管]
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

# 2ml离心管带磁拆装盖加液模块V3.0

## Purpose (用途说明)


## Typical Scenarios (典型应用场景)
1、制药研发对照品、注射液小体积配液
2、食品安全 / 农残 / 重金属检测前处理
3、疾控 / 医院检验样本自动化开盖加液
4、蛋白提取、细胞裂解、样本稀释工作站
5、标准溶液、标样、质控品自动配制
6、微流控、免疫检测试剂自动化分装
7、科研实验室 2ml 管批量处理平台

## Working Principle (工作原理)
通过Y 轴工位流转 + Z 轴旋盖夹爪 + 多通道定量加液 + 气缸夹紧定位，全自动完成上料→开盖→加液→关盖→下料-吸磁全流程

## Limits & Cautions (限制与注意事项)
运动区域禁止伸手：Y/Z 轴、夹爪动作时严禁伸入，防止夹伤
定期清洁：传感器、夹爪、治具保持干净，避免漏检、打滑
禁止空旋：无瓶时禁止夹爪旋转，防止磨损、损坏

## Remarks (备注)

