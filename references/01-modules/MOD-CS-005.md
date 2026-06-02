---
identification_version:
  id: MOD-CS-005
  module_part_no: HHM14C-050
  name: 50ml离心管超声模块
  category: 超声
  version: 2.0
  status: active
  last_updated: 2026/1/23
  owner: 蔡勤超
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "四工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: "NA"
  volume_ml_max: "NA"
  temperature_c_min: 0
  temperature_c_max: 60
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: ""
process_capability_1:
  function_code: 726
  function_description: "#1 启动超声"
  operation_target: 50ml离心管
  operation_workflow: 冷却循环水开启 - 超声水槽水温达标（25±5℃）- 放入离心管（有盖） - 超声振子开启 - 超声（20min）- 停止 - 取出离心管 - 置于蒸发槽内 - 离心管底部水珠吸入海绵并蒸干 - 取走离心管
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 动态
  consumables: "离心管-50ml / "
  sub_functions: [726 1#超声启动 727 2#超声启动 728 排废水]
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
  wear_parts: 无
tags_meta:
  tags: [超声]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.2238
  power_kva_peak: 0.373
  voltage_v: 220
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: false
  requires_water: true
  requires_drain: false
---

# 50ml离心管超声模块

## Purpose (用途说明)
50ml离心管，可通过外超声波震动的方式，实现超声溶解操作

## Typical Scenarios (典型应用场景)
食品 / 环境样品自动化超声提取前处理
按照国标BJS202310、31658.17要求，对50ml 离心管内样品进行全自动超声提取、溶解、混匀，替代人工手扶超声，支持多工位并行，全程控温控水位，满足检测实验标准化作业。

## Working Principle (工作原理)
模块内置超声换能振子，将电能转换为高频机械振动，振动能量传导至贴合放置的 50ml 离心管；管内液体产生空化、湍流、微冲击效应，借助声波作用力，实现样品破碎、萃取、混匀、溶解、乳化处理，多工位同步作业可批量加工试样。

## Limits & Cautions (限制与注意事项)
1、仅适配50ml 离心管，必须带盖、拧紧、无漏液，禁止敞口超声。
2、必须水位正常、温控正常才能启动，缺水、超温禁止运行。
3、一次最多放4 个，需均匀放置，禁止偏载、空槽运行。
4、运行中禁止开盖、伸手、触碰超声槽，防止烫伤、触电、机械夹伤。
5、超声后必须擦干管底再下料，防止水滴污染平台。
6、维护需断电、停水、降温后操作，定期清洁水槽与除垢。

## Remarks (备注)

