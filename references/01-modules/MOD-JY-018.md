---
identification_version:
  id: MOD-JY-018
  module_part_no: HHM10E-055
  name: 35ml消解管拔塞加酸模块
  category: 加液
  version: 1.0
  status: active
  last_updated: 2026/4/30
  owner: 唐宋
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
  required_peripherals: ""
process_capability_1:
  function_code: 145
  function_description: 启动拆盖加液装盖
  operation_target: 35ml消解管
  operation_workflow: 载台移至放料位-放置消解管至载台-载台移至加装拔盖位-拔盖-载台移至加酸位-载台旋转并加酸-加酸完成载台停止旋转-载台移至加装拔盖位-装盖-载台移至下料位-取下消解管；
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 100
  consumables: 35ml消解管
  sub_functions: [146-启动拆盖加液，147-启动加液，148-启动拆盖，149-启动装盖，151-启动拔盖加酸]
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
  wear_parts: ""
tags_meta:
  tags: [消解管，拆盖，加酸]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.24
  power_kva_peak: 0.4
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 35ml消解管拔塞加酸模块

## Purpose (用途说明)


## Typical Scenarios (典型应用场景)
场景 1：环境水质检测实验室（地表水 / 污水理化检测）
案例：环保监测站废水重金属消解前处理，消解管自动拔盖、定量加硝酸 / 混酸，批量预处理水样，配套微波消解仪做重金属（铅、镉、铬）检测，替代人工开盖加酸，规避强酸腐蚀风险。
场景 2：土壤理化分析项目
案例：农科院土壤肥力与重金属检测，土壤样品装入消解试管，设备自动拔盖、精准加注消解酸液，消解后测定土壤有机质、重金属含量，大批量土样自动化前处理。
场景 3：矿产矿石成分化验
案例：矿冶化验实验室矿石元素分析，矿石粉末置于消解管，模块完成拔盖 + 定量加酸消解预处理，后续原子吸收光谱测铜铁锌等矿物组分。

## Working Principle (工作原理)
设备经 Y、Z 轴电机复位初始化，机械手送料至上料位；Y 轴转运消解管至拔盖位，气缸配合 Z 轴完成拔盖；再移至加酸位，滑台气缸下探加液管，试管旋转后电磁阀定量加酸，最后 Y 轴送至下料位。

## Limits & Cautions (限制与注意事项)
1、加酸为强酸介质，管路阀件耐腐需达标，严防漏液腐蚀元器件。
2、消解管转速 60~300r/min，不可超限调速，避免样品飞溅。
3、各气缸磁性开关需稳固，感应异常会造成夹管、拔盖失效

## Remarks (备注)

