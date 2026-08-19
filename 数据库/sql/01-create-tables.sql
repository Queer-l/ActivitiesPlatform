-- =============================================
-- 校园活动管理系统 - 按 plan.txt 创建数据库与表结构
-- 目标数据库: SQL Server / SQLCMD
-- =============================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CampusActivityDB')
BEGIN
    CREATE DATABASE CampusActivityDB;
END
GO

USE CampusActivityDB;
GO

-- 1. 用户表 User
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[User]') AND type = 'U')
BEGIN
    CREATE TABLE [User] (
        Id          INT IDENTITY(1,1) NOT NULL,
        UserName    NVARCHAR(50)      NOT NULL,
        [Password]  NVARCHAR(100)     NOT NULL,
        RealName    NVARCHAR(50)      NOT NULL,
        [Role]      TINYINT           NOT NULL CONSTRAINT DF_User_Role DEFAULT 1,
        Email       NVARCHAR(100)     NULL,
        IsActive    TINYINT           NOT NULL CONSTRAINT DF_User_IsActive DEFAULT 1,

        CONSTRAINT PK_User PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_User_UserName UNIQUE (UserName),
        CONSTRAINT CK_User_Role CHECK ([Role] IN (1, 2, 3)),
        CONSTRAINT CK_User_IsActive CHECK (IsActive IN (0, 1))
    );
END
GO

-- 2. 活动申请表 ActivityApply
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'ActivityApply') AND type = 'U')
BEGIN
    CREATE TABLE ActivityApply (
        Id                 INT IDENTITY(1,1) NOT NULL,
        Title              NVARCHAR(100)     NOT NULL,
        Content            NVARCHAR(MAX)     NOT NULL,
        PublisherUserName  NVARCHAR(50)      NOT NULL,
        AuditStatus        TINYINT           NOT NULL CONSTRAINT DF_ActivityApply_AuditStatus DEFAULT 0,
        RejectReason       NVARCHAR(MAX)     NULL,
        CreateTime         DATETIME          NOT NULL CONSTRAINT DF_ActivityApply_CreateTime DEFAULT GETDATE(),
        MaxCount           INT               NOT NULL,
        CurrentCount       INT               NOT NULL CONSTRAINT DF_ActivityApply_CurrentCount DEFAULT 0,
        SignInCount        INT               NOT NULL CONSTRAINT DF_ActivityApply_SignInCount DEFAULT 0,
        ActivityStartTime  DATETIME          NOT NULL,
        ActivityEndTime    DATETIME          NOT NULL,
        EnrollStartTime    DATETIME          NOT NULL,
        EnrollEndTime      DATETIME          NOT NULL,

        CONSTRAINT PK_ActivityApply PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_ActivityApply_AuditStatus CHECK (AuditStatus IN (0, 1, 2)),
        CONSTRAINT CK_ActivityApply_Counts CHECK (MaxCount >= 0 AND CurrentCount >= 0 AND SignInCount >= 0),
        CONSTRAINT CK_ActivityApply_ActivityTime CHECK (ActivityStartTime < ActivityEndTime),
        CONSTRAINT CK_ActivityApply_EnrollTime CHECK (EnrollStartTime < EnrollEndTime)
    );
END
GO

-- 3. 正式活动表 Activity
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'Activity') AND type = 'U')
BEGIN
    CREATE TABLE Activity (
        Id                 INT IDENTITY(1,1) NOT NULL,
        Title              NVARCHAR(100)     NOT NULL,
        Content            NVARCHAR(MAX)     NOT NULL,
        PublisherUserName  NVARCHAR(50)      NOT NULL,
        AuditStatus        TINYINT           NOT NULL CONSTRAINT DF_Activity_AuditStatus DEFAULT 1,
        RejectReason       NVARCHAR(MAX)     NULL,
        CreateTime         DATETIME          NOT NULL CONSTRAINT DF_Activity_CreateTime DEFAULT GETDATE(),
        MaxCount           INT               NOT NULL,
        CurrentCount       INT               NOT NULL CONSTRAINT DF_Activity_CurrentCount DEFAULT 0,
        SignInCount        INT               NOT NULL CONSTRAINT DF_Activity_SignInCount DEFAULT 0,
        ActivityStartTime  DATETIME          NOT NULL,
        ActivityEndTime    DATETIME          NOT NULL,
        EnrollStartTime    DATETIME          NOT NULL,
        EnrollEndTime      DATETIME          NOT NULL,

        CONSTRAINT PK_Activity PRIMARY KEY CLUSTERED (Id),
        -- Activity is used by the backend for both applications and published activities.
        -- Keep this constraint aligned with AuditStatusEnum: 0 = pending, 1 = passed, 2 = rejected.
        CONSTRAINT CK_Activity_AuditStatus CHECK (AuditStatus IN (0, 1, 2)),
        CONSTRAINT CK_Activity_Counts CHECK (MaxCount >= 0 AND CurrentCount >= 0 AND SignInCount >= 0),
        CONSTRAINT CK_Activity_ActivityTime CHECK (ActivityStartTime < ActivityEndTime),
        CONSTRAINT CK_Activity_EnrollTime CHECK (EnrollStartTime < EnrollEndTime)
    );
