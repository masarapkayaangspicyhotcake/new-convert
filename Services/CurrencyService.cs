

using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class CurrencyService
{
    private readonly HttpClient _http;
    private readonly ILogger<CurrencyService> _logger;
    private const string list_key = "441fd97157a1a495ff9b711ccadd38d4";
    private const string conversion_key = "811ecfc3cf3c1fefbd2794b513fd5266";

    public CurrencyService(HttpClient http, ILogger<CurrencyService> logger)
    {
        _http = http;
        _logger = logger; 
    }

   public async Task<decimal?> GetHistoricalRateAsync(string baseCurrency, string targetCurrency, string date, decimal amount = 1m)
    {
        string url = $"https://api.exchangerate.host/convert?access_key={conversion_key}&from={baseCurrency}&to={targetCurrency}&amount={amount}&date={date}";
        
        try
        {
            _logger.LogInformation("Making request to: {Url}", url);
            
            var response = await _http.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("API response: {Json}", json);
            
            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(json);
                
                if (doc.RootElement.TryGetProperty("success", out var success) && success.GetBoolean())
                {
                    if (doc.RootElement.TryGetProperty("result", out var result))
                    {
                        return result.GetDecimal();
                    }
                    else
                    {
                        _logger.LogWarning("No 'result' field found in successful API response.");
                    }
                }
                else
                {
                    if (doc.RootElement.TryGetProperty("error", out var error))
                    {
                        _logger.LogError("API returned error: {Error}", error.ToString());
                    }
                    else
                    {
                        _logger.LogError("API returned success=false but no error details");
                    }
                }
            }
            else
            {
                _logger.LogError("HTTP request failed with status: {StatusCode}, Response: {Response}", 
                    response.StatusCode, json);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching rate for {From} to {To} on {Date}", 
                baseCurrency, targetCurrency, date);
        }
        
        return null;
    }
    public async Task<Dictionary<string, string>> GetSupportedCurrenciesAsync()
    {
        string url = $"https://api.exchangerate.host/list?access_key={list_key}";

        try
        {
            var response = await _http.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("currencies", out var currencies))
                {
                    var currencyList = new Dictionary<string, string>();

                    foreach (var currency in currencies.EnumerateObject())
                    {
                        currencyList[currency.Name] = currency.Value.GetString() ?? currency.Name;
                    }

                    return currencyList;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching currencies: {ex.Message}");
        }

        return new Dictionary<string, string>();
    }
}