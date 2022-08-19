using auth_sevice.src.Types;

namespace auth_sevice.src.Models
{
  public class RefreshToken : BaseModel
  {
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
  }
}