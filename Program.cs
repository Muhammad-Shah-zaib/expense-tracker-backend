global using expense_tracker.Dtos;
global using expense_tracker.Data;
global using expense_tracker.Services;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Adding DIs
builder.Services.AddScoped<Argon2HasherService>();

builder.Services.AddDbContext<ExpenseTrackerContext>(options =>
{
    options.UseNpgsql("Name=ConnectionStrings:DefaultConnection");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
