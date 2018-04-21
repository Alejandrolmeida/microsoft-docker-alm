using Microsoft.EntityFrameworkCore;

namespace Selfie.Backend.Models
{
    public class BootCampContext : DbContext
    {
        public BootCampContext(DbContextOptions<BootCampContext> options)
            : base(options)
        {
        }

        public DbSet<Assistant> Assistants { get; set; }
    }
}