using StackExchange.Redis;

namespace VendingMachineApp.Services;

public interface IRedisService
{
    Task PublishEmailAsync(string toEmail, string subject, string htmlMessage, byte[]? attachmentBytes = null, string? attachmentFileName = null);
    Task SetCacheAsync(string key, string value, TimeSpan expiry);
    Task<string?> GetCacheAsync(string key);
    Task RemoveCacheAsync(string key);
}

public class RedisService : IRedisService
{
    private readonly ConnectionMultiplexer _redis;
    private readonly RedisOptions _options;
    private readonly ILogger<RedisService> _logger;

    public RedisService(RedisOptions options, ILogger<RedisService> logger)
    {
        _options = options;
        _logger = logger;

        var config = new ConfigurationOptions
        {
            EndPoints = { $"{options.Host}:{options.Port}" },
            AbortOnConnectFail = false
        };

        if (!string.IsNullOrEmpty(options.Password))
            config.Password = options.Password;

        _redis = ConnectionMultiplexer.Connect(config);
    }

    public async Task PublishEmailAsync(string toEmail, string subject, string htmlMessage, byte[]? attachmentBytes = null, string? attachmentFileName = null)
    {
        try
        {
            var db = _redis.GetDatabase();

            var data = new Dictionary<string, string>
            {
                { "toEmail", toEmail },
                { "subject", subject },
                { "htmlMessage", htmlMessage }
            };

            if (attachmentBytes != null)
                data["attachmentBytes"] = Convert.ToBase64String(attachmentBytes);
            if (!string.IsNullOrEmpty(attachmentFileName))
                data["attachmentFileName"] = attachmentFileName;

            var fields = data.Select(kv => new NameValueEntry(kv.Key, kv.Value)).ToArray();

            await db.StreamAddAsync(_options.EmailStream, fields);

            _logger.LogInformation("Email queued to Redis stream for {ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish email to Redis stream for {ToEmail}", toEmail);
            throw;
        }
    }

    public async Task SetCacheAsync(string key, string value, TimeSpan expiry)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> GetCacheAsync(string key)
    {
        var db = _redis.GetDatabase();
        return await db.StringGetAsync(key);
    }

    public async Task RemoveCacheAsync(string key)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
    }
}
