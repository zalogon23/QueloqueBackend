namespace backend.Models.Dtos
{
  public class PublicMessage
  {
    public string Content { get; set; }
    public string SenderId { get; set; }
    public string SenderName { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
  }
  public class PrivateMessage
  {
    public string Content { get; set; }
    public string SenderId { get; set; }
    public string SenderName { get; set; }
    public string ReceiverId { get; set; }
  }
}