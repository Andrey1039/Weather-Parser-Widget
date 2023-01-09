using System;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace Weather
{
    class WeatherData
    {
        public string Temperature { get; }
        public string WeatherStatus { get; }
        public string City { get; }
        public string ImageSourse { get; }
        public Color FontColor { get; }

        public WeatherData(string html)
        {
            this.Temperature = GetTemperature(html);
            this.WeatherStatus = GetWeatherStatus(html);
            this.City = GetCity(html);
            this.ImageSourse = GetImage(html);
            this.FontColor = GetColorFont(html);
        }

        // Определение цвета шрифта
        private Color ColorFont(DateTime sunrise, DateTime sunset) 
        {
            if (DateTime.Now >= sunrise && DateTime.Now <= sunset)
                return Colors.Black;
            else return Colors.White;
        }

        // Получение текущей температуры
        private string GetTemperature(string html)
        {
            string temperature = Regex.Match(html, 
                @"<span data-testid=""TemperatureValue"" class=""CurrentConditions--tempValue--.*?"">(\W*\d+\W)</span>").Groups[1].Value;

            return temperature;
        }

        // Получение текущего погодного статуса
        private string GetWeatherStatus(string html)
        {
            string weatherStatus = Regex.Match(html, 
                @"<div data-testid=""wxPhrase"" class=""CurrentConditions--phraseValue--.*?"">(.*?)</div>").Groups[1].Value;

            return weatherStatus;
        }

        // Получение региона
        private string GetCity(string html)
        {
            string city = Regex.Match(html,
                @"<h1 class=""CurrentConditions--location--.*?>(.*?)</h1>").Groups[1].Value;

            return city;
        }

        // Получение изображения погоды
        private string GetImage(string html)
        {
            string imageSourse = Regex.Match(html, @"style=""background-image:url\((.*?)\)").Groups[1].Value;

            return imageSourse;
        }

        // Получение времени заката и рассвета
        private Color GetColorFont(string html)
        {
            Match time = Regex.Match(html, 
                @"<p class=""SunriseSunset--dateValue--.*?"">(\d+[:]\d+)</p></div>");

            DateTime sunrise, sunset;
            DateTime.TryParse(time.Groups[1].Value, out sunrise);

            time = time.NextMatch();
            DateTime.TryParse(time.Groups[1].Value, out sunset);

            Color colorFont = ColorFont(sunrise, sunset);

            return colorFont;
        }
    }
}
