using BlogApp.API.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.API.Data.Configurations
{
    public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
    {
        public void Configure(EntityTypeBuilder<BlogPost> builder)
        {
            //Required
            builder.Property(b => b.Title).IsRequired();
            builder.Property(b => b.ShortDescription).IsRequired();
            builder.Property(b => b.Content).IsRequired();
            builder.Property(b => b.FeaturedImageUrl).IsRequired();
            builder.Property(b => b.UrlHandle).IsRequired();
            builder.Property(b => b.PublishedDate).IsRequired();
            builder.Property(b => b.Author).IsRequired();
            builder.Property(b => b.IsVisible).IsRequired();
        }
    }
}
