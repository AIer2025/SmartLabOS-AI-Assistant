-- =====================================================================
--  SmartLabOS 售前提案 AI Agent —— 数据库结构（MySQL 8.x, utf8mb4）
--
--  说明：本项目复用 DataMaintenance 既有的 `SmartLabOS-Presales-AI` 库与下述两表。
--       若库/表已存在（由 DataMaintenance 建过），本脚本幂等，可安全重复执行。
--
--  执行：mysql -u root -p < 01-presales-schema.sql
-- =====================================================================
SET NAMES utf8mb4;

CREATE DATABASE IF NOT EXISTS `SmartLabOS-Presales-AI`
  DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
USE `SmartLabOS-Presales-AI`;

-- 1) 售前项目需求（含「模块选定/确认」两列）------------------------------
CREATE TABLE IF NOT EXISTS `SmartLabOS-PresalesProject` (
    `id`                BIGINT       NOT NULL AUTO_INCREMENT COMMENT '自增主键',
    `project_name`      VARCHAR(255) NOT NULL                COMMENT '项目名称(唯一)，同时作为 projects/ 下目录名',
    `project_dir`       VARCHAR(512)                         COMMENT '项目输出目录绝对路径',
    `current_status`    VARCHAR(1500)                        COMMENT '客户项目现状(≤1500字)',
    `protocols`         JSON                                 COMMENT '已选流程标准(potocol/MD 文件名数组)',
    `challenges`        JSON                                 COMMENT '客户挑战(字符串数组)',
    `expectations`      JSON                                 COMMENT '客户期望(字符串数组)',
    `process_scope`     JSON                                 COMMENT '流程范围(多选)',
    `loading_method`    VARCHAR(32)                          COMMENT '上下料方式',
    `software_type`     VARCHAR(32)                          COMMENT '软件功能',
    `modules`           JSON                                 COMMENT '已选定/已确认模块ID数组(如 ["MOD-CC-001"])',
    `modules_confirmed` TINYINT(1)   NOT NULL DEFAULT 0      COMMENT '是否已点击「模块确认」',
    `gen_status`        VARCHAR(32)  NOT NULL DEFAULT 'draft' COMMENT 'draft/queued/running/succeeded/failed',
    `last_command_file` VARCHAR(512)                         COMMENT '最近一次生成的标记/指令文件名',
    `last_generated_at` TIMESTAMP NULL                       COMMENT '最近一次成功生成时间',
    `created_at`        TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at`        TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_presales_name` (`project_name`),
    KEY `idx_presales_status` (`gen_status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='SmartLabOS 售前项目需求信息';

-- 2) 方案生成执行记录 ------------------------------------------------------
CREATE TABLE IF NOT EXISTS `SmartLabOS-PresalesGeneration` (
    `id`            BIGINT       NOT NULL AUTO_INCREMENT COMMENT '自增主键',
    `project_id`    BIGINT       NOT NULL                COMMENT '所属售前项目ID',
    `project_name`  VARCHAR(255)                         COMMENT '项目名称(冗余)',
    `command_file`  VARCHAR(512)                         COMMENT '本次生成标记(如 Anthropic-Messages-API-时间戳)',
    `status`        VARCHAR(32)  NOT NULL DEFAULT 'queued' COMMENT 'queued/running/succeeded/failed/canceled',
    `exit_code`     INT                                  COMMENT '结束码(0=成功)',
    `output_files`  JSON                                 COMMENT '生成的文件名数组(*.html / *-URS.html / *.md)',
    `log_excerpt`   LONGTEXT                             COMMENT '运行日志摘要',
    `started_at`    TIMESTAMP    NULL                    COMMENT '开始时间',
    `finished_at`   TIMESTAMP    NULL                    COMMENT '结束时间',
    `created_at`    TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    KEY `idx_gen_project` (`project_id`),
    KEY `idx_gen_status`  (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='SmartLabOS 售前方案生成执行记录';

-- 完成。
