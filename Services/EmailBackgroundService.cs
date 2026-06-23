using System.Text.Json;
using StackExchange.Redis;

namespace VendingMachineApp.Services;

public class EmailBackgroundService : BackgroundService
{
    private readonly RedisOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailBackgroundService> _logger;
    private ConnectionMultiplexer? _redis;

    public EmailBackgroundService(RedisOptions options, IServiceProvider serviceProvider, ILogger<EmailBackgroundService> logger)
    {
        _options = options;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConfigurationOptions
        {
            EndPoints = { $"{_options.Host}:{_options.Port}" },
            AbortOnConnectFail = false
        };

        if (!string.IsNullOrEmpty(_options.Password))
            config.Password = _options.Password;

        _redis = await ConnectionMultiplexer.ConnectAsync(config);
        var db = _redis.GetDatabase();

        await EnsureConsumerGroupAsync(db);

        _logger.LogInformation("EmailBackgroundService started, listening to stream: {Stream}", _options.EmailStream);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var messages = await db.StreamReadGroupAsync(
                    _options.EmailStream,
                    _options.ConsumerGroup,
                    _options.ConsumerName,
                    count: 1,
                    position: ">"
                );

                foreach (var message in messages)
                {
                    await ProcessMessageAsync(db, message, stoppingToken);
                }

                if (messages.Length == 0)
                    await Task.Delay(1000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading from Redis stream");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task EnsureConsumerGroupAsync(IDatabase db)
    {
        try
        {
            await db.StreamCreateConsumerGroupAsync(_options.EmailStream, _options.ConsumerGroup, "0-0", true);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Consumer group already exists
        }
    }

    private async Task ProcessMessageAsync(IDatabase db, StreamEntry message, CancellationToken stoppingToken)
    {
        var messageId = message.Id;
        var fields = message.Values.ToDictionary(v => v.Name.ToString(), v => v.Value.ToString());

        var toEmail = fields.GetValueOrDefault("toEmail", "");
        var subject = fields.GetValueOrDefault("subject", "");
        var htmlMessage = fields.GetValueOrDefault("htmlMessage", "");
        var attachmentBase64 = fields.GetValueOrDefault("attachmentBytes");
        var attachmentFileName = fields.GetValueOrDefault("attachmentFileName");

        _logger.LogInformation("Processing email message {MessageId} for {ToEmail}", messageId, toEmail);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            bool success;
            if (!string.IsNullOrEmpty(attachmentBase64))
            {
                var fileBytes = Convert.FromBase64String(attachmentBase64);
                success = await emailService.SendEmailWithAttachmentAsync(toEmail, subject, htmlMessage, fileBytes, attachmentFileName ?? "attachment.pdf");
            }
            else
            {
                success = await emailService.SendEmailAsync(toEmail, subject, htmlMessage);
            }

            if (success)
            {
                await db.StreamAcknowledgeAsync(_options.EmailStream, _options.ConsumerGroup, messageId);
                _logger.LogInformation("Email {MessageId} sent and acknowledged", messageId);
            }
            else
            {
                _logger.LogWarning("Email {MessageId} failed to send, will be retried", messageId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception processing email message {MessageId}", messageId);
        }
    }

    public override void Dispose()
    {
        _redis?.Dispose();
        base.Dispose();
    }
}
