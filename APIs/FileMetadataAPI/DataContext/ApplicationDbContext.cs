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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<File>()
                .HasMany(f => f.FileShares)
                .WithOne(fs => fs.File)
                .HasForeignKey(fs => fs.FileId);

            modelBuilder.Entity<FileShare>()
                .HasKey(fs => new { fs.FileId, fs.UserId });
        }


    }
}