global using mvcDapper3.Models;
global using mvcDapper3.AppCodes.AppService;
global using mvcDapper3.AppCodes.AppBase;
global using mvcDapper3.AppCodes.AppInterface;
global using mvcDapper3.AppCodes.AppRepo;
global using mvcDapper3.AppCodes.AppClass;

global using Dapper;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Rendering;
global using Microsoft.Data.SqlClient;
global using Microsoft.EntityFrameworkCore;
global using System.Data;
global using System.Globalization;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using X.PagedList;
global using X.PagedList.Extensions;
global using Serilog;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Scalar.AspNetCore;
using mvcDapper3.AppCodes.AppMiddleware;
using Microsoft.AspNetCore.HttpOverrides;

using Serilog.Events;
using Serilog.Filters;
using Serilog.Settings.Configuration;

var builder = WebApplication.CreateBuilder(args);

#region Serilog設定
// Create new logger
Log.Logger = new LoggerConfiguration() 
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
// Register the logger
builder.Host.UseSerilog();
#endregion

#region DI 注入設定
builder.Services.AddScoped<IConnFactory, ConnFactory>();
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IAuthRepo, AuthRepo>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICrypto, CryptographyService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<CaptchaService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<JWTServices>();
builder.Services.AddScoped<ISocialLoginService, SocialLoginService>();

// Add HttpClient for social login services
builder.Services.AddHttpClient();

builder.Services.AddControllersWithViews()
    .AddDataAnnotationsLocalization(); // 啟用讀取metadata類別

// Configure Antiforgery for OAuth scenarios
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddAutoMapper(typeof(Program).Assembly);

#endregion

#region Reverse Proxy Configuration
// Configure forwarded headers for reverse proxy scenarios (like Cloudflare tunnel)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Accept forwarded headers from any source (for Cloudflare tunnel)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
#endregion

#region Controller設定
builder.Services.AddControllers
    (options =>{options.RespectBrowserAcceptHeader = true;});
builder.Services.AddRazorPages();
#endregion

#region 解決 Json 格式中文亂碼問題
builder.Services.AddRazorPages()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.Encoder =
            JavaScriptEncoder.Create(UnicodeRanges.BasicLatin,
            UnicodeRanges.CjkUnifiedIdeographs);
    });
#endregion

#region 環境設定檔設定
var cwd = AppDomain.CurrentDomain.BaseDirectory;
var env = builder.Environment.EnvironmentName;

builder.Configuration
    .SetBasePath(cwd)
    // .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
#endregion

#region OpenAPI 設定
builder.Services.AddEndpointsApiExplorer(); // ApiExplorer
builder.Services.AddOpenApi();              // Scalar UI
// builder.Services.AddApiDescription();
#endregion

#region Session設定
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
// 設定 Session 參數值
builder.Services.AddSession(options =>
    {
        // 設定 Session 過期時間, 單位為分鐘
        options.IdleTimeout = TimeSpan.FromMinutes(20);
        // Use SameAsRequest for reverse proxy scenarios - will be secure if forwarded as HTTPS
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Name = "mvcDapper3";
        // 表示此 Cookie 限伺服器讀取設定，document.cookie 無法存取
        options.Cookie.HttpOnly = true;
        // Allow cross-site cookies for reverse proxy scenarios
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

//enable the session-based TempData provider
builder.Services.AddRazorPages().AddSessionStateTempDataProvider();
builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
#endregion

#region WebAPI JWT 設定
var str_issuer = builder.Configuration.GetSection("JwtSettings")
    .GetValue<string>("Issuer") ?? "mvcDapper3";
var str_audience = builder.Configuration.GetSection("JwtSettings")
    .GetValue<string>("Audience") ?? "mvcDapper3";
var str_signing_key = builder.Configuration.GetSection("JwtSettings")
    .GetValue<string>("SignKey") ?? "n$8U7nT9oDUrYM7BRsTe*mSc1##zHS5Q";

builder.Services.AddAuthentication(
    options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true; // 當驗證失敗時，會顯示失敗的詳細錯誤原因
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 配置驗證發行者
            ValidateIssuer = true, // 是否要啟用驗證發行者
            ValidIssuer = str_issuer, // 簽發者
                                      // 配置驗證接收者
                                      // (由於一般沒有區分特別對象，因此通常不太需要設定，也不太需要驗證)
            ValidateAudience = false, //是否要啟用驗證接收者
            ValidAudience = str_audience, // 接收者
                                          // 是否要啟用驗證有效時間
            ValidateLifetime = true,
            // 是否要啟用驗證金鑰
            ValidateIssuerSigningKey = true,
            // 配置簽章驗證用金鑰
            // 這裡配置是用來解Http Request內Token加密
            // 如果Secret Key跟當初建立Token所使用的Secret Key不一樣的話會導致驗證失敗
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(str_signing_key)
            ),
            // 是不是有過期時間
            RequireExpirationTime = true,
            //時間偏移量（允許誤差時間）
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // Enable reading the token from the cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("jwtToken"))
                {
                    context.Token = context.Request.Cookies["jwtToken"];
                }
                return Task.CompletedTask;
            }
        };
    });
#endregion


var app = builder.Build();

// Configure forwarded headers for reverse proxy (must be first)
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    #region OpenAPI 設定
    app.MapOpenApi();
    app.MapScalarApiReference();// use Scalar UI
    // app.UseApiDescription();
    #endregion
}

// Conditional HTTPS redirection - only redirect if not behind a reverse proxy
app.Use(async (context, next) =>
{
    // Check if request is coming through a reverse proxy with HTTPS
    var forwardedProto = context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
    var isHttpsForwarded = string.Equals(forwardedProto, "https", StringComparison.OrdinalIgnoreCase);
    
    // Only redirect to HTTPS if not already HTTPS and not forwarded as HTTPS
    if (!context.Request.IsHttps && !isHttpsForwarded && !app.Environment.IsDevelopment())
    {
        var httpsUrl = $"https://{context.Request.Host}{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";
        context.Response.Redirect(httpsUrl, permanent: true);
        return;
    }
    
    await next();
});

app.UseRouting();

#region 設定使用 Session
app.UseSession();
#endregion

// Use the JWT cookie middleware to extract the JWT token from the cookie
app.UseJwtCookieMiddleware();

// Use the authentication redirect middleware to redirect unauthenticated users to login page
app.UseAuthenticationRedirect();

app.UseAuthentication();
app.UseAuthorization();

// app.MapStaticAssets();
app.UseStaticFiles();


app.MapAreaControllerRoute(
    name: "AdminArea",
    areaName: "Admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// .WithStaticAssets();

#region #使用SeriLog-輸出網址到console，字串有符合正規式的話就會開啟網頁
try
{
    Log.Information("--> Build application");
    Log.Information("--> Application Start");
    Log.Information("--> ENV: [" + env + "]");
    // Log.Information("Now listening on: https://localhost:7890");
    // Log.Information("Now listening on: http://localhost:5890");
}
catch (Exception e)
{
    Log.Fatal(e, "Application terminated unexpectedly");
}
#endregion

SqlMapper.AddTypeHandler(new DateTimeHandler());

app.Run();
