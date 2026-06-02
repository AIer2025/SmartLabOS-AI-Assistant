---
identification_version:
  id: WS-PEST-Centrifuge-SJY-07
  name: 食检院-兽残离心净化平台
  version: 1.0
  status: active
  last_updated: 2026-4-10
  owner: 林雪平
  maturity: 验证中
hardware_config:
  platform_id: PLT-1200
  gripper: ERG32-150T旋转夹爪组件加长款-包胶棒间距16
  pipette: 1ml移液枪
  scanner: 扫码枪
  transfer_platform: NA
  waste_basket: 废料框
  hmi: 工控机
  modules:
    - id: MOD-XC-001
      quantity: 2ml离心管/西林瓶拆盖吸磁模块
      role: 2
    - id: MOD-GL-002
      quantity: 5cc 针式过滤模块V1.0
      role: 2
    - id: MOD-LX-006
      quantity: 2ml*6孔位高速离心机
      role: 2
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
    step_description: 过滤
    consumables:
      - 50ml西林瓶，1ml枪头，2ml西林瓶，5cc滤筒
    workflow_steps:
      - step: 1
        name: 过滤
        module_id: MOD-GL-002
        duration_min: 1
        parameters:
    total_cycle_time_min: 6
    parallel_capacity: 2
    batch_capacity: 60
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 146
    throughput_12h: 220
    target_analytes:
      - 127 种药物残留量
    target_samples:
      - 肉类
      - 牛奶
  - name: 工站实验流程2
    matches_standards:
      - 1.GB/T 21311-2007《动物源性食品中硝基呋喃类药物代谢物残留量检测方法 高效液相色谱 / 串联质谱法》
    step_description: 过滤
    consumables:
      - 50ml西林瓶，1ml枪头，2ml西林瓶，5cc滤筒
    workflow_steps:
      - step: 1
        name: 过滤
        module_id: MOD-GL-002
        duration_min: 1
        parameters:
    total_cycle_time_min: 6
    parallel_capacity: 2
    batch_capacity: 20
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 146
    throughput_12h: 220
    target_analytes:
      - 硝基呋喃
    target_samples:
      - 肌肉
      - 蛋奶
  - name: 工站实验流程3
    matches_standards:
      - 1.GB 31656.13-2021《食品安全国家标准 水产品中硝基呋喃类代谢物多残留的测定 液相色谱 - 串联质谱法》
    step_description: 取上层转移-离心-上清过滤
    consumables:
      - 50ml西林瓶，1ml枪头，2ml西林瓶，5cc滤筒
    workflow_steps:
      - step: 1
        name: 移液
        module_id: 移液枪
        duration_min: 1
        parameters:
      - step: 2
        name: 离心
        module_id: MOD-LX-006
        duration_min: 1
        parameters:
          时间: 10min
      - step: 3
        name: 过滤
        module_id: MOD-NS-002
        duration_min: 1
        parameters:
    total_cycle_time_min: 34
    parallel_capacity: 4
    batch_capacity: 20
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 64
    throughput_12h: 84
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
tags: [离心，净化平台]
---

# 食检院-兽残离心净化平台

## 方案定位
实现各类样品基质的离心净化，一站式完成扫码、开关盖、离心分离、筒式过滤等操作；支持多标准提取操作整合，多样品并行处理，可 24 小时不间断运行，适配高通量、复杂前处理需求。

## 适用客户画像
平台适配畜禽肉、水产、果蔬等不同基质样品的前处理需求

## 配置说明
2ml离心管拆盖模块，2ml高速离心机，5cc筒式过滤模块

## 部署要求
占地 1.02㎡，220V电压，通风系统、排废系统
