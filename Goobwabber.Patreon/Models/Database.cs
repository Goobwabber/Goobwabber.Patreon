using Microsoft.EntityFrameworkCore;
using System;

namespace Goobwabber.Patreon.Models
{
    public class Database : DbContext
    {
        public Database(DbContextOptions<Database> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<User> Users { get; set; }

        public class User
        {
            public string UserId { get; set; }
            public bool Patron { get; set; }
            public DateTime LastCheckDate { get; set; }
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public DateTime TokenExpiry { get; set; }
        }
    }
}
