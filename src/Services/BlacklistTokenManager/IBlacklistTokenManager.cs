namespace auth_sevice.src.Services
{
  public interface IBlacklistTokenManager
  {
    public bool IsRefreshTokenIdExist(Guid refreshTokenId);
    public bool CreateToken(Guid refreshTokenId);
  }
}