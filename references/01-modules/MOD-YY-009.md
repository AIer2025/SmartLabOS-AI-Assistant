---
identification_version:
  id: MOD-YY-009
  module_part_no: HHM09Q-901
  name: 50-250ml容量瓶拆盖倒液模块
  category: 移液
  version: 1.0
  status: active
  last_updated: 2026/5/22
  owner: 唐宋
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "单工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 50
  volume_ml_max: 250
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
  function_code: 160
  function_description: 拆盖/倒液/装盖
  operation_target: 50ml容量瓶，100ml容量瓶，200ml容量瓶，250ml容量瓶，50mlpp瓶
  operation_workflow: 机械臂放入PP瓶和容量瓶-模块执行先拆PP盖-将PP盖放在瓶盖过渡位-模块检测容量瓶瓶瓶盖盖度-夹紧气缸走到位置拆盖-将容量瓶抬起-Y轴走到PP瓶接液位-夹紧气缸Z轴走到倒液位-点推杆走到倒液角度-倒液完PP瓶和容量瓶装盖-机械臂先搬走容量瓶再搬走PP瓶
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 230
  consumables: 50ml容量瓶，100ml容量瓶，200ml容量瓶，250ml容量瓶，50mlpp瓶
  sub_functions: [161-PP拆盖，162-PP装盖，163-拆盖/倒液/不装PP盖]
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
  wear_parts: 手指
tags_meta:
  tags: [容量瓶，PP瓶，拆盖，倒液]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.36
  power_kva_peak: 0.6
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 50-250ml容量瓶拆盖倒液模块

## Purpose (用途说明)
50-250ml容量瓶倒液模块是实现从容量瓶将液体倒入50mlPP瓶的操作，同时具备50mlPP瓶拆盖功能

## Typical Scenarios (典型应用场景)
自动化定容平台

## Working Principle (工作原理)
设备接收上位机 Modbus TCP 指令，依靠机械夹持机构 + 旋转拧盖机构 + 伺服翻转机构，依次完成 PP 瓶拆盖→容量瓶夹持翻转定量倒液→PP 瓶旋盖，全流程自动化。

## Limits & Cautions (限制与注意事项)
1、瓶体规格受限：仅适配 50/100/200/250ml 普兰德、百泉容量瓶与指定型号 50ml 垒固 PP 瓶，非标瓶体无法使用。
2、分装容量约束：倒液量只能在 1~15mL 区间调节，超出量程无法精准定量。
3、液体依赖自重出料：高粘度、易挂壁粘稠液体易出现残液、滴漏，不适用。

## Remarks (备注)

