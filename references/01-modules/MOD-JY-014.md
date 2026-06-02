---
identification_version:
  id: MOD-JY-014
  module_part_no: HHM10E-100
  name: 消解管拆盖加液模块(100ML)
  category: 加液
  version: 1.0
  status: active
  last_updated: 2025/3/28
  owner: 汪健
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
  volume_ml_max: 100
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
  function_code: 145
  function_description: 拆盖加液装盖
  operation_target: 100ml消解管
  operation_workflow: 载台移至上下料位 - 放入消解管 - 载台移至拆盖位 - 夹紧消解管 - 气缸夹爪下降至消解管塞处 - 夹爪夹住管塞 - 提起 - 载台移至上下料位 - 取走消解管操作 - 消解管返回 - 载台移至拆盖位 - 气缸夹爪下降 - 管塞塞进消解管 - 气缸夹爪上升 - 载台移至上下料位 - 取下消解管
  absolute_accuracy_pct: ±5%
  repeat_accuracy_pct: ""
  cycle_time_sec: 60
  consumables: 消解管-100ml
  sub_functions: [146-拆盖加液, 147-加液, 148-拆盖, 149-装盖]
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
    - 微波消解
    - 石墨赶酸
delivery_maintenance:
  lead_time_weeks: 8
  maintenance_cycle_months: 6
  wear_parts: 无
tags_meta:
  tags: [加液, 拆装盖, 100ml消解管，喷淋式加液]
documentation:
  urs_url: ""
  urd_url: ""
  plc_source_url: PLC源程序连接
price:
  cost_internal_rmb: null
  sales_floor_rmb: ""
  quote_rmb: ""
electrical_environment:
  power_kva_nominal: 0.344
  power_kva_peak: 0.574
  voltage_v: 24
  noise_db: 65
  heat_generating: "false"
  requires_ventilation: true
  requires_compressed_air: true
  requires_water: false
  requires_drain: true
---

# 消解管拆盖加液模块(100ML)

## Purpose (用途说明)
100ml消解管拆盖加酸装盖，可喷淋式加液

## Typical Scenarios (典型应用场景)
1、水质 COD 检测100ml 消解管自动拆盖 + 喷淋加强酸前处理
2、总磷、总氮、总重金属检测5 种消解液自动定量添加

## Working Principle (工作原理)
本模块通过Y 轴与 Z 轴联动定位，配合气缸夹爪完成消解管自动拆盖；采用喷淋式加液结构与多路电磁阀控制，实现5 种强酸 / 易挥发消解液的定量加注（精度 ±5%）；加液过程中密封压紧 + 外部抽雾，防止酸雾泄漏与腐蚀；反应完成后自动复位出料，停机前可自动冲洗管路并收集废液，全程自动化完成拆盖、密封加液、抽雾、冲洗等一体化作业。

## Limits & Cautions (限制与注意事项)
1、加液前必须启动外部抽风系统，并确保喷淋模组压紧密封管口，防止酸雾泄漏。
2、拆盖与加液前，必须将消解管完全夹紧，避免管体转动、倾斜或漏液。
3、操作强酸、易挥发消解液时，严禁打开防护罩，严禁将手伸入设备运动区域。
4、长时间停机前必须执行管路自动冲洗，并将废液排入收集盒，防止结晶与腐蚀。
5、开机必须完成轴回零、气缸复位等初始化流程，运行中禁止随意修改参数或手动触发阀件。

## Remarks (备注)

