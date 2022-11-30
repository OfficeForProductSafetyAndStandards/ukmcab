using UKMCAB.Common.ConnectionStrings;

namespace UKMCAB.Infrastructure.Cache;

public class RedisConnectionString : ConnectionString
{
    public RedisConnectionString(string dataConnectionString) : base(dataConnectionString) { }
    public static implicit operator string(RedisConnectionString d) => d._connectionString;
    public static implicit operator RedisConnectionString(string d) => new(d);
}