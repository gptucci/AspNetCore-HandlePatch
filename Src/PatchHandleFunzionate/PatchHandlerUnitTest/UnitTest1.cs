using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PatchHanlde;
using PatchHanlde.Controllers;
using PatchHanlde.JsonPatch;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace PatchHandlerUnitTest
{
    public class UnitTest1
    {
        private readonly WeatherForecastController _controller;
        private readonly IDataSource datasource;

        public UnitTest1()
        {
            datasource= new DataSource();
            var mock = new Mock<ILogger<WeatherForecastController>>();
            ILogger<WeatherForecastController> logger = mock.Object;
            _controller=new WeatherForecastController(logger, datasource);
        }

        [Fact]
        public void TestGetAll()
        {
            var okResult = _controller.Get() as OkObjectResult;

            var items = Assert.IsType<List<WeatherForecast>>(okResult.Value);
        }

        [Fact]
        public void TestSimplePatch01()
        {
            Operation[] operation = { new Operation("replace", "/TemperatureC", NumberToJsonElement(9)) };

            var noContentResult = _controller.PartiallyUpdateWeatherForecest(1, operation) as NoContentResult;

            Assert.NotNull(noContentResult);

            Assert.Equal(datasource.WeatherForecastList.FirstOrDefault(x => x.Id==1).TemperatureC, 9);
        }

        [Fact]
        public void TestPatchLinked01()
        {
            Operation[] operation = { new Operation("replace", "/Linked/0/TemperatureC", NumberToJsonElement(99)) };

            var noContentResult = _controller.PartiallyUpdateWeatherForecest(1, operation) as NoContentResult;

            Assert.NotNull(noContentResult);

            Assert.Equal(datasource.WeatherForecastList.FirstOrDefault(x => x.Id==1).Linked.FirstOrDefault().TemperatureC, 99);
        }

        [Fact]
        public void TestPatchNumber01()
        {
            Operation[] operation = { new Operation("replace", "/Linked/0/Numbers/0", NumberToJsonElement(999)) };

            var noContentResult = _controller.PartiallyUpdateWeatherForecest(1, operation) as NoContentResult;

            Assert.NotNull(noContentResult);

            Assert.Equal(datasource.WeatherForecastList.FirstOrDefault(x => x.Id==1).Linked[0].Numbers[0],999);
        }

        public static JsonElement NumberToJsonElement(int value)
        {
            var abw = new ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(abw);
            writer.WriteNumberValue(value);
            writer.Flush();
            var reader = new Utf8JsonReader(abw.WrittenSpan);
            using var document = JsonDocument.ParseValue(ref reader);
            return document.RootElement.Clone();
        }
    }
}