---
id: MOD-DISP-SHAKE-24
name: 24 通道加液振荡模块
category: 加液振荡
version: 1.0
status: active
last_updated: 2026-04-25

dimensions_mm: { length: 300, width: 200, height: 280 }
weight_kg: 16
module_slots: 1
tray_slots: 1
mount_type: 标准模块位

function: 提取
sub_functions: [加液, 涡旋振荡]
throughput:
  capacity: 24
  unit: 管
  cycle_time_min: 5
parameters:
  volume_ml: { min: 0.5, max: 30 }
  speed_rpm: { min: 200, max: 2500 }

consumables: [试管-50ml]
sample_container: [试管-50ml]
gripper_required: [试管-50ml]

power_kva_nominal: 0.5
power_kva_peak: 0.8
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

# 24 通道加液振荡模块
QuEChERS 等流程的核心提取模块。
