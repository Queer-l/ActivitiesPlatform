接口与数据库设计文档
一、基础信息
后端地址：http://localhost:5196/api
认证方式：JWT Bearer Token
请求头：Authorization: Bearer {token}
时间格式：yyyy-MM-dd HH:mm:ss（北京时间）
角色说明：
01 = 普通用户
02 = 普通管理员
3 = 超级管理员
统一返回格式
json
{
"code":200,
"msg":"操作成功",
"data":{}
}
错误码
0200：成功
0400：参数错误 / 业务失败
0401：未登录 / Token 过期
0403：权限不足
二、接口清单
1. 用户模块
1.1 密码登录
请求方式：POST
接口地址：/User/LoginByPwd
请求参数（form/query）
ouserName（string，必填）：用户名 / 学工号
opwd（string，必填）：密码
返回示例
json
{
"code":200,
"msg":"登录成功",
"data":{
"token":"eyJbciJIUzI1NiIInRIIpX]...",
"userName":"admin",
"role":3
}
}
1.2 发送邮箱验证码
请求方式：POST
接口地址：/User/SendEmailCode
请求参数
oemail（string，必填）：绑定的邮箱地址
返回：{"code":200,"msg":"验证码已发送","data":null}
1.3 邮箱验证码登录
请求方式：POST
接口地址：/User/LoginByEmail
请求参数
oemail（string，必填）：绑定邮箱
code（string，必填，4 位）：邮箱验证码
返回：同 1.1
1.4 绑定邮箱
请求方式：POST
接口地址：/User/BindEmail
请求参数
ouserName（string，必填）：当前用户名
oemail（string，必填）：要绑定的邮箱
code（string，必填）：邮箱验证码
返回：{"code":200,"msg":"绑定成功","data":null}
1.5 邮箱重置密码
请求方式：POST
接口地址：/User/ResetPasswordByEmail
请求参数
oemail（string，必填）：绑定邮箱
code（string，必填）：邮箱验证码
onewPwd（string，必填，≥6 位）：新密码
返回：{"code":200,"msg":"密码重置成功","data":null}
1.6 提交密码重置申请
请求方式：POST
接口地址：/User/ApplyResetPwd
请求参数
ouserName（string，必填）：用户名
orealName（string，必填）：真实姓名
oreason（string，必填）：申请理由
返回：{"code":200,"msg":"提交成功","data":null}
1.7 获取用户列表（管理员）
请求方式：GET
接口地址：/User/GetUsersList
权限：role≥2
返回：用户数组，字段：id、userName、password、realName、role、email、isActive
1.8 添加用户（仅超级管理员）
请求方式：POST
接口地址：/User/AddUser
权限：role=3
请求参数
ouserName（string，必填，唯一）：用户名 / 学工号
orealName（string，必填）：真实姓名
orole（int，必填，1/2/3）：用户角色
oemail（string，可选）：邮箱
返回：{"code":200,"msg":"添加成功,初始密码123456","data":null}
1.9 编辑用户（管理员）
请求方式：POST
接口地址：/User/AdminEditUser
权限：role≥2
请求参数
ouserName（string，必填）：目标用户名
oemail（string，可选）：新邮箱
orole（int，可选，1/2/3）：新角色
返回：{"code":200,"msg":"编辑成功","data":null}
1.10 获取密码重置申请
请求方式：GET
接口地址：/User/GetPwdResetApplies
权限：role≥2
返回：申请数组，字段：id、userName、realName、reason、status、rejectReason
1.11 处理密码重置申请
请求方式：POST
接口地址：/User/HandlePwdResetApply
权限：role≥2
请求参数
oid（int，必填）：申请 ID
oisPass（bool，必填）：是否通过
orejectReason（string，可选，驳回时必填）：驳回理由
返回：{"code":200,"msg":"处理成功","data":null}
2. 活动模块
2.1 提交活动申请
请求方式：POST
接口地址：/Activity/ApplActivity
请求体（JSON）
json
{
"title":"校园读书分享会",
"content":"活动详情...",
"publisherUserName":"student001",
"maxCount":50,
"activityStartTime":"2026-05-20 14:00:00",
"activityEndTime":"2026-05-20 16:00:00",
"enrollStartTime":"2026-05-18 08:00:00",
"enrollEndTime":"2026-05-19 20:00:00"
}
参数说明
title（string，必填，≤100 字）：活动名称
content（string，必填）：活动详情
publisherUserName（string，必填）：当前用户名
maxCount（int，必填，≥1）：最大参与人数
activityStartTime（datetime，必填）：活动开始时间
activityEndTime（datetime，必填，晚于开始时间）：活动结束时间
enrollStartTime（datetime，必填）：报名开始时间
enrollEndTime（datetime，必填，晚于报名开始、早于活动开始）：报名结束时间
返回：{"code":200,"msg":"申请提交成功","data":null}
2.2 获取待审核申请（管理员）
请求方式：GET
接口地址：/Activity/GetActivityApplies
权限：role≥2
返回：申请数组，字段同 2.1，含 auditStatus、auditStatusText、rejectReason、createTime
2.3 审核活动
请求方式：POST
接口地址：/Activity/CheckActivityApplies
权限：role≥2
请求参数
oid（int，必填）：申请 ID
oisPass（bool，必填）：是否通过
orejectReason（string，可选，驳回时必填）：驳回理由
返回：{"code":200,"msg":"审核成功","data":null}
2.4 取消自己的申请
请求方式：POST
接口地址：/Activity/CancelActivityApply
权限：登录用户，仅自己的申请
请求参数
oapplyId（int，必填）：申请 ID
返回：{"code":200,"msg":"取消成功","data":null}
2.5 下架已发布活动
请求方式：POST
接口地址：/Activity/TakeDownActivity
权限：role≥2 或活动发布者
请求参数
oactivityId（int，必填）：正式活动 ID
返回：{"code":200,"msg":"下架成功","data":null}
2.6 获取已发布活动列表
请求方式：GET
接口地址：/Activity/GetActivityslist
返回字段
oid：活动 ID
otitle：活动名称
ocontent：活动详情
opublisherUserName：发布者用户名
oauditStatus：审核状态（1 = 通过）
oauditStatusText：审核文字
ocreateTime：创建时间
omaxCount：最大人数
ocurrentCount：当前报名数
osignInCount：已签到数
oactivityStartTime：活动开始
oactivityEndTime：活动结束
oenrollStartTime：报名开始
oenrollEndTime：报名结束
ostatusText：活动状态（预热 / 报名 / 待开始 / 进行 / 结束）
omessages：留言数组
2.7 发表活动留言
请求方式：POST
接口地址：/Activity/AddMessage
权限：登录用户
请求参数
oactivityId（int，必填）：正式活动 ID
ocontent（string，必填，≤200 字）：留言内容
返回：{"code":200,"msg":"留言成功","data":null}
2.8 删除留言
请求方式：POST
接口地址：/Activity/DeleteMessage
权限：留言作者或 role≥2
请求参数
oactivityId（int，必填）：正式活动 ID
omessageId（int，必填）：留言 ID
返回：{"code":200,"msg":"删除成功","data":null}
2.9 获取活动留言
请求方式：GET
接口地址：/Activity/GetMessagesByActivity?activityId=1
请求参数：activityId（int，必填）
返回：留言数组，字段：id、activityId、userName、content、createTime
3. 报名模块
3.1 报名活动
请求方式：POST
接口地址：/Enroll/DoEnroll
权限：登录用户，活动处于 "报名中"，未报名、人数未满
请求参数
oactivityId（int，必填）：正式活动 ID
返回：{"code":200,"msg":"报名成功","data":null}
3.2 取消报名
请求方式：POST
接口地址：/Enroll/CancelEnroll
权限：登录用户，仅自己的报名
请求参数
oactivityId（int，必填）：正式活动 ID
返回：{"code":200,"msg":"取消成功","data":null}
4. 签到模块
4.1 生成签到码
请求方式：POST
接口地址：/SignIn/GenerateSignInCode
权限：活动发布者
请求参数
oactivityId（int，必填）：正式活动 ID
返回：{"code":200,"msg":"生成成功","data":"abcdef1234"}（10 位随机码，10 分钟有效）
4.2 扫码签到
请求方式：POST
接口地址：/SignIn/ScanSignIn
权限：登录用户，已报名、活动 "进行中"、未签到
请求参数
ocode（string，必填，10 位）：签到码
返回：{"code":200,"msg":"签到成功","data":null}
4.3 获取签到名单
请求方式：GET
接口地址：/SignIn/GetSignInList?activityId=1
权限：活动发布者
请求参数：activityId（int，必填）
返回：签到数组，字段：id、userName、activityId、signInTime