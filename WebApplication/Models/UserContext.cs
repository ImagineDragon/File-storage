using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace WebApplication.Models
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<File> Files { get; set; }
    }
}