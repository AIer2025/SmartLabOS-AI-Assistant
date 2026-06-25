---
identification_version:
  id: MOD-ZY-008
  module_part_no: HHM09L-150
  name: 150ml锥形瓶全量转移
  category: 移液
  version: 1.0
  status: active
  last_updated: 2025/6/6
  owner: 蔡勤超/深圳
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
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: ""
process_capability_1:
  function_code: 580
  function_description: 全量转移启动
  operation_target: 150ml锥形瓶
  operation_workflow: 载台移至上下料位 - 放入锥形瓶/容量瓶 - 真空吸紧 - 载台移至取液位 - 破真空 - 取液钢针下降伸入锥形瓶底部 - 注射泵吸取样品 - 切换排液管路 - 注射泵将样品排入容量瓶 - 重复吸取至样品取净 - 清洗气缸下压 - 加清洗液 - 吸取清洗后样品 - 排入容量瓶 - 重复清洗并吸取至样品取净 - 取液钢针返回 - 真空吸紧 - 载台移至上下料位 - 破真空 - 取下锥形瓶/容量瓶 - 清洗管路 - 载台移至排废位 - 排废液
  absolute_accuracy_pct: 200ul
  repeat_accuracy_pct: NA
  cycle_time_sec: 180
  consumables: "锥形瓶-150ml / 容量瓶-50ml"
  sub_functions: []
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
    - 正压加液
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [全量, 转移, 150ml锥形瓶]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.11
  power_kva_peak: 0.17
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 150ml锥形瓶全量转移

## Purpose (用途说明)
实现150ml锥形瓶的液体全量移液至50ml容量瓶，模块里包含，全量移液、锥形瓶清洗、管路清洗等功能。

## Typical Scenarios (典型应用场景)
实验室自动化液体全量转移与清洗一体化案例
适用于生物、医药、检测实验室，将150ml 锥形瓶内的样本 / 试剂完全转移至 50ml 容量瓶，并自动完成锥形瓶清洗 + 管路清洗，实现无残留、无污染、全流程自动化移液。

## Working Principle (工作原理)
通过机械手（或人工）上料后，真空吸附固定容器，取液轴移位、钢针下移，注射泵配合电磁阀切换取液 / 排液回路，循环完成瓶内液体全量转移；转移结束后，启动气缸与电磁阀联动清洗管路，最后载台归位、释放容器，全程自动化实现液体无残留转移与管路清洁。

## Limits & Cautions (限制与注意事项)
1、仅适配150ml 锥形瓶、50ml 容量瓶，必须精准定位、真空吸紧再运行。
2、取液 / 排液针必须到位后再动作，禁止空撞、空抽、空排。3、移液前检查管路无堵塞、无漏液，电磁阀切换正常。
4、移液完成必须执行瓶体 + 管路清洗，防止残留与交叉污染。
5、运行中禁止触碰运动轴、针头、载台；异常立即急停。
6、维护需断电、断气、排空管路，禁止带压检修。

## Remarks (备注)
第一：液量转移基本完成，转移能够达到95％以上
第二：喷淋效果不佳，更改150ml锥形瓶吸液位置后，喷淋效果一半瓶壁能喷淋清洗到，另外一半瓶壁没有喷淋清洗到，淋洗效果并不理想，清洗效果不好
