using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using backend.Models.Dtos;
using backend.Models.Options;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers
{
  [ApiController]
  [Route("api")]
  public class QueloqueController : Controller
  {
    private readonly IJWTOptions _jwtOptions;
    private readonly AuthenticationTokensServices _authenticationTokensServices;
    public QueloqueController(
      IJWTOptions jwtOptions,
      AuthenticationTokensServices authenticationTokensServices
      )
    {
      _jwtOptions = jwtOptions;
      _authenticationTokensServices = authenticationTokensServices;
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDtoRequest loginDto)
    {

      // LOGIC FOR GETTING THE USER
      var user = new
      {
        Id = loginDto.Username
      };
      // ---------------------------

      if (user is null)
      {
        return Unauthorized();
      }

      var authenticationToken = await _authenticationTokensServices.CreateAuthenticationToken(user.Id);

      if (authenticationToken is null)
      {
        return Unauthorized();
      }

      var loginDtoResponse = new LoginDtoResponse
      {
        Token = authenticationToken.Token,
        RefreshToken = authenticationToken.RefreshToken
      };
      return Ok(loginDtoResponse);
    }
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshDtoRequest refreshDto)
    {
      try
      {
        var refreshedAuthenticationToken = await _authenticationTokensServices.GetRefreshedAuthenticationToken(refreshDto.RefreshToken);
        Console.WriteLine($"Este es el refresh {refreshedAuthenticationToken.Token}");
        if (refreshedAuthenticationToken is null)
        {
          return Unauthorized();
        }

        var refreshDtoResponse = new RefreshDtoResponse
        {
          Token = refreshedAuthenticationToken.Token,
          RefreshToken = refreshedAuthenticationToken.RefreshToken
        };
        return Ok(refreshDtoResponse);
      }
      catch (Exception err)
      {
        Console.WriteLine(err.Message);
        return BadRequest();
      }
    }
  }
}