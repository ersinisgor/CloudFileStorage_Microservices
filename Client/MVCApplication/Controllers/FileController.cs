using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Net.Http;

namespace MVCApplication.Controllers
{
    public class FileController(IHttpClientFactory httpClientFactory, ILogger<FileController> logger) : Controller
    {
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = httpClientFactory.CreateClient("GatewayAPI");
                var token = User.Claims.FirstOrDefault(c => c.Type == "token")?.Value;
                if (string.IsNullOrEmpty(token))
                {
                    logger.LogWarning("Authentication token is missing for user: {UserId}", User.FindFirst("nameid")?.Value);
                    ViewBag.Error = "Authentication token is missing.";
                    return View(new List<FileViewModel>());
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                logger.LogInformation("Sending request to /api/files with token: {TokenPrefix} for user: {UserId}",
                    token[..10], User.FindFirst("nameid")?.Value);

                var response = await client.GetAsync("/api/files");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    logger.LogError("Failed to retrieve files from /api/files. Status: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    ViewBag.Error = $"Failed to retrieve files: {response.StatusCode} - {errorContent}";
                    return View(new List<FileViewModel>());
                }

                var files = await response.Content.ReadFromJsonAsync<List<FileViewModel>>();
                if (files == null)
                {
                    logger.LogWarning("Received null response from /api/files, returning empty list for user: {UserId}",
                        User.FindFirst("nameid")?.Value);
                    ViewBag.Error = "No files found.";
                    return View(new List<FileViewModel>());
                }

                // Oturum açan kullanıcının ID'sini al
                var userId = int.Parse(User.FindFirst("nameid")?.Value ?? throw new Exception("User ID not found."));

                // IsOwner özelliğini ayarla
                foreach (var file in files)
                {
                    file.IsOwner = file.OwnerId == userId;
                }

                logger.LogInformation("Successfully retrieved {FileCount} files for user: {UserId}",
                    files.Count, User.FindFirst("nameid")?.Value);
                return View(files);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to retrieve files from /api/files. Status: {StatusCode}", ex.StatusCode);
                ViewBag.Error = ex.StatusCode == System.Net.HttpStatusCode.BadGateway
                    ? "File service is currently unavailable. Please try again later."
                    : "Unable to retrieve files. Please try again later.";
                return View(new List<FileViewModel>());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while retrieving files for user: {UserId}",
                    User.FindFirst("nameid")?.Value);
                ViewBag.Error = $"Unexpected error: {ex.Message}";
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
                    ViewBag.Error = "Please select a file to upload.";
                    return RedirectToAction("Index");
                }

                var client = httpClientFactory.CreateClient("GatewayAPI");
                var token = User.Claims.FirstOrDefault(c => c.Type == "token")?.Value;
                if (string.IsNullOrEmpty(token))
                {
                    logger.LogWarning("Authentication token is missing for user: {UserId}", User.FindFirst("nameid")?.Value);
                    ViewBag.Error = "Authentication token is missing.";
                    return RedirectToAction("Index");
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Prepare file data for FileMetadataAPI
                using var content = new MultipartFormDataContent();
                content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);
                content.Add(new StringContent(file.FileName), "name");
                content.Add(new StringContent(description ?? ""), "description");
                content.Add(new StringContent(visibility), "visibility");

                // Send to FileMetadataAPI, which will handle storage
                var response = await client.PostAsync("/api/files", content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    logger.LogError("File upload failed for user: {UserId}. Status: {StatusCode}, Content: {ErrorContent}",
                        User.FindFirst("nameid")?.Value, response.StatusCode, errorContent);
                    ViewBag.Error = $"File upload failed: {errorContent}";
                    return RedirectToAction("Index");
                }

