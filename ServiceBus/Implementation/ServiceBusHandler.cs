using Azure.Messaging.ServiceBus;
using BackendService.Configuration;
using BackendService.DTOs;
using BackendService.ServiceBus.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BackendService.ServiceBus.Implementation;

public class ServiceBusHandler : IServiceBusHandler
{
    private readonly ServiceBusSender _sender;
    private readonly ServiceBusSender _replySender;
    private readonly ServiceBusSender _listSender;

    public ServiceBusHandler(IOptions<AzureServiceBusSettings> options)
    {
        var client = new ServiceBusClient(options.Value.ConnectionString);
        _sender = client.CreateSender(options.Value.QueueName);
        _replySender = client.CreateSender(options.Value.ReplyQueueName);
        _listSender = client.CreateSender(options.Value.GetQueueName);
    }

    public async Task SendMessageAsync(WorkTaskDto workTask)
    {
        try
        {
            var body = JsonSerializer.Serialize(workTask);
            var message = new ServiceBusMessage(body)
            {
                MessageId = Guid.NewGuid().ToString()
            };

            await _sender.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task SendListTaskMessageAsync(List<WorkTaskDto> workTaskList)
    {
        try
        {
            var body = JsonSerializer.Serialize(workTaskList);
            var message = new ServiceBusMessage(body)
            {
                MessageId = Guid.NewGuid().ToString()
            };

            await _listSender.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task SendCreatedTaskMessageAsync(WorkTaskDto workTask)
    {
        try
        {
            var body = JsonSerializer.Serialize(workTask);
            var message = new ServiceBusMessage(body)
            {
                MessageId = Guid.NewGuid().ToString(),
                ApplicationProperties =
                {
                    ["IsResponse"] = true,
                    ["MessageType"] = "done"
                }
            };

            await _sender.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task SendMessageDoneAsync(string messageId, int statusCode, string message, string? body)
    {
        try
        {
            var messageDto = new MessageDto
            {
                StatusCode = statusCode,
                Body = body,
                MessageId = messageId,
                Message = message
            };
            var serializeObject = JsonConvert.SerializeObject(messageDto);
            var serviceBusMessage = new ServiceBusMessage(serializeObject)
            {
                MessageId = messageId,
                ApplicationProperties =
                {
                    ["IsResponse"] = true,
                    ["MessageType"] = "done"
                }
            };
            
            await _replySender.SendMessageAsync(serviceBusMessage);
            Console.WriteLine($"Acknowledgment sent for MessageId: {messageId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send acknowledgment: {ex.Message}");
        }
    }
}