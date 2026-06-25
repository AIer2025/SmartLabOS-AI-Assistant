---
identification_version:
  id: MOD-ZD-002
  module_part_no: HHM01G-250
  name: 250ml锥形瓶恒温振荡模块
  category: 振荡
  version: 1.0
  status: active
  last_updated: 2025/8/22
  owner: 唐宋
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 25
  module_slots: 1
  tray_slots: "四工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: "NA"
  volume_ml_max: "NA"
  temperature_c_min: "室温"
  temperature_c_max: "80℃"
  speed_rpm_min: 0
  speed_rpm_max: 300
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: 140
  function_description: 该模块实现将250ml锥形瓶在恒温条件下振荡，完成皂化反应
  operation_target: 250ml锥形瓶
  operation_workflow: "Step1：Y轴移动到锥形瓶上下料位，机械手单批次先放2个锥形瓶； / Step2：Y轴移栽至合盖振荡位，合盖气缸伸出完成合盖，接近传感器感应记录锥形瓶所在恒温加热台上位置； / Step3：控制固态继电器使加热片通电发热达到设定温度，并恒温保持设定温度，恒温时间可设置(30min作为参考)。 / Step4：打开R轴电机，锥形瓶恒温加热同时振荡混匀； / Step5：恒温振荡完成后，y轴回到上下料位，合盖气缸退回，机械手下料；"
  absolute_accuracy_pct: 控温精度±3℃
  repeat_accuracy_pct: NA
  cycle_time_sec: 1800
  consumables: 250ml锥形瓶
  sub_functions: [NA]
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
    []
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [控温振荡、皂化反应]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.375
  power_kva_peak: 0.625
  voltage_v: 220
  noise_db: 65
  heat_generating: "true"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 250ml锥形瓶恒温振荡模块

## Purpose (用途说明)
用于控温振荡，皂化反应

## Typical Scenarios (典型应用场景)
维生素检测样品前处理中的皂化反应

## Working Principle (工作原理)
皂化反应是热驱动反应，温度控制皂化反应的终点，振荡解决油水两不相溶的问题。

## Limits & Cautions (限制与注意事项)
仅适配 250ml 标准带盖锥形瓶，单批次 2 瓶；温控仅室温至 80℃、精度 ±3℃，振荡幅度固定 10mm、转速 0–300rpm。DC24V/AC220V 供电、总功率 625W，合盖压力 0.5MPa；禁超规格 / 破损瓶，需避光防溢，安装水平，定期校准温控与传感器

## Remarks (备注)

