---
identification_version:
  id: MOD-YJ-005
  module_part_no: HHM11C-125
  name: 125mlPP瓶连续加盐模块
  category: 加盐
  version: 1.0
  status: active
  last_updated: 2025/5/23
  owner: 高新章
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
  volume_ml_max: 125
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
  function_code: 130
  function_description: 加盐
  operation_target: 125mlPP瓶
  operation_workflow: "盐罐放入浮动载台 - 齿盘啮合 - 125mlPP瓶载台移至上下料位 - 称重传感器清零 - 125mlPP瓶放入载台 - 载台移至加盐位 - 齿盘转动 - 快速加盐 - 加盐量至95% - 慢速加盐 - 加盐量100% - 停止 - 加盐重量数值稳定 - 补充加盐 - 125mlPP瓶载台移至上下料位 - 取下125mlPP瓶； / 加盐过程中，无盐落下10s，振动盐罐浮动部件； / 更换125mlPP瓶，重复测试"
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: ±0.05g
  cycle_time_sec: 100
  consumables: "PP瓶-125ml / 盐罐-150g / "
  sub_functions: [131-滑台气缸缩回，123-滑台气缸伸出，124-盐罐气缸缩回]
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
    []
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [连续加盐]
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
  power_kva_peak: 0.15
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 125mlPP瓶连续加盐模块

## Purpose (用途说明)
向125mlPP瓶内添加指定重量的盐粉，并通过称重天平确定重量

## Typical Scenarios (典型应用场景)
1、水质检测 **125ml PP 瓶定量加盐（NaCl）** 前处理
2、环境水样氯离子调节、固定、消解前加盐工艺场景
3、COD、总氮、总磷等检测3g NaCl 精准定量添加
4、实验室粉末试剂高精度定量加料自动化场景

## Working Principle (工作原理)
通过 Y 轴移位、螺杆送料与振动辅助下料，配合称重天平实时闭环控制，双路独立精准向 PP 瓶添加 3g NaCl，精度达 ±0.05g、单次加盐≤10s

## Limits & Cautions (限制与注意事项)
1、仅限125ml 标准 PP 瓶使用，禁止其他规格容器。
2、仅适用于NaCl 盐粉，不可用于其他粉末、颗粒、腐蚀性物料。
3、单次加盐量3g，精度要求 **±0.05g**，不可超量程加料。

## Remarks (备注)

