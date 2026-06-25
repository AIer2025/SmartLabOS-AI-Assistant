---
identification_version:
  id: MOD-JY-003
  module_part_no: HHM10H-150
  name: 150ml锥形瓶加液
  category: 加液
  version: 1.0
  status: active
  last_updated: 2025/6/13
  owner: 黄锐
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 150
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 1
  pressure_bar_max: 1.2
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: 570
  function_description: 加第一种液体
  operation_target: 150ml锥形瓶
  operation_workflow: 载台移至上下料位 - 机械臂从托盘取出锥形瓶 - 锥形瓶放入载台 - 载台移至加液位 - 加液口下降至加液位 - 排气开启 - 加液 - 停止 - 排气关闭（延迟）- 载台移至上下料位 - 机械臂取下锥形瓶 - 放入托盘
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 90
  consumables: 150ml锥形瓶
  sub_functions: [571-加第二种液体]
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
  tags: [150ml锥形瓶，加液]
documentation:
  urs_url: URS连接
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.14
  power_kva_peak: 0.27
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 150ml锥形瓶加液

## Purpose (用途说明)
载台移至上下料位 - 机械臂从托盘取出锥形瓶 - 锥形瓶放入载台 - 载台移至加液位 - 加液口下降至加液位 - 排气开启 - 加液 - 停止 - 排气关闭（延迟）- 载台移至上下料位 - 机械臂取下锥形瓶 - 放入托盘 - 等待30min - 机械臂从托盘取出锥形瓶 - 锥形瓶放入载台 - 载台移至加液位 - 加液口下降至加液位 - 排气开启 - 加液 - 停止 - 排气关闭（延迟）- 载台移至上下料位 - 机械臂取下锥形瓶 - 放入托盘；

## Typical Scenarios (典型应用场景)
食品/环境/药品/工业检测中的酸消解前处理是本模块的核心战场——两组150ml锥形瓶 + 两种5ml酸液精确加液（±0.1ml）+ 30分钟间隔分步加液，专为需要标准酸消解流程的自动化实验室设计，是样品前处理链路中的加酸核心环节

## Working Principle (工作原理)
模块通过 Z轴+Y轴双轴联动，配合中航电磁阀精确控制流量，实现两组150ml锥形瓶的两种5ml酸液（硝酸-高氯酸混合液 / 盐酸溶液）分步定量加液，精度±0.1ml

## Limits & Cautions (限制与注意事项)
只认150ml锥形瓶、只准±0.1ml、必须等30分钟

## Remarks (备注)

