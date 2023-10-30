using BlogApp.API.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.API.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            //Required
            builder.Property(c => c.Name).IsRequired();
            builder.Property(c => c.UrlHandle).IsRequired();
        }
    }
}
