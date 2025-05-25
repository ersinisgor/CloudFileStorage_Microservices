namespace MVCApplication.Models
{
    public class ShareFileCommand
    {
        public int FileId { get; set; }
        public string Visibility { get; set; }
        public List<FileShareDTO> FileShares { get; set; } = new();
    }
}
