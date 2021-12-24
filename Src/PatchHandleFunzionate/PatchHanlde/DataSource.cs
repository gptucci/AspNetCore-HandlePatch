namespace PatchHanlde
{
    public interface IDataSource
    {
        List<WeatherForecast> WeatherForecastList { get;  }
    }

    public class DataSource : IDataSource
    {
        public DataSource()
        {
            InitializeWeatherForecastList();
        }

        public List<WeatherForecast> WeatherForecastList { get;  }=new List<WeatherForecast>();

        private void InitializeWeatherForecastList()
        {
            for (int i = 0; i < 100; i++)
            {
                WeatherForecast weatherForecast = new WeatherForecast()
                {
                    Id= i,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    TemperatureC = 10,
                    Summary = $"Summary{i}",
                    Numbers = new int[] { 1, 2, },
                };

                weatherForecast.Linked.Add(new WeatherForecast
                {
                    Id = i+1000,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    TemperatureC = 11,
                    Summary = $"Linked01-{i}",
                    Numbers = new int[] { 3, 4, },
                });

                weatherForecast.Linked.Add(new WeatherForecast
                {
                    Id=i+2000,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    TemperatureC = 12,
                    Summary = $"Linked02-{i}",
                    Numbers = new int[] { 5, 6, },
                });

                WeatherForecastList.Add(weatherForecast);
            }
        }
    }
}