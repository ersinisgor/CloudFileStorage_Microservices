using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using MVCApplication.Models;
using System.Net.Http.Headers;
using System.Net.Http;

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
        public async Task<IActionResult> Login([FromForm] LoginViewModel model)
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
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddDays(1)
                        };

                        Response.Cookies.Append("AuthToken", authResult.Token, cookieOptions);

                        if (!string.IsNullOrEmpty(authResult.RefreshToken))
                        {
                            Response.Cookies.Append("RefreshToken", authResult.RefreshToken, cookieOptions);
                        }

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

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var token = Request.Cookies["AuthToken"];
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userId))
                {
                    var client = _httpClient;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    await client.PostAsJsonAsync("/api/auth/logout", new { UserId = userId, Token = token });
                }

                Response.Cookies.Delete("AuthToken");
                Response.Cookies.Delete("RefreshToken");

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                _logger.LogInformation("User logged out successfully");
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout error");
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
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