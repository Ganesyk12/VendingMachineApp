namespace VendingMachineApp.Services;

public class RedisOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
    public string Password { get; set; } = "";
    public string EmailStream { get; set; } = "email_queue";
    public string ConsumerGroup { get; set; } = "email_workers";
    public string ConsumerName { get; set; } = "worker_1";
}
