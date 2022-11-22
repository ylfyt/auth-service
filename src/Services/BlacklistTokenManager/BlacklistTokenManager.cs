namespace auth_sevice.src.Services
{
  public class BlacklistToken
  {
    public Guid Jid { get; set; }
    public long ExpiredAt { get; set; }
  }

  public class BlacklistTokenManager : IBlacklistTokenManager
  {
    private static List<BlacklistToken> blacklistRefreshTokens = new List<BlacklistToken>();
    private static bool IsCleanUpStarted = false;

    public bool CreateToken(Guid refreshTokenId)
    {
      var expiredAt = DateTimeOffset.Now.AddSeconds(ServerInfo.JWT_ACCESS_TOKEN_EXPIRY_TIME).ToUnixTimeSeconds();
      blacklistRefreshTokens.Add(new BlacklistToken
      {
        ExpiredAt = expiredAt,
        Jid = refreshTokenId
      });

      if (!IsCleanUpStarted)
      {
        IsCleanUpStarted = true;
        StartCleanUp();
      }

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

    private static void StartCleanUp()
    {
      Console.WriteLine($"Next clean up in {(ServerInfo.JWT_ACCESS_TOKEN_EXPIRY_TIME + 60) / 60} minutes");
      System.Threading.Tasks.Task.Factory.StartNew(() =>
      {
        Thread.Sleep(Convert.ToInt32(ServerInfo.JWT_ACCESS_TOKEN_EXPIRY_TIME + 60) * 1000);
        Console.WriteLine("Start clean up");
        blacklistRefreshTokens = blacklistRefreshTokens.Where(token =>
        {
          var nowTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
          if (token.ExpiredAt < nowTimestamp)
          {
            Console.WriteLine($"Delete {token.Jid}");
          }

          return token.ExpiredAt > nowTimestamp;
        }).ToList();
        Console.WriteLine("Done");

        if (blacklistRefreshTokens.Count == 0)
        {
          IsCleanUpStarted = false;
          System.Console.WriteLine("Clean up is stopped. Blacklisted token is empty");
          return;
        }
        StartCleanUp();
      });
    }
  }
}