-- =============================================
-- 校园活动管理系统 - plan.txt 表结构 QA 验证脚本
-- =============================================

USE CampusActivityDB;
GO

PRINT N'1. plan.txt 核心表数量（预期 8 张）';
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_NAME IN (
      'User', 'ActivityApply', 'Activity', 'Message',
      'Enroll', 'SignIn', 'ActivitySignInCode', 'ResetPwdApply'
  )
ORDER BY TABLE_NAME;

PRINT N'2. 约束清单';
SELECT
    o.name AS TableName,
    kc.name AS ConstraintName,
    kc.type_desc AS ConstraintType
FROM sys.key_constraints kc
JOIN sys.objects o ON kc.parent_object_id = o.object_id
WHERE o.name IN ('User','ActivityApply','Activity','Message','Enroll','SignIn','ActivitySignInCode','ResetPwdApply')
UNION ALL
SELECT
    o.name,
    cc.name,
    'CHECK_CONSTRAINT'
FROM sys.check_constraints cc
JOIN sys.objects o ON cc.parent_object_id = o.object_id
WHERE o.name IN ('User','ActivityApply','Activity','Message','Enroll','SignIn','ActivitySignInCode','ResetPwdApply')
ORDER BY TableName, ConstraintName;

PRINT N'3. 非聚集索引清单';
SELECT
    OBJECT_NAME(object_id) AS TableName,
    name AS IndexName,
    type_desc AS IndexType,
    has_filter,
    filter_definition
FROM sys.indexes
WHERE object_id IN (
    OBJECT_ID(N'[User]'), OBJECT_ID(N'ActivityApply'), OBJECT_ID(N'Activity'),
    OBJECT_ID(N'[Message]'), OBJECT_ID(N'Enroll'), OBJECT_ID(N'SignIn'),
    OBJECT_ID(N'ActivitySignInCode'), OBJECT_ID(N'ResetPwdApply')
)
  AND is_primary_key = 0
  AND is_unique_constraint = 0
ORDER BY TableName, IndexName;

PRINT N'4. 种子数据概览';
SELECT 'User' AS TableName, COUNT(*) AS [RowCount] FROM [User]
UNION ALL SELECT 'ActivityApply', COUNT(*) FROM ActivityApply
UNION ALL SELECT 'Activity', COUNT(*) FROM Activity
UNION ALL SELECT 'Message', COUNT(*) FROM [Message]
UNION ALL SELECT 'Enroll', COUNT(*) FROM Enroll
UNION ALL SELECT 'SignIn', COUNT(*) FROM SignIn
UNION ALL SELECT 'ActivitySignInCode', COUNT(*) FROM ActivitySignInCode
UNION ALL SELECT 'ResetPwdApply', COUNT(*) FROM ResetPwdApply;

PRINT N'5. 报名与签到一致性检查（预期无结果）';
SELECT s.*
FROM SignIn s
LEFT JOIN Enroll e
    ON e.UserName = s.UserName
   AND e.ActivityId = s.ActivityId
WHERE e.Id IS NULL;

PRINT N'6. 活动报名人数与签到人数统计';
SELECT
    a.Id AS ActivityId,
    a.Title,
    COUNT(DISTINCT e.Id) AS EnrolledCount,
    COUNT(DISTINCT s.Id) AS SignedInCount,
    CAST(
        CASE
            WHEN COUNT(DISTINCT e.Id) = 0 THEN 0
            ELSE COUNT(DISTINCT s.Id) * 100.0 / COUNT(DISTINCT e.Id)
        END AS DECIMAL(5,2)
    ) AS SignInRate
FROM Activity a
LEFT JOIN Enroll e ON e.ActivityId = a.Id
LEFT JOIN SignIn s ON s.ActivityId = a.Id
GROUP BY a.Id, a.Title
ORDER BY a.Id;
GO
