-- =====================================================================
--  SmartLabOS-Presales-AI  「售前方案自动生成」功能表结构
--  目标: MySQL 8.x  字符集: utf8mb4
--
--  说明:
--    本脚本在已有 01-schema.sql 的基础上，新增两张表用于
--    「SmartLabOS 售前方案自动生成」功能：
--      - SmartLabOS-PresalesProject     : 客户项目需求信息（可保存/再编辑）
--      - SmartLabOS-PresalesGeneration   : 每次「方案生成」的执行记录（历史留痕）
--
--    需求信息中的多值字段（已选流程标准、挑战、期望、流程范围）以 JSON 数组保存，
--    便于一次性读写并保持顺序；可查询的标量字段（项目名、状态等）提为独立列。
--
--  执行: mysql -u root -p < 03-presales-schema.sql
-- =====================================================================

SET NAMES utf8mb4;

USE `SmartLabOS-Presales-AI`;

-- ---------------------------------------------------------------------
-- 1. 售前项目需求  PresalesProject
--    一行 = 一个用户新建的客户项目（对应 projects/<project_name> 目录）
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `SmartLabOS-PresalesProject` (
    `id`              BIGINT       NOT NULL AUTO_INCREMENT COMMENT '自增主键',
    `project_name`    VARCHAR(255) NOT NULL                COMMENT '项目名称(唯一)，同时作为 projects/ 下的目录名',
    `project_dir`     VARCHAR(512)                         COMMENT '项目输出目录绝对路径',

    `current_status`  VARCHAR(1500)                        COMMENT '6.2.1 客户项目现状(≤1500字)',
    `protocols`       JSON                                 COMMENT '6.2.2 已选流程标准(potocol/MD 文件名数组)',
    `challenges`      JSON                                 COMMENT '6.2.3 客户挑战(字符串数组, ≤20项, 每项≤200字)',
    `expectations`    JSON                                 COMMENT '6.2.4 客户期望(字符串数组, ≤20项, 每项≤200字)',
    `process_scope`   JSON                                 COMMENT '6.2.5 流程范围(采样/制样/分样/前处理/检测/出具报告 多选)',
    `loading_method`  VARCHAR(32)                          COMMENT '6.2.5 上下料方式: 人工上下料 / 自动上下料',
    `software_type`   VARCHAR(32)                          COMMENT '6.2.6 软件功能: 设备操作软件 / 智慧实验室软件',

    `gen_status`      VARCHAR(32)  NOT NULL DEFAULT 'draft' COMMENT '方案生成状态: draft/queued/running/succeeded/failed',
    `last_command_file` VARCHAR(512)                       COMMENT '最近一次生成所用的指令 txt 文件名',
    `last_generated_at` TIMESTAMP NULL                     COMMENT '最近一次成功生成时间',

    `created_at`      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at`      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_presales_name` (`project_name`),
    KEY `idx_presales_status` (`gen_status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='SmartLabOS 售前项目需求信息';

-- ---------------------------------------------------------------------
-- 2. 方案生成执行记录  PresalesGeneration
--    一行 = 一次「方案生成」运行（调用 Claude Code 的一次任务）
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `SmartLabOS-PresalesGeneration` (
    `id`            BIGINT       NOT NULL AUTO_INCREMENT COMMENT '自增主键',
    `project_id`    BIGINT       NOT NULL                COMMENT '所属售前项目ID',
    `project_name`  VARCHAR(255)                         COMMENT '项目名称(冗余, 便于查询)',
    `command_file`  VARCHAR(512)                         COMMENT '本次生成所用指令 txt 文件名',
    `status`        VARCHAR(32)  NOT NULL DEFAULT 'queued' COMMENT 'queued/running/succeeded/failed/canceled',
    `exit_code`     INT                                  COMMENT 'claude 进程退出码',
    `output_files`  JSON                                 COMMENT '生成的文件名数组(*.html / *-URS.html / *.docx WORD提案)',
    `log_excerpt`   LONGTEXT                             COMMENT 'claude 运行日志摘要(stdout/stderr)',
    `started_at`    TIMESTAMP    NULL                    COMMENT '开始时间',
    `finished_at`   TIMESTAMP    NULL                    COMMENT '结束时间',
    `created_at`    TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    KEY `idx_gen_project` (`project_id`),
    KEY `idx_gen_status`  (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='SmartLabOS 售前方案生成执行记录';

-- 完成。
