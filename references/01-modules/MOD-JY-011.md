---
identification_version:
  id: MOD-JY-011
  module_part_no: HHM10F-901
  name: 25-200ml容量瓶拆盖加液模块
  category: 加液
  version: 2.0
  status: active
  last_updated: 2024/12/23
  owner: 何向东
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 25
  volume_ml_max: 200
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
  function_code: 523
  function_description: 拆盖-加液-装盖
  operation_target: 25ml容量瓶，50ml容量瓶，100ml容量瓶，200ml容量瓶
  operation_workflow: 夹紧组件移至上下料位 - 放入容量瓶（同规格2个）- 夹紧 - 夹紧组件移至拔瓶塞位 -  钧舵夹爪下降至拔瓶塞位 - 夹紧瓶赛并旋转向上拔出瓶塞 - 夹爪保持夹住瓶塞 - 夹紧组件移至加液位 - 加液管下降至刻度线以下 - 加液（加液至瓶颈）- 夹紧组件移至拔瓶塞位 - 钧舵夹爪旋转并向下运动塞入瓶塞 - 夹紧组件移至上下料位 - 夹爪打开 - 取下容量瓶
  absolute_accuracy_pct: ±1%
  repeat_accuracy_pct: ±0.5%
  cycle_time_sec: 120
  consumables: "容量瓶-25ml / 容量瓶-50ml / 容量瓶-100ml / 容量瓶-200ml"
  sub_functions: [520 拆盖，521 加液，522 装盖]
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
  tags: [容量瓶，拧盖，合盖，加液]
documentation:
  urs_url: URS连接
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.252
  power_kva_peak: 0.42
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 25-200ml容量瓶拆盖加液模块

## Purpose (用途说明)
本模块实现：25-200ml容量瓶的上下料、拔瓶塞、加液、盖瓶塞等功能。

## Typical Scenarios (典型应用场景)
1. 第三方检测机构样本前处理工作站
2. 制药企业标准溶液配制线
3. 疾控 / 医院检验中心自动化配液系统
4. 高校 / 科研院所高通量化学实验平台
5. 体外诊断（IVD）试剂生产配液线
6. 计量 / 质检院所容量分析自动化系统

## Working Principle (工作原理)
本模块通过Y 轴工位移动 + Z 轴拔塞 / 盖塞 + 加液 Z 轴定深加液 + 气缸夹紧定位，全自动完成容量瓶上料→拔塞→加液→盖塞→下料全流程

## Limits & Cautions (限制与注意事项)
运动区域禁止伸手，防止夹伤、撞机！

## Remarks (备注)

