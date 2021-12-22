using Microsoft.AspNetCore.Mvc;

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


        //[HttpPatch("{id:guid}")]
        //public IActionResult PartiallyUpdateEmployeeForCompany( Guid id, [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        //{
        //    //if (patchDoc is null)
        //    //    return BadRequest("patchDoc object sent from client is null.");
        //    //var result = _service.EmployeeService.GetEmployeeForPatch(companyId, id, compTrackChanges: false, empTrackChanges: true);
        //    //patchDoc.ApplyTo(result.employeeToPatch);
        //    //_service.EmployeeService.SaveChangesForPatch(result.employeeToPatch, result.employeeEntity);
        //    return NoContent();
        //}
    }



}