
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;
using static Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId;
using RhenusTestApp;
using RhenusTestApp.Models;
using RhenusTestApp.Services;
using RhenusTestApp.Utils;

var builder = WebApplication.CreateBuilder(args);

var accessTokenSecret = builder.Configuration["Jwt:AccessTokenSecret"];
var isProduction = builder.Environment.IsProduction();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            Description =
                "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        }
    );
});

builder.Services.AddDbContext<AppDbContext>(
    optionsBuilder =>
        optionsBuilder
            .UseSqlite("Data Source=app.db")
            .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .EnableDetailedErrors()
            .ConfigureWarnings(b => b.Log(ConnectionOpened, CommandExecuted, ConnectionClosed))
);


builder.Services.AddSingleton<TokenGenerator>();
builder.Services.AddSingleton<TokenValidator>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddIdentityCore<User>(options =>
    {
        options.User.RequireUniqueEmail = false;
        options.Password.RequireDigit = isProduction;
        options.Password.RequireLowercase = isProduction;
        options.Password.RequireNonAlphanumeric = isProduction;
        options.Password.RequireUppercase = isProduction;
        if (isProduction)
        {
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 3;
        }
    })
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessTokenSecret)),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.SaveToken = true;
    });

builder.Services.AddAuthorization(options =>
{

    options.AddPolicy(
        "user",
        policy => policy.RequireAuthenticatedUser()
    );
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("signup", Users.SignUpAsync);
app.MapPost("signin", Users.SignInAsync);
app.MapPost("refresh", Auth.RefreshTokenAsync);
app.MapDelete("signout", Auth.SignOutAsync);
app.MapPost(
        "game", Game.PlayAsync)
    .RequireAuthorization("user");


app.Run();
