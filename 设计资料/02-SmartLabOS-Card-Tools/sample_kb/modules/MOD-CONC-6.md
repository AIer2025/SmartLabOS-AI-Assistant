---
id: MOD-CONC-6
name: 6 通道氮吹浓缩模块
category: 浓缩
version: 1.0
status: active
last_updated: 2026-04-25

dimensions_mm: { length: 280, width: 200, height: 420 }
weight_kg: 20
module_slots: 1
tray_slots: 1
mount_type: 标准模块位

function: 浓缩
sub_functions: [氮吹, 加热, 定容]
throughput:
  capacity: 6
  unit: 管
  cycle_time_min: 25
parameters:
  volume_ml: { min: 0.5, max: 15 }
  temperature_c: { min: 25, max: 80 }

consumables: [试管-15ml]
sample_container: [试管-15ml]
gripper_required: [试管-15ml]

power_kva_nominal: 0.5
power_kva_peak: 0.7
voltage_v: 220
heat_generating: true
requires_ventilation: false
requires_compressed_air: false
requires_water: false
requires_drain: false

compatible_platforms: [PLT-1200, PLT-1400, PLT-1800]
incompatible_platforms: []

dependencies: []
conflicts: []
typical_pairings: []

tags: [核心模块]
---

# 6 通道氮吹浓缩模块
氮吹加热浓缩定容,工艺末端核心模块。
