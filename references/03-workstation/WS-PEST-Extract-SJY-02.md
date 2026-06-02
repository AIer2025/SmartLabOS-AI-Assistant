---
identification_version:
  id: WS-PEST-Extract-SJY-02
  name: 食检院-兽残提取平台
  version: 1.0
  status: active
  last_updated: 2026-4-10
  owner: 林雪平
  maturity: 验证中
hardware_config:
  platform_id: PLT-1200
  gripper: EPG40-50平行夹爪组件-包胶棒间距16
  pipette: 5ml移液枪
  scanner: 扫码枪
  transfer_platform: NA
  waste_basket: 废料框
  hmi: 工控机
  modules:
    - id: MOD-WX-006
      quantity: 50ml离心管加液涡旋模块
      role: 1
    - id: MOD-ZD-001
      quantity: 50ml离心管垂直振荡模块
      role: 1
    - id: MOD-YY-004
      quantity: 50ml离心管液体全量转移模块
      role: 1
    - id: MOD-LX-005
      quantity: 50ml*4孔位离心模块
      role: 1
    - id: MOD-JY-006
      quantity: 注射泵加液系统-25ml
      role: 1
  module_layout: 3x3
  tray_layout: 4x2
electrical_environment:
  power_kva_nominal: 1.8
  power_kva_peak: 3
  voltage_v: 220V,24V
  requires_ventilation: true
  requires_compressed_air: true
  ambient_temp_c_min: 4
  ambient_temp_c_max: 25
