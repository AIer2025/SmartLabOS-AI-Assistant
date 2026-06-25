---
identification_version:
  id: MOD-HY-004
  module_part_no: HHM01H-250
  name: 250ml锥形瓶加液混匀模块
  category: 混匀
  version: 1.0
  status: active
  last_updated: 2025/8/29
  owner: 许云燕
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
  volume_ml_max: 250
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 100
  speed_rpm_max: 500
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: 空压机
process_capability_1:
  function_code: 610
  function_description: 实现250ml锥形瓶的拆盖、加液（包括温水、酸性溶液、碱性溶液）、混匀的操作
  operation_target: "250ml锥形瓶，磁力子 "
  operation_workflow: "上料：两个带盖的250ml锥形瓶； / 拆盖：逆时针旋转盖子拧开； / 加液：往锥形瓶加入试剂，加入体积可设置； / 装盖：顺时针拧紧盖子； / 下料：两个带盖的250ml锥形瓶；"
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 252
  consumables: 250ml锥形瓶、磁力子
  sub_functions: [搅拌]
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
  tags: [加液混匀、磁力子搅拌]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.42
  power_kva_peak: 0.7
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 250ml锥形瓶加液混匀模块

## Purpose (用途说明)
适配 250ml 标准锥形瓶，自动完成拆盖、多通道加液（温水 / 酸碱液）、磁力混匀、装盖全流程。用于样品前处理、试剂配制、反应体系制备、萃取 / 消解预处理等场景，实现标准化自动化操作，减少人工误差、降低污染风险，适配实验室高通量处理

## Typical Scenarios (典型应用场景)
1、食品检测：农残 / 重金属样品消解前自动加酸、混匀预处理；
2、环境监测：水样 / 土壤样品加提取液、恒温混匀；
3、医药化工：中间体配制、反应液酸碱调节与混匀；
4、科研实验：多组分试剂配比、缓冲液配制、样品稀释混匀

## Working Principle (工作原理)
由 Y 轴移栽、Z 轴拆盖、6 通道加液、磁力混匀及压紧组件构成。Y 轴将瓶移至拆盖位，气缸压紧，Z 轴旋转拆盖；再移至加液位，多通道定量加液；底部磁力机构旋转混匀；最后回位装盖，完成自动化流程

## Limits & Cautions (限制与注意事项)
仅适配标准 250ml 锥形瓶，非标 / 异形瓶不可用；可加温水、稀酸碱液，禁强腐蚀、高粘度、易结晶 / 易燃易爆液体。磁力混匀力度固定，不适用含大量固体颗粒样品；DC24V 供电、依赖稳定气源，安装需水平，定期校准夹爪、管路与传感器。

## Remarks (备注)

