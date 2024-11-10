using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BloggingPlatform.Data.Entities;

namespace BloggingPlatform.Data.EntityConfigurations;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPosts");

        builder.HasKey(bp => bp.Id);

        builder.Property(bp => bp.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(bp => bp.Content)
            .IsRequired();

        builder.Property(bp => bp.CreatedAt)
            .IsRequired();

        builder.Property(bp => bp.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(bp => bp.UpdatedAt);

        builder.Property(bp => bp.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(bp => bp.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(bp => bp.DeletedDate);
        
        builder.HasMany(bp => bp.Comments)
            .WithOne(c => c.BlogPost)
            .HasForeignKey(c => c.BlogPostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}