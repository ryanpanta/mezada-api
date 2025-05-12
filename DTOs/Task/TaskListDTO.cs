using WebApiMezada.Models.Enums;

namespace WebApiMezada.DTOs.Task
{
    public class TaskListDTO
    {
        public string Id { get; set; }
        public string Title { get; set; }
        
        public EnumCategory Category { get; set; }
        
        public List<string> ChildIds { get; set; } = new List<string>();

        public string FamilyGroupId { get; set; }
        
        public bool Active { get; set; }
        public DateOnly CreatedAt { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public bool IsIncreasing { get; set; }
        
        
    }
}
