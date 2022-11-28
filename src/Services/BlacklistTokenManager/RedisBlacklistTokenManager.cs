namespace auth_sevice.src.Services;

using Redis.OM.Modeling;
using Redis.OM;

[Document]
public class RedisBlacklistedToken
{
  [RedisIdField]
  [Indexed]
  public Guid Id { get; set; }

  [Indexed]
  public long CreatedAt { get; set; }

}

public class RedisBlacklistTokenManager : IBlacklistTokenManager
{
  private readonly RedisConnectionProvider _provider;
  public RedisBlacklistTokenManager(RedisConnectionProvider provider)
  {
    _provider = provider;
  }
  public bool CreateToken(Guid refreshTokenId)
  {
    var collection = _provider.RedisCollection<RedisBlacklistedToken>();
    var res = collection.Insert(new RedisBlacklistedToken
    {
      Id = refreshTokenId,
      CreatedAt = DateTimeOffset.Now.ToUnixTimeSeconds()
    });
    Console.WriteLine(res);
    throw new NotImplementedException();
  }

  public bool IsRefreshTokenIdExist(Guid refreshTokenId)
  {
    Console.WriteLine(10);
    var collection = _provider.RedisCollection<RedisBlacklistedToken>();
    Console.WriteLine(11);
    try
    {
      var data = collection.FirstOrDefault(token => token.CreatedAt < DateTimeOffset.Now.ToUnixTimeSeconds());

      Console.WriteLine(12);
      Console.WriteLine(data?.Id);
      Console.WriteLine(13);
    }
    catch (System.Exception e)
    {
      Console.WriteLine(e);
      throw;
    }
    throw new NotImplementedException();
  }
}