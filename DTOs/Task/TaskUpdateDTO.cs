using WebApiMezada.Models.Enums;

namespace WebApiMezada.DTOs.Task
{
    public class TaskUpdateDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public EnumCategory Category { get; set; }
        public int DefaultIncrement { get; set; }
        public int LimitValue { get; set; }
        public List<string> ChildIds { get; set; } = new List<string>();
    }
}