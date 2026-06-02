---
identification_version:
  id: MOD-PH-002
  module_part_no: HHM31D-001
  name: 萃取瓶调PH模块
  category: PH
  version: 1.0
  status: active
  last_updated: 2025/8/29
  owner: 黄锐
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "单工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: "NA"
  volume_ml_max: "NA"
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 100
  speed_rpm_max: 500
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: 71
  function_description: 实现萃取瓶内pH测定，往萃取瓶加酸/碱溶液调节pH的功能
  operation_target: 250ml萃取瓶，500ml萃取瓶，1200ml萃取瓶
  operation_workflow: "上料：一个不带盖的萃取瓶（250/500/1500ml)，内装有待调试样品溶液； / 初始PH测定：样品一直混匀着，将电极伸入待测样品至浸没探头，开始测定，待数据至稳定值读取数值； / 调测PH：往萃取瓶内加入强酸/强碱溶液，一边加入一边混匀，同时测定样品溶液实时的pH值，直至pH在目标值范围内； / 下料：取出电极，不带盖萃取瓶搬离； / 清洗电极：用纯化水冲洗电极； / 风吹干电极：风吹吹干电极；"
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: NA
  cycle_time_sec: 75
  consumables: 250ml萃取瓶/500ml萃取瓶/1200ml萃取瓶
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
  tags: [测PH值，调PH值]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.258
  power_kva_peak: 0.43
  voltage_v: 24
  noise_db: 65
  heat_generating: "true"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: true
  requires_drain: true
---

# 萃取瓶调PH模块

## Purpose (用途说明)
用于液液萃取前处理，适配 250/500/1200ml 广口瓶 / 试管。实现 pH 电极自动校准、清洗吹干、精准加酸碱调节液、混匀闭环调节 pH 值。适用于环境水样、食品提取液、化工样品等，实现自动化、高精度 pH 调节，减少人工误差与污染。

## Typical Scenarios (典型应用场景)
1、环境检测：水样萃取前自动调 pH 至酸性 / 中性，适配重金属、有机物提取；
2、食品检测：粮油 / 果蔬提取液调 pH，用于农残、真菌毒素液液萃取；
3、化工医药：中间体 / 反应液 pH 精准调节，保障萃取分离效率；
4、科研实验：多批次样品 pH 闭环调节，提升实验重复性。

## Working Principle (工作原理)
由 XYZ 三轴运动、pH 电极、2 路加液、清洗吹干及混匀组件组成。电极自动校准、清洗吹干后，三轴移动至样品位测 pH；按差值自动加酸 / 碱液，混匀后复测，循环闭环调节至目标 pH；完成后再次清洗吹干电极，待机。

## Limits & Cautions (限制与注意事项)
仅适配 250/500/1200ml 广口瓶 / 定制试管，单工位处理；加液量程 0–5ml、精度 ±1%，不适强腐蚀 / 高粘度 / 含固体样品。电极需定期校准维护，依赖稳定 DC24V 与洁净气源；安装水平，避免振动，防止漂移与交叉污染。

## Remarks (备注)

