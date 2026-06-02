---
identification_version:
  id: MOD-GL-002
  module_part_no: HHM04J-005
  name: 5cc 针式过滤模块
  category: 过滤
  version: 1.0
  status: active
  last_updated: 2025/9/19
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
  volume_ml_max: 5
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
  function_code: 310
  function_description: 启动针式过滤
  operation_target: 15ml离心管/10ml西林瓶，2ml西林瓶/2ml离心管，13mm针式过滤器
  operation_workflow: "初始化-载台移至上下料位-机械臂放入接样瓶-检测成功-机械臂放入针式过滤器-监测成功-机械臂放入样品瓶-检测成功-Y1轴载台移至取样位-注射泵吸液-Z2轴轴抬升-抽空气1ml / 预过滤-Y2轴移至过滤位-Y1轴原样瓶移至过滤位-Z2轴下降插入过滤器-过滤一定量样品至原样瓶-取样针Z2轴上升至安全位 / 过滤-接样瓶Y1轴移动到过滤位-过滤器Y2轴移动到过滤位-Z2轴下降插入过滤器-过滤剩余样品至接样瓶-取样针Z2轴上升至安全位 / 洗针-洗针池Y1轴移动到取样针正下方-注入洗针池3ml清洗剂-注射泵吸吐三次-排废-注入3ml纯水-注射泵吸吐三次-排废 / 空吹-注射泵吸满空气-空吹-取样针上升至安全位 / 下料-机械臂下料原样瓶-针式过滤器-接样品"
  absolute_accuracy_pct: ""
  repeat_accuracy_pct: ""
  cycle_time_sec: 520
  consumables: "离心管-15ml / 西林瓶-10ml / 离心管-2ml / 西林瓶-2ml / 针式过滤器-13mm"
  sub_functions: [251-清洗标针]
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
  wear_parts: 加液电磁阀
tags_meta:
  tags: [针式过滤]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.132
  power_kva_peak: 0.22
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: true
---

# 5cc 针式过滤模块

## Purpose (用途说明)
本模块为实验室样品前处理专用过滤设备，用于样品液精密过滤除杂，自动完成吸液、过滤、洗针、空吹全流程，适配微量样品净化需求。

## Typical Scenarios (典型应用场景)
广泛用于环境监测、食品检测、医药化工、科研实验室，适配水质、食品、生物样品、试剂溶液等过滤净化，为色谱、质谱分析提供洁净样品

## Working Principle (工作原理)
注射泵吸取样品液，注射泵套上针式过滤器，样品液通过针式过滤器进行过滤，对滤后溶液进行接样收集。

## Limits & Cautions (限制与注意事项)
清洗不彻底，活塞走到0位，注射器里还存在液体不能排出，排空气也不行

## Remarks (备注)

