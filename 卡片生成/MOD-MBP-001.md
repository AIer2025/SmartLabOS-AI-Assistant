---
identification_version:
  id: MOD-MBP-001
  module_part_no: HHM10N-150
  name: 150ml面包瓶拔塞加液模块
  category: 拔塞加液
  version: 1.0
  status: active
  last_updated: 2026/4/17
  owner: 唐宋
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 15
  module_slots: 1
  tray_slots: "单工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 150
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: "177 / "
  function_description: " 150ml面包瓶 拆盖-加液-装盖："
  operation_target: 150ml面包瓶
  operation_workflow: 平台夹爪从 150ml面包瓶托盘位夹取 150ml面包瓶，放到单通道模块载台上，运行到拆盖位进行拆盖，运行到加液位加液，运行到装盖位装盖，回到载台原点，机械臂下料
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 30S
  consumables: 面包瓶-150ml
  sub_functions: [175 拆盖, 176装盖, 178加液]
platform_compatibility:
  compatible_platforms:
    - PLT-1200
  incompatible_platforms:
    []
module_relationships:
  dependencies:
    - 前置/后置模块
  conflicts:
    - 相互影响模块
  typical_pairings:
    - 和模块配合使用
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: 手指包胶
tags_meta:
  tags: [拔塞，加液]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 150
  power_kva_peak: 279
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: false
---

# 150ml面包瓶拔塞加液模块

## Purpose (用途说明)
150ml面包瓶拔塞，加液

## Typical Scenarios (典型应用场景)
中烟项目

## Working Principle (工作原理)


## Limits & Cautions (限制与注意事项)
150ml面包瓶加工精度影响，会影响拆合盖成功率，需要对面包瓶做检查

## Remarks (备注)

