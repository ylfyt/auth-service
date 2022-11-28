using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using auth_sevice.src.Data;
using auth_sevice.src.Dtos;
using auth_sevice.src.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace auth_sevice.src.Services
{
  public class TokenManager : ITokenManager
  {
    private readonly DataContext context;
    private readonly IBlacklistTokenManager blacklistTokenManager;

    public TokenManager(DataContext context, IBlacklistTokenManager blacklistTokenManager)
    {
      this.context = context;
      this.blacklistTokenManager = blacklistTokenManager;
    }

    public string CreateAccessToken(User user, Guid refreshTokenId)
    {
      List<Claim> claims = new List<Claim>{
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("jid", refreshTokenId.ToString())
                };
      var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(ServerInfo.JWT_ACCESS_TOKEN_SECRET_KEY!));
      var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
      var jwt = new JwtSecurityToken(
          claims: claims,
          signingCredentials: cred,
          expires: DateTime.Now.AddSeconds(ServerInfo.JWT_ACCESS_TOKEN_EXPIRY_TIME)
      );

      var token = new JwtSecurityTokenHandler().WriteToken(jwt);
      return token;
    }


    public async Task<Tuple<string?, Guid>> CreateRefreshToken(User user)
    {
      var refreshTokenId = Guid.NewGuid();

      var tokens = await context.RefreshTokens.Where(t => t.UserId == user.Id).CountAsync();
      if (tokens >= 2) return new Tuple<string?, Guid>(null, refreshTokenId);

      var claims = new List<Claim>{
        new Claim("jid", refreshTokenId.ToString())
      };

      var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(ServerInfo.JWT_REFRESH_TOKEN_SECRET_KEY)!);
      var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
      var jwt = new JwtSecurityToken(
          claims: claims,
          signingCredentials: cred,
          expires: DateTime.Now.AddDays(ServerInfo.JWT_REFRESH_TOKEN_EXPIRY_TIME)
      );
      var token = new JwtSecurityTokenHandler().WriteToken(jwt);
      var newRefreshToken = new RefreshToken
      {
        Id = refreshTokenId,
        UserId = user.Id
      };
      await context.RefreshTokens.AddAsync(newRefreshToken);
      await context.SaveChangesAsync();
      return new Tuple<string?, Guid>(token, refreshTokenId);
    }

    public bool ValidateRefreshToken(string token, out Guid refreshTokenId)
    {
      refreshTokenId = Guid.Empty;
      try
      {
        var claims = new JwtSecurityTokenHandler().ValidateToken(token,
                  new TokenValidationParameters
                  {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(ServerInfo.JWT_REFRESH_TOKEN_SECRET_KEY)),
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ClockSkew = TimeSpan.Zero,
                    ValidateLifetime = false
                  }, out SecurityToken validatedToken
              );

        var rawRefreshTokenId = claims.FindFirstValue("jid");
        var rawExp = claims.FindFirstValue("exp");
        if (rawRefreshTokenId == null || rawExp == null) return false;

        if (!Guid.TryParse(rawRefreshTokenId, out refreshTokenId))
          return false;

        var exp = long.Parse(rawExp);
        if (DateTimeOffset.Now.ToUnixTimeSeconds() > exp) return false;

        return true;
      }
      catch (System.Exception)
      {
        return false;
      }
    }

    public AccessTokenPayload? ValidateAccessToken(string token)
    {
      try
      {
        var claims = new JwtSecurityTokenHandler().ValidateToken(token,
                  new TokenValidationParameters
                  {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(ServerInfo.JWT_ACCESS_TOKEN_SECRET_KEY)),
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ClockSkew = TimeSpan.Zero
                  }, out SecurityToken validatedToken
              );

        var rawRefreshTokenId = claims.FindFirstValue("jid");
        var username = claims.FindFirstValue(ClaimTypes.Name);
        if (rawRefreshTokenId == null || username == null) return null;

        if (!Guid.TryParse(rawRefreshTokenId, out Guid refreshTokenId))
          return null;

        var exist = blacklistTokenManager.IsRefreshTokenIdExist(refreshTokenId);
        if (exist) return null;

        return new AccessTokenPayload
        {
          Username = username,
        };
      }
      catch (System.Exception)
      {
        return null;
      }
    }
  }
}