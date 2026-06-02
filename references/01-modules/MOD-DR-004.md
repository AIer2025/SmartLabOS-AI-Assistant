---
identification_version:
  id: MOD-DR-004
  module_part_no: HHM08B-010
  name: 10ml比色管定容模块
  category: 定容
  version: 1.0
  status: active
  last_updated: 2025/9/5
  owner: 唐宋
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 25
  module_slots: 1
  tray_slots: "单工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 10
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
  function_code: 530
  function_description: 视觉定容
  operation_target: 10ml比色管
  operation_workflow: 载台移至上下料位 - 放入比色管- 移至加液位 - 气缸夹紧 - 加液口下降至加液位 - 加液 - 相机拍照 - 液面至刻度线齐平处停止加液- 称重 -加液口上升至初始位 - 气缸打开 - 载台移至上下料位 - 取下比色管
  absolute_accuracy_pct: 40mg
  repeat_accuracy_pct: 18mg
  cycle_time_sec: 60
  consumables: 比色管-10ml
  sub_functions: [无]
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
    - 正压加液模块
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: 无
tags_meta:
  tags: [定容]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.134
  power_kva_peak: 0.224
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 10ml比色管定容模块

## Purpose (用途说明)
10ml 比色管定容模块是实验室样品前处理专用自动化设备，核心用于 10ml 比色管的精准自动加水定容，替代人工目视定容，保障实验精度与一致性，适配各类样品溶液定量制备场景

## Typical Scenarios (典型应用场景)
广泛用于环境监测、食品检测、化工、医药、科研实验室等领域，为水质、土壤、食品、化工原料等样品的比色分析、分光光度检测，提供精准定量的样品溶液。

## Working Principle (工作原理)
本模块由 PLC 控制，通过 Y 轴换工位、Z 轴控加液管升降，气缸固定比色管。电磁阀加水，视觉相机实时识别液面，液面与刻度线对齐即停，精度 ±0.1mm，自动完成夹紧、定容、下料全流程，替代人工，高效精准

## Limits & Cautions (限制与注意事项)
仅限 10ml 标准比色管，液体需透明、无沉淀气泡；环境需洁净、无强光振动。注意：避免腐蚀液体，定期清洁镜头与管路；严禁硬物碰撞，保持定位精度，防止漏水误检。

## Remarks (备注)

