-- =============================================
-- 校园活动管理系统 - 数据备份与恢复策略
-- 成员C：系统数据备份与恢复
-- =============================================
-- 执行前请确认 D:\数据库\backup 目录存在，或修改 @BackupDir。
-- 恢复脚本会覆盖数据库，正式环境执行前必须确认无人连接。
-- =============================================

USE master;
GO

DECLARE @DbName SYSNAME = N'CampusActivityDB';
DECLARE @BackupDir NVARCHAR(260) = N'D:\数据库\backup\';
DECLARE @BackupFile NVARCHAR(4000);

SET @BackupFile = @BackupDir + @DbName + N'_full_' +
    CONVERT(CHAR(8), GETDATE(), 112) + N'_' +
    REPLACE(CONVERT(CHAR(8), GETDATE(), 108), ':', '') + N'.bak';

PRINT N'开始完整备份：' + @BackupFile;

BACKUP DATABASE CampusActivityDB
TO DISK = @BackupFile
WITH INIT, COMPRESSION, CHECKSUM, STATS = 10;

RESTORE VERIFYONLY
FROM DISK = @BackupFile
WITH CHECKSUM;

PRINT N'备份完成并通过校验。';
GO

-- =============================================
-- 恢复示例
-- =============================================
-- 1. 将 @RestoreFile 改为真实 .bak 文件路径
-- 2. 确认需要覆盖 CampusActivityDB 后再执行
-- 3. 如果数据文件逻辑名不同，先执行：
--    RESTORE FILELISTONLY FROM DISK = N'D:\数据库\backup\xxx.bak';
-- =============================================
/*
USE master;
GO

DECLARE @RestoreFile NVARCHAR(4000) = N'D:\数据库\backup\CampusActivityDB_full_20260518_120000.bak';

ALTER DATABASE CampusActivityDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

RESTORE DATABASE CampusActivityDB
FROM DISK = @RestoreFile
WITH REPLACE, RECOVERY, CHECKSUM, STATS = 10;

ALTER DATABASE CampusActivityDB SET MULTI_USER;

PRINT N'数据库恢复完成。';
GO
*/
