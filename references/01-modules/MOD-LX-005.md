---
identification_version:
  id: MOD-LX-005
  module_part_no: HHM15A-050
  name: 50ml*4孔位离心机模块（不带XYZ）
  category: 离心
  version: 3.0
  status: active
  last_updated: 2025/4/25
  owner: 杨宇/深圳
physical_specs:
  length_mm: 460
  width_mm: 460
  height_mm: 230
  weight_kg: 55
  module_slots: 3
  tray_slots: "四工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 50
  temperature_c_min: 1
  temperature_c_max: 10
  speed_rpm_min: 500
  speed_rpm_max: 8000
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: -80
peripherals:
  required_peripherals: ""
process_capability_1:
  function_code: 188
  function_description: 离心机启动
  operation_target: 50ml离心管
  operation_workflow: 入口开启 - 放入离心管1 - 转盘旋转 - 提篮对准入口 - 放入离心管2 - 转盘旋转 - 提篮对准入口 - 放入离心管3 - 转盘旋转 - 提篮对准入口 - 放入离心管4 - 入口关闭 - 离心5min - 停止 - 打开塞盖 - 依次取出离心管
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 420
  consumables: 离心管-50ml
  sub_functions: [185-离心机定位, 186-离心机预冷, 187-离心机取盖, 188-离心机启动, 189-离心机除水, 190-离心机停止]
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
  wear_parts: NA
tags_meta:
  tags: [离心]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 1
  power_kva_peak: 1.5
  voltage_v: 24
  noise_db: 65
  heat_generating: "true"
  requires_ventilation: true
  requires_compressed_air: false
  requires_water: false
  requires_drain: false
---

# 50ml*4孔位离心机模块（不带XYZ）

## Purpose (用途说明)
实现2-4个50ml带盖溶液离心，离心转速和时长可设定、调节。

## Typical Scenarios (典型应用场景)
适用于生物、医药、检测实验室，对50ml 带盖 离心管内液体样本进行自动化离心分离，替代人工操作，实现样本处理全流程无人化

## Working Principle (工作原理)
核心原理：电气控制 + 气动执行 + 高速离心分离，通过 PLC 统一协调，实现自动上料→定位→关门→高速离心→开门→取料全流程自动化

## Limits & Cautions (限制与注意事项)
仅适配50ml 带盖 离心管，单次最多4个，必须对称放置。
转速最高8000r/min，振动＞300μm必须停机报警。
门未关严禁止离心，运行中禁止开门。
禁止非指定容器、超载、偏载、无盖运行

## Remarks (备注)

