using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using TaskFlow.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Registre services into DI container
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.MapControllers();

app.Run();
