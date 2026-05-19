using DoctorAppBackend.Data;
using DoctorAppBackend.Models;
using DoctorAppBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BC = BCrypt.Net.BCrypt;


var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers(); // ✅ Required for API
builder.Services.AddControllersWithViews(); // Optional (if using MVC views)


//auhtentication 
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
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions => sqlOptions.CommandTimeout(60)));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:5173",
                        "https://localhost:5173"  // 👈 Add https too

        )
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});



//email service
// Add after other services
builder.Services.AddSingleton<EmailService>();  // 👈 Add this

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("AllowReactApp");

// 2. Identify WHO the user is (JWT Check)
app.UseAuthentication(); // 

//what user can do 

app.UseAuthorization();




using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Admins.Any())
    {
        db.Admins.Add(new Admin
        {
            FullName = "Super Admin",
            Email = "admin@clinic.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123")
        });
        db.SaveChanges();
    }
}

// ✅ THIS IS WHAT YOU WERE MISSING
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");




using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Admins.Any())
    {
        db.Admins.Add(new Admin
        {
            FullName = "Super Admin",
            Email = "admin@clinic.com",
            PasswordHash = BC.HashPassword("Admin@123")
        });
        db.SaveChanges();
    }
}

app.Run();