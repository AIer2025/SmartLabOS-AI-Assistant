---
identification_version:
  id: MOD-TDS-001
  module_part_no: 3256000077
  name: TDS测定模块
  category: TDS
  version: 2.0
  status: active
  last_updated: 2025/5/9
  owner: 朱园林
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 15
  module_slots: 1
  tray_slots: "单工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: "NA"
  volume_ml_max: "NA"
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
  function_code: 60
  function_description: 测TDS
  operation_target: 30ml塑料瓶，125ml塑料瓶
  operation_workflow: 电极移至高位等待 - 校正液放入校正液载台 - 电极移至校正位 - 深入使校正液没过电极头 - 自动校准（使用不同浓度校正液重复校准过程） - 电极移出至冲洗位 -冲洗吹干（参考冲洗吹干效果测试结果） - 电极移出至冲洗位 - 冲洗吹干 - 样品放入样品载台- 电极移到检测位 - 深入使样品没过电极头 - 检测 - 电极移出至冲洗位 - 冲洗吹干 - 电极移至高位 - 取下样品
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 270
  consumables: "塑料瓶-30ml / 塑料瓶-125ml"
  sub_functions: [60 测TDS ，61 TDS校准]
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
  wear_parts: ""
tags_meta:
  tags: [测TDS, TDS校准]
documentation:
  urs_url: 无
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.1464
  power_kva_peak: 0.244
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: false
  requires_water: false
  requires_drain: false
---

# TDS测定模块

## Purpose (用途说明)
自动化控制电导率电极对样本液测定，并实现电极的清洗

## Typical Scenarios (典型应用场景)
水样 / 溶液 TDS（电导率）自动化检测 + 电极自动维护
适用于水质检测、环境监测、实验室样本分析，对待测样本液进行全自动 TDS 测定，全程自动完成电极校准→样本检测→清洗→吹干，无需人工值守，保证测量准确、无交叉污染。

## Working Principle (工作原理)
模块基于电导率换算原理开展检测，电极探头浸入水样后，水体中溶解离子可传导微弱电流，模块电路实时检测水体电导率数值。

## Limits & Cautions (限制与注意事项)
1、检测前必须完成三点校准，校准失效 / 电极异常禁止测样。2、电极必须清洗 + 吹干后再测下一样本，防止交叉污染。
3、运行中禁止触碰电极、XYZ 运动轴、清洗位，避免撞针与损坏。
4、确保清洗液、气源、废液排放正常，无水、无气禁止运行
5、异常立即急停，断电断气后再维护，禁止湿手操作电控部件。
6、电极轻拿轻放，禁止碰撞、划伤探头，保持探头清洁

## Remarks (备注)

