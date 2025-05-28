using MediatR;
using Microsoft.AspNetCore.Mvc;
using AuthenticationAPI.Commands;
using AuthenticationAPI.Queries;
using AuthenticationAPI.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace AuthenticationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IMediator mediator, ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ILogger<AuthController> _logger = logger;

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="command">The registration command containing user details.</param>
        /// <returns>The registered user's DTO.</returns>
        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register([FromBody] RegisterCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Logs in a user and returns an authentication result.
        /// </summary>
        /// <param name="query">The login query containing email and password.</param>
        /// <returns>The authentication result containing token and refresh token.</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResult>> Login([FromBody] LoginQuery query)
        {
            try
            {
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid credentials");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Refreshes the JWT token using a refresh token.
        /// </summary>
        /// <param name="command">The refresh token command containing the refresh token.</param>
        /// <returns>A new authentication result with a new token and refresh token.</returns>
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResult>> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during refresh token");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Logs out the current user by invalidating their refresh token.
        /// </summary>
        /// <returns>A success message.</returns>
        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            try
            {
                await _mediator.Send(new LogoutCommand());
                return Ok(new { Message = "Logout successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}