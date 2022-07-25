using auth_sevice.Src.Types;

namespace auth_sevice.Src.Models
{
  public class RefreshToken : BaseModel
  {
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
  }
}