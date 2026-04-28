---
id: MOD-PIPETTE-8
name: 8 通道移液模块
category: 移液
version: 1.0
status: active
last_updated: 2026-04-25

dimensions_mm: { length: 350, width: 200, height: 420 }
weight_kg: 22
module_slots: 1
tray_slots: 1
mount_type: 标准模块位

function: 提取
sub_functions: [移液, 转移]
throughput:
  capacity: 8
  unit: 管
  cycle_time_min: 4
parameters:
  volume_ml: { min: 0.05, max: 10 }

consumables: [移液吸头]
sample_container: [试管-15ml, 试管-50ml]
gripper_required: []

power_kva_nominal: 0.3
power_kva_peak: 0.5
voltage_v: 220
heat_generating: false
requires_ventilation: false
requires_compressed_air: false
requires_water: false
requires_drain: false

compatible_platforms: [PLT-1200, PLT-1400, PLT-1800]
incompatible_platforms: []

dependencies: []
conflicts: []
typical_pairings: []

tags: [核心模块, 高频]
---

# 8 通道移液模块
精密液体转移核心模块。
