---
identification_version:
  id: MOD-WX-005
  module_part_no: HHM50B-005
  name: 5ml离心管拆盖加液涡旋吸磁模块
  category: 涡旋
  version: 1.0
  status: active
  last_updated: 2025/9/12
  owner: 涂高祥
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "单工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 0.02
  volume_ml_max: 4
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 1
  speed_rpm_max: 3000
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: 空压机
process_capability_1:
  function_code: 167
  function_description: 拆盖/加液/装盖/涡旋
  operation_target: 5ml离心管
  operation_workflow: 初始化-上料离心管-传感器检测反馈上料成功-Y轴到拆盖位置-气缸动作夹紧离心管-拧盖-Y轴到上下料位-移液-Y轴到加液为止-加液-Y轴到拧盖位-拧盖-Y轴到涡旋位-涡旋-Y轴到下料位-下料-结束
  absolute_accuracy_pct: "(1)1ml：±0.7％ / (2)0.3~0.5ml：±2％ / (3)0.02~0.1ml：±3％"
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 60
  consumables: 离心管-5ml
  sub_functions: [165-拆盖，166-装盖，168-拆盖/加液/装盖/涡旋/拆盖，169-加液/装盖/涡旋/拆盖，170-装盖/涡旋，171-加液/装盖/涡旋]
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
    - 正压加液模块
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: 涡旋压紧件上的海绵
tags_meta:
  tags: [加液、涡旋、吸磁]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.344
  power_kva_peak: 0.574
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 5ml离心管拆盖加液涡旋吸磁模块

## Purpose (用途说明)
5ml离心管拆盖，加入一定液体，装盖，涡旋混匀，拆盖吸磁，弃掉废液。

## Typical Scenarios (典型应用场景)
本模块主要用于生物样本前处理、核酸提取、蛋白纯化、免疫磁分离、IVD 检测，实现离心管自动拆盖、吸磁、多通道加液、盖盖、涡旋混匀，显著提升自动化水平与实验稳定性

## Working Principle (工作原理)
模块由平移 / Z 轴电机、电爪、吸磁机构、4 通道加液电磁阀、涡旋电机组成，PLC 全流程控制。
平移电机将 5ml 离心管移至拆盖位，吸磁机构吸附管内磁珠，电爪旋开管盖；移至加液位，电磁阀精准加注 20μL–4ml 试剂；随后旋紧管盖，移至涡旋位高速混匀（0–3000rpm），最后送回上下料位，完成拆盖、吸磁、加液、旋盖、涡旋全自动化作业。

## Limits & Cautions (限制与注意事项)
限制：仅适配 5ml 尖底离心管；液体需兼容材质、无强腐蚀；磁珠粒径 / 磁性需匹配吸磁机构；加液范围 20μL–4ml、涡旋≤3000rpm。

## Remarks (备注)

