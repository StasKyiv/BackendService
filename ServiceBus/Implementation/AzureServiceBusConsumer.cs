using AutoMapper;
using Azure.Messaging.ServiceBus;
using BackendService.Configuration;
using BackendService.DTOs;
using BackendService.Entities;
using BackendService.Repository.Interfaces;
using BackendService.ServiceBus.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BackendService.ServiceBus.Implementation;

public class AzureServiceBusConsumer : IHostedService
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;
    private readonly IMapper _mapper;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AzureServiceBusConsumer(IOptions<AzureServiceBusSettings> options, IMapper mapper,
        IServiceScopeFactory serviceScopeFactory)
    {
        _mapper = mapper;
        _serviceScopeFactory = serviceScopeFactory;
        var settings = options.Value;

        _client = new ServiceBusClient(settings.ConnectionString);
        _processor = _client.CreateProcessor(settings.QueueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false
        });

        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _processor.StartProcessingAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync();
        await _processor.DisposeAsync();
        await _client.DisposeAsync();
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        try
        {
            Console.WriteLine($"Received message: {args.Message.MessageId}");

            var messageType = args.Message.ApplicationProperties["MessageType"].ToString();
            var isResponse = args.Message.ApplicationProperties["IsResponse"].ToString();
            if (!Convert.ToBoolean(isResponse))
            {
                switch (messageType)
                {
                    case "post":
                        await CreateTask(args);
                        break;
                    case "put":
                        await UpdateTask(args);
                        break;
                    case "get":
                        await GetTaskList();
                        break;
                }

                // Complete the message to remove it from the queue
                await args.CompleteMessageAsync(args.Message);
            }
        }
        catch (Exception ex)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var serviceBusHandler = scope.ServiceProvider.GetRequiredService<IServiceBusHandler>();
            await serviceBusHandler
                .SendMessageDoneAsync(args.Message.MessageId, 500, $"{ex.Message}", null);
            Console.WriteLine($"Message processing failed: {ex.Message}");
            await args.CompleteMessageAsync(args.Message);
        }
    }

    private async Task CreateTask(ProcessMessageEventArgs args)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBaseRepository<WorkTask>>();
        var serviceBusHandler = scope.ServiceProvider.GetRequiredService<IServiceBusHandler>();

        var body = args.Message.Body.ToString();
        var taskDto = JsonConvert.DeserializeObject<WorkTaskDto>(body);
        
        taskDto.Status = RemoveWhitespacesForStatus(taskDto.Status);
        
        var entity = _mapper.Map<WorkTask>(taskDto);
        
        await repository.CreateAsync(entity);
        
        var taskDtoList = _mapper.Map<WorkTaskDto>(entity);
        
        await serviceBusHandler
            .SendCreatedTaskMessageAsync(taskDtoList);
    }

    private async Task UpdateTask(ProcessMessageEventArgs args)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBaseRepository<WorkTask>>();
        var serviceBusHandler = scope.ServiceProvider.GetRequiredService<IServiceBusHandler>();

        var updateBodyDto = args.Message.Body.ToString();
        var updateTaskDto = JsonConvert.DeserializeObject<WorkTaskDto>(updateBodyDto);
        
        updateTaskDto.Status = RemoveWhitespacesForStatus(updateTaskDto.Status);
        
        var updateTask = _mapper.Map<WorkTask>(updateTaskDto);
        
        var taskFromDb = await repository.FindByIdAsync(updateTask.ID);
        if (taskFromDb != null)
        {
            taskFromDb.Status = updateTask.Status;
            await repository.UpdateAsync(taskFromDb);

            await serviceBusHandler
                .SendMessageDoneAsync(args.Message.MessageId,
                    200, "Processed successfully", null);
        }
    }

    private async Task GetTaskList()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBaseRepository<WorkTask>>();
        var serviceBusHandler = scope.ServiceProvider.GetRequiredService<IServiceBusHandler>();

        var taskList = await repository.GetAllAsync();

        var taskDtoList = _mapper.Map<List<WorkTaskDto>>(taskList);
        await serviceBusHandler
            .SendListTaskMessageAsync(taskDtoList);
    }

    private static string RemoveWhitespacesForStatus(string status)
    {
        return new string(status.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }
    
    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Error occurred: {args.Exception.Message}");
        return Task.CompletedTask;
    }
}