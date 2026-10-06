using BarberWeb.Application.Services;
using BarberWeb.Application.Services.Interfaces;
using BarberWeb.Domain.Interfaces;
using BarberWeb.Infrastructure.Context;
using BarberWeb.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mySqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(mySqlConnectionString, ServerVersion.AutoDetect(mySqlConnectionString)));

//Repo
builder.Services.AddScoped<IOpeningHoursRepository, EfOpeningHoursRepository>();
builder.Services.AddScoped<ISchedulingHoursRepository, EfSchedulingHoursRepository>();
builder.Services.AddScoped<IProfessionalServiceOfferingRepository, EfProfessionalServiceOfferingRepository>();
//Services
builder.Services.AddScoped<IAvailableHoursService, AvailableHoursService>();
builder.Services.AddScoped<IOpeningHoursService, OpeningHoursService>();
builder.Services.AddScoped<IProfessionalServiceOfferingService, ProfessionalServiceOfferingService>();
builder.Services.AddScoped<ISchedulingHoursService, SchedulingHoursService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
