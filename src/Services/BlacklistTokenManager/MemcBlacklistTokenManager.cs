using Enyim.Caching;

namespace auth_sevice.src.Services;

public class MemcBlacklistTokenManager : IBlacklistTokenManager
{

  private readonly IMemcachedClient memcachedClient;

  public MemcBlacklistTokenManager(IMemcachedClient _memcachedClient)
  {
    memcachedClient = _memcachedClient;
  }

  public bool CreateToken(Guid refreshTokenId)
  {
    var success = memcachedClient.Set(refreshTokenId.ToString(), true, (int)ServerInfo.JWT_ACCESS_TOKEN_EXPIRY_TIME);

    return success;
  }

  public bool IsRefreshTokenIdExist(Guid refreshTokenId)
  {
    var exist = memcachedClient.Get(refreshTokenId.ToString());
    return exist != null;
  }
}