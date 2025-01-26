using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HotelSystem.Services
{
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "f5ca8c7c2531e5b9147f67dc";  // Twój klucz API

        // Konstruktor
        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Pobieranie kursów walut
        public async Task<CurrencyRates> GetExchangeRatesAsync(string baseCurrency)
        {
            try
            {
                var url = $"https://v6.exchangerate-api.com/v6/{_apiKey}/latest/{baseCurrency}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserializuj odpowiedź do nowego modelu
                var rates = JsonConvert.DeserializeObject<CurrencyRates>(responseContent);

                return rates;
            }
            catch
            {
                return null;
            }
        }
    }

    // Model odpowiedzi API
    public class CurrencyRates
    {
        public string Base { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
