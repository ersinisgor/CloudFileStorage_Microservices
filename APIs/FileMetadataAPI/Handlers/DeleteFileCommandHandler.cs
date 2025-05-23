﻿using MediatR;
using FileMetadataAPI.DataContext;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FileMetadataAPI.Commands;

namespace FileMetadataAPI.Handlers
{
    internal class DeleteFileCommandHandler(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<DeleteFileCommand>
    {
        public async Task Handle(DeleteFileCommand request, CancellationToken cancellationToken)
        {
            var userId = int.Parse(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new Exception("User ID not found."));

            var file = await context.Files
                .FirstOrDefaultAsync(f => f.Id == request.Id && f.OwnerId == userId, cancellationToken)
                ?? throw new Exception("File not found or authorization error.");

            context.Files.Remove(file);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}