---
identification_version:
  id: SOL-PEST-CDSJY-01
  name: 成都食检院-127种药物残留-肉类
  version: 1.0
  status: active
  last_updated: 2026-05-25
  owner: 林雪平
  maturity: 已验证
  proposal_type: A
business_context:
  target_industries:
    - 食品安全检测
  target_samples:
    - 肉类
  target_analytes:
    - 127种药物残留
  matches_standards:
    - GB 31658.26-2025《食品安全国家标准 动物性食品中 127 种药物残留量的筛查 液相色谱 - 高分辨质谱法》
workstations:
  - ws_id: WS-PEST-Extract-SJY-01
    sequence: 1
    role: 提取
    quantity: 1
  - ws_id: WS-PEST-Extract-SJY-02
    sequence: 2
    role: 提取
    quantity: 1
  - ws_id: WS-PEST-Hydrolysis-SJY-03
    sequence: 3
    role: 水解
    quantity: 1
  - ws_id: WS-PEST-Cleanup-SJY-04
    sequence: 4
    role: 提取净化
    quantity: 1
  - ws_id: WS-PEST-SPE-SJY-05
    sequence: 5
    role: SPE净化
    quantity: 1
  - ws_id: WS-PEST-Concentrate-SJY-06
    sequence: 6
    role: 提取净化
    quantity: 1
  - ws_id: WS-PEST-Centrifuge-SJY-07
    sequence: 7
    role: 离心净化
    quantity: 1
throughput_target:
  samples_per_day: 48
  samples_per_batch: 32
  total_cycle_time_min: 186
  parallel_capacity: 4
  throughput_8h: 32
  throughput_12h: 48
performance:
  recovery_rate: NA
  rsd_percent: NA
  detection_limit: NA
commercial:
  price_rmb:
    hardware: null
    installation: null
    training: null
    first_year_service: null
    total: null
  lead_time_weeks: 4
  warranty_months: 12
deployments:
  - customer: 成都食检院
    location: 成都
    date: 2026-05-25
    sample_throughput: 48样/天
    feedback: 客户反馈良好
    contact_available: true
alternatives:
  []
upgrades:
  []
tags: [食品检测, 兽残，127种药物残留-肉类]
---

# 成都食检院-127种药物残留-肉类

## 方案定位
面向食品安全检测的兽残前处理全自动化产线，覆盖提取平台→均质&加内标&调PH平台→恒温反应平台→提取平台→浓缩和复溶平台→2ml离心净化平台→LC-MS/MS检测平台

## 适用客户画像
1. 各级食品检验检测机构
2. 海关、农业农村部门技术中心
3. 大型食品/水产企业 QC 实验室

## 工艺选择理由
依据 GB 31658.26-2025《食品安全国家标准 动物性食品中 127 种药物残留量的筛查 液相色谱 - 高分辨质谱法》，结合客户日通量 48 样需求，选用 PLT-1200 + 7 模块组合……

## 配置说明
产线由 7 个 Workstation 组成

## 部署要求
占地约 12 ㎡，220V/24V 电源，需通风、压缩空气、地排，环境温度 4~40 ℃

## 已知限制
NA

## 关键风险
NA
