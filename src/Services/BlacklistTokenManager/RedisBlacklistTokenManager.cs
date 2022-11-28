using StackExchange.Redis;

namespace auth_sevice.src.Services;

public class RedisBlacklistTokenManager : IBlacklistTokenManager
{
  private readonly IDatabase redisDb;

  public RedisBlacklistTokenManager(IConnectionMultiplexer _redis)
  {
    redisDb = _redis.GetDatabase();
  }
  public bool CreateToken(Guid refreshTokenId)
  {
    var duration = TimeSpan.FromSeconds(ServerInfo.JWT_ACCESS_TOKEN_EXPIRY_TIME);
    var success = redisDb.StringSet(refreshTokenId.ToString(), true, duration);
    return success;
  }

  public bool IsRefreshTokenIdExist(Guid refreshTokenId)
  {
    var value = redisDb.StringGet(refreshTokenId.ToString());
    if (value.IsNull)
      return false;
    
    return true;
  }
}