using WebApiMezada.Models.Enums;

namespace WebApiMezada.DTOs.Task
{
    public class ChildAssignmentDTO
    {
        public string ChildId { get; set; } = string.Empty;
        public int CustomIncrement { get; set; }
    }
}
