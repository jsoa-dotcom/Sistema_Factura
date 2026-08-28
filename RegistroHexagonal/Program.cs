using Microsoft.EntityFrameworkCore;
using RegistroHexagonal.Application.Ports;
using RegistroHexagonal.Application.UseCases;
using RegistroHexagonal.Infrastructure.Persistence;
using RegistroHexagonal.Infrastructure.Presentation.Middleware;
using RegistroHexagonal.Presentation.Endpoints;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        buffered: true,
        flushToDiskInterval: TimeSpan.FromSeconds(2),
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=facturacion.db"));

builder.Services.AddScoped<IClienteRepository, SqliteClienteRepository>();
builder.Services.AddScoped<ICategoriaRepository, SqliteCategoriaRepository>();
builder.Services.AddScoped<IProductoRepository, SqliteProductoRepository>();
builder.Services.AddScoped<IFacturaRepository, SqliteFacturaRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IFacturaService, FacturaService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapClienteEndpoints();
app.MapCategoriaEndpoints();
app.MapProductoEndpoints();
app.MapFacturaEndpoints();

app.Run();