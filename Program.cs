global using expense_tracker.Dtos;
global using expense_tracker.Data;
global using expense_tracker.Services;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Mvc;
using System.Text;
using expense_tracker.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

// configuring JWT
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

//Adding DIs
builder.Services.AddScoped<Argon2HasherService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddDbContext<ExpenseTrackerContext>(options =>
{
    options.UseNpgsql("Name=ConnectionStrings:DefaultConnection")
        .LogTo(Console.WriteLine, LogLevel.Information);
});

// adding JWT Auth
var jwtConfig = builder.Configuration.GetSection("JwtConfig").Get<JwtConfig>();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(jwt =>
    {
        var key = Encoding.ASCII.GetBytes(jwtConfig!.Secret);
        jwt.SaveToken = true;

        jwt.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ValidIssuer = jwtConfig!.Issuer,
            ValidAudience = jwtConfig!.Audience,
        };
    });

builder.Services.AddAuthorization();

// adding CORS for our frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
