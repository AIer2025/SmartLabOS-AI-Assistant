---
identification_version:
  id: MOD-HY-002
  module_part_no: HHM01C-904
  name: 50-250ml容量瓶翻转摇匀模块
  category: 混匀
  version: 1.0
  status: active
  last_updated: 2026/4/17
  owner: 唐宋
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "单工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 50
  volume_ml_max: 250
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 300
  speed_rpm_max: 650
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: -50
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: 500
  function_description: 容量瓶翻转/摇匀
  operation_target: 50ml容量瓶,100ml容量瓶,200ml容量瓶,250ml容量瓶
  operation_workflow: 放入容量瓶- 真空发生器打开 - 真空吸嘴吸住容量瓶 - 容量瓶升至等待位 - 夹紧手指移至夹料位 - 容量瓶升至夹料位 - 夹紧 - 真空发生器关闭 - 容量瓶移至翻转位 - 翻转180° - 停顿 - 返回0° - 停顿 - 重复翻转、停顿、返回和停顿动作至翻转摇匀周期 - 容量瓶移至夹料位 - 手指松开 - 容量瓶下降至等待位 - 等待Y轴退回避让位 - 容量瓶下降至上下料位 - 取下容量瓶
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 8
  consumables: 50ml容量瓶,100ml容量瓶,200ml容量瓶,250ml容量瓶
  sub_functions: []
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
  wear_parts: 夹爪包胶
tags_meta:
  tags: [翻转, 摇匀, 容量瓶]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.23
  power_kva_peak: 0.39
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 50-250ml容量瓶翻转摇匀模块

## Purpose (用途说明)
本模块用于25～200ml 标准容量瓶的自动化上下料与180° 往复翻转摇匀，替代人工摇晃，实现溶液快速混匀、标准化处理，适配化学分析前处理流程

## Typical Scenarios (典型应用场景)
1、标准溶液配制后混匀
2、样品定容后均质混匀
3、难溶物质充分混合


## Working Principle (工作原理)
本模块依靠三轴运动 + 真空吸附 + 气动夹持 + 旋转驱动，实现容量瓶自动上下料与 180° 往复翻转摇匀

## Limits & Cautions (限制与注意事项)
上料放置：容量瓶直立放稳、瓶塞盖紧，真空吸附到位后再启动。
2. 夹持调整：手指气缸夹紧力度适中，过紧夹碎、过松脱落。
3. 参数设置：转速、时间按样品适配，低速启动、避免液体飞溅。
4. 运行安全：运行中严禁伸手靠近翻转区、夹爪、运动部件。
5. 日常维护：定期清洁吸嘴、夹爪；检查真空管路、气缸动作；长期不用断电关气

## Remarks (备注)

