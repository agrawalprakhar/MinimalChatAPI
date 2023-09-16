using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Models;
using System.Collections.Generic;

namespace MinimalChatApplication.Data
{
    // DataContext.cs
    public class MinimalChatContext : DbContext
    {
        public MinimalChatContext(DbContextOptions<MinimalChatContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Message>().ToTable("Message");
            modelBuilder.Entity<Logs>().ToTable("Log");


            //modelBuilder.Entity<Message>()
            //   .HasOne(m => m.Receiver)
            //   .WithMany()
            //   .HasForeignKey(m => m.ReceiverId)
            //   .OnDelete(DeleteBehavior.NoAction);
            ////configure sender
            //modelBuilder.Entity<Message>()
            //  .HasOne(m => m.Sender)
            //  .WithMany()
            //  .HasForeignKey(m => m.SenderId)
            //  .OnDelete(DeleteBehavior.NoAction);
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }

        public DbSet<Logs> Log { get; set; }



    }

}
