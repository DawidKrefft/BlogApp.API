using BlogApp.API.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.API.Data.Configurations
{
    public class BlogImageConfiguration : IEntityTypeConfiguration<BlogImage>
    {
        public void Configure(EntityTypeBuilder<BlogImage> builder)
        {
            // Required
            builder.Property(b => b.FileName).IsRequired();
            builder.Property(b => b.FileExtension).IsRequired();
            builder.Property(b => b.Title).IsRequired();
            builder.Property(b => b.Url).IsRequired();
        }
    }
}
