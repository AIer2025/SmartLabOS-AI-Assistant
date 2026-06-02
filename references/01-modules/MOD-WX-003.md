---
identification_version:
  id: MOD-WX-003
  module_part_no: HHM03H-015
  name: 15ml离心管拆盖正压加液涡旋模块
  category: 涡旋
  version: 2.0
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
  volume_ml_max: 15
  temperature_c_min: "NA"
  temperature_c_max: "NA"
  speed_rpm_min: 1
  speed_rpm_max: 3000
  pressure_bar_min: 1
  pressure_bar_max: 4
  vacuum_kpa: NA
peripherals:
  required_peripherals: 空压机
process_capability_1:
  function_code: 171
  function_description: 离心管装盖涡旋混匀拆盖
  operation_target: 15ml离心管
  operation_workflow: "1.拆盖：载台移至上下料位 - 放入离心管 - 载台移至拆合盖位 - 夹紧离心管 - 钧舵夹爪向下移动到达离心管位置 - 夹紧瓶盖 - 旋转并向上打开瓶盖（保持夹住瓶盖）； / 2.加液：松开离心管 - 载台移至加液位 - 加液 / 3.关盖：载台移至拆合盖位 - 夹紧离心管 - 钧舵夹爪旋转并向下运动盖上瓶盖 - 松开离心管 / 4.涡旋"
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 300
  consumables: 离心管-50ml
  sub_functions: [165-拆盖, 166-装盖, 167-去加液位，168-去待机位，169-拆盖/去加液，170-装盖/涡旋]
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
    - 正压加液系统
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: 工位硅胶条，涡旋下压胶皮，涡旋胶皮
tags_meta:
  tags: [正压加液、加液涡旋、涡旋]
documentation:
  urs_url: URS连接
  urd_url: URD连接
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

# 15ml离心管拆盖正压加液涡旋模块

## Purpose (用途说明)
用于实验室样本前处理，实现试剂添加与样本混匀全流程自动化，减少人工干预、避免污染、提升处理一致性与效率

## Typical Scenarios (典型应用场景)
1、核酸检测前处理（PCR 样本制备）
2、生化 / 免疫检测试剂配制与样本混匀
3、生物制药细胞上清 / 发酵液处理
4、微生物 / 环境检测样品前处理

## Working Principle (工作原理)
利用电机驱动离心管水平圆周偏心运动，使管内液体受离心力作用沿管壁形成螺旋涡流，产生层流剪切力 + 湍流扰动，打破颗粒团聚、加速试剂溶解均匀

## Limits & Cautions (限制与注意事项)
涡旋速度最高3000

## Remarks (备注)

