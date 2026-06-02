---
identification_version:
  id: WS-PEST-Thermostatic-SJY-08
  name: 食检院-兽残恒温反应储存平台
  version: 1.0
  status: active
  last_updated: 2026-4-10
  owner: 林雪平
  maturity: 验证中
hardware_config:
  platform_id: PLT-1200
  gripper: ERG32-150T旋转夹爪组件-一体式包胶手指
  pipette: NA
  scanner: NA
  transfer_platform: NA
  waste_basket: NA
  hmi: 工控机
  modules:
    - id: MOD-WX-004
      quantity: 50ml离心管单涡旋模块
      role: 1
    - id: MOD-CC-003
      quantity: 离心管恒温37°C衍生反应模块
      role: 1
  module_layout: 3x1
  tray_layout: 3X1
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
      - 1.GB/T 21311-2007《动物源性食品中硝基呋喃类药物代谢物残留量检测方法 高效液相色谱 / 串联质谱法》
    step_description: 涡旋-恒温反应
    consumables:
      - 50ml离心管
    workflow_steps:
      - step: 1
        name: 涡旋
        module_id: MOD-WX-004
        duration_min: 1
        parameters:
          时间: 30min
      - step: 2
        name: 恒温反应
        module_id: MOD-CC-003
        duration_min: 1
        parameters:
          时间: 16h
    total_cycle_time_min: 16
    parallel_capacity: 2
    batch_capacity: 48
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 0
    throughput_12h: 0
    target_analytes:
      - 硝基呋喃
    target_samples:
      - 肌肉
      - 蛋奶
  - name: 工站实验流程3
    matches_standards:
      - 1.GB 31656.13-2021《食品安全国家标准 水产品中硝基呋喃类代谢物多残留的测定 液相色谱 - 串联质谱法》
    step_description: 涡旋-恒温反应
    consumables:
      - 50ml离心管
    workflow_steps:
      - step: 1
        name: 涡旋
        module_id: MOD-WX-004
        duration_min: 1
        parameters:
          时间: 30min
      - step: 2
        name: 恒温反应
        module_id: MOD-CC-003
        duration_min: 1
        parameters:
          时间: 16h
    total_cycle_time_min: 16
    parallel_capacity: 2
    batch_capacity: 48
    target_industries:
      - 1.食品安全检测行业
    throughput_8h: 0
    throughput_12h: 0
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
tags: [恒温反应储存平台]
---

# 食检院-兽残恒温反应储存平台

## 方案定位
恒温存储平台专为兽残检测全流程设计，核心满足两大关键需求，为检测结果精准性与样品稳定性提供双重保障。一方面，平台可实现硝基呋喃检测专属的恒温衍生功能，通过密闭式避光舱体设计，隔绝光照对衍生反应的干扰，精准控温至 37℃（控温精度 ±0.5℃），并支持衍生时间 0–999min 自由设定，确保样品在稳定环境中完成充分衍生，避免光照或温度波动导致的目标物降解。另一方面，针对前处理与检测仪器节拍不一致的痛点，平台配备低温暂存模块，可提供 2–8℃恒温存储环境（控温精度 ±1℃），适配前处理结束后样品的短期保存需求。由于兽残类目标物在常温下易分解，无法长时间放置，该平台通过低温环境有效延缓目标物降解速度，保障样品在等待检测期间的稳定性，待检测仪器空闲后即可快速取出上机，避免因节拍错位影响检测结果。平台支持 96 位样品位，兼容 2mL 进样瓶，可与自动化前处理系统、检测仪器联动，实现衍生、暂存、转运全流程自动化，大幅提升检测效率与数据可靠性。

## 适用客户画像
平台适配畜禽肉、水产、果蔬等不同基质样品的前处理需求

## 配置说明
恒温模块


## 部署要求
占地 1.02㎡，220V电压，通风系统、排废系统
