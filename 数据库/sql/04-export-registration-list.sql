-- =============================================
-- 校园活动管理系统 - 报名名单导出查询（plan.txt 表结构）
-- =============================================

USE CampusActivityDB;
GO

DECLARE @ActivityId INT = 1; -- 修改为要导出的活动ID

SELECT
    a.Id              AS ActivityId,
    a.Title           AS ActivityTitle,
    u.UserName        AS UserName,
    u.RealName        AS RealName,
    u.Email           AS Email,
    e.EnrollTime      AS EnrollTime,
    CASE WHEN s.Id IS NULL THEN N'否' ELSE N'是' END AS HasSignedIn,
    s.SignInTime      AS SignInTime
FROM Enroll e
JOIN [User] u ON e.UserName = u.UserName
JOIN Activity a ON e.ActivityId = a.Id
LEFT JOIN SignIn s
    ON s.UserName = e.UserName
   AND s.ActivityId = e.ActivityId
WHERE e.ActivityId = @ActivityId
ORDER BY e.EnrollTime DESC, u.UserName ASC;
GO

-- 命令行导出示例（PowerShell / CMD 中执行，按实际服务器修改 -S）：
-- sqlcmd -S localhost -d CampusActivityDB -E -W -s "," -f 65001 -Q "SET NOCOUNT ON; SELECT a.Id AS ActivityId,a.Title AS ActivityTitle,u.UserName,u.RealName,u.Email,e.EnrollTime,CASE WHEN s.Id IS NULL THEN N'否' ELSE N'是' END AS HasSignedIn,s.SignInTime FROM Enroll e JOIN [User] u ON e.UserName=u.UserName JOIN Activity a ON e.ActivityId=a.Id LEFT JOIN SignIn s ON s.UserName=e.UserName AND s.ActivityId=e.ActivityId WHERE e.ActivityId=1 ORDER BY e.EnrollTime DESC,u.UserName ASC;" -o enroll_activity_1.csv
