---
identification_version:
  id: MOD-HY-003
  module_part_no: HHM01C-901
  name: 25-200ml容量瓶翻转摇匀模块
  category: 混匀
  version: 3.0
  status: active
  last_updated: 2025/8/8
  owner: 黄锐
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 25
  volume_ml_max: 200
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 100
  speed_rpm_max: 1000
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: -50
peripherals:
  required_peripherals: 空压机
process_capability_1:
  function_code: 500
  function_description: 兼容25ml-200ml容量瓶的上下料、翻转摇匀功能
  operation_target: 25ml容量瓶/50ml容量瓶/100ml容量瓶/200ml容量瓶
  operation_workflow: "上料：1个带盖带样品的25/50/100/200ml容量瓶；翻转摇匀：拿起带料容量瓶，顶塞、托底，反复倒立颠摇 10～15 次； / 每次倒立停留 2～3 秒，确保上下完全混匀；下料：1个带盖带样品的25/50/100/200ml容量瓶；"
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 10
  consumables: 25ml容量瓶/50ml容量瓶/100ml容量瓶/200ml容量瓶
  sub_functions: [NA]
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
    - 自动化称量模块
  typical_pairings:
    - 25-200ml容量瓶加液定容模块
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [翻转摇匀、定容平台]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.09
  power_kva_peak: 0.156
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 25-200ml容量瓶翻转摇匀模块

## Purpose (用途说明)
用于容量瓶定容后的翻转摇匀操作

## Typical Scenarios (典型应用场景)
理化检测、水质分析、中药理化、化工标液配制、实验室全自动样品前处理线，配套定容模块、液体转移模块组成完整自动化定容摇匀流水线

## Working Principle (工作原理)
防洒液原理、兼容多规格原理、均质混匀原理、平稳控速原理

## Limits & Cautions (限制与注意事项)
仅支持 25–200ml 标准容量瓶，固定 2 通道并行；仅做 360° 翻转摇匀，不可变速或改角度。DC24V 供电、总功率 156W，依赖机械手上下料；仅适配标准瓶型，禁异形 / 破损瓶，夹紧力需匹配，防滑落，安装需水平，定期校准传感器与气缸

## Remarks (备注)

