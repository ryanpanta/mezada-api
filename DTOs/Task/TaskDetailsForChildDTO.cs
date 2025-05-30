namespace WebApiMezada.DTOs.Task
{
    public class TaskDetailsForChildDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int InitialValue { get; set; }
        public int DefaultIncrement { get; set; }
        public int LimitValue { get; set; }
        public int CurrentBalance { get; set; }
        public int BonusBalance { get; set; }
    }
    
   
}