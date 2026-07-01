using HRCoreDB.Data;
using HRCoreDB.Extensions;
using HRCoreService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().ConfigureApiBehaviorOptions(option =>
{
    // Tắt tính năng tự động chặn lỗi model
    option.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddEndpointsApiExplorer();

// --- 1. CẤU HÌNH CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",       // Vite dev server
                "http://localhost:5174",       // Vite fallback port
                "http://localhost:3000",       // Vite proxy port
                "http://192.168.61.30:3000",   // Máy HR (frontend + backend)
                "http://192.168.61.101:3000",  // Máy Attendance
                "http://192.168.61.36:3000"    // Máy Payroll
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// --- 2. CẤU HÌNH SWAGGER ---
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description  = "Chỉ cần paste token vào đây, Swagger tự thêm 'Bearer' ở đầu.",
        Name         = "Authorization",
        In           = ParameterLocation.Header,
        Type         = SecuritySchemeType.Http,   // Http scheme → tự thêm "Bearer "
        Scheme       = "bearer",                  // lowercase bắt buộc
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("HRCoreDB");
builder.Services.AddHRCore(connectionString!);
builder.Services.AddHostedService<ExpiredContractWorker>();
builder.Services.AddScoped<IEmailService, EmailService>();

// --- 2. CẤU HÌNH ĐỌC JWT TỪ APPSETTINGS ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey   = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true, // Kiểm tra thời gian hết hạn của token
        ValidateIssuerSigningKey = true, // Soi chữ ký của token
        ValidIssuer              = jwtSettings["Issuer"],
        ValidAudience            = jwtSettings["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(secretKey)
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- BẬT CORS (phải đặt trước Authentication) ---
app.UseCors("FrontendDev");

// --- BẬT BẢO VỆ API ---
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- SEED DỮ LIỆU DEMO (chạy 1 lần khi app khởi động) ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HRCoreDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.Run();