---
id: SOL-QUECHERS-MID
name: QuEChERS 中通量农残提取方案
version: 1.0
status: active
last_updated: 2026-04-25
owner: SmartLabOS 应用组
maturity: 已交付

target_industry:
  - 食品检测
  - 农产品质量监督
target_samples:
  - 蔬菜
  - 水果
  - 茶叶
target_analytes:
  - 农药残留(多组分)
throughput_target:
  samples_per_day: 200
  samples_per_batch: 24
turnaround_time_h: 4
matches_standards:
  - GB 23200.113
  - GB 23200.121

platform:
  id: PLT-1400
  quantity: 1

modules:
  - id: MOD-WEIGH-12
    quantity: 1
    role: 自动称量
  - id: MOD-DISP-SHAKE-24
    quantity: 1
    role: 加液振荡(QuEChERS 提取)
  - id: MOD-CENT-24
    quantity: 1
    role: 离心分相
  - id: MOD-PIPETTE-8
    quantity: 1
    role: 上清液转移
  - id: MOD-SPE-12
    quantity: 1
    role: PSA/C18 分散净化
  - id: MOD-CONC-6
    quantity: 1
    role: 氮吹浓缩定容

accessories:
  - 试管-50ml
  - 试管-15ml
  - SPE 柱-3ml
  - 抓手-15ml试管
  - 抓手-50ml试管

workflow:
  - step: 1
    name: 称样
    module_id: MOD-WEIGH-12
    duration_min: 8
    parameters:
      mass_g: { min: 5, max: 10 }
  - step: 2
    name: 加乙腈+盐包,涡旋振荡
    module_id: MOD-DISP-SHAKE-24
    duration_min: 5
    parameters:
      volume_ml: 10
      speed_rpm: 1500
      time_s: 60
  - step: 3
    name: 离心分相
    module_id: MOD-CENT-24
    duration_min: 5
    parameters:
      speed_rpm: 4000
      time_s: 300
  - step: 4
    name: 上清液转移
    module_id: MOD-PIPETTE-8
    duration_min: 4
    parameters:
      volume_ml: 6
  - step: 5
    name: 分散净化
    module_id: MOD-SPE-12
    duration_min: 8
    parameters:
      volume_ml: 6
  - step: 6
    name: 氮吹浓缩定容
    module_id: MOD-CONC-6
    duration_min: 25
    parameters:
      temperature_c: 40
      volume_ml: 1
    note: 浓缩到近干后用 1ml 流动相复溶

total_cycle_time_min: 55
parallel_capacity: 24

performance:
  recovery_rate: { min: 0.7, max: 1.2 }
  rsd_percent: 10
  validated: true

resource_summary:
  total_modules_used: 6
  total_trays_used: 6
  total_power_kva_peak: 3.8
  ventilation_required: false
  compressed_air_required: true
  water_required: false

price_rmb:
  hardware: null
  installation: null
  training: null
  first_year_service: null
  total: null
lead_time_weeks: 10
warranty_months: 12

deployments:
  - customer: 某省级食品检测中心
    location: 西部某市
    date: 2025-11
    sample_throughput: 180样/天
    feedback: 通量达标,RSD 优于人工
    contact_available: false

alternatives:
  - id: SOL-QUECHERS-HIGH
    differ_in: 换 PLT-1800 双倍通量,价格高 60%
upgrades:
  - id: SOL-QUECHERS-HIGH
    upgrade_path: 平台升级到 PLT-1800,模块翻倍

tags: [主推方案, 食品检测, 中通量, QuEChERS]
---

# QuEChERS 中通量农残提取方案

## 方案定位
针对食品检测院所、农产品质检中心等中通量(150~250 样/天)用户,基于 GB 23200 系列标准的 QuEChERS 自动化方案。一台 PLT-1400 平台覆盖从称样到定容的全流程,人工只需上下样和补充耗材。

## 适用客户画像
- 客户类型:省/市级食品检测中心、第三方检测公司、大型商超自检中心
- 典型样品量级:日均 150~250 样,旺季可冲到 300 样
- 预算区间:中等(具体请联系销售团队)
- 实验室条件:常规 220V 单回路 + 压缩空气;不需要特殊通风

## 工艺流程说明
完全遵循 GB 23200.113/121 的 QuEChERS 流程。称样后加乙腈和盐包(MgSO₄ + NaCl)涡旋提取,4000 rpm 离心 5 分钟分相,自动取上清液过 PSA/C18 分散净化柱,最后氮吹浓缩到近干后用 1ml 流动相复溶,直接进 LC-MS/MS 或 GC-MS/MS。

整个流程单批次 24 样,端到端 55 分钟,理论日通量 24 样 × (8 小时 × 60 / 55) ≈ 210 样,与目标通量匹配。

## 配置说明
**核心模块**(必选):称量、加液振荡、离心、移液、SPE、浓缩共 6 个,刚好填满 PLT-1400 的 6 个模块位。

**为什么选 PLT-1400 而不是 1200**:1200 平台只能装 4 个模块,需要把 SPE 或浓缩外置,会增加人工搬运,影响通量。

**为什么不选 1800**:1800 价格高 60%,通量翻倍,但 250 样/天以下用 1800 会浪费产能。

## 性能数据
- 实测加标回收率 70~120%(已在某省级食品检测中心 6 个月数据)
- RSD ≤ 10%,优于人工操作的 ~15%
- 与人工对比:人工日通量约 60~80 样,本方案 180~210 样,效率提升 2.5~3 倍

## 部署要求
- 场地:平台占地 1.4×0.85 m,加上前后维护空间需 ~3 m²,承重 ≥ 500 kg/m²
- 电力:单相 220V/20A,建议独立回路;峰值功率 3.8 kVA
- 公用工程:需压缩空气(0.5~0.7 MPa,无油干燥),需排废液管
- 环境:常规实验室温湿度即可,无特殊洁净度要求

## 已知限制
- 不适用于动物源样品(脂肪含量高,需改用动物源 QuEChERS 方案 SOL-QUECHERS-ANIMAL)
- 浓度极低(< 1 μg/kg)的目标物建议改用 GPC 凝胶净化方案
- 同时检测多组高极性差异农药时,需评估 PSA/C18 净化效果

## 客户常见问题
- **能否兼容其他国标方法?** 工艺流程基本一致,模块也能用,主要差异在参数(称样量、试剂)。
- **耗材成本多少?** 单样耗材成本约 X 元(具体请联系销售团队)。
- **故障停机怎么办?** 单模块故障可手动接管该步骤,继续跑;平台级故障 4 小时响应。

## 备注
- 订货周期 10 周,含工厂出厂测试和现场安装调试 2 周
- 培训:含 5 天现场培训 + 30 天远程支持
- 首年服务:包含 4 次定期保养 + 故障 4 小时响应
