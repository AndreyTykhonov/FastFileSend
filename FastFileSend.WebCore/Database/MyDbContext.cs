using FastFileSend.Main.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FastFileSend.WebCore.DataBase
{
    public class MyDbContext : DbContext
    {
        public DbSet<HistoryModel> History { get; set; }
        public DbSet<FileItem> Files { get; set; }
        public DbSet<User> Users { get; set; }

        public MyDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Database=fastfilesend;MultipleActiveResultSets=true;Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }
    }
}
