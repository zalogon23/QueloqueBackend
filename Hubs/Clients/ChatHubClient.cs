using System.Threading.Tasks;
using backend.Models.Dtos;

namespace backend.Hubs.Clients
{
  public interface ChatHubClient
  {
    Task ReceivePublicMessage(PublicMessage publicMessage);
    Task ReceivePrivateMessage(PrivateMessage privateMessage);
  }
}