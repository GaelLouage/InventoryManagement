using Infrastructuur.Entities;
using Infrastructuur.Repositories.Interfaces;
using MongoDB.Driver;
using Infrastructuur.Extensions;
using Infrastructuur.Constants;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
// add mongo database
builder.Services.AddMongoRepository(DbName.INVENTORYMANAGEMENTSYSTEM, "C:/Users/louag/OneDrive/Bureau/jsonmongodbConnection/appsettings.json", "MyMongoDBConnection");
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
