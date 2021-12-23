//using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

using PatchHanlde.JsonPatch;

namespace PatchHanlde.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }


        [HttpPatch("{id:int}")]
        public IActionResult PartiallyUpdateWeatherForecest(int id, [FromBody] Operation[] operations)
        {
            //if (patchDoc is null)
            //    return BadRequest("patchDoc object sent from client is null.");

            // simulate get from db with ID == id
            WeatherForecast weatherForecast = new WeatherForecast()
            {
                Date = DateTime.Now,
                TemperatureC = 10,
                Summary = "Summary",
                Numbers = new int[] { 1, 2, },
            };

            weatherForecast.Linked.Add(new WeatherForecast
            {
                Date = DateTime.Now,
                TemperatureC = 11,
                Summary = "Summary2",
                Numbers = new int[] { 3, 4, },
            });

            weatherForecast.Linked.Add(new WeatherForecast
            {
                Date = DateTime.Now,
                TemperatureC = 12,
                Summary = "Summary3",
                Numbers = new int[] { 5, 6, },
            });

            foreach (var operation in operations)
            {
                operation.Patch(weatherForecast);
            }

            //patchDoc.ApplyTo(weatherForecast);

            return NoContent();
        }
    }



}