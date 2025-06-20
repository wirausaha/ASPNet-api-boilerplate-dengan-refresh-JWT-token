using AspApi.Data;
using AspApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using AspApi.DTOServices;
using Microsoft.AspNetCore.Http.Features;



var builder = WebApplication.CreateBuilder(args);

// Tambahkan services ke dalam kontainer.
// Add services to the container.

builder.Services.AddControllers();
// Swagger/OpenAPI services can be deleted if not needed
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

/*=========
| Modifikasi untuk konek ke Railway
=========== */
var connectionString = builder.Configuration.GetConnectionString("Default") 
                      ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default");



/*==========================================
| ini untuk koneksi ke database MySQL     |
| This is for MySQL database connection   |
==========================================*/ 
// builder.Services.AddDbContext<DataContext>(options =>
//     options.UseMySql(builder.Configuration.GetConnectionString("MySqlConnection"),
//     new MySqlServerVersion(new Version(8, 0, 29)))
//     );

/*==========================================
| ini untuk koneksi ke database Postgre     |
| This is for PostgreSQL database connection|
==========================================*/ 
// Kalau tidak menggunakan Railway ambil dari appsetting.json
//connectionString = (builder.Configuration.GetConnectionString("PostgreDocker")); 
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(connectionString));

/* ==== Catatan diatas untuk PostGreSQL ====*/

builder.Services.AddAuthorization();


builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<SysTokenService>();

builder.Services.AddScoped<ValidasiTokenService>();


// Daftarkan service untuk deteksi bahasa
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILanguageProvider, LanguageProvider>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder.WithOrigins("http://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // Maks ukuran 10MB
});

var app = builder.Build();

/*============= Khusus Railway, kalau bukan di remark ====== */
// Bind ke port dari Railway / Render / Fly.io
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

// Configure the HTTP request pipeline.
// Konfigurasi pipeline permintaan HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Middleware untuk menentukan bahasa berdasarkan header Accept-Language
// Middleware to determine language based on Accept-Language header
app.Use(async (context, next) =>
{
    var langHeader = context.Request.Headers["Accept-Language"].FirstOrDefault();
    context.Items["Lang"] = langHeader?.StartsWith("id", StringComparison.OrdinalIgnoreCase) == true ? "id" : "en";
    await next();
});


app.UseCors("AllowFrontend");


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseStaticFiles();

// Routing sederhana
app.MapGet("/", () => "API ASP.NET Core kamu sudah jalan!");

app.Run();
