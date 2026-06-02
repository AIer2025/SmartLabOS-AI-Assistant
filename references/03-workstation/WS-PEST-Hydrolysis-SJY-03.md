---
identification_version:
  id: WS-PEST-Hydrolysis-SJY-03
  name: 食检院-兽残水解平台
  version: 1.0
  status: active
  last_updated: 2026-4-10
  owner: 林雪平
  maturity: 验证中
hardware_config:
  platform_id: PLT-1200
  gripper: EPG40-50平行夹爪组件-包胶棒间距20
  pipette: 200UL移液枪
  scanner: 扫码枪
  transfer_platform: NA
  waste_basket: 废料框
  hmi: 工控机
  modules:
    - id: MOD-WX-006
      quantity: 50ml离心管加液涡旋模块
      role: 1
    - id: MOD-JZ-002
      quantity: 50ml离心管均质模块
      role: 1
    - id: MOD-YJ-002
      quantity: 50ml离心管加盐涡旋模块
      role: 1
    - id: MOD-PH-001
      quantity: 50ml离心管调PH模块
      role: 1
    - id: MOD-JY-012
      quantity: 正压加液
      role: 1
  module_layout: 3x3
  tray_layout: 4x2
electrical_environment:
  power_kva_nominal: 1.8
  power_kva_peak: 3
  voltage_v: 220V,24V
  requires_ventilation: false
  requires_compressed_air: true
  ambient_temp_c_min: 25
  ambient_temp_c_max: 25
