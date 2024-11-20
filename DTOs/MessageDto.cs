namespace BackendService.DTOs;

public class MessageDto
{
    public int StatusCode { get; set; }
    public string MessageId { get; set; }
    public string? Body { get; set; }
    public string? Message { get; set; }
}