using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;
using LibraryManagementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Use console logging so the application can run without Windows Event Log access.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DbContext
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Register Services
builder.Services.AddScoped<LibraryService>();

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key not configured in appsettings.json");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

var app = builder.Build();

// Ensure the database and tables exist and seed initial data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    db.Database.EnsureCreated();

    // Seed default Admin if none exists
    if (!db.Users.Any(u => u.Role == UserRole.Admin))
    {
        db.Users.Add(new User
        {
            FullName = "Head Curator",
            Email = "admin@library.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }

    // Seed sample books if catalogue is empty
    if (!db.Books.Any())
    {
        db.Books.AddRange(
            new Book
            {
                Title = "The Great Gatsby",
                Author = "F. Scott Fitzgerald",
                ISBN = "978-0743273565",
                TotalCopies = 5,
                AvailableCopies = 5,
                PublishedYear = 1925
            },
            new Book
            {
                Title = "To Kill a Mockingbird",
                Author = "Harper Lee",
                ISBN = "978-0446310789",
                TotalCopies = 4,
                AvailableCopies = 4,
                PublishedYear = 1960
            },
            new Book
            {
                Title = "1984",
                Author = "George Orwell",
                ISBN = "978-0451524935",
                TotalCopies = 6,
                AvailableCopies = 6,
                PublishedYear = 1949
            },
            new Book
            {
                Title = "Dune",
                Author = "Frank Herbert",
                ISBN = "978-0441172719",
                TotalCopies = 3,
                AvailableCopies = 3,
                PublishedYear = 1965
            }
        );
        db.SaveChanges();
    }

    // Seed sample newspapers
    if (!db.Newspapers.Any())
    {
        db.Newspapers.AddRange(
            new Newspaper { Title = "The Daily Times", Publisher = "Times Media", Language = "English", PublicationDate = DateTime.UtcNow, Copies = 10 },
            new Newspaper { Title = "National Chronicle", Publisher = "Chronicle Group", Language = "English", PublicationDate = DateTime.UtcNow, Copies = 5 }
        );
        db.SaveChanges();
    }

    // Seed sample magazines
    if (!db.Magazines.Any())
    {
        db.Magazines.AddRange(
            new Magazine { Title = "National Geographic", Category = "Science & Nature", Publisher = "NG Media", IssueNumber = "Vol. 245", PublishedYear = 2024, Copies = 8 },
            new Magazine { Title = "Tech Monthly", Category = "Technology", Publisher = "TechPress", IssueNumber = "Issue 112", PublishedYear = 2025, Copies = 6 }
        );
        db.SaveChanges();
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve the default frontend document before resolving static files.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
