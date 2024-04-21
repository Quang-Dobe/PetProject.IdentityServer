using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetProject.Identity.Data.Identity;
using PetProject.Identity.Utils;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var identityConnectionString = builder.Configuration.GetConnectionString("Identity");
var identityMigrationName = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

builder.Services.AddMyIdentity<IdentityUser, IdentityRole, MyIdentityDbContext>();
builder.Services.AddMyIdentityDbContext<MyIdentityDbContext>(identityConnectionString, identityMigrationName);
builder.Services.AddMyIdentityServer4<IdentityUser>(identityConnectionString, identityMigrationName);

// Seeding data
//MyIdentitySeeding.EnsureSeedData<IdentityUser, IdentityRole, MyIdentityDbContext>(identityConnectionString, identityMigrationName);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseIdentityServer();

app.UseAuthorization();

app.MapControllers();

app.Run();
