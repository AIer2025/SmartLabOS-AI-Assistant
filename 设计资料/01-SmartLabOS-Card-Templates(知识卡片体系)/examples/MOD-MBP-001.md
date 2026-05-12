---
id: MOD-MBP-001
Module_Part_No: HHM10N-150
name: 150ml面包瓶拔塞加液模块
category: 拔塞加液
version: 1.0
status: active
last_updated: 2026/4/17
owner: 唐宋

dimensions_mm:
  length: 450
  width: 180
  height: 600
weight_kg: 15
module_slots: 1
tray_slots: 1
mount_type: 标准模块位

function: 177 拆盖 加液 装盖：平台夹爪从 150ml面包瓶托盘位夹取 150ml面包瓶，放到单通道模块载台上，运行到拆盖位进行拆盖，运行到加液位加液，运行到装盖位装盖，回到载台原点，机械臂下料
sub_functions: [175 拆盖, 176装盖, 178加液]
throughput:
  capacity: 1
  unit: ""
  cycle_time_min: 30S
parameters:
  volume_ml: { min: null, max: 150 }
  temperature_c: { min: null, max: null }
  speed_rpm: { min: null, max: null }
  pressure_bar: { min: 3, max: 4 }

consumables:
  - 面包瓶-150ml
sample_container:
  - 面包瓶-150ml
gripper_required:
  - 标准夹爪

power_kva_nominal: 150
power_kva_peak: 279
voltage_v: 24
heat_generating: false
requires_ventilation: false
requires_compressed_air: true
requires_water: true
requires_drain: false
noise_db: 65

compatible_platforms:
  - PLT-1200
incompatible_platforms:
  []

dependencies:
  - 前置/后置模块
conflicts:
  - 相互影响模块
typical_pairings:
  - 和模块配合使用

price_rmb: null
lead_time_weeks: 8
maintenance_cycle_months: 6

tags: [拔塞，加液]
---

# 150ml面包瓶拔塞加液模块

## 用途说明
150ml面包瓶拔塞，加液

## 典型应用场景
中烟项目

## 工作原理
""

## 限制与注意事项
150ml面包瓶加工精度影响，会影响拆合盖成功率，需要对面包瓶做检查

## 备注
""
