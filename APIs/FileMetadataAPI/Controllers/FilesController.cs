using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileMetadataAPI.Commands;
using FileMetadataAPI.Queries;
using FileMetadataAPI.DTOs;
using FluentValidation;

namespace FileMetadataAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FilesController(IMediator mediator) : ControllerBase
    {

        [HttpGet("{id}")]
        public async Task<ActionResult<FileDTO>> GetFile(int id)
        {
            try
            {
                var result = await mediator.Send(new GetFileByIdQuery { Id = id });
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<FileDTO>> CreateFile([FromBody] CreateFileCommand command)
        {
            try
            {
                var result = await mediator.Send(command);
                return CreatedAtAction(nameof(GetFile), new { id = result.Id }, result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

       
    }
}