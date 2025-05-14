using WebApiMezada.DTOs.FamilyGroup;
using WebApiMezada.DTOs.Suggestion;
using WebApiMezada.Models;

namespace WebApiMezada.Services.FamilyGroup
{
    public interface ISuggestionService
    {
        Task<SuggestionModel> CreateOrUpdateSuggestion(SuggestionCreateDTO dto, string userId);
        Task<List<SuggestionListDTO>> GetSuggestions(string userId);
    }
}