workflows:
  - name: 工站实验流程1
    matches_standards:
      - GB 31658.26-2025《食品安全国家标准 动物性食品中 127 种药物残留量的筛查 液相色谱 - 高分辨质谱法》
    step_description: 加液涡旋-加液涡旋-振荡-离心-上清全量转移合并
    consumables:
      - 50ML离心管，5ml枪头
    workflow_steps:
      - step: 1
        name: 加液涡旋
        module_id: MOD-WX-006
        duration_min: 1
        parameters:
          缓冲液: 3.0ml
        note: 涡旋时间=10S
      - step: 2
        name: 加液涡旋
        module_id: MOD-WX-006
        duration_min: 1
        parameters:
          乙腈: 10.0ml
        note: 涡旋时间=60S
      - step: 3
        name: 振荡
        module_id: MOD-ZD-001
        duration_min: 1
        parameters:
          振荡时间: 600S
      - step: 4
        name: 离心
        module_id: MOD-LX-005
        duration_min: 1
        parameters:
          离心转速: 4000RPM
        note: 离心时间=300S
      - step: 5
        name: 全量转移
        module_id: MOD-YY-004
        duration_min: 1
      - step: 重复一次
    total_cycle_time_min: 81
    parallel_capacity: 4
    batch_capacity: 16
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 20
    throughput_12h: 32
    target_analytes:
      - 127 种药物残留量
    target_samples:
      - 肉类
  - name: 工站实验流程2
    matches_standards:
      - 1.GB 31658.26-2025《食品安全国家标准 动物性食品中 127 种药物残留量的筛查 液相色谱 - 高分辨质谱法》
    step_description: 加液涡旋-振荡-离心-上清全量转移合并
    consumables:
      - 50ML离心管
    workflow_steps:
      - step: 1
        name: 加液涡旋
        module_id: MOD-WX-006
        duration_min: 1
        parameters:
          乙腈: 20.0ml
        note: 涡旋时间=60S
      - step: 2
        name: 振荡
        module_id: MOD-ZD-001
        duration_min: 1
        parameters:
          振荡时间: 600S
      - step: 3
        name: 离心
        module_id: MOD-LX-005
        duration_min: 1
        parameters:
          离心转速: 4000RPM
        note: 离心时间=300S
      - step: 4
        name: 全量转移
        module_id: MOD-YY-004
        duration_min: 1
    total_cycle_time_min: 44
    parallel_capacity: 4
    batch_capacity: 16
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 44
    throughput_12h: 64
    target_analytes:
      - 127 种药物残留量
    target_samples:
      - 牛奶
  - name: 工站实验流程3
    matches_standards:
      - 1.GB/T 21311-2007《动物源性食品中硝基呋喃类药物代谢物残留量检测方法 高效液相色谱 / 串联质谱法》
    step_description: 加液涡旋-振荡-离心-弃去液体
    consumables:
      - 50ML离心管
    workflow_steps:
      - step: 1
        name: 加液
        module_id: MOD-WX-006
        duration_min: 1
        parameters:
          甲醇+水: 10.0ml
      - step: 2
        name: 振荡
        module_id: MOD-ZD-001
        duration_min: 1
        parameters:
          振荡时间: 600S
      - step: 3
        name: 离心
        module_id: MOD-LX-005
        duration_min: 1
        parameters:
          离心转速: 4000RPM
        note: 离心时间=300S
      - step: 4
        name: 弃去液体
        module_id: MOD-YY-004
        duration_min: 1
    total_cycle_time_min: 36
    parallel_capacity: 4
    batch_capacity: 16
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 42
    throughput_12h: 80
    target_analytes:
      - 硝基呋喃
    target_samples:
      - 肌肉
  - name: 工站实验流程4
    matches_standards:
      - 1.GB/T 21311-2007《动物源性食品中硝基呋喃类药物代谢物残留量检测方法 高效液相色谱 / 串联质谱法》
    step_description: 加液涡旋-振荡-离心-取上层收集合并，重复两次
    consumables:
      - 50ML离心管
    workflow_steps:
      - step: 1
        name: 加液
        module_id: MOD-WX-006
        duration_min: 1
        parameters:
          乙酸乙酯: 10-20ml
      - step: 2
        name: 振荡
        module_id: MOD-ZD-001
        duration_min: 1
        parameters:
          振荡时间: 600S
      - step: 3
        name: 离心
        module_id: MOD-LX-005
        duration_min: 1
        parameters:
          离心转速: 10000RPM
        note: 离心时间=300S
      - step: 4
        name: 全量转移
        module_id: MOD-YY-004
        duration_min: 1
    total_cycle_time_min: 79
    parallel_capacity: 4
    batch_capacity: 16
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 24
    throughput_12h: 36
    target_analytes:
      - 硝基呋喃
    target_samples:
      - 肌肉
  - name: 工站实验流程5
    matches_standards:
      - 1.GB 31656.13-2021《食品安全国家标准 水产品中硝基呋喃类代谢物多残留的测定 液相色谱 - 串联质谱法》
    step_description: 加液-涡旋-离心-取上层
    consumables:
      - 50ML离心管
    workflow_steps:
      - step: 1
        name: 加液
        module_id: MOD-WX-006
        duration_min: 1
        parameters:
          乙酸乙酯: 8ml
      - step: 2
        name: 涡旋
        module_id: MOD-WX-006
        duration_min: 1
        parameters:
          涡旋时间: 30S
      - step: 3
        name: 离心
        module_id: MOD-LX-005
        duration_min: 1
        parameters:
          离心转速: 6000RPM
        note: 离心时间=300S
      - step: 4
        name: 全量转移
        module_id: MOD-YY-004
        duration_min: 1
    total_cycle_time_min: 39
    parallel_capacity: 4
    batch_capacity: 16
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 48
    throughput_12h: 72
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
    sample_throughput: 44样/天
    feedback: 客户反馈良好
    contact_available: true
alternatives:
  []
upgrades:
  []
tags: [提取清洗平台]
---

# 食检院-兽残提取平台

## 方案定位
实现各类样品基质的自动化提取，一站式完成扫码、开关盖、移液、液体 / 固体试剂添加、涡旋混匀、振荡提取、离心分离、上清转移等操作；支持多标准提取操作整合，多样品并行处理，可 24 小时不间断运行，适配高通量、复杂前处理需求。

## 适用客户画像
平台适配畜禽肉、水产、果蔬等不同基质样品的前处理需求

## 配置说明
实现各类样品基质的自动化提取，一站式完成扫码、开关盖、移液、液体 / 固体试剂添加、涡旋混匀、振荡提取、离心分离、上清转移等操作；支持多标准提取操作整合，多样品并行处理，可 24 小时不间断运行，适配高通量、复杂前处理需求。


## 部署要求
占地 1.5㎡，220V电压，通风系统、排废系统
