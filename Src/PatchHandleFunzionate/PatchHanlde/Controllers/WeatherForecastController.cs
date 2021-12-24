//using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

using PatchHanlde.JsonPatch;
using System.Text.Json;

namespace PatchHanlde.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
    //    private static readonly string[] Summaries = new[]
    //    {
    //    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    //};

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDataSource dataSource;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDataSource dataSource)
        {
            _logger = logger;
            this.dataSource=dataSource;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IActionResult Get()
        {
            return Ok(dataSource.WeatherForecastList);
        }


        [HttpPatch("{id:int}")]
        public IActionResult PartiallyUpdateWeatherForecest(int id, [FromBody] Operation[] operations)
        {
            
            //if (patchDoc is null)
            //    return BadRequest("patchDoc object sent from client is null.");

            // simulate get from db with ID == id
            //WeatherForecast weatherForecast = new WeatherForecast()
            //{
            //    Date = DateOnly.FromDateTime(DateTime.Now),
            //    TemperatureC = 10,
            //    Summary = "Summary",
            //    Numbers = new int[] { 1, 2, },
            //};

            //weatherForecast.Linked.Add(new WeatherForecast
            //{
            //    Date = DateOnly.FromDateTime(DateTime.Now),
            //    TemperatureC = 11,
            //    Summary = "Summary2",
            //    Numbers = new int[] { 3, 4, },
            //});

            //weatherForecast.Linked.Add(new WeatherForecast
            //{
            //    Date = DateOnly.FromDateTime(DateTime.Now),
            //    TemperatureC = 12,
            //    Summary = "Summary3",
            //    Numbers = new int[] { 5, 6, },
            //});

            WeatherForecast? weatherForecast = dataSource?.WeatherForecastList?.FirstOrDefault(x => x.Id==id);
            if (weatherForecast==null)
            {

                return NotFound();
            }


            foreach (var operation in operations)
            {
                operation.SetCustomConversionHook(OnCustomConversion, false);
                operation.Patch(weatherForecast);
            }

            //patchDoc.ApplyTo(weatherForecast);

            return NoContent();
        }

        private object OnCustomConversion(Operation operation, Type type)
        {
            if(type == typeof(DateOnly))
            {
                return DateOnly.FromDateTime(operation.Value.GetDateTime());
            }

            throw new Exception($"Custom conversion not supported from type {type.Name}");
        }


        [HttpGet("{id:int}")]
        public IActionResult GetWeatherForecast(int id)
        {
            var WeatherForecast = dataSource?.WeatherForecastList?.FirstOrDefault(x => x.Id==id);

            return Ok(WeatherForecast);
        }
    }



}