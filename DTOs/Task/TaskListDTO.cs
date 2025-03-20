using WebApiMezada.Models.Enums;

namespace WebApiMezada.DTOs.Task
{
    public class TaskListDTO
    {
        public bool Active { get; set; }
        public DateOnly CreatedAt { get; set; }
        public string Description { get; set; }
        public string FamilyGroupId { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public int Points { get; set; }
        public EnumTaskStatus Status { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
