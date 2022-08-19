using System.ComponentModel.DataAnnotations;

namespace auth_sevice.src.Dtos
{
  public class TokenDto
  {
    public string Access { get; set; } = string.Empty;
    public string Refresh { get; set; } = string.Empty;
  }

  public class RefreshTokenDto
  {
    [MinLength(4)]
    public string token { get; set; } = string.Empty;
  }

  public class AccessTokenDto
  {
    [MinLength(4)]
    public string token { get; set; } = string.Empty;
  }
}