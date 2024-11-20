namespace BackendService.Configuration;

public class AzureServiceBusSettings
{
    public string ConnectionString { get; set; }
    public string QueueName { get; set; }
    public string ReplyQueueName { get; set; }
    public string GetQueueName { get; set; }
}