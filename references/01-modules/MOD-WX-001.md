---
identification_version:
  id: MOD-WX-001
  module_part_no: HHM03H-002
  name: 2ml离心管拆装盖涡旋模块
  category: 涡旋
  version: 3.0
  status: active
  last_updated: 2026/1/30
  owner: 蔡勤超
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 15
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 2
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 1
  speed_rpm_max: 3000
  pressure_bar_min: 1.5
  pressure_bar_max: 3
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: 169
  function_description: 2ml离心管  加液/装盖/涡旋/拆盖：
  operation_target: 2ml离心管
  operation_workflow: 载台移至上下料位 - 放入离心管 - 载台移至拆合盖位 - 拆盖（保持夹住瓶盖）- 载台移至加液位 - 加液（纯水）- 载台移至拆合盖位 - 合盖 - 载台移至涡旋位 - 涡旋（3000rpm，100s）- 载台移至上下料位 - 取下离心管；更换离心管，重复测试
  absolute_accuracy_pct: ±1
  repeat_accuracy_pct: ±0.5
  cycle_time_sec: 295
  consumables: 2ml 离心管
  sub_functions: [165-拆盖, 166-装盖]
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
  wear_parts: 涡旋底部胶皮
tags_meta:
  tags: [拆盖，装盖，涡旋]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 380
  power_kva_peak: 230
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: true
---

# 2ml离心管拆装盖涡旋模块

## Purpose (用途说明)
利用 2ml 离心管拆装盖涡旋模块完成试管拆盖作业，再按规范作业流程完成封盖复位装盖

## Typical Scenarios (典型应用场景)
血样

## Working Principle (工作原理)
该模块搭配涡旋主机使用，依靠偏心圆周旋转动力，配合顶部弹性压紧限位结构，实现拆盖、涡旋混匀、复位装盖一体化作业。工作时将 2ml 离心管整齐定位放置，压紧机构抵住管盖使其保持静止，设备带动管身做圆周偏心转动，利用管身与管盖之间形成的相对扭转力，完成自动松盖拆盖；松开压紧限位后，管盖与管体同步跟随模块做高频涡旋运动，管内液体形成漩涡流动，快速完成样品混匀；混匀结束后再次压紧固定管盖，调整运转方向与扭力，使管身反向旋转，匀速将管盖旋紧密封，整套流程借助同一组涡旋驱动结构，通过压紧限位的切换，依次完成拆盖、混匀、装盖全流程作业。

## Limits & Cautions (限制与注意事项)
涡旋不着超过3000，一般在2000-2500

## Remarks (备注)

