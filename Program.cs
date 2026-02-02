using ExcelFuncReader.Data;
using ExcelFuncReader.Models;
using ExcelFuncReader.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));

builder.Services.AddScoped<ExcelParser>();
builder.Services.AddScoped<RedisAggregator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/imports", async (IFormFile file, AppDbContext dbContext, ExcelParser parser) =>
{
    if (file.Length == 0)
    {
        return Results.BadRequest(new { message = "File is empty." });
    }

    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
    if (extension is not ".xlsx" and not ".xls")
    {
        return Results.BadRequest(new { message = "Only .xlsx or .xls files are supported." });
    }

    var importJob = new ImportJob
    {
        FileName = file.FileName,
        Status = ImportStatus.Processing,
        CreatedAt = DateTimeOffset.UtcNow
    };

    dbContext.ImportJobs.Add(importJob);
    await dbContext.SaveChangesAsync();

    try
    {
        await using var stream = file.OpenReadStream();
        var records = parser.Parse(stream, importJob.Id);

        dbContext.FunctionRecords.AddRange(records);
        importJob.Status = ImportStatus.Completed;
        importJob.CompletedAt = DateTimeOffset.UtcNow;
        importJob.TotalRows = records.Count;
        await dbContext.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        importJob.Status = ImportStatus.Failed;
        importJob.ErrorMessage = ex.Message;
        importJob.CompletedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync();
        return Results.Problem("Failed to process the file.");
    }

    return Results.Accepted($"/imports/{importJob.Id}/status", new { importJob.Id });
});

app.MapGet("/imports/{id:guid}/status", async (Guid id, AppDbContext dbContext) =>
{
    var job = await dbContext.ImportJobs.FindAsync(id);
    return job is null
        ? Results.NotFound()
        : Results.Ok(job);
});

app.MapGet("/imports/{id:guid}/results", async (
    Guid id,
    AppDbContext dbContext,
    RedisAggregator aggregator) =>
{
    var job = await dbContext.ImportJobs.FindAsync(id);
    if (job is null)
    {
        return Results.NotFound();
    }

    var records = await dbContext.FunctionRecords
        .Where(record => record.ImportJobId == id)
        .OrderBy(record => record.RowNumber)
        .ToListAsync();

    await aggregator.CacheAggregatesAsync(records);

    return Results.Ok(new
    {
        job.Id,
        job.FileName,
        job.Status,
        job.CreatedAt,
        job.CompletedAt,
        job.TotalRows,
        Records = records
    });
});

app.Run();
