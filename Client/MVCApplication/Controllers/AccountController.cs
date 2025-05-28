using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using MVCApplication.Models;

namespace MVCApplication.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IHttpClientFactory httpClientFactory, ILogger<AccountController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("GatewayAPI");
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var loginData = new
                {
                    Email = model.Email,
                    Password = model.Password
                };

                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Gateway üzerinden AuthenticationAPI'ye login isteği gönder
                var response = await _httpClient.PostAsync("/api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var authResult = JsonSerializer.Deserialize<AuthResultViewModel>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (authResult?.Token != null && authResult.User != null)
                    {
                        // JWT token'ı güvenli HTTP-only cookie'ye kaydet
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true, // HTTPS gerektir
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddDays(1) // Token süresi ile eşleştir
                        };

                        Response.Cookies.Append("AuthToken", authResult.Token, cookieOptions);

                        // Refresh token'ı da kaydet (opsiyonel)
                        if (!string.IsNullOrEmpty(authResult.RefreshToken))
                        {
                            Response.Cookies.Append("RefreshToken", authResult.RefreshToken, cookieOptions);
                        }

                        // Kullanıcı bilgilerini session'a kaydet (UI için)
                        HttpContext.Session.SetString("UserInfo", JsonSerializer.Serialize(authResult.User));

                        // Authentication cookie oluştur (ASP.NET Core Identity sistemi için)
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, authResult.User.Id.ToString()),
                            new Claim(ClaimTypes.Name, authResult.User.Name ?? ""),
                            new Claim(ClaimTypes.Email, authResult.User.Email ?? ""),
                            new Claim(ClaimTypes.Role, authResult.User.Role ?? "User")
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        _logger.LogInformation("User {Email} logged in successfully", model.Email);
                        return RedirectToAction("Index", "File");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Login failed for {Email}: {Error}", model.Email, errorContent);

                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred during login");
                return View(model);
            }

            ModelState.AddModelError("", "Login failed");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Cookie'leri temizle
                Response.Cookies.Delete("AuthToken");
                Response.Cookies.Delete("RefreshToken");

                // Session'ı temizle
                HttpContext.Session.Clear();

                // Authentication cookie'yi temizle
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                _logger.LogInformation("User logged out successfully");
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout error");
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var registerData = new
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password
                };

                var json = JsonSerializer.Serialize(registerData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("User {Email} registered successfully", model.Email);
                    TempData["SuccessMessage"] = "Registration successful. Please login.";
                    return RedirectToAction("Login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Registration failed for {Email}: {Error}", model.Email, errorContent);

                    ModelState.AddModelError("", "Registration failed. Please try again.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error for {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred during registration");
                return View(model);
            }
        }
    }
}