---
identification_version:
  id: MOD-CS-003
  module_part_no: HHM14A-902
  name: 50-250ml容量瓶超声模块
  category: 超声
  version: 1.0
  status: active
  last_updated: 2026/4/17
  owner: 唐宋
physical_specs:
  length_mm: 615
  width_mm: 304
  height_mm: 162
  weight_kg: 50
  module_slots: 0
  tray_slots: "十二工位"
  mount_type: 托盘位
module_performance:
  volume_ml_min: 50
  volume_ml_max: 250
  temperature_c_min: 20
  temperature_c_max: 30
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 1
  pressure_bar_max: 6
  vacuum_kpa: NA
peripherals:
  required_peripherals: 半导体制冷
process_capability_1:
  function_code: 110
  function_description: " 50ml-250ml 容量瓶超声"
  operation_target: 50ml容量瓶,100ml容量瓶,200ml容量瓶,250ml容量瓶
  operation_workflow: 放入容量瓶至上料位 - 夹爪闭合夹紧 - 注水至设置水位-冷却循环水开启 - 超声水槽水温达标（25±5℃） - 超声（20min）- 排水 - 夹爪打开 - 取下容量瓶
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 1200
  consumables: "容量瓶-50ml / 容量瓶-100ml / 容量瓶-200ml / 容量瓶250ml"
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
    - 称量模块
  typical_pairings:
    - --和模块配合使用
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: 超声振子
tags_meta:
  tags: [超声，容量瓶]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.36
  power_kva_peak: 0.6
  voltage_v: 24
  noise_db: 74
  heat_generating: "true"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: true
---

# 50-250ml容量瓶超声模块

## Purpose (用途说明)
本模块用于50~250ml 标准容量瓶的自动化超声溶解与清洗，替代人工手持操作，实现多瓶并行、标准化处理，适配样品前处理环节

## Typical Scenarios (典型应用场景)
1、环境样品前处理
2、食品 / 化工样品溶解定容
3、实验室批量样品高通量制备

## Working Principle (工作原理)
模块由三轴运动、气动夹持、超声振子、水槽温控组成，实现容量瓶自动上下料、超声溶解、恒温辅助的自动化流程。

## Limits & Cautions (限制与注意事项)
1、放瓶要求：容量瓶直立、放稳、夹持到位，防止倾斜、碰撞、掉落。
2、水位要求：水槽水位需达标，严禁无水 / 低水位超声，避免振子烧毁。
3、加热使用：加热时缓慢升温，避免骤热；容量瓶外壁保持干燥，防止温差炸裂。
4、运动安全：运行中严禁伸手靠近夹持、升降、平移机构，避免夹伤、撞伤。
5、日常维护：定期换水、清洁水槽、检查振子；长期不用排空水、断电

## Remarks (备注)

