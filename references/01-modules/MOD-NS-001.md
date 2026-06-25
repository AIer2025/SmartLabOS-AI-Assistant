---
identification_version:
  id: MOD-NS-001
  module_part_no: HHM05H-010
  name: 10ml大容量连续浓缩模块
  category: 浓缩
  version: 1.0
  status: active
  last_updated: 2025/7/10
  owner: 蔡勤超
physical_specs:
  length_mm: 450
  width_mm: 380
  height_mm: 600
  weight_kg: 50
  module_slots: 2
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 4
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: -95
peripherals:
  required_peripherals: 真空泵，空压机
process_capability_1:
  function_code: 200
  function_description: 浓缩
  operation_target: 10ml西林瓶,50ml尖底（圆底）瓶，100ml尖底（圆底）瓶，200ml尖底（圆底）瓶
  operation_workflow: 载台移至上下料位 - 放入样品瓶/西林瓶 - 加热吹风达到设定温度 - 真空泵开启 - 抽真空/加液管路准备就绪 - 载台移至取液位/浓缩位 - 压紧西林瓶瓶口 - 钢针伸入样品瓶底部 - 吸取样品 - 加样品进西林瓶 - 离心浓缩 - 至浓缩终点 - 吸取样品 - 加样品进西林瓶 - 离心浓缩 - 重复至全部样品浓缩完毕 - 停止 - 真空电磁阀关闭 - 破真空 - 载台移至上下料位 - 取下西林瓶
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: ""
  consumables: "10ml西林瓶,50ml尖底（圆底）瓶，100ml尖底（圆底）瓶，200ml尖底（圆底）瓶 / "
  sub_functions: [202-浓缩排空，203-预热，204-退出预热]
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
  wear_parts: 胶条，底部胶皮，顶部胶皮，密封圈
tags_meta:
  tags: [连续浓缩，大容量]
documentation:
  urs_url: URS连接
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.38
  power_kva_peak: 0.64
  voltage_v: 48
  noise_db: 70
  heat_generating: "true"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: true
---

# 10ml大容量连续浓缩模块

## Purpose (用途说明)
这个模块 = "大瓶取液 → 小瓶离心浓缩蒸干" 的自动化工作站，核心解决的是：把50~200ml大瓶中的有机溶剂样品，安全、可控、自动地浓缩蒸干到10ml西林瓶中，替代人工旋转蒸发操作

## Typical Scenarios (典型应用场景)
这个模块的典型应用 = 把50~200ml大瓶有机溶剂样品，通过"抽真空+高速离心+45~50℃热风"三重手段，安全、可控、自动地浓缩蒸干到10ml西林瓶，核心替代的是：人工旋转蒸发（危险、费时、不一致），四大场景的共同痛点是：样品量大、溶剂有毒、需低温防分解、蒸干终点难判断——模块全部解决

## Working Principle (工作原理)
原理 = 注射泵抽排转移 + 离心贴壁（5000~8000rpm）+ 真空降沸点 + 45~50℃热风加速 + 红外测温安全监控 + 流量/温度/真空度曲线判断终点，六位一体实现大瓶有机溶剂 → 10ml西林瓶的自动化浓缩蒸干

## Limits & Cautions (限制与注意事项)
六大红线不可碰：转速不超8000rpm、温度不超50℃、仅限尖底/圆底瓶+10ml西林瓶、必须初始化回零、必须先关真空再破真空再升Z轴、必须执行清洗工序——其余皆可调，但蒸干终点必须靠曲线判断，不可只靠时间。

## Remarks (备注)

