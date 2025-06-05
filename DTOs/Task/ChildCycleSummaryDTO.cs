namespace WebApiMezada.DTOs.Task
{
    public class ChildCycleSummaryDTO
    {
        public string ChildId { get; set; } = string.Empty;
        public string ChildName { get; set; } = string.Empty;
        public int TotalBalance { get; set; } // CurrentBalance + BonusBalance (sugestão de mesada)
        public int TotalBonus { get; set; }
        public int RewardBalance { get; set; } // Contribuição das recompensas
        public int PenaltyBalance { get; set; } // Contribuição das penalidades
        public List<BalanceHistoryDTO> BalanceHistory { get; set; } = new List<BalanceHistoryDTO>(); // Evolução do saldo
    }
}
