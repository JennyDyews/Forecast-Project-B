using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json; //Requires nuget package System.Net.Http.Json
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text.Json;

using Weather.Models;

namespace Weather.Services
{
    public class OpenWeatherService
    {
        public EventHandler<string> WeatherForecastAvailable;
        HttpClient httpClient = new HttpClient();
        readonly string apiKey = "f4604e9e8a94b2bf9f4e1bc2fe7f0b56"; // Your API Key

        //part of your event code here
        public async Task<Forecast> GetForecastAsync(string City)
        {
            //https://openweathermap.org/current
            var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var uri = $"https://api.openweathermap.org/data/2.5/forecast?q={City}&units=metric&lang={language}&appid={apiKey}";
            //var uri = $"http://maps.openweathermap.org/maps/2.0/weather/TA2/{City}?date=1527811200&opacity=0.9&fill_bound=true&palette=0:FF0000;10:00FF00;20:0000FF&appid={apiKey}";

            Forecast forecast = await ReadWebApiAsync(uri);

            //part of your event code here
            OnWeatherForecastAvailable($"New weather forecast for {City} available");


            return forecast;

        }

        protected virtual void OnWeatherForecastAvailable(string s)
        {
            WeatherForecastAvailable?.Invoke(this, s);
        }
        public async Task<Forecast> GetForecastAsync(double latitude, double longitude)
        {
            //https://openweathermap.org/current
            var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var uri = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude}&lon={longitude}&units=metric&lang={language}&appid={apiKey}";

            Forecast forecast = await ReadWebApiAsync(uri);

            //part of your event code here
            OnWeatherForecastAvailable($"New weather forecast for ({latitude}, {longitude}) available");

            return forecast;
        }
        private async Task<Forecast> ReadWebApiAsync(string uri)
        {
            // part of your read web api code here

            // part of your data transformation to Forecast here

            HttpResponseMessage response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            WeatherApiData wd = await response.Content.ReadFromJsonAsync<WeatherApiData>();
            Forecast forecast = new Forecast();

            forecast.City = wd.city.name;


            forecast.Items = new List<ForecastItem>();

            wd.list.ForEach(wdListItem => { forecast.Items.Add(GetForecastItem(wdListItem)); });

            return forecast;
        }

        private ForecastItem GetForecastItem(List wdListItem)
        {

            ForecastItem item = new ForecastItem();
            item.DateTime = UnixTimeStampToDateTime(wdListItem.dt);

            item.Temperature = wdListItem.main.temp;
            item.Description = wdListItem.weather.Count > 0 ? wdListItem.weather.First().description : "No info";
            item.WindSpeed = wdListItem.wind.speed;
            item.Icon = wdListItem.weather.Count > 0 ? wdListItem.weather.First().icon : "No info";

            return item;
        }
        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }

}