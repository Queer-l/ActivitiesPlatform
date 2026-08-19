using Microsoft.AspNetCore.Mvc;

namespace CampusActivityApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    /// <summary>
    /// ASP.NET Core 模板自带的天气示例接口。
    /// </summary>
    /// <remarks>
    /// 当前校园活动业务不依赖该控制器，保留它仅用于接口模板示例。
    /// </remarks>
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        /// <summary>
        /// 创建天气示例控制器。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取五天随机天气示例数据。
        /// </summary>
        /// <returns>天气示例集合。</returns>
        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
