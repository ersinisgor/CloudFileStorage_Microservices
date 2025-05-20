namespace FileMetadataAPI.Models
{
    public class File
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OwnerId { get; set; }
        public DateTime UploadDate { get; set; }
        public string Visibility { get; set; }
    }
}