workflows:
  - name: 工站实验流程1
    matches_standards:
      - GB 31658.26-2025《食品安全国家标准 动物性食品中 127 种药物残留量的筛查 液相色谱 - 高分辨质谱法》
    step_description: 加盐涡旋-静置10min
    consumables:
      - 50ML离心管，30ML盐罐
    workflow_steps:
      - step: 1
        name: 加盐涡旋
        module_id: MOD-YJ-002
        duration_min: 1
        parameters:
          无水硫酸钠: 6g，氯化钠=1.5g
        note: 涡旋时间=30S
      - step: 2
        name: 静置=600s
    total_cycle_time_min: 14
    parallel_capacity: 2
    batch_capacity: 18
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 66
    throughput_12h: 98
    target_analytes:
      - 127 种药物残留量
    target_samples:
      - 肉类
  - name: 工站实验流程2
    matches_standards:
      - GB 31658.26-2025《食品安全国家标准 动物性食品中 127 种药物残留量的筛查 液相色谱 - 高分辨质谱法》
    step_description: 加盐涡旋-静置10min
    consumables:
      - 50ML离心管，30ML盐罐
    workflow_steps:
      - step: 1
        name: 加盐涡旋
        module_id: MOD-YJ-002
        duration_min: 1
        parameters:
          无水硫酸钠: 4g，氯化钠=1g
        note: 涡旋时间=30S
      - step: 2
        name: 静置=600s
    total_cycle_time_min: 14
    parallel_capacity: 2
    batch_capacity: 18
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 66
    throughput_12h: 98
    target_analytes:
      - 127 种药物残留量
    target_samples:
      - 牛奶
  - name: 工站实验流程3
    matches_standards:
      - 1.GB/T 21311-2007《动物源性食品中硝基呋喃类药物代谢物残留量检测方法 高效液相色谱 / 串联质谱法》
    step_description: 加液-均质-加内标与衍生品涡旋
    consumables:
      - 50ML离心管，内标瓶
    workflow_steps:
      - step: 1
        name: 加液
        module_id: MOD-WX-006
        duration_min: 1
        parameters:
          0.2mol/L盐酸: 10.0ml
      - step: 2
        name: 均质
        module_id: MOD-JZ-002
        duration_min: 1
        parameters:
          速度: 10000Rpm，时间=60s
      - step: 3
        name: 加内标
        module_id: 移液枪
        duration_min: 1
        parameters:
          混标: 100ul
      - step: 4
        name: 衍生品涡旋
        module_id: MOD-WX-006
        duration_min: 1
        parameters:
          时间: 30S
    total_cycle_time_min: 10
    parallel_capacity: 2
    batch_capacity: 18
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 48
    throughput_12h: 72
    target_analytes:
      - 硝基呋喃
    target_samples:
      - 肌肉
      - 蛋奶
  - name: 工站实验流程4
    matches_standards:
      - 1.GB 31656.13-2021《食品安全国家标准 水产品中硝基呋喃类代谢物多残留的测定 液相色谱 - 串联质谱法》
    step_description: 加内标与衍生剂涡旋-加液涡旋
    consumables:
      - 50ML离心管，内标瓶，衍生剂瓶
    workflow_steps:
      - step: 1
        name: 加标
        module_id: 移液枪
        duration_min: 1
        parameters:
          混标: 50ul，硝基苯甲醛溶液=150ul
      - step: 2
        name: 涡旋
        module_id: MOD-WX-006
        duration_min: 1
        parameters:
          时间: 60s
      - step: 3
        name: 加液
        module_id: MOD-JY-012
        duration_min: 1
        parameters:
          0.5mol/L盐酸溶液: 5ml
      - step: 4
        name: 涡旋
        module_id: MOD-WX-006
        duration_min: 1
        parameters:
    total_cycle_time_min: 9
    parallel_capacity: 2
    batch_capacity: 18
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 106
    throughput_12h: 160
    target_analytes:
      - 硝基呋喃
    target_samples:
      - 鱼肉
  - name: 工站实验流程5
    matches_standards:
      - 2.GB/T 21311-2007《动物源性食品中硝基呋喃类药物代谢物残留量检测方法 高效液相色谱 / 串联质谱法》
    step_description: 加液涡旋-调PH
    consumables:
      - 50ML离心管，内标瓶，衍生剂瓶
    workflow_steps:
      - step: 1
        name: 加液涡旋
        module_id: MOD-WX-006
        duration_min: 1
        parameters:
          0.3mol/L磷酸钾: 1-2ML
          时间: 30s
      - step: 2
        name: 调PH
        module_id: MOD-PH-001
        duration_min: 1
        parameters:
          PH: 7.4±0.2，2.0mol/L氢氧化钠
    total_cycle_time_min: 10
    parallel_capacity: 2
    batch_capacity: 18
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 96
    throughput_12h: 144
    target_analytes:
      - 硝基呋喃
    target_samples:
      - 肌肉
      - 蛋奶
  - name: 工站实验流程6
    matches_standards:
      - 1.GB 31656.13-2021《食品安全国家标准 水产品中硝基呋喃类代谢物多残留的测定 液相色谱 - 串联质谱法》
    step_description: 调PH
    consumables:
      - 50ML离心管，内标瓶，衍生剂瓶
    workflow_steps:
      - step: 1
        name: 拆盖
        module_id: MOD-WX-006
        duration_min: 1
      - step: 2
        name: 调PH
        module_id: MOD-PH-001
        duration_min: 1
        parameters:
          PH: 7.0-7.5，1.0mol/L磷酸氢二钾溶液
      - step: 3
        name: 合盖
        module_id: MOD-WX-006
        duration_min: 1
    total_cycle_time_min: 9
    parallel_capacity: 2
    batch_capacity: 18
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 105
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
    sample_throughput: 66样/天
    feedback: 客户反馈良好
    contact_available: true
alternatives:
  []
upgrades:
  []
tags: [均质、加标，调pH]
---

# 食检院-兽残水解平台

## 方案定位
集成复杂样品基质的均质、加标、调 pH 等核心前处理功能，可一站式完成开关盖、液体试剂精准添加、固体试剂定量投加、涡旋高速混匀、均质强力提取、振摇温和萃取、多梯度精密加标、智能调 pH 等全流程实验操作。平台适配畜禽肉、水产、果蔬等不同基质样品的前处理需求，各模块协同联动，无需人工干预，能根据实验方法自由设定加标浓度、调 pH 梯度、振摇频率、均质时间等关键参数，精准把控反应条件，有效规避人工操作带来的误差，显著提升实验重复性与处理效率，满足高通量检测对标准化、自动化前处理的严苛要求。

## 适用客户画像
平台适配畜禽肉、水产、果蔬等不同基质样品的前处理需求

## 配置说明
水解（均质、加标和调pH)平台包括搬运模块、加液涡旋模块、均质模块、加内标模块、振摇模块、加盐涡旋模块、调pH模块和上下样模块

## 部署要求
占地 1.02㎡，220V电压，通风系统、排废系统
