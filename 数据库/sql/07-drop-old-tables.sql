-- =============================================
-- 校园活动管理系统 - 删除旧 6 表结构
-- 删除对象: Roles, Users, Activities, Registrations, CheckIns, Blacklist
-- =============================================

USE CampusActivityDB;
GO

IF OBJECT_ID(N'CheckIns', N'U') IS NOT NULL
    DROP TABLE CheckIns;
GO

IF OBJECT_ID(N'Registrations', N'U') IS NOT NULL
    DROP TABLE Registrations;
GO

IF OBJECT_ID(N'Blacklist', N'U') IS NOT NULL
    DROP TABLE Blacklist;
GO

IF OBJECT_ID(N'Activities', N'U') IS NOT NULL
    DROP TABLE Activities;
GO

IF OBJECT_ID(N'Users', N'U') IS NOT NULL
    DROP TABLE Users;
GO

IF OBJECT_ID(N'Roles', N'U') IS NOT NULL
    DROP TABLE Roles;
GO

PRINT N'旧 6 表已删除：Roles, Users, Activities, Registrations, CheckIns, Blacklist';
GO
