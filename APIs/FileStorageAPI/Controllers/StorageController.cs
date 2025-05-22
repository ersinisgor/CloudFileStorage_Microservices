using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using FileStorageAPI.Commands.UploadFile;


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

    }
}