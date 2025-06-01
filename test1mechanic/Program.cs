using test1mechanic.Repositories;
using test1mechanic.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();
builder.Services.AddScoped<VisitService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run("https://localhost:5001/");