using BloggingPlatform.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloggingPlatform.Data;

public class BloggingPlatformDbContext(DbContextOptions<BloggingPlatformDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BloggingPlatformDbContext).Assembly);
    }
}