---
identification_version:
  id: MOD-YY-002
  module_part_no: HHM09N-001
  name: 1ml移液模块
  category: 移液
  version: 1.0
  status: active
  last_updated: 2026/1/23
  owner: 徐翔宇
physical_specs:
  length_mm: 400
  width_mm: 180
  height_mm: 600
  weight_kg: 15
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 0.5
  volume_ml_max: 1
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: ""
process_capability_1:
  function_code: 100
  function_description: 移液
  operation_target: 4ml西林瓶，2ml离心管，1ml枪头
  operation_workflow: 载台移至上下料位 - 机械臂依次放入枪头/西林瓶/离心管 - 移至上枪头位 - 下降取枪头 - 上升至安全位 - 载台移动至吸液位 - 枪头下降吸液 - 定量泵吸液 - 枪头上升至安全位 - 载台移动至移液位 - 枪头下降全部吐液 - 上升至安全位 - 载台移动至上枪头位 - 退枪头 - 载台移动至上下料位 - 机械臂依次取出枪头/西林瓶/离心管 - 放入托盘；停顿1分钟，重复工作流程
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 270
  consumables: "西林瓶-4ml / 离心管-2ml / 枪头-1ml"
  sub_functions: [100 移液]
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
  wear_parts: 密封圈
tags_meta:
  tags: [移液]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.096
  power_kva_peak: 0.16
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 1ml移液模块

## Purpose (用途说明)
实现容器间液体样品/试剂的转移，可双通道转移，可单通道转移。

## Typical Scenarios (典型应用场景)
成都食检院

## Working Principle (工作原理)
1ml 移液模块依托气压活塞式吸排液原理工作，通过模组内置精密驱动机构带动活塞上下往复运动，利用密闭腔体形成正负气压差；下移活塞排出内部空气产生负压，精准吸取 1ml 定量液体，上推活塞形成正压，平稳完成液体定量排液释放，配合机械定位与行程限位结构，精准控制吸液、排液行程距离，从而稳定实现1ml 固定量程精准移取、加注液体，可搭配自动化设备完成批量、标准化连续移液作业。

## Limits & Cautions (限制与注意事项)
不同的移液量，移液参数不是线性关系，需要做测试进行调整

## Remarks (备注)

