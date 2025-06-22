using System.Text;
using Newtonsoft.Json;
using Serilog;

namespace mvcDapper3.AppCodes.AppService;

public interface ISocialLoginService
{
    string GetGoogleAuthUrl(string state);
    Task<GoogleUserInfo> GetGoogleUserInfoAsync(string code);
    string GenerateState();
    bool ValidateState(string state, string sessionState);
}

public class GoogleUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public bool VerifiedEmail { get; set; }
}

public class SocialLoginService : ISocialLoginService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISessionService _sessionService;

    public SocialLoginService(
        IConfiguration configuration, 
        IHttpClientFactory httpClientFactory,
        ISessionService sessionService)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _sessionService = sessionService;
    }

    public string GetGoogleAuthUrl(string state)
    {
        try
        {
            var clientId = _configuration["Authentication:Google:ClientId"];
            var redirectUri = _configuration["AppSettings:BaseUrl"] + "/Account/GoogleCallback";
            
            var authUrl = "https://accounts.google.com/o/oauth2/v2/auth" +
                         $"?client_id={Uri.EscapeDataString(clientId)}" +
                         $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                         "&response_type=code" +
                         "&scope=openid%20email%20profile" +
                         $"&state={Uri.EscapeDataString(state)}";

            Log.Information("Generated Google OAuth URL for state: {State}", state);
            return authUrl;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error generating Google OAuth URL");
            throw;
        }
    }

    public async Task<GoogleUserInfo> GetGoogleUserInfoAsync(string code)
    {
        try
        {
            var clientId = _configuration["Authentication:Google:ClientId"];
            var clientSecret = _configuration["Authentication:Google:ClientSecret"];
            var redirectUri = _configuration["AppSettings:BaseUrl"] + "/Account/GoogleCallback";

            // Exchange code for access token
            var tokenRequest = new Dictionary<string, string>
            {
                {"client_id", clientId},
                {"client_secret", clientSecret},
                {"code", code},
                {"grant_type", "authorization_code"},
                {"redirect_uri", redirectUri}
            };

            using var httpClient = _httpClientFactory.CreateClient();
            
            var tokenContent = new FormUrlEncodedContent(tokenRequest);
            var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenContent);
            
            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                Log.Error("Google token exchange failed: {StatusCode} - {Content}", 
                    tokenResponse.StatusCode, errorContent);
                throw new Exception($"Google token exchange failed: {tokenResponse.StatusCode}");
            }

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<dynamic>(tokenJson);
            var accessToken = tokenData?.access_token?.ToString();

            if (string.IsNullOrEmpty(accessToken))
            {
                Log.Error("No access token received from Google");
                throw new Exception("No access token received from Google");
            }

            // Get user info using access token
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var userInfoResponse = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
            
            if (!userInfoResponse.IsSuccessStatusCode)
            {
                var errorContent = await userInfoResponse.Content.ReadAsStringAsync();
                Log.Error("Google user info request failed: {StatusCode} - {Content}", 
                    userInfoResponse.StatusCode, errorContent);
                throw new Exception($"Google user info request failed: {userInfoResponse.StatusCode}");
            }

            var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();
            var userInfo = JsonConvert.DeserializeObject<GoogleUserInfo>(userInfoJson);

            if (userInfo == null)
            {
                Log.Error("Failed to deserialize Google user info");
                throw new Exception("Failed to deserialize Google user info");
            }

            Log.Information("Successfully retrieved Google user info for: {Email}", userInfo.Email);
            return userInfo;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting Google user info for code: {Code}", code);
            throw;
        }
    }

    public string GenerateState()
    {
        var state = Guid.NewGuid().ToString("N");
        Log.Information("Generated OAuth state: {State}", state);
        return state;
    }

    public bool ValidateState(string state, string sessionState)
    {
        var isValid = !string.IsNullOrEmpty(state) && 
                     !string.IsNullOrEmpty(sessionState) && 
                     state == sessionState;
        
        Log.Information("State validation result: {IsValid} (provided: {State}, session: {SessionState})", 
            isValid, state, sessionState);
        
        return isValid;
    }
}
