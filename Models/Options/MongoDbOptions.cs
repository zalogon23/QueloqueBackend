namespace backend.Models.Options
{
  public class MongoDbOptions : IMongoDbOptions
  {
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string SessionsCollectionName { get; set; }
    public string UsersCollectionName { get; set; }
    public string AuthenticationTokensCollectionName { get; set; }
  }
  public interface IMongoDbOptions
  {
    string ConnectionString { get; set; }
    string DatabaseName { get; set; }
    string SessionsCollectionName { get; set; }
    string UsersCollectionName { get; set; }
    string AuthenticationTokensCollectionName { get; set; }
  }
}