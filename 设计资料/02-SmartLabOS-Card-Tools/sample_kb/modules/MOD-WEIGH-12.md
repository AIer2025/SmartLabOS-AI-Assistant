---
id: MOD-WEIGH-12
name: 12 通道自动称量模块
category: 称量
version: 1.0
status: active
last_updated: 2026-04-25

dimensions_mm: { length: 250, width: 180, height: 320 }
weight_kg: 12
module_slots: 1
tray_slots: 1
mount_type: 标准模块位

function: 称量
sub_functions: [自动称样, 去皮]
throughput:
  capacity: 12
  unit: 样品
  cycle_time_min: 8
parameters:
  mass_g: { min: 0.1, max: 50 }

consumables: [试管-50ml]
sample_container: [试管-50ml]
gripper_required: [试管-50ml]

power_kva_nominal: 0.2
power_kva_peak: 0.3
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

tags: [核心模块, 入口]
---

# 12 通道自动称量模块
工艺链入口模块,负责样品自动称样。
