---
identification_version:
  id: MOD-PY-003
  module_part_no: HHM30B-010
  name: 10ml比色管金属浴模块
  category: 平遥
  version: 1.0
  status: active
  last_updated: 2025/5/16
  owner: 唐宋
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "十二工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 10
  temperature_c_min: 25
  temperature_c_max: 100
  speed_rpm_min: 0
  speed_rpm_max: 400
  pressure_bar_min: "NA"
  pressure_bar_max: "NA"
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: 240
  function_description: 启动周期平遥
  operation_target: 10ml比色管
  operation_workflow: "1.机械手（或人工）逐一取两只已前处理好的比色管放置到治具加热平台上 / 2.单次摇匀10s（可设定）后停止摇动，并持续保温加热控制设定温度 / 3.根据前处理速度（预计间隔10min（可设定）），将放入另外两只比色管 / 4.重复第2-3步 / 5.当第一次放入的比色管达到设定时长（30min（可设定））时由机械手取出放入其他模块进行放气后重新插入该模组 / 6.当每组比色管加热时间满60min（含取出放气时间）后取出比色管流转至下工序"
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 100
  consumables: 比色管-10ml
  sub_functions: [241-机构定位到上下料位置]
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
  tags: [金属浴]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.444
  power_kva_peak: 0.74
  voltage_v: 220
  noise_db: 65
  heat_generating: "true"
  requires_ventilation: false
  requires_compressed_air: false
  requires_water: false
  requires_drain: false
---

# 10ml比色管金属浴模块

## Purpose (用途说明)
12通道10ml比色管加热定时平摇等功能

## Typical Scenarios (典型应用场景)
1、环境监测水样前处理
2、食品 / 农产品检测前处理
3、化工 / 制药样品恒温反应
4、科研实验恒温孵育

## Working Principle (工作原理)
由机械手放置比色管，通过步进电机实现可调转速与时长的平摇混匀，搭配三路独立加热棒与热电偶精准控温 100±1℃，按设定间隔分批放管、定时保温，达到指定时长完成样品加热

## Limits & Cautions (限制与注意事项)
1、仅适用于10ml 规格比色管，不得使用其他规格试管。
2、通道数最大12 通道，不可超量放置样品。
3、加热温度最高 100℃，禁止超温运行。
4、摇匀转速限定40–300rpm，超出范围会导致运行异常。

## Remarks (备注)

