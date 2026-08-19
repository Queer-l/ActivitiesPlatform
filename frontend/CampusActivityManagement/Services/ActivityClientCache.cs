using System.Text.Json;
using Microsoft.JSInterop;
using CampusActivityManagement.Models;

namespace CampusActivityManagement.Services;

/// <summary>
/// “我的活动”页面使用的浏览器本地缓存。
/// 后端提交活动申请后暂不返回新活动 ID，因此前端先把申请写入 localStorage，保证用户提交后能立即在本机看到记录。
/// </summary>
public class ActivityClientCache
{
    /// <summary>localStorage 中保存活动草稿/本地记录的键名。</summary>
    private const string StorageKey = "myActivityRecords";

    /// <summary>
    /// 通过 JSInterop 访问浏览器 localStorage。
    /// Blazor WebAssembly 中没有直接的 .NET localStorage API，因此统一从这里桥接浏览器存储。
    /// </summary>
    private readonly IJSRuntime _jsRuntime;

    /// <summary>
    /// 本地缓存可能由不同版本前端写入，属性大小写不一定完全一致。
    /// 开启大小写不敏感可以降低升级后读旧缓存失败的概率。
    /// </summary>
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ActivityClientCache(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// 读取本地活动记录。
    /// 如果缓存内容损坏，会主动清理，避免页面反复解析失败。
    /// </summary>
    public async Task<List<ActivityDto>> GetActivitiesAsync()
    {
        // localStorage 中只保存 JSON 字符串；没有记录时统一返回空集合，调用方不需要反复判空。
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<ActivityDto>();
        }

        try
        {
            // 缓存结构对应 ActivityDto 列表；反序列化返回 null 时也按空集合处理。
            return JsonSerializer.Deserialize<List<ActivityDto>>(json, _jsonOptions) ?? new List<ActivityDto>();
        }
        catch (JsonException)
        {
            // 本地缓存不是权威数据源，损坏后清掉比保留错误数据更安全。
            // 页面后续仍会从服务端接口重新获取可见活动。
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            return new List<ActivityDto>();
        }
    }

    /// <summary>
    /// 新增或更新一条本地活动记录。
    /// 有后端 ID 的记录按 ID 合并；没有后端 ID 的草稿会作为独立记录保存。
    /// </summary>
    public async Task UpsertAsync(ActivityDto activity)
    {
        var activities = await GetActivitiesAsync();

        // 已经拿到后端真实 ID 的活动用 ID 合并，避免同一活动在“我的活动”中重复出现。
        // Id 为 0 或负数的本地草稿不会和真实活动直接按 ID 合并，后续页面会用业务键去重。
        var index = activities.FindIndex(a => a.Id == activity.Id && activity.Id != 0);

        if (index >= 0)
        {
            activities[index] = activity;
        }
        else
        {
            activities.Add(activity);
        }

        await SaveAsync(activities);
    }

    /// <summary>
    /// 活动申请提交成功后，创建一条“待审核”的本地记录。
    /// 负数 ID 只在前端本地使用，后端真正返回活动后会按标题、发布者和开始时间移除草稿。
    /// </summary>
    public async Task UpsertSubmittedApplyAsync(ActivityApplyRequest request)
    {
        // ApplyActivity 提交后不返回新活动 ID。
        // 这里用标题、发布者、开始时间生成一个稳定的负数 ID，既能在本地列表中区分记录，
        // 也能避免和后端真实的正数 ID 冲突。
        var pendingActivity = new ActivityDto
        {
            Id = -Math.Abs(HashCode.Combine(request.Title, request.PublisherUserName, request.ActivityStartTime)),
            Title = request.Title,
            Content = request.Content,
            PublisherUserName = request.PublisherUserName,
            AuditStatus = 0,
            AuditStatusText = "待审核",
            StatusText = "待审核",
            CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            MaxCount = request.MaxCount,
            ActivityStartTime = request.ActivityStartTime,
            ActivityEndTime = request.ActivityEndTime,
            EnrollStartTime = request.EnrollStartTime,
            EnrollEndTime = request.EnrollEndTime
        };

        await UpsertAsync(pendingActivity);
    }

    /// <summary>
    /// 活动被发布者撤销/下架后，把本地记录标记为已撤销，便于“我的活动”继续展示历史状态。
    /// </summary>
    public async Task MarkTakenDownAsync(ActivityDto activity)
    {
        // 下架后的活动可能已经不再出现在普通活动列表里。
        // 写入本地缓存后，“我的活动”仍能显示这条历史记录和它的最终状态。
        activity.StatusText = "已撤销";
        activity.AuditStatusText = "已撤销";
        await UpsertAsync(activity);
    }

    /// <summary>把活动集合序列化回 localStorage。</summary>
    private async Task SaveAsync(List<ActivityDto> activities)
    {
        // 每次保存都覆盖整个列表，逻辑简单且数据量较小，适合当前“我的活动”缓存场景。
        var json = JsonSerializer.Serialize(activities, _jsonOptions);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }
}
