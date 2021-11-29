using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Hubs.Clients;
using backend.Models.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace backend.Hubs
{
  [Authorize]
  public class ChatHub : Hub<ChatHubClient>
  {
    private readonly SessionsServices _sessionsServices;
    public ChatHub(SessionsServices sessionsServices)
    {
      _sessionsServices = sessionsServices;
    }
    public override async Task<Task> OnConnectedAsync()
    {
      var userId = Context?.User?.Identity?.Name;
      if (userId != null)
      {
        string connectionId = Context.ConnectionId;
        await _sessionsServices.AddSession(userId, connectionId);
        Console.WriteLine($"The user {userId} connected to the ChatHub.");
      }
      else
      {
        Console.WriteLine($"Anonymous user connected to the ChatHub.");
      }
      return base.OnConnectedAsync();
    }
    public async Task SendPublicMessage(PublicMessage _publicMessage)
    {
      Console.WriteLine($"The {Context.User.Identity.Name} is sending {_publicMessage.Content}");
      var publicMessage = new PublicMessage
      {
        Content = _publicMessage.Content,
        Latitude = _publicMessage.Latitude,
        Longitude = _publicMessage.Longitude,
        SenderId = Context.User.Identity.Name,
        SenderName = _publicMessage.SenderName
      };
      await Clients.All.ReceivePublicMessage(publicMessage);
    }

    public async Task SendPrivateMessage(PrivateMessage _privateMessage)
    {
      Console.WriteLine($"The {Context.User.Identity.Name} is sending {_privateMessage.Content}");
      var privateMessage = new PrivateMessage
      {
        Content = _privateMessage.Content,
        SenderId = Context.User.Identity.Name,
        SenderName = _privateMessage.SenderName,
        ReceiverId = _privateMessage.ReceiverId
      };
      var ids = new List<string>{
        privateMessage.SenderId,
        privateMessage.ReceiverId
      };
      var sessions = await _sessionsServices.GetSessionsForId(ids);
      var sessionsIds = new List<string>();
      foreach (var session in sessions)
      {
        sessionsIds.Add(session.ConnectionId);
      }
      await Clients.Clients(sessionsIds).ReceivePrivateMessage(privateMessage);
    }
  }
}