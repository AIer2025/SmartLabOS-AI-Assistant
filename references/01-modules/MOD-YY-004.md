---
identification_version:
  id: MOD-YY-004
  module_part_no: HHM09F-050
  name: 50ml全量液体转移模块
  category: 移液
  version: 1.0
  status: active
  last_updated: 2026/1/30
  owner: 宾俊
physical_specs:
  length_mm: 450
  width_mm: 180
  height_mm: 600
  weight_kg: 20
  module_slots: 1
  tray_slots: "双工位"
  mount_type: 标准模块位
module_performance:
  volume_ml_min: 0
  volume_ml_max: 50
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
  function_code: "155 / "
  function_description: " 50ml离心管 液体转移："
  operation_target: 50ml离心管
  operation_workflow: 载台移至上下料位 - 放入样品离心管（有盖）- 放入空离心管（无盖）- 载台移至拆合盖位 - 气缸推出夹紧离心管 - 钧舵夹爪下降至拆合盖位置 - 夹紧瓶盖 - 均舵夹爪旋转并向上运动打开瓶盖 - 钧舵夹爪上移至拆盖等待位（夹爪保持夹住瓶盖）- 载台移至旋转倒液位 - 旋转倒液（旋转速度）- 接液离心管随倒液离心管动作下降（下降速度）- 倒液离心管旋转到95°停顿1s - 倒液离心管回正（旋转速度） - 接液离心管回升 - 载台移至拆合盖位 - 均舵夹爪下降至离心管拆合盖位置 - 钧舵夹爪旋转并向下运动盖上瓶盖 - 均舵夹爪松开瓶盖并上升至等待位 - 载台移至上下料位 - 取下离心管
  absolute_accuracy_pct: NA
  repeat_accuracy_pct: NA
  cycle_time_sec: 40S
  consumables: 离心管-50ml
  sub_functions: [160 拆盖, 倒液, 装盖]
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
  wear_parts: 手指胶皮
tags_meta:
  tags: [拆盖，全量转移]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.312
  power_kva_peak: 0.52
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: false
  requires_compressed_air: true
  requires_water: false
  requires_drain: false
---

# 50ml全量液体转移模块

## Purpose (用途说明)
全量液体转移模块是适配50ml 尖底离心管的自动化液体处理单元，核心用于离心后样本的全量液体精准转移，集成上下料、开合盖、旋转倒液、接液等一体化功能，实现样本处理全流程自动化，避免人工操作误差与污染，适配实验室自动化样本处理场景

## Typical Scenarios (典型应用场景)
1、临床生化 / 免疫检测样本前处理
2、核酸检测样本处理（PCR 前处理
3、生物制药 / 细胞培养上清液转移
4、食品 / 环境检测样品前处理

## Working Principle (工作原理)
该模块通过三轴联动 + 旋转倒液 + 气动夹紧 + 电动夹爪组合，实现 50ml 尖底离心管从上下料、自动开 / 关盖、旋转倒液、精准接液到下料的全自动流程，核心是机械定位 + 精准旋转 + 动态随动接液 + 闭环检测

## Limits & Cautions (限制与注意事项)
1、严禁手动干预运行中设备：运行时开盖、伸手、拨管易造成夹伤、撞机、样品污染。
2、定期校准：各轴零点、夹爪行程、旋转角度、传感器位置需定期校准，防止定位不准、拆盖失败、漏液。
3、清洁要求：接触样品区域每次运行后清洁，避免交叉污染、残留结晶影响夹紧与旋转。
4、异常处理：拆盖失败、掉管、漏液、电机报错时，先断电 ，停气再处理，禁止带电排查。
5、负载限制：夹爪、气缸、电机不可超额定负载，长期重载会导致打滑、变形、寿命缩短。

## Remarks (备注)

