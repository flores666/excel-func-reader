using ExcelFuncReader.Data;
using ExcelFuncReader.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));

builder.Services.AddScoped<ExcelParser>();
builder.Services.AddScoped<RedisAggregator>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IImportResultsService, ImportResultsService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/imports", async (IFormFile file, IImportService importService, CancellationToken ct) =>
    {
        try
        {
            var jobId = await importService.ImportAsync(file, ct);
            return Results.Accepted($"/imports/{jobId}/status", new { Id = jobId });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return Results.Problem("Failed to process the file.");
        }
    })
    .DisableAntiforgery();

app.MapGet("/imports/{id:guid}/status", async (Guid id, AppDbContext dbContext) =>
{
    var job = await dbContext.ImportJobs.FindAsync(id);
    return job is null
        ? Results.NotFound()
        : Results.Ok(job);
});

app.MapGet("/imports/{id:guid}/results", async (
    Guid id,
    IImportResultsService resultsService,
    CancellationToken ct) =>
{
    try
    {
        var result = await resultsService.GetResultsAsync(id, ct);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.Run();