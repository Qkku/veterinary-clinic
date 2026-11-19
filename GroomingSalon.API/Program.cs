
using System.Text;
using System.Security.Claims;
using GroomingSalon.API.Data;
using GroomingSalon.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using GroomingSalon.API.Services;
using GroomingSalon.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);
}

builder.Services.AddScoped<ScheduleService>();
builder.Services.AddHostedService<AppointmentCompletionService>();
builder.Services.AddControllers();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IPasswordHasher<Groomer>, PasswordHasher<Groomer>>();
builder.Services.AddSignalR();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Введите JWT-токен"
    });

    options.AddSecurityRequirement(document =>
        new Microsoft.OpenApi.OpenApiSecurityRequirement
        {
            [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "JWT key не задан или короче 32 байт. Добавьте Jwt:Key через User Secrets.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GroomingSalon.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GroomingSalon.Frontend";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // Проверка блокировки при каждом запросе с JWT
        options.Events = new JwtBearerEvents
        {

            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments(
                        "/hubs/notifications"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },

            OnTokenValidated = async context =>
            {
                var idValue = context.Principal?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!uint.TryParse(idValue, out var userId))
                {
                    context.Fail("Некорректный токен.");
                    return;
                }

                var role = context.Principal?
                    .FindFirst(ClaimTypes.Role)?.Value;

                var db = context.HttpContext.RequestServices
                    .GetRequiredService<ApplicationDbContext>();

                if (role == "Groomer")
                {
                    var groomerIsActive = await db.Groomers
                        .AsNoTracking()
                        .AnyAsync(g => g.Id == userId && g.IsActive == true);

                    if (!groomerIsActive)
                        context.Fail("Аккаунт мастера неактивен или не существует.");
                }
                else if (role == "Admin" || role == "Customer")
                {
                    var userIsActive = await db.Users
                        .AsNoTracking()
                        .AnyAsync(u => u.Id == userId && !u.IsBanned);

                    if (!userIsActive)
                        context.Fail("Аккаунт заблокирован или не существует.");
                }
                else
                {
                    context.Fail("Неизвестная роль пользователя.");
                }
            }

        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseStaticFiles();

app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications")
   .RequireAuthorization();

app.Run();
