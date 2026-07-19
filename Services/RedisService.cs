using StackExchange.Redis;

namespace VendingMachineApp.Services;

public interface IRedisService
{
    bool IsAvailable { get; }
    Task PublishEmailAsync(string toEmail, string subject, string htmlMessage, byte[]? attachmentBytes = null, string? attachmentFileName = null);
    Task SetCacheAsync(string key, string value, TimeSpan expiry);
    Task<string?> GetCacheAsync(string key);
    Task RemoveCacheAsync(string key);
}

public class RedisService : IRedisService
{
    private readonly ConnectionMultiplexer? _redis;
    private readonly RedisOptions _options;
    private readonly ILogger<RedisService> _logger;

    public bool IsAvailable { get; }

    public RedisService(RedisOptions options, ILogger<RedisService> logger)
    {
        _options = options;
        _logger = logger;

        try
        {
            var config = new ConfigurationOptions
            {
                EndPoints = { $"{options.Host}:{options.Port}" },
                AbortOnConnectFail = false,
                ConnectTimeout = 3000
            };

            if (!string.IsNullOrEmpty(options.Password))
                config.Password = options.Password;

            _redis = ConnectionMultiplexer.Connect(config);
            IsAvailable = _redis.IsConnected;

            if (!IsAvailable)
                _logger.LogWarning("Redis connected but not ready at {Host}:{Port}", options.Host, options.Port);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis is not available at {Host}:{Port}. Redis features will be disabled.", options.Host, options.Port);
            IsAvailable = false;
            _redis = null;
        }
    }

    public async Task PublishEmailAsync(string toEmail, string subject, string htmlMessage, byte[]? attachmentBytes = null, string? attachmentFileName = null)
    {
        if (!IsAvailable || _redis == null)
        {
            _logger.LogWarning("Redis not available, skipping email to {ToEmail}", toEmail);
            return;
        }

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
        }
    }

    public async Task SetCacheAsync(string key, string value, TimeSpan expiry)
    {
        if (!IsAvailable || _redis == null) return;

        try
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(key, value, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis SetCacheAsync failed for key {Key}", key);
        }
    }

    public async Task<string?> GetCacheAsync(string key)
    {
        if (!IsAvailable || _redis == null) return null;

        try
        {
            var db = _redis.GetDatabase();
            return await db.StringGetAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis GetCacheAsync failed for key {Key}", key);
            return null;
        }
    }

    public async Task RemoveCacheAsync(string key)
    {
        if (!IsAvailable || _redis == null) return;

        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis RemoveCacheAsync failed for key {Key}", key);
        }
    }
}
