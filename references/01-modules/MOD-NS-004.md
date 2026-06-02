---
identification_version:
  id: MOD-NS-004
  module_part_no: HHM05J-050
  name: 50ml西林瓶连续离心浓缩
  category: 浓缩
  version: 1.0
  status: active
  last_updated: 2026/5/13
  owner: 宾俊
physical_specs:
  length_mm: 450
  width_mm: 380
  height_mm: 600
  weight_kg: 45
  module_slots: 2
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 30
  temperature_c_min: 25
  temperature_c_max: 55
  speed_rpm_min: 1000
  speed_rpm_max: 8000
  pressure_bar_min: 0.1
  pressure_bar_max: 7
  vacuum_kpa: -95
peripherals:
  required_peripherals: 真空泵
process_capability_1:
  function_code: 200
  function_description: 浓缩
  operation_target: 50ml西林瓶
  operation_workflow: 载台移至上下料位 - 放入西林瓶 - 加热吹风达到设定温度 - 真空泵开启 - 抽真空/加液管路准备就绪 - 载台移至浓缩位 - 压紧瓶口 - 加样品 - 离心浓缩 - 至浓缩终点(按浓缩时间) -破真空- 加样品 - 离心浓缩 - 重复至全部样品浓缩完毕 -最后一次按流量判断浓缩终点- 停止 - 真空电磁阀关闭 - 破真空 - 载台移至上下料位 - 取下西林瓶
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 1800
  consumables: 50ml西林瓶/125mlpp瓶/50ml尖底瓶/100ml/200ml尖底瓶
  sub_functions: [201-复溶, 202-浓缩排空, 203-预热, 204-退出预热, 205-浓缩+复溶]
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
  wear_parts: 底座胶皮,胶条,定位胶垫，密封圈
tags_meta:
  tags: [浓缩, 复溶，连续]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.56
  power_kva_peak: 1
  voltage_v: 48
  noise_db: 65
  heat_generating: "true"
  requires_ventilation: true
  requires_compressed_air: true
  requires_water: true
  requires_drain: true
---

# 50ml西林瓶连续离心浓缩

## Purpose (用途说明)
该模块用于有机溶剂样品的自动化移液 + 离心浓缩蒸干，适配多种样品瓶与目标西林瓶，实现连续化前处理

## Typical Scenarios (典型应用场景)
案例 1：有机污染物样品浓缩
案例 2：农药残留样品前处理
案例 3：药物研发样品蒸干（药物合成 / 中间体）
案例 4：食品添加剂与香精香料浓缩
案例 5：批量样品自动化前处理（高通量）

## Working Principle (工作原理)
1、移液：注射泵 + 电磁阀切换，自动抽排、清洗，实现无污染精准转移。
2、浓缩：离心（扩面积）+ 加热（提温度）+ 真空（降沸点） 三效合一，快速蒸干有机溶剂。
3、全程自动化：三轴运动、定位、夹紧、密封、抽真空、加热、旋转、清洗、出料，全流程无人值守

## Limits & Cautions (限制与注意事项)
1、装瓶与定位
瓶子必须放正、夹紧到位，偏心会导致高速旋转振动、漏液、碎瓶。
西林瓶必须瓶口完好、无裂纹、无崩边，否则密封不严、抽不上真空。
2、移液与清洗
移液前确认管路无气泡、电磁阀切换正常，避免空抽、不准量。
每次样品后必须自动清洗管路与针头，防止交叉污染。
禁止干抽、空转注射泵，易磨损泵芯。
3、浓缩过程
浓缩前确认：真空管路不漏、加热风扇正常、密封轴干净无杂质。
运行中严禁开盖、伸手、靠近旋转区，防高速飞溅、夹伤。
红外测温异常时立即停机，防止超温。
4、结束与维护
蒸干后必须先破真空、再抬压头、再取瓶，防止负压倒吸。
长期不用：排空管路、清洗腔体、断电关气、干燥存放。

## Remarks (备注)

