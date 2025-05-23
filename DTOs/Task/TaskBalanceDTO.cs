namespace WebApiMezada.DTOs.Task
{
    public class TaskBalanceDTO
    {
        public string TaskId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public decimal BonusBalance { get; set; }
    }
}
