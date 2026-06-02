---
identification_version:
  id: MOD-YJ-002
  module_part_no: HHM03B-050-V3.0
  name: 50ml离心管加盐涡旋模块（无触摸屏版）
  category: 加盐
  version: 3.0
  status: active
  last_updated: 2026/2/27
  owner: 蔡勤超
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 21.4
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 0
  volume_ml_max: 50
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 1000
  speed_rpm_max: 3000
  pressure_bar_min: 1
  pressure_bar_max: 6
  vacuum_kpa: NA
peripherals:
  required_peripherals: 空压机
process_capability_1:
  function_code: 223
  function_description: 拆盖加盐装盖涡旋
  operation_target: 50ml离心管,8g盐管
  operation_workflow: 离心管放入载台 - 载台移至拆合盖位 - 夹紧离心管 - 拆盖（保持夹住瓶盖）- 松开离心管 - 载台移至加盐位/加液位 - 加盐/加液 - 载台移至涡旋位 - 涡旋压板下压 - 涡旋 - 停止 - 涡旋压板升起 - 载台移至拆盖位 - 夹紧离心管 - 合盖 - 松开离心管 - 载台移至上下料位 - 取下离心管
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 97
  consumables: "尖底离心管-50ml / 盐管"
  sub_functions: [220-拆盖，221-装盖，222-拆盖加液装盖涡旋，223-拆盖加盐装盖涡旋，224-加液，225-加盐，227-装盖涡旋]
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
    - 正压加液系统
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: 涡旋底部胶皮，涡旋限位胶皮，拆盖爪指
tags_meta:
  tags: [加液，加盐，涡旋]
documentation:
  urs_url: URS连接
  urd_url: URD连接
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.132
  power_kva_peak: 0.22
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 50ml离心管加盐涡旋模块（无触摸屏版）

## Purpose (用途说明)
实现向50ml尖底试管定量添加固体粉末/液体，并通过涡旋的方式使样品液充分混合/溶解

## Typical Scenarios (典型应用场景)
1、核酸检测样本前处理（裂解盐添加
2、生化 / 免疫检测试剂配制
3、环境水样重金属检测前处理
4、食品样品添加剂 / 盐分检测前处理
5、生物制药缓冲液 / 培养基配制

## Working Principle (工作原理)
加盐涡旋模块依靠三轴联动定位 + 旋转夹紧 + 粉末重力下料 + 高速涡旋混匀 + 传感器闭环检测，实现 50ml 尖底离心管的自动开盖→加固体粉末→关盖→涡旋混匀全流程自动化，核心是机械精确定位、重力定量下料、机械摩擦夹紧、高速振动混匀、异常闭环检测

## Limits & Cautions (限制与注意事项)
1、容器限制：仅适配 50ml 尖底离心管 与专用粉末盐管，不支持其他规格试管、圆底管、异形管。
2、粉末特性限制：仅适用于流动性好、干燥、无吸湿性、无结块的细粉末；潮湿、易吸潮、易结块、带静电、颗粒过大 / 过细粉末不可用，易堵塞、下料不均或残留。
3、液体限制：试管内液体为水相、缓冲液、澄清样品液；高粘度、易起泡、强腐蚀、高固含液体不适用，影响混匀效果或损坏部件。
4、加盐方式限制：采用重力落料，无精准计量，靠盐管容积定量；无法用于微量、高精度称量场景。
5、涡旋限制：最大 3000rpm、100s，不可长时间超高速运行；超重 / 满管易振动移位、漏液。

## Remarks (备注)

