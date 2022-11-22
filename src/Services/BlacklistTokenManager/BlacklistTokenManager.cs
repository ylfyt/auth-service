namespace auth_sevice.src.Services
{
  public class BlacklistToken
  {
    public Guid Jid { get; set; }
    public long ExpiredAt { get; set; }
  }

  public class BlacklistTokenManager : IBlacklistTokenManager
  {
    public static List<BlacklistToken> blacklistRefreshTokens = new List<BlacklistToken>();

    public bool CreateToken(Guid refreshTokenId)
    {
      var expiredAt = DateTimeOffset.Now.AddSeconds(ServerInfo.JWT_ACCESS_TOKEN_EXPIRY_TIME).ToUnixTimeSeconds();
      blacklistRefreshTokens.Add(new BlacklistToken
      {
        ExpiredAt = expiredAt,
        Jid = refreshTokenId
      });

      return true;
    }

    public bool IsRefreshTokenIdExist(Guid refreshTokenId)
    {
      for (int i = 0; i < blacklistRefreshTokens.Count; i++)
      {
        if (blacklistRefreshTokens[i].Jid.Equals(refreshTokenId))
          return true;
      }

      return false;
    }

    public static void StartCleanUp()
    {
      Console.WriteLine($"Mext clean up in {(ServerInfo.JWT_ACCESS_TOKEN_EXPIRY_TIME) / 60} minutes");
      System.Threading.Tasks.Task.Factory.StartNew(() =>
      {
        Thread.Sleep(Convert.ToInt32(ServerInfo.JWT_ACCESS_TOKEN_EXPIRY_TIME) * 1000);
        Console.WriteLine("Start clean up");
        blacklistRefreshTokens = blacklistRefreshTokens.Where(token =>
        {
          if (token.ExpiredAt < DateTimeOffset.Now.ToUnixTimeSeconds())
          {
            Console.WriteLine($"Delete {token.Jid}");
          }

          return token.ExpiredAt > DateTimeOffset.Now.ToUnixTimeSeconds();
        }).ToList();
        Console.WriteLine("Done");
        StartCleanUp();
      });
    }
  }
}