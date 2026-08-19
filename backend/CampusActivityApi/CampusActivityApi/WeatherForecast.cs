namespace CampusActivityApi;

/// <summary>
/// ASP.NET Core 模板生成的天气示例模型。
/// </summary>
/// <remarks>
/// 当前业务接口没有依赖该模型，可保留用于模板示例接口。
/// </remarks>
public class WeatherForecast
{
    /// <summary>
    /// 预报日期。
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// 摄氏温度。
    /// </summary>
    public int TemperatureC { get; set; }

    /// <summary>
    /// 华氏温度，根据摄氏温度计算。
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    /// <summary>
    /// 天气概要描述。
    /// </summary>
    public string? Summary { get; set; }
}
