---
identification_version:
  id: MOD-YJ-004
  module_part_no: HHM03J-050
  name: 50ml离心管加盐涡旋模块
  category: 加盐
  version: 2.0
  status: active
  last_updated: 2025/5/9
  owner: 高新章
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
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 1
  speed_rpm_max: 3000
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: 无
process_capability_1:
  function_code: 223
  function_description: 从自定的粉末容器向50ml尖底试管定量添加固体粉末，并通过涡旋的方式使加入的粉末与试管中原有的样品液充分混合/溶解
  operation_target: 50ml离心管
  operation_workflow: "盐管装入载台 - 盐管载台滑板处于待料位 - 盐管载台滑板移至开盐管盖位 - 盐管盖打开 - 加盐 - 盐管载台滑板移至盐管合盖位 - 盐管盖关闭 - 盐管载台滑板移至待料位 - 取下盐管 /  / 离心管放入载台 - 载台移至拆合盖位 - 夹紧离心管 - 拆盖（保持夹住瓶盖）- 松开离心管 - 载台移至加盐位/加液位 - 加盐/加液 - 载台移至涡旋位 - 涡旋压板下压 - 涡旋 - 停止 - 涡旋压板升起 - 载台移至拆盖位 - 夹紧离心管 - 合盖 - 松开离心管 - 载台移至上下料位 - 取下离心管"
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 300
  consumables: 离心管-50ml
  sub_functions: [220 拆盖，225 加盐，221 装盖，227 涡旋]
platform_compatibility:
  compatible_platforms:
    - PLT-800
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
  tags: [加盐，涡旋]
documentation:
  urs_url: 无
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.132
  power_kva_peak: 0.22
  voltage_v: 48
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 50ml离心管加盐涡旋模块

## Purpose (用途说明)
从自定的粉末容器向50ml尖底试管定量添加固体粉末，并通过涡旋的方式使加入的粉末与试管中原有的样品液充分混合/溶解

## Typical Scenarios (典型应用场景)
样品前处理自动化加盐混匀
适用于食品、环境、药品检测前处理，对50ml 尖底试管内样品液自动定量加盐→自动拆盖 / 加盖→高速涡旋溶解，全程无人操作，满足国标样品前处理中 “加盐 — 混匀 — 溶解” 的标准流程

## Working Principle (工作原理)
设备驱动定量送料机构完成固体盐类定量投放，将试剂加入 50ml 离心管内；随后电机带动承载工位做偏心圆周运动，转化为高频涡旋振荡，管内液体产生涡流翻滚冲击，使加入的盐料快速分散溶解，实现自动加盐与混匀一体化作业。

## Limits & Cautions (限制与注意事项)
1、仅适配50ml 尖底试管，必须放正、夹紧、无破损，禁止歪放、空管运行。
2、拆盖 / 加盖必须传感器确认成功，未盖紧禁止涡旋，防止喷溅。
3、加盐时盐管与试管对位准确，禁止漏盐、洒粉
4、涡旋转速≤3000rpm，运行中禁止触碰试管与按压机构。
5、运行时保持气缸压力正常、夹爪无松动，异常立即急停。
6、维护需断电断气，清理残留盐粉，防止卡轴、腐蚀。

## Remarks (备注)

