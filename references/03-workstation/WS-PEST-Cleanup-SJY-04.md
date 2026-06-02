---
identification_version:
  id: WS-PEST-Cleanup-SJY-04
  name: 食检院-兽残提取净化平台
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
    step_description: 离心
    consumables:
      - 50ML离心管
    workflow_steps:
      - step: 1
        name: 离心
        module_id: MOD-LX-005
        duration_min: 1
        parameters:
          离心转速: 7000RPM
        note: 离心时间=300S
    total_cycle_time_min: 17
    parallel_capacity: 4
    batch_capacity: 16
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 112
    throughput_12h: 168
    target_analytes:
      - 127 种药物残留量
    target_samples:
      - 肉类
      - 牛奶
  - name: 工站实验流程2
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
  - name: 工站实验流程3
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
      - 蛋奶
  - name: 工站实验流程4
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
tags: [提取，离心净化]
---

# 食检院-兽残提取净化平台

## 方案定位
SPE 净化和过滤自动化平台整合各类复杂样品基质的固相萃取净化、洗脱液高效浓干、精准复溶定容及样品过滤转移至 2mL 进样小瓶等全流程实验操作，适配食品、农产品等多基质兽残检测需求。其中核心的 SPE 模块可实现全流程自动化智能控制，对活化、平衡、吹干、上样、淋洗、吹干、洗脱、吹干八大关键操作步骤进行精准流速调控，流速精度可达微升级别，避免人工操作导致的流速不均、柱干失效等问题。同时，平台支持根据不同检测标准或个性化实验方法，自由编辑、存储并调用多组操作程序，灵活调整各步骤的时间、流速与试剂用量，大幅提升净化效率与结果重复性，满足高通量检测的标准化、规范化要求。

## 适用客户画像
平台适配畜禽肉、水产、果蔬等不同基质样品的前处理需求

## 配置说明
SPE净化和过滤平台包括搬运模块、浓缩模块、过滤模块、拆盖模块和上下样模块。

## 部署要求
占地 1.02㎡，220V电压，通风系统、排废系统
