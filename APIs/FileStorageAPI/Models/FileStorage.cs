namespace FileStorageAPI.Models
{
    public class FileStorage
    {
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
    }
}