using BackendService.Enums;

namespace BackendService.Entities;

public class WorkTask
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Status { get; set; }
    public string? AssignedTo { get; set; }
}