using BlogApp.API.Extensions;
using BlogApp.API.Images.Extensions;
using BlogApp.API.Middlewares;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Service Configuration
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();

// Validators
builder.Services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

// Database Configuration
builder.Services.AddDbContexts(builder.Configuration);

// Repository Configuration
builder.Services.AddRepositories();

// Security and Identity Configuration
builder.Services.AddBlogAppIdentity();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.ConfigureIdentityOptions();

// API Documentation and Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

// CORS Configuration
builder.Services.AddCorsSettings(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware Configuration
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseBlogAppStaticFiles();
app.MapControllers();

// Initialize Database
await app.Services.InitializeDBAsync();

app.Run();
