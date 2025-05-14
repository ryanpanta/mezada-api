using FluentValidation;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApiMezada.Configurations;
using WebApiMezada.DTOs.Suggestion;
using WebApiMezada.Models;
using WebApiMezada.Models.Enums;
using WebApiMezada.Services.User;

namespace WebApiMezada.Services.FamilyGroup
{
    public class SuggestionService : ISuggestionService
    {
        private readonly IMongoCollection<SuggestionModel> _suggestionCollection;
        private readonly IMongoCollection<UserModel> _userCollection;
        private readonly IValidator<SuggestionCreateDTO> _validator;
        private readonly IUserService _userService;

        public SuggestionService(IOptions<SuggestionDatabaseSettings> suggestionSettings, IUserService userService,
            IOptions<UserDatabaseSettings> userSettings, IValidator<SuggestionCreateDTO> validator)
        {
            var client = new MongoClient(suggestionSettings.Value.ConnectionString);
            var database = client.GetDatabase(suggestionSettings.Value.DatabaseName);
            _suggestionCollection =
                database.GetCollection<SuggestionModel>(suggestionSettings.Value.SuggestionCollectionName);
            _userCollection = database.GetCollection<UserModel>(userSettings.Value.UserCollectionName);
            _userService = userService;
            _validator = validator;
        }


        public async Task<SuggestionModel> CreateOrUpdateSuggestion(SuggestionCreateDTO dto, string userId)
        {
            var validationResult = _validator.Validate(dto);
            if (!validationResult.IsValid)
                throw new ValidationException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var user = await GetUserOrThrow(userId);
            if (user.Role != EnumRoles.Child)
                throw new UnauthorizedAccessException("Apenas filhos podem criar ou editar sugestões.");

            if (string.IsNullOrEmpty(dto.Id))
            {
                var suggestion = new SuggestionModel
                {
                    ChildId = userId,
                    Message = dto.Message,
                };

                await _suggestionCollection.InsertOneAsync(suggestion);
                return suggestion;
            }
            else
            {
                var suggestion = await _suggestionCollection
                    .Find(s => s.Id == dto.Id && s.ChildId == userId)
                    .FirstOrDefaultAsync();
                
                if (suggestion == null)
                    throw new KeyNotFoundException("Sugestão não encontrada ou não pertence ao usuário.");

                suggestion.Message = dto.Message;
                suggestion.CreatedAt = DateTime.UtcNow;

                await _suggestionCollection.ReplaceOneAsync(s => s.Id == suggestion.Id, suggestion);
                return suggestion;
            }
        }

        public async Task<List<SuggestionListDTO>> GetSuggestions(string userId)
        {
            var user = await GetUserOrThrow(userId);
            if (string.IsNullOrEmpty(user.FamilyGroupId))
                throw new InvalidOperationException("Usuário não pertence a nenhum grupo.");

            var filterBuilder = Builders<SuggestionModel>.Filter;
            var filterDefinition = filterBuilder.Eq(s => s.ChildId, userId);

            if (user.Role == EnumRoles.Parent)
            {
                var childrenIds = await _userCollection
                    .Find(u => u.FamilyGroupId == user.FamilyGroupId && u.Role == EnumRoles.Child && u.Active)
                    .Project(u => u.Id)
                    .ToListAsync();

                filterDefinition = filterBuilder.In(s => s.ChildId, childrenIds);
            }

            var suggestions = await _suggestionCollection
                .Find(filterDefinition)
                .ToListAsync();

            var suggestionDtos = new List<SuggestionListDTO>();
            foreach (var suggestion in suggestions)
            {
                var child = await _userCollection.Find(u => u.Id == suggestion.ChildId).FirstOrDefaultAsync();
                suggestionDtos.Add(new SuggestionListDTO
                {
                    Id = suggestion.Id,
                    ChildId = suggestion.ChildId,
                    ChildName = child?.Name ?? "Desconhecido",
                    Message = suggestion.Message,
                    SuggestedAt = suggestion.CreatedAt
                });
            }

            return suggestionDtos.OrderByDescending(s => s.SuggestedAt).ToList();
        }

        private async Task<UserModel> GetUserOrThrow(string userId)
        {
            var user = await _userService.GetUserById(userId);
            return user ?? throw new KeyNotFoundException("Usuário não encontrado.");
        }
    }
}