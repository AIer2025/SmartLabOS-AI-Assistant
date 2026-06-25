-- =====================================================================
--  SmartLabOS-Presales-AI  数据库与表结构脚本
--  目标: MySQL 8.x (Community / 8.5)  字符集: utf8mb4
--
--  说明:
--    数据库名与表名按需求中的命名带连字符 "-"，
--    在 MySQL 中必须使用反引号 `...` 引用。
--
--    每张表采用 "关键字段列 + 全量保真" 的混合结构:
--      - 可查询的关键字段 (id / name / status / owner ...) 提为独立列
--      - data_json      : 整段 front-matter 解析后的 JSON (MySQL JSON 类型, 可查询)
--      - raw_frontmatter: 原始 YAML front-matter 原文 (无损保留)
--      - body_markdown  : front-matter 之后的 Markdown 正文 (无损保留)
--
--  执行: mysql -u root -p < 01-schema.sql
-- =====================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

CREATE DATABASE IF NOT EXISTS `SmartLabOS-Presales-AI`
    DEFAULT CHARACTER SET utf8mb4
    DEFAULT COLLATE utf8mb4_unicode_ci;

USE `SmartLabOS-Presales-AI`;

-- ---------------------------------------------------------------------
-- 1. 模块  Module  (references/01-modules)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `SmartLabOS-Module` (
    `id`              VARCHAR(64)  NOT NULL COMMENT '卡片ID，如 MOD-CC-001',
    `name`            VARCHAR(255)          COMMENT '模块名称',
    `module_part_no`  VARCHAR(64)           COMMENT '模块物料号',
    `category`        VARCHAR(64)           COMMENT '模块分类',
    `version`         VARCHAR(32)           COMMENT '版本',
    `status`          VARCHAR(32)           COMMENT '状态 active/...',
    `owner`           VARCHAR(64)           COMMENT '负责人',
    `last_updated`    VARCHAR(32)           COMMENT '最后更新(原文保留为字符串)',
    `tags`            VARCHAR(512)          COMMENT '标签',
    `data_json`       JSON                  COMMENT 'front-matter 全量 JSON',
    `raw_frontmatter` LONGTEXT              COMMENT '原始 YAML front-matter',
    `body_markdown`   LONGTEXT              COMMENT 'Markdown 正文',
    `source_file`     VARCHAR(255)          COMMENT '来源文件名',
    `created_at`      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at`      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    KEY `idx_module_name`     (`name`),
    KEY `idx_module_category` (`category`),
    KEY `idx_module_status`   (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='SmartLabOS 模块主数据';

-- ---------------------------------------------------------------------
-- 2. 平台基类  PlatformBase  (references/02-platforms)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `SmartLabOS-PlatformBase` (
    `id`               VARCHAR(64)  NOT NULL COMMENT '卡片ID，如 PLT-800',
    `name`             VARCHAR(255)          COMMENT '平台名称',
    `platform_part_no` VARCHAR(64)           COMMENT '平台物料号',
    `version`          VARCHAR(32)           COMMENT '版本',
    `status`           VARCHAR(32)           COMMENT '状态',
    `owner`            VARCHAR(64)           COMMENT '负责人',
    `last_updated`     VARCHAR(32)           COMMENT '最后更新',
    `tags`             VARCHAR(512)          COMMENT '标签',
    `data_json`        JSON                  COMMENT 'front-matter 全量 JSON',
    `raw_frontmatter`  LONGTEXT              COMMENT '原始 YAML front-matter',
    `body_markdown`    LONGTEXT              COMMENT 'Markdown 正文',
    `source_file`      VARCHAR(255)          COMMENT '来源文件名',
    `created_at`       TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at`       TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    KEY `idx_platform_name`   (`name`),
    KEY `idx_platform_status` (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='SmartLabOS 平台基类主数据';

-- ---------------------------------------------------------------------
-- 3. 工作站  WorkStation  (references/03-workstation)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `SmartLabOS-WorkStation` (
    `id`              VARCHAR(64)  NOT NULL COMMENT '卡片ID，如 WS-PEST-Extract-SJY-01',
    `name`            VARCHAR(255)          COMMENT '工作站名称',
    `platform_id`     VARCHAR(64)           COMMENT '所用平台ID',
    `maturity`        VARCHAR(64)           COMMENT '成熟度 验证中/已验证',
    `version`         VARCHAR(32)           COMMENT '版本',
    `status`          VARCHAR(32)           COMMENT '状态',
    `owner`           VARCHAR(64)           COMMENT '负责人',
    `last_updated`    VARCHAR(32)           COMMENT '最后更新',
    `tags`            VARCHAR(512)          COMMENT '标签',
    `data_json`       JSON                  COMMENT 'front-matter 全量 JSON',
    `raw_frontmatter` LONGTEXT              COMMENT '原始 YAML front-matter',
    `body_markdown`   LONGTEXT              COMMENT 'Markdown 正文',
    `source_file`     VARCHAR(255)          COMMENT '来源文件名',
    `created_at`      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at`      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    KEY `idx_ws_name`     (`name`),
    KEY `idx_ws_platform` (`platform_id`),
    KEY `idx_ws_status`   (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='SmartLabOS 工作站主数据';

-- ---------------------------------------------------------------------
-- 4. 解决方案  Solution  (references/04-solutions)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `SmartLabOS-Solution` (
    `id`              VARCHAR(64)  NOT NULL COMMENT '卡片ID，如 SOL-PEST-CDSJY-01',
    `name`            VARCHAR(255)          COMMENT '方案名称',
    `proposal_type`   VARCHAR(32)           COMMENT '提案类型 A/B/...',
    `maturity`        VARCHAR(64)           COMMENT '成熟度',
    `version`         VARCHAR(32)           COMMENT '版本',
    `status`          VARCHAR(32)           COMMENT '状态',
    `owner`           VARCHAR(64)           COMMENT '负责人',
    `last_updated`    VARCHAR(32)           COMMENT '最后更新',
    `tags`            VARCHAR(512)          COMMENT '标签',
    `data_json`       JSON                  COMMENT 'front-matter 全量 JSON',
    `raw_frontmatter` LONGTEXT              COMMENT '原始 YAML front-matter',
    `body_markdown`   LONGTEXT              COMMENT 'Markdown 正文',
    `source_file`     VARCHAR(255)          COMMENT '来源文件名',
    `created_at`      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at`      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    KEY `idx_sol_name`   (`name`),
    KEY `idx_sol_status` (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='SmartLabOS 解决方案主数据';

-- ---------------------------------------------------------------------
-- 5. 项目  Project  (references/05-Project)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `SmartLabOS-Project` (
    `id`              VARCHAR(64)  NOT NULL COMMENT '卡片ID，如 PRJ-CDSJY-2026',
    `name`            VARCHAR(255)          COMMENT '项目名称',
    `customer`        VARCHAR(255)          COMMENT '客户',
    `proposal_type`   VARCHAR(32)           COMMENT '提案类型',
    `stage`           VARCHAR(64)           COMMENT '阶段',
    `version`         VARCHAR(32)           COMMENT '版本',
    `status`          VARCHAR(32)           COMMENT '状态',
    `owner`           VARCHAR(64)           COMMENT '负责人',
    `last_updated`    VARCHAR(32)           COMMENT '最后更新',
    `tags`            VARCHAR(512)          COMMENT '标签',
    `data_json`       JSON                  COMMENT 'front-matter 全量 JSON',
    `raw_frontmatter` LONGTEXT              COMMENT '原始 YAML front-matter',
    `body_markdown`   LONGTEXT              COMMENT 'Markdown 正文',
    `source_file`     VARCHAR(255)          COMMENT '来源文件名',
    `created_at`      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at`      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    KEY `idx_prj_name`     (`name`),
    KEY `idx_prj_customer` (`customer`),
    KEY `idx_prj_status`   (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='SmartLabOS 项目主数据';

SET FOREIGN_KEY_CHECKS = 1;

-- 完成。可执行 02-verify.sql 查看导入结果。
