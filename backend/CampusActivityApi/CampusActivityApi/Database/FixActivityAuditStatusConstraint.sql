-- 修复 Activity.AuditStatus 的 CHECK 约束。
-- 正确业务约定为：0=待审核，1=通过，2=驳回。
-- 需要使用具备 ALTER TABLE 权限的数据库账号执行。

-- 防止在错误数据库或表缺失时静默执行。
IF OBJECT_ID(N'dbo.Activity', N'U') IS NULL
BEGIN
    THROW 50000, 'Table dbo.Activity does not exist.', 1;
END;

-- 删除旧约束；旧库中该约束可能只允许 AuditStatus=1，导致活动申请无法写入待审核状态。
IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Activity_AuditStatus'
      AND parent_object_id = OBJECT_ID(N'dbo.Activity')
)
BEGIN
    ALTER TABLE [dbo].[Activity] DROP CONSTRAINT [CK_Activity_AuditStatus];
END;

-- 重新创建正确约束，使活动申请、审核通过和驳回三个状态都能保存。
ALTER TABLE [dbo].[Activity]
ADD CONSTRAINT [CK_Activity_AuditStatus] CHECK ([AuditStatus] IN (0, 1, 2));
