using ExcelFuncReader.Data;
using ExcelFuncReader.Data.Entity;
using ExcelFuncReader.Models;
using StackExchange.Redis;

namespace ExcelFuncReader.Services;

public class RedisAggregator(IConnectionMultiplexer redis)
{
    public async Task CacheAggregatesAsync(IEnumerable<FunctionRecord> records)
    {
        var database = redis.GetDatabase();

        var grouped = records
            .Where(record => !string.IsNullOrWhiteSpace(record.OrganizationCode)
                && !string.IsNullOrWhiteSpace(record.CodeStructuralUnit)
                && !string.IsNullOrWhiteSpace(record.FunctionDescription))
            .GroupBy(record => new { record.OrganizationCode, record.CodeStructuralUnit });

        foreach (var group in grouped)
        {
            var key = BuildKey(group.Key.OrganizationCode, group.Key.CodeStructuralUnit);
            var values = group
                .Select(record => record.FunctionDescription.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct()
                .Select(value => (RedisValue)value)
                .ToArray();

            if (values.Length > 0)
            {
                await database.SetAddAsync(key, values);
            }
        }
    }

    private static string BuildKey(string organizationCode, string codeStructuralUnit)
    {
        return $"rsmv:org:{organizationCode}:unit:{codeStructuralUnit}:functions";
    }
}
