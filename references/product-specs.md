# SmartLabOS 产品技术规格

> ⚠️ 请将「百泉聚兴智慧实验室SmartLabOS系统技术规格书.pdf」中的关键内容填入此文件。
> 以下为需要填充的框架，请用实际数据替换占位内容。

## 1. 系统架构概述

SmartLabOS 采用微服务架构，由 11 个独立部署的服务组成，按照五层架构组织：
- 接入层：ApiGateway + SSO
- 业务服务层：APS/LIMS、MES、EAM、WMS、SysModular、ConfigModular
- 调度控制层：Scheduling + WCS
- 设备通信层：ModbusService
- 物理设备层：PLC/天平/泵/电机/阀门/AGV

### 技术栈
- 运行时：.NET 8.0 / ASP.NET Core
- Web 框架：Furion 4.9
- API 网关：Ocelot 23.4 + Polly
- ORM：SqlSugar（Repository 模式）
- 消息队列：RabbitMQ / Redis Queue
- 数据库：MySQL / 达梦（支持国产信创）
- 操作系统：麒麟 V10 / CentOS
- IoT 通信：Modbus RTU/TCP/ASCII，40+ 设备驱动

## 2. 微服务列表

| 编号 | 服务名称 | 功能定位 |
|------|---------|---------|
| 01 | ApiGateway | API 网关：路由/鉴权/限流/熔断 |
| 02 | ApsModular | APS/LIMS：工艺流程/实验任务/报告 |
| 03 | ConfigModular | 配置中心：字典/建模/审批流/报表引擎 |
| 04 | EAM | 设备管理：台账/保养/维修/报警 |
| 05 | MES SmartLabOS | 制造执行：Protocol/工单/EBR/APIJSON |
| 06 | ModbusService | 设备通信：40+ 品牌设备驱动 |
| 07 | Scheduling | 任务调度：5 Worker + RabbitMQ |
| 08 | SSO | 统一认证：JWT/AuthCode/Refresh |
| 09 | SysModular | 系统管理：RBAC/用户/角色/日志 |
| 10 | WCS | 仓库控制：机器人调度/gRPC |
| 11 | WMS | 仓库管理：建模/三模式调度/出入库 |

## 3. 核心功能模块

### 3.1 样品检验管理（LIMS）
- 检验申请 → 检验计划 → 取样接收 → 任务分配 → 结果录入 → 数据审核 → 报告生成
- 支持 GB 8170 修约、自定义公式、检出限判断
- 支持电子签名和电子印章
- 支持留样管理和全链路追溯

### 3.2 制造执行（MES）
- Protocol 方法库：JSON 定义实验自动化流程
- 销售订单 → 生产工单 → 实验任务（8 态状态机）
- EBR 电子批记录（满足 GMP/GLP 审计）

### 3.3 仓库管理（WMS）
- 数字化仓库建模
- 货架/库位三级管理
- 三模式 WCS 调度
- 电子锁控 + 温度监控

### 3.4 设备资产管理（EAM）
- 设备台账、保养计划、维修管理、报警管理

## 4. 性能指标

| 指标 | 要求 |
|------|------|
| 并发用户数 | ≥ 50 |
| 常规页面响应 | ≤ 2 秒 |
| 复杂查询响应 | ≤ 5 秒 |
| 系统可用性 | ≥ 99.5% |
| 仪器数据采集延迟 | 串口 ≤ 1 秒 |
| 数据保留期限 | 在线 ≥ 3 年，归档 ≥ 10 年 |

## 5. 系统集成接口

| 接口 | 说明 | 对接系统 |
|------|------|---------|
| 主数据同步 | 物料等基础信息 | ERP |
| 样品请验 | 自动请验 | MES/WMS/ERP |
| 检验结果反馈 | 数据提取 | MES/WMS/ERP |
| 在线分析数据 | 实时数据库 | DCS/RTDB |
