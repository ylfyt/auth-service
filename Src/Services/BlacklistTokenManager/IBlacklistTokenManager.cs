namespace auth_sevice.Src.Services
{
  public interface IBlacklistTokenManager
  {
    public bool IsRefreshTokenIdExist(Guid refreshTokenId);
    public bool CreateToken(Guid refreshTokenId);
  }
}