---
identification_version:
  id: MOD-GL-001
  module_part_no: HHM04M-002.5
  name: 2.5cc注射器过滤模块
  category: 过滤
  version: 1.0
  status: active
  last_updated: 2026/2/6
  owner: 王志
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
  volume_ml_max: 2.5
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 1
  pressure_bar_max: 6
  vacuum_kpa: NA
peripherals:
  required_peripherals: 空压机
process_capability_1:
  function_code: 637
  function_description: 过滤启动
  operation_target: "注射针筒-2.5cc / 针式过滤器-25mm / 西林瓶接样瓶-2ml"
  operation_workflow: 搬臂移液枪从样品瓶吸样品液，注射泵往过滤器里打入空气，正压使样品液通过过滤器先排掉前面一部分清洗过滤器进行预过滤，然后再进行过滤，对滤后溶液进行接样收集
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 210
  consumables: "注射针筒-2.5cc / 针式过滤器-25mm / 移液枪-1ml / 西林瓶接样瓶-2ml"
  sub_functions: [NA]
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
  wear_parts: NA
tags_meta:
  tags: [注射器过滤]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.09816
  power_kva_peak: 0.1636
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: true
---

# 2.5cc注射器过滤模块

## Purpose (用途说明)
预期实现2.5cc 注射器过滤工艺，将样品液中杂质分离出来，回收得到纯清的液体

## Typical Scenarios (典型应用场景)
1、生物样本微量除菌除杂
2、核酸 / 蛋白样品纯化前过滤
3、试剂配制与缓冲液除菌过滤
4、环境 / 食品微量样品前处理
5、微量贵重样品保护过滤

## Working Principle (工作原理)
通过搬臂移液枪完成样品液精准加样；由注射泵向过滤器内泵入空气形成正压环境，驱动样品液在正压作用下穿过滤膜。先排出部分初滤液，实现滤器预冲洗与预过滤，去除滤器杂质、消除污染；后续继续正压过滤，最终收集经滤膜净化后的合格滤液。

## Limits & Cautions (限制与注意事项)
1、严禁运行中干预：设备运行时禁止伸手、触碰运动部件、打开防护，防止夹伤、漏液污染、设备损坏。
2、定期校准：轴零点、注射泵流量、压力传感器、密封气缸行程需定期校准，防止定位不准、过滤失败、压力误报。
3、密封件维护：O 型圈、密封板定期清洁、更换；老化会导致漏气、正压不足、过滤效率下降。
4、滤膜更换：每批次更换新滤膜，防止交叉污染、堵塞；堵塞需及时停机更换，避免超压损坏。
5、异常处理：压力报警、漏液、滴漏、位置异常时，先断电断气再处理，禁止带电排查。
6、清洁要求：接触样品区域每次运行后清洁，防止残留结晶、污染样品、影响密封效果

## Remarks (备注)

