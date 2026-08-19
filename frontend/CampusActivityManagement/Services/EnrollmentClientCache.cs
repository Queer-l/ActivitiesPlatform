using System.Text.Json;
using Microsoft.JSInterop;

namespace CampusActivityManagement.Services;

/// <summary>
/// 当前用户报名状态的浏览器本地缓存。
/// 后端当前只提供报名/取消报名接口，未提供“查询当前用户是否已报名”的接口，因此详情页用该缓存驱动签到区域显隐。
/// </summary>
public class EnrollmentClientCache
{
    /// <summary>localStorage 中保存报名状态记录的键名。</summary>
    private const string StorageKey = "activityEnrollments";

    /// <summary>用于读写浏览器 localStorage 的 JSInterop 入口。</summary>
    private readonly IJSRuntime _jsRuntime;

    /// <summary>兼容历史缓存字段大小写差异，避免前端升级后无法读取旧报名记录。</summary>
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EnrollmentClientCache(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// 查询当前用户是否已报名某活动。
    /// </summary>
    public async Task<bool> IsEnrolledAsync(string userKey, int activityId)
    {
        // userKey 可能是数字用户 ID，也可能是用户名；比较逻辑集中在 IsSameRecord 中处理。
        var records = await GetRecordsAsync();
        return records.Any(r => IsSameRecord(r, userKey, activityId));
    }

    /// <summary>
    /// 报名成功后写入缓存。
    /// </summary>
    public async Task MarkEnrolledAsync(string userKey, int activityId)
    {
        var records = await GetRecordsAsync();

        // 同一用户同一活动只保存一条记录，避免重复点击报名或重复接口回调导致缓存膨胀。
        if (!records.Any(r => IsSameRecord(r, userKey, activityId)))
        {
            records.Add(new EnrollmentRecord(userKey, activityId));
            await SaveAsync(records);
        }
    }

    /// <summary>
    /// 取消报名成功后移除缓存。
    /// </summary>
    public async Task MarkCanceledAsync(string userKey, int activityId)
    {
        var records = await GetRecordsAsync();

        // 取消报名后移除匹配记录；如果没有找到匹配项，就不写回 localStorage，减少无意义写入。
        var updated = records
            .Where(r => !IsSameRecord(r, userKey, activityId))
            .ToList();

        if (updated.Count != records.Count)
        {
            await SaveAsync(updated);
        }
    }

    private async Task<List<EnrollmentRecord>> GetRecordsAsync()
    {
        // localStorage 空值表示当前浏览器还没有任何报名状态缓存。
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<EnrollmentRecord>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<EnrollmentRecord>>(json, _jsonOptions) ?? new List<EnrollmentRecord>();
        }
        catch (JsonException)
        {
            // 报名缓存只是前端辅助状态，损坏时直接清理。
            // 后续报名/取消报名操作会重新写入正确状态。
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            return new List<EnrollmentRecord>();
        }
    }

    private async Task SaveAsync(List<EnrollmentRecord> records)
    {
        // 报名状态记录很小，直接整表序列化写回即可，避免引入更复杂的局部更新逻辑。
        var json = JsonSerializer.Serialize(records, _jsonOptions);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    private static bool IsSameRecord(EnrollmentRecord record, string userKey, int activityId)
    {
        // 用户名大小写差异不应该导致同一个报名状态被识别成两条记录。
        return record.ActivityId == activityId
            && string.Equals(record.UserKey, userKey, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// localStorage 中的最小报名状态单元。
    /// 只保存“谁报名了哪个活动”，其它活动详情仍以服务端返回为准。
    /// </summary>
    private sealed record EnrollmentRecord(string UserKey, int ActivityId);
}
