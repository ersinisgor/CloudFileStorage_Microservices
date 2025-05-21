using Microsoft.EntityFrameworkCore;
using File = FileMetadataAPI.Models.File;
using FileShare = FileMetadataAPI.Models.FileShare;

namespace FileMetadataAPI.DataContext
{
    internal class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<File> Files { get; set; }
        public DbSet<FileShare> FileShares { get; set; }

        
    }
}