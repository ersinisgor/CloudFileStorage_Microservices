namespace FileMetadataAPI.Models
{
    public enum Visibility
    {
        Private,
        Public,
        Shared
    }

    internal class File
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OwnerId { get; set; }
        public DateTime UploadDate { get; set; }
        public Visibility Visibility { get; set; }
        public List<FileShare> FileShares { get; set; } = new();
    }
}
