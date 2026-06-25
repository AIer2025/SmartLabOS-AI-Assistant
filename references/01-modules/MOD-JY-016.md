---
identification_version:
  id: MOD-JY-016
  module_part_no: HHM02N-002
  name: 2ml离心管&西林瓶拆装盖加液模块
  category: 加液
  version: 3.0
  status: active
  last_updated: 2025/4/11
  owner: 蔡勤超
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 0.1
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
  function_description: 拆盖/加液/装盖
  operation_target: 2ml离心管,2ml西林瓶
  operation_workflow: 载台移至上下料位 - 放入西林瓶- 移至拆盖位 - 电动夹爪下行 - 拆盖 -电动夹爪上行-移至加液位 - 加液- 移至拆盖位 - 电动夹爪下行 - 装盖 -电动夹爪上行-移至下料位
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 60
  consumables: 离心管-2ml,西林瓶-2ml
  sub_functions: [175-拆盖, 176-装盖]
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
  wear_parts: 手指
tags_meta:
  tags: [加液]
documentation:
  urs_url: ""
  urd_url: ""
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

# 2ml离心管&西林瓶拆装盖加液模块

## Purpose (用途说明)
兼容2ml离心管和2ml西林瓶拆盖，加入一定液体，装盖

## Typical Scenarios (典型应用场景)
1、制药研发对照品、注射液小体积配液
2、食品安全 / 农残 / 重金属检测前处理
3、疾控 / 医院检验样本自动化开盖加液
4、蛋白提取、细胞裂解、样本稀释工作站
5、标准溶液、标样、质控品自动配制
6、微流控、免疫检测试剂自动化分装
7、科研实验室 2ml 管批量处理平台

## Working Principle (工作原理)
通过Y 轴工位流转 + Z 轴旋盖夹爪 + 多通道定量加液 + 气缸夹紧定位，全自动完成上料→开盖→加液→关盖→下料全流程

## Limits & Cautions (限制与注意事项)
运动区域禁止伸手：Y/Z 轴、夹爪动作时严禁伸入，防止夹伤
定期清洁：传感器、夹爪、治具保持干净，避免漏检、打滑
禁止空旋：无瓶时禁止夹爪旋转，防止磨损、损坏

## Remarks (备注)

