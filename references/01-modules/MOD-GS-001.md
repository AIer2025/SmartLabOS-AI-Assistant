---
identification_version:
  id: MOD-GS-001
  module_part_no: HHM47A-100
  name: 100ml石墨赶酸模块
  category: 赶酸
  version: 2.0
  status: active
  last_updated: 2025/9/5
  owner: 汪健
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 25
  module_slots: 1
  tray_slots: "八工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 100
  temperature_c_min: 25
  temperature_c_max: 200
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: 冷水机
process_capability_1:
  function_code: 210
  function_description: 石墨赶酸
  operation_target: 100ml消解管
  operation_workflow: 石墨消解支座移至上下料位 - 消解管（8个）放入支座槽内 -支座移至前排加液位-加液-支座移至后排加液位-加液-支座移至赶酸工作位 - 防护罩下压与支座上端贴合 - 加热至200℃ - 恒温120分钟 - 持续排除酸雾 - 加热停止 - 冷却水开启 - 降温至室温 - 冷却水关闭 - 防护罩返回 - 石墨消解支座移至上下料位 - 取下消解管
  absolute_accuracy_pct: ±5%
  repeat_accuracy_pct: ""
  cycle_time_sec: 10800
  consumables: 消解管-100ml
  sub_functions: []
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
    - 正压加液
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: 无
tags_meta:
  tags: [加液, 拆装盖，加热赶酸, 冷却, 酸气排放]
documentation:
  urs_url: 无
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.87
  power_kva_peak: 1.45
  voltage_v: 220
  noise_db: 65
  heat_generating: "true"
  requires_ventilation: true
  requires_compressed_air: true
  requires_water: true
  requires_drain: false
---

# 100ml石墨赶酸模块

## Purpose (用途说明)
石墨赶酸模块是实验室样品前处理专用设备，核心用于消解管内样品的自动加液、高温加热赶酸、冷却排酸气全流程处理，适配元素分析等实验的样品消解预处理需求，提升实验效率与精度

## Typical Scenarios (典型应用场景)
石墨赶酸模块主要用于环境、地质、食品、化工、医药等实验室，对土壤、矿石、食品、材料等样品进行三酸消解 + 高温赶酸 + 自动排酸，为 ICP-MS/AAS/AFS 等仪器提供洁净、稳定、低空白的待测液，显著提升前处理效率与数据可靠性

## Working Principle (工作原理)
石墨赶酸模块本质是一套自动化、密闭式高温消解 + 赶酸 + 冷却排废系统，由三轴运动 + 加热系统 + 冷却系统 + 气路阀组 + 温控系统协同完成样品前处理

## Limits & Cautions (限制与注意事项)
石墨赶酸模块用于样品三酸消解、高温赶酸与冷却排废，自动化程度高、精度稳定。限制：最高 200℃、8 管通量，禁高氯酸 / 高爆样品。注意：强腐蚀高温风险，需通风防护、规范维护，严禁干烧与违规试剂

## Remarks (备注)

