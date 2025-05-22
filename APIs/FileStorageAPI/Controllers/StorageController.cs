using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class StorageController(IMediator mediator) : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            var command = new UploadFileCommand { File = file };
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] DownloadFileRequest request)
        {
            var query = new DownloadFileQuery { FilePath = request.FilePath };
            var result = await mediator.Send(query);
            return File(result.Content, result.ContentType, result.FileName);
        }

        [HttpDelete("{filePath}")]
        public async Task<IActionResult> Delete(string filePath)
        {
            var command = new DeleteFileCommand { FilePath = filePath };
            await mediator.Send(command);
            return NoContent();
        }
    }
}