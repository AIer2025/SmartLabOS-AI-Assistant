---
identification_version:
  id: WS-PEST-Concentrate-SJY-06
  name: 食检院-兽残浓缩复溶平台
  version: 1.0
  status: active
  last_updated: 2026-4-10
  owner: 林雪平
  maturity: 验证中
hardware_config:
  platform_id: PLT-1200
  gripper: EPG40-50平行夹爪组件-包胶棒间距16
  pipette: 1ml移液枪
  scanner: 扫码枪
  transfer_platform: NA
  waste_basket: 废料框
  hmi: 工控机
  modules:
    - id: MOD-NS-005
      quantity: 15ml西林瓶浓缩模块
      role: 2
    - id: MOD-NS-002
      quantity: 50ml西林瓶浓缩模块
      role: 2
    - id: MOD-CS-002
      quantity: 15ml西林瓶超声模块
      role: 2
    - id: MOD-JY-012
      quantity: 正压加液
      role: 1
  module_layout: 3x3
  tray_layout: 4x2
electrical_environment:
  power_kva_nominal: 1.8
  power_kva_peak: 3
  voltage_v: 220V,48V,24V
  requires_ventilation: true
  requires_compressed_air: true
  ambient_temp_c_min: 25
  ambient_temp_c_max: 25
workflows:
  - name: 工站实验流程1
    matches_standards:
      - GB 31658.26-2025《食品安全国家标准 动物性食品中 127 种药物残留量的筛查 液相色谱 - 高分辨质谱法》
    step_description: 浓缩-复溶-超声
    consumables:
      - 15ml西林瓶，1ml枪头，50ml离心管
    workflow_steps:
      - step: 1
        name: 移液
        module_id: 移液枪
        duration_min: 1
        parameters:
      - step: 2
        name: 浓缩
        module_id: MOD-NS-005
        duration_min: 2
        parameters:
          WD: 40°，浓干
      - step: 3
        name: 复溶
        module_id: MOD-NS-005
        duration_min: 2
        parameters:
          甲醇: 200ul，涡旋=10S
      - step: 4
        name: 复溶
        module_id: MOD-NS-005
        duration_min: 2
        parameters:
          水: 800ul，涡旋=30S
      - step: 5
        name: 超声
        module_id: MOD-CS-002
        duration_min: 1
        parameters:
          时间: 60s
    total_cycle_time_min: 33
    parallel_capacity: 4
    batch_capacity: 16
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 42
    throughput_12h: 84
    target_analytes:
      - 127 种药物残留量
    target_samples:
      - 肉类
      - 牛奶
  - name: 工站实验流程2
    matches_standards:
      - 1.GB/T 21311-2007《动物源性食品中硝基呋喃类药物代谢物残留量检测方法 高效液相色谱 / 串联质谱法》
    step_description: 浓缩干-复溶-加液涡旋
    consumables:
      - 50ml西林瓶，1ml枪头，50ml离心管
    workflow_steps:
      - step: 1
        name: 浓缩
        module_id: MOD-NS-002
        duration_min: 2
        parameters:
          WD: 40°，浓干
      - step: 2
        name: 复溶
        module_id: MOD-NS-002
        duration_min: 2
        parameters:
          0.1%的加酸水: 1ml，涡旋=10S
      - step: 3
        name: 加液
        module_id: MOD-JY-012
        duration_min: 1
        parameters:
          乙腈饱和正己烷溶液: 3ml
      - step: 4
        name: 涡旋
        module_id: MOD-NS-002
        duration_min: 2
        parameters:
          涡旋: 30S
    total_cycle_time_min: 31
    parallel_capacity: 4
    batch_capacity: 16
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 60
    throughput_12h: 92
    target_analytes:
      - 硝基呋喃
    target_samples:
      - 肌肉
      - 蛋奶
  - name: 工站实验流程3
    matches_standards:
      - 1.GB 31656.13-2021《食品安全国家标准 水产品中硝基呋喃类代谢物多残留的测定 液相色谱 - 串联质谱法》
    step_description: 浓缩干-复溶
    consumables:
      - 50ml西林瓶，1ml枪头，50ml离心管
    workflow_steps:
      - step: 1
        name: 浓缩
        module_id: MOD-NS-002
        duration_min: 2
        parameters:
          WD: 40°，浓干
      - step: 2
        name: 加液
        module_id: MOD-JY-012
        duration_min: 1
        parameters:
          5%甲醇溶液: 1ml
      - step: 3
        name: 涡旋
        module_id: MOD-NS-002
        duration_min: 2
        parameters:
          涡旋: 10S
    total_cycle_time_min: 18
    parallel_capacity: 4
    batch_capacity: 16
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 104
    throughput_12h: 160
    target_analytes:
      - 硝基呋喃
    target_samples:
      - 鱼肉
commercial:
  price_rmb:
    hardware: null
    installation: null
    training: null
    first_year_service: null
    total: null
  lead_time_weeks: 12
  warranty_months: 12
deployments:
  - customer: 成都食检院
    location: 成都
    date: 2026-05-25
    sample_throughput: 60样/天
    feedback: 客户反馈良好
    contact_available: true
alternatives:
  []
upgrades:
  []
tags: [浓缩、复溶平台]
---

# 食检院-兽残浓缩复溶平台

## 方案定位
萃取净化自动化平台深度整合各类复杂样品基质的液液萃取净化、洗脱液高效浓干、精准复溶定容等前处理全流程实验操作，可一站式完成从样品提取到上机检测前的标准化处理。平台针对大体积样品处理需求进行专项优化，不仅能实现大体积提取液的自动化萃取净化，有效分离目标物与基质杂质，还可对大体积萃取液开展高效浓缩，通过智能控温、精准控压技术规避目标物挥发损失，同时配套自动复溶与定容模块，保障定容体积精度符合检测标准。该平台适配畜禽肉、水产、果蔬等多类型基质样品，大幅减少人工干预，提升实验重复性与通量，满足高通量检测对标准化、自动化前处理的严苛要求。

## 适用客户画像
平台适配畜禽肉、水产、果蔬等不同基质样品的前处理需求

## 配置说明
萃取净化平台包括搬运模块、加液涡旋模块、浓缩模块和上下样模块。


## 部署要求
占地 1.02㎡，220V电压，通风系统、排废系统
