using MediatR;
using FileMetadataAPI.DataContext;
using FileMetadataAPI.DTOs;
using AutoMapper;
using System.Security.Claims;
using FileMetadataAPI.Commands;
using File = FileMetadataAPI.Models.File;
using System.Net.Http.Headers;

namespace FileMetadataAPI.Handlers
{
    internal class CreateFileCommandHandler(
        ApplicationDbContext context,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory) : IRequestHandler<CreateFileCommand, FileDTO>
    {
        public async Task<FileDTO> Handle(CreateFileCommand request, CancellationToken cancellationToken)
        {
            var userIdClaim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new ForbiddenException("User ID claim not found.");
            }
            var userId = int.Parse(userIdClaim.Value);

            // Create file entity without path initially
            var file = mapper.Map<File>(request);
            file.OwnerId = userId;
            file.UploadDate = DateTime.UtcNow;

            // Save to generate ID
            context.Files.Add(file);
            await context.SaveChangesAsync(cancellationToken);

            // Upload file to FileStorageAPI with generated ID
            var client = httpClientFactory.CreateClient("GatewayAPI");
            var token = httpContextAccessor.HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(request.File.OpenReadStream()), "file", request.File.FileName);

            var response = await client.PostAsync($"/api/storage/upload?id={file.Id}", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                context.Files.Remove(file);
                await context.SaveChangesAsync(cancellationToken);
                throw new Exception("Failed to upload file to storage.");
            }

            var storageResult = await response.Content.ReadFromJsonAsync<StorageResponseDTO>();
            if (storageResult == null || string.IsNullOrEmpty(storageResult.FilePath))
            {
                context.Files.Remove(file);
                await context.SaveChangesAsync(cancellationToken);
                throw new Exception("Invalid response from storage service.");
            }

            // Update file with storage path
            file.Path = storageResult.FilePath;
            await context.SaveChangesAsync(cancellationToken);

            var fileDto = mapper.Map<FileDTO>(file);
            fileDto.IsOwner = true; // Yeni oluşturulan dosya, kullanıcıya aittir
            return fileDto;
        }
    }
}