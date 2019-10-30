using Microsoft.EntityFrameworkCore;
using FileHash.Models;

namespace FileHash.Data
{
    public class FileHashContext : DbContext
    {
        public FileHashContext() { }
        public FileHashContext(DbContextOptions<FileHashContext> options) : base(options) { }

        public DbSet<FileMeta> FileMeta { get; set; }
    }
}