namespace backend.Models.Options
{
  public class JWTOptions : IJWTOptions
  {
    public string SecretKey { get; set; }
  }
  public interface IJWTOptions
  {
    string SecretKey { get; set; }
  }
}