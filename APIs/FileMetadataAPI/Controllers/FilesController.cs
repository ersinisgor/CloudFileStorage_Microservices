using MediatR;
using Microsoft.AspNetCore.Mvc;
using FileMetadataAPI.Commands;
using FileMetadataAPI.Queries;
using FileMetadataAPI.DTOs;
using FluentValidation;
using FileMetadataAPI.Handlers;
using Microsoft.AspNetCore.Authorization;

namespace FileMetadataAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FilesController(IMediator mediator, ILogger<FilesController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<FileDTO>>> GetFiles()
        {
            try
            {
                logger.LogInformation("Getting files for user: {UserId}", User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var files = await mediator.Send(new GetFilesQuery());
                return Ok(files);
            }
            catch (ValidationException ex)
            {
                logger.LogWarning("Validation error in GetFiles: {Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving files for user: {UserId}", User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FileDTO>> GetFile(int id)
        {
            try
            {
                logger.LogInformation("Getting file {FileId} for user: {UserId}", id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var result = await mediator.Send(new GetFileByIdQuery { Id = id });
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                logger.LogWarning("File {FileId} not found for user: {UserId}", id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return NotFound(new { Error = ex.Message });
            }
            catch (ForbiddenException ex)
            {
                logger.LogWarning("Access forbidden to file {FileId} for user: {UserId}", id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving file {FileId} for user: {UserId}", id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { Error = ex.Message });
            }

        }

        [HttpPost]
        public async Task<ActionResult<FileDTO>> CreateFile([FromForm] CreateFileCommand command)
        {
            try
            {
                logger.LogInformation("Creating file for user: {UserId}", User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var result = await mediator.Send(command);
                return CreatedAtAction(nameof(GetFile), new { id = result.Id }, result);
            }
            catch (ValidationException ex)
            {
                logger.LogWarning("Validation error in CreateFile: {Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating file for user: {UserId}", User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<FileDTO>> UpdateFile([FromRoute] int id, [FromBody] UpdateFileCommand command)
        {
            try
            {
                if (id != command.Id)
                {
                    logger.LogWarning("ID mismatch in UpdateFile: route id {RouteId}, command id {CommandId}", id, command.Id);
                    return BadRequest(new { Error = "ID mismatch." });
                }

                logger.LogInformation("Updating file {FileId} for user: {UserId}", id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var result = await mediator.Send(command);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                logger.LogWarning("Validation error in UpdateFile: {Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating file {FileId} for user: {UserId}", id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFile(int id)
        {
            try
            {
                logger.LogInformation("Deleting file {FileId} for user: {UserId}", id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                await mediator.Send(new DeleteFileCommand { Id = id });
                return NoContent();
            }
            catch (ValidationException ex)
            {
                logger.LogWarning("Validation error in DeleteFile: {Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting file {FileId} for user: {UserId}", id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("{id}/share")]
        public async Task<ActionResult> ShareFile(int id, [FromBody] ShareFileCommand command)
        {
            try
            {
                if (id != command.FileId)
                {
                    logger.LogWarning("ID mismatch in ShareFile: route id {RouteId}, command id {CommandId}", id, command.FileId);
                    return BadRequest(new { Error = "ID mismatch." });
                }

                logger.LogInformation("Sharing file {FileId} for user: {UserId}", id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                await mediator.Send(command);
                return Ok();
            }
            catch (ValidationException ex)
            {
                logger.LogWarning("Validation error in ShareFile: {Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sharing file {FileId} for user: {UserId}", id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}