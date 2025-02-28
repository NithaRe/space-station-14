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
            _cfg.OnValueChanged(CCVars.OrientikApiEnabled, v => _purchasesEnabled = v, true);
            _cfg.OnValueChanged(CCVars.OrientikApiUrl, v => _apiUrl = v, true);
            _cfg.OnValueChanged(CCVars.OrientikApiKey, v => _apiKey = v, true);
        }

        public async Task<(bool success, string message, int newBalance)> SendPurchaseRequest(string ckey, int price)
        {
            if (!_purchasesEnabled)
                return (false, "В config.toml отключены покупки", 0);

            if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(_apiKey))
                return (false, "APIUrl или APIKey не настроены в config.toml", 0);

            var payload = new { ckey = ckey, price = price };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/purchases");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = content;

            try
            {
                var response = await HttpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var botResponse = JsonSerializer.Deserialize<ResponseFromBot>(responseContent);

                if (botResponse != null && botResponse.success)
                {
                    return (true, botResponse.message ?? "Покупка успешна", botResponse.newBalance);
                }
                return (false, botResponse?.message ?? "Неизвестная ошибка", 0);
            }
            catch (Exception ex)
            {
                _log.Error($"Ошибка при отправке запроса: {ex.Message}");
                return (false, "Ошибка соединения с ботом", 0);
            }
        }

        public async Task<(bool success, string message, int balance)> GetBalance(string ckey)
        {
            if (!_purchasesEnabled)
                return (false, "В config.toml отключены покупки", 0);

            if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(_apiKey))
                return (false, "APIUrl или APIKey не настроены в config.toml", 0);

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}/balance?ckey={Uri.EscapeDataString(ckey)}");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            try
            {
                var response = await HttpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var botResponse = JsonSerializer.Deserialize<ResponseFromBot>(responseContent);

                if (botResponse != null && botResponse.success)
                {
                    return (true, "Баланс получен", botResponse.balance ?? botResponse.newBalance);
                }
                return (false, botResponse?.message ?? "Неизвестная ошибка", 0);
            }
            catch (Exception ex)
            {
                _log.Error($"Ошибка при запросе баланса: {ex.Message}");
                return (false, "Ошибка соединения с ботом", 0);
            }
        }

        public sealed class ResponseFromBot
        {
            public bool success { get; set; }
            public string? message { get; set; }
            public int newBalance { get; set; }
            public int? balance { get; set; }
        }
    }
}
