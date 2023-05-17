using System;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using RhenusTestApp.Models;
using RhenusTestApp.Services;

namespace RhenusTestApp;


public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Token> Tokens { get; set; }
    public DbSet<GameProfile> GameProfiles { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Token>().HasIndex(b => b.UserId);
        builder.Entity<User>().HasOne(u => u.GameProfile).WithOne(g => g.User).HasForeignKey<GameProfile>(g => g.UserId);

    }
}


