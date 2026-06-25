---
identification_version:
  id: MOD-JY-017
  module_part_no: HHM23G-125
  name: 125ml PP瓶拆盖正压加液混匀模块
  category: 加液
  version: 1.0
  status: active
  last_updated: 2025/9/12
  owner: 白昌辉
physical_specs:
  length_mm: 250
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 100
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 0
  speed_rpm_max: 1000
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: ""
process_capability_1:
  function_code: 90
  function_description: 拆盖加液混匀
  operation_target: 125mlPP瓶
  operation_workflow: PP瓶放入载台 - 夹紧 - 载台移至拆盖位 - 拆盖夹爪下降至拆盖位 - 拆盖 - 拆盖夹爪上升至等待位（夹持瓶盖）- 载台移至加液混匀位 - 下压加液组件下压封闭瓶口 - 加液（体积） - 夹紧组件打开 - 混匀（时间）- 停止 - 夹紧 - 下压加液组件返回 - 载台移至倾斜移液位 - 载台倾斜15° - 移液 - 载台回正 - 载台移至拆盖位 - 拆盖夹爪下降装盖 - 载台移至上下料位 - 取下PP瓶
  absolute_accuracy_pct: ±5%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 70
  consumables: PP瓶-125ml
  sub_functions: [91-拆盖, 92-加液, 93-装盖]
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
    - 正压加液
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: ""
tags_meta:
  tags: [加液, 混匀]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.24
  power_kva_peak: 0.4
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 125ml PP瓶拆盖正压加液混匀模块

## Purpose (用途说明)
125mlpp瓶的上下料、开合盖、加液、混匀、倾斜移液功能。

## Typical Scenarios (典型应用场景)
1、水质 COD / 总磷 / 总氮检测125ml 样品瓶自动拆盖、加消解液、旋转混匀一体化前处理
2、环境水样、土壤浸提液定量加液 + 高速混匀 + 倾斜移液全自动流程
3、食品 / 农产品检测5 种试剂分步加注、充分混匀的样品前处理工位

## Working Principle (工作原理)
模块通过 Y 轴、Z 轴联动定位，在拆盖位由电动夹爪完成旋盖开合；加液位实现5 种试剂定量加注，无刷电机带动瓶体高速旋转混匀；倾斜组件实现15° 倾斜移液，全程由传感器检测与气缸夹紧保障稳定，自动完成拆盖、加液、混匀、移液、装盖全流程。

## Limits & Cautions (限制与注意事项)
仅限125ml 标准 PP 瓶使用，拆盖、混匀、移液前必须夹紧瓶体；混匀转速不超1000rpm，加液量≤50ml；运行时严禁触碰运动部件与夹爪；开机必须完成轴回零初始化，传感器保持清洁；维护前需断电断气，确保安全。

## Remarks (备注)

