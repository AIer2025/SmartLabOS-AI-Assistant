---
id: MOD-CENT-24
name: 24 管离心模块
category: 离心
version: 1.0
status: active
last_updated: 2026-04-25

dimensions_mm: { length: 320, width: 320, height: 380 }
weight_kg: 38
module_slots: 1
tray_slots: 1
mount_type: 标准模块位

function: 提取
sub_functions: [离心分相]
throughput:
  capacity: 24
  unit: 管
  cycle_time_min: 5
parameters:
  speed_rpm: { min: 500, max: 5000 }

consumables: [试管-50ml, 试管-15ml]
sample_container: [试管-50ml, 试管-15ml]
gripper_required: [试管-50ml, 试管-15ml]

power_kva_nominal: 0.7
power_kva_peak: 1.2
voltage_v: 220
heat_generating: false
requires_ventilation: false
requires_compressed_air: false
requires_water: false
requires_drain: false

compatible_platforms: [PLT-1200, PLT-1400, PLT-1800]
incompatible_platforms: [PLT-800]

dependencies: []
conflicts: []
typical_pairings: []

tags: [核心模块]
---

# 24 管离心模块
高速离心分相模块。
