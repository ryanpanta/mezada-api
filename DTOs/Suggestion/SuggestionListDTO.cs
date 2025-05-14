namespace WebApiMezada.DTOs.Suggestion;

public class SuggestionListDTO
{
    public string Id { get; set; } = string.Empty;
    public string ChildId { get; set; } = string.Empty;
    public string ChildName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SuggestedAt { get; set; }
}