namespace WebApiMezada.DTOs.Task
{
    public class CycleSummaryDTO
    {
        public string GroupId { get; set; } = string.Empty;
        public string CycleId { get; set; } = string.Empty;
        public decimal TotalPositiveBalance { get; set; }
        public decimal TotalNegativeBalance { get; set; }
        public List<TaskBalanceDTO> TaskBalances { get; set; } = new List<TaskBalanceDTO>();
    }
}
