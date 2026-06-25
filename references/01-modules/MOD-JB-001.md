---
identification_version:
  id: MOD-JB-001
  module_part_no: HHM23L-250
  name: 250m1锥形瓶恒温磁力搅拌模块
  category: 搅拌
  version: 1.0
  status: active
  last_updated: 2025/7/18
  owner: 唐宋
physical_specs:
  length_mm: 450
  width_mm: 250
  height_mm: 600
  weight_kg: 25
  module_slots: 1
  tray_slots: "四工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 1
  volume_ml_max: 250
  temperature_c_min: 25
  temperature_c_max: 80
  speed_rpm_min: 1
  speed_rpm_max: 2000
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: NA
process_capability_1:
  function_code: 590
  function_description: 搅拌启动
  operation_target: 250ml锥形瓶
  operation_workflow: 初始化加热至设定温度-载台移至上下料位 - 机械手放入一组锥形瓶 - 移动至搅拌位 - 搅拌电机工作开始搅拌 -到达设定时间- 移动至上下料位 - 机械手放入第二组锥形瓶-移动至搅拌位 - 搅拌电机工作开始搅拌 -到达设定时间- 移动至上下料位 -依次取下锥形瓶
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: ""
  consumables: "250ml锥形瓶 / "
  sub_functions: [591-搅拌中途上料
592-预热开启
593-预热关闭]
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
  tags: [恒温，磁力搅拌]
documentation:
  urs_url: URS连接
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.5
  power_kva_peak: 0.8
  voltage_v: 48
  noise_db: 65
  heat_generating: "true"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 250m1锥形瓶恒温磁力搅拌模块

## Purpose (用途说明)
这是一套专为250ml锥形瓶设计的"恒温加热 + 磁力搅拌"集成模块，核心解决的是中大量（100~250ml）液体反应的"精确控温 + 均匀混合"问题，是有机合成、生物制药、分析检测领域最常用的基础反应装备之一。

## Typical Scenarios (典型应用场景)
250ml锥形瓶恒温磁力搅拌模块的典型案例 = 教学（蓝瓶子）+ 生物（酶催化37℃±0.3℃）+ 材料（环氧树脂300cP不跳磁）+ 有机合成（异丙醇20min提纯/己二酸45℃控温）+ 化工中试（50℃收率+15%）+ 分析检测（RSD≤0.3%），核心解决的是手工做不到的"精度（±0.5℃）+ 均匀度（CV≤3%）+ 平行对比（6工位）+ 高粘度不跳磁"四大难题

## Working Principle (工作原理)
250ml锥形瓶恒温磁力搅拌模块的工作原理 = 直流无刷电机产生旋转磁场 → PTFE钕铁硼搅拌子无接触同步旋转（100~1500rpm，CV≤3%）+PID算法动态调节功率 → 维持±0.5℃恒温，两大系统通过"搅拌消除温差→测温准确→控温精准"形成正向闭环

## Limits & Cautions (限制与注意事项)
250ml锥形瓶恒温磁力搅拌模块的限制 = 液量50~167ml（2/3以内）+ 温度≤80℃ + 转速100~1500rpm + 升温≤5℃/min + 不搅拌不加热 + 70℃以上≤2小时；注意事项的核心 = 防炸瓶（温差控制+不空烧）、防跳磁（低速启动+匹配搅拌子）、防触电（三孔接地）、记住：这个模块最大的三个杀手是 ① 不搅拌就加热 → 炸瓶 ② 高速直接启动 → 跳磁打瓶 ③ 液量超过2/3 → 沸腾溢出

## Remarks (备注)

