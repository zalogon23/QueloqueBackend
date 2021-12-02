using System;
using System.Threading.Tasks;
using backend.Models.Objects;
using backend.Models.Options;
using MongoDB.Driver;

namespace backend.Services
{
  public class UsersServices
  {
    private readonly IMongoCollection<User> _users;
    public UsersServices(MongoClient mongoClient, IMongoDbOptions mongoDbOptions)
    {
      var database = mongoClient.GetDatabase(mongoDbOptions.DatabaseName);
      _users = database.GetCollection<User>(mongoDbOptions.UsersCollectionName);
    }
    public async Task<User> GetUserByLogin(string username, string password)
    {
      var user = await _users.Find(x => x.Username == username).FirstOrDefaultAsync();


      if (user is null) return null;

      bool isValidPassword = _IsPasswordMatch(password, user.Password);

      if (!isValidPassword) return null;

      return user;
    }
    public async Task<User> GetUserById(string id)
    {
      var user = await _users.Find(x => x.Id == id).FirstOrDefaultAsync();
      return user;
    }
    private bool _IsPasswordMatch(string password, string hash)
    {
      //Some logic to match password and hash 
      return true;
    }
  }
}