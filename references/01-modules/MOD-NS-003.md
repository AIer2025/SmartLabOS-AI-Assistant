---
identification_version:
  id: MOD-NS-003
  module_part_no: HHM05D-004
  name: 4ml西林瓶离心浓缩模块V1.0
  category: 浓缩
  version: 1.0
  status: active
  last_updated: 2025/6/20
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
  volume_ml_max: 2
  temperature_c_min: 25
  temperature_c_max: 55
  speed_rpm_min: 1000
  speed_rpm_max: 8000
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: -95
peripherals:
  required_peripherals: 真空泵，空压机
process_capability_1:
  function_code: 200
  function_description: 浓缩
  operation_target: 4ml西林瓶
  operation_workflow: 西林瓶载台移至放料位-加热吹风达到设定温度-真空泵开启-抽真空/加液管路准备就绪-安装西林瓶至载台-载台移至加液位-加液2ml-载台移至浓缩位-压紧瓶口-离心浓缩-停止-真空电磁阀关闭-破真空-载台移至放料位-取下西林瓶
  absolute_accuracy_pct: ±1
  repeat_accuracy_pct: ±0.5
  cycle_time_sec: 0
  consumables: 4ml西林瓶
  sub_functions: [202-浓缩排空，203-预热，204-退出预热]
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
  tags: [浓缩]
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

# 4ml西林瓶离心浓缩模块V1.0

## Purpose (用途说明)
该模块用于将 4ml西林瓶内的有机溶剂样品，通过 抽真空 + 高速旋转离心 + 加热吹热风 的方式，直接在瓶内浓缩蒸干，实现小体积样品的自动化蒸干处理

## Typical Scenarios (典型应用场景)
这个模块 = "4ml西林瓶上料 → 瓶内真空离心热风蒸干 → 放料" 的全自动工作站，核心解决的是：把4ml西林瓶中的有机溶剂样品，不转移、不倒瓶、低温可控地浓缩蒸干，替代人工旋转蒸发，特别适合药物残留、农药检测、环境监测等需要批量处理小体积有机样品的场景

## Working Principle (工作原理)
离心贴壁（增大面积）+ 真空降压（降低沸点）+ 45~50℃热风（提供能量），三效协同实现低温快速蒸干，通过真空度+流量+温度三条曲线实时判断终点，全程自动化，替代人工旋转蒸发

## Limits & Cautions (限制与注意事项)
本模块限制少、操作简单，最核心的注意事项就是三条——温度不超50℃、转速5000~6000r/min、蒸干终点必须靠曲线判断不可靠时间，其余按流程执行即可安全运行

## Remarks (备注)

