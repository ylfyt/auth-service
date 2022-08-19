using auth_sevice.src.Dtos;
using auth_sevice.src.Models;

namespace auth_sevice.src.Services
{
  public interface ITokenManager
  {
    public Task<Tuple<string?, Guid>> CreateRefreshToken(User user);

    public string CreateAccessToken(User user, Guid refreshTokenId);
    public string? Verify(string token);
    public AccessTokenPayload? VerifyAccessToken(string token);
  }
}