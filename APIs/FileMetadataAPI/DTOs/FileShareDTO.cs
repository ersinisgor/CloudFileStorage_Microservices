using FileMetadataAPI.Models;

namespace FileMetadataAPI.DTOs
{
    public class FileShareDTO
    {
        public int UserId { get; set; }
        public Permission Permission { get; set; }
    }
}