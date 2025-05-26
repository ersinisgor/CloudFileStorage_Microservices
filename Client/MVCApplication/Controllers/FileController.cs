using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MVCApplication.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Security.Claims;

namespace MVCApplication.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FileController> _logger;

        public FileController(IHttpClientFactory httpClientFactory, ILogger<FileController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("GatewayAPI");
            _logger = logger;
        }

        private void AddAuthorizationHeader()
        {
            // JWT token'ı cookie'den al ve Header'a ekle
            var token = HttpContext.Request.Cookies["AuthToken"];

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                _logger.LogDebug("Authorization header added with token");
            }
            else
            {
                _logger.LogWarning("No auth token found in cookies");
            }
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                AddAuthorizationHeader();

                var response = await _httpClient.GetAsync("/api/files");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var files = JsonSerializer.Deserialize<List<FileViewModel>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return View(files ?? new List<FileViewModel>());
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Unauthorized access to files API");
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    _logger.LogError("Error retrieving files: {StatusCode}", response.StatusCode);
                    ViewBag.Error = "Error retrieving files";
                    return View(new List<FileViewModel>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while retrieving files");
                ViewBag.Error = "An error occurred while retrieving files";
                return View(new List<FileViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, string description, string visibility)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    ViewBag.Error = "Please select a file";
                    return RedirectToAction("Index");
                }

                AddAuthorizationHeader();

                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                content.Add(fileContent, "file", file.FileName);
                content.Add(new StringContent(file.FileName), "name");
                content.Add(new StringContent(description ?? ""), "description");
                content.Add(new StringContent(visibility ?? "Private"), "visibility");

                var response = await _httpClient.PostAsync("/api/files", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("File uploaded successfully: {FileName}", file.FileName);
                    return RedirectToAction("Index");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error uploading file: {StatusCode}, {Error}", response.StatusCode, errorContent);
                    ViewBag.Error = "Error uploading file";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while uploading file");
                ViewBag.Error = "An error occurred while uploading the file";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                AddAuthorizationHeader();

                var response = await _httpClient.DeleteAsync($"/api/files/{id}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("File deleted successfully: {FileId}", id);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    _logger.LogError("Error deleting file {FileId}: {StatusCode}", id, response.StatusCode);
                    ViewBag.Error = "Error deleting file";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting file {FileId}", id);
                ViewBag.Error = "An error occurred while deleting the file";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Download(int id)
        {
            try
            {
                AddAuthorizationHeader();

                // FileStorageAPI üzerinden dosyayı indir
                var response = await _httpClient.GetAsync($"/api/storage/download/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    var fileName = response.Content.Headers.ContentDisposition?.FileNameStar ?? "download";
                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

                    return File(fileBytes, contentType, fileName);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    _logger.LogError("Error downloading file {FileId}: {StatusCode}", id, response.StatusCode);
                    ViewBag.Error = "Error downloading file";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while downloading file {FileId}", id);
                ViewBag.Error = "An error occurred while downloading the file";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSharing(int id, string visibility, string sharedUserIds, string permission)
        {
            try
            {
                AddAuthorizationHeader();

                var shareCommand = new
                {
                    FileId = id,
                    Visibility = visibility,
                    SharedUserIds = sharedUserIds?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(s => int.TryParse(s.Trim(), out var userId) ? userId : 0)
                                                  .Where(userId => userId > 0)
                                                  .ToList() ?? new List<int>(),
                    Permission = permission ?? "Read"
                };

                var json = JsonSerializer.Serialize(shareCommand);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/api/files/{id}/share", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("File sharing updated successfully: {FileId}", id);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error updating file sharing {FileId}: {StatusCode}, {Error}", id, response.StatusCode, errorContent);
                    ViewBag.Error = "Error updating file sharing";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating file sharing {FileId}", id);
                ViewBag.Error = "An error occurred while updating file sharing";
                return RedirectToAction("Index");
            }
        }
    }
}