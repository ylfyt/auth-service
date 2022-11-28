using System.ComponentModel.DataAnnotations;

namespace auth_sevice.src.Dtos
{
  public class TokenDto
  {
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
  }

  public class RefreshTokenDto
  {
    [MinLength(4)]
    public string RefreshToken { get; set; } = string.Empty;
  }

  public class AccessTokenDto
  {
    [MinLength(4)]
    public string AccessToken { get; set; } = string.Empty;
  }
}