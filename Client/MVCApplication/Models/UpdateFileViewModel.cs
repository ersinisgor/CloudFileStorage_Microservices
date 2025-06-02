using System.ComponentModel.DataAnnotations;

namespace MVCApplication.Models
{
    public class UpdateFileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "File name is required.")]
        [MaxLength(255, ErrorMessage = "The file name cannot be longer than 255 characters.")]
        public string Name { get; set; }

        [MaxLength(1000, ErrorMessage = "The description cannot be longer than 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "The visibility field is required.")]
        public string Visibility { get; set; }

        public string? SharedUserIds { get; set; }

        public string Permission { get; set; }
    }
}