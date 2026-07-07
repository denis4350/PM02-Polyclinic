using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace HospitalSystem.Services
{
    public class ApiService
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<string> GetCurrencyRatesAsync()
        {
            string url = "https://www.cbr-xml-daily.ru/daily_json.js";

            string rawJson = await client.GetStringAsync(url);
            var parsed = JObject.Parse(rawJson);

            var valute = parsed["Valute"];
            string date = parsed["Date"]?.ToString();

            decimal usd = valute?["USD"]?["Value"]?.Value<decimal>() ?? 0;
            decimal eur = valute?["EUR"]?["Value"]?.Value<decimal>() ?? 0;

            var ru = new CultureInfo("ru-RU");
            string usdText = usd.ToString("0.00", ru);
            string eurText = eur.ToString("0.00", ru);

            string dateText = DateTime.TryParse(date, out var parsedDate)
                ? parsedDate.ToString("dd.MM.yyyy")
                : "";

            return $"Курс ЦБ РФ на {dateText}: доллар США — {usdText} ₽, евро — {eurText} ₽";
        }
    }
}