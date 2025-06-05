namespace WebApiMezada.DTOs.Task
{
    public class CycleSummaryDTO
    {
        public string GroupId { get; set; } = string.Empty;
        public string CycleId { get; set; } = string.Empty;
        public int TotalPositiveBalance { get; set; } // Mantido para visão geral
        public int TotalNegativeBalance { get; set; } // Mantido para visão geral
        public List<ChildCycleSummaryDTO> ChildrenSummaries { get; set; } = new List<ChildCycleSummaryDTO>(); // Detalhes por filho
    }
}
