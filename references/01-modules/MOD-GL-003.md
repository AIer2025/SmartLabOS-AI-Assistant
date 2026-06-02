---
identification_version:
  id: MOD-GL-003
  module_part_no: HHM04A-030
  name: 30ML过滤模块
  category: 过滤
  version: 1.0
  status: active
  last_updated: 2025/4/25
  owner: 涂高祥
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
  volume_ml_max: 30
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
  function_code: 80
  function_description: 过滤
  operation_target: 30ml定制过滤杯
  operation_workflow: PP瓶载台移至上下料位 - 放入PP瓶 - 放入过滤杯 - 载台移至过滤位 - 压板下压 - 加压过滤 - 过滤完毕 - 压板上升 - 过滤杯载台移至上下料位 - 取下过滤杯 - PP瓶载台移至上下料位 - 取下PP瓶
  absolute_accuracy_pct: ""
  repeat_accuracy_pct: ""
  cycle_time_sec: 180
  consumables: 定制过滤杯-30ml
  sub_functions: [81-前后气缸伸出, 82-前后气缸缩回]
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
  tags: [过滤]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.3444
  power_kva_peak: 0.574
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 30ML过滤模块

## Purpose (用途说明)
30mlPP瓶的装载，过滤器的装载，实现往过滤器移样品后，微正压将样品透过滤膜，容器接收滤液的功能

## Typical Scenarios (典型应用场景)
盐湖所无机样品前处理系统

## Working Principle (工作原理)
模块通过 Y 轴滚珠丝杆电机实现PP 瓶与过滤筒的工位移动，光电传感器完成物料检测；气缸下压密封过滤筒后，以10kPa 微正压推动样品透过滤膜，实现固液分离；过滤完成后 Y 轴依次退回上下料位，完成滤筒与滤液瓶的自动出料，全程自动化运行。

## Limits & Cautions (限制与注意事项)
1、仅限30ml 标准 PP 瓶使用，过滤筒需匹配规格，禁止混用
2、过滤前必须保证气缸压紧密封，防止微正压泄漏导致过滤失败。
3、过滤气压严格控制在10kPa 左右，禁止超压导致漏液或爆膜。
4、运行时严禁触碰Y 轴运动区、气缸下压区，避免夹伤与碰撞。

## Remarks (备注)

