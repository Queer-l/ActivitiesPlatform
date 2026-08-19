-- =============================================
-- 校园活动管理系统 - 按 plan.txt 表结构初始化测试数据
-- =============================================

USE CampusActivityDB;
GO

IF NOT EXISTS (SELECT 1 FROM [User] WHERE UserName = N'admin')
BEGIN
    INSERT INTO [User] (UserName, [Password], RealName, [Role], Email, IsActive)
    VALUES (N'admin', N'Admin@123456', N'系统管理员', 3, N'admin@campus.edu.cn', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM [User] WHERE UserName = N'teacher01')
BEGIN
    INSERT INTO [User] (UserName, [Password], RealName, [Role], Email, IsActive)
    VALUES (N'teacher01', N'Teacher@123', N'活动管理员王老师', 2, N'teacher01@campus.edu.cn', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM [User] WHERE UserName = N'student01')
BEGIN
    INSERT INTO [User] (UserName, [Password], RealName, [Role], Email, IsActive)
    VALUES (N'student01', N'Student@123', N'测试学生张三', 1, N'student01@campus.edu.cn', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM [User] WHERE UserName = N'student02')
BEGIN
    INSERT INTO [User] (UserName, [Password], RealName, [Role], Email, IsActive)
    VALUES (N'student02', N'Student@123', N'测试学生李四', 1, N'student02@campus.edu.cn', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM ActivityApply WHERE Title = N'校园科技创新分享会')
BEGIN
    INSERT INTO ActivityApply (
        Title, Content, PublisherUserName, AuditStatus, RejectReason, MaxCount,
        CurrentCount, SignInCount, ActivityStartTime, ActivityEndTime,
        EnrollStartTime, EnrollEndTime
    )
    VALUES (
        N'校园科技创新分享会',
        N'邀请优秀项目团队分享人工智能、物联网、软件开发方向的参赛经验。',
        N'teacher01',
        0,
        NULL,
        120,
        0,
        0,
        '2026-06-20 14:00:00',
        '2026-06-20 17:00:00',
        '2026-06-01 08:00:00',
        '2026-06-18 23:59:59'
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM Activity WHERE Title = N'春季校园运动会')
BEGIN
    INSERT INTO Activity (
        Title, Content, PublisherUserName, AuditStatus, RejectReason, MaxCount,
        CurrentCount, SignInCount, ActivityStartTime, ActivityEndTime,
        EnrollStartTime, EnrollEndTime
    )
    VALUES (
        N'春季校园运动会',
        N'一年一度的春季校园运动会，包含田径、球类等多个项目。',
        N'teacher01',
        1,
        NULL,
        500,
        2,
        1,
        '2026-05-25 08:00:00',
        '2026-05-25 17:00:00',
        '2026-05-01 08:00:00',
        '2026-05-24 23:59:59'
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM Activity WHERE Title = N'校园社团文化节')
BEGIN
    INSERT INTO Activity (
        Title, Content, PublisherUserName, AuditStatus, RejectReason, MaxCount,
        CurrentCount, SignInCount, ActivityStartTime, ActivityEndTime,
        EnrollStartTime, EnrollEndTime
    )
    VALUES (
        N'校园社团文化节',
        N'全校社团联合展示，包含文艺演出、社团招新和互动体验。',
        N'teacher01',
        1,
        NULL,
        300,
        0,
        0,
        '2026-06-05 09:00:00',
        '2026-06-05 21:00:00',
        '2026-05-20 08:00:00',
        '2026-06-04 23:59:59'
    );
END
GO

DECLARE @sportsActivityId INT = (SELECT Id FROM Activity WHERE Title = N'春季校园运动会');

IF @sportsActivityId IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM Enroll WHERE UserName = N'student01' AND ActivityId = @sportsActivityId)
BEGIN
    INSERT INTO Enroll (UserName, ActivityId, EnrollTime)
    VALUES (N'student01', @sportsActivityId, '2026-05-10 14:30:00');
END

IF @sportsActivityId IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM Enroll WHERE UserName = N'student02' AND ActivityId = @sportsActivityId)
BEGIN
    INSERT INTO Enroll (UserName, ActivityId, EnrollTime)
    VALUES (N'student02', @sportsActivityId, '2026-05-11 09:10:00');
END

IF @sportsActivityId IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM SignIn WHERE UserName = N'student01' AND ActivityId = @sportsActivityId)
BEGIN
    INSERT INTO SignIn (UserName, ActivityId, SignInTime)
    VALUES (N'student01', @sportsActivityId, '2026-05-25 08:15:00');
END

IF @sportsActivityId IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM ActivitySignInCode WHERE Code = N'SPORTS2026')
BEGIN
    INSERT INTO ActivitySignInCode (Code, ActivityId, ExpireTime)
    VALUES (N'SPORTS2026', @sportsActivityId, '2026-05-25 17:00:00');
END
GO

IF NOT EXISTS (SELECT 1 FROM [Message] WHERE UserName = N'student01' AND Content = N'活动安排很清楚，期待参加。')
BEGIN
    DECLARE @sportsActivityIdForMsg INT = (SELECT Id FROM Activity WHERE Title = N'春季校园运动会');
    INSERT INTO [Message] (ActivityId, UserName, Content, CreateTime)
    VALUES (@sportsActivityIdForMsg, N'student01', N'活动安排很清楚，期待参加。', '2026-05-12 10:00:00');
END
GO

IF NOT EXISTS (SELECT 1 FROM ResetPwdApply WHERE UserName = N'student02' AND [Status] = 0)
BEGIN
    INSERT INTO ResetPwdApply (UserName, RealName, Reason, [Status], RejectReason)
    VALUES (N'student02', N'测试学生李四', N'忘记登录密码，申请管理员重置。', 0, NULL);
END
GO

PRINT N'============= 数据初始化结果验证 =============';
SELECT Id, UserName, RealName, [Role], Email, IsActive FROM [User] ORDER BY Id;
SELECT Id, Title, PublisherUserName, AuditStatus, MaxCount, CurrentCount, SignInCount FROM Activity ORDER BY Id;
SELECT 'ActivityApply' AS TableName, COUNT(*) AS [RowCount] FROM ActivityApply
UNION ALL SELECT 'Message', COUNT(*) FROM [Message]
UNION ALL SELECT 'Enroll', COUNT(*) FROM Enroll
UNION ALL SELECT 'SignIn', COUNT(*) FROM SignIn
UNION ALL SELECT 'ActivitySignInCode', COUNT(*) FROM ActivitySignInCode
UNION ALL SELECT 'ResetPwdApply', COUNT(*) FROM ResetPwdApply;
GO
