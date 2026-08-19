-- =============================================
-- 校园活动管理系统 - 按 plan.txt 表结构创建索引
-- =============================================

USE CampusActivityDB;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_User_Role_IsActive' AND object_id = OBJECT_ID(N'[User]'))
    CREATE NONCLUSTERED INDEX IX_User_Role_IsActive
    ON [User]([Role], IsActive)
    INCLUDE (UserName, RealName, Email);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ActivityApply_AuditStatus_CreateTime' AND object_id = OBJECT_ID(N'ActivityApply'))
    CREATE NONCLUSTERED INDEX IX_ActivityApply_AuditStatus_CreateTime
    ON ActivityApply(AuditStatus, CreateTime DESC)
    INCLUDE (Title, PublisherUserName);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Activity_PublisherUserName_CreateTime' AND object_id = OBJECT_ID(N'Activity'))
    CREATE NONCLUSTERED INDEX IX_Activity_PublisherUserName_CreateTime
    ON Activity(PublisherUserName, CreateTime DESC)
    INCLUDE (Title, ActivityStartTime, ActivityEndTime);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Activity_EnrollTime' AND object_id = OBJECT_ID(N'Activity'))
    CREATE NONCLUSTERED INDEX IX_Activity_EnrollTime
    ON Activity(EnrollStartTime, EnrollEndTime)
    INCLUDE (Title, MaxCount, CurrentCount);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Message_ActivityId_CreateTime' AND object_id = OBJECT_ID(N'[Message]'))
    CREATE NONCLUSTERED INDEX IX_Message_ActivityId_CreateTime
    ON [Message](ActivityId, CreateTime DESC)
    INCLUDE (UserName);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Enroll_UserName' AND object_id = OBJECT_ID(N'Enroll'))
    CREATE NONCLUSTERED INDEX IX_Enroll_UserName
    ON Enroll(UserName, EnrollTime DESC)
    INCLUDE (ActivityId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Enroll_ActivityId' AND object_id = OBJECT_ID(N'Enroll'))
    CREATE NONCLUSTERED INDEX IX_Enroll_ActivityId
    ON Enroll(ActivityId, EnrollTime DESC)
    INCLUDE (UserName);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SignIn_ActivityId' AND object_id = OBJECT_ID(N'SignIn'))
    CREATE NONCLUSTERED INDEX IX_SignIn_ActivityId
    ON SignIn(ActivityId, SignInTime DESC)
    INCLUDE (UserName);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ActivitySignInCode_ActivityId' AND object_id = OBJECT_ID(N'ActivitySignInCode'))
    CREATE NONCLUSTERED INDEX IX_ActivitySignInCode_ActivityId
    ON ActivitySignInCode(ActivityId, ExpireTime DESC)
    INCLUDE (Code);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ResetPwdApply_Status' AND object_id = OBJECT_ID(N'ResetPwdApply'))
    CREATE NONCLUSTERED INDEX IX_ResetPwdApply_Status
    ON ResetPwdApply([Status])
    INCLUDE (UserName, RealName);
GO

PRINT N'============================================';
PRINT N'  plan.txt 表结构索引创建完成';
PRINT N'============================================';
GO
