namespace auth_sevice.Src.Dtos
{
  public class TokenDto
  {
    public string Access { get; set; } = string.Empty;
    public string Refresh { get; set; } = string.Empty;
  }

  public class RefreshTokenDto
  {
    public string token { get; set; } = string.Empty;
  }
}