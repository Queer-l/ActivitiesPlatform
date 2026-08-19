-- Fix Activity.AuditStatus constraint for the current one-table activity workflow.
-- The backend stores pending applications and published activities in dbo.Activity,
-- so AuditStatus must allow: 0 = pending, 1 = passed, 2 = rejected.
--
-- Required permission:
--   The executing database user must be db_owner/db_ddladmin or have ALTER permission
--   on dbo.Activity. The application account CampusApp normally has only read/write
--   data permissions, so an administrator may need to run this script.
--
-- Safe to rerun:
--   The script drops the old CK_Activity_AuditStatus if it exists, recreates it with
--   the correct allowed values, then prints the final constraint definition.

USE CampusActivityDB;
GO

-- Fail early when the target table is missing or invisible to the current user.
IF OBJECT_ID(N'dbo.Activity', N'U') IS NULL
BEGIN
    THROW 50001, 'Table dbo.Activity does not exist.', 1;
END
GO

-- Remove the legacy constraint. Older scripts created it as "AuditStatus = 1",
-- which blocks pending applications inserted with AuditStatus = 0.
IF OBJECT_ID(N'dbo.CK_Activity_AuditStatus', N'C') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Activity DROP CONSTRAINT CK_Activity_AuditStatus;
END
GO

-- Recreate the constraint to match the backend AuditStatusEnum values.
ALTER TABLE dbo.Activity
ADD CONSTRAINT CK_Activity_AuditStatus
CHECK (AuditStatus IN (0, 1, 2));
GO

-- Return the final definition so the operator can verify the repair immediately.
SELECT
    cc.name AS ConstraintName,
    cc.definition AS Definition
FROM sys.check_constraints AS cc
WHERE cc.parent_object_id = OBJECT_ID(N'dbo.Activity')
  AND cc.name = N'CK_Activity_AuditStatus';
GO
