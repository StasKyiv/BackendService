using BackendService.Enums;

namespace BackendService.DTOs;

public class WorkTaskDto
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public string? AssignedTo { get; set; }
}