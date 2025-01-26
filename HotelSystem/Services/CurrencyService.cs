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
           
            var url = $"https://api.exchangerate-api.com/v4/latest/{baseCurrency}";
            var response = await _httpClient.GetStringAsync(url);

            
            var rates = JsonConvert.DeserializeObject<ExchangeRates>(response);
            return rates;
        }
        catch (Exception ex)
        {
            
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


