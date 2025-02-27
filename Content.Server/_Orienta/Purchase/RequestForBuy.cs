using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Content.Shared.Backmen.CCVar;
using Content.Shared.FixedPoint;
using Robust.Shared.Configuration;
using Robust.Shared.IoC;
using Robust.Shared.Log;

namespace Content.Server._Orienta.Purchase
{
    public sealed class ResponseFromBot
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public float NewBalance { get; set; }
        public float? Balance { get; set; }
    }

    public sealed class RequestForBuy : EntitySystem
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        private static readonly HttpClient HttpClient = new();
        private string _apiUrl = string.Empty;
        private string _apiKey = string.Empty;
        private bool _purchasesEnabled = false;
        private readonly ISawmill _log = Logger.GetSawmill("orientik");

        public override void Initialize()
        {
            base.Initialize();
            _cfg.OnValueChanged(CCVars.PurchaseApiEnabled, v => _purchasesEnabled = v, true);
            _cfg.OnValueChanged(CCVars.PurchaseApiUrl, v => _apiUrl = v, true);
            _cfg.OnValueChanged(CCVars.PurchaseApiKey, v => _apiKey = v, true);
        }

        public async Task<(bool Success, string Message, float NewBalance)> SendPurchaseRequest(string ckey, int price)
        {
            if (!_purchasesEnabled)
                return (false, "В config.toml отключены покупки", 0f);

            if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(_apiKey))
                return (false, "APIUrl или APIKey не настроены в config.toml", 0f);

            var payload = new { ckey = ckey, price = price };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = content;

            try
            {
                var response = await HttpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var botResponse = JsonSerializer.Deserialize<ResponseFromBot>(responseContent);

                if (botResponse != null && botResponse.Success)
                {
                    return (true, botResponse.Message ?? "Покупка успешна", botResponse.NewBalance);
                }
                return (false, botResponse?.Message ?? "Неизвестная ошибка", 0f);
            }
            catch (Exception ex)
            {
                _log.Error($"Ошибка при отправке запроса: {ex.Message}");
                return (false, "Ошибка соединения с ботом", 0f);
            }
        }

        public async Task<(bool Success, string Message, float Balance)> GetBalance(string ckey)
        {
            if (!_purchasesEnabled)
                return (false, "В config.toml отключены покупки", 0f);

            if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(_apiKey))
                return (false, "APIUrl или APIKey не настроены в config.toml", 0f);

            var payload = new { ckey = ckey };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/balance");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = content;

            try
            {
                var response = await HttpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var botResponse = JsonSerializer.Deserialize<ResponseFromBot>(responseContent);

                if (botResponse != null && botResponse.Success)
                {
                    return (true, "Баланс получен", botResponse.Balance ?? botResponse.NewBalance);
                }
                return (false, botResponse?.Message ?? "Неизвестная ошибка", 0f);
            }
            catch (Exception ex)
            {
                _log.Error($"Ошибка при запросе баланса: {ex.Message}");
                return (false, "Ошибка соединения с ботом", 0f);
            }
        }
    }
}
