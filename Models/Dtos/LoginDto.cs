namespace backend.Models.Dtos
{
  public class LoginDtoRequest
  {
    public string Username { get; set; }
    public string Password { get; set; }
  }
  public class LoginDtoResponse
  {
    public string Token { get; set; }
    public string RefreshToken { get; set; }
  }
}