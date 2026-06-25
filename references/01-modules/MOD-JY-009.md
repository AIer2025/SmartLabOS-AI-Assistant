---
identification_version:
  id: MOD-JY-009
  module_part_no: HHM02M-015
  name: 15ml离心管拆装盖加液模块
  category: 加液
  version: 2.0
  status: active
  last_updated: 2025/1/16
  owner: 何向东
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 15
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: 169
  function_description: 拆盖-加液-装盖
  operation_target: 15ml离心管
  operation_workflow: 载台移动到上下料位-放入离心管-载台移动到拆盖-夹紧离心管-Z轴向下移动到达离心管拆盖位置-钧舵夹爪夹紧瓶盖-旋转并向上运动打开管盖-夹持管盖-旋转并向下运动盖上管盖-载台移动到上下料位-取下离心管；
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 106
  consumables: 离心管-15ml
  sub_functions: [165 拆盖，166 装盖，167 加液]
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
  wear_parts: 手指胶皮，胶条
tags_meta:
  tags: [15ml离心管，拧盖，合盖，加液]
documentation:
  urs_url: URS连接
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.228
  power_kva_peak: 0.38
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 15ml离心管拆装盖加液模块

## Purpose (用途说明)
本模块是专为 15ml 尖底离心管设计的自动化开合盖执行单元，用于实验室、自动化产线中离心管的开盖、合盖、夹紧、取放瓶等标准化操作，可配合机械手或人工实现无人 / 少人化拆盖作业

## Typical Scenarios (典型应用场景)
1.实验室自动化样本处理工作站
2. 生物制药样品制备线
3. 疾控 / 检测中心样本前处理
4. 科研实验室高通量处理平台
5. 体外诊断试剂灌装产线

## Working Principle (工作原理)
本模块通过Y 轴送料定位 + Z 轴旋盖升降 + 夹爪夹紧旋转 + 光电传感检测协同动作，自动完成 50ml 尖底离心管的夹紧、开盖、取瓶、合盖全流程

## Limits & Cautions (限制与注意事项)
专用性：仅适配 15ml 尖底离心管，不可混用其他规格
运动参数：Y 轴 100mm、Z 轴 50mm，速度 50mm/s，定位 ±0.1mm
动作逻辑：先定位→再夹紧→同步旋转 + 轴向移动完成旋盖
安全限制：严禁超程超速、屏蔽传感器、无瓶空旋、伸手入工作区
使用前提：传感器正常、夹紧有效、供电稳定方可运行

## Remarks (备注)

