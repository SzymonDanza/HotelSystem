using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class CurrencyService
{
    private readonly HttpClient _httpClient;

    public CurrencyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExchangeRates> GetExchangeRatesAsync(string baseCurrency)
    {
        try
        {
            // Przykład URL API (zmień na odpowiedni)
            var url = $"https://api.exchangerate-api.com/v4/latest/{baseCurrency}";
            var response = await _httpClient.GetStringAsync(url);

            // Zwrócenie deserializowanych danych z API
            var rates = JsonConvert.DeserializeObject<ExchangeRates>(response);
            return rates;
        }
        catch (Exception ex)
        {
            // Logowanie błędu, jeżeli API nie jest dostępne
            Console.WriteLine($"Błąd podczas pobierania kursów walut: {ex.Message}");
            return null;
        }
    }
}

public class ExchangeRates
{
    public string Base { get; set; }
    public Dictionary<string, decimal> Rates { get; set; }
}