END
GO

-- 4. 留言表 Message
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Message]') AND type = 'U')
BEGIN
    CREATE TABLE [Message] (
        Id          INT IDENTITY(1,1) NOT NULL,
        ActivityId  INT               NOT NULL,
        UserName    NVARCHAR(50)      NOT NULL,
        Content     NVARCHAR(MAX)     NOT NULL,
        CreateTime  DATETIME          NOT NULL CONSTRAINT DF_Message_CreateTime DEFAULT GETDATE(),

        CONSTRAINT PK_Message PRIMARY KEY CLUSTERED (Id)
    );
END
GO

-- 5. 报名表 Enroll
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'Enroll') AND type = 'U')
BEGIN
    CREATE TABLE Enroll (
        Id          INT IDENTITY(1,1) NOT NULL,
        UserName    NVARCHAR(50)      NOT NULL,
        ActivityId  INT               NOT NULL,
        EnrollTime  DATETIME          NOT NULL CONSTRAINT DF_Enroll_EnrollTime DEFAULT GETDATE(),

        CONSTRAINT PK_Enroll PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Enroll_UserName_ActivityId UNIQUE (UserName, ActivityId)
    );
END
GO

-- 6. 签到表 SignIn
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SignIn') AND type = 'U')
BEGIN
    CREATE TABLE SignIn (
        Id          INT IDENTITY(1,1) NOT NULL,
        UserName    NVARCHAR(50)      NOT NULL,
        ActivityId  INT               NOT NULL,
        SignInTime  DATETIME          NOT NULL CONSTRAINT DF_SignIn_SignInTime DEFAULT GETDATE(),

        CONSTRAINT PK_SignIn PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_SignIn_UserName_ActivityId UNIQUE (UserName, ActivityId)
    );
END
GO

-- 7. 签到码表 ActivitySignInCode
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'ActivitySignInCode') AND type = 'U')
BEGIN
    CREATE TABLE ActivitySignInCode (
        Id          INT IDENTITY(1,1) NOT NULL,
        Code        NVARCHAR(20)      NOT NULL,
        ActivityId  INT               NOT NULL,
        ExpireTime  DATETIME          NOT NULL,

        CONSTRAINT PK_ActivitySignInCode PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_ActivitySignInCode_Code UNIQUE (Code)
    );
END
GO

-- 8. 密码重置申请表 ResetPwdApply
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'ResetPwdApply') AND type = 'U')
BEGIN
    CREATE TABLE ResetPwdApply (
        Id            INT IDENTITY(1,1) NOT NULL,
        UserName      NVARCHAR(50)      NOT NULL,
        RealName      NVARCHAR(50)      NOT NULL,
        Reason        NVARCHAR(MAX)     NOT NULL,
        [Status]      TINYINT           NOT NULL CONSTRAINT DF_ResetPwdApply_Status DEFAULT 0,
        RejectReason  NVARCHAR(MAX)     NULL,

        CONSTRAINT PK_ResetPwdApply PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_ResetPwdApply_Status CHECK ([Status] IN (0, 1, 2))
    );
END
GO

PRINT N'============================================';
PRINT N'  按 plan.txt 创建表完成';
PRINT N'  共 8 张表：User, ActivityApply, Activity, Message, Enroll, SignIn, ActivitySignInCode, ResetPwdApply';
PRINT N'============================================';
GO
