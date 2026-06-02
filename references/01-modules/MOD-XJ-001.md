---
identification_version:
  id: MOD-XJ-001
  module_part_no: NA
  name: 150ml锥形瓶电热板加热消解
  category: 消解
  version: 1.0
  status: active
  last_updated: 2025/5/9
  owner: 汪健
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: "NA"
  volume_ml_max: "NA"
  temperature_c_min: 0
  temperature_c_max: 300
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 1
  pressure_bar_max: 6
  vacuum_kpa: NA
peripherals:
  required_peripherals: 抽风机
process_capability_1:
  function_code: 540
  function_description: 150ml锥形瓶的加热、将酸气引入通风厨排放功能
  operation_target: 150ml锥形瓶
  operation_workflow: 加热台移至上下料位 - 放入锥形瓶 - 瓶座移至消解位 - 加热台与防护罩贴合 - 排气扇开启 - 加热开启 -保温- 消解 - 持续排除酸雾 - 按时间固定工艺 - 加热停止 - 排气扇关闭 - 加热台移至上下料位 - 取下锥形瓶
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 1800
  consumables: 150ml锥形瓶
  sub_functions: [540-启动锥形瓶加热消解]
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
    - 正压加液模块
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [加热消解]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 1.08
  power_kva_peak: 1.8
  voltage_v: 220
  noise_db: 65
  heat_generating: "true"
  requires_ventilation: true
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 150ml锥形瓶电热板加热消解

## Purpose (用途说明)
锥形瓶的加热、将酸气引入通风厨排放功能

## Typical Scenarios (典型应用场景)
实验室样品自动化加热消解与酸气处理
面向食品、环境、水质、土壤等检测实验室，对150ml 锥形瓶内样品进行高温加热消解，同时自动密封、抽风排酸气，实现无人值守、安全高效的样品前处理。

## Working Principle (工作原理)
150mL 锥形瓶搭配电热板加热消解，依靠电热板内部电阻丝通电后将电能转化为热能，热量通过板面以热传导形式贴合锥形瓶瓶底传递至瓶内消解试样，瓶内液体受热产生自然对流，让整体物料均匀升温，设备温控组件实时监测板面温度并动态调节加热功率，稳定维持设定消解温度，高温环境下消解试剂与样品充分发生氧化、分解、溶出等化学反应，破坏样品原有基体结构，使待测组分分离析出，锥形瓶敞口结构可顺畅排出反应产生的废气与气体，保障消解反应持续稳定进行，整体操作需在通风环境下开展，加热过程循序渐进控温，避免液体暴沸溅出，保证消解作业安全与检测结果准确性。

## Limits & Cautions (限制与注意事项)
1、仅适配150ml 锥形瓶，必须放正、无破损、无漏液，禁止空瓶高温干烧。
2、加热温度 **≤300℃**，PT100 传感器异常、温控失效禁止加热。
3、加热前必须防护罩合盖到位，并开启排气扇排酸气，严禁敞开加热。
4、运行中禁止触碰高温加热台、防护罩、运动轴，防止烫伤与夹伤。
5、消解完成必须自然降温后再取瓶，维护需断电冷却后操作。
6、禁止在无通风、易燃、腐蚀性超标环境使用，保持排气通畅。

## Remarks (备注)

