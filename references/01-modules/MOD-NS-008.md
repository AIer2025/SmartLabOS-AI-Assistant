---
identification_version:
  id: MOD-NS-008
  module_part_no: HHM05D-010
  name: 10ml西林瓶离心浓缩模块
  category: 浓缩
  version: 2.0
  status: active
  last_updated: 2025/5/9
  owner: 宾俊
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 27
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 10
  temperature_c_min: 25
  temperature_c_max: 55
  speed_rpm_min: 1000
  speed_rpm_max: 6000
  pressure_bar_min: 1
  pressure_bar_max: 6
  vacuum_kpa: -95
peripherals:
  required_peripherals: 真空泵
process_capability_1:
  function_code: 200
  function_description: 浓缩
  operation_target: 10ml西林瓶
  operation_workflow: "西林瓶载台移至放料位-加热吹风达到设定温度-真空泵开启电磁阀关闭-安装西林瓶至载台-载台移至浓缩位-压紧瓶口-旋转-真空电磁阀开启-涡旋终点-电机停止转动-真空电磁阀关闭-破真空-载台移至下料位-取下西林瓶；更换样品，重复测试 / 绘制流量值和真空度值曲线，判断终点，结束离心浓缩过程"
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 900
  consumables: 西林瓶-10ml
  sub_functions: [200 浓缩]
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
  wear_parts: 下胶垫、胶条、定位胶皮，密封圈
tags_meta:
  tags: [离心，浓缩]
documentation:
  urs_url: 无
  urd_url: 无
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.24
  power_kva_peak: 0.38
  voltage_v: 48
  noise_db: 67
  heat_generating: "true"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: true
---

# 10ml西林瓶离心浓缩模块

## Purpose (用途说明)
10ml西林瓶内，对强酸性溶液进行浓缩与脱酸，通过抽真空+高速旋转离心+加热的方式，实现浓缩蒸干功能

## Typical Scenarios (典型应用场景)
样品全自动真空离心加热浓缩蒸干
面向食品、环境、药品、水质检测前处理，对50ml 西林瓶内有机溶剂样品，通过真空 + 高速旋转离心 + 恒温热风三合一方式，实现样品快速浓缩、定量蒸干，全程自动化、无人值守，满足检测实验室样品浓缩标准作业。

## Working Principle (工作原理)
西林瓶固定于转子工位，高速旋转产生离心力，液体贴附瓶壁形成薄层；腔体内形成负压降低溶剂沸点，辅以加热促使溶剂汽化挥发并排出；离心力规避液体暴沸飞溅，逐步缩减液体容积，实现样品浓缩

## Limits & Cautions (限制与注意事项)
1、仅适配50ml 西林瓶，必须放正、无破损、无漏液，禁止歪瓶、空瓶运行。
2、浓缩前必须密封到位，未压紧、漏气禁止抽真空与高速旋转。
3、转速控制在1000–6000r/min，严禁超速；热风温度45–50℃，超温禁止运行。
4、必须先破真空、再上升气缸，禁止带压开盖。
运行中严禁触碰旋转载台、下压气缸、热风出口，防烫伤、夹伤、甩溅。
5、异常立即急停，维护需断电、断气、破真空、冷却后操作。

## Remarks (备注)

