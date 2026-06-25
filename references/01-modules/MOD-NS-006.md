---
identification_version:
  id: MOD-NS-006
  module_part_no: HHM11B-250
  name: 35ml西林瓶离心浓缩模块（防腐高温）
  category: 浓缩
  version: 1.0
  status: active
  last_updated: 2025/10/17
  owner: 宾俊
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 27
  module_slots: 1
  tray_slots: "单工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 10
  temperature_c_min: 25
  temperature_c_max: 110
  speed_rpm_min: 1
  speed_rpm_max: 8000
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: -95Kpa
peripherals:
  required_peripherals: 真空泵
process_capability_1:
  function_code: 200
  function_description: 浓缩
  operation_target: 35ml西林瓶
  operation_workflow: "西林瓶载台移至放料位-加热吹风达到设定温度-真空泵开启电磁阀关闭-安装西林瓶至载台-载台移至浓缩位-压紧瓶口-旋转-真空电磁阀开启-涡旋终点-电机停止转动-真空电磁阀关闭-破真空-载台移至下料位-取下西林瓶；更换样品，重复测试 / 绘制流量值和真空度值曲线，判断终点，结束离心浓缩过程"
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 900
  consumables: 西林瓶-35ml
  sub_functions: [200 浓缩]
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
  wear_parts: 无
tags_meta:
  tags: [离心，浓缩]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.504
  power_kva_peak: 0.84
  voltage_v: 48
  noise_db: 65
  heat_generating: "true"
  requires_ventilation: true
  requires_compressed_air: true
  requires_water: false
  requires_drain: true
---

# 35ml西林瓶离心浓缩模块（防腐高温）

## Purpose (用途说明)
35ml西林瓶内，对强酸性溶液进行浓缩与脱酸，通过抽真空+高速旋转离心+加热的方式，实现浓缩蒸干功能

## Typical Scenarios (典型应用场景)
大连物化所

## Working Principle (工作原理)
模块依托离心旋转负压蒸发原理工作，整机采用防腐材质打造并适配高温控温系统。将 35ml 西林瓶均匀对称固定于转子工位，设备驱动转子高速旋转产生强大离心力，使瓶内液体紧贴瓶壁形成薄液膜；同步配合腔体负压抽真空降低溶剂沸点，再辅以高温恒温加热，加快溶剂分子快速挥发汽化，挥发气体统一外排，持续浓缩瓶内样品；依靠离心力抑制液体暴沸、飞溅与冲盖，在防腐耐蚀腔体环境下，高效完成样品低温 / 高温负压离心浓缩作业。

## Limits & Cautions (限制与注意事项)
这个模块是"强酸蒸干专用机"，温度≤110℃、转速≤8000rpm、单瓶处理、双电压供电、防辐射选型——用对了是利器，用错了（如超温、顺序反了、瓶型不对）就是炸瓶事故。

## Remarks (备注)

