using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Net.Http.Headers;

namespace MVCApplication.Controllers
{
    public class HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                var client = httpClientFactory.CreateClient("GatewayAPI");
                var response = await client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    logger.LogWarning("Login failed for email: {Email}. Status: {StatusCode}, Content: {ErrorContent}", email, response.StatusCode, errorContent);
                    ViewBag.Error = "Invalid credentials.";
                    return View();
                }

                var authResult = await response.Content.ReadFromJsonAsync<AuthResultViewModel>();
                if (authResult?.Token == null || authResult.User == null)
                {
                    logger.LogWarning("Invalid auth result received for email: {Email}", email);
                    ViewBag.Error = "Authentication failed.";
                    return View();
                }

                var claims = new List<Claim>
                {
                    new Claim("nameid", authResult.User.Id),
                    new Claim(ClaimTypes.Name, authResult.User.Name ?? string.Empty),
                    new Claim(ClaimTypes.Email, authResult.User.Email),
                    new Claim(ClaimTypes.Role, authResult.User.Role),
                    new Claim("token", authResult.Token) // Store JWT token for API calls
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                logger.LogInformation("Login successful for email: {Email}", email);
                return RedirectToAction("Index", "File");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error during login for email: {Email}", email);
                ViewBag.Error = "An error occurred during login.";
                return View();
            }
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string name, string email, string password, string role)
        {
            try
            {
                var client = httpClientFactory.CreateClient("GatewayAPI");
                var response = await client.PostAsJsonAsync("/api/auth/register", new { Name = name, Email = email, Password = password, Role = role });
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserViewModel>();
                    if (user == null)
                    {
                        ViewBag.Error = "Invalid response from authentication service.";
                        return View();
                    }
                    return RedirectToAction("Login");
                }

                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                var errorMessage = error?.Error ?? "Registration failed. Please try again.";
                logger.LogError("Registration failed with status {StatusCode}: {Error}", response.StatusCode, errorMessage);
                ViewBag.Error = errorMessage;
                return View();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to connect to GatewayAPI");
                ViewBag.Error = "Unable to connect to the authentication service. Please try again later.";
                return View();
            }
            catch (System.Text.Json.JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize response from GatewayAPI");
                ViewBag.Error = "Invalid response from authentication service.";
                return View();
            }

        }

        public async Task<IActionResult> Logout()
        {
            var client = httpClientFactory.CreateClient("GatewayAPI");
            var token = User.Claims.FirstOrDefault(c => c.Type == "token")?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userId))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                await client.PostAsJsonAsync("/api/auth/logout", new { UserId = userId, Token = token });
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}