using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.Objects;
using backend.Models.Options;
using MongoDB.Driver;

namespace backend.Services
{
  public class SessionsServices
  {
    private readonly IMongoCollection<Session> _sessions;
    public SessionsServices(MongoClient mongoClient, IMongoDbOptions mongoDbOptions)
    {
      var database = mongoClient.GetDatabase(mongoDbOptions.DatabaseName);
      _sessions = database.GetCollection<Session>(mongoDbOptions.SessionsCollectionName);
    }

    public async Task AddSession(string userId, string connectionId)
    {
      var session = new Session
      {
        ConnectionId = connectionId,
        UserId = userId
      };

      await _RemoveOldSessions(userId);
      await _sessions.InsertOneAsync(session);
    }
    public async Task<List<Session>> GetSessionsForId(List<string> ids)
    {
      var filter = new FilterDefinitionBuilder<Session>().Where(x => ids.Contains(x.UserId));
      return await _sessions.Find(filter).ToListAsync();
    }
    private async Task _RemoveOldSessions(string userId)
    {
      await _sessions.DeleteManyAsync(x => x.UserId == userId);
    }
  }
}