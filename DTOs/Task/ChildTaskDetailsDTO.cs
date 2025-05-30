namespace WebApiMezada.DTOs.Task
{
    public class ChildTaskDetailsDTO
    {
        public string ChildId { get; set; } = string.Empty;
        public string ChildName { get; set; } = string.Empty;
        public int CurrentBalance { get; set; }
        public int BonusBalance { get; set; }
        public int CustomIncrement { get; set; }
    }
    
   
}