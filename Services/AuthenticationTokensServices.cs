using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models.Objects;
using backend.Models.Options;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace backend.Services
{
  public class AuthenticationTokensServices
  {
    private readonly IMongoCollection<AuthenticationToken> _authenticationTokens;
    private readonly string _secretKey;
    public AuthenticationTokensServices(
       MongoClient mongoClient,
       IMongoDbOptions mongoDbOptions,
       IJWTOptions jwtOptions
    )
    {
      var database = mongoClient.GetDatabase(mongoDbOptions.DatabaseName);
      _authenticationTokens = database.GetCollection<AuthenticationToken>(mongoDbOptions.AuthenticationTokensCollectionName);
      _secretKey = jwtOptions.SecretKey;
    }
    public async Task<AuthenticationToken> CreateAuthenticationToken(string userId)
    {
      DateTime expiresAt = DateTime.UtcNow.AddDays(7);
      string token = _CreateTokenString(userId);
      string refreshToken = _CreateRefreshTokenString(expiresAt);

      var authenticationToken = new AuthenticationToken
      {
        Token = token,
        RefreshToken = refreshToken,
        IsValid = true,
        ExpiresAt = expiresAt
      };
      await _authenticationTokens.InsertOneAsync(authenticationToken);
      return authenticationToken;
    }
    public async Task<AuthenticationToken> GetRefreshedAuthenticationToken(string refreshToken)
    {
      var formerAuthenticationToken = await _GetAuthenticationToken(refreshToken);
      if (formerAuthenticationToken is null)
      {
        return null;
      }
      await _InvalidateFormerAuthenticationTokens(refreshToken);
      string userId = _GetUserIdFromToken(formerAuthenticationToken.Token);
      var authenticationToken = await CreateAuthenticationToken(userId);
      return authenticationToken;
    }
    private async Task _InvalidateFormerAuthenticationTokens(string refreshToken)
    {
      var filter = new FilterDefinitionBuilder<AuthenticationToken>().Where(x => x.RefreshToken == refreshToken);
      var update = Builders<AuthenticationToken>.Update.Set(x => x.IsValid, false);
      await _authenticationTokens.UpdateManyAsync(filter, update);
    }
    private string _GetUserIdFromToken(string token)
    {
      SecurityToken validatedToken;
      TokenValidationParameters validationParameters = new TokenValidationParameters
      {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
      };
      ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out validatedToken);
      if (principal is null) return "";
      string userId = principal.FindFirst(ClaimTypes.Name).Value;
      return userId;
    }
    private async Task<AuthenticationToken> _GetAuthenticationToken(string refreshToken)
    {
      var authenticationToken = await _authenticationTokens.Find(x => x.RefreshToken == refreshToken && x.IsValid && x.ExpiresAt > DateTime.UtcNow).FirstOrDefaultAsync();
      if (authenticationToken is null)
      {
        return null;
      }
      return authenticationToken;
    }

    private string _CreateTokenString(string userId)
    {
      var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
      var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

      var claims = new Claim[]{
          new Claim(ClaimTypes.Name, userId)
      };

      var tokenDescriptor = new JwtSecurityToken(
          claims: claims,
          expires: System.DateTime.UtcNow.AddSeconds(10),
          signingCredentials: signingCredentials
      );

      var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
      return token;
    }
    private string _CreateRefreshTokenString(DateTime expiresAt)
    {
      var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
      var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
      string randomString = _CreateRandomString();

      var claims = new Claim[]{
          new Claim(ClaimTypes.Name, randomString)
      };

      var tokenDescriptor = new JwtSecurityToken(
          claims: claims,
          expires: expiresAt,
          signingCredentials: signingCredentials
      );

      var refreshToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
      return refreshToken;
    }

    private string _CreateRandomString()
    {
      var rngCrypto = new RNGCryptoServiceProvider();
      var randomBytes = new byte[64];
      rngCrypto.GetBytes(randomBytes);
      return Convert.ToBase64String(randomBytes);
    }
  }
}