using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared._Orienta.OCCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Content.Server.Players.PlayTimeTracking;
using System.Text.Json.Serialization;
using Robust.Shared.Utility;
using Content.Shared.Roles;

namespace Content.Server._Orienta.Orienteer;

public sealed class PlayTimeSender
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    private ISawmill _sawmill = default!;

    private readonly HttpClient _httpClient = new();

    private string _apiUrl = string.Empty;
    private string _apiToken = string.Empty;

    public void Initialize()
    {
        _cfg.OnValueChanged(OCCVars.OrienteerApiUrl, v => _apiUrl = v, true);
        _cfg.OnValueChanged(OCCVars.OrienteerApiToken, v => _apiToken = v, true);
        _sawmill = Logger.GetSawmill("orienteer");
    }

    private struct PlayTimeOrienteerDto
    {
        public PlayTimeOrienteerDto()
        {

        }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = "";

        [JsonPropertyName("overall")]
        public double OverAll { get; set; } = 0.0;
    }

    public async Task PlayTimeSend(ICommonSession session, double overall_total_minutes)
    {
        try
        {
            var apiToken = _cfg.GetCVar(OCCVars.OrienteerApiToken);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var response = await _httpClient.PostAsJsonAsync(_apiUrl, new PlayTimeOrienteerDto
            {
                UserId = session.UserId.UserId.ToString(),
                OverAll = overall_total_minutes
            }, cts.Token);

            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _sawmill.Error($"Failed to send playtime data: {ex.Message}");
        }
    }
}
