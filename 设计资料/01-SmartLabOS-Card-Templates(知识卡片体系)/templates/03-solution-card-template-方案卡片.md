---
# ========== 标识与版本 ==========
id: SOL-XXXX                          # 方案唯一编号,建议 SOL- 前缀
name: 方案显示名称                     # 如"标准农残提取方案"
version: 1.0
status: draft                         # draft | active | deprecated
last_updated: 2026-04-25
owner: 方案负责人或团队
maturity: 概念                        # 概念 | 验证中 | 已交付 | 标准化

# ========== 适用场景 ==========
target_industry:                       # 目标行业
  - 食品检测
  - 环境监测
target_samples:                        # 适用样品类型
  - 蔬菜
  - 水果
  - 土壤
target_analytes:                       # 目标分析物
  - 农药残留
  - 重金属
throughput_target:                     # 通量需求
  samples_per_day: 0
  samples_per_batch: 0
turnaround_time_h: 0                  # 单批周转时间
matches_standards:                     # 对标的方法标准
  - GB 23200.X
  - EPA Method XXX

# ========== 硬件配置 ==========
platform:
  id: PLT-XXXX                        # 引用平台卡片 ID
  quantity: 1                         # 平台数量(多平台方案可 >1)

modules:                               # 模块清单(引用模块卡片 ID)
  - id: MOD-XXXX
    quantity: 3
    role: 加液振荡阶段
  - id: MOD-XXXX
    quantity: 1
    role: 离心分离
  - id: MOD-XXXX
    quantity: 1
    role: 移液转移

accessories:                           # 配套耗材/抓手清单
  - 试管-15ml
  - SPE 柱-3ml
  - 抓手-15ml试管

# ========== 工艺流程 ==========
workflow:
  - step: 1
    name: 称量
    module_id: MOD-XXXX
    duration_min: 5
    parameters:
      mass_g: { min: 1, max: 5 }
    note: 可选,该步骤的特别说明
  - step: 2
    name: 加液
    module_id: MOD-XXXX
    duration_min: 3
    parameters:
      volume_ml: 10
  - step: 3
    name: 振荡提取
    module_id: MOD-XXXX
    duration_min: 30
    parameters:
      speed_rpm: 1500
      temperature_c: 25
  # ... 后续步骤

total_cycle_time_min: 0               # 单样品端到端总耗时
parallel_capacity: 0                  # 并行处理能力(同时跑几个样)

# ========== 性能与指标 ==========
performance:
  recovery_rate: { min: 0.7, max: 1.2 }  # 加标回收率范围
  rsd_percent: 10                     # 相对标准偏差上限
  detection_limit: null               # 可选,检出限
  validated: false                    # 是否已做方法学验证

# ========== 资源占用汇总(自动核算) ==========
resource_summary:
  total_modules_used: 0               # 应 ≤ 平台 max_modules
  total_trays_used: 0                 # 应 ≤ 平台 max_trays
  total_power_kva_peak: 0.0           # 应 ≤ 平台 power_kva_peak
  ventilation_required: false        # 是否触发通风规则
  compressed_air_required: false
  water_required: false

# ========== 商务信息 ==========
price_rmb:
  hardware: null                      # 硬件总价
  installation: null                  # 安装调试
  training: null                      # 培训
  first_year_service: null           # 首年服务
  total: null
lead_time_weeks: null
warranty_months: 12

# ========== 已交付案例 ==========
deployments:
  - customer: 某某检测中心             # 可脱敏
    location: 城市
    date: 2026-01
    sample_throughput: 200样/天
    feedback: 简要客户反馈            # 可选
    contact_available: false         # 是否可作为参考客户

# ========== 替代方案与升级路径 ==========
alternatives:                          # 同类需求的其他方案
  - id: SOL-XXXX
    differ_in: 通量更高,价格更高
upgrades:                              # 该方案可平滑升级到的方案
  - id: SOL-XXXX
    upgrade_path: 增加 X 模块即可

# ========== 标签 ==========
tags: [主推方案, 食品检测, 中通量]
---

# 方案显示名称

## 方案定位
一段话说清楚:这个方案给谁用、解决什么问题、为什么选它而不是别的。这是销售对客户的第一句话。

## 适用客户画像
- 客户类型(第三方检测、政府机构、企业自检等)
- 典型样品量级
- 预算区间
- 实验室条件要求(场地、电力、通风)

## 工艺流程说明
按 frontmatter 里 workflow 的步骤展开,用人话讲清楚每一步在做什么、为什么这么做、关键参数怎么定的。这部分给工艺工程师和有经验的客户看。

## 配置说明
解释为什么选这个平台、为什么是这个模块组合。哪些是核心,哪些是可选。

## 性能数据
- 实测回收率、RSD 数据(如已有)
- 与人工操作的对比
- 与同类自动化方案的对比

## 部署要求
- 场地:面积、承重
- 电力:总功率、回路要求
- 公用工程:通风、压缩空气、纯水、排液
- 环境:温湿度、洁净度

## 已知限制
- 不适用的样品类型
- 参数边界(浓度过高/过低时的表现)
- 维护要求与停机时间

## 客户常见问题
列几个销售最常被问到的问题和标准答复,后期会很值钱。

## 备注
订货流程、培训计划、售后承诺等。
