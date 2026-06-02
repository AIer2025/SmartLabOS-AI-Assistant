---
identification_version:
  id: MOD-YY-001
  module_part_no: HHM09G-010
  name: 自动化精密移液模块10ml（定量环）
  category: 移液
  version: 1.0
  status: active
  last_updated: 2025/6/13
  owner: 黄锐
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 25
  module_slots: 1
  tray_slots: "双工位"
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
  required_peripherals: 空压机
process_capability_1:
  function_code: 600
  function_description: 移液
  operation_target: 25ml容量瓶，50ml容量瓶，100ml容量瓶，200ml容量瓶，125ml PP广口瓶
  operation_workflow: 放入容量瓶/PP瓶 - 夹紧 - 上样针移至吸液位 - 针头伸入瓶内液面下 - 吸入样品液 - 清除死体积 - 上样针移至放液位 - 针头伸入瓶内（瓶颈） - 放液 - 上样针移至清洗位 - 针头内外同时冲洗 - 上样针移至吹干位 - 吹干 - 取下容量瓶/PP瓶；
  absolute_accuracy_pct: "(1)1ml  ±0.7% / (2)2ml ±0.5% / (3)5ml ±0.3% / (4)10ml ±0.2%"
  repeat_accuracy_pct: "(1)1ml  ±0.7% / (2)2ml ±0.5% / (3)5ml ±0.3% / (4)10ml ±0.2%"
  cycle_time_sec: 0
  consumables: 25ml容量瓶，50ml容量瓶，100ml容量瓶，200ml容量瓶，125ml PP广口瓶
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
  tags: [精密移液]
documentation:
  urs_url: URS连接
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.36
  power_kva_peak: 0.54
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: true
---

# 自动化精密移液模块10ml（定量环）

## Purpose (用途说明)
对心定位 + 精密移液（0.5~10ml）+ 自动清洗干燥，三位一体实现多瓶型、多规格的全自动精密移液，替代人工操作，避免交叉污染

## Typical Scenarios (典型应用场景)
精密移液（0.5~10ml）+ 多瓶型适配（25~200ml容量瓶/125ml PP瓶）+ 自动清洗干燥，三位一体，专为药物配制、环境检测、临床检验、批量QC等需要高精度小体积移液的自动化场景设计

## Working Principle (工作原理)
Z轴精确定位 + 注射泵定量吸排 + 多阀控制流路 + 正压清洗 + CDA内外吹干，五位一体实现0.5~10ml五档精密移液，每次操作后自动清洗干燥，完全替代人工移液，杜绝交叉污染

## Limits & Cautions (限制与注意事项)
瓶型只认5种、移液只认5档、每次必清洗必干燥、摆臂只认4个角度、上料必过光电检测——其余参数（清洗次数、吹干时长、取液高度）均可按需设定，但上述限制不可突破，否则精度失控或设备损坏

## Remarks (备注)

