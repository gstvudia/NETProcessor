
using Microsoft.EntityFrameworkCore;

namespace NETProcessor.API.Data
{
    public class Context : DbContext
    {
        public Context(DbContextOptions options) : base(options){}

        //public DbSet<Values> Values { get; set; }
        //public DbSet<User> Users { get; set; }
        //public DbSet<Photo> Photos { get; set; }
    }
}
