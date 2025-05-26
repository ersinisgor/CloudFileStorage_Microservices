namespace FileMetadataAPI.DTOs
{
    public class FileDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int OwnerId { get; set; }
        public DateTime UploadDate { get; set; }
        public string Visibility { get; set; }
        public string Path { get; set; } // Added to store file path
        public List<FileShareDTO> FileShares { get; set; } = new();
    }
}