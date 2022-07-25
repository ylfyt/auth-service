namespace auth_sevice.Src.Services
{
  public class BlacklistTokenManager : IBlacklistTokenManager
  {
    public static List<Guid> refreshTokenIds = new List<Guid>();

    public bool CreateToken(Guid refreshTokenId)
    {
      refreshTokenIds.Add(refreshTokenId);

      System.Threading.Tasks.Task.Factory.StartNew(() =>
        {
          Thread.Sleep(Convert.ToInt32(ServerInfo.JWT_ACCESS_TOKEN_EXPIRY_TIME * 1000));
          System.Console.WriteLine($"Delete token {refreshTokenId}");
          refreshTokenIds.Remove(refreshTokenId);
        });
      return true;
    }

    public bool IsRefreshTokenIdExist(Guid refreshTokenId)
    {
      for (int i = 0; i < refreshTokenIds.Count; i++)
      {
        if (refreshTokenIds[i].Equals(refreshTokenId))
          return true;
      }

      return false;
    }
  }
}