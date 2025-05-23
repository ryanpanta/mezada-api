using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApiMezada.Configurations;
using WebApiMezada.Models;

namespace WebApiMezada.Services.FamilyGroup
{
    public class TaskHistoryService : ITaskHistoryService
    {
        private readonly IMongoCollection<CycleModel> _cycleCollection;

        public TaskHistoryService(IOptions<CycleDatabaseSettings> cycleSettings)
        {
            var client = new MongoClient(cycleSettings.Value.ConnectionString);
            var database = client.GetDatabase(cycleSettings.Value.DatabaseName);
            _cycleCollection = database.GetCollection<CycleModel>(cycleSettings.Value.CycleCollectionName);
        }

        public async Task<CycleModel> GetCurrentCycleByGroupId(string familyGroupId)
        {
            var activeCycle = await _cycleCollection
                .Find(c => c.FamilyGroupId == familyGroupId && c.IsActive)
                .FirstOrDefaultAsync();

            return activeCycle;
        }
    }
}