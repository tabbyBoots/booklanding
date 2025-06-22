using mvcDapper3.Models.ViewModel;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;

namespace mvcDapper3.Controllers;

public class AccountController : BaseController
{
    private readonly IAuthService _authService;
    private readonly ICrypto _crypto;
    private readonly EmailService _emailService;
    private readonly JWTServices _jwtServices;
    private readonly IWebHostEnvironment _env;
    private readonly CaptchaService _captchaService;
    private readonly IConfiguration _config;
    private readonly ISocialLoginService _socialLoginService;

    public AccountController(
        IAuthService authService,
        ICrypto crypto,
        EmailService emailService,
        CaptchaService captchaService,
        JWTServices jwtServices,
        IWebHostEnvironment env,
        IConfiguration config,
        ISocialLoginService socialLoginService)
    {
        _authService = authService;
        _crypto = crypto;
        _emailService = emailService;
        _captchaService = captchaService;
        _jwtServices = jwtServices;
        _env = env;
        _config = config;
        _socialLoginService = socialLoginService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        try
        {
            // Get current user info for logging before clearing session
            var currentUser = HttpContext.User?.Identity?.Name ?? "Unknown";
            
            // Clear all session data
            HttpContext.Session.Clear();
            
            // Remove JWT token cookie with proper security options
            Response.Cookies.Delete("jwtToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });
            
            // Remove any other potential authentication cookies
            Response.Cookies.Delete("AspNetCore.Session", new CookieOptions
            {
                Path = "/"
            });
            
            // Clear any temporary data
            TempData.Clear();
            
            // Add security headers to prevent caching
            Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");
            
            Log.Information("用戶 {UserName} 登出成功，所有 tokens 和 cookies 已清除", currentUser);
            
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "登出過程中發生錯誤");
            // Even if there's an error, still redirect to home page
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public IActionResult LogoutGet()
    {
        // Redirect GET requests to a logout form or directly logout
        // For security, we'll create a simple form that posts to the logout action
        return View("LogoutConfirm");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View(new vmLogin());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(vmLogin model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var sessionCaptcha = GetSessionValue("CaptchaCode");
        if (string.IsNullOrEmpty(sessionCaptcha) || model.CaptchaCode != sessionCaptcha)
        {
            ModelState.AddModelError("CaptchaCode", "驗證碼不正確");
            return View(model);
        }

        var user = await _authService.AuthUserAsync(model.UserNo, model.Password);
        if (user != null)
        {
            try
            {
                var token = _jwtServices.GenerateToken(user);
                
                // Check if request is coming through a reverse proxy with HTTPS
                var forwardedProto = Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
                var isHttpsForwarded = string.Equals(forwardedProto, "https", StringComparison.OrdinalIgnoreCase);
                var isSecure = Request.IsHttps || isHttpsForwarded;
                
                Response.Cookies.Append("jwtToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isSecure,
                    SameSite = SameSiteMode.Lax, // Changed from Strict to Lax for reverse proxy compatibility
                    Expires = DateTime.Now.AddHours(8)
                });

                Log.Information("用戶 {UserNo} 登入成功", user.UserNo);

                if (user.RoleNo == "Admin" || user.RoleNo == "Mis" || user.RoleNo == "User")
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }
                
                return RedirectToAction("Index", "Home");
            }
            catch (ArgumentException ex)
            {
                Log.Error(ex, "JWT Token generation failed for user {UserNo}. User data: UserName={UserName}, RoleNo={RoleNo}", 
                    user.UserNo, user.UserName, user.RoleNo);
                ModelState.AddModelError("", "登入過程中發生錯誤，請聯繫系統管理員");
                return View(model);
            }
        }

        ModelState.AddModelError("", "帳號或密碼不正確");
        return View(model);
    }

    private string GenerateRandomCode()
    {
        return new Random().Next(100000, 999999).ToString();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult CaptchaImage()
    {
        var captchaCode = GenerateRandomCode();
        SetSessionValue("CaptchaCode", captchaCode);

        Log.Information($"Generated new captcha code: {captchaCode}");

        using var image = new Image<Rgba32>(120, 40);
        var font = SystemFonts.CreateFont("Arial", 16);

        image.Mutate(ctx => ctx
            .Fill(Color.White)
            .DrawText(captchaCode, font, Color.Black, new PointF(10, 10)));

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);

        Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, max-age=0");
        Response.Headers.Append("Pragma", "no-cache");
        Response.Headers.Append("Expires", "0");

        return File(ms.ToArray(), "image/png");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RefreshCaptcha()
    {
        try
        {
            var newCode = GenerateRandomCode();
            SetSessionValue("CaptchaCode", newCode);

            Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, max-age=0");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");

            Response.ContentType = "application/json";

            Log.Information($"Generated new captcha code for login: {newCode}");

            return Json(new { captchaCode = newCode });
        }
        catch (Exception ex)
        {
            Log.Error($"Error generating captcha for login: {ex.Message}");
            return StatusCode(500, new { error = "Error generating captcha" });
        }
    }

    public IActionResult Register()
    {
        var (captchaCode, imageBytes) = _captchaService.GenerateCaptcha();
        SetSessionValue("CaptchaCode", captchaCode);
        ViewBag.CaptchaImage = Convert.ToBase64String(imageBytes);
        return View(new vmRegister());
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RefreshCaptchaImg()
    {
        try
        {
            var (captchaCode, imageBytes) = _captchaService.GenerateCaptcha();
            SetSessionValue("CaptchaCode", captchaCode);

            return Json(new { captchaImage = Convert.ToBase64String(imageBytes) });
        }
        catch (Exception ex)
        {
            Log.Error($"Error generating captcha: {ex.Message}");
            return StatusCode(500, new { error = "Error generating captcha" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Register(vmRegister model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var sessionCaptcha = GetSessionValue("CaptchaCode");
        if (string.IsNullOrEmpty(sessionCaptcha) || sessionCaptcha != model.CaptchaCode)
        {
            ModelState.AddModelError("CaptchaCode", "驗證碼不正確");
            return View(model);
        }

        if (await _authService.ChkUsernameExistAsync(model.Username))
        {
            ModelState.AddModelError("Username", "帳號已被使用");
            return View(model);
        }

        try
        {
            var activationToken = await _authService.RegisterUserAsync(model);
            var activationLink = $"{_config["AppSettings:BaseUrl"]}/Account/Activate?token={activationToken}";
            await _emailService.SendActivationEmailAsync(model.Email, activationLink);

            TempData["SuccessMessage"] = "註冊成功！請檢查您的電子郵件並點擊連結以啟用帳號。";
            ModelState.Clear();
            return View(new vmRegister());
        }
        catch (Exception ex)
        {
            Log.Error($"註冊失敗: {ex.Message}");
            ModelState.AddModelError("", "註冊過程中發生錯誤，請稍後再試");
            return View(model);
        }
    }

    [HttpGet]
    [Route("Account/Activate")]
    [AllowAnonymous]
    public async Task<IActionResult> Activate(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                Log.Warning("啟用帳號失敗: Token 為空");
                ViewBag.IsSuccess = false;
                ViewBag.Message = "無效的啟用連結";
                return View("ActivationConfirmation");
            }

            bool isActivated = await _authService.ActivateUserAsync(token);

            if (!isActivated)
            {
                Log.Warning($"啟用帳號失敗: Token {token} 無效或已過期");
                ViewBag.IsSuccess = false;
                ViewBag.Message = "無效的啟用連結或連結已過期";
                return View("ActivationConfirmation");
            }
            ViewBag.IsSuccess = true;
            ViewBag.Message = "帳號啟用成功！請使用您的帳號登入系統。";
            return View("ActivationConfirmation");
        }
        catch (Exception ex)
        {
            Log.Error($"啟用帳號失敗: {ex.Message}");
            ViewBag.IsSuccess = false;
            ViewBag.Message = "啟用帳號時發生錯誤，請聯繫管理員";
            return View("ActivationConfirmation");
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        var (captchaCode, imageBytes) = _captchaService.GenerateCaptcha();
        SetSessionValue("CaptchaCode", captchaCode);
        ViewBag.CaptchaImage = Convert.ToBase64String(imageBytes);
        return View(new vmForgotPassword());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(vmForgotPassword model)
    {
        string sessionCaptcha = GetSessionValue("CaptchaCode") ?? "";
        if (string.IsNullOrEmpty(sessionCaptcha) || sessionCaptcha != model.CaptchaCode)
        {
            ModelState.AddModelError("CaptchaCode", "驗證碼不正確");
            var (newCaptchaCode, newImageBytes) = _captchaService.GenerateCaptcha();
            SetSessionValue("CaptchaCode", newCaptchaCode);
            ViewBag.CaptchaImage = Convert.ToBase64String(newImageBytes);
            return View(model);
        }

        if (ModelState.IsValid)
        {
            try
            {
                var token = await _authService.GeneratePasswordResetTokenAsync(model.Email);
                if (!string.IsNullOrEmpty(token))
                {
                    var resetUrl = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme);
                    var subject = "密碼重設通知";
                    var body = $@"<h1>密碼重設請求</h1>
                                    <p>您已請求重設密碼，請點擊以下連結進行密碼重設：</p>
                                    <p><a href='{resetUrl}'>重設密碼</a></p>
                                    <p>此連結將在10分鐘後失效</p>
                                    <p>如果您未請求此操作，請忽略此郵件。</p>
                                    <p>本信件為系統自動寄出，請勿回覆！</p>";
                    await _emailService.SendPasswordResetEmailAsync(model.Email, subject, body);
                    Log.Information("密碼重設郵件已發送至 {Email}", model.Email);
                }
                return View("ForgotPasswordConfirmation");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "處理忘記密碼請求時發生錯誤");
                ModelState.AddModelError("", "處理請求時發生錯誤，請稍後再試。");
            }
        }

        var (finalCaptchaCode, finalImageBytes) = _captchaService.GenerateCaptcha();
        SetSessionValue("CaptchaCode", finalCaptchaCode);
        ViewBag.CaptchaImage = Convert.ToBase64String(finalImageBytes);
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Home");
        }

        var model = new vmResetPassword { Token = token };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(vmResetPassword model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var result = await _authService.ResetPasswordTokenAsync(model, model.Token);
            if (result)
            {
                Log.Information("密碼重設成功");
                return View("ResetPasswordConfirmation");
            }
            else
            {
                ModelState.AddModelError("", "密碼重設失敗，請檢查重設連結是否有效或已過期");
            }
        }
        catch (PasswordResetException ex)
        {
            Log.Error(ex, "密碼重設失敗");
            ModelState.AddModelError("", ex.UserFriendlyMessage);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "處理重設密碼請求時發生錯誤");
            ModelState.AddModelError("", "處理請求時發生錯誤，請稍後再試。");
        }
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GoogleLogin()
    {
        try
        {
            // Generate state for CSRF protection
            var state = _socialLoginService.GenerateState();
            SetSessionValue("GoogleOAuthState", state);

            // Get Google OAuth URL
            var authUrl = _socialLoginService.GetGoogleAuthUrl(state);

            Log.Information("Redirecting to Google OAuth with state: {State}", state);
            return Redirect(authUrl);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error initiating Google login");
            TempData["ErrorMessage"] = "Google 登入初始化失敗，請稍後再試";
            return RedirectToAction("Login");
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback(string code, string state, string error, string error_description)
    {
        try
        {
            // Check for OAuth errors
            if (!string.IsNullOrEmpty(error))
            {
                Log.Warning("Google OAuth error: {Error} - {ErrorDescription}", error, error_description);
                TempData["ErrorMessage"] = "Google 登入被取消或發生錯誤";
                return RedirectToAction("Login");
            }

            // Validate state parameter for CSRF protection
            var sessionState = GetSessionValue("GoogleOAuthState");
            if (!_socialLoginService.ValidateState(state, sessionState))
            {
                Log.Warning("Invalid OAuth state. Expected: {SessionState}, Received: {State}", sessionState, state);
                TempData["ErrorMessage"] = "登入驗證失敗，請重新嘗試";
                return RedirectToAction("Login");
            }

            // Clear the state from session
            SetSessionValue("GoogleOAuthState", "");

            if (string.IsNullOrEmpty(code))
            {
                Log.Warning("No authorization code received from Google");
                TempData["ErrorMessage"] = "Google 登入失敗，未收到授權碼";
                return RedirectToAction("Login");
            }

            // Get user info from Google
            var googleUserInfo = await _socialLoginService.GetGoogleUserInfoAsync(code);
            
            if (googleUserInfo == null || string.IsNullOrEmpty(googleUserInfo.Email))
            {
                Log.Warning("Failed to retrieve user info from Google");
                TempData["ErrorMessage"] = "無法從 Google 取得使用者資訊";
                return RedirectToAction("Login");
            }

            // Authenticate or create social login user
            var user = await _authService.AuthenticateSocialUserAsync(
                "Google", 
                googleUserInfo.Id, 
                googleUserInfo.Name, 
                googleUserInfo.Email);

            if (user == null)
            {
                Log.Warning("Failed to authenticate social user for Google ID: {GoogleId}", googleUserInfo.Id);
                TempData["ErrorMessage"] = "社群登入失敗，請聯繫系統管理員";
                return RedirectToAction("Login");
            }

            // Generate JWT token and set cookie
            var token = _jwtServices.GenerateToken(user);
            
            // Check if request is coming through a reverse proxy with HTTPS
            var forwardedProto = Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
            var isHttpsForwarded = string.Equals(forwardedProto, "https", StringComparison.OrdinalIgnoreCase);
            var isSecure = Request.IsHttps || isHttpsForwarded;
            
            Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = isSecure,
                SameSite = SameSiteMode.Lax, // Changed from None to Lax for better compatibility
                Expires = DateTime.Now.AddHours(8)
            });

            Log.Information("Google login successful for user: {Email}, UserNo: {UserNo}", user.ContactEmail, user.UserNo);

            // Redirect based on user role
            if (user.RoleNo == "Admin" || user.RoleNo == "Mis" || user.RoleNo == "User")
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing Google OAuth callback");
            TempData["ErrorMessage"] = "Google 登入處理失敗，請稍後再試";
            return RedirectToAction("Login");
        }
    }
}
