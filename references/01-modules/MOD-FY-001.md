---
identification_version:
  id: MOD-FY-001
  module_part_no: HHM40Q-001
  name: 水质分样平台
  category: 分样
  version: 1.0
  status: active
  last_updated: 2026/4/24
  owner: 唐宋
physical_specs:
  length_mm: 1200
  width_mm: 850
  height_mm: 2055
  weight_kg: 348
  module_slots: 6
  tray_slots: "四工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 1000
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: "NA"
  speed_rpm_max: "NA"
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: 空压机
process_capability_1:
  function_code: 670
  function_description: 从来样瓶中抽取液体，通过出液针注入到多种的接样容器中，完成分样操作
  operation_target: 15ml比色管,25ml比色管,30mlPP瓶,50ml离心管,250ml锥型瓶,500ml培养瓶,1000ml培养瓶
  operation_workflow: "初始化-根据感应器判断加水是否正常 / 来样瓶-来样瓶上样-扫码-拆盖-Y轴到吸液位置 / 接样瓶-接样瓶上样-扫码-拆盖-Y轴到接液位置 / 分液-吸液针到吸液位置-润洗管道-吸液-吐液针已到吐液位置-吐液-重复分样过程 / 下料-来样品拧盖-下料-接样瓶拧盖-下料 / 清洗-吸液针到洗针次-吐液针到洗针次-清洗"
  absolute_accuracy_pct: ±2%
  repeat_accuracy_pct: ±2%
  cycle_time_sec: 180
  consumables: 15ml比色管,25ml比色管,30mlPP瓶,50ml离心管,250ml锥型瓶,500ml培养瓶
  sub_functions: [670-接样1(50ml离心管)移液, 671-接样2(比色管)移液, 672-接样3(比色管)移液, 673-接样4(锥形瓶)移液, 674-管路清洗, 675-500ml肖特瓶拆盖, 676-500ml肖特瓶装盖, 677-1#接样瓶装盖, 678-1接样瓶装盖, 680-2接样瓶拆盖, 681-2接样瓶装盖, 683-3接样瓶拆盖, 684-3接样瓶装盖, 686-4接样瓶拆盖, 687-4接样瓶装盖, 689-移液前准备]
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
  wear_parts: 蠕动泵泵头软管
tags_meta:
  tags: [分液, 分样]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 1
  power_kva_peak: 1.6
  voltage_v: 220
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: true
---

# 水质分样平台

## Purpose (用途说明)
用于液体样品的自动化精准分样，可适配带盖 / 无盖、大小规格接样容器，自动完成样品识别、定量分装、容器封盖及管路清洗，支持单一样品多分装，高效适配实验室或工业批量分样需求

## Typical Scenarios (典型应用场景)
长春工厂-智慧水质理化检测实验室

## Working Principle (工作原理)
依托多轴滑台、气动夹爪、旋转机构、蠕动泵及扫码组件协同运作，自动完成来样瓶扫码、拆盖、吸液，对接样容器分类完成扫码、拆盖 / 直放、精准定量注液、封盖，支持多样品配比分液，作业后自动洗针复位，全流程自动化流转。

## Limits & Cautions (限制与注意事项)
三个分液通道不能并发，需要一个一个完成

## Remarks (备注)

