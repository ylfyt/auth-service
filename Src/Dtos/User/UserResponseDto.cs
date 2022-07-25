using auth_sevice.Src.Models;
using auth_sevice.Src.Types;

namespace auth_sevice.Src.Dtos
{
  public class DataUser : BaseData
  {
    public User User { get; set; } = null!;
    public TokenDto Token { get; set; } = null!;
  }

  public class VerifyResponse
  {
    public bool Valid { get; set; }
    public AccessTokenPayload? Payload { get; set; } = null!;
  }
}