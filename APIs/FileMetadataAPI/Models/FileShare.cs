namespace FileMetadataAPI.Models
{
    internal class FileShare
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public int UserId { get; set; }
        public string Permission { get; set; }
        public File File { get; set; }
    }
}
