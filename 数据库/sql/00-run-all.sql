-- =============================================
-- Main SQLCMD entry point.
-- Run this file in SQLCMD mode so the :r include directives below are expanded.
-- Execution order matters:
--   1. Create database and tables if they do not exist.
--   2. Repair legacy Activity.AuditStatus constraint for the one-table workflow.
--   3. Create query indexes.
--   4. Seed demo data.
--   5. Run QA checks.
-- =============================================

-- =============================================
-- 校园活动管理系统 - 一键建库脚本（SQLCMD 模式）
-- 成员C 数据库交付主入口
-- =============================================
-- 执行方式：
--   1. SSMS：菜单 Query -> SQLCMD Mode，然后执行本文件
--   2. 命令行：sqlcmd -S localhost -E -C -f 65001 -i sql\00-run-all.sql
-- =============================================

PRINT N'============================================';
PRINT N'  CampusActivityDB 初始化开始';
PRINT N'============================================';
GO

:r .\sql\01-create-tables.sql
:r .\sql\08-fix-activity-audit-status-constraint.sql
:r .\sql\02-create-indexes.sql
:r .\sql\03-seed-data.sql
:r .\sql\06-member-c-qa-checks.sql

PRINT N'============================================';
PRINT N'  CampusActivityDB 初始化与验证完成';
PRINT N'============================================';
GO
