-- =====================================================================
--  SmartLabOS-Presales-AI  「模块选定 / 模块确认」功能增量迁移
--  目标: MySQL 8.x  字符集: utf8mb4
--
--  说明:
--    在 03-presales-schema.sql 的基础上，为 `SmartLabOS-PresalesProject`
--    增加两列，用于支撑「方案生成」前的「模块选定 → 模块确认」两步流程：
--      - modules            : 已选定/已确认的模块ID数组(JSON)，元素形如 "MOD-CC-001"
--      - modules_confirmed  : 是否已点击「模块确认」(0=仅选定未确认 / 1=已确认)
--
--    MySQL 8.0 的 ALTER TABLE 不支持 ADD COLUMN IF NOT EXISTS，
--    这里用 INFORMATION_SCHEMA 判断后动态执行，保证可重复运行(幂等)。
--
--  执行: mysql -u root -p < 04-presales-modules.sql
-- =====================================================================

SET NAMES utf8mb4;

USE `SmartLabOS-Presales-AI`;

-- ---- modules 列 ----------------------------------------------------
SET @col_exists := (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'SmartLabOS-PresalesProject'
      AND COLUMN_NAME = 'modules');
SET @ddl := IF(@col_exists = 0,
    'ALTER TABLE `SmartLabOS-PresalesProject` ADD COLUMN `modules` JSON NULL COMMENT ''已选定/已确认的模块ID数组(如 ["MOD-CC-001"])'' AFTER `software_type`',
    'SELECT 1');
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---- modules_confirmed 列 ------------------------------------------
SET @col_exists := (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'SmartLabOS-PresalesProject'
      AND COLUMN_NAME = 'modules_confirmed');
SET @ddl := IF(@col_exists = 0,
    'ALTER TABLE `SmartLabOS-PresalesProject` ADD COLUMN `modules_confirmed` TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''是否已点击「模块确认」: 0未确认 / 1已确认'' AFTER `modules`',
    'SELECT 1');
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 完成。
