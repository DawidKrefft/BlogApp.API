using BlogApp.API.Authorization;
using BlogApp.API.Cors;
using BlogApp.API.Data;
using BlogApp.API.Images.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddBlogAppIdentity();
builder.Services.ConfigureIdentityOptions();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddCorsSettings(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.UseBlogAppStaticFiles();
app.MapControllers();

app.Run();
