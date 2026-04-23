using CommentsApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using CommentsApp.Application.Models;

namespace CommentsApp.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentAttachment> CommentAttachments =>Set<CommentAttachment>();
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>()
                    .HasOne(c=>c.Parent)
                    .WithMany(c=>c.Replies)
                    .HasForeignKey(c=>c.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<CommentAttachment>(entity =>
        {
            entity.ToTable("CommentAttachment");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.Path)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Size)
                .IsRequired();

            entity.Property(x => x.IsImage)
                .IsRequired();

            entity.HasOne(x => x.Comment)
                .WithMany(c => c.Attachments)
                .HasForeignKey(x => x.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.CommentId);
        });

    }
}