-- =============================================
-- 校园活动管理系统 - plan.txt 表结构验证查询
-- =============================================

USE CampusActivityDB;
GO

-- 查询所有用户表（BASE TABLE），预期包含 plan.txt 的 8 张表
SELECT
    TABLE_SCHEMA AS 架构,
    TABLE_NAME   AS 表名,
    TABLE_TYPE   AS 类型
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_NAME IN (
      'User', 'ActivityApply', 'Activity', 'Message',
      'Enroll', 'SignIn', 'ActivitySignInCode', 'ResetPwdApply'
  )
ORDER BY TABLE_NAME;
GO

-- 查看用户表结构
EXEC sp_help '[User]';
GO

-- 查看所有 plan 表的列
SELECT
    TABLE_NAME              AS 表名,
    COLUMN_NAME             AS 列名,
    DATA_TYPE               AS 类型,
    CHARACTER_MAXIMUM_LENGTH AS 最大长度,
    IS_NULLABLE             AS 允许空,
    COLUMN_DEFAULT          AS 默认值
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (
      'User', 'ActivityApply', 'Activity', 'Message',
      'Enroll', 'SignIn', 'ActivitySignInCode', 'ResetPwdApply'
)
ORDER BY TABLE_NAME, ORDINAL_POSITION;
GO

-- 查看各表数据量
SELECT 'User' AS TableName, COUNT(*) AS [RowCount] FROM [User]
UNION ALL SELECT 'ActivityApply', COUNT(*) FROM ActivityApply
UNION ALL SELECT 'Activity', COUNT(*) FROM Activity
UNION ALL SELECT 'Message', COUNT(*) FROM [Message]
UNION ALL SELECT 'Enroll', COUNT(*) FROM Enroll
UNION ALL SELECT 'SignIn', COUNT(*) FROM SignIn
UNION ALL SELECT 'ActivitySignInCode', COUNT(*) FROM ActivitySignInCode
UNION ALL SELECT 'ResetPwdApply', COUNT(*) FROM ResetPwdApply;
GO

-- 查看当前所有登录账号
SELECT
    name                  AS 登录名,
    type_desc             AS 登录类型,
    is_disabled           AS 是否禁用,
    create_date           AS 创建时间,
    default_database_name AS 默认数据库
FROM sys.server_principals
WHERE type IN ('S', 'U', 'G')
  AND name NOT LIKE '##%'
ORDER BY type_desc, name;
GO

-- 创建远程连接登录账号（开发环境）
USE master;
GO
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'CampusApp')
BEGIN
    CREATE LOGIN CampusApp
        WITH PASSWORD = '111111',
        CHECK_POLICY = OFF,
        DEFAULT_DATABASE = CampusActivityDB;
END
GO

USE CampusActivityDB;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'CampusApp')
BEGIN
    CREATE USER CampusApp FOR LOGIN CampusApp;
    ALTER ROLE db_datareader ADD MEMBER CampusApp;
    ALTER ROLE db_datawriter ADD MEMBER CampusApp;
END
GO

SELECT
    sp.name        AS 登录名,
    sp.is_disabled AS 是否禁用,
    dp.name        AS 数据库用户,
    r.name         AS 数据库角色
FROM sys.server_principals sp
LEFT JOIN sys.database_principals dp ON sp.sid = dp.sid
LEFT JOIN sys.database_role_members rm ON dp.principal_id = rm.member_principal_id
LEFT JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id
WHERE sp.name = 'CampusApp';
GO

-- 推荐连接字符串:
-- Server=localhost;Database=CampusActivityDB;User Id=CampusApp;Password=111111;TrustServerCertificate=True
