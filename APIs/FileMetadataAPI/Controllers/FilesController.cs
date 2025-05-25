using MediatR;
using Microsoft.AspNetCore.Mvc;
using FileMetadataAPI.Commands;
using FileMetadataAPI.Queries;
using FileMetadataAPI.DTOs;
using FluentValidation;
using FileMetadataAPI.Handlers;

namespace FileMetadataAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<FileDTO>>> GetFiles()
        {
            try
            {
                var result = await mediator.Send(new GetFilesQuery());
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

        [HttpGet("{id}")]
        public async Task<ActionResult<FileDTO>> GetFile(int id)
        {
            try
            {
                var result = await mediator.Send(new GetFileByIdQuery { Id = id });
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (ForbiddenException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
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

        [HttpPut("{id}")]
        public async Task<ActionResult<FileDTO>> UpdateFile([FromRoute] int id, [FromBody] UpdateFileCommand command)
        {
            try
            {
                if (id != command.Id)
                    return BadRequest(new { Error = "ID mismatch." });

                var result = await mediator.Send(command);
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFile(int id)
        {
            try
            {
                await mediator.Send(new DeleteFileCommand { Id = id });
                return NoContent();
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

        [HttpPost("{id}/share")]
        public async Task<ActionResult> ShareFile(int id, [FromBody] ShareFileCommand command)
        {
            try
            {
                if (id != command.FileId)
                    return BadRequest(new { Error = "ID mismatch." });

                await mediator.Send(command);
                return Ok();
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