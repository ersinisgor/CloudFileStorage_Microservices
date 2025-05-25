namespace MVCApplication.Models
{
    public class FileViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OwnerId { get; set; }
        public DateTime UploadDate { get; set; }
        public string Visibility { get; set; }
        public string Path { get; set; }
        public List<FileShareViewModel> FileShares { get; set; } = new();
    }

    public class FileShareViewModel
    {
        public int UserId { get; set; }
        public string Permission { get; set; }
    }
}