---
identification_version:
  id: MOD-SPE-004
  module_part_no: HHM04H-901
  name: 6cc自动上样SPE模块
  category: SPE
  version: 1.0
  status: active
  last_updated: 2025/8/1
  owner: 王志
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
  volume_ml_max: 50
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
  function_code: 121
  function_description: 实现将样品富集和洗脱的功能
  operation_target: SPE柱：6ccHLB固相柱（通过更换载具可适配3cc、10ccSPE柱），样品瓶：125PP瓶、50离心管、15ml离心管、20ml西林瓶、10ml西林瓶（可通过更换载具适配不同的容器对象），接收瓶：50离心管或西林瓶（可通过更换载具适配不同的容器对象）
  operation_workflow: "1.离心管/西林瓶/SPE柱载台移至上下料位 -机械臂将离心管/西林瓶/SPE柱（带填料）放入载台 -Y1轴移至注液位-机械臂1200萃取瓶放入载台- 缓冲池移至取液针下方 -缓存池注入活化液5ml- 取液针插入缓冲池-注液针插入SPE柱-注射泵吸取全部活化液-切换三通阀 - 将液体注入SPE柱（按照指定速度-重复加活化液2-吹气-Z1轴抬升； / 2.取样针Y轴伸出-取液针Z1轴下降1200ml样品萃取瓶底部-注射泵吸取样品液- 切换三通阀- 注射针插入SPE柱-按照8-10ml/min的速度吐液-重复取样瓶液动作直至取完-吹气-Z1轴抬升-Z2轴抬升 /  / 洗针-取样针Y轴缩回-洗针池移至正下方-注入一定量的液体-取样针下探到洗针次底部-吸吐5次-液体吸入注射泵-Z1轴抬升-Y1轴SPE柱移至注液针下方-Z2轴下降注液针插入SPE柱-三通阀切换-液体注入SPE柱-吹干； / 3. 缓存池移动到取液针下方- 正压加液向缓存池注入20ml甲醇 -  取液针插入缓冲池-注液针插入SPE柱-注射泵吸取全部淋洗液-切换三通阀 - 将液体注入SPE柱-按照设定次数重复注液动作-20ml纯水同样动作注入SPE柱-吹气； / 4. 50ml接样瓶移至SPE柱下方- 正压加液向缓存池注入20ml洗脱液 - 取液针插入缓冲池-注液针插入SPE柱-注射泵吸取全部洗脱液-切换三通阀 - 将液体注入SPE柱---吹气-Z1轴抬升-Z2轴抬升； / 5. Y1轴移动到洗针位- 正压加液向洗针池内注入10ml纯水-取液针下探-吸吐-排废-Z1轴抬升；"
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±1%
  cycle_time_sec: 360
  consumables: "离心管-50ml/西林瓶-50ml / SPE柱-10ml / "
  sub_functions: [NA]
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
    []
  conflicts:
    []
  typical_pairings:
    - 正压加液
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: NA
tags_meta:
  tags: [SPE，净化，浓缩]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.168
  power_kva_peak: 0.28
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: true
---

# 6cc自动上样SPE模块

## Purpose (用途说明)
用于实现有机样品净化，通过SPE色谱柱分离除去样品中杂质

## Typical Scenarios (典型应用场景)
中药样品农残检测过程中的样品净化，生物样本的提取净化，中药成分分析的样品净化，食品样品农药残留检测样品净化

## Working Principle (工作原理)
6cc自动连续上样SPE模块，首先是在标准6cc SPE柱（含上下塞板+填料）里面塞入一个密封胶塞，目的使SPE柱处于密封状态，降低干扰物和液体残留的影响，提高目标物的回收率。通过注射泵吸取缓存池中的活化液注射到密封SPE柱里，润湿和活化SPE柱填料;注射泵从样口瓶里吸取样品液注射到密封SPE柱里，样品中的目标物被吸附在SPE柱填料上；然后注射泵从缓存池吸取淋洗液，淋洗填料上的干扰物和杂质，最后注射泵从缓存池吸取洗脱液，将吸附在填料上的目标物洗脱出来，接样瓶收集洗脱液。

## Limits & Cautions (限制与注意事项)
本模块固定 2 路并行，适配 6cc HLB 柱，换载具可用 3cc/6cc 柱；样品 / 接收瓶需匹配指定类型。仅 1–50ml、0.1–15ml/min 保证 ±1% 精度；禁高粘、强腐蚀、易堵样品。DC24V、6mm 气源，仅支持 MODBUS TCP，流程模板固定，共用针头需规范洗针，每 6 个月维护

## Remarks (备注)

