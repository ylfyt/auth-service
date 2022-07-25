using auth_sevice.Src.Models;

namespace auth_sevice.Src.Services
{
  public interface ITokenManager
  {
    public Task<Tuple<string?, Guid>> CreateRefreshToken(User user);

    public string CreateAccessToken(User user, Guid refreshTokenId);
    public string? Verify(string token);
    public bool VerifyAccessToken(string token);
  }
}