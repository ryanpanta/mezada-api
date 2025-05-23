namespace WebApiMezada.DTOs.TaskHistory;

public class TaskHistoryListDTO
{
    public string Id { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string ChildId { get; set; } = string.Empty;
    public string ChildName { get; set; } = string.Empty;
    public string AccountedById { get; set; } = string.Empty;
    public string AccountedByName { get; set; } = string.Empty;
    public int Value { get; set; }
    public DateTime AccountedAt { get; set; }
    public bool IsReverted { get; set; }
}