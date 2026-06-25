---
identification_version:
  id: MOD-SPE-002
  module_part_no: HHM04F-020
  name: 20cc SPE模块V2.0
  category: SPE
  version: 2.0
  status: active
  last_updated: 2025/5/23
  owner: 王志/深圳
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 15
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 20
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: 121
  function_description: 活化/上样/淋洗/洗脱/
  operation_target: 20ml固相萃取柱,35ml钳口瓶,10ml钳口瓶
  operation_workflow: "活化：钳口瓶载台移至上下料位 - 钳口瓶放入载台 - SPE柱载台移至排废位 - 密封板下压 - 排废池移至排废位 -  加5ml甲醇 - 正压吹气 - 加5ml纯化水 - 正压吹气 -密封板抬起 / 预洗：SPE柱载台移至上下料位 - 排废池移至SPE柱下方 - 加样品进SPE柱 - SPE柱移至排废位 - 正压吹气 / 淋洗：SPE柱载台移至加液位 - SPE柱移至排废位 - 密封板下压 - 加5ml纯水 - 正压吹气 - 加5ml20%j甲醇 - 正压吹气 - SPE柱吹干 - 密封板抬起 / 洗脱：钳口瓶载台移至接液位 - 加5ml洗脱液 -密封板下压 - 正压吹气 - 密封板抬起 - 钳口瓶载台移至上下料位 - 取下钳口瓶"
  absolute_accuracy_pct: 0.1
  repeat_accuracy_pct: 0.2
  cycle_time_sec: 900
  consumables: "固相萃取柱-20ml / 钳口瓶-35ml / 钳口瓶-10ml / "
  sub_functions: [122-活化, 123-SPE底座到过柱位, 124-extra 30cc spe全流程, 125-过滤&接液]
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
    - 浓缩模块
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: ""
tags_meta:
  tags: [SPE, 20cc]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.18
  power_kva_peak: 0.31
  voltage_v: 48
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: true
---

# 20cc SPE模块V2.0

## Purpose (用途说明)
本模块用于实现20ml SPE工艺—上料>>活化>>预柱>>淋洗>>洗脱>>收集，参考31658具体描述为：固相萃取柱依次用甲醇5ml和水5ml活化，取备用液过柱，依次用水5ml和20%甲醇水溶液5ml淋洗，抽干，用洗脱液10ml洗脱，收集洗脱液。

## Typical Scenarios (典型应用场景)
1、环境样品前处理
2、食品 / 化工样品净化


## Working Principle (工作原理)
设备通过多轴机械手自动完成样品瓶上下料，依靠气缸压紧实现萃取柱密封，通过多路电磁阀控液、正压气体施压，依次完成萃取柱活化、样品上柱、杂质淋洗、目标物质洗脱流程，借助气压状态判断液体排净情况，整套工序循环运转，实现样品自动化固相萃取分离提纯。

## Limits & Cautions (限制与注意事项)
1、仅适配 20ml SPE 柱、35ml/10ml 钳口瓶，严禁用其他规格容器。
2、密封必须到位才能加液 / 加压，漏气、密封不良禁止运行。3、加液、吹气、压力、时间严格按工艺参数执行，不可随意修改。
4、管路、电磁阀、加液口严禁混用、错接，防止交叉污染。
5、运行中禁止触碰密封压板、SPE 柱、载台、针头，异常立即急停。
6、维护需断电、断气、泄压，排净管路残留液体再检修。

## Remarks (备注)

