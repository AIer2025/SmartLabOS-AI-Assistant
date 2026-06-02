---
identification_version:
  id: MOD-DR-002
  module_part_no: HHM08E-035
  name: 35ml消解管清洗定容模块
  category: 定容
  version: 1.0
  status: active
  last_updated: 2026/4/24
  owner: 涂高祥
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
  volume_ml_max: 35
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
  function_code: 721
  function_description: 定容
  operation_target: 35ml消解管
  operation_workflow: 载台移至放料位-放置消解管至载台-清洗消解管外壁-载台移至加装拔盖位-拔盖-载台移至定容位-定容-载台移至加装拔盖位-装盖-载台移至放料位-取下消解管；
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±1%
  cycle_time_sec: 150
  consumables: 35ml消解管
  sub_functions: [722-平台洗针]
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
    - 正压加液
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [定容, 清洗，34ml消解管]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.12
  power_kva_peak: 0.2
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: true
---

# 35ml消解管清洗定容模块

## Purpose (用途说明)
本模块专为35ml 消解管设计，集成外壁清洗吹干、自动拔盖、正压定容三大核心功能，用于消解后样品的自动化前处理，衔接微波消解与后续检测工序

## Typical Scenarios (典型应用场景)
案例 1：环境土壤重金属检测前处理
案例 2：食品农产品元素分析
案例 3：水质 / 废水样品痕量分析
案例 4：地质矿物样品元素测试
案例 5：第三方检测 / 科研实验室高通量制备

## Working Principle (工作原理)
依托多轴机械传动、气动夹持、流体泵阀控液及传感检测技术，配合机械手完成工位转运，通过泵体控流实现冲洗吹干，靠丝杆与气缸联动完成自动拔盖，以正压供液搭配液位传感精准完成定容，全程自动化联动作业。

## Limits & Cautions (限制与注意事项)
1、放管要求：消解管直立、放正、卡紧到位，避免倾斜、掉落、碰撞。
2、清洗吹干：清洗时废液管路畅通；吹干气压适中，避免管体移位、水渍残留。
3、拔盖操作：盖子完好、无粘连、无变形；夹爪夹紧到位后再拔盖，防止滑盖、碎盖。
4、定容加液：加液管对中、缓慢伸入；避免碰擦管壁、溅液、污染。
5、日常维护：定期清洁喷淋口、吹干嘴、排液管路；长期不用排空液体、断电关气

## Remarks (备注)

