namespace MVCApplication.Models
{
    public class UpdateFileDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Visibility { get; set; }
        public string SharedUserIds { get; set; }
        public string Permission { get; set; }
    }
}
