using Microsoft.AspNetCore.Mvc;
using MediatR;
using FileStorageAPI.Commands.UploadFile;
using FileStorageAPI.DTOs;
using FileStorageAPI.Queries.DownloadFile;
using FileStorageAPI.Commands;


namespace FileStorageAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController(IMediator mediator, IHttpClientFactory httpClientFactory) : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromQuery] int id, [FromForm] IFormFile file)
        {
            try
            {
                // Validate file and ID
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { Error = "File is required." });
                }
                if (id <= 0)
                {
                    return BadRequest(new { Error = "Invalid file ID." });
                }

                var command = new UploadFileCommand { File = file };
                var filePath = await mediator.Send(command);
                return Ok(new { FilePath = filePath, FileId = id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] int id)
        {
            try
            {
                // Validate ID
                if (id <= 0)
                {
                    return BadRequest(new { Error = "Invalid file ID." });
                }

                // Fetch file metadata from FileMetadataAPI via GatewayAPI
                var client = httpClientFactory.CreateClient("GatewayAPI");
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                client.DefaultRequestHeaders.Authorization = new("Bearer", token);

                var response = await client.GetAsync($"/api/files/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                var fileDto = await response.Content.ReadFromJsonAsync<FileDTO>();
                if (fileDto == null)
                {
                    return NotFound(new { Error = "File metadata not found." });
                }

                // Download file content
                var query = new DownloadFileQuery { FilePath = fileDto.Path };
                var result = await mediator.Send(query);
                return File(result.Content, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Fetch file metadata from FileMetadataAPI via GatewayAPI
                var client = httpClientFactory.CreateClient("GatewayAPI");
                var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                client.DefaultRequestHeaders.Authorization = new("Bearer", token);

                var response = await client.GetAsync($"/api/files/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                var fileDto = await response.Content.ReadFromJsonAsync<FileDTO>();
                if (fileDto == null)
                {
                    return NotFound(new { Error = "File metadata not found." });
                }

                // Delete file
                var command = new DeleteFileCommand { FilePath = fileDto.Path };
                await mediator.Send(command);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}