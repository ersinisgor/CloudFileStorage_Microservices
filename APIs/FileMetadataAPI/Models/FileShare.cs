namespace FileMetadataAPI.Models
{
    internal class FileShare
    {
        public int FileId { get; set; }
        public File File { get; set; }
        public int UserId { get; set; }
        public string Permission { get; set; }
    }
}
