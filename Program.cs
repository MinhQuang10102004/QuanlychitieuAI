using System.Text;
using OpenAI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuanLyChiTieu;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRazorPages();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ChiTieuContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ChiTieuDB")));

builder.Services.AddScoped<IPasswordHasher<NguoiDung>, PasswordHasher<NguoiDung>>();
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["OpenAI:ApiKey"];

    if (string.IsNullOrWhiteSpace(apiKey))
    {
        throw new InvalidOperationException("OpenAI:ApiKey phải được cấu hình.");
    }

    return new OpenAIClient(apiKey);
});
builder.Services.AddScoped<AiService>();
// builder.Services.AddScoped<ChatBotService>();
// builder.Services.AddScoped<AiInsightService>();
builder.Services.AddScoped<HouseholdService>();
builder.Services.AddScoped<SalaryBudgetService>();
builder.Services.AddScoped<InvitationService>();
builder.Services.AddScoped<QuanLyChiTieu.Services.INotificationService, QuanLyChiTieu.Services.NotificationService>();
builder.Services.AddScoped<HouseholdFinanceService>();
builder.Services.AddScoped<ExpensePredictionService>();
builder.Services.AddScoped<CalculateBudget>();
builder.Services.AddScoped<BudgetApprovalService>();
builder.Services.AddScoped<OptimizeSpending>();
builder.Services.AddScoped<AnalyzeSpendingTrend>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key");
var jwtIssuer = jwtSection.GetValue<string>("Issuer") ?? throw new InvalidOperationException("Jwt:Issuer is not configured");
var jwtAudience = jwtSection.GetValue<string>("Audience") ?? throw new InvalidOperationException("Jwt:Audience is not configured");

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Equals("set-in-env-or-user-secrets", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("Jwt:Key must be provided via secrets or environment variables.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

await SeedData.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseSession();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();

