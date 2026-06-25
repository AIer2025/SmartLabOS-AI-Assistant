---
identification_version:
  id: MOD-JY-007
  module_part_no: HHM10K-901
  name: 自动补液系统
  category: 加液
  version: 1.0
  status: active
  last_updated: 2025/8/22
  owner: 徐翔宇
physical_specs:
  length_mm: 450
  width_mm: 450
  height_mm: 400
  weight_kg: 50
  module_slots: 0
  tray_slots: "四工位"
  mount_type: 平台外置
module_performance:
  volume_ml_min: "NA"
  volume_ml_max: "NA"
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: -70
peripherals:
  required_peripherals: 真空泵
process_capability_1:
  function_code: xxx
  function_description: 检测试剂瓶液位，自动完成补液
  operation_target: 试剂瓶
  operation_workflow: 液位传感器反馈检测不到液面-通气电磁阀切换到负压-补液电磁阀连通-从补液试剂瓶向正压加液试剂瓶内补液-液面到达液位传感器位置-补液电磁阀关闭-通气电磁阀切换到正压
  absolute_accuracy_pct: ±2%
  repeat_accuracy_pct: ""
  cycle_time_sec: 300
  consumables: 试剂瓶
  sub_functions: [NA]
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
    []
  conflicts:
    []
  typical_pairings:
    []
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [补液、加液]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.375
  power_kva_peak: 0.625
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: false
---

# 自动补液系统

## Purpose (用途说明)
自动液位检测，完成补液

## Typical Scenarios (典型应用场景)
适用于实验室高通量前处理，包括 SPE 柱活化 / 淋洗 / 洗脱补液、容量瓶定容、色谱流动相补给、恒温振荡试剂添加及样品稀释 / 内标加注。覆盖环境、食品、医药等检测场景，实现精准定量、防污染、降误差、标准化流程，适配自动化工作站

## Working Principle (工作原理)
液位传感器检测高低液位，当低于低液位时，开启自动补液，抽真空降低 瓶内压力，把液体从补液中抽取回来；达到高液位，自动停止补液

## Limits & Cautions (限制与注意事项)
仅适配指定规格容器与试剂类型，不适用于高粘度、易结晶、强腐蚀或易燃易爆液体。补液精度受温湿度、气压影响，管路易堵需定期维护；依赖稳定电源与洁净气源，防交叉污染需规范管路清洗，不可超量程补液，安装需水平稳固。

## Remarks (备注)

