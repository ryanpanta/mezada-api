using WebApiMezada.Models.Enums;

namespace WebApiMezada.DTOs.Task
{
    public class TaskCreateDTO
    {
        public string? Id { get; set; } = string.Empty;
        public string Title { get; set; }
        public string Description { get; set; }
        
        public EnumCategory Category { get; set; }
        public int DefaultIncrement { get; set; }
        public int LimitValue { get; set; }
        public int InitialValue { get; set; }
        public List<ChildAssignmentDTO> Children { get; set; } = new List<ChildAssignmentDTO>();
    }
}
