---
identification_version:
  id: PRJ-CDSJY-2026
  name: 成都市食品检验研究院-食品检测自动化前处理系统
  customer: 成都食检院
  version: 1.0
  status: active
  last_updated: 2026-05-25
  owner: 林雪平
  proposal_type: A
  stage: 详细设计
customer_info:
  customer_org: 成都市食品药品检验研究院
  location: 成都市
  industry: 食品安全检测
  contact_person: NA
  contact_info: NA
  kickoff_date: 2025-08-05
project_scope:
  business_objective: "一、按标准单独执行 / 1、高通量筛查：24个小时完成96份样品前处理。 / 2、硝基呋喃：36个小时完成48份样品前处理。 / 二、不同标准并行 / 1、硝基呋喃水解和衍生化需要19个小时，可并行完成： / 2、高通量兽残：64份样品的提取。 / "
  urgency: 高
  milestones:
    - 2026-03 详细设计冻结
    - 2026-05 设备到货安装
    - 2026-06 安装调试完成
    - 2026-07 验收交付
solutions:
  - sol_id: SOL-PEST-CDSJY-01
    sequence: 1
    line_name: 127种药物残留-肉类
    role: 肉类
    quantity: 1
  - sol_id: SOL-PEST-CDSJY-02
    sequence: 1
    line_name: 127种药物残留-牛奶
    role: 牛奶
    quantity: 1
  - sol_id: SOL-PEST-CDSJY-03
    sequence: 1
    line_name: 硝基呋喃21311-肌肉
    role: 肌肉
    quantity: 1
  - sol_id: SOL-PEST-CDSJY-04
    sequence: 1
    line_name: 硝基呋喃21311-蛋奶
    role: 蛋奶
    quantity: 1
  - sol_id: SOL-PEST-CDSJY-05
    sequence: 1
    line_name: 硝基呋喃31656.13-鱼肉
    role: 鱼肉
    quantity: 1
combined_throughput:
  total_samples_per_day: "一、按标准单独执行
1、高通量筛查：24个小时完成96份样品前处理。
2、硝基呋喃：36个小时完成48份样品前处理。
二、不同标准并行
1、硝基呋喃水解和衍生化需要19个小时，可并行完成：
2、高通量兽残：64份样品的提取。
"
  parallel_lines: 8
  full_workflow_time_h: "1、大兽残-肉蛋  96个样/ 26h
2、大兽残-牛奶  96个样/18.8h
3、21311-肉类  48个样/34h
4、21311-蛋奶  48个样/31.6h
5、31656-鱼肉  48个样/32h
"
site_requirements:
  floor_area_m2: 20
  ceiling_height_m: 3
  floor_load_kgm2: 500
  total_power_kva: "24kw"
  voltage_v: 220V,380V
  requires_ventilation: true
  requires_water_drain: true
  requires_compressed_air: true
  ambient_temp_c_min: 4
  ambient_temp_c_max: 26
delivery_plan:
  delivery_date: 2026/5/31
  install_weeks: 4
  training_days: 5
  warranty_months: 12
commercial:
  price_rmb:
    hardware: null
    installation: null
    training: null
    first_year_service: null
    total: null
references_group:
  matches_standards:
    - 1.GB 31658.26-2025《食品安全国家标准 动物性食品中 127 种药物残留量的筛查 液相色谱 - 高分辨质谱法》
    - 2.GB/T 21311-2007《动物源性食品中硝基呋喃类药物代谢物残留量检测方法 高效液相色谱 / 串联质谱法》
    - 3.GB 31658.2-2021《食品安全国家标准 动物性食品中氯霉素残留量的测定 液相色谱 - 串联质谱法》
    - 4.GB/T 21316-2007《动物源性食品中磺胺类药物残留量的测定 液相色谱 - 质谱 / 质谱法》
    - 5.GB/T 20366-2006《动物源产品中喹诺酮类残留量的测定 液相色谱 - 串联质谱法》
  customer_documents:
    - 客户兽残检测 SOP-2025-08
    - 客户招标文件 RFP-CDSJY-2026
success_metrics:
  - 一、按标准单独执行
  - 1、高通量筛查：24个小时完成96份样品前处理。
  - 2、硝基呋喃：36个小时完成48份样品前处理。
  - 二、不同标准并行
  - 1、硝基呋喃水解和衍生化需要19个小时，可并行完成：
  - 2、高通量兽残：64份样品的提取。
tags: [食品检测, 兽残, 127种药物残留，硝基呋喃]
---

# 成都市食品检验研究院-食品检测自动化前处理系统

## 项目背景
成都市食品检验研究院，属市市监局正处级公益二类事业单位，承担食品、食用农产品等监督检验、风险监测与技术研究，是国家食品复检机构，牵头食用农产品抽检监测，开展多兽药残留检测技术攻关。
成都食检院开展食品中硝基呋喃、氯霉素、磺胺类、沙星类兽残检测，前处理实操难点突出且极具针对性。四类兽残均为痕量残留，基质干扰极强，前处理是核心控险环节。人工操作前处理核心难点包括全程手工容错率极低，对人员熟练度要求高。样品均质易不均，高脂高蛋白基质分层，痕量兽残取样偏差大，重复性差。萃取极易乳化，分层困难且目标物易损失；SPE 柱活化、平衡、流速人工把控难，易柱干、堵塞、填料吸附，回收率波动大。氮吹浓缩控温控速难，热敏感物易分解、目标物易挥发，定容精度不足。基质杂质难除引发严重离子抑制，人工操作个体差异大，步骤繁琐，直接影响检测回收率、精准度与结果有效性。
兽残专项自动化检测方案以多品类兽残高通量大筛查为核心基础，深度整合自动化前处理与上机检测全流程，可同步实现硝基呋喃、氯霉素、磺胺类、沙星类四大重点兽残的全自动化检测。方案覆盖样品自动均质、萃取、净化、浓缩、定容、进样等关键环节，全程替代人工繁琐操作，精准规避人工均质不均、萃取乳化、SPE 柱操作偏差、氮吹损失等痛点，有效解决人工操作个体差异大、重复性差、效率低的问题，大幅提升检测回收率与精准度，同时降低基质干扰影响，兼顾高通量筛查与靶向定量需求，适配各类食品基质检测场景，实现兽残检测提质增效、结果精准可控。

## 业务目标
一、按标准单独执行
1、高通量筛查：24个小时完成96份样品前处理。
2、硝基呋喃：36个小时完成48份样品前处理。
二、不同标准并行
1、硝基呋喃水解和衍生化需要19个小时，可并行完成：
2、高通量兽残：64份样品的提取。


## 工艺选择理由
本方案可实现五大国家标准项下兽残的自动化前处理与一体化检测，高效覆盖硝基呋喃、氯霉素、磺胺类、沙星类等重点检测项目，核心检测效率与通量实现跨越式提升

## 配置亮点
针对不同样品检项的差异化需求，自动化系统可灵活适配、精准调度，核心环节全程无需人工干预，所有样品统一收样后，均可在 20 天内完成全流程检测并出具权威报告，兼顾高通量检测需求与结果时效性。

## 现场部署要点
机房地坪承重 ≥ 500 kg/㎡；UPS 3 kVA 在线式；排废管路独立；温湿度全程监控。

## 风险与缓解
风险：客户后端 LC-HRMS 与前处理通量不匹配 → 缓解：错峰排班 + 中转缓存。

## 可复用经验
NA
