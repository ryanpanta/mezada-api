namespace WebApiMezada.DTOs.Task
{
    public class RemoveChildFromTaskDTO
    {
        public string TaskId { get; set; } = string.Empty;
        public string ChildId { get; set; } = string.Empty;
    }
}