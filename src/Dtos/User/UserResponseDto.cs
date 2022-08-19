using auth_sevice.src.Models;
using auth_sevice.src.Types;

namespace auth_sevice.src.Dtos
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