# =============================================
# 校园活动管理系统 - 启动 SQL Server 服务
# =============================================
# 用途: 启动本机 MSSQLSERVER 服务，并处理启动失败的情况
# 用法: 以管理员身份在 PowerShell 中执行 .\start_sql.ps1
# 注意: 启动 Windows 服务需要管理员权限
# =============================================

try {
    # 尝试启动 SQL Server 默认实例
    Start-Service MSSQLSERVER -ErrorAction Stop
    Write-Host "MSSQLSERVER started successfully" -ForegroundColor Green
} catch {
    # 启动失败时输出错误信息，并查询系统事件日志帮助排查
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host "Checking event log for SQL Server related errors..." -ForegroundColor Yellow

    # 查询最近 5 条与服务控制管理器相关的错误事件
    Get-EventLog -LogName System -Source "Service Control Manager" -Newest 5 -EntryType Error `
        | Where-Object { $_.Message -like '*SQL*' } `
        | Format-List TimeGenerated, Message
}