                logger.LogInformation("File uploaded successfully for user: {UserId}", User.FindFirst("nameid")?.Value);
                return RedirectToAction("Index");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error during file upload for user: {UserId}", User.FindFirst("nameid")?.Value);
                ViewBag.Error = $"File upload failed: {ex.Message}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during file upload for user: {UserId}", User.FindFirst("nameid")?.Value);
                ViewBag.Error = "File upload failed.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSharing(int id, string visibility, string sharedUserIds, string permission)
        {
            try
            {
                var client = httpClientFactory.CreateClient("GatewayAPI");
                var token = User.Claims.FirstOrDefault(c => c.Type == "token")?.Value;
                if (string.IsNullOrEmpty(token))
                {
                    logger.LogWarning("Authentication token is missing for user: {UserId}", User.FindFirst("nameid")?.Value);
                    ViewBag.Error = "Authentication token is missing.";
                    return RedirectToAction("Index");
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Dosyanın sahibi olduğunu doğrula
                var fileResponse = await client.GetAsync($"/api/files/{id}");
                if (!fileResponse.IsSuccessStatusCode)
                {
                    logger.LogError("Failed to retrieve file ID: {FileId} for user: {UserId}", id, User.FindFirst("nameid")?.Value);
                    ViewBag.Error = "File not found or access denied.";
                    return RedirectToAction("Index");
                }

                var file = await fileResponse.Content.ReadFromJsonAsync<FileViewModel>();
                var userId = int.Parse(User.FindFirst("nameid")?.Value ?? throw new Exception("User ID not found."));
                if (file?.OwnerId != userId)
                {
                    logger.LogWarning("User {UserId} attempted to update sharing for file ID: {FileId} they do not own", userId, id);
                    ViewBag.Error = "You are not authorized to update this file's sharing settings.";
                    return RedirectToAction("Index");
                }

                var sharedUserIdList = string.IsNullOrEmpty(sharedUserIds)
                    ? new List<int>()
                    : sharedUserIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();

                var request = new ShareFileCommand
                {
                    FileId = id,
                    Visibility = visibility,
                    FileShares = sharedUserIdList.Select(userId => new FileShareDTO { UserId = userId, Permission = permission }).ToList()
                };

                var response = await client.PostAsJsonAsync($"/api/files/{id}/share", request);
                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Sharing settings updated for file ID: {FileId} by user: {UserId}",
                        id, User.FindFirst("nameid")?.Value);
                    return RedirectToAction("Index");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed to update sharing settings for file ID: {FileId} by user: {UserId}. Status: {StatusCode}, Content: {ErrorContent}",
                    id, User.FindFirst("nameid")?.Value, response.StatusCode, errorContent);
                ViewBag.Error = $"Failed to update sharing settings: {errorContent}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during sharing update for file ID: {FileId} by user: {UserId}",
                    id, User.FindFirst("nameid")?.Value);
                ViewBag.Error = "Failed to update sharing settings.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = httpClientFactory.CreateClient("GatewayAPI");
                var token = User.Claims.FirstOrDefault(c => c.Type == "token")?.Value;
                if (string.IsNullOrEmpty(token))
                {
                    logger.LogWarning("Authentication token is missing for user: {UserId}", User.FindFirst("nameid")?.Value);
                    ViewBag.Error = "Authentication token is missing.";
                    return RedirectToAction("Index");
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Dosyanın sahibi olduğunu doğrula
                var fileResponse = await client.GetAsync($"/api/files/{id}");
                if (!fileResponse.IsSuccessStatusCode)
                {
                    logger.LogError("Failed to retrieve file ID: {FileId} for user: {UserId}", id, User.FindFirst("nameid")?.Value);
                    ViewBag.Error = "File not found or access denied.";
                    return RedirectToAction("Index");
                }

                var file = await fileResponse.Content.ReadFromJsonAsync<FileViewModel>();
                var userId = int.Parse(User.FindFirst("nameid")?.Value ?? throw new Exception("User ID not found."));
                if (file?.OwnerId != userId)
                {
                    logger.LogWarning("User {UserId} attempted to delete file ID: {FileId} they do not own", userId, id);
                    ViewBag.Error = "You are not authorized to delete this file.";
                    return RedirectToAction("Index");
                }

                var response = await client.DeleteAsync($"/api/files/{id}");
                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("File ID: {FileId} deleted successfully by user: {UserId}",
                        id, User.FindFirst("nameid")?.Value);
                    return RedirectToAction("Index");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed to delete file ID: {FileId} by user: {UserId}. Status: {StatusCode}, Content: {ErrorContent}",
                    id, User.FindFirst("nameid")?.Value, response.StatusCode, errorContent);
                ViewBag.Error = $"Failed to delete file: {errorContent}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during file deletion for file ID: {FileId} by user: {UserId}",
                    id, User.FindFirst("nameid")?.Value);
                ViewBag.Error = "Failed to delete file.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Download(int id)
        {
            var client = httpClientFactory.CreateClient("GatewayAPI");
            var token = User.Claims.FirstOrDefault(c => c.Type == "token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                logger.LogWarning("Authentication token is missing for user: {UserId}", User.FindFirst("nameid")?.Value);
                ViewBag.Error = "Authentication token is missing.";
                return RedirectToAction("Index");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var metadataResponse = await client.GetAsync($"https://localhost:5000/api/files/{id}");
            if (!metadataResponse.IsSuccessStatusCode)
            {
                return NotFound("File not found or access denied.");
            }

            var downloadResponse = await client.GetAsync($"https://localhost:5000/api/storage/download?id={id}");
            if (!downloadResponse.IsSuccessStatusCode)
            {
                return NotFound("File download failed.");
            }

            var fileBytes = await downloadResponse.Content.ReadAsByteArrayAsync();
            var fileName = downloadResponse.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? $"file_{id}";
            var contentType = downloadResponse.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

            return File(fileBytes, contentType, fileName);
        }
    }
}