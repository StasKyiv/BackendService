using BackendService.DTOs;

namespace BackendService.ServiceBus.Interfaces;

public interface IServiceBusHandler
{
    Task SendMessageAsync(WorkTaskDto workTask);
    Task SendMessageDoneAsync(string messageId, int statusCode, string message, string? body);
    Task SendListTaskMessageAsync(List<WorkTaskDto> workTaskList);
    Task SendCreatedTaskMessageAsync(WorkTaskDto workTask);
}