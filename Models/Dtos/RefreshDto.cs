namespace backend.Models.Dtos
{
  public class RefreshDtoRequest
  {
    public string RefreshToken { get; set; }
  }
  public class RefreshDtoResponse
  {
    public string Token { get; set; }
    public string RefreshToken { get; set; }
  }
}